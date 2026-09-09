using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Planora_EnterproseHostWebApp.Models;

namespace Planora_EnterproseHostWebApp.Pages.Events.EventHub
{
    public class EventHubPollsModel : PageModel
    {
        public EventHubPollResponse EventHubPollData { get; set; } = new EventHubPollResponse();

        public IActionResult OnGet(int eventId, int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null || HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToPage("/Login/Login");

            ViewData["EventId"] = eventId;
            ViewData["SlotId"] = slotId;

            var helper = new CommonHelper();
            try
            {
                var resp = helper.GetEventHubPollResp(userId.Value, eventId,slotId);
                if (resp != null && resp.Status == 1)
                {
                    EventHubPollData = resp;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to load polls: " + ex.Message);
            }

            return Page();
        }

        public IActionResult OnGetGetPolls(int eventId,int slotId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { status = 0, message = "Session expired." });

            var helper = new CommonHelper();
            var response = helper.GetEventHubPollResp(userId.Value, eventId,slotId);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new { status = 1, polls = response.Polls });
            }

            return new JsonResult(new { status = 0, message = response?.Message ?? "Failed to fetch polls." });
        }
        public IActionResult OnPostAddPoll([FromBody] AddPollRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return new JsonResult(new { success = false, message = "Session expired." });

            if (request == null || string.IsNullOrWhiteSpace(request.Question))
                return new JsonResult(new { success = false, message = "Poll question is required." });

            request.UserId = userId.Value;

            // Filter out empty options submitted by user
            request.Options = request.Options?
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim())
                .ToList() ?? new List<string>();

            if (request.Options.Count < 2)
                return new JsonResult(new { success = false, message = "Please provide at least two valid options." });

            var helper = new CommonHelper();
            var response = helper.AddPollsRequest(request);

            if (response != null && response.Status == 1)
            {
                return new JsonResult(new { success = true, message = response.Message ?? "Poll created successfully." });
            }

            return new JsonResult(new { success = false, message = response?.Message ?? "Failed to create poll." });
        }
    }
}