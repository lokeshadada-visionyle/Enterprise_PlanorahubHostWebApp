using Microsoft.AspNetCore.Http;
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

        // Bindable properties for UI initialization
        public bool IsFoodEnabled { get; set; } = true;
        public string ServingType { get; set; } = "live";
        public string AvailabilityScope { get; set; } = "AllSessions";
        public List<FoodEventDates> SavedAvailability { get; set; } = new();

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            ViewData["StepIndex"] = 8;

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            var eventId = HttpContext.Session.GetInt32("createdEventId");

            if (eventId.HasValue && eventId.Value != 0)
            {
                try
                {
                    var helper = new CommonHelper();

                    // 1. Get Food Details
                    var foodDetailsResp = helper.GetFoodDetails(userId.Value, eventId.Value);
                    if (foodDetailsResp != null)
                    {
                        IsFoodEnabled = foodDetailsResp.IsFoodEnabled;
                        ServingType = string.IsNullOrWhiteSpace(foodDetailsResp.ServingType) ? "live" : foodDetailsResp.ServingType;
                    }

                    // 2. Get Food Availability
                    var availabilityResp = helper.GetFoodAvaliablity(userId.Value, eventId.Value);
                    if (availabilityResp != null)
                    {
                        AvailabilityScope = string.IsNullOrWhiteSpace(availabilityResp.AvailabilityScope) ? "AllSessions" : availabilityResp.AvailabilityScope;
                        SavedAvailability = availabilityResp.EventDates ?? new List<FoodEventDates>();
                    }

                    // 3. Get Food Categories
                    var catResp = helper.GetFoodMenuCategory(userId.Value, eventId.Value);
                    if (catResp?.Status == 1 && catResp.Categories != null)
                    {
                        Categories = catResp.Categories.Select(c => new Categories
                        {
                            CategoryId = c.CategoryId,
                            CategoryName = c.CategoryName,
                            MenuItems = new List<FoodMenuItems>()
                        }).ToList();
                    }

                    // 4. Get Menu Items & Map to Categories
                    var menuResp = helper.GetMenuDetails(userId.Value, eventId.Value);
                    if (menuResp?.Status == 1 && menuResp.Categories != null)
                    {
                        foreach (var cat in Categories)
                        {
                            var existingCat = menuResp.Categories.FirstOrDefault(c => c.CategoryId == cat.CategoryId);
                            if (existingCat != null)
                            {
                                cat.MenuItems = existingCat.MenuItems ?? new List<FoodMenuItems>();
                            }
                        }
                    }
                }
                catch
                {
                    // Fail gracefully on initial load rendering defaults
                }
            }

            return Page();
        }

        public IActionResult OnGetDateAndSlots()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Login/Login");
            }

            var eventId = HttpContext.Session.GetInt32("createdEventId");

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
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            int currentProg = HttpContext.Session.GetInt32("StepProgress") ?? 0;

            if (!userId.HasValue || userId.Value <= 0 || string.IsNullOrEmpty(isLoggedIn) || !isLoggedIn.Equals("true", StringComparison.OrdinalIgnoreCase))
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

            // -------------------------------------------------------------
            // STEP 1: ADD / UPDATE FOOD DETAILS
            // -------------------------------------------------------------
            try
            {
                var existingDetails = helper.GetFoodDetails(userId.Value, eventId.Value);
                if (existingDetails != null && existingDetails.Status == 1)
                {
                    helper.UpdateFoodDeatils(new UpdateFoodDetailsReq
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        IsFoodEnabled = isFoodEnabled,
                        ServingType = servingType,
                        RequireScan = false
                    });
                }
                else
                {
                    helper.AddFoodDeatils(new AddFoodDetailsReq
                    {
                        UserId = userId.Value,
                        EventId = eventId.Value,
                        IsFoodEnabled = isFoodEnabled,
                        ServingType = servingType,
                        RequireScan = false
                    });
                }
            }
            catch
            {
                FoodError = "Unable to save food settings right now. Please try again.";
                return Page();
            }

            // If Food is disabled, bypass remaining steps and continue workflow
            if (!isFoodEnabled)
            {
                HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProg, 9));
                return RedirectToPage("/CreateEvent/AccessControl");
            }

            // -------------------------------------------------------------
            // STEP 2: SAVE FOOD AVAILABILITY
            // -------------------------------------------------------------
            string scope = Request.Form["food-scope"].ToString();
            if (string.IsNullOrWhiteSpace(scope)) scope = "AllSessions";

            var eventDates = scope == "SelectedDates" ? ParseFoodAvailability() : new List<FoodEventDates>();

            try
            {
                var availabilityResult = helper.SaveFoodAvaliablity(new SaveFoodAvailabilityReq
                {
                    UserId = userId.Value,
                    EventId = eventId.Value,
                    AvailabilityScope = scope,
                    EventDates = eventDates
                });

                if (availabilityResult is null || availabilityResult.Status != 1)
                {
                    FoodError = availabilityResult?.Message ?? "Unable to save food availability.";
                    return Page();
                }
            }
            catch
            {
                FoodError = "Unable to save food availability right now. Please try again.";
                return Page();
            }

            // -------------------------------------------------------------
            // STEP 3: ADD / UPDATE MENU ITEMS
            // -------------------------------------------------------------
            var menuItems = servingType.Equals("counter", StringComparison.OrdinalIgnoreCase)
                ? ParseMenuItems(idPrefix: "counter-menu-", hasExtras: false)
                : ParseMenuItems(idPrefix: "live-menu-", hasExtras: true);

            if (menuItems.Count > 0)
            {
                try
                {
                    var existingMenu = helper.GetMenuDetails(userId.Value, eventId.Value);
                    if (existingMenu != null && existingMenu.Status == 1 && existingMenu.Categories != null && existingMenu.Categories.Any(c => c.MenuItems != null && c.MenuItems.Any()))
                    {
                        var updateReq = new UpdateMenuItemReq
                        {
                            UserId = userId.Value,
                            EventId = eventId.Value,
                            MenuItems = menuItems.Select(m => new UpdateMenuItems
                            {
                                MenuItemId = m.MenuItemId,
                                CategoryId = m.CategoryId,
                                ItemName = m.ItemName,
                                IsVeg = m.IsVeg,
                                Extras = m.Extras
                            }).ToList()
                        };

                        var menuResult = helper.UpdateMenuItems(updateReq);
                        if (menuResult is null || menuResult.Status != 1)
                        {
                            FoodError = menuResult?.Message ?? "Unable to update the menu items.";
                            return Page();
                        }
                    }
                    else
                    {
                        var addReq = new AddMenuItemReq
                        {
                            UserId = userId.Value,
                            EventId = eventId.Value,
                            MenuItems = menuItems
                        };

                        var menuResult = helper.AddMenuItems(addReq);
                        if (menuResult is null || menuResult.Status != 1)
                        {
                            FoodError = menuResult?.Message ?? "Unable to add the menu items.";
                            return Page();
                        }
                    }
                }
                catch
                {
                    FoodError = "Unable to save the menu right now. Please try again.";
                    return Page();
                }
            }

            // -------------------------------------------------------------
            // STEP 4: UPLOAD MENU FILE (IF ATTACHED)
            // -------------------------------------------------------------
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
                    // Non-blocking upload fallback
                }
            }

            HttpContext.Session.SetInt32("StepProgress", Math.Max(currentProg, 8));
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

                int.TryParse(idx, out var menuItemId);
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
                    MenuItemId = menuItemId,
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