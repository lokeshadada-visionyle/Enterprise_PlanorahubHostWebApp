using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubRegistrationModel : PageModel
    {
        public EventHubGuestRegistrationResp eventHubGuestRegistrationRespData { get; set; } = new();
        public EventTicketTypeResp TicketTypeData { get; set; } = new();
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
                var resp = helper.GetEventHubGuestRegistrationResp(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1) eventHubGuestRegistrationRespData = resp;
                else ApiError = resp?.Message ?? "Failed to load dashboard metrics.";
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load dashboard metrics: " + ex.Message;
            }
            try
            {
                var resp = helper.GetEventHubGuestTicketTypeResp(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1)
                    TicketTypeData = resp;
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
        public JsonResult GetEventHubRegistrationData(int eventId, int slotId, int ticketTypeId, bool isComplete,string search)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            var helper = new CommonHelper();
            try
            {
                return new JsonResult(helper.GetEventHubGuestRegistrationResp(userId.Value, eventId, slotId,ticketTypeId,isComplete,search));
            }
            catch (Exception ex)
            {
                return new JsonResult(new EventHubGuestRegistrationResp { Status = 0, Message = ex.Message });
            }
        }
    }
}
