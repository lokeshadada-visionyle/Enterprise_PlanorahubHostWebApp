using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubOverViewModel : PageModel
    {
        public EventHubOverviewResp eventHubOverviewData { get; set; } = new();
        public string ApiError { get; set; } = string.Empty;

        public IActionResult OnGet(int eventId, int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToPage("/Login/Login");

            ViewData["EventId"] = eventId;
            ViewData["SlotId"] = slotId;

            var helper = new CommonHelper();
            try
            {
                var resp = helper.GetEventHubOverView(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1) eventHubOverviewData = resp;
                else ApiError = resp?.Message ?? "Failed to load dashboard metrics.";
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load dashboard metrics: " + ex.Message;
            }

            return Page();
        }

        public JsonResult OnGetOverviewData(int eventId, int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            var helper = new CommonHelper();
            try
            {
                return new JsonResult(helper.GetEventHubOverView(userId.Value, eventId, slotId));
            }
            catch (Exception ex)
            {
                return new JsonResult(new EventHubOverviewResp { Status = 0, Message = ex.Message });
            }
        }
    }
}