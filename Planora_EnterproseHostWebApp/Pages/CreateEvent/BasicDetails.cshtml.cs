using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;
using Planora_EnterproseHostWebApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class BasicDetailsModel : PageModel
    {
        [BindProperty]
        public string LayoutType { get; set; }

        [BindProperty]
        public string AgeRestriction { get; set; }

        [BindProperty]
        public string Parking { get; set; }

        [BindProperty]
        public string Washrooms { get; set; }

        [BindProperty]
        public string Accessibility { get; set; }

        [BindProperty]
        public string Catering { get; set; }

        [BindProperty]
        public IFormFile CoverUpload { get; set; }

        public string EventType { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        public List<City> Cities { get; set; } = new List<City>();

        public string ApiError { get; private set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            EventType = HttpContext.Session.GetEventType();

            LoadDropdownData(userId.Value);

            return Page();
        }

        [RequestSizeLimit(50 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        public IActionResult OnPost(AddBasicDetailsReq model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            EventType = HttpContext.Session.GetEventType();

            var helper = new CommonHelper();

            model.UserId = userId.Value;
            model.hashValue = "";

            var coverFile = CoverUpload ?? Request.Form.Files.GetFile("CoverUpload");

            if (coverFile != null && coverFile.Length > 0)
            {
                try
                {
                    const long maxFileSize = 5 * 1024 * 1024;

                    if (coverFile.Length > maxFileSize)
                    {
                        throw new Exception($"File '{coverFile.FileName}' exceeds the maximum allowed size of 5 MB.");
                    }

                    string extension = Path.GetExtension(coverFile.FileName)?.ToLowerInvariant();

                    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    {
                        throw new Exception($"Invalid image format for '{coverFile.FileName}'. Only JPG, JPEG, and PNG are allowed.");
                    }

                    using (var ms = new MemoryStream())
                    {
                        coverFile.CopyTo(ms);
                        byte[] fileBytes = ms.ToArray();
                        model.ImageBase64 = Convert.ToBase64String(fileBytes);
                    }

                    model.FileName = coverFile.FileName;
                }
                catch (Exception ex)
                {
                    ApiError = "Cover image could not be processed: " + ex.Message;
                    LoadDropdownData(userId.Value);
                    return Page();
                }
            }
            Response basicResponse;

            try
            {
                basicResponse = helper.AddEventDetails(model);
            }
            catch (Exception ex)
            {
                ApiError = "Unable to reach the event service. " + ex.Message;
                LoadDropdownData(userId.Value);
                return Page();
            }

            if (basicResponse == null)
            {
                ApiError = "Unable to save event details. No response received.";
                LoadDropdownData(userId.Value);
                return Page();
            }

            if (basicResponse.Status != 1)
            {
                ApiError = !string.IsNullOrWhiteSpace(basicResponse.Message)
                    ? basicResponse.Message
                    : "Unable to save event details.";
                LoadDropdownData(userId.Value);
                return Page();
            }

            int eventId;

            try
            {
                eventId = Convert.ToInt32(basicResponse.EventId);
            }
            catch
            {
                ApiError = "Event was created but EventId is invalid.";
                LoadDropdownData(userId.Value);
                return Page();
            }

            if (eventId <= 0)
            {
                ApiError = "Event was created but a valid EventId was not returned.";
                LoadDropdownData(userId.Value);
                return Page();
            }

            HttpContext.Session.SetInt32("createdEventId", eventId);

            var things = new List<ThingToKnow>();

            if (!string.IsNullOrWhiteSpace(LayoutType))
            {
                things.Add(new ThingToKnow { Category = "LayoutType", value = LayoutType });
            }

            if (!string.IsNullOrWhiteSpace(AgeRestriction))
            {
                things.Add(new ThingToKnow { Category = "AgeRestriction", value = AgeRestriction });
            }

            things.Add(new ThingToKnow { Category = "Parking", value = Parking });

            if (!string.IsNullOrWhiteSpace(Washrooms))
            {
                things.Add(new ThingToKnow { Category = "Washrooms", value = Washrooms });
            }

            things.Add(new ThingToKnow { Category = "Accessibility", value = Accessibility });

            if (!string.IsNullOrWhiteSpace(Catering))
            {
                things.Add(new ThingToKnow { Category = "Catering", value = Catering });
            }

            var thingsReq = new AddThingToKnowReq
            {
                UserId = userId.Value,
                EventId = eventId,
                Things = things
            };

            AddThingToKnowResp thingsResponse;

            try
            {
                thingsResponse = helper.AddThingsToKnow(thingsReq);
            }
            catch (Exception ex)
            {
                ApiError = "Event was created, but we couldn't reach the service to save facility details. " + ex.Message;
                LoadDropdownData(userId.Value);
                return Page();
            }

            if (thingsResponse == null)
            {
                ApiError = "Event was created, but no response was received while saving facility details.";
                LoadDropdownData(userId.Value);
                return Page();
            }

            if (thingsResponse.Status != 1)
            {
                ApiError = !string.IsNullOrWhiteSpace(thingsResponse.Message)
                    ? thingsResponse.Message
                    : "Event was created, but facility details could not be saved.";
                LoadDropdownData(userId.Value);
                return Page();
            }

            TempData["EventId"] = eventId;

            return RedirectToPage("/CreateEvent/ScheduleBuilder");
        }

        private void LoadDropdownData(int userId)
        {
            var helper = new CommonHelper();

            try
            {
                GetEventCategoryResp categoryResponse = helper.GetEventCategory(userId);

                if (categoryResponse != null && categoryResponse.Status == 1)
                {
                    Categories = categoryResponse.Categories ?? new List<Category>();
                }
                else
                {
                    ApiError = categoryResponse?.Message ?? "Failed to load categories.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load categories: " + ex.Message;
            }

            try
            {
                GetCitiesResp cityResponse = helper.GetCityResp(userId, string.Empty);

                if (cityResponse != null && cityResponse.Status == 1)
                {
                    Cities = cityResponse.City ?? new List<City>();
                }
                else
                {
                    var cityError = cityResponse?.Message ?? "Failed to load cities.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? cityError : $"{ApiError} {cityError}";
                }
            }
            catch (Exception ex)
            {
                var cityError = "Failed to load cities: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? cityError : $"{ApiError} {cityError}";
            }
        }
    }
}