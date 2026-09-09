namespace Planora_EnterproseHostWebApp.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string hashValue { get; set; } = "";
    }
    public class LoginResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
        public int UserId { get; set; }
    }
}
