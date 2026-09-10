using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubStaffModel : PageModel
    {
        public EventHubVendorResponse eventHubVendorData { get; set; } = new EventHubVendorResponse();

        public IActionResult OnGet(int eventId, int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToPage("/Login/Login");

            ViewData["EventId"] = eventId;
            ViewData["SlotId"] = slotId;

            var helper = new CommonHelper();
            var errors = new List<string>();
            try
            {
                var resp = helper.GetVendorsAndStaffListResp(userId.Value, eventId,slotId);
                if (resp != null && resp.Status == 1)
                    eventHubVendorData = resp;
                else
                    errors.Add(resp?.Message ?? "Failed to load vendor data.");
            }
            catch (Exception ex)
            {
                errors.Add("Failed to load vendor data: " + ex.Message);
            }
            return Page();
        }

        public IActionResult OnGetGetVendors(int eventId,int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { status = 0, message = "Session expired." });

            var helper = new CommonHelper();
            var response = helper.GetVendorsAndStaffListResp(userId.Value, eventId,slotId);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new
                {
                    status = 1,
                    vendors = response.Vendors,
                });
            }

            return new JsonResult(new { status = 0, message = response?.Message ?? "Failed to fetch vendors." });
        }

        public IActionResult OnGetGetEventDatesAndSlots(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { status = 0, message = "Session expired." });

            var helper = new CommonHelper();
            var response = helper.GetEventHubDateAndSlot(userId.Value, eventId);

            if (response != null)
            {
                return new JsonResult(new { status = 1, data = response });
            }

            return new JsonResult(new { status = 0, message = "Failed to fetch dates and slots." });
        }
        public IActionResult OnPostAddVendor([FromBody] AddVendorRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { success = false, message = "Session expired." });

            request.UserId = userId.Value;

            if (!request.VendorId.HasValue)
            {
                request.VendorId = 0;
            }

            var helper = new CommonHelper();
            var response = helper.AddVendorSatff(request);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new { success = true, message = response.Message ?? "Vendor/Staff added successfully." });
            }

            return new JsonResult(new { success = false, message = response?.Message ?? "Failed to add vendor/staff." });
        }
        public IActionResult OnGetGetEvents(int eventId = 0)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { status = 0, message = "Session expired." });

            var helper = new CommonHelper();
            var response = helper.GetEventListResp(userId.Value, eventId);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new
                {
                    status = 1,
                    data = new
                    {
                        eventId = response.EventId,
                        eventName = response.EventName,
                        venue = response.Venue,
                        date = response.Date
                    }
                });
            }

            return new JsonResult(new { status = 0, message = response?.Message ?? "Failed to fetch events." });
        }
        public IActionResult OnPostEditVendor([FromBody] EditVendorRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { success = false, message = "Session expired." });

            request.UserId = userId.Value;

            var helper = new CommonHelper();
            var response = helper.UpdateVendor(request);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new { success = true, message = response.Message ?? "Vendor updated successfully." });
            }

            return new JsonResult(new { success = false, message = response?.Message ?? "Failed to update vendor." });
        }
        public IActionResult OnPostRegenerateVendorPin(int eventId, int vendorId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { success = false, message = "Session expired." });

            var helper = new CommonHelper();
            var response = helper.RegenerateVendorPin(userId.Value, eventId, vendorId);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new { success = true, message = response.Message });
            }

            return new JsonResult(new { success = false, message = response?.Message ?? "Failed to regenerate PIN." });
        }

        public IActionResult OnPostRemoveVendor(int eventId, int vendorId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { success = false, message = "Session expired." });

            var helper = new CommonHelper();
            var response = helper.RemoveVendor(userId.Value, eventId, vendorId);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new { success = true, message = response.Message });
            }

            return new JsonResult(new { success = false, message = response?.Message ?? "Failed to remove vendor." });
        }
    }
}