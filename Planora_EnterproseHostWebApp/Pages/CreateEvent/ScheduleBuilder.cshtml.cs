using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class ScheduleBuilderModel : PageModel
    {
        private static readonly JsonSerializerOptions PreserveCasingJsonOptions =
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };

        public List<City> Cities { get; set; } = new();

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 3;
            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            try
            {
                var helper = new CommonHelper();
                var response = helper.GetCityResp(userId.Value, string.Empty);
                if (response?.City != null)
                {
                    Cities = response.City;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cities: {ex.Message}");
            }

            return Page();
        }

        public IActionResult OnPostSaveVenue([FromBody] SaveVenuePayload payload)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            int? eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid. Please select an event first." });
            }

            if (payload?.Venues == null || !payload.Venues.Any())
            {
                return new JsonResult(new { success = false, message = "Please add at least one venue." });
            }

            try
            {
                var helper = new CommonHelper();
                var request = new AddVenueAndHallReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    Venues = payload.Venues
                };

                var response = helper.AddVenueAndHall(request);

                if (response != null && (response.Status == 1 || response.Status == 200))
                {
                    return new JsonResult(new
                    {
                        success = true,
                        message = response.Message ?? "Venue and halls saved successfully."
                    });
                }

                return new JsonResult(new { success = false, message = response?.Message ?? "Failed to save venue and halls." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save Venue Error: {ex}");
                return new JsonResult(new { success = false, message = "An error occurred while saving venue and hall details." });
            }
        }

        public IActionResult OnPostUpdateVenue([FromBody] UpdateVenuePayload payload)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            int? eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid. Please select an event first." });
            }

            if (payload?.Venues == null || !payload.Venues.Any())
            {
                return new JsonResult(new { success = false, message = "Please add at least one venue." });
            }

            try
            {
                var helper = new CommonHelper();
                var request = new UpdateVenueAndHallReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    Venues = payload.Venues
                };

                var response = helper.UpdateVenueAndHall(request);

                if (response != null && (response.Status == 1 || response.Status == 200))
                {
                    return new JsonResult(new
                    {
                        success = true,
                        message = response.Message ?? "Venue and halls updated successfully."
                    });
                }

                return new JsonResult(new { success = false, message = response?.Message ?? "Failed to update venue and halls." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Venue Error: {ex}");
                return new JsonResult(new { success = false, message = "An error occurred while updating venue and hall details." });
            }
        }

        public IActionResult OnGetGetVenues()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid." });
            }

            try
            {
                var helper = new CommonHelper();
                var request = new GenVenueHallReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value
                };

                var response = helper.GetVenueHallsResp(request);

                if (response != null && (response.Status == 1 || response.Status == 200) && response.Venues != null)
                {
                    return new JsonResult(new { success = true, data = response.Venues }, PreserveCasingJsonOptions);
                }

                return new JsonResult(new { success = false, message = response?.Message ?? "No venues found." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get Venues Error: {ex}");
                return new JsonResult(new { success = false, message = "Unable to load saved venues." });
            }
        }

        public IActionResult OnGetGetHallsByVenue(int venueId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid." });
            }

            try
            {
                var helper = new CommonHelper();
                var response = helper.GetVenueHallsBYIdResp(userId.Value, eventId.Value, venueId);

                if (response != null && (response.Status == 1 || response.Status == 200) && response.Hall != null)
                {
                    return new JsonResult(new { success = true, data = response.Hall }, PreserveCasingJsonOptions);
                }

                return new JsonResult(new { success = false, message = response?.Message ?? "No halls found for this venue." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get Halls By Venue Error: {ex}");
                return new JsonResult(new { success = false, message = "Unable to load halls for the selected venue." });
            }
        }

        public IActionResult OnPostSaveSpeakers([FromBody] SaveScheduleRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid. Please select an event first." });
            }

            if (request?.Speakers == null || !request.Speakers.Any())
            {
                return new JsonResult(new { success = false, message = "Please add at least one speaker." });
            }

            try
            {
                var helper = new CommonHelper();
                var speakerRequest = new AddSpeakerReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    Ent_Speaker = request.Speakers
                };

                var speakerResponse = helper.AddSpeaker(speakerRequest);

                if (speakerResponse != null && (speakerResponse.Status == 1 || speakerResponse.Status == 200))
                {
                    return new JsonResult(new
                    {
                        success = true,
                        message = speakerResponse.Message ?? "Speakers saved successfully."
                    });
                }

                return new JsonResult(new { success = false, message = speakerResponse?.Message ?? "Failed to save speakers." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save Speakers Error: {ex}");
                return new JsonResult(new { success = false, message = "An error occurred while saving speaker details." });
            }
        }

        public IActionResult OnPostUpdateSpeakers([FromBody] UpdateSpeakersPayload payload)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid. Please select an event first." });
            }

            if (payload?.Speakers == null || !payload.Speakers.Any())
            {
                return new JsonResult(new { success = false, message = "Please add at least one speaker." });
            }

            try
            {
                var helper = new CommonHelper();
                var speakerRequest = new UpdateSpeakerReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    Ent_UpdateSpeaker = payload.Speakers
                };

                var speakerResponse = helper.UpdateSpeaker(speakerRequest);

                if (speakerResponse != null && (speakerResponse.Status == 1 || speakerResponse.Status == 200))
                {
                    return new JsonResult(new
                    {
                        success = true,
                        message = speakerResponse.Message ?? "Speakers updated successfully."
                    });
                }

                return new JsonResult(new { success = false, message = speakerResponse?.Message ?? "Failed to update speakers." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Speakers Error: {ex}");
                return new JsonResult(new { success = false, message = "An error occurred while updating speaker details." });
            }
        }

        public IActionResult OnGetGetSpeakers()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");

            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid." });
            }

            try
            {
                var helper = new CommonHelper();
                var response = helper.GetSpearkersResp(userId.Value, eventId.Value);

                if (response != null && (response.Status == 1 || response.Status == 200) && response.Speakers != null)
                {
                    return new JsonResult(new { success = true, data = response.Speakers }, PreserveCasingJsonOptions);
                }

                return new JsonResult(new { success = false, message = response?.Message ?? "No speakers found." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get Speakers Error: {ex}");
                return new JsonResult(new { success = false, message = "Unable to load speakers." });
            }
        }

        public IActionResult OnPostSaveSchedule([FromBody] SaveScheduleRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");

            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid." });
            }

            if (request == null)
            {
                return new JsonResult(new { success = false, message = "Invalid schedule request." });
            }

            try
            {
                var helper = new CommonHelper();

                // Save Event Dates and Event Slots
                if (request.Slots != null && request.Slots.Any())
                {
                    var slotRequest = new AddEventDateSlotsReq
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        EventDates = request.Slots
                    };

                    var slotResponse = helper.AddEventDateAndSlots(slotRequest);

                    if (slotResponse == null || (slotResponse.Status != 1 && slotResponse.Status != 200))
                    {
                        return new JsonResult(new
                        {
                            success = false,
                            message = slotResponse?.Message ?? "Failed to save schedule."
                        });
                    }
                }

                if (request.Speakers != null && request.Speakers.Any())
                {
                    var speakerRequest = new AddSpeakerReq
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        Ent_Speaker = request.Speakers
                    };

                    var speakerResponse = helper.AddSpeaker(speakerRequest);

                    if (speakerResponse == null || (speakerResponse.Status != 1 && speakerResponse.Status != 200))
                    {
                        return new JsonResult(new
                        {
                            success = false,
                            message = speakerResponse?.Message ?? "Failed to save speakers."
                        });
                    }
                }
                int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
                HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 4));
                return new JsonResult(new { success = true, message = "Schedule saved successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save Schedule Error: {ex}");
                return new JsonResult(new { success = false, message = "An error occurred while saving schedule." });
            }
        }
        public IActionResult OnPostUpdateSchedule([FromBody] UpdateScheduleRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");

            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid." });
            }

            if (request?.Slots == null || !request.Slots.Any())
            {
                return new JsonResult(new { success = false, message = "Invalid schedule request." });
            }

            try
            {
                var helper = new CommonHelper();

                var slotRequest = new UpdateEventDateSlotsReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    EventDates = request.Slots
                };

                var slotResponse = helper.UpdateEventDateAndSlots(slotRequest);

                if (slotResponse == null || (slotResponse.Status != 1 && slotResponse.Status != 200))
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = slotResponse?.Message ?? "Failed to update schedule."
                    });
                }

                int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
                HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 4));
                return new JsonResult(new { success = true, message = "Schedule updated successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Schedule Error: {ex}");
                return new JsonResult(new { success = false, message = "An error occurred while updating schedule." });
            }
        }

        public IActionResult OnGetGetSchedule()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { success = false, message = "Unauthorized" });
            }

            if (!eventId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Event session expired or invalid." });
            }

            try
            {
                var helper = new CommonHelper();
                // Call your helper method to fetch saved event dates and slots
                var response = helper.GetEventDateAndSlots(userId.Value, eventId.Value);

                if (response != null && (response.Status == 1 || response.Status == 200) && response.EventDates != null)
                {
                    return new JsonResult(new { success = true, data = response.EventDates }, PreserveCasingJsonOptions);
                }

                return new JsonResult(new { success = false, message = response?.Message ?? "No schedule found." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get Schedule Error: {ex}");
                return new JsonResult(new { success = false, message = "Unable to load schedule." });
            }
        }
    }

    public class SaveVenuePayload
    {
        public List<Venue> Venues { get; set; } = new();
    }

    public class UpdateVenuePayload
    {
        public List<UpdateVenue> Venues { get; set; } = new();
    }

    public class UpdateSpeakersPayload
    {
        public List<UpdateSpeakerModel> Speakers { get; set; } = new();
    }

    public class UpdateScheduleRequest
    {
        public List<UpdateEventDateModel> Slots { get; set; } = new();
    }
}