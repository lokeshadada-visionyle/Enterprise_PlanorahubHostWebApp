using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Plans
{
    public class MyPlanModel : PageModel
    {
        public EnterprisePlanDashResp PlanDash { get; set; } = new();
        public string ApiError { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            LoadPlanDash(userId.Value);

            return Page();
        }

        private void LoadPlanDash(int userId)
        {
            var helper = new CommonHelper();

            try
            {
                EnterprisePlanDashResp response = helper.GetEnterprisePlanDash(userId);

                if (response != null && response.Status == 1)
                {
                    PlanDash = response;
                }
                else
                {
                    ApiError = response?.Message ?? "Failed to load plan dashboard.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load plan dashboard: " + ex.Message;
            }
        }
    }
}