using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class PayoutAccountModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("UserId") is null ||
                HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            // Private events never settle ticket money through this step —
            // bounce straight to Add-ons & Coupons even if someone reaches
            // this URL directly (sidebar link, back button, bookmark).
            if (HttpContext.Session.IsPrivateEvent())
            {
                return RedirectToPage("/CreateEvent/AddonsCoupons");
            }

            return Page();
        }
    }
}
