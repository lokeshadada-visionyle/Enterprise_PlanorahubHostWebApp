using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        // Tracks whether this event already has saved policy data.
        [BindProperty]
        public bool HasExistingData { get; set; }

        private const string TermsPrefix = "Terms & Conditions: ";
        private const string ThingsPrefix = "Things to Know: ";

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 4;

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            var eventId = HttpContext.Session.GetInt32("createdEventId");
            if (!eventId.HasValue)
            {
                return RedirectToPage("/CreateEvent/ScheduleBuilder");
            }

            PolicyRequest.EventId = eventId.Value;
            PolicyRequest.FAQs = new List<FAQItem>();

            var helper = new CommonHelper();
            EventFAQResponse existing = helper.GetEventFaqResp(userId.Value, eventId.Value);

            if (existing != null && existing.Status == 1)
            {
                // Existing data found -> set flag to trigger Update API on form submission
                HasExistingData = true;

                PolicyRequest.IsPollsEnabled = existing.IsPollsEnabled;
                PolicyRequest.IsFeedbackEnabled = existing.IsFeedbackEnabled;
                PolicyRequest.CanGuestAttactPhoto = existing.CanGuestAttactPhoto;

                foreach (var line in existing.Description ?? new List<string>())
                {
                    if (line.StartsWith(TermsPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        TermsAndConditions = line.Substring(TermsPrefix.Length);
                    }
                    else if (line.StartsWith(ThingsPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        ThingsToKnow = line.Substring(ThingsPrefix.Length);
                    }
                }

                if (existing.FAQs != null && existing.FAQs.Count > 0)
                {
                    PolicyRequest.FAQs = existing.FAQs
                        .Select(f => new FAQItem
                        {
                            FAQItemId = f.FAQItemId,
                            Question = f.Question,
                            Answer = f.Answer
                        })
                        .ToList();
                }
            }
            else
            {
                // Fresh form defaults
                HasExistingData = false;
                PolicyRequest.IsPollsEnabled = true;
                PolicyRequest.CanGuestAttactPhoto = true;
                PolicyRequest.IsFeedbackEnabled = true;
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            long userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var eventId = HttpContext.Session.GetInt32("createdEventId");

            if (!eventId.HasValue)
            {
                return RedirectToPage("/CreateEvent/ScheduleBuilder");
            }

            PolicyRequest.UserId = userId;
            PolicyRequest.EventId = eventId.Value;
            PolicyRequest.Description = new List<string>();

            if (!string.IsNullOrWhiteSpace(TermsAndConditions))
            {
                PolicyRequest.Description.Add(TermsPrefix + TermsAndConditions.Trim());
            }

            if (!string.IsNullOrWhiteSpace(ThingsToKnow))
            {
                PolicyRequest.Description.Add(ThingsPrefix + ThingsToKnow.Trim());
            }

            PolicyRequest.FAQs?.RemoveAll(f => string.IsNullOrWhiteSpace(f.Question) && string.IsNullOrWhiteSpace(f.Answer));

            var helper = new CommonHelper();

            if (HasExistingData)
            {
                // Construct Update API Request
                var updateRequest = new UpdateEventFAQRequest
                {
                    UserId = userId,
                    EventId = eventId.Value,
                    IsPollsEnabled = PolicyRequest.IsPollsEnabled,
                    IsFeedbackEnabled = PolicyRequest.IsFeedbackEnabled,
                    CanGuestAttactPhoto = PolicyRequest.CanGuestAttactPhoto,
                    Description = PolicyRequest.Description,
                    FAQs = PolicyRequest.FAQs?.Select(f => new UpdateFAQItem
                    {
                        FAQItemId = f.FAQItemId,
                        Question = f.Question,
                        Answer = f.Answer
                    }).ToList() ?? new List<UpdateFAQItem>()
                };

                Response updateResponse = helper.UpdateEventPolicyReq(updateRequest);

                if (updateResponse != null)
                {
                    int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
                    HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 5));
                    return RedirectToPage("/CreateEvent/TicketPricing");
                }

                ModelState.AddModelError(string.Empty, "An error occurred while updating policies.");
            }
            else
            {
                // Call Add/Create API for initial save
                AddEventPolicyResp response = helper.AddEventPolicyReq(PolicyRequest);

                if (response != null && response.Status == 1)
                {
                    int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
                    HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 5));
                    return RedirectToPage("/CreateEvent/TicketPricing");
                }

                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while saving policies.");
            }

            return Page();
        }
    }
}