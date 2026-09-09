using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubPayoutModel : PageModel
    {
        public void OnGet(int eventId, int slotId)
        {
            ViewData["EventId"] = eventId;
            ViewData["SlotId"] = slotId;
        }
    }
}
