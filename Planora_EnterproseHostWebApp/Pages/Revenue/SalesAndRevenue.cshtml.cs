using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Revenue
{
    public class RevenueModel : PageModel
    {
        public SalesAndRevenueResp SalesData { get; set; } = new();
        public SalesAndRevenueByEventResp EventSalesData { get; set; } = new();
        public SalesAndRevenueByDaysResp EventDaysData { get; set; } = new();
        public EventRefundsResp EventRefundsData { get; set; } = new();
        public string ApiError { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            LoadSalesAndRevenue(userId.Value);

            if (EventId == 0 && SalesData.Events.Any())
            {
                EventId = SalesData.Events.First().EventId;
            }

            if (EventId != 0)
            {
                LoadSalesAndRevenueByEvent(userId.Value, EventId);
                LoadSalesAndRevenueByDays(userId.Value, EventId);
                LoadEventRefunds(userId.Value, EventId);
            }

            return Page();

        }

        private void LoadSalesAndRevenue(int userId)
        {
            var helper = new CommonHelper();

            try
            {
                SalesAndRevenueResp response = helper.GetSalesAndRevenue(userId);

                if (response != null && response.Status == 1)
                {
                    SalesData = response;
                }
                else
                {
                    ApiError = response?.Message ?? "Failed to load sales and revenue data.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load sales and revenue data: " + ex.Message;
            }
        }

        private void LoadSalesAndRevenueByEvent(int userId, int eventId)
        {
            var helper = new CommonHelper();

            try
            {
                SalesAndRevenueByEventResp response = helper.GetSalesAndRevenueByEvent(userId, eventId);

                if (response != null && response.Status == 1)
                {
                    EventSalesData = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load event revenue breakdown.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load event revenue breakdown: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }

        private void LoadSalesAndRevenueByDays(int userId, int eventId)
        {
            var helper = new CommonHelper();

            try
            {
                SalesAndRevenueByDaysResp response = helper.GetSalesAndRevenueByDays(userId, eventId);

                if (response != null && response.Status == 1)
                {
                    EventDaysData = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load day/slot revenue breakdown.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load day/slot revenue breakdown: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }

        private void LoadEventRefunds(int userId, int eventId)
        {
            var helper = new CommonHelper();

            try
            {
                EventRefundsResp response = helper.GetEventRefunds(userId, eventId);

                if (response != null && response.Status == 1)
                {
                    EventRefundsData = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load refunds.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load refunds: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }
    }
}