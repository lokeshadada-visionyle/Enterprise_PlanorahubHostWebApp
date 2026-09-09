using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubSeatingModel : PageModel
    {
        public EventHubSeatStatusResponse seatingDataResp { get; set; } = new();
        public SeatResponseModel seatingLayoutData { get; set; } = new();
        public string ApiError { get; set; } = string.Empty;
        public IActionResult OnGet(int eventId, int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToPage("/Login/Login");

            ViewData["EventId"] = eventId;
            ViewData["SlotId"] = slotId;
            var helper = new CommonHelper();
            var errors = new List<string>();
            try
            {
                var resp = helper.GetEventHubSeatingResp(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1)
                    seatingDataResp = resp;
                else
                    errors.Add(resp?.Message ?? "Failed to load dashboard metrics.");
            }
            catch (Exception ex)
            {
                errors.Add("Failed to load dashboard metrics: " + ex.Message);
            }
            try
            {
                var resp = helper.GetEventHubSeatinLayoutResp(userId.Value, eventId, slotId,1);
                if (resp != null && resp.Status == 1)
                    seatingLayoutData = resp;
                else
                    errors.Add(resp?.Message ?? "Failed to load dashboard metrics.");
            }
            catch (Exception ex)
            {
                errors.Add("Failed to load dashboard metrics: " + ex.Message);
            }

            return Page();
        }
        public JsonResult GetEventHubGuestData(int eventId, int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            var helper = new CommonHelper();
            try
            {
                return new JsonResult(helper.GetEventHubSeatingResp(userId.Value, eventId, slotId));
            }
            catch (Exception ex)
            {
                return new JsonResult(new EventHubSeatStatusResponse { Status = 0, Message = ex.Message });
            }
        }
        public JsonResult GetEventHubSeatingLayoutData(int eventId, int slotId,int ticketTypeId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            var helper = new CommonHelper();
            try
            {
                return new JsonResult(helper.GetEventHubSeatinLayoutResp(userId.Value, eventId, slotId,ticketTypeId));
            }
            catch (Exception ex)
            {
                return new JsonResult(new SeatResponseModel { Status = 0, Message = ex.Message });
            }
        }
    }
}
