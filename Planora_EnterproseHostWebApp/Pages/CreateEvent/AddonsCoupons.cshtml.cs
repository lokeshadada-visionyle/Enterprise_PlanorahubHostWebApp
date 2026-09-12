using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;
using Planora_EnterproseHostWebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class AddonRowVm
    {
        public int? AddOnId { get; set; }
        public string Name { get; set; } = "";
        public string Price { get; set; } = "";
        public string Stock { get; set; } = "";
        public string Description { get; set; } = "";
        public string? Error { get; set; }
    }

    public class PromoRowVm
    {
        public int? PromoCodeId { get; set; }
        public string Code { get; set; } = "";
        public string DiscountType { get; set; } = "Percentage (%)";
        public string Value { get; set; } = "";
        public string MaxUses { get; set; } = "";
        public string PerUserLimit { get; set; } = "";
        public string MinOrder { get; set; } = "";
        public string ValidFrom { get; set; } = "";
        public string ValidTo { get; set; } = "";
        public string AppliesTo { get; set; } = "All";
        public string Status { get; set; } = "Active";
        public bool CanCombine { get; set; }
        public bool FirstTimeBuyer { get; set; }
        public string? Error { get; set; }
    }

    public class AddonsCouponsModel : PageModel
    {
        public GetDiscountExposureResp DiscountExposureData { get; set; } = new GetDiscountExposureResp();
        public TicketTypeResp TicketTypeData { get; set; } = new TicketTypeResp();
        public List<AddonRowVm> AddonRows { get; set; } = new();
        public List<PromoRowVm> PromoRows { get; set; } = new();
        public string? ApiError { get; private set; }
        public bool IsPrivateEvent { get; private set; }
        public bool IsEditMode { get; private set; }

        private static readonly Regex NamePattern = new(@"^[a-zA-Z0-9 ]+$", RegexOptions.Compiled);
        private static readonly Regex CodePattern = new(@"^[a-zA-Z0-9]+$", RegexOptions.Compiled);

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 6;

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            IsPrivateEvent = HttpContext.Session.IsPrivateEvent();

            var eventId = HttpContext.Session.GetInt32("createdEventId");
            IsEditMode = eventId.HasValue && eventId.Value > 0;
            var helper = new CommonHelper();

            try
            {
                if (IsEditMode)
                {
                    DiscountExposureData = helper.GetDiscountExposure(userId.Value, eventId!.Value);
                    TicketTypeData = helper.GetTicketType(userId.Value, eventId.Value);

                    var addOnsResp = helper.GetEventAddOns(userId.Value, eventId.Value);
                    if (addOnsResp?.AddOns != null && addOnsResp.AddOns.Any())
                    {
                        AddonRows = addOnsResp.AddOns
                            .Where(a => a.IsActive)
                            .OrderBy(a => a.SortOrder)
                            .Select(a => new AddonRowVm
                            {
                                AddOnId = a.AddOnId,
                                Name = a.Name ?? "",
                                Price = a.Price.ToString(CultureInfo.InvariantCulture),
                                Stock = a.StockLimit.ToString(CultureInfo.InvariantCulture),
                                Description = a.Description ?? ""
                            })
                            .ToList();
                    }

                    var promoResp = helper.GetPromoCodes(userId.Value, eventId.Value);
                    if (promoResp?.PromoCodes != null && promoResp.PromoCodes.Any())
                    {
                        PromoRows = promoResp.PromoCodes
                            .Select(p => new PromoRowVm
                            {
                                PromoCodeId = p.PromoCodeId,
                                Code = p.Code ?? "",
                                DiscountType = string.Equals(p.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase)
                                    ? "Percentage (%)" : "Fixed amount (\u20a6)",
                                Value = p.DiscountValue.ToString(CultureInfo.InvariantCulture),
                                MaxUses = p.MaxUses.ToString(CultureInfo.InvariantCulture),
                                PerUserLimit = p.PerUserLimit.ToString(CultureInfo.InvariantCulture),
                                MinOrder = p.MinOrderAmount.ToString(CultureInfo.InvariantCulture),
                                ValidFrom = p.ValidFrom == default ? "" : p.ValidFrom.ToString("yyyy-MM-dd"),
                                ValidTo = p.ValidTo == default ? "" : p.ValidTo.ToString("yyyy-MM-dd"),
                                AppliesTo = p.ApplicableTicketTypeId > 0 ? p.ApplicableTicketTypeId.ToString() : "All",
                                Status = string.IsNullOrWhiteSpace(p.Status) ? "Active" : p.Status,
                                CanCombine = p.CanCombineCodes,
                                FirstTimeBuyer = p.IsFirstTimeBuyer
                            })
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load existing add-ons and promo codes.";
                Debug.WriteLine(ex.Message);
            }

            if (AddonRows.Count == 0) AddonRows.Add(new AddonRowVm());
            if (PromoRows.Count == 0) PromoRows.Add(new PromoRowVm());

            return Page();
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            IsPrivateEvent = HttpContext.Session.IsPrivateEvent();
            var eventId = HttpContext.Session.GetInt32("createdEventId");
            IsEditMode = eventId.HasValue && eventId.Value > 0;

            if (!eventId.HasValue || eventId.Value <= 0)
            {
                ApiError = "No event was found in progress. Please start again from Basic Details.";
                AddonRows.Add(new AddonRowVm());
                PromoRows.Add(new PromoRowVm());
                return Page();
            }

            var helper = new CommonHelper();

            // Reload so the page renders correctly if we bail out below.
            try
            {
                TicketTypeData = helper.GetTicketType(userId.Value, eventId.Value);
                DiscountExposureData = helper.GetDiscountExposure(userId.Value, eventId.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            var (addOnRows, addOnErrors) = ParseAndValidateAddOns(Request.Form);
            var (promoRows, promoErrors) = ParseAndValidatePromoCodes(Request.Form);

            AddonRows = addOnRows;
            PromoRows = promoRows;

            if (addOnErrors.Count > 0 || promoErrors.Count > 0)
            {
                ApiError = "Please fix the highlighted fields below.";
                return Page();
            }

            try
            {
                if (!SaveAddOns(helper, userId.Value, eventId.Value, addOnRows, out var addOnError))
                {
                    ApiError = addOnError;
                    return Page();
                }

                if (!SavePromoCodes(helper, userId.Value, eventId.Value, promoRows, out var promoError))
                {
                    ApiError = promoError;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ApiError = "Unable to reach the event service. Please try again.";
                Debug.WriteLine(ex.Message);
                return Page();
            }

            int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
            HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 7));

            return IsPrivateEvent
                ? RedirectToPage("/CreateEvent/FoodBeverage")
                : RedirectToPage("/CreateEvent/AccessControl");
        }

        // ------------------------------------------------------------------
        // Add-ons: Add / Update / Delete reconciliation (no duplicate saves)
        // ------------------------------------------------------------------
        private bool SaveAddOns(CommonHelper helper, long userId, int eventId, List<AddonRowVm> rows, out string? error)
        {
            error = null;

            var usableRows = rows.Where(r => !string.IsNullOrWhiteSpace(r.Name)).ToList();

            // Authoritative server state right now, so a stale/duplicate save can never happen
            // even if two tabs or a slow round trip are involved.
            List<EventAddOnItem> existingAddOns;
            try
            {
                existingAddOns = helper.GetEventAddOns(userId, eventId)?.AddOns?.Where(a => a.IsActive).ToList()
                                 ?? new List<EventAddOnItem>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetEventAddOns failed: " + ex.Message);
                existingAddOns = new List<EventAddOnItem>();
            }

            var existingIds = existingAddOns.Select(a => a.AddOnId).ToHashSet();
            var submittedIds = usableRows.Where(r => r.AddOnId.HasValue).Select(r => r.AddOnId!.Value).ToHashSet();

            // Rows the host removed from the form are deleted on the server too.
            foreach (var deletedId in existingIds.Except(submittedIds))
            {
                try { helper.DeleteAddon(userId, deletedId); }
                catch (Exception ex) { Debug.WriteLine("DeleteAddon failed for " + deletedId + ": " + ex.Message); }
            }

            var toAdd = new List<AddOns>();
            var toUpdate = new List<UpdateAddOns>();
            int sortOrder = 0;

            foreach (var row in usableRows)
            {
                decimal.TryParse(row.Price, NumberStyles.Number, CultureInfo.InvariantCulture, out var price);
                int.TryParse(row.Stock, out var stock);
                var description = string.IsNullOrWhiteSpace(row.Description) ? null : row.Description.Trim();

                if (row.AddOnId.HasValue && existingIds.Contains(row.AddOnId.Value))
                {
                    toUpdate.Add(new UpdateAddOns
                    {
                        AddOnId = row.AddOnId.Value,
                        Name = row.Name.Trim(),
                        Description = description,
                        Price = price,
                        StockLimit = stock,
                        SortOrder = sortOrder++
                    });
                }
                else
                {
                    toAdd.Add(new AddOns
                    {
                        Name = row.Name.Trim(),
                        Description = description,
                        Price = price,
                        StockLimit = stock,
                        SortOrder = sortOrder++
                    });
                }
            }

            if (toAdd.Count > 0)
            {
                var addResult = helper.AddAddons(new AddAddonsReq { UserId = userId, EventId = eventId, AddOns = toAdd });
                if (addResult == null || addResult.Status != 1)
                {
                    error = !string.IsNullOrWhiteSpace(addResult?.Message) ? addResult.Message : "Unable to save new add-ons.";
                    return false;
                }
            }

            if (toUpdate.Count > 0)
            {
                var updateResult = helper.UpdateAddons(new UpdateAddonsReq { UserId = userId, EventId = eventId, AddOns = toUpdate });
                if (updateResult == null || updateResult.Status != 1)
                {
                    error = !string.IsNullOrWhiteSpace(updateResult?.Message) ? updateResult.Message : "Unable to update existing add-ons.";
                    return false;
                }
            }

            return true;
        }

        // ------------------------------------------------------------------
        // Promo codes: Add / Update / Delete reconciliation (no duplicate saves)
        // ------------------------------------------------------------------
        private bool SavePromoCodes(CommonHelper helper, long userId, int eventId, List<PromoRowVm> rows, out string? error)
        {
            error = null;

            var usableRows = rows.Where(r => !string.IsNullOrWhiteSpace(r.Code)).ToList();

            List<PromoCodeItem> existingPromos;
            try
            {
                existingPromos = helper.GetPromoCodes(userId, eventId)?.PromoCodes ?? new List<PromoCodeItem>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetPromoCodes failed: " + ex.Message);
                existingPromos = new List<PromoCodeItem>();
            }

            var existingIds = existingPromos.Select(p => p.PromoCodeId).ToHashSet();
            var submittedIds = usableRows.Where(r => r.PromoCodeId.HasValue).Select(r => r.PromoCodeId!.Value).ToHashSet();

            foreach (var deletedId in existingIds.Except(submittedIds))
            {
                try { helper.DeletePromoCode(userId, deletedId); }
                catch (Exception ex) { Debug.WriteLine("DeletePromoCode failed for " + deletedId + ": " + ex.Message); }
            }

            var toAdd = new List<PromoCodes>();
            var toUpdate = new List<UpdatePromoCodes>();

            foreach (var row in usableRows)
            {
                decimal.TryParse(row.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value);
                int.TryParse(row.MaxUses, out var maxUses);
                int.TryParse(row.PerUserLimit, out var perUserLimit);
                decimal.TryParse(row.MinOrder, NumberStyles.Number, CultureInfo.InvariantCulture, out var minOrder);
                DateTime.TryParse(row.ValidFrom, out var validFrom);
                DateTime.TryParse(row.ValidTo, out var validTo);

                var discountType = row.DiscountType.StartsWith("Percentage", StringComparison.OrdinalIgnoreCase) ? "Percentage" : "Fixed";
                var appliesToId = ResolveTicketTypeId(row.AppliesTo);
                var code = row.Code.Trim().ToUpperInvariant();

                if (row.PromoCodeId.HasValue && existingIds.Contains(row.PromoCodeId.Value))
                {
                    toUpdate.Add(new UpdatePromoCodes
                    {
                        PromoCodeId = row.PromoCodeId.Value,
                        Code = code,
                        DiscountType = discountType,
                        DiscountValue = value,
                        MaxUses = maxUses,
                        PerUserLimit = perUserLimit,
                        ValidFrom = validFrom,
                        ValidTo = validTo,
                        MinOrderAmount = minOrder,
                        ApplicableTicketTypeId = appliesToId,
                        CanCombineCodes = row.CanCombine,
                        IsFirstTimeBuyer = row.FirstTimeBuyer,
                        Status = row.Status
                    });
                }
                else
                {
                    toAdd.Add(new PromoCodes
                    {
                        Code = code,
                        DiscountType = discountType,
                        DiscountValue = value,
                        MaxUses = maxUses,
                        PerUserLimit = perUserLimit,
                        ValidFrom = validFrom,
                        ValidTo = validTo,
                        MinOrderAmount = minOrder,
                        ApplicableTicketTypeId = appliesToId,
                        CanCombineCodes = row.CanCombine,
                        IsFirstTimeBuyer = row.FirstTimeBuyer,
                        Status = row.Status
                    });
                }
            }

            if (toAdd.Count > 0)
            {
                var addResult = helper.AddCouponsReq(new AddPromoCodeReq { UserId = userId, EventId = eventId, PromoCodes = toAdd });
                if (addResult == null || addResult.Status != 1)
                {
                    error = !string.IsNullOrWhiteSpace(addResult?.Message) ? addResult.Message : "Unable to save new promo codes.";
                    return false;
                }
            }

            if (toUpdate.Count > 0)
            {
                var updateResult = helper.UpdatePromoCode(new UpdatePromoCodeReq { UserId = userId, EventId = eventId, PromoCodes = toUpdate });
                if (updateResult == null || updateResult.Status != 1)
                {
                    error = !string.IsNullOrWhiteSpace(updateResult?.Message) ? updateResult.Message : "Unable to update existing promo codes.";
                    return false;
                }
            }

            return true;
        }

        // ------------------------------------------------------------------
        // Parsing + server-side validation (mirrors BasicDetails.cshtml validation rules)
        // ------------------------------------------------------------------
        private (List<AddonRowVm> rows, List<string> errors) ParseAndValidateAddOns(IFormCollection form)
        {
            var rows = new List<AddonRowVm>();
            var errors = new List<string>();
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var indices = form.Keys
                .Where(k => k.StartsWith("addon-name-", StringComparison.Ordinal))
                .Select(k => k.Substring("addon-name-".Length))
                .Distinct()
                .OrderBy(x => int.TryParse(x, out var n) ? n : int.MaxValue);

            foreach (var idx in indices)
            {
                var row = new AddonRowVm
                {
                    Name = form[$"addon-name-{idx}"].ToString(),
                    Price = form[$"addon-price-{idx}"].ToString(),
                    Stock = form[$"addon-stock-{idx}"].ToString(),
                    Description = form[$"addon-desc-{idx}"].ToString()
                };

                if (int.TryParse(form[$"addon-id-{idx}"].ToString(), out var id) && id > 0)
                    row.AddOnId = id;

                // A fully empty extra row (e.g. added then left untouched) is silently dropped.
                if (string.IsNullOrWhiteSpace(row.Name) && string.IsNullOrWhiteSpace(row.Price) && string.IsNullOrWhiteSpace(row.Stock))
                {
                    continue;
                }

                var name = row.Name?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(name))
                {
                    row.Error = "Add-on name is required.";
                }
                else if (!NamePattern.IsMatch(name))
                {
                    row.Error = "Only letters, numbers, and spaces are allowed.";
                }
                else if (!seenNames.Add(name.ToUpperInvariant()))
                {
                    row.Error = $"\"{name}\" is already used by another add-on above.";
                }
                else if (!decimal.TryParse(row.Price, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price < 0)
                {
                    row.Error = "Enter a valid price of 0 or more.";
                }
                else if (!int.TryParse(row.Stock, out var stock) || stock < 0)
                {
                    row.Error = "Enter a valid stock count of 0 or more.";
                }

                if (row.Error != null) errors.Add(row.Error);
                rows.Add(row);
            }

            if (rows.Count == 0) rows.Add(new AddonRowVm());

            return (rows, errors);
        }

        private (List<PromoRowVm> rows, List<string> errors) ParseAndValidatePromoCodes(IFormCollection form)
        {
            var rows = new List<PromoRowVm>();
            var errors = new List<string>();
            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var indices = form.Keys
                .Where(k => k.StartsWith("promocode-", StringComparison.Ordinal))
                .Select(k => k.Substring("promocode-".Length))
                .Distinct()
                .OrderBy(x => int.TryParse(x, out var n) ? n : int.MaxValue);

            foreach (var idx in indices)
            {
                var row = new PromoRowVm
                {
                    Code = form[$"promocode-{idx}"].ToString(),
                    DiscountType = form[$"promotype-{idx}"].ToString(),
                    Value = form[$"promovalue-{idx}"].ToString(),
                    MaxUses = form[$"promomax-{idx}"].ToString(),
                    PerUserLimit = form[$"promouser-{idx}"].ToString(),
                    MinOrder = form[$"promomin-{idx}"].ToString(),
                    ValidFrom = form[$"promofrom-{idx}"].ToString(),
                    ValidTo = form[$"promountil-{idx}"].ToString(),
                    AppliesTo = form[$"promoapplies-{idx}"].ToString(),
                    Status = form[$"promostatus-{idx}"].ToString(),
                    CanCombine = form[$"stack-{idx}"] == "on",
                    FirstTimeBuyer = form[$"firstorder-{idx}"] == "on"
                };

                if (string.IsNullOrWhiteSpace(row.DiscountType)) row.DiscountType = "Percentage (%)";
                if (string.IsNullOrWhiteSpace(row.Status)) row.Status = "Active";
                if (string.IsNullOrWhiteSpace(row.AppliesTo)) row.AppliesTo = "All";

                if (int.TryParse(form[$"promo-id-{idx}"].ToString(), out var id) && id > 0)
                    row.PromoCodeId = id;

                if (string.IsNullOrWhiteSpace(row.Code) && string.IsNullOrWhiteSpace(row.Value))
                {
                    continue;
                }

                var code = row.Code?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(code))
                {
                    row.Error = "Promo code is required.";
                }
                else if (!CodePattern.IsMatch(code))
                {
                    row.Error = "Only letters and numbers are allowed — no spaces or symbols.";
                }
                else if (!seenCodes.Add(code.ToUpperInvariant()))
                {
                    row.Error = $"\"{code}\" is already used by another promo code above.";
                }
                else if (!decimal.TryParse(row.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) || value <= 0)
                {
                    row.Error = "Enter a discount value greater than 0.";
                }
                else if (row.DiscountType.StartsWith("Percentage", StringComparison.OrdinalIgnoreCase) && value > 100)
                {
                    row.Error = "A percentage discount can't exceed 100.";
                }
                else if (!string.IsNullOrWhiteSpace(row.MaxUses) && (!int.TryParse(row.MaxUses, out var maxUses) || maxUses < 0))
                {
                    row.Error = "Max redemptions must be 0 or more.";
                }
                else if (!string.IsNullOrWhiteSpace(row.PerUserLimit) && (!int.TryParse(row.PerUserLimit, out var perUser) || perUser < 0))
                {
                    row.Error = "Per-user limit must be 0 or more.";
                }
                else if (!string.IsNullOrWhiteSpace(row.MinOrder) && (!decimal.TryParse(row.MinOrder, NumberStyles.Number, CultureInfo.InvariantCulture, out var minOrder) || minOrder < 0))
                {
                    row.Error = "Minimum order must be 0 or more.";
                }
                else if (!string.IsNullOrWhiteSpace(row.ValidFrom) && !string.IsNullOrWhiteSpace(row.ValidTo)
                         && DateTime.TryParse(row.ValidFrom, out var vf) && DateTime.TryParse(row.ValidTo, out var vt)
                         && vt < vf)
                {
                    row.Error = "Valid Until can't be before Valid From.";
                }

                if (row.Error != null) errors.Add(row.Error);
                rows.Add(row);
            }

            if (rows.Count == 0) rows.Add(new PromoRowVm());

            return (rows, errors);
        }

        private static int ResolveTicketTypeId(string appliesToValue)
        {
            // The <select> posts either "All" or the numeric TicketTypeId itself.
            return int.TryParse(appliesToValue, out var id) && id > 0 ? id : 0;
        }
    }
}
