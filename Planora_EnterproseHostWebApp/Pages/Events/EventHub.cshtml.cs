using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Planora_EnterproseHostWebApp.Pages.Events
{
    public class EventHubModel : PageModel
    {
        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToPage("/Login/Login");

            int eventId = 2089;
            int slotId = 2000;

            return RedirectToPage("/Events/EventHub/EventHubOverView", new { eventId, slotId });
        }
    }
}