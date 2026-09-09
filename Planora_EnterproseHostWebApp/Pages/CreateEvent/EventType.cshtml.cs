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
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

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

        // The wizard's single write point for the event type. Saved to
        // Session here (not the URL) so every later step, and the shared
        // layout, can trust it — including across RedirectToPage() hops,
        // which drop query strings.
        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            HttpContext.Session.SetEventType(EventType);

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