using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;
using Planora_EnterproseHostWebApp.Models;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class TicketPricingModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty(SupportsGet = true)]
        public long UserId { get; set; }

        [BindProperty]
        public string EventType { get; set; } = string.Empty;

        [BindProperty]
        public string PricingMode { get; set; } = string.Empty;

        public string Currency { get; private set; } = "NGN";

        public bool IsPrivateEvent { get; private set; }

        public List<EventDates> EventDates { get; private set; } = new List<EventDates>();

        [BindProperty]
        public bool HasExistingTicketData { get; set; }

        public string ExistingTicketDataJson { get; private set; } = "[]";

        public string? ErrorMessage { get; private set; }

        public string? SuccessMessage { get; private set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            ViewData["StepIndex"] = 5;

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            ResolveIds();

            EventType = ResolveEventType();
            IsPrivateEvent = string.Equals(EventType, "private", StringComparison.OrdinalIgnoreCase);

            var helper = new CommonHelper();

            try
            {
                var response = helper.GetEventCurrency(userId.Value, eventId.Value);

                if (response != null && !string.IsNullOrWhiteSpace(response.Currency))
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

            try
            {
                var tierResp = helper.GetTicketTypeTierResp(userId.Value, eventId.Value);

                if (tierResp != null && tierResp.Status == 1 && tierResp.TicketTypes?.Count > 0)
                {
                    HasExistingTicketData = true;

                    PricingMode = (tierResp.TicketTypes.Count == 1 &&
                                   string.Equals(tierResp.TicketTypes[0].TypeName, "Free", StringComparison.OrdinalIgnoreCase))
                        ? "free"
                        : "paid";

                    ExistingTicketDataJson = JsonSerializer.Serialize(tierResp.TicketTypes, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Unable to load existing ticket tiers: " + ex.Message;
            }

            if (IsPrivateEvent && string.Equals(PricingMode, "free", StringComparison.OrdinalIgnoreCase))
            {
                PricingMode = "paid";
            }

            return Page();
        }

        public IActionResult OnPostSetCurrency([FromBody] SetCurrencyRequestDto req)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) ||
                !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { status = 0, message = "Not authenticated." });
            }

            if (!eventId.HasValue || eventId.Value <= 0)
            {
                return new JsonResult(new { status = 0, message = "No event in progress." });
            }

            if (req == null || string.IsNullOrWhiteSpace(req.Currency))
            {
                return new JsonResult(new { status = 0, message = "Currency is required." });
            }

            var helper = new CommonHelper();
            try
            {
                var response = helper.AddEventCurrencyReq(new SetEventCurrencyReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    Currency = req.Currency.Trim()
                });

                if (response != null && (response.Status == 0 || response.Status == 1 || response.Status == 200))
                {
                    return new JsonResult(new { status = 1, message = "Currency updated.", currency = req.Currency.Trim() });
                }

                return new JsonResult(new { status = 0, message = response?.Message ?? "Unable to update currency." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = 0, message = "Unable to update currency: " + ex.Message });
            }
        }

        public class SetCurrencyRequestDto
        {
            public string Currency { get; set; } = string.Empty;
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            ResolveIds();

            EventType = ResolveEventType();
            IsPrivateEvent = string.Equals(EventType, "private", StringComparison.OrdinalIgnoreCase);

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

            Debug.WriteLine(PricingMode);

            if (IsPrivateEvent)
            {
                PricingMode = "paid";
            }
            else
            {
                PricingMode = string.Equals(PricingMode, "free", StringComparison.OrdinalIgnoreCase)
                    ? "free"
                    : "paid";
            }

            List<ParsedTier> parsedTiers = PricingMode == "free"
                ? BuildFreeTier(eventId.Value)
                : BuildPaidTiers(eventId.Value);

            if (parsedTiers.Count == 0)
            {
                ErrorMessage = "Please add at least one ticket tier.";
                return Page();
            }

            var newTiers = parsedTiers.Where(t => t.TicketTypeId <= 0).ToList();
            var existingTiers = parsedTiers.Where(t => t.TicketTypeId > 0).ToList();

            var helper = new CommonHelper();
            try
            {
                if (newTiers.Count > 0)
                {
                    var addRequest = new CreateTicketTypeRequest
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        TicketTypes = newTiers.Select(MapToAddRequest).ToList()
                    };

                    var addResponse = helper.AddTicketTypeTierReq(addRequest);

                    if (addResponse == null)
                    {
                        ErrorMessage = "The ticket service returned an empty response while adding new tiers.";
                        return Page();
                    }

                    if (addResponse.Status != 0 && addResponse.Status != 1 && addResponse.Status != 200)
                    {
                        ErrorMessage = string.IsNullOrWhiteSpace(addResponse.Message)
                            ? "Unable to save new ticket tiers."
                            : addResponse.Message;
                        return Page();
                    }
                }

                if (existingTiers.Count > 0)
                {
                    var updateRequest = new UpdateTicketTypeRequest
                    {
                        UserId = (int)userId.Value,
                        EventId = eventId.Value,
                        TicketTypes = existingTiers.Select(MapToUpdateRequest).ToList()
                    };

                    var updateResponse = helper.UpdateTicketTypeTierReq(updateRequest);

                    if (updateResponse == null)
                    {
                        ErrorMessage = "The ticket service returned an empty response while updating existing tiers.";
                        return Page();
                    }

                    if (updateResponse.Status != 0 && updateResponse.Status != 1 && updateResponse.Status != 200)
                    {
                        ErrorMessage = string.IsNullOrWhiteSpace(updateResponse.Message)
                            ? "Unable to update existing ticket tiers."
                            : updateResponse.Message;
                        return Page();
                    }
                }

                int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
                HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 6));

                return RedirectToPage(
                    IsPrivateEvent
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

        private class ParsedTier
        {
            public int TicketTypeId { get; set; }
            public bool IsVisible { get; set; }
            public string TypeName { get; set; } = string.Empty;
            public string Price { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string SaleStart { get; set; } = string.Empty;
            public string SaleEnd { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool AutoExpireEnabled { get; set; }
            public string AutoExpireMode { get; set; } = string.Empty;
            public string MemberListUrl { get; set; } = string.Empty;
            public string ExpiryDate { get; set; } = string.Empty;
            public string ExpiryTime { get; set; } = string.Empty;
            public int AllocationLimit { get; set; }
            public bool MemberOnly { get; set; }
            public int StandingCapacity { get; set; }
            public int OpenSeatingCapacity { get; set; }
            public int NumberOfTables { get; set; }
            public int ChairsPerTable { get; set; }
            public bool IsSeating { get; set; }
            public int Rows { get; set; }
            public int Columns { get; set; }
            public string AccessMode { get; set; } = string.Empty;
            public List<int> SelectedDateIds { get; set; } = new List<int>();
            public List<int> SelectedSlotIds { get; set; } = new List<int>();
            public List<Inclusions> Inclusions { get; set; } = new List<Inclusions>();
        }

        private static AddTicketTypeReq MapToAddRequest(ParsedTier t) => new AddTicketTypeReq
        {
            IsVisible = t.IsVisible,
            TypeName = t.TypeName,
            Price = t.Price,
            Quantity = t.Quantity,
            SaleStart = t.SaleStart,
            SaleEnd = t.SaleEnd,
            Description = t.Description,
            AutoExpireEnabled = t.AutoExpireEnabled,
            AutoExpireMode = t.AutoExpireMode,
            MemberListUrl = t.MemberListUrl,
            ExpiryDate = t.ExpiryDate,
            ExpiryTime = t.ExpiryTime,
            AllocationLimit = t.AllocationLimit,
            MemberOnly = t.MemberOnly,
            StandingCapacity = t.StandingCapacity,
            OpenSeatingCapacity = t.OpenSeatingCapacity,
            NumberOfTables = t.NumberOfTables,
            ChairsPerTable = t.ChairsPerTable,
            IsSeating = t.IsSeating,
            Rows = t.Rows,
            Columns = t.Columns,
            AccessMode = t.AccessMode,
            SelectedDateIds = t.SelectedDateIds,
            SelectedSlotIds = t.SelectedSlotIds,
            Inclusions = t.Inclusions
        };

        private static UpdateTicketTypes MapToUpdateRequest(ParsedTier t) => new UpdateTicketTypes
        {
            TicketTypeId = t.TicketTypeId,
            IsVisible = t.IsVisible,
            TypeName = t.TypeName,
            Price = t.Price,
            Quantity = t.Quantity,
            SaleStart = t.SaleStart,
            SaleEnd = t.SaleEnd,
            Description = t.Description,
            AutoExpireEnabled = t.AutoExpireEnabled,
            AutoExpireMode = t.AutoExpireMode,
            ExpiryDate = t.ExpiryDate,
            ExpiryTime = t.ExpiryTime,
            AllocationLimit = t.AllocationLimit,
            MemberOnly = t.MemberOnly,
            StandingCapacity = t.StandingCapacity,
            OpenSeatingCapacity = t.OpenSeatingCapacity,
            NumberOfTables = t.NumberOfTables,
            ChairsPerTable = t.ChairsPerTable,
            IsSeating = t.IsSeating,
            Rows = t.Rows,
            Coulmns = t.Columns,
            AccessMode = t.AccessMode,
            SelectedDateIds = t.SelectedDateIds,
            SelectedSlotIds = t.SelectedSlotIds,
            Inclusions = t.Inclusions.Select(i => new UpdateInclusions
            {
                InclusionId = 0,
                InclusionName = i.InclusionName,
                Description = i.Description,
                SortOrder = i.SortOrder
            }).ToList()
        };

        public IActionResult OnPostUploadMembers([FromBody] UploadMembersRequestDto req)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            ResolveIds();

            if (userId <= 0)
                return new JsonResult(new { status = 0, message = "Invalid user session." });

            if (req == null ||
                string.IsNullOrWhiteSpace(req.FileBase64) ||
                string.IsNullOrWhiteSpace(req.FileName) ||
                string.IsNullOrWhiteSpace(req.TierSuffix))
            {
                return new JsonResult(new { status = 0, message = "No file provided." });
            }

            var allowedExtensions = new[] { ".csv", ".xlsx" };
            var ext = System.IO.Path.GetExtension(req.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return new JsonResult(new { status = 0, message = "Only .csv or .xlsx files are supported." });
            }

            const int maxBase64Length = 14_000_000;
            if (req.FileBase64.Length > maxBase64Length)
            {
                return new JsonResult(new { status = 0, message = "File is too large. Please upload a file under 10MB." });
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

                if (response.Status == 1 && !string.IsNullOrWhiteSpace(response.FileURL))
                {
                    RememberUploadedMemberListUrl(eventId.Value, req.TierSuffix, response.FileURL);
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

        private const string MemberListUploadsSessionKeyPrefix = "MemberListUploads_";

        private Dictionary<string, string> GetMemberListUploadMap(int eventId)
        {
            var raw = HttpContext.Session.GetString(MemberListUploadsSessionKeyPrefix + eventId);
            if (string.IsNullOrWhiteSpace(raw))
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(raw)
                       ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private void RememberUploadedMemberListUrl(int eventId, string tierSuffix, string fileUrl)
        {
            var map = GetMemberListUploadMap(eventId);
            map[tierSuffix] = fileUrl;
            HttpContext.Session.SetString(MemberListUploadsSessionKeyPrefix + eventId, JsonSerializer.Serialize(map));
        }

        private string GetVerifiedMemberListUrl(int eventId, string tierSuffix, string submittedUrl)
        {
            if (string.IsNullOrWhiteSpace(submittedUrl))
                return string.Empty;

            var map = GetMemberListUploadMap(eventId);
            return map.TryGetValue(tierSuffix, out var storedUrl) &&
                   string.Equals(storedUrl, submittedUrl, StringComparison.Ordinal)
                ? storedUrl
                : string.Empty;
        }

        private string ResolveEventType()
        {
            var eventType = string.Empty;

            if (Request.HasFormContentType)
            {
                eventType = Request.Form["EventType"].FirstOrDefault() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(eventType))
            {
                eventType = Request.Query["type"].FirstOrDefault() ?? string.Empty;
            }

            if (string.Equals(eventType, "private", StringComparison.OrdinalIgnoreCase))
            {
                return "private";
            }

            if (string.Equals(eventType, "public", StringComparison.OrdinalIgnoreCase))
            {
                return "public";
            }

            return HttpContext.Session.IsPrivateEvent() ? "private" : "public";
        }

        private void ResolveIds()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                RedirectToPage("/Login/Login");
            }

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
                var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("UserId")
                    ?? User.FindFirstValue("userId");

                if (long.TryParse(claim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var claimUserId))
                {
                    UserId = claimUserId;
                }
            }
        }

        private int ReadInt(params string[] names)
        {
            foreach (var name in names)
            {
                if (int.TryParse(Request.Query[name].FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var queryValue))
                {
                    return queryValue;
                }

                if (Request.HasFormContentType && int.TryParse(Request.Form[name].FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var formValue))
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
                if (long.TryParse(Request.Query[name].FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var queryValue))
                {
                    return queryValue;
                }

                if (Request.HasFormContentType && long.TryParse(Request.Form[name].FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var formValue))
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
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
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
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }
            }
            return 0;
        }

        private List<ParsedTier> BuildPaidTiers(int eventId)
        {
            var result = new List<ParsedTier>();

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

                string accessMode = GetForm(form, $"acc-mode-{suffix}", "all");
                var selectedDateIds = GetFormIntList(form, $"acc-{suffix}-day-");
                var selectedSlotIds = GetFormIntList(form, $"acc-{suffix}-slot-");

                var inclusions = BuildInclusionsForTier(form, suffix);

                result.Add(new ParsedTier
                {
                    TicketTypeId = GetInt(form, $"tier-id-{suffix}"),
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
                    MemberListUrl = GetVerifiedMemberListUrl(
                        eventId,
                        suffix,
                        GetForm(form, $"tier-memberfileurl-{suffix}")),
                    StandingCapacity = GetInt(form, $"standcap-{suffix}"),
                    OpenSeatingCapacity = GetInt(form, $"opencap-{suffix}"),
                    NumberOfTables = GetInt(form, $"tables-{suffix}"),
                    ChairsPerTable = GetInt(form, $"chairs-{suffix}"),
                    IsSeating = GetBool(form, $"tier-seating-{suffix}"),
                    Rows = GetInt(form, $"rows-{suffix}"),
                    Columns = GetInt(form, $"seats-{suffix}"),
                    AccessMode = accessMode,
                    SelectedDateIds = selectedDateIds,
                    SelectedSlotIds = selectedSlotIds,
                    Inclusions = inclusions
                });
            }

            return result;
        }

        private List<ParsedTier> BuildFreeTier(int eventId)
        {
            if (!Request.HasFormContentType)
                return new List<ParsedTier>();

            var form = Request.Form;
            var quantity = GetInt(form, "free-qty");
            var capacity = GetInt(form, "free-cap");

            string accessMode = GetForm(form, "acc-mode-free", "all");
            var selectedDateIds = GetFormIntList(form, "acc-free-day-");
            var selectedSlotIds = GetFormIntList(form, "acc-free-slot-");

            var inclusions = BuildInclusionsForTier(form, "free");

            return new List<ParsedTier>
            {
                new ParsedTier
                {
                    TicketTypeId = GetInt(form, "free-tier-id"),
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
                    AccessMode = accessMode,
                    SelectedDateIds = selectedDateIds,
                    SelectedSlotIds = selectedSlotIds,
                    Inclusions = inclusions
                }
            };
        }

        private List<Inclusions> BuildInclusionsForTier(IFormCollection form, string suffix)
        {
            var inclusions = new List<Inclusions>();

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