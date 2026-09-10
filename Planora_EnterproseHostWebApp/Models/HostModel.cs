namespace Planora_EnterproseHostWebApp.Models
{
    public class SalesAndRevenueResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string GrossRev { get; set; } = string.Empty;
        public int TotalEventsForGrossRev { get; set; }
        public string Refunded { get; set; } = string.Empty;
        public int TotalRefunds { get; set; }
        public int TotalCancelled { get; set; }
        public string Commission { get; set; } = string.Empty;
        public string CommissionPer { get; set; } = string.Empty;
        public string NetToHost { get; set; } = string.Empty;
        public List<SalesAndRevenueEvent> Events { get; set; } = new();
    }

    public class SalesAndRevenueEvent
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventStatus { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public int SoldTickets { get; set; }
        public string GrossRev { get; set; } = string.Empty;
        public string Refunds { get; set; } = string.Empty;
        public string Commission { get; set; } = string.Empty;
        public string NetToHost { get; set; } = string.Empty;
    }

    public class SalesAndRevenueByEventResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string CardRev { get; set; } = string.Empty;
        public int CardPayments { get; set; }
        public string CardPer { get; set; } = string.Empty;
        public string BankTransferRev { get; set; } = string.Empty;
        public int BankTransferPayments { get; set; }
        public string BankTransferPer { get; set; } = string.Empty;
        public string UUSDRev { get; set; } = string.Empty;
        public int UUSDPayments { get; set; }
        public string UUSDPer { get; set; } = string.Empty;
        public string ComplimentaryRev { get; set; } = string.Empty;
        public int ComplimentoryPayments { get; set; }
        public string ComplimentoryPer { get; set; } = string.Empty;
        public List<TicketTypesRev> TicketTypesRev { get; set; } = new();
    }

    public class TicketTypesRev
    {
        public int TicketTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public int SoldTickets { get; set; }
        public string GrossRev { get; set; } = string.Empty;
        public string SharePer { get; set; } = string.Empty;
    }

    public class SalesAndRevenueByDaysResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<EventDays> EventDays { get; set; } = new();
    }

    public class EventDays
    {
        public string DayName { get; set; } = string.Empty;
        public string SlotName { get; set; } = string.Empty;
        public int SoldTickets { get; set; }
        public string GrossRev { get; set; } = string.Empty;
    }

    public class EventRefundsReq
    {
        public long UserId { get; set; }
        public int EventId { get; set; }
        public string hashValue { get; set; } = string.Empty;
    }

    public class EventRefundsResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Refunds> Refunds { get; set; } = new();
    }

    public class Refunds
    {
        public string Reference { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string Revenue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class EnterprisePlanDashResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalEvents { get; set; }
        public int EventsReached { get; set; }
        public int TotalGuests { get; set; }
        public int GuestsReached { get; set; }
        public string TicketRev { get; set; } = string.Empty;
        public string Commission { get; set; } = string.Empty;
        public List<PlanEvent> Events { get; set; } = new();
    }

    public class PlanEvent
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventStatus { get; set; } = string.Empty;
        public int TotalGuests { get; set; }
        public int GuestsAttended { get; set; }
        public int TotalDaysCount { get; set; }
        public int TotalSlotsCount { get; set; }
        public string TicketRev { get; set; } = string.Empty;
        public string EventChargeType { get; set; } = string.Empty;
    }

    public class EnterprisePackageDetailsResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Region { get; set; } = string.Empty;
        public string BillingTerms { get; set; } = string.Empty;
        public string ContractValue { get; set; } = string.Empty;
        public string ContractStartDate { get; set; } = string.Empty;
        public string ContractEndDate { get; set; } = string.Empty;
        public int TotalEvents { get; set; }
        public int EventsReached { get; set; }
        public int EventsLeft { get; set; }
        public string EventsPer { get; set; } = string.Empty;
        public int TotalGuests { get; set; }
        public int GuestsReached { get; set; }
        public int GuestsLeft { get; set; }
        public string GuestsPer { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int UsersReached { get; set; }
        public int UsersLeft { get; set; }
        public string UsersPer { get; set; } = string.Empty;
        public int WhatsAppCreditesReached { get; set; }
        public int TotalWhatsAppCredits { get; set; }
        public int WhatsAppCreditsLeft { get; set; }
        public string WhatsAppCreditsPer { get; set; } = string.Empty;
        public int EmailCreditesReached { get; set; }
        public int TotalEmailAppCredits { get; set; }
        public int EmailCreditsLeft { get; set; }
        public string EmailCreditsPer { get; set; } = string.Empty;
        public int TotalAppliedCredits { get; set; }
    }

    public class EnterpriseChannelCreditsDashResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalCreditsApplied { get; set; }
        public int WhatsAppCreditsLeft { get; set; }
        public int EmailCreditsLeft { get; set; }
        public int EmailCreditesReached { get; set; }
        public int WhatsAppCreditesReached { get; set; }
        public string WhatsAppUnitRate { get; set; } = string.Empty;
        public string EmailUnitRate { get; set; } = string.Empty;
    }

    public class EnterpriseBillingResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TotalContractedRevenue { get; set; } = string.Empty;
        public string TotalPaid { get; set; } = string.Empty;
        public string BalanceAmount { get; set; } = string.Empty;
        public string TicketedRevenue { get; set; } = string.Empty;
        public string TicketCommissionPer { get; set; } = string.Empty;
        public string PlatformCommission { get; set; } = string.Empty;
        public List<Invoices> Invoices { get; set; } = new();
    }

    public class Invoices
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string IssuedDate { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string TotalAmount { get; set; } = string.Empty;
        public List<Charges> Charges { get; set; } = new();
    }

    public class Charges
    {
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
    }

    public class EnterpriseAuditReportResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AuditReport> AuditReport { get; set; } = new();
    }

    public class AuditReport
    {
        public string Metric { get; set; } = string.Empty;
        public string Consumed { get; set; } = string.Empty;
        public string ConsumedValue { get; set; } = string.Empty;
        public string AdminConfiguration { get; set; } = string.Empty;
    }

    public class GuestCheckInsDashboardResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalBadges { get; set; }
        public int PendingBadges { get; set; }
        public int FailedBadges { get; set; }
        public int BadgesReprinted { get; set; }
        public int Registered { get; set; }
        public int TotalCheckIns { get; set; }
        public int NotArrived { get; set; }
        public string AttendancePer { get; set; } = string.Empty;
        public List<Halls> Halls { get; set; } = new();
    }

    public class Halls
    {
        public string HallName { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public int TotalScans { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ManualCheckInReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public int EventId { get; set; }
        public int SlotId { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    public class ManualCheckInGuestsResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Guests> Guests { get; set; } = new();
    }

    public class Guests
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string TIcketReference { get; set; } = string.Empty;
        public string TicketTypeName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string SlotName { get; set; } = string.Empty;
        public string SeatNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CheckInGuestsReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public int EventId { get; set; }
        public int SlotId { get; set; }
        public int GuestId { get; set; }
    }

    public class SeatingLayoutResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<SeatingTicketType> TicketTypes { get; set; } = new();
    }

    public class SeatingTicketType
    {
        public int TicketTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public List<SeatingSeat> Seats { get; set; } = new();
    }

    public class SeatingSeat
    {
        public int SeatId { get; set; }
        public string SeatNo { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class EventScanLogsResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ScanLogGuest> Guests { get; set; } = new();
    }

    public class ScanLogGuest
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string TicketTypeName { get; set; } = string.Empty;
        public string SeatNo { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public string CheckInBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
