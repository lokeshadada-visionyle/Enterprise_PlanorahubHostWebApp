using Microsoft.AspNetCore.Authorization;
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

        public bool IsEditMode { get; set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 2;

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            EventType = HttpContext.Session.GetEventType();
            LoadDropdownData(userId.Value);

            var createdEventId = HttpContext.Session.GetInt32("createdEventId");
            IsEditMode = createdEventId.HasValue && createdEventId.Value > 0;

            if (createdEventId.HasValue && createdEventId.Value > 0)
            {
                var helper = new CommonHelper();

                try
                {
                    var eventDetails = helper.GetEventDetailsById(userId.Value, createdEventId.Value);
                    var eventThinkToKnow = helper.GetEventThinksToKnowById(userId.Value, createdEventId.Value);

                    if (eventDetails != null)
                    {
                        ViewData["EventName"] = eventDetails.EventName;
                        ViewData["EventCategory"] = eventDetails.EventCategory;
                        ViewData["TagLine"] = eventDetails.TagLine;
                        ViewData["CityId"] = eventDetails.CityId;
                        ViewData["Description"] = eventDetails.Description;
                        ViewData["ImageUrl"] = eventDetails.ImageUrl;
                    }

                    if (eventThinkToKnow != null && eventThinkToKnow.Things != null)
                    {
                        string GetValue(string categoryName) =>
                            eventThinkToKnow.Things
                                .FirstOrDefault(t => string.Equals(t.Category, categoryName, StringComparison.OrdinalIgnoreCase))?.Value;

                        LayoutType = GetValue("LayoutType");
                        AgeRestriction = GetValue("AgeRestriction");
                        Parking = GetValue("Parking");
                        Washrooms = GetValue("Washrooms");
                        Accessibility = GetValue("Accessibility");
                        Catering = GetValue("Catering");
                    }
                }
                catch (Exception ex)
                {
                    ApiError = "Could not reload existing event details: " + ex.Message;
                }
            }

            return Page();
        }

        [RequestSizeLimit(50 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        public IActionResult OnPost(AddBasicDetailsReq model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            EventType = HttpContext.Session.GetEventType();
            int currentMaxProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
            HttpContext.Session.SetInt32("StepProgress", Math.Max(currentMaxProgress, 3));
            var helper = new CommonHelper();

            var existingEventId = HttpContext.Session.GetInt32("createdEventId");
            IsEditMode = existingEventId.HasValue && existingEventId.Value > 0;

            model.UserId = userId.Value;
            model.hashValue = "";

            var coverFile = CoverUpload ?? Request.Form.Files.GetFile("CoverUpload");
            string newlyUploadedImageUrl = null;

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

                    string imageBase64;

                    using (var ms = new MemoryStream())
                    {
                        coverFile.CopyTo(ms);
                        byte[] fileBytes = ms.ToArray();
                        imageBase64 = Convert.ToBase64String(fileBytes);
                    }

                    if (IsEditMode)
                    {
                        var uploadReq = new UploadCoverImageReq
                        {
                            UserId = userId.Value,
                            EventId = existingEventId.Value,
                            FileName = coverFile.FileName,
                            ImageBase64 = imageBase64
                        };

                        UploadCoverImageResp uploadResp;

                        try
                        {
                            uploadResp = helper.UploadCoverImage(uploadReq);
                        }
                        catch (Exception ex)
                        {
                            ApiError = "Unable to reach the image upload service. " + ex.Message;
                            LoadDropdownData(userId.Value);
                            return Page();
                        }
                        if (uploadResp == null || uploadResp.Status != 1 || string.IsNullOrWhiteSpace(uploadResp.ImageURL))
                        {
                            ApiError = !string.IsNullOrWhiteSpace(uploadResp?.Message)
                                ? uploadResp.Message
                                : "Unable to upload the new cover image.";
                            LoadDropdownData(userId.Value);
                            return Page();
                        }

                        newlyUploadedImageUrl = uploadResp.ImageURL;
                    }
                    else
                    {
                        model.ImageBase64 = imageBase64;
                        model.FileName = coverFile.FileName;
                    }
                }
                catch (Exception ex)
                {
                    ApiError = "Cover image could not be processed: " + ex.Message;
                    LoadDropdownData(userId.Value);
                    return Page();
                }
            }

            int eventId;

            if (IsEditMode)
            {
                eventId = existingEventId.Value;

                var postedExistingImageUrl = Request.Form["existingImageUrl"].ToString();
                var finalImageUrl = !string.IsNullOrWhiteSpace(newlyUploadedImageUrl)
                    ? newlyUploadedImageUrl
                    : postedExistingImageUrl;

                var updateReq = new UpdateEventDetailsRequest
                {
                    UserId = userId.Value,
                    EventId = eventId,
                    EventType = EventType,
                    EventName = model.EventName,
                    EventCategory = model.EventCategory.ToString(),
                    TagLine = model.TagLine,
                    CityId = model.CityId,
                    TimeZone = model.TimeZone,
                    description = model.description,
                    ImageUrl = finalImageUrl
                };

                Response updateResponse;

                try
                {
                    updateResponse = helper.UpdateEventDetails(updateReq);
                }
                catch (Exception ex)
                {
                    ApiError = "Unable to reach the event service. " + ex.Message;
                    LoadDropdownData(userId.Value);
                    return Page();
                }

                if (updateResponse == null || updateResponse.Status != 1)
                {
                    ApiError = !string.IsNullOrWhiteSpace(updateResponse?.Message)
                        ? updateResponse.Message
                        : "Unable to update event details.";
                    LoadDropdownData(userId.Value);
                    return Page();
                }
            }
            else
            {
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

                if (basicResponse == null || basicResponse.Status != 1)
                {
                    ApiError = !string.IsNullOrWhiteSpace(basicResponse?.Message)
                        ? basicResponse.Message
                        : "Unable to save event details.";
                    LoadDropdownData(userId.Value);
                    return Page();
                }

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
            }

            HttpContext.Session.SetInt32("createdEventId", eventId);

            var things = new List<ThingToKnow>();

            if (!string.IsNullOrWhiteSpace(LayoutType))
            {
                things.Add(new ThingToKnow { Category = "LayoutType", Value = LayoutType });
            }

            if (!string.IsNullOrWhiteSpace(AgeRestriction))
            {
                things.Add(new ThingToKnow { Category = "AgeRestriction", Value = AgeRestriction });
            }

            if (!string.IsNullOrWhiteSpace(Parking))
            {
                things.Add(new ThingToKnow { Category = "Parking", Value = Parking });
            }

            if (!string.IsNullOrWhiteSpace(Washrooms))
            {
                things.Add(new ThingToKnow { Category = "Washrooms", Value = Washrooms });
            }

            if (!string.IsNullOrWhiteSpace(Accessibility))
            {
                things.Add(new ThingToKnow { Category = "Accessibility", Value = Accessibility });
            }

            if (!string.IsNullOrWhiteSpace(Catering))
            {
                things.Add(new ThingToKnow { Category = "Catering", Value = Catering });
            }

            if (IsEditMode)
            {
                // ATTACH EXISTING ITEM IDs BEFORE UPDATING
                try
                {
                    var existingThingsResp = helper.GetEventThinksToKnowById(userId.Value, eventId);
                    if (existingThingsResp != null && existingThingsResp.Things != null)
                    {
                        foreach (var thing in things)
                        {
                            var existingItem = existingThingsResp.Things.FirstOrDefault(t =>
                                string.Equals(t.Category, thing.Category, StringComparison.OrdinalIgnoreCase));

                            if (existingItem != null)
                            {
                                thing.ThingToKnowItemId = existingItem.ThingToKnowItemId;
                            }
                        }
                    }
                }
                catch
                {
                    // Fail gracefully if fetching existing item IDs encounters an issue
                }

                var updateThingsReq = new AddThingToKnowReq
                {
                    UserId = userId.Value,
                    EventId = eventId,
                    Things = things
                };

                AddThingToKnowResp updateThingsResponse;

                try
                {
                    updateThingsResponse = helper.UpdateThingsToKnow(updateThingsReq);
                }
                catch (Exception ex)
                {
                    ApiError = "Event details were updated, but we couldn't reach the service to update facility details. " + ex.Message;
                    LoadDropdownData(userId.Value);
                    return Page();
                }

                if (updateThingsResponse == null || updateThingsResponse.Status != 1)
                {
                    ApiError = !string.IsNullOrWhiteSpace(updateThingsResponse?.Message)
                        ? updateThingsResponse.Message
                        : "Event details were updated, but facility details could not be updated.";
                    LoadDropdownData(userId.Value);
                    return Page();
                }
            }
            else
            {
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

                if (thingsResponse == null || thingsResponse.Status != 1)
                {
                    ApiError = !string.IsNullOrWhiteSpace(thingsResponse?.Message)
                        ? thingsResponse.Message
                        : "Event was created, but facility details could not be saved.";
                    LoadDropdownData(userId.Value);
                    return Page();
                }
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