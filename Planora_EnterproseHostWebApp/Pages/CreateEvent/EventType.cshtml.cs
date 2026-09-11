using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.CreateHost
{
    public class EventTypeModel : PageModel
    {
        public GetEventTypeResp Response { get; private set; } = new();
        public string? ApiError { get; private set; }

        [BindProperty]
        public string EventType { get; set; } = "public";

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 1;

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
                return RedirectToPage("/Login/Login");
            }

            // --- FIX 1: Restore saved selection from session ---
            var savedType = HttpContext.Session.GetEventType();
            if (!string.IsNullOrEmpty(savedType))
            {
                EventType = savedType;
            }

            // --- FIX 2: Ensure progress tracks highest step reached ---
            //int currentMaxProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
            //HttpContext.Session.SetInt32("StepProgress", Math.Max(currentMaxProgress, 2));

            try
            {
                var helper = new CommonHelper();
                var result = helper.EventType(userId.Value);

                if (result != null && result.Status == 1)
                {
                    Response = result;
                }
                else
                {
                    ApiError = result != null && !string.IsNullOrWhiteSpace(result.Message)
                        ? result.Message
                        : "Unable to load event type settings.";
                }
            }
            catch
            {
                ApiError = "Unable to reach the event service. Please try again.";
            }

            return Page();
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

            HttpContext.Session.SetEventType(EventType);
            int currentMaxProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
            HttpContext.Session.SetInt32("StepProgress", Math.Max(currentMaxProgress, 2));
            return RedirectToPage("/CreateEvent/BasicDetails");
        }

        // Small view helpers so the .cshtml doesn't need per-cell if/else.
        // Instance methods (not static) so the view can call them as
        // Model.BadgeClass(...) / Model.BadgeText(...).
        public string BadgeClass(bool value) =>
            value ? "badge badge--live" : "badge badge--nodot badge--neutral";

        public string BadgeText(bool value, string yes = "Yes", string no = "\u2014") =>
            value ? "--" : no;

        public string LockedBadgeText(bool value) =>
            value ? "on" : "off";
    }
}