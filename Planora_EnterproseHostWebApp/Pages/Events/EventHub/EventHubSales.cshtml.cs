using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubSalesModel : PageModel
    {
        public EventHubSalesResponse salesResp { get; set; } = new();
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
                var resp = helper.GetEventHubSalesResp(userId.Value, eventId,slotId);
                if (resp != null && resp.Status == 1)
                    salesResp = resp;
                else
                    ApiError = resp?.Message ?? "Failed to load sales data.";
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load sales data: " + ex.Message;
            }

            return Page();
        }

        public JsonResult GetEventHubSalesData(int eventId,int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            var helper = new CommonHelper();
            try
            {
                return new JsonResult(helper.GetEventHubSalesResp(userId.Value, eventId,slotId));
            }
            catch (Exception ex)
            {
                return new JsonResult(new EventHubSalesResponse { Status = 0, Message = ex.Message });
            }
        }
    }
}