using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubRsvpModel : PageModel
    {
        public EventHubRsvpGuestResp eventHubRsvpData { get; set; } = new();
        public EventHubRsvpResponseResp rsvpResponseData { get; set; } = new();

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
                var resp = helper.GetEventHubGuestRsvpResp(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1)
                    eventHubRsvpData = resp;
                else
                    errors.Add(resp?.Message ?? "Failed to load dashboard metrics.");
            }
            catch (Exception ex)
            {
                errors.Add("Failed to load dashboard metrics: " + ex.Message);
            }
            try
            {
                var resp = helper.GetEventHubGuestRsvpResponseResp(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1)
                    rsvpResponseData = resp;
                else
                    errors.Add(resp?.Message ?? "Failed to load ticket types.");
            }
            catch (Exception ex)
            {
                errors.Add("Failed to load ticket types: " + ex.Message);
            }

            if (errors.Any())
            {
                ApiError = string.Join(" | ", errors);
            }
            return Page();
        }
        public JsonResult GetEventHubRsvpData(int eventId, int slotId, int ticketTypeId, string search)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            var helper = new CommonHelper();
            try
            {
                return new JsonResult(helper.GetEventHubGuestRsvpResp(userId.Value, eventId, slotId, ticketTypeId, search));
            }
            catch (Exception ex)
            {
                return new JsonResult(new EventHubGuestResp { Status = 0, Message = ex.Message });
            }
        }
        public JsonResult GetEventHubRsvpResponseData(int eventId, int slotId,string search,string resp)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            var helper = new CommonHelper();
            try
            {
                return new JsonResult(helper.GetEventHubGuestRsvpResponseResp(userId.Value, eventId, slotId, search,resp));
            }
            catch (Exception ex)
            {
                return new JsonResult(new EventHubGuestResp { Status = 0, Message = ex.Message });
            }
        }
    }
}
