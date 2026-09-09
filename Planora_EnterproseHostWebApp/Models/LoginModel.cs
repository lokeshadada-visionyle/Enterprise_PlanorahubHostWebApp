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

    public class EnterpriseResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string JoinedOn { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string SettlementCurrency { get; set; } = string.Empty;
    }

    public class UpdateEnterpriseReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string JoinedOn { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string SettlementCurrency { get; set; } = string.Empty;
    }
}
