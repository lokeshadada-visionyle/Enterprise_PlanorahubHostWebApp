namespace Planora_EnterproseHostWebApp.Models
{
    public class HostDashboardResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string EnterpriseName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string MemberSince { get; set; } = string.Empty;
        public int TotalEvents { get; set; }
        public int LiveEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int EventsThisMonth { get; set; }
        public int TotalTicketSold { get; set; }
        public int TicketssoldThisMonth { get; set; }
        public string TicketPerThisMonth { get; set; } = string.Empty;
        public string GrossRevenue { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string GrossRevenuePerThisMonth { get; set; } = string.Empty;
        public int LiveCheckIns { get; set; }
        public string LiveEventName { get; set; } = string.Empty;
        public string LiveEventCapacityPer { get; set; } = string.Empty;
    }
    public class HostDashboardWeelkyResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string WeeklyPer { get; set; } = string.Empty;
        public List<WeeklyEvents> WeeklyEvents { get; set; } = new();
    }
    public class WeeklyEvents
    {
        public string Day { get; set; } = string.Empty;
        public int Events { get; set; }
    }
    public class LiveEventsResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<LiveEvents> LiveEvents { get; set; } = new();
    }
    public class LiveEvents
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
        public int CheckIns { get; set; }
        public string Revenue { get; set; } = string.Empty;
    }

    public class MyEventResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int TotalEvents { get; set; }
        public int LiveEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int CancelledEvents { get; set; }
        public int DraftEvents { get; set; }
        public int TicketsSold { get; set; }
        public string TotalRevenue { get; set; } = string.Empty;
        public int TotalCheckIns { get; set; }
        public List<MyEvents> MyEvents { get; set; } = new List<MyEvents>();
    }
    public class MyEvents
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventCategory { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
        public string EventChargeType { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public int SoldTickets { get; set; }
        public string SoldTicketsPer { get; set; } = string.Empty;
        public int CheckIns { get; set; }
        public string Revenue { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class EventHubOverviewResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Revenue { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public int TicketsSold { get; set; }
        public string AddOnRevenue { get; set; } = string.Empty;
        public int TotalCheckIns { get; set; }
        public List<TicketTypes> TicketTypes { get; set; } = new();
        public List<WeeklyRevenue> WeeklyRevenue { get; set; } = new();
    }
    public class TicketTypes
    {
        public int TicketTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string TicketSoldPer { get; set; } = string.Empty;
    }
    public class WeeklyRevenue
    {
        public string Day { get; set; } = string.Empty;
        public string Revenue { get; set; } = string.Empty;
    }
    public class EventHubDatesAndSlotsResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int CheckIns { get; set; }
        public int PendingCheckIn { get; set; }
        public string Revenue { get; set; } = string.Empty;
        public List<EventHubDates> EventDates { get; set; } = new();
    }
    public class EventHubDates
    {
        public int EventDateId { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public List<Slots> Slots { get; set; } = new();
    }
    public class Slots
    {
        public int SlotId { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public string SessionTIme { get; set; } = string.Empty;
    }
    public class EventHubGuestResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int TotalSeats { get; set; }
        public int SoldSeats { get; set; }
        public int LeftSeats { get; set; }
        public string AttendancePer { get; set; }
        public int FoodOrders { get; set; }
        public string FoodServedPer { get; set; }
        public int TotalPolls { get; set; }
        public int PollsRating { get; set; }
        public int TotalPollVotes { get; set; }
        public string Revenue { get; set; }
        public List<Guest> Guests { get; set; } = new List<Guest>();
    }
    public class Guest
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public string TicketType { get; set; }
        public string SeatNo { get; set; }
        public string RsvpStatus { get; set; }
        public string PaymentStatus { get; set; }
        public bool IsCheckIn { get; set; }
    }
    public class EventTicketTypeResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<EventHubTicketType> TicketTypes { get; set; } = new List<EventHubTicketType>();
    }
    public class EventHubTicketType
    {
        public int TicketTypeId { get; set; }
        public string TypeName { get; set; }
        public string Price { get; set; }
        public string LayoutType { get; set; }
        public string HallName { get; set; }
    }
    public class EventHubGuestRegistrationResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int TotalRegistrations { get; set; }
        public int RegistrationCompleted { get; set; }
        public string RegistrationPer { get; set; }
        public int MissingAnswers { get; set; }
        public int TotalFields { get; set; }
        public int BasicField { get; set; }
        public int FieldsAdded { get; set; }
        public List<EventHubGuest> Guests { get; set; } = new List<EventHubGuest>();
    }
    public class EventHubGuest
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public string PhoneNo { get; set; }
        public string Organization { get; set; }
        public string Jobtitle { get; set; }
        public string Dietary { get; set; }
        public string TicketType { get; set; }
        public string SubmittedAt { get; set; }
        public string Status { get; set; }
    }
    public class EventHubRsvpGuestResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<EventHubRsvpGuest> RsvpGuests { get; set; } = new List<EventHubRsvpGuest>();
    }
    public class EventHubRsvpGuest
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string TicketType { get; set; }
        public string AddedFrom { get; set; }
        public string Status { get; set; }
    }
    public class EventHubRsvpResponseResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int TotalGuests { get; set; }
        public int TotalInvitations { get; set; }
        public int TotalEmailsSent { get; set; }
        public int TotalWhatsAppSent { get; set; }
        public int Accepted { get; set; }
        public int DeclinedPer { get; set; }
        public int TotalTicketsIssued { get; set; }
        public int TicketsToIssue { get; set; }
        public List<RsvpGuestResponse> RsvpGuestResponses { get; set; } = new List<RsvpGuestResponse>();
    }
    public class RsvpGuestResponse
    {
        public int GuestId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Answers { get; set; }
        public string TicketRef { get; set; }
        public string Response { get; set; }
    }
    public class EventHubSeatStatusResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int TotalSeats { get; set; }
        public int SeatsSold { get; set; }
        public int SeatsLeft { get; set; }
        public List<EventHubRsvpTicketType> TicketTypes { get; set; } = new List<EventHubRsvpTicketType>();
    }
    public class EventHubRsvpTicketType
    {
        public int TicketTypeId { get; set; }
        public string TypeName { get; set; }
        public string Price { get; set; }
        public string LayoutType { get; set; }
        public string HallName { get; set; }
        public int TotalSeats { get; set; }
        public int SeatsSold { get; set; }
        public int HeldSeats { get; set; }
    }
    public class SeatResponseModel
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<SeatModel> Seats { get; set; } = new List<SeatModel>();
    }
    public class SeatModel
    {
        public int SeatId { get; set; }
        public string SeatNo { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCheckIn { get; set; }
    }
}
