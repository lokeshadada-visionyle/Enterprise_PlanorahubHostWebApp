using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubInsightsModel : PageModel
    {
        public EventHubInsightResponse insightsResp { get; set; } = new();
        public EventHubSalesResponse salesResp { get; set; } = new();
        public EventHubPollAnalyticsResponse pollAnalyticsResp { get; set; } = new();
        public string ApiError { get; set; } = string.Empty;

        public IActionResult OnGet(int eventId, int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToPage("/Login/Login");

            ViewData["EventId"] = eventId;
            ViewData["SlotId"] = slotId;

            var helper = new CommonHelper();

            // Fetch General Insights
            try
            {
                var resp = helper.GetEventHubInsightsResp(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1)
                    insightsResp = resp;
                else
                    ApiError = resp?.Message ?? "Failed to load insights.";
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load insights: " + ex.Message;
            }

            // Fetch Sales Summary
            try
            {
                var resp = helper.GetEventHubSalesSummaryResp(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1)
                    pollAnalyticsResp = resp;
                else if (string.IsNullOrEmpty(ApiError))
                    ApiError = resp?.Message ?? "Failed to load sales summary.";
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ApiError))
                    ApiError = "Failed to load sales summary: " + ex.Message;
            }

            // Fetch Poll Analytics
            try
            {
                var resp = helper.GetEventHubSalesSummaryResp(userId.Value, eventId, slotId);
                if (resp != null && resp.Status == 1)
                    pollAnalyticsResp = resp;
                else if (string.IsNullOrEmpty(ApiError))
                    ApiError = resp?.Message ?? "Failed to load poll analytics.";
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ApiError))
                    ApiError = "Failed to load poll analytics: " + ex.Message;
            }

            return Page();
        }

        public JsonResult GetEventHubInsightsData(int eventId, int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            var helper = new CommonHelper();
            try
            {
                return new JsonResult(helper.GetEventHubInsightsResp(userId.Value, eventId, slotId));
            }
            catch (Exception ex)
            {
                return new JsonResult(new EventHubInsightResponse { Status = 0, Message = ex.Message });
            }
        }
    }
}