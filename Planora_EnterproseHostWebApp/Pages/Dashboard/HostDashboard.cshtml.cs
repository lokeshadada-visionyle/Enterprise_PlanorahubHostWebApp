using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Dashboard
{
    public class HostDashboardModel : PageModel
    {
        public HostDashboardResp Dashboard { get; set; } = new();
        public List<WeeklyEvents> WeeklyEventsData { get; set; } = new();
        public List<LiveEvents> LiveEventsData { get; set; } = new();
        public string WeeklyPer { get; set; } = string.Empty;
        public string ApiError { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            LoadDashboardData(userId.Value);

            return Page();
        }
        public IActionResult OnPost()
        {
            return RedirectToPage("/Dashboard/HostDashboard");
        }

        private void LoadDashboardData(int userId)
        {
            var helper = new CommonHelper();

            try
            {
                HostDashboardResp dashboardResponse = helper.GetHostDashboard(userId);

                if (dashboardResponse != null && dashboardResponse.Status == 1)
                {
                    Dashboard = dashboardResponse;
                }
                else
                {
                    ApiError = dashboardResponse?.Message ?? "Failed to load dashboard metrics.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load dashboard metrics: " + ex.Message;
            }

            try
            {
                HostDashboardWeelkyResp weeklyResponse = helper.GetHostDashboardWeekly(userId);

                if (weeklyResponse != null && weeklyResponse.Status == 1)
                {
                    WeeklyEventsData = weeklyResponse.WeeklyEvents ?? new List<WeeklyEvents>();
                    WeeklyPer = weeklyResponse.WeeklyPer ?? string.Empty;
                }
                else
                {
                    var error = weeklyResponse?.Message ?? "Failed to load weekly events.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load weekly events: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }

            try
            {
                LiveEventsResp liveEventsResponse = helper.GetLiveEvent(userId);

                if (liveEventsResponse != null && liveEventsResponse.Status == 1)
                {
                    LiveEventsData = liveEventsResponse.LiveEvents ?? new List<LiveEvents>();
                }
                else
                {
                    var error = liveEventsResponse?.Message ?? "Failed to load live events.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load live events: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }
    }
}