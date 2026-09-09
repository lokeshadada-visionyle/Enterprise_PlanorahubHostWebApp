using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Login
{
    public class ProfileModel : PageModel
    {
        public EnterpriseResp Enterprise { get; set; } = new();
        public string ApiError { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        [BindProperty(Name = "full-name")]
        public string FullName { get; set; } = string.Empty;

        [BindProperty(Name = "work-email")]
        public string WorkEmail { get; set; } = string.Empty;

        [BindProperty(Name = "phone")]
        public string Phone { get; set; } = string.Empty;

        [BindProperty(Name = "org-name")]
        public string OrgName { get; set; } = string.Empty;

        [BindProperty(Name = "website")]
        public string Website { get; set; } = string.Empty;

        [BindProperty(Name = "settlement")]
        public string Settlement { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            LoadEnterprise(userId.Value);

            return Page();
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            // Load current values first, so fields not present on the
            // submitted panel (e.g. JoinedOn) aren't lost.
            LoadEnterprise(userId.Value);

            var helper = new CommonHelper();

            try
            {
                var updateResp = helper.UpdateEnterprise(
                    userId.Value,
                    string.IsNullOrWhiteSpace(FullName) ? Enterprise.UserName : FullName,
                    string.IsNullOrWhiteSpace(OrgName) ? Enterprise.CompanyName : OrgName,
                    Enterprise.JoinedOn,
                    string.IsNullOrWhiteSpace(WorkEmail) ? Enterprise.Email : WorkEmail,
                    string.IsNullOrWhiteSpace(Phone) ? Enterprise.PhoneNo : Phone,
                    string.IsNullOrWhiteSpace(Website) ? Enterprise.Website : Website,
                    string.IsNullOrWhiteSpace(Settlement) ? Enterprise.SettlementCurrency : Settlement);

                if (updateResp != null && updateResp.Status == 1)
                {
                    SuccessMessage = "Changes saved successfully.";
                }
                else
                {
                    ApiError = updateResp?.Message ?? "Failed to save changes.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "Failed to save changes: " + ex.Message;
            }

            // Reload fresh data so the page reflects what was actually saved.
            LoadEnterprise(userId.Value);

            return Page();
        }

        private void LoadEnterprise(int userId)
        {
            var helper = new CommonHelper();

            try
            {
                EnterpriseResp response = helper.GetEnterprise(userId);

                if (response != null && response.Status == 1)
                {
                    Enterprise = response;
                }
                else
                {
                    ApiError = response?.Message ?? "Failed to load account details.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load account details: " + ex.Message;
            }
        }
    }
}