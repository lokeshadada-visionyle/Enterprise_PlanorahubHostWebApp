using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using System.Text.RegularExpressions;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class RSVPModel : PageModel
    {
        public string? RsvpError { get; private set; }

        public GetRsvpCustomsResp? Existing { get; private set; }

        public List<CustomFormTemplate> TemplateList { get; private set; } = new List<CustomFormTemplate>();

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 10;
            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }
            var eventId = HttpContext.Session.GetInt32("createdEventId");

            if (userId is not null && eventId != 0)
            {
                var helper = new CommonHelper();

                try
                {
                    Existing = helper.GetRsvpCustoms(userId.Value, eventId.Value);
                }
                catch { }

                try
                {
                    var tplResp = helper.GetRsvpDefaultCustomForms(userId.Value,eventId.Value);
                    if (tplResp != null && tplResp.Status == 1 && tplResp.CustomForms != null)
                    {
                        TemplateList = tplResp.CustomForms;
                    }
                }
                catch { }
            }

            return Page();
        }

        public IActionResult OnGetLoadTemplate([FromQuery(Name = "formTitle")] string formTitle)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
           
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            var helper = new CommonHelper();

            // Pass trimmed title to database helper
            var resp = helper.GetDefaultRsvpFormsByTitle(userId.Value, eventId.Value, formTitle.Trim());

            if (resp != null && resp.Status == 1)
            {
                return new JsonResult(new { status = 1, fields = resp.Fields });
            }

            return new JsonResult(new { status = 0, message = resp?.Message ?? "Failed to fetch fields" });
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            var eventId = HttpContext.Session.GetInt32("createdEventId");

            if (eventId == 0)
            {
                RsvpError = "We couldn't find the event you're building. Please restart from Basic Details.";
                return Page();
            }

            bool isRsvpEnabled = Request.Form["rsvp-required"] == "yes";

            var custom = isRsvpEnabled
                ? ParseCustomFields()
                : new List<CustomFields>();

            var helper = new CommonHelper();
            Response result;

            try
            {
                result = helper.AddRsvpCustoms(new AddRsvpCustomsReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    IsRsvpEnabled = isRsvpEnabled,
                    Custom = custom
                });
            }
            catch
            {
                RsvpError = "Unable to save RSVP settings right now. Please try again.";
                return Page();
            }

            if (result is null || result.Status != 1)
            {
                RsvpError = result != null && !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Unable to save your RSVP settings.";

                return Page();
            }
            int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
            HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 11));
            return RedirectToPage("/CreateEvent/BrandingPage");
        }

        private List<CustomFields> ParseCustomFields()
        {
            var labelRegex = new Regex(@"^rq-(\d+)-label$");
            var result = new List<CustomFields>();

            foreach (var key in Request.Form.Keys)
            {
                var match = labelRegex.Match(key);
                if (!match.Success) continue;

                var idx = int.Parse(match.Groups[1].Value);
                if (idx <= 2) continue;

                var label = Request.Form[$"rq-{idx}-label"].ToString();
                if (string.IsNullOrWhiteSpace(label)) continue;

                var required = Request.Form[$"rq-{idx}-req"].Any(v => v == "on" || v == "true");

                result.Add(new CustomFields
                {
                    CustomId = 0,
                    CustomName = label.Trim(),
                    IsRequired = required
                });
            }

            return result;
        }
    }
}