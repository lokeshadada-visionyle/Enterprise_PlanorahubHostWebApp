using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class ContentSectionModel : PageModel
    {
        [BindProperty]
        public AddEventPolicyReq PolicyRequest { get; set; } = new AddEventPolicyReq();

        [BindProperty]
        public string TermsAndConditions { get; set; }

        [BindProperty]
        public string ThingsToKnow { get; set; }

        public void OnGet()
        {
            PolicyRequest.IsPollsEnabled = true;
            PolicyRequest.CanGuestAttactPhoto = true;
            PolicyRequest.IsFeedbackEnabled = true;
        }

        public IActionResult OnPost()
        {
            long userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var eventId = HttpContext.Session.GetInt32("createdEventId");

            PolicyRequest.UserId = userId;
            PolicyRequest.EventId = eventId.Value;
            PolicyRequest.Description = new List<string>();

            if (!string.IsNullOrWhiteSpace(TermsAndConditions))
            {
                PolicyRequest.Description.Add("Terms & Conditions: " + TermsAndConditions.Trim());
            }

            if (!string.IsNullOrWhiteSpace(ThingsToKnow))
            {
                PolicyRequest.Description.Add("Things to Know: " + ThingsToKnow.Trim());
            }

            PolicyRequest.FAQs?.RemoveAll(f => string.IsNullOrWhiteSpace(f.Question) && string.IsNullOrWhiteSpace(f.Answer));

            var helper = new CommonHelper();
            
            AddEventPolicyResp response = helper.AddEventPolicyReq(PolicyRequest);

            if (response != null && response.Status == 1)
            {
                return RedirectToPage("/CreateEvent/TicketPricing");
            }

            ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while saving policies.");
            return Page();
        }
    }
}