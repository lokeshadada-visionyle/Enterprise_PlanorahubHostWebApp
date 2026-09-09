using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events
{
    public class EventHubModel : PageModel
    {
        public IActionResult OnGet(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToPage("/Login/Login");
            int slotId = 0;

            try
            {
                var helper = new CommonHelper();

                EventHubDatesAndSlotsResp resp = helper.GetEventHubDateAndSlot(userId.Value, eventId);

                if (resp != null && resp.Status == 1 && resp.EventDates != null)
                {
                    slotId = resp.EventDates
                                 .SelectMany(d => d.Slots)
                                 .FirstOrDefault()?.SlotId ?? 0;
                }
            }
            catch (Exception ex)
            {
                // Optionally handle/log error
            }

            return RedirectToPage("/Events/EventHub/EventHubOverView", new { eventId = eventId, slotId = slotId });
        }
    }
}