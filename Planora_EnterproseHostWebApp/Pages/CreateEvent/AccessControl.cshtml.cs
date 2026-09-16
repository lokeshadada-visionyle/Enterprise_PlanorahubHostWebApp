using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class AccessControlModel : PageModel
    {
        [BindProperty]
        public AddEventFieldsReq EventFields { get; set; } = new AddEventFieldsReq();
        public int ExistingFormId { get; set; }
        public string ApiError { get; set; } = string.Empty;
        public bool? RegistrationRequired { get; set; }
        public List<RegistrationFormSummary> DefaultTemplates { get; set; } = new();
        public int? SelectedTemplateId { get; set; }
        public bool HasExistingForm { get; set; }
        public string SelectedDiscovery { get; set; }   // "listed" | "standalone"
        public string SelectedAccess { get; set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var formId = HttpContext.Session.GetInt32("registrationFormId");
            ViewData["StepIndex"] = 9;
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
                return RedirectToPage("/Login/Login");

            if (eventId is null)
            {
                ApiError = "We couldn't find your event in progress. Please start again.";
                return RedirectToPage("/CreateEvent/EventType");
            }

            var helper = new CommonHelper();

            var accessResp = helper.GetAccessRegistration(new DummyRequest
            {
                UserId = userId.Value,
                EventId = eventId.Value
            });

            if (accessResp != null && accessResp.Status == 1)
            {
                SelectedAccess = accessResp.AccessGatewayMode;
                SelectedDiscovery = accessResp.IsUnListed ? "standalone" : "listed";

                var tplId = HttpContext.Session.GetInt32("SelectedTemplateFormId");
                if (tplId.HasValue) SelectedTemplateId = tplId.Value;
            }

            var regRequiredStr = HttpContext.Session.GetString("RegistrationRequired");
            if (!string.IsNullOrEmpty(regRequiredStr))
            {
                RegistrationRequired = regRequiredStr.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            if (formId.HasValue && formId.Value > 0)
            {
                var existing = helper.GetCustomRegistrationForm(userId.Value, formId.Value);
                if (existing != null && existing.Status == 1 && existing.Form != null && existing.Form.Fields.Any())
                {
                    ExistingFormId = existing.Form.FormId;
                    EventFields.UserId = userId.Value;
                    EventFields.EventId = eventId.Value;
                    EventFields.Fields = existing.Form.Fields
                        .OrderBy(f => f.SortOrder)
                        .Select(f => new FieldItem
                        {
                            FieldId = f.FieldId,
                            Label = f.Label,
                            FieldType = f.FieldType,
                            IsRequired = f.IsRequired
                        })
                        .ToList();
                    HasExistingForm = true;
                }
            }

            var templates = helper.GetDefaultRegistrationForms(userId.Value, eventId.Value);
            if (templates != null && templates.Status == 1)
                DefaultTemplates = templates.RegistrationForms ?? new();
            else
                ApiError = templates?.Message ?? "Failed to load form templates.";

            return Page();
        }

        public IActionResult OnPostDeleteField([FromBody] DeleteEventFieldsReq body)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value <= 0)
                return new JsonResult(new { success = false, message = "Not authenticated." }) { StatusCode = 401 };

            if (body == null || body.FieldId <= 0)
                return new JsonResult(new { success = false, message = "Invalid field id." }) { StatusCode = 400 };

            var helper = new CommonHelper();
            var response = helper.DeleteCustomFormFieldReq(new DeleteEventFieldsReq
            {
                UserId = userId.Value,
                FieldId = body.FieldId
            });

            if (response == null)
                return new JsonResult(new { success = false, message = "No response from service." });

            return new JsonResult(new { success = response.Status == 1, message = response.Message });
        }

        // AJAX handler: GET /CreateEvent/AccessControl?handler=TemplateFields&formId=3
        public JsonResult OnGetTemplateFields(int formId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            if (userId is null || eventId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return new JsonResult(new { success = false, message = "Session expired. Please log in again." });
            }

            var helper = new CommonHelper();
            var resp = helper.GetDefaultRegistrationFormsById(userId.Value, eventId.Value, formId);

            if (resp == null || resp.Status != 1)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = resp?.Message ?? "Failed to load template fields."
                });
            }
            HttpContext.Session.SetInt32("SelectedTemplateFormId", formId);
            return new JsonResult(new
            {
                success = true,
                fields = resp.Fields.OrderBy(f => f.SortOrder).Select(f => new
                {
                    label = f.Label,
                    fieldType = f.FieldType,
                    isRequired = f.IsRequired
                })
            });
        }

        public IActionResult OnPost(string? actionType)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || userId == 0 || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            var eventId = HttpContext.Session.GetInt32("createdEventId");

            if (eventId is null)
            {
                ApiError = "Event ID is missing.";
                return Page();
            }

            bool isPrivate = HttpContext.Session.IsPrivateEvent();

            if (actionType == "back")
            {
                return isPrivate
                    ? RedirectToPage("/CreateEvent/FoodBeverage")
                    : RedirectToPage("/CreateEvent/AddonsCoupons");
            }

            if (actionType == "skip" && isPrivate)
            {
                return RedirectToPage("/CreateEvent/RSVP");
            }

            bool registrationRequired = string.Equals(Request.Form["reg-required"].ToString(), "yes", StringComparison.OrdinalIgnoreCase);
            HttpContext.Session.SetString("RegistrationRequired", registrationRequired ? "true" : "false");

            string accessMode = Request.Form["access"].ToString();
            string discovery = Request.Form["discovery"].ToString();

            bool isUnListed = string.Equals(discovery, "standalone", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(discovery))
            {
                isUnListed = isPrivate;
            }
            if (string.IsNullOrWhiteSpace(accessMode))
            {
                accessMode = isPrivate ? "invite" : "open";
            }

            var helper = new CommonHelper();

            var accessRequest = new EventAccessModel
            {
                UserId = userId.Value,
                EventId = eventId.Value,
                AccessGatewayMode = accessMode,
                IsUnListed = isUnListed
            };

            var accessResponse = helper.AddAccessRegistration(accessRequest);

            if (accessResponse == null || accessResponse.Status != 1)
            {
                ApiError = accessResponse?.Message ?? "Failed to save registration access.";
                return Page();
            }

            if (!registrationRequired)
            {
                int currentProg = HttpContext.Session.GetInt32("StepProgress") ?? 0;
                HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProg, 10));
                return isPrivate
                    ? RedirectToPage("/CreateEvent/RSVP")
                    : RedirectToPage("/CreateEvent/BrandingPage");
            }
            EventFields.UserId = userId.Value;
            EventFields.EventId = eventId.Value;
            EventFields.FormTitle = "Event Registration Form";

            int savedFormId;
            bool success;
            string? errMsg;
            var existingFormId = HttpContext.Session.GetInt32("registrationFormId");
            bool hasExistingForm = false;
            int resolvedExistingFormId = 0;
            if (existingFormId.HasValue && existingFormId.Value > 0)
            {
                var existingCheck = helper.GetCustomRegistrationForm(userId.Value, existingFormId.Value);
                if (existingCheck != null && existingCheck.Status == 1 && existingCheck.Form != null)
                {
                    hasExistingForm = true;
                    resolvedExistingFormId = existingCheck.Form.FormId;
                }
            }
            if (hasExistingForm && resolvedExistingFormId > 0)
            {
                var updateResp = helper.UpdateRegistrationFormRe(new UpdateEventFieldsReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    FormId = resolvedExistingFormId,
                    FormTitle = "Event Registration Form",
                    Fields = (EventFields.Fields ?? new List<FieldItem>()).Select(f => new UpdateFieldItem
                    {
                        FieldId = f.FieldId ?? 0,
                        Label = f.Label,
                        FieldType = f.FieldType,
                        IsRequired = f.IsRequired
                    }).ToList()
                });
                success = updateResp != null && updateResp.Status == 1;
                errMsg = updateResp?.Message;
                savedFormId = resolvedExistingFormId;
            }
            else
            {
                var addResp = helper.AddRegistrationFormReq(EventFields);
                success = addResp != null && addResp.Status == 1;
                errMsg = addResp?.Message;
                savedFormId = addResp?.FormId ?? 0;
            }

            if (success)
            {
                HttpContext.Session.SetInt32("registrationFormId", savedFormId);

                // Persist selections so OnGet can restore them on "Back"
                HttpContext.Session.SetString("AccessGatewayMode", accessMode);
                HttpContext.Session.SetString("DiscoveryMode", isUnListed ? "standalone" : "listed");

                int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
                HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 9));
                return isPrivate
                    ? RedirectToPage("/CreateEvent/RSVP")
                    : RedirectToPage("/CreateEvent/BrandingPage");
            }

            ApiError = errMsg ?? "Failed to save registration fields.";
            return Page();
        }
    }
}