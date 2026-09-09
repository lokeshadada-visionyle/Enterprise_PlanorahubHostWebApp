using System.Text.Json.Serialization;

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
    public class EventHubFoodMenuResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<EventHubMenuItems> MenuItems { get; set; }
    }
    public class EventHubMenuItems
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public int MaxServes { get; set; }
        public bool IsAvailable { get; set; }
    }
    public class EventHubFoodReadyToOrderreq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public int SLotId { get; set; }
        public bool Ready { get; set; }
    }
    public class EventHubServingLogResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int TotalServes { get; set; }
        public int TotalRefused { get; set; }
        public int ItemsOutOfStock { get; set; }
        public List<Servings> Servings { get; set; }
    }
    public class Servings
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; }
        public string Time { get; set; }
        public string ItemName { get; set; }
        public string Counter { get; set; }
        public string ServedBy { get; set; }
        public string Status { get; set; }
    }
    public class EventHubVendorResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<Vendors> Vendors { get; set; } = new List<Vendors>();
    }
    public class Vendors
    {
        public int VendorId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
        public string Pin { get; set; }
        public List<string> AssignedSlots { get; set; } = new List<string>();
        public int StaffCount { get; set; }
        public List<Staff> Staff { get; set; } = new List<Staff>();
    }
    public class Staff
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public string Zone { get; set; }
        public string PhoneNo { get; set; }
        public string Pin { get; set; }
        public List<string> AssignedSlots { get; set; } = new List<string>();
    }
    public class Request
    {
        public long UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public int VendorId { get; set; }
    }
    public class AddVendorRequest
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; }
        public string PhoneNo { get; set; }
        public string Role { get; set; }
        public int? VendorId { get; set; }
        public string Pin { get; set; }
        public int TicketTypeId { get; set; }
        public bool WholeEvent { get; set; }
        public List<int> SlotIds { get; set; } = new List<int>();
        public string hashValue { get; set; }
    }
    public class EditVendorRequest
    {
        public int UserId { get; set; }
        public string hashValue { get; set; }
        public int VendorId { get; set; }
        public int EventId { get; set; }
        public string VendorName { get; set; }
        public string PhoneNo { get; set; }
        public string Pin { get; set; }
        public bool WholeEvent { get; set; }
        public List<int> SlotIds { get; set; } = new List<int>();
    }
    public class EventResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string CoverImage { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public List<string> TicketTypes { get; set; } = new List<string>();
    }
    public class EventHubPollResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Poll> Polls { get; set; } = new List<Poll>();
    }
    public class Poll
    {
        public int PollId { get; set; }
        public string PollStatus { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<Option> Options { get; set; } = new List<Option>();
    }
    public class Option
    {
        public int OptionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Percentage { get; set; } = string.Empty;
    }
    public class AddPollRequest
    {
        public long UserId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public int EventId { get; set; }
        public int SlotId { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string VisibleTo { get; set; } = string.Empty;
    }
    public class EventHubBroadcastResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Broadcast> Broadcasts { get; set; } = new List<Broadcast>();
    }
    public class Broadcast
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Channels { get; set; } = new List<string>();
        public string SentAgo { get; set; } = string.Empty;
        public int Sent { get; set; }
        public int Opened { get; set; }
    }
    public class EventHubSalesResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string GrossRevenue { get; set; }
        public int TotalTickets { get; set; }
        public int TicketsSold { get; set; }
        public string AddOnRev { get; set; }
        public string Refunds { get; set; }
        public List<SalesTicketType> TicketTypes { get; set; } = new List<SalesTicketType>();
    }
    public class SalesTicketType
    {
        public string TypeName { get; set; }
        public int TotalTickets { get; set; }
        public int SoldTickets { get; set; }
        public string Revenue { get; set; }
    }
    public class EventHubInsightResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int TotalCheckins { get; set; }
        public int Registered { get; set; }
        public int AttendancePer { get; set; }
        public string BusiestTime { get; set; }
        public int TotalScans { get; set; }
        public int TotalPolls { get; set; }
        public int TotalVotes { get; set; }
        public string AvgRating { get; set; }
        public int AddonSalesCount { get; set; }
        public string AddonRevenue { get; set; }
    }
    public class EventHubPollAnalyticsResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int TotalPolls { get; set; }
        public int TotalResponses { get; set; }
        public string VotesPer { get; set; }
        public string AvgRating { get; set; }
        public List<EventHubSummaryPoll> Polls { get; set; } = new List<EventHubSummaryPoll>();
    }
    public class EventHubSummaryPoll
    {
        public string Question { get; set; }
        public int TotalResponses { get; set; }
        public string TopAnswer { get; set; }
        public string TopAnswerPer { get; set; }
        public string Status { get; set; }
    }
}
