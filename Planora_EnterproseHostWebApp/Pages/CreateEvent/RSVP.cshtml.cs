using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class RSVPModel : PageModel
    {
        public string? RsvpError { get; private set; }
        public GetRsvpCustomsResp? Existing { get; private set; }
        public List<CustomFormTemplate> TemplateList { get; private set; } = new List<CustomFormTemplate>();

        public bool HasExisting => Existing != null && Existing.Status == 1;

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            ViewData["StepIndex"] = 10;

            if (!userId.HasValue || userId.Value <= 0 ||
                !eventId.HasValue || eventId.Value <= 0 ||
                string.IsNullOrWhiteSpace(isLoggedIn) ||
                !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            var helper = new CommonHelper();

            // 1. Fetch Existing RSVP Settings
            try
            {
                Existing = helper.GetRsvpCustoms(userId.Value, eventId.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetRsvpCustoms ERROR: " + ex.Message);
                Existing = null;
            }

            // 2. Always fetch template list for the drop-down selector
            LoadTemplatesForPage(userId.Value, eventId.Value);

            return Page();
        }

        public IActionResult OnGetLoadTemplate([FromQuery(Name = "formTitle")] string? formTitle)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 ||
                !eventId.HasValue || eventId.Value <= 0 ||
                string.IsNullOrWhiteSpace(isLoggedIn) ||
                !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { status = 0, message = "Session expired. Please login again." });
            }

            if (string.IsNullOrWhiteSpace(formTitle))
            {
                return new JsonResult(new { status = 0, message = "Please select a template." });
            }

            var helper = new CommonHelper();

            try
            {
                var response = helper.GetDefaultRsvpFormsByTitle(userId.Value, formTitle.Trim());

                if (response != null && response.Status == 1)
                {
                    return new JsonResult(new
                    {
                        status = 1,
                        fields = response.Fields ?? new List<TemplateField>()
                    });
                }

                return new JsonResult(new
                {
                    status = 0,
                    message = response?.Message ?? "No fields were returned for this template."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Load Template ERROR: " + ex.Message);
                return new JsonResult(new { status = 0, message = "Unable to load the selected template." });
            }
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 ||
                !eventId.HasValue || eventId.Value <= 0 ||
                string.IsNullOrWhiteSpace(isLoggedIn) ||
                !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            bool isRsvpEnabled = string.Equals(
                Request.Form["rsvp-required"].ToString(),
                "yes",
                StringComparison.OrdinalIgnoreCase
            );

            List<CustomFields> custom = isRsvpEnabled ? ParseCustomFields() : new List<CustomFields>();
            var helper = new CommonHelper();
            Response? result;
            bool isUpdate = false;

            try
            {
                var existingResponse = helper.GetRsvpCustoms(userId.Value, eventId.Value);
                isUpdate = existingResponse != null && existingResponse.Status == 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RSVP POST - Get Existing ERROR: " + ex.Message);
                isUpdate = false;
            }

            try
            {
                if (isUpdate)
                {
                    var updateRequest = new UpdateRsvpCustomsReq
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        IsRsvpEnabled = isRsvpEnabled,
                        Custom = custom.Select(c => new UpdateCustomFields
                        {
                            CustomName = c.CustomName,
                            IsRequired = c.IsRequired
                        }).ToList()
                    };

                    result = helper.updateRsvpCustoms(updateRequest);
                }
                else
                {
                    var addRequest = new AddRsvpCustomsReq
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        IsRsvpEnabled = isRsvpEnabled,
                        Custom = custom
                    };

                    result = helper.AddRsvpCustoms(addRequest);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RSVP SAVE ERROR: " + ex.Message);
                RsvpError = "Unable to save RSVP settings right now. Please try again.";
                LoadTemplatesForPage(userId.Value, eventId.Value);
                return Page();
            }

            if (result == null || result.Status != 1)
            {
                RsvpError = result != null && !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Unable to save your RSVP settings.";

                LoadTemplatesForPage(userId.Value, eventId.Value);
                return Page();
            }

            int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
            HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 11));

            return RedirectToPage("/CreateEvent/BrandingPage");
        }

        private List<CustomFields> ParseCustomFields()
        {
            var labelRegex = new Regex(@"^rq-(\d+)-label$", RegexOptions.Compiled);
            var result = new List<CustomFields>();

            foreach (var key in Request.Form.Keys)
            {
                var match = labelRegex.Match(key);
                if (!match.Success) continue;

                if (!int.TryParse(match.Groups[1].Value, out int index)) continue;

                string label = Request.Form[$"rq-{index}-label"].ToString();
                if (string.IsNullOrWhiteSpace(label)) continue;

                bool required = Request.Form[$"rq-{index}-req"].Any(value =>
                    string.Equals(value, "on", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "true", StringComparison.OrdinalIgnoreCase));

                result.Add(new CustomFields
                {
                    CustomId = 0,
                    CustomName = label.Trim(),
                    IsRequired = required
                });
            }

            return result;
        }

        private void LoadTemplatesForPage(int userId, int eventId)
        {
            try
            {
                var helper = new CommonHelper();
                var response = helper.GetRsvpDefaultCustomForms(userId, eventId);

                if (response != null && response.Status == 1 && response.CustomForms != null)
                {
                    TemplateList = response.CustomForms;
                }
                else
                {
                    TemplateList = new List<CustomFormTemplate>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetRsvpDefaultCustomForms ERROR: " + ex.Message);
                TemplateList = new List<CustomFormTemplate>();
            }
        }
    }
}