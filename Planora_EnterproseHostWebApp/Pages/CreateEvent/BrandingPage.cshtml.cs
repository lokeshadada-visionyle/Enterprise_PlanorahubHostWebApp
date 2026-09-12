using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class BrandingPageModel : PageModel
    {
        public string? BrandingError { get; private set; }

        public EventPageDetailsResp PageDetails { get; set; } = new EventPageDetailsResp();

        public GetLandingPageResp? Branding { get; set; }

        public bool HasExistingLandingPage => Branding != null && Branding.Status == 1;

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 11;
            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            var eventId = HttpContext.Session.GetInt32("createdEventId");

            if (!eventId.HasValue || eventId.Value <= 0)
            {
                BrandingError = "Event ID is missing.";
                return Page();
            }

            var helper = new CommonHelper();
            EventPageDetailsResp result;

            try
            {
                result = helper.EventLandingPageDetailsResp(userId.Value, eventId.Value);
            }
            catch
            {
                BrandingError = "Unable to load event preview right now. Please try again.";
                return Page();
            }

            if (result is null || result.Status != 1)
            {
                BrandingError = result != null && !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Unable to load event preview.";
                return Page();
            }

            PageDetails = result;

            // --- Load any previously-saved branding (Ent_GetLandingPage) ---
            try
            {
                var brandingResult = helper.GetLandingPageResp(userId.Value, eventId.Value);
                if (brandingResult != null && brandingResult.Status == 1)
                {
                    Branding = brandingResult;
                }
            }
            catch
            {
                // No saved branding yet, or the lookup failed - the form just falls back
                // to its defaults. Branding hasn't been saved before at this point, so
                // this isn't treated as a page-blocking error.
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            if (userId is null || userId.Value <= 0)
            {
                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue || eventId.Value <= 0)
            {
                BrandingError = "Event ID is missing.";
                return Page();
            }

            bool isPrivate = HttpContext.Session.IsPrivateEvent();

            var primaryColour = Request.Form["primary-colour"].ToString().Trim();
            var accentColour = Request.Form["accent-colour"].ToString().Trim();

            var backgroundColour = Request.Form["background-colour"].ToString().Trim();
            if (string.IsNullOrWhiteSpace(backgroundColour))
                backgroundColour = "#FFFFFF";

            var link = isPrivate
                ? Request.Form["page-slug"].ToString().Trim()
                : Request.Form["page-slug-public"].ToString().Trim();

            var description = Request.Form["social-desc"].ToString().Trim();

            var hasExistingPage = string.Equals(
                Request.Form["has-existing-page"].ToString(),
                "true",
                StringComparison.OrdinalIgnoreCase);

            var helper = new CommonHelper();

            // --- Logo upload (Ent_UploadLogo) ---
            // Default to whatever logo was already saved, so re-saving branding without
            // touching the logo input doesn't wipe it out.
            var logoUrl = Request.Form["existing-logo-url"].ToString().Trim();
            var logoFile = Request.Form.Files["logo-upload"];

            if (logoFile != null && logoFile.Length > 0)
            {
                const long maxBytes = 5 * 1024 * 1024; // 5MB, adjust to match your hint text
                if (logoFile.Length > maxBytes)
                {
                    BrandingError = "Logo file is too large (max 5MB).";
                    return Page();
                }

                string base64;
                using (var ms = new MemoryStream())
                {
                    await logoFile.CopyToAsync(ms);
                    base64 = Convert.ToBase64String(ms.ToArray());
                }

                UploadImageResp uploadResult;
                try
                {
                    uploadResult = helper.UploadBrandingImage(new FileUploadModel
                    {
                        UserId = userId.Value,
                        FileName = logoFile.FileName,
                        ImageBase64 = base64
                    });
                }
                catch
                {
                    BrandingError = "Unable to upload your logo right now. Please try again.";
                    return Page();
                }

                if (uploadResult is null || uploadResult.Status != 1)
                {
                    BrandingError = uploadResult != null && !string.IsNullOrWhiteSpace(uploadResult.Message)
                        ? uploadResult.Message
                        : "Unable to upload your logo.";
                    return Page();
                }

                logoUrl = uploadResult.ImageURL;
            }

            Response result;
            try
            {
                if (hasExistingPage)
                {
                    result = helper.UpdateLadndingPage(new UpdateLandingPageReq
                    {
                        UserId = userId.Value,
                        Logo = logoUrl,
                        EventId = eventId.Value,
                        PrimaryColour = primaryColour,
                        AccentColour = accentColour,
                        BackGroundColour = backgroundColour,
                        Link = link,
                        Description = description
                    });
                }
                else
                {
                    result = helper.AddLadndingPage(new AddLandingPageReq
                    {
                        UserId = userId.Value,
                        Logo = logoUrl,
                        EventId = eventId.Value,
                        PrimaryColour = primaryColour,
                        AccentColour = accentColour,
                        BackGroundColour = backgroundColour,
                        Link = link,
                        Description = description
                    });
                }
            }
            catch
            {
                BrandingError = "Unable to save branding right now. Please try again.";
                return Page();
            }

            if (result is null || result.Status != 1)
            {
                BrandingError = result != null && !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Unable to save your branding and page settings.";
                return Page();
            }
            int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
            HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 12));
            return RedirectToPage("/CreateEvent/ReviewAndPublish");
        }
    }
}