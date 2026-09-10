using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Plans
{
    public class MyPlanModel : PageModel
    {
        public EnterprisePlanDashResp PlanDash { get; set; } = new();
        public EnterprisePackageDetailsResp PackageDetails { get; set; } = new();
        public EnterpriseChannelCreditsDashResp ChannelCredits { get; set; } = new();
        public EnterpriseBillingResp Billing { get; set; } = new();
        public EnterpriseAuditReportResp AuditReportData { get; set; } = new();
        public string ApiError { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            LoadPlanDash(userId.Value);
            LoadPackageDetails(userId.Value);
            LoadChannelCredits(userId.Value);
            LoadBilling(userId.Value);
            LoadAuditReport(userId.Value);

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

        private void LoadPackageDetails(int userId)
        {
            var helper = new CommonHelper();

            try
            {
                EnterprisePackageDetailsResp response = helper.GetEnterprisePackageDetails(userId);

                if (response != null && response.Status == 1)
                {
                    PackageDetails = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load package details.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load package details: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }

        private void LoadChannelCredits(int userId)
        {
            var helper = new CommonHelper();

            try
            {
                EnterpriseChannelCreditsDashResp response = helper.GetEnterpriseChannelCreditsDash(userId);

                if (response != null && response.Status == 1)
                {
                    ChannelCredits = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load communication wallet.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load communication wallet: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }

        private void LoadBilling(int userId)
        {
            var helper = new CommonHelper();

            try
            {
                EnterpriseBillingResp response = helper.GetEnterpriseBilling(userId);

                if (response != null && response.Status == 1)
                {
                    Billing = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load billing details.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load billing details: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }

        private void LoadAuditReport(int userId)
        {
            var helper = new CommonHelper();

            try
            {
                EnterpriseAuditReportResp response = helper.GetEnterpriseAuditReport(userId);

                if (response != null && response.Status == 1)
                {
                    AuditReportData = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load audit report.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load audit report: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }
    }
}