using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Planora_EnterproseHostWebApp.Pages.CreateEvent
{
    public class FoodBeverageModel : PageModel
    {
        public string? FoodError { get; private set; }

        public List<Categories> Categories { get; private set; } = new();

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Login/Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");

            if (userId is not null && eventId.HasValue && eventId.Value != 0)
            {
                try
                {
                    var helper = new CommonHelper();

                    // Load categories from API
                    var catResp = helper.GetFoodMenuCategory(userId.Value, eventId.Value);
                    if (catResp is not null && catResp.Status == 1 && catResp.Categories != null)
                    {
                        Categories = catResp.Categories.Select(c => new Categories
                        {
                            CategoryId = c.CategoryId,
                            CategoryName = c.CategoryName
                        }).ToList();
                    }

                    // Load existing menu details if available
                    var menu = helper.GetMenuDetails(userId.Value, eventId.Value);
                    if (menu is not null && menu.Status == 1 && menu.Categories != null)
                    {
                        foreach (var cat in Categories)
                        {
                            var existingCat = menu.Categories.FirstOrDefault(c => c.CategoryId == cat.CategoryId);
                            if (existingCat != null)
                            {
                                cat.MenuItems = existingCat.MenuItems;
                            }
                        }
                    }
                }
                catch
                {
                    // Fail gracefully on background data fetch errors
                }
            }

            return Page();
        }

        public JsonResult OnGetDateAndSlots()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var eventId = HttpContext.Session.GetInt32("createdEventId");

            if (userId is null || eventId is null || eventId == 0)
            {
                return new JsonResult(new { success = false, message = "Your session has expired. Please log in again." });
            }

            try
            {
                var helper = new CommonHelper();
                var resp = helper.GetEventDateAndSlots(userId.Value, eventId.Value);

                if (resp is null || resp.Status != 1)
                {
                    return new JsonResult(new { success = false, message = resp?.Message ?? "Unable to load the event schedule." });
                }

                var dates = (resp.EventDates ?? new List<EventDates>()).Select((d, i) => new
                {
                    eventDateId = d.EventDateId,
                    label = $"Day {i + 1} — {d.SessionName}",
                    date = d.EventDateValue,
                    slots = (d.EventSlots ?? new List<EventSlot>()).Select(s => new
                    {
                        slotId = s.SlotId,
                        name = s.SlotName,
                        time = $"{s.StartTime} – {s.EndTime}",
                        hall = "Hall " + s.HallId
                    })
                });

                return new JsonResult(new { success = true, dates });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Unable to reach the schedule service. " + ex.Message });
            }
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
            {
                return RedirectToPage("/Login/Login");
            }

            var eventId = HttpContext.Session.GetInt32("createdEventId");
            if (eventId is null || eventId == 0)
            {
                FoodError = "We couldn't find the event you're building. Please restart from Basic Details.";
                return Page();
            }

            bool isFoodEnabled = Request.Form["food-enabled"] == "true";
            string servingType = Request.Form["serving-type"].ToString();
            if (string.IsNullOrWhiteSpace(servingType)) servingType = "live";

            var helper = new CommonHelper();

            try
            {
                helper.AddFoodDeatils(new AddFoodDetailsReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    IsFoodEnabled = isFoodEnabled,
                    ServingType = servingType
                });
            }
            catch
            {
                FoodError = "Unable to save food settings right now. Please try again.";
                return Page();
            }

            if (!isFoodEnabled)
            {
                return RedirectToPage("/CreateEvent/AccessControl");
            }

            string scope = Request.Form["food-scope"].ToString();
            if (string.IsNullOrWhiteSpace(scope)) scope = "AllSessions";

            var eventDates = scope == "SelectedDates" ? ParseFoodAvailability() : new List<FoodEventDates>();

            Response availabilityResult;
            try
            {
                availabilityResult = helper.SaveFoodAvaliablity(new SaveFoodAvailabilityReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    AvailabilityScope = scope,
                    EventDates = eventDates
                });
            }
            catch
            {
                FoodError = "Unable to save food availability right now. Please try again.";
                return Page();
            }

            if (availabilityResult is null || availabilityResult.Status != 1)
            {
                FoodError = availabilityResult != null && !string.IsNullOrWhiteSpace(availabilityResult.Message)
                    ? availabilityResult.Message
                    : "Unable to save food availability.";
                return Page();
            }

            var menuItems = servingType.Equals("counter", StringComparison.OrdinalIgnoreCase)
                ? ParseMenuItems(idPrefix: "counter-menu-", hasExtras: false)
                : ParseMenuItems(idPrefix: "live-menu-", hasExtras: true);

            if (menuItems.Count > 0)
            {
                Response menuResult;
                try
                {
                    menuResult = helper.AddMenuItems(new AddMenuItemReq
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        MenuItems = menuItems
                    });
                }
                catch
                {
                    FoodError = "Unable to save the menu right now. Please try again.";
                    return Page();
                }

                if (menuResult is null || menuResult.Status != 1)
                {
                    FoodError = menuResult != null && !string.IsNullOrWhiteSpace(menuResult.Message)
                        ? menuResult.Message
                        : "Unable to save the menu.";
                    return Page();
                }
            }

            var csvFileName = Request.Form["menu-csv-filename"].ToString();
            var csvBase64 = Request.Form["menu-csv-base64"].ToString();
            if (!string.IsNullOrWhiteSpace(csvBase64))
            {
                try
                {
                    helper.UploadFoodMenu(new UploadMenuFilePOST
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        FileName = string.IsNullOrWhiteSpace(csvFileName) ? "menu.csv" : csvFileName,
                        FileBase64 = csvBase64
                    });
                }
                catch
                {
                }
            }

            return RedirectToPage("/CreateEvent/AccessControl");
        }

        private List<MenuItems> ParseMenuItems(string idPrefix, bool hasExtras)
        {
            var nameRegex = new Regex($@"^{Regex.Escape(idPrefix)}(\d+)-name$");
            var result = new List<MenuItems>();

            foreach (var key in Request.Form.Keys)
            {
                var match = nameRegex.Match(key);
                if (!match.Success) continue;

                var idx = match.Groups[1].Value;
                var name = Request.Form[$"{idPrefix}{idx}-name"].ToString();
                if (string.IsNullOrWhiteSpace(name)) continue;

                int.TryParse(Request.Form[$"{idPrefix}{idx}-cat"].ToString(), out var categoryId);
                var diet = Request.Form[$"{idPrefix}{idx}-diet"].ToString();

                var isVeg = diet.Equals("veg", StringComparison.OrdinalIgnoreCase)
                    || diet.Equals("vegan", StringComparison.OrdinalIgnoreCase);

                var extras = 0;
                if (hasExtras)
                {
                    int.TryParse(Request.Form[$"{idPrefix}{idx}-extra"].ToString(), out extras);
                }

                result.Add(new MenuItems
                {
                    CategoryId = categoryId,
                    ItemName = name.Trim(),
                    IsVeg = isVeg,
                    Extras = extras
                });
            }

            return result;
        }

        private List<FoodEventDates> ParseFoodAvailability()
        {
            var dayRegex = new Regex(@"^food-day-(\d+)$");
            var result = new List<FoodEventDates>();

            foreach (var key in Request.Form.Keys)
            {
                var match = dayRegex.Match(key);
                if (!match.Success) continue;

                var dayChecked = Request.Form[key].Any(v => v == "on");
                if (!dayChecked) continue;

                int.TryParse(match.Groups[1].Value, out var eventDateId);
                var slotPrefix = $"food-day-{eventDateId}-s-";
                var slotIds = new List<int>();

                foreach (var skey in Request.Form.Keys)
                {
                    if (!skey.StartsWith(slotPrefix)) continue;
                    if (!Request.Form[skey].Any(v => v == "on")) continue;

                    if (int.TryParse(skey.Substring(slotPrefix.Length), out var slotId))
                    {
                        slotIds.Add(slotId);
                    }
                }

                result.Add(new FoodEventDates
                {
                    EventDateId = eventDateId,
                    SlotId = slotIds
                });
            }

            return result;
        }
    }
}