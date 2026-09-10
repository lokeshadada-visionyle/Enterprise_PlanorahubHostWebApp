using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Extensions;
using Planora_EnterproseHostWebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class AddonsCouponsModel : PageModel
    {
        public Response AddOnsResponse { get; private set; } = new();
        public AddEventPolicyResp CouponsResponse { get; private set; } = new();
        public GetDiscountExposureResp DiscountExposureData { get; set; } = new GetDiscountExposureResp();
        public TicketTypeResp TicketTypeData { get; set; } = new TicketTypeResp();
        public string? ApiError { get; private set; }
        public bool IsPrivateEvent { get; private set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 6;
            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            IsPrivateEvent = HttpContext.Session.IsPrivateEvent();

            var eventId = HttpContext.Session.GetInt32("createdEventId");
            var helper = new CommonHelper();
            try
            {
                DiscountExposureData = helper.GetDiscountExposure(userId.Value, eventId.Value);
                TicketTypeData = helper.GetTicketType(userId.Value, eventId.Value);
            }
            catch (Exception ex)
            {
                ApiError = "Failed to load discount exposure details.";
                Debug.WriteLine(ex.Message);
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                return RedirectToPage("/Login/Login");
            }

            var eventId = HttpContext.Session.GetInt32("createdEventId");
            

            var addOns = ParseAddOns(Request.Form);
            var promoCodes = ParsePromoCodes(Request.Form);

            try
            {
                var helper = new CommonHelper();

                if (addOns.Count > 0)
                {
                    var addOnsResult = helper.AddAddons(new AddAddonsReq
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        AddOns = addOns
                    });

                    if (addOnsResult == null || addOnsResult.Status != 1)
                    {
                        AddOnsResponse = addOnsResult ?? new Response();
                        ApiError = addOnsResult != null && !string.IsNullOrWhiteSpace(addOnsResult.Message)
                            ? addOnsResult.Message
                            : "Unable to save add-ons.";
                        return Page();
                    }

                    AddOnsResponse = addOnsResult;
                }

                if (promoCodes.Count > 0)
                {
                    var couponsResult = helper.AddCouponsReq(new AddPromoCodeReq
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        PromoCodes = promoCodes
                    });

                    if (couponsResult == null || couponsResult.Status != 1)
                    {
                        CouponsResponse = couponsResult ?? new AddEventPolicyResp();
                        ApiError = couponsResult != null && !string.IsNullOrWhiteSpace(couponsResult.Message)
                            ? couponsResult.Message
                            : "Unable to save promo codes.";
                        return Page();
                    }

                    CouponsResponse = couponsResult;
                }
            }
            catch
            {
                ApiError = "Unable to reach the event service. Please try again.";
                return Page();
            }
            int currentProgress = HttpContext.Session.GetInt32("StepProgress") ?? 0;
            HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProgress, 7));
            bool isPrivate = HttpContext.Session.IsPrivateEvent();
            return isPrivate
                ? RedirectToPage("/CreateEvent/FoodBeverage")
                : RedirectToPage("/CreateEvent/AccessControl");
        }

        private static List<AddOns> ParseAddOns(IFormCollection form)
{
    var result = new List<AddOns>();

    var indices = form.Keys
        .Where(k => k.StartsWith("addon-name-", StringComparison.Ordinal))
        .Select(k => k.Substring("addon-name-".Length))
        .Distinct()
        .OrderBy(x => x);

    int sortOrder = 0;

    foreach (var idx in indices)
    {
        string name = form[$"addon-name-{idx}"].ToString();

        if (string.IsNullOrWhiteSpace(name))
            continue;

        // Price
        decimal.TryParse(
            form[$"addon-price-{idx}"].ToString(),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var price
        );

        // Stock
        int.TryParse(
            form[$"addon-stock-{idx}"].ToString(),
            out var stock
        );

        // Description
        string description =
            form[$"addon-desc-{idx}"].ToString();


        result.Add(new AddOns
        {
            Name = name.Trim(),
            Price = price,
            StockLimit = stock,
            SortOrder = sortOrder++,

            // IMPORTANT
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim()
        });
    }

    return result;
}

        private static List<PromoCodes> ParsePromoCodes(IFormCollection form)
        {
            var result = new List<PromoCodes>();

            var slugs = form.Keys
                .Where(k => k.StartsWith("promocode-", StringComparison.Ordinal))
                .Select(k => k.Substring("promocode-".Length))
                .Distinct();

            foreach (var slug in slugs)
            {
                string code = form[$"promocode-{slug}"];
                if (string.IsNullOrWhiteSpace(code)) continue;

                decimal.TryParse(form[$"promovalue-{slug}"], NumberStyles.Number, CultureInfo.InvariantCulture, out var value);
                int.TryParse(form[$"promomax-{slug}"], out var maxUses);
                int.TryParse(form[$"promouser-{slug}"], out var perUserLimit);
                DateTime.TryParse(form[$"promofrom-{slug}"], out var validFrom);
                DateTime.TryParse(form[$"promountil-{slug}"], out var validTo);

                string discountTypeRaw = form[$"promotype-{slug}"];
                string discountType = discountTypeRaw.StartsWith("Percentage", StringComparison.OrdinalIgnoreCase)
                    ? "Percentage"
                    : "Fixed";

                string appliesToRaw = form[$"promoapplies-{slug}"];

                result.Add(new PromoCodes
                {
                    Code = code.Trim(),
                    DiscountType = discountType,
                    DiscountValue = value,
                    MaxUses = maxUses,
                    PerUserLimit = perUserLimit,
                    ValidFrom = validFrom,
                    ValidTo = validTo,
                    ApplicableTicketTypeId = ResolveTicketTypeId(appliesToRaw),
                    CanCombineCodes = form[$"stack-{slug}"] == "on",
                    IsFirstTimeBuyer = form[$"firstorder-{slug}"] == "on",
                    Status = form[$"promostatus-{slug}"]
                });
            }

            return result;
        }

        private static int ResolveTicketTypeId(string appliesToLabel)
        {
            return appliesToLabel switch
            {
                "All tiers" => 0,
                "Early Bird only" => 1,
                "Standard only" => 2,
                "VIP only" => 3,
                _ => 0 // "Standard, VIP" — multi-tier not representable yet
            };
        }
    }
}