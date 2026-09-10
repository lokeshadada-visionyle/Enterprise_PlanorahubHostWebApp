using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using System.Diagnostics;

namespace Planora_EnterproseHostWebApp.Pages.Events
{
    public class CheckInModel : PageModel
    {
        public EventHubDatesAndSlotsResp DatesAndSlots { get; set; } = new();
        public GuestCheckInsDashboardResp Dashboard { get; set; } = new();
        public string ApiError { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public int EventId { get; set; }
        public int SlotId { get; set; }
        public int EventDateId { get; set; }
        public int TicketTypeId { get; set; }
        public ManualCheckInGuestsResp GuestList { get; set; } = new();
        public EventTicketTypeResp TicketTypes { get; set; } = new();
        public SeatingLayoutResp SeatingLayout { get; set; } = new();
        public EventScanLogsResp ScanLogs { get; set; } = new();

        public IActionResult OnGet(int eventId, int slotId = 0, int eventDateId = 0, int ticketTypeId = 0)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            EventId = eventId;
            TicketTypeId = ticketTypeId;

            LoadDatesAndSlots(userId.Value, eventId);

            if (slotId != 0 && eventDateId != 0)
            {
                SlotId = slotId;
                EventDateId = eventDateId;
            }
            else
            {
                var firstDay = DatesAndSlots.EventDates?.FirstOrDefault();
                EventDateId = firstDay?.EventDateId ?? 0;
                SlotId = firstDay?.Slots?.FirstOrDefault()?.SlotId ?? 0;
            }

                        if (SlotId != 0)
            {
                LoadDashboard(userId.Value, eventId, SlotId);
                LoadTicketTypes(userId.Value, eventId, SlotId);
                LoadGuestList(userId.Value, eventId, SlotId, EventDateId, TicketTypeId);
                LoadSeatingLayout(userId.Value, eventId, SlotId, EventDateId, TicketTypeId);
                LoadScanLogs(userId.Value, eventId, SlotId);
            }

            return Page();

        }

        private void LoadDatesAndSlots(int userId, int eventId)
        {
            var helper = new CommonHelper();

            try
            {
                var response = helper.GetEventHubDateAndSlot(userId, eventId);

                if (response != null && response.Status == 1)
                {
                    DatesAndSlots = response;
                }
                else
                {
                    ApiError = response?.Message ?? "Failed to load event dates and slots.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load event dates and slots: " + ex.Message;
            }
        }

        private void LoadDashboard(int userId, int eventId, int slotId)
        {
            var helper = new CommonHelper();

            try
            {
                var response = helper.GetGuestCheckInsDashboard(userId, eventId, slotId);

                if (response != null && response.Status == 1)
                {
                    Dashboard = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load check-in dashboard.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load check-in dashboard: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }

        public IActionResult OnPostManualCheckIn(int eventId, int slotId, string reference)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            var helper = new CommonHelper();

            try
            {
                var response = helper.ManualCheckIn(userId.Value, eventId, slotId, reference);

                if (response != null && response.Status == 1)
                {
                    SuccessMessage = $"{reference} checked in successfully.";
                }
                else
                {
                    ApiError = response?.Message ?? "Check-in failed.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "Check-in failed: " + ex.Message;
            }

            // Reload the page's normal data so the dashboard reflects the new check-in.
            EventId = eventId;
            LoadDatesAndSlots(userId.Value, eventId);
            var firstDay = DatesAndSlots.EventDates?.FirstOrDefault();
            SlotId = slotId != 0 ? slotId : firstDay?.Slots?.FirstOrDefault()?.SlotId ?? 0;
            EventDateId = firstDay?.EventDateId ?? 0;
            if (SlotId != 0)
            {
                LoadDashboard(userId.Value, eventId, SlotId);
                LoadGuestList(userId.Value, eventId, SlotId, EventDateId, TicketTypeId);
                LoadSeatingLayout(userId.Value, eventId, SlotId, EventDateId, TicketTypeId);
            }

            return Page();
        }

        public IActionResult OnPostCheckInGuest(int eventId, int slotId, int eventDateId, int ticketTypeId, int guestId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            var helper = new CommonHelper();

            try
            {
                var response = helper.CheckInGuest(userId.Value, eventId, slotId, guestId);

                if (response != null && response.Status == 1)
                {
                    SuccessMessage = "Guest checked in successfully.";
                }
                else
                {
                    ApiError = response?.Message ?? "Check-in failed.";
                }
            }
            catch (Exception ex)
            {
                ApiError = "Check-in failed: " + ex.Message;
            }

            // Reload the page's normal data so the dashboard/guest list reflects the change.
            EventId = eventId;
            SlotId = slotId;
            EventDateId = eventDateId;
            TicketTypeId = ticketTypeId;
            LoadDatesAndSlots(userId.Value, eventId);
            LoadDashboard(userId.Value, eventId, SlotId);
            LoadTicketTypes(userId.Value, eventId, SlotId);
            LoadGuestList(userId.Value, eventId, SlotId, EventDateId, TicketTypeId);
            LoadSeatingLayout(userId.Value, eventId, SlotId, EventDateId, TicketTypeId);

            return Page();
        }

        private void LoadGuestList(int userId, int eventId, int slotId, int eventDateId, int ticketTypeId)
        {
            var helper = new CommonHelper();

            try
            {
                var response = helper.GetManualCheckInGuests(userId, eventId, slotId, eventDateId, ticketTypeId);

                if (response != null && response.Status == 1)
                {
                    GuestList = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load guest list.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load guest list: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }

        private void LoadTicketTypes(int userId, int eventId, int slotId)
        {
            var helper = new CommonHelper();

            try
            {
                var response = helper.GetEventHubGuestTicketTypeResp(userId, eventId, slotId);
                Debug.WriteLine($"TicketTypes response: Status={response?.Status}, Message={response?.Message}, Count={response?.TicketTypes?.Count ?? -1}");

                if (response != null && response.Status == 1)
                {
                    TicketTypes = response;
                }
                else
                {
                    Debug.WriteLine("Ticket types call did not return Status=1.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load ticket types: " + ex.Message);
            }
        }

        private void LoadSeatingLayout(int userId, int eventId, int slotId, int eventDateId, int ticketTypeId)
        {
            var helper = new CommonHelper();

            try
            {
                var response = helper.GetSeatingLayout(userId, eventId, slotId, eventDateId, ticketTypeId);

                if (response != null && response.Status == 1)
                {
                    SeatingLayout = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load seating layout.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load seating layout: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }

        private void LoadScanLogs(int userId, int eventId, int slotId)
        {
            var helper = new CommonHelper();

            try
            {
                var response = helper.GetEventScanLogs(userId, eventId, slotId);

                if (response != null && response.Status == 1)
                {
                    ScanLogs = response;
                }
                else
                {
                    var error = response?.Message ?? "Failed to load scan log.";
                    ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to load scan log: " + ex.Message;
                ApiError = string.IsNullOrWhiteSpace(ApiError) ? error : $"{ApiError} {error}";
            }
        }
    }
}