using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubBroadcastModel : PageModel
    {
        public EventHubBroadcastResponse broadcastResp { get; set; } = new();
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
                var resp = helper.GetEventHubBroadCastResp(userId.Value, eventId,slotId);
                if (resp != null && resp.Status == 1)
                    broadcastResp = resp;
                else
                    ApiError = resp?.Message ?? "Failed to load broadcast history.";
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load broadcast history: " + ex.Message;
            }

            return Page();
        }

        public JsonResult GetEventHubBroadcastData(int eventId,int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            var helper = new CommonHelper();
            try
            {
                return new JsonResult(helper.GetEventHubBroadCastResp(userId.Value, eventId,slotId));
            }
            catch (Exception ex)
            {
                return new JsonResult(new EventHubBroadcastResponse { Status = 0, Message = ex.Message });
            }
        }
    }
}