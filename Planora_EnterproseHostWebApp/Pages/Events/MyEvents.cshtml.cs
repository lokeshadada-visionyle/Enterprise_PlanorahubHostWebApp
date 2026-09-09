using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events
{
    public class MyEventsModel : PageModel
    {
        public MyEventResponse myEventData { get; set; } = new MyEventResponse();
        public string ApiError { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int PageNo { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToPage("/Login/Login");

            var helper = new CommonHelper();
            var errors = new List<string>();

            try
            {
                var resp = helper.GetMyEvents(userId.Value, PageNo, 10, Status, Search);
                if (resp != null && resp.Status == 1)
                    myEventData = resp;
                else
                    errors.Add(resp?.Message ?? "Failed to load dashboard metrics.");
            }
            catch (Exception ex)
            {
                errors.Add("Failed to load dashboard metrics: " + ex.Message);
            }

            if (errors.Any())
            {
                ApiError = string.Join(" | ", errors);
            }

            return Page();
        }

        public JsonResult OnGetMyEventsData(int pageNo, int pageSize, string search, string status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return new JsonResult(new { status = 0, message = "Not authenticated" });

            try
            {
                var helper = new CommonHelper();
                var resp = helper.GetMyEvents(userId.Value, pageNo, pageSize, status, search);

                if (resp != null && resp.Status == 1)
                    return new JsonResult(new { status = 1, data = resp });

                return new JsonResult(new { status = 0, message = resp?.Message ?? "Failed to load events." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = 0, message = "Error: " + ex.Message });
            }
        }
    }
}