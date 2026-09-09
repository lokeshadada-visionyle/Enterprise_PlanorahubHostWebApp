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
}
