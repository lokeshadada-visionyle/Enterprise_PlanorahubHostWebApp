using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class ReviewAndPublishModel : PageModel
    {
        public EventSummaryResp Summary { get; set; } = new EventSummaryResp();
        public string PageError { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            int eventId = HttpContext.Session.GetInt32("createdEventId") ?? 0;

            if (userId is null ||
                HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            if (eventId == 0)
            {
                PageError = "Event ID is missing.";
                return Page();
            }

            var helper = new CommonHelper();
            EventSummaryResp result;

            try
            {
                result = helper.GetEventDetailsSummary(userId.Value, eventId);
            }
            catch
            {
                PageError = "Unable to load event summary right now. Please try again.";
                return Page();
            }

            if (result is null || result.Status != 1)
            {
                PageError = result != null && !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Unable to load event summary.";
                return Page();
            }

            Summary = result;
            return Page();
        }
        public IActionResult OnPostPublish(bool isPublish)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            int? eventId = HttpContext.Session.GetInt32("createdEventId");

            if (userId is null ||
                HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }


            if (eventId == 0)
            {
                PageError = "Event ID is missing.";
                return Page();
            }

            var helper = new CommonHelper();
            Response result;

            try
            {
                result = helper.PublishEvent(userId.Value, eventId.Value, isPublish);
            }
            catch
            {
                PageError = isPublish
                    ? "Unable to publish this event right now. Please try again."
                    : "Unable to save this event as a draft right now. Please try again.";
                return Page();
            }

            if (result is null || result.Status != 1)
            {
                PageError = result != null && !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : (isPublish ? "Unable to publish this event." : "Unable to save as draft.");
                return Page();
            }

            return RedirectToPage("/Dashboard/HostDashboard");
        }
    }
}