using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubFoodVendorsModel : PageModel
    {
        public EventHubFoodMenuResponse eventHubFoodMenuData { get; set; } = new EventHubFoodMenuResponse();

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
                var resp = helper.GetEventHubFoodMenuResp(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1)
                    eventHubFoodMenuData = resp;
                else
                    errors.Add(resp?.Message ?? "Failed to load dashboard metrics.");
            }
            catch (Exception ex)
            {
                errors.Add("Failed to load dashboard metrics: " + ex.Message);
            }
            return Page();
        }

        public IActionResult OnPostToggleReadyToOrder(int eventId, int slotId, bool isReady)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { success = false, message = "Session expired." });

            var helper = new CommonHelper();
            var req = new EventHubFoodReadyToOrderreq
            {
                UserId = userId.Value,
                EventId = eventId,
                SLotId = slotId,
                Ready = isReady
            };

            var response = helper.UpdateReadyToOrder(req);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new { success = true, message = response.Message });
            }

            return new JsonResult(new { success = false, message = response?.Message ?? "Failed to update status." });
        }
        public IActionResult OnGetGetServingList(int eventId, int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { status = 0, message = "Session expired." });

            var helper = new CommonHelper();
            var response = helper.GetEventHubServingListResp(userId.Value, eventId, slotId);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new
                {
                    status = 1,
                    totalServes = response.TotalServes,
                    totalRefused = response.TotalRefused,
                    itemsOutOfStock = response.ItemsOutOfStock,
                    servings = response.Servings
                });
            }

            return new JsonResult(new { status = 0, message = response?.Message ?? "Failed to fetch serving list." });
        }
    }
}