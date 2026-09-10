using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using System;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubPromosAddonsModel : PageModel
    {
        public EventHubAddonReportResponse AddonReport { get; set; } = new();
        public string ApiError { get; set; } = string.Empty;

        public IActionResult OnGet(int eventId, int slotId, string search = "")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            ViewData["EventId"] = eventId;
            ViewData["SlotId"] = slotId;

            try
            {
                var helper = new CommonHelper();
                var resp = helper.GetEventHubAddonsPromoResp(userId.Value, eventId, slotId, search);

                if (resp != null && resp.Status == 1)
                {
                    AddonReport = resp;
                }
                else
                {
                    ApiError = resp?.Message ?? "Failed to load Add-ons and Promo Code data.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "An error occurred while loading data: " + ex.Message;
            }

            return Page();
        }
    }
}