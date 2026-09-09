using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;
using Planora_EnterproseHostWebApp.Models;
using System.Globalization;
using System.Security.Claims;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class TicketPricingModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty(SupportsGet = true)]
        public long UserId { get; set; }

        [BindProperty]
        public string PricingMode { get; set; } = "paid";

        public string Currency { get; private set; } = "NGN";

        public List<EventDates> EventDates { get; private set; } = new List<EventDates>();

        public string? ErrorMessage { get; private set; }

        public string? SuccessMessage { get; private set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            ResolveIds();

            var helper = new CommonHelper();
            try
            {
                var response = helper.GetEventCurrency(userId.Value, eventId.Value);

                if (response != null &&
                    !string.IsNullOrWhiteSpace(response.Currency))
                {
                    Currency = response.Currency.Trim();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Unable to load event currency: " + ex.Message;
            }

            try
            {
                var slots = helper.GetEventDateAndSlots(userId.Value, eventId.Value);
                if (slots != null && slots.Status == 1 && slots.EventDates != null)
                {
                    EventDates = slots.EventDates;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Unable to load event dates/slots: " + ex.Message;
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            ResolveIds();

            if (eventId <= 0)
            {
                ErrorMessage = "A valid EventId is required.";
                return Page();
            }

            if (userId <= 0)
            {
                ErrorMessage = "A valid UserId is required.";
                return Page();
            }

            PricingMode = string.Equals(
                PricingMode,
                "free",
                StringComparison.OrdinalIgnoreCase)
                ? "free"
                : "paid";


            List<AddTicketTypeReq> ticketTypes;

            if (PricingMode == "free")
            {
                ticketTypes = BuildFreeTicketType();
            }
            else
            {
                ticketTypes = BuildPaidTicketTypes();
            }


            if (ticketTypes.Count == 0)
            {
                ErrorMessage = "Please add at least one ticket tier.";
                return Page();
            }


            var request = new CreateTicketTypeRequest
            {
                UserId = userId.Value,
                EventId = eventId.Value,
                TicketTypes = ticketTypes
            };

            var helper = new CommonHelper();
            try
            {
                var response = helper.AddTicketTypeTierReq(request);

                if (response == null)
                {
                    ErrorMessage = "The ticket service returned an empty response.";
                    return Page();
                }

                if (response.Status != 0 &&
                    response.Status != 1 &&
                    response.Status != 200)
                {
                    ErrorMessage = string.IsNullOrWhiteSpace(response.Message)
                        ? "Unable to save ticket types."
                        : response.Message;

                    return Page();
                }


                return RedirectToPage(
                    HttpContext.Session.IsPrivateEvent()
                        ? "/CreateEvent/AddonsCoupons"
                        : "/CreateEvent/PayoutAccount",
                    new
                    {
                        eventId = eventId,
                        userId = userId
                    });
            }
            catch (Exception ex)
            {
                ErrorMessage = "Unable to save ticket types: " + ex.Message;
                return Page();
            }
        }

        public IActionResult OnPostUploadMembers([FromBody] UploadMembersRequestDto req)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            ResolveIds();

            if (userId <= 0)
                return new JsonResult(new { status = 0, message = "Invalid user session." });

            if (req == null ||
                string.IsNullOrWhiteSpace(req.FileBase64) ||
                string.IsNullOrWhiteSpace(req.FileName))
            {
                return new JsonResult(new { status = 0, message = "No file provided." });
            }

            if (eventId <= 0)
                return new JsonResult(new { status = 0, message = "Invalid EventId." });

            try
            {
                var helper = new CommonHelper();

                var response = helper.UploadMembersList(
                    userId.Value,
                    eventId.Value,
                    req.FileName,
                    req.FileBase64);

                if (response == null)
                {
                    return new JsonResult(new { status = 0, message = "Empty response from upload service." });
                }

                return new JsonResult(new
                {
                    status = response.Status,
                    message = response.Message,
                    fileUrl = response.FileURL
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = 0, message = "Upload failed: " + ex.Message });
            }
        }

        public class UploadMembersRequestDto
        {
            public int EventId { get; set; }
            public string TierSuffix { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string FileBase64 { get; set; } = string.Empty;
        }
        private void ResolveIds()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            if (eventId <= 0)
                EventId = ReadInt("EventId", "eventId");

            if (userId <= 0)
                UserId = ReadLong("UserId", "userId");


            if (eventId <= 0)
                EventId = ReadSessionInt("EventId", "eventId");

            if (userId <= 0)
                UserId = ReadSessionLong("UserId", "userId");


            if (userId <= 0)
            {
                var claim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("UserId")
                    ?? User.FindFirstValue("userId");

                if (long.TryParse(
                    claim,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var claimUserId))
                {
                    UserId = claimUserId;
                }
            }
        }


        private int ReadInt(params string[] names)
        {
            foreach (var name in names)
            {
                if (int.TryParse(
                    Request.Query[name].FirstOrDefault(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var queryValue))
                {
                    return queryValue;
                }

                if (Request.HasFormContentType &&
                    int.TryParse(
                        Request.Form[name].FirstOrDefault(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var formValue))
                {
                    return formValue;
                }
            }

            return 0;
        }


        private long ReadLong(params string[] names)
        {
            foreach (var name in names)
            {
                if (long.TryParse(
                    Request.Query[name].FirstOrDefault(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var queryValue))
                {
                    return queryValue;
                }

                if (Request.HasFormContentType &&
                    long.TryParse(
                        Request.Form[name].FirstOrDefault(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var formValue))
                {
                    return formValue;
                }
            }

            return 0;
        }


        private int ReadSessionInt(params string[] names)
        {
            foreach (var name in names)
            {
                var value = HttpContext.Session.GetString(name);

                if (int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var result))
                {
                    return result;
                }
            }

            return 0;
        }


        private long ReadSessionLong(params string[] names)
        {
            foreach (var name in names)
            {
                var value = HttpContext.Session.GetString(name);

                if (long.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var result))
                {
                    return result;
                }
            }

            return 0;
        }


        private List<AddTicketTypeReq> BuildPaidTicketTypes()
        {
            var result = new List<AddTicketTypeReq>();

            if (!Request.HasFormContentType)
                return result;

            var form = Request.Form;

            var suffixes = form.Keys
                .Where(k => k.StartsWith("tiername-", StringComparison.OrdinalIgnoreCase))
                .Select(k => k["tiername-".Length..])
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var suffix in suffixes)
            {
                var typeName = GetForm(form, $"tiername-{suffix}");

                if (string.IsNullOrWhiteSpace(typeName))
                    continue;

                bool autoExpire = GetBool(form, $"expire-{suffix}");
                string expiryMode = GetForm(form, $"tier-expiry-mode-{suffix}", "date");

                // Parse Access Mode & Selected Dates/Slots
                string accessMode = GetForm(form, $"acc-mode-{suffix}", "all");
                var selectedDateIds = GetFormIntList(form, $"acc-{suffix}-day-");
                var selectedSlotIds = GetFormIntList(form, $"acc-{suffix}-slot-");

                // Parse Inclusions for this tier
                var inclusions = BuildInclusionsForTier(form, suffix);

                result.Add(new AddTicketTypeReq
                {
                    IsVisible = GetBool(form, $"tier-visible-{suffix}", true),
                    TypeName = typeName.Trim(),
                    Price = GetForm(form, $"price-{suffix}", "0"),
                    Quantity = GetInt(form, $"qty-{suffix}"),
                    SaleStart = GetForm(form, $"salestart-{suffix}"),
                    SaleEnd = GetForm(form, $"saleend-{suffix}"),
                    Description = GetForm(form, $"desc-{suffix}"),
                    AutoExpireEnabled = autoExpire,
                    AutoExpireMode = autoExpire ? expiryMode : string.Empty,
                    ExpiryDate = autoExpire && expiryMode.Equals("date", StringComparison.OrdinalIgnoreCase)
                        ? GetForm(form, $"expdate-{suffix}")
                        : string.Empty,
                    ExpiryTime = autoExpire && expiryMode.Equals("date", StringComparison.OrdinalIgnoreCase)
                        ? GetForm(form, $"exptime-{suffix}")
                        : string.Empty,
                    AllocationLimit = autoExpire && expiryMode.Equals("alloc", StringComparison.OrdinalIgnoreCase)
                        ? GetInt(form, $"expalloc-{suffix}")
                        : 0,
                    MemberOnly = GetBool(form, $"membersonly-{suffix}"),
                    MemberListUrl = GetForm(form, $"tier-memberfileurl-{suffix}"),
                    StandingCapacity = GetInt(form, $"standcap-{suffix}"),
                    OpenSeatingCapacity = GetInt(form, $"opencap-{suffix}"),
                    NumberOfTables = GetInt(form, $"tables-{suffix}"),
                    ChairsPerTable = GetInt(form, $"chairs-{suffix}"),
                    IsSeating = GetBool(form, $"tier-seating-{suffix}"),
                    Rows = GetInt(form, $"rows-{suffix}"),
                    Columns = GetInt(form, $"seats-{suffix}"),

                    // New Fields
                    AccessMode = accessMode,
                    SelectedDateIds = selectedDateIds,
                    SelectedSlotIds = selectedSlotIds,
                    Inclusions = inclusions
                });
            }

            return result;
        }

        // =========================
        // FREE TICKET
        // =========================
        private List<AddTicketTypeReq> BuildFreeTicketType()
        {
            if (!Request.HasFormContentType)
                return new List<AddTicketTypeReq>();

            var form = Request.Form;
            var quantity = GetInt(form, "free-qty");
            var capacity = GetInt(form, "free-cap");

            // Parse Access Mode & Selected Dates/Slots for Free tier
            string accessMode = GetForm(form, "acc-mode-free", "all");
            var selectedDateIds = GetFormIntList(form, "acc-free-day-");
            var selectedSlotIds = GetFormIntList(form, "acc-free-slot-");

            // Parse Inclusions for Free tier
            var inclusions = BuildInclusionsForTier(form, "free");

            return new List<AddTicketTypeReq>
            {
                new AddTicketTypeReq
                {
                    IsVisible = true,
                    TypeName = "Free",
                    Price = "0",
                    Quantity = quantity,
                    SaleStart = string.Empty,
                    SaleEnd = string.Empty,
                    Description = GetForm(form, "free-desc", "General admission"),
                    AutoExpireEnabled = false,
                    AutoExpireMode = string.Empty,
                    ExpiryDate = string.Empty,
                    ExpiryTime = string.Empty,
                    AllocationLimit = 0,
                    MemberOnly = false,
                    StandingCapacity = GetInt(form, "free-standing-cap", capacity),
                    OpenSeatingCapacity = GetInt(form, "free-open-cap", capacity),
                    NumberOfTables = GetInt(form, "free-tables"),
                    ChairsPerTable = GetInt(form, "free-chairs"),
                    IsSeating = true,
                    Rows = GetInt(form, "free-rows"),
                    Columns = GetInt(form, "free-seats-row"),

                    // New Fields
                    AccessMode = accessMode,
                    SelectedDateIds = selectedDateIds,
                    SelectedSlotIds = selectedSlotIds,
                    Inclusions = inclusions
                }
            };
        }

        // =========================
        // HELPER METHODS FOR PARSING
        // =========================

        private List<Inclusions> BuildInclusionsForTier(IFormCollection form, string suffix)
        {
            var inclusions = new List<Inclusions>();

            // Find all checked inclusions for this tier
            // Expecting inputs named like: incl-name-{suffix}-{index} and incl-desc-{suffix}-{index}
            var keys = form.Keys
                .Where(k => k.StartsWith($"incl-name-{suffix}-", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int sortOrder = 1;
            foreach (var key in keys)
            {
                var index = key[$"incl-name-{suffix}-".Length..];
                var name = GetForm(form, key);
                var desc = GetForm(form, $"incl-desc-{suffix}-{index}");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    inclusions.Add(new Inclusions
                    {
                        InclusionName = name.Trim(),
                        Description = desc.Trim(),
                        SortOrder = sortOrder++
                    });
                }
            }

            return inclusions;
        }

        private static List<int> GetFormIntList(IFormCollection form, string keyPrefix)
        {
            var results = new List<int>();

            foreach (var key in form.Keys.Where(k => k.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase)))
            {
                var value = form[key].FirstOrDefault();

                // Handles key form structure like: acc-early-day-101 where key ending is ID or input value holds the ID
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                {
                    results.Add(id);
                }
                else
                {
                    var keySuffix = key[keyPrefix.Length..];
                    if (int.TryParse(keySuffix, NumberStyles.Integer, CultureInfo.InvariantCulture, out var keyId))
                    {
                        results.Add(keyId);
                    }
                }
            }

            return results.Distinct().ToList();
        }

        private static string GetForm(IFormCollection form, string key, string defaultValue = "")
        {
            return form.TryGetValue(key, out var value) ? value.FirstOrDefault() ?? defaultValue : defaultValue;
        }

        private static int GetInt(IFormCollection form, string key, int defaultValue = 0)
        {
            return int.TryParse(GetForm(form, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : defaultValue;
        }

        private static bool GetBool(IFormCollection form, string key, bool defaultValue = false)
        {
            var value = GetForm(form, key);
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            if (bool.TryParse(value, out var boolValue)) return boolValue;
            return value == "1" || value.Equals("on", StringComparison.OrdinalIgnoreCase) || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}