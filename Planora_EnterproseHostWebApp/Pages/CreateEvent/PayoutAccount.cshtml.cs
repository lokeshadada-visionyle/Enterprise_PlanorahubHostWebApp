using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class PayoutAccountModel : PageModel
    {
        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 6;
            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }
            if (HttpContext.Session.IsPrivateEvent())
            {
                return RedirectToPage("/CreateEvent/AddonsCoupons");
            }

            return Page();
        }
    }
}
