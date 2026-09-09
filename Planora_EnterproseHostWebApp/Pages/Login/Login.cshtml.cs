using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace Planora_EnterproseHostWebApp.Pages.Login
{
    public class LoginModel : PageModel
    {
        private static readonly Regex EmailPattern =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        [BindProperty]
        public bool RememberMe { get; set; }

        public string LoginError { get; set; } = "";

        public IActionResult OnGet()
        {
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            if (HttpContext.Session.GetString("IsLoggedIn") == "true")
            {
                return RedirectToPage("/Dashboard/HostDashboard");
            }
            return Page();
        }

        private void SetSession(int userId, string userName, string email)
        {
            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetString("UserName", userName);
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("IsLoggedIn", "true");
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                LoginError = "Please enter your email and password.";
                return Page();
            }

            Email = Email.Trim();

            if (!EmailPattern.IsMatch(Email))
            {
                LoginError = "Enter a valid email address.";
                return Page();
            }

            if (Password.Length > 256)
            {
                LoginError = "Password must be 256 characters or fewer.";
                return Page();
            }

            try
            {
                var helper = new CommonHelper();
                var result = helper.Login(Email, Password);

                if (result != null && result.Status == 1)
                {
                    var userName = Email.Split('@')[0];
                    SetSession(result.UserId, userName, Email);
                    return RedirectToPage("/Dashboard/HostDashboard");
                }

                LoginError = result != null && !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Invalid email or password.";
            }
            catch
            {
                LoginError = "Connection error. Please try again.";
            }

            return Page();
        }
    }
}