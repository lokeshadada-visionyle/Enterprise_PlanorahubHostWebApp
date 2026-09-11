using System.Text.Json.Serialization;

namespace Planora_EnterproseHostWebApp.Models
{
    public class Response
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
        public int EventId { get; set; }
    }

    public class GetEventTypeResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";

        public bool PublicListedInDiscovery { get; set; }
        public bool PrivateListedInDiscovery { get; set; }

        public bool PublicHasLandingPage { get; set; }
        public bool PrivateHasLandingPage { get; set; }

        public bool PublicOpenRegistration { get; set; }
        public bool PrivateOpenRegistration { get; set; }

        public bool PublicEmailVerification { get; set; }
        public bool PrivateEmailVerification { get; set; }

        public bool PublicInviteList { get; set; }
        public bool PrivateInviteList { get; set; }

        public bool PublicRsvpApproval { get; set; }
        public bool PrivateRsvpApproval { get; set; }

        public bool PublicTicketing { get; set; }
        public bool PrivateTicketing { get; set; }

        public decimal PublicTicketCommission { get; set; }
        public decimal PrivateTicketCommission { get; set; }

        public bool PublicAddOnsCoupons { get; set; }
        public bool PrivateAddOnsCoupons { get; set; }

        public bool PublicFood { get; set; }
        public bool PrivateFood { get; set; }
    }

    public class Category
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
    public class GetEventCategoryResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
    }
    public class AddBasicDetailsReq
    {
        public int UserId { get; set; }
        public string hashValue { get; set; } = "";
        public string EventType { get; set; }
        public string EventName { get; set; }
        public int EventCategory { get; set; }
        public string TagLine { get; set; }
        public int CityId { get; set; }
        public string TimeZone { get; set; }
        public string description { get; set; }
        public string FileName { get; set; }
        public string ImageBase64 { get; set; }

    }
    public class EventDetails
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string EventCategory { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string TimeZone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
    public class UpdateEventDetailsRequest
    {
        public int UserId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string EventCategory { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string TimeZone { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
    public class AddThingToKnowReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; } = "";
        public int EventId { get; set; }
        //public int ImageURL { get; set; }
        public List<ThingToKnow> Things { get; set; }
    }
    public class ThingToKnow
    {
        public int? ThingToKnowItemId { get; set; }
        public string Category { get; set; }
        public string Value { get; set; } = "";
    }
    public class UpdateThingToKnowReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; } = "";
        public int EventId { get; set; }
        //public int ImageURL { get; set; }
        public List<ThingToKnow> Things { get; set; }
    }
    public class AddThingToKnowResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";

        public int InsertedCount { get; set; }
        public int EventId { get; set; }

        public bool IsEnterprise { get; set; }
        public bool Ispublicfreefirst { get; set; }

        public string EventChargeType { get; set; }
        public bool IsFirstFreeEvent { get; set; }
    }
    public class ThingsToKnowResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int InsertedCount { get; set; }
        public int EventId { get; set; }
        //public string ImageURL { get; set; } = string.Empty;
        public bool IsEnterprise { get; set; }
        public bool Ispublicfreefirst { get; set; }
        public string EventChargeType { get; set; } = string.Empty;
        public bool IsFirstFreeEvent { get; set; }
        public List<ThingToKnowItem> Things { get; set; } = new();
    }
    public class ThingToKnowItem
    {
        public int ThingToKnowItemId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
    public class UploadCoverImageReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; } = "";
        public int EventId { get; set; }
        public string FileName { get; set; } = "";
        public string ImageBase64 { get; set; } = "";
    }
    public class UploadCoverImageResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
        public long EventId { get; set; }
        public string ImageURL { get; set; } = "";
    }
    public class GetCitiesResp
    {
        public int Status { get; set; } = 1;
        public string Message { get; set; }
        public List<City> City { get; set; }
    }

    public class City
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        [JsonPropertyName("CountryName")]
        public string CountryName { get; set; }
        [JsonPropertyName("CountryCode")]
        private string CountryCode
        {
            set { if (string.IsNullOrEmpty(CountryName)) CountryName = value; }
        }
    }
    public class SaveScheduleRequest
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public List<Venue> Venues { get; set; } = new List<Venue>();
        public List<EventDateModel> Slots { get; set; } = new List<EventDateModel>();
        public List<Ent_Speaker> Speakers { get; set; } = new();
    }
    public class AddVenueAndHallReq
    {
        public int UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public List<Venue> Venues { get; set; }
    }
    public class Venue
    {
        public string VenueName { get; set; }
        public string Address { get; set; }
        public List<Hall> Halls { get; set; }
    }
    public class Hall
    {
        public string HallName { get; set; }
        public int Capacity { get; set; }
    }
    public class GenVenueHallReq
    {
        public int UserId { get;set;  }
        public int EventId { get; set; }
        public string hashValue { get; set; }
    }
    public class GetVenueAndHallResp
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public int EventId { get; set; }
        public List<GetVenue> Venues { get; set; } = new();
    }
    public class GetVenue
    {
        public int VenueId { get; set; }
        public string? VenueName { get; set; }
        public string? Address { get; set; }
        public List<GetHall> Halls { get; set; } = new();
    }
    public class GetHall
    {
        public int HallId { get; set; }
        public string? HallName { get; set; }
        public int Capacity { get; set; }
    }
    public class GetHallByVenueIdResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<GetHalls> Hall { get; set; }
    }
    public class GetHalls
    {
        public int HallId { get; set; }
        public string HallName { get; set; }
        public int Capacity { get; set; }
    }
    public class AddEventDateSlotsReq
    {
        public int UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public List<EventDateModel> EventDates { get; set; }
    }
    public class EventDateModel
    {
        public string SessionName { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
        public List<EventSlotModel> EventSlots { get; set; } = new List<EventSlotModel>();
    }
    public class EventSlotModel
    {
        public string SlotName { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int HallId { get; set; }
        public int Capacity { get; set; }
        public int SpeakersId { get; set; }
    }
    public class AddSpeakerReq
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string hashValue { get; set; }
        public List<Ent_Speaker>Ent_Speaker {  get; set; }  
    }
    public class Ent_Speaker
    {
        public string Name {  get; set; }
        public string JobTitle{  get; set; }
        public string Organization {  get; set; }
        public string Bio {  get; set; }
    }
    public class SpeakerResponseModel
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<Speaker> Speakers { get; set; } = new List<Speaker>();
    }
    public class Speaker
    {
        public int SpeakerId { get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set; }
        public string Organization { get; set; }
        public string Bio { get; set; }
    }

    public class AddEventPolicyReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public bool IsPollsEnabled{ get; set; }
        public bool IsFeedbackEnabled { get; set; }
        public bool CanGuestAttactPhoto { get; set; }
        public List<string> Description {  get; set; }
        public List<FAQItem> FAQs { get; set; }
    }
    public class FAQItem
    {
        public int FAQItemId { get; set; }
        public string Question {  get; set; }
        public string Answer {  get; set; }
    }
    public class EventFAQResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int EventId { get; set; }
        public bool IsPollsEnabled { get; set; }
        public bool IsFeedbackEnabled { get; set; }
        public bool CanGuestAttactPhoto { get; set; }
        public bool IsEnterprise { get; set; }
        public List<string> Description { get; set; } = new List<string>();
        public List<GetFAQItem> FAQs { get; set; } = new List<GetFAQItem>();
    }
    public class GetFAQItem
    {
        public int FAQItemId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
    public class UpdateEventFAQRequest
    {
        public long UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }

        public bool IsPollsEnabled { get; set; }
        public bool IsFeedbackEnabled { get; set; }
        public bool CanGuestAttactPhoto { get; set; }

        public List<string> Description { get; set; } = new List<string>();

        public List<UpdateFAQItem> FAQs { get; set; } = new List<UpdateFAQItem>();
    }
    public class UpdateFAQItem
    {
        public int FAQItemId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
    public class AddEventPolicyResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int EventId { get; set; }
        public bool IsEnterprise { get; set; }
        public bool Ispublicfreefirst { get; set; }
        public string EventChargeType { get; set; }
        public int Capacity { get; set; }
    }

    public class GetEventCurrencyResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int EventId { get; set; }
        public string Currency { get; set; }
    }
    public class CreateTicketTypeRequest
    {
        public long UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public List<AddTicketTypeReq> TicketTypes { get; set; } = new List<AddTicketTypeReq>();
    }
    public class AddTicketTypeReq
    {
        public bool IsVisible { get; set; }
        public string TypeName { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string SaleStart { get; set; }
        public string SaleEnd { get; set; }
        public string Description { get; set; }
        public bool AutoExpireEnabled { get; set; }
        public string AutoExpireMode { get; set; }
        public string MemberListUrl { get; set; }
        public string ExpiryDate { get; set; }
        public string ExpiryTime { get; set; }
        public int AllocationLimit { get; set; }
        public bool MemberOnly { get; set; }
        public int StandingCapacity { get; set; }
        public int OpenSeatingCapacity { get; set; }
        public int NumberOfTables { get; set; }
        public int ChairsPerTable { get; set; }
        public bool IsSeating { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string AccessMode { get; set; }
        public List<int> SelectedDateIds { get; set; }
        public List<int> SelectedSlotIds { get; set; }
        public List<Inclusions> Inclusions {  get; set; }
    }
    public class Inclusions
    {
        public string InclusionName { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }
    public class TicketTypesResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int EventId { get; set; }
        public List<TicketPricingTypes> TicketTypes { get; set; } = new List<TicketPricingTypes>();
    }

    public class TicketPricingTypes
    {
        public int TicketTypeId { get; set; }
        public bool IsVisible { get; set; }
        public string TypeName { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string SaleStart { get; set; }
        public string SaleEnd { get; set; }
        public string Description { get; set; }
        public bool AutoExpireEnabled { get; set; }
        public string AutoExpireMode { get; set; }
        public string ExpiryDate { get; set; }
        public string ExpiryTime { get; set; }
        public int AllocationLimit { get; set; }
        public bool MemberOnly { get; set; }
        public int StandingCapacity { get; set; }
        public int OpenSeatingCapacity { get; set; }
        public int NumberOfTables { get; set; }
        public int ChairsPerTable { get; set; }
        public bool IsSeating { get; set; }
        public int Rows { get; set; }
        public int Coulmns { get; set; }
        public string AccessMode { get; set; }
        public List<int> SelectedDateIds { get; set; } = new List<int>();
        public List<int> SelectedSlotIds { get; set; } = new List<int>();
        public List<GetInclusions> Inclusions { get; set; } = new List<GetInclusions>();
    }
    public class GetInclusions
    {
        public int InclusionId { get; set; }
        public string InclusionName { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }
    public class UploadMembersListReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public string FileName { get; set; }
        public string FileBase64 { get; set; }
    }
    public class UploadMembersListResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public long EventId { get; set; }
        public string FileURL { get; set; }
    }
    public class EventDateAndSlotsResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EventId { get; set; }
        public List<EventDates> EventDates { get; set; }
    }
    public class EventDates
    {
        public int EventDateId { get; set; }
        public string SessionName { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("EventDate")]
        public string EventDateValue { get; set; } = string.Empty;

        public List<EventSlot> EventSlots { get; set; } = new List<EventSlot>();
    }
    public class EventSlot
    {
        public int SlotId { get; set; }
        public string SlotName { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int HallId { get; set; }
        public int Capacity { get; set; }
        public string Speakers { get; set; } = string.Empty;
    }

    public class AddAddonsReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public List<AddOns> AddOns { get; set; }
    }
    public class AddOns
    {
        public string Name {  get; set; }
        public string Description { get; set; }
        public decimal Price {  get; set; }
        public int StockLimit {  get; set; }
        public int SortOrder { get; set; }
    }
    public class AddPromoCodeReq
    {
        public long UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public List<PromoCodes> PromoCodes { get; set; }
    }
    public class PromoCodes
    {
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public int MaxUses { get; set; }
        public int PerUserLimit { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int ApplicableTicketTypeId { get; set; }
        public bool CanCombineCodes { get; set; }
        public bool IsFirstTimeBuyer { get; set; }
        public string Status { get; set; }
    }
    public class GetDiscountExposureResp
    {
        public List<DiscountExposureItem> Discounts {  get; set; }
        public decimal MaximumDiscountExposure { get; set; } 
    }
    public class DiscountExposureItem
    {
        public string Code { get; set; }
        public string Description {  get; set; }
        public int MaxUse {  get; set; }
        public bool Ispaused { get; set; }
        public decimal DiscountExposure {  get; set; }
    }
    public class TicketTypeResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<TicketType> TicketTypes {  get; set; }
    }
    public class TicketType
    {
        public int TicketTypeId {  get; set; }
        public string TypeName {  get; set; }
    }

    public class AddFoodDetailsReq
    {
        public int UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public bool IsFoodEnabled { get; set; }
        public string ServingType { get; set; }
        public bool RequireScan { get; set; }
    }
    public class SaveFoodAvailabilityReq
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public string AvailabilityScope { get; set; } = string.Empty;
        public List<FoodEventDates> EventDates { get; set; } = new List<FoodEventDates>();
    }
    public class FoodEventDates
    {
        public int EventDateId { get; set; }
        public List<int> SlotId { get; set; } = new List<int>();
    }
    public class AddMenuItemReq
    {
        public int UserId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public int EventId { get; set; }
        public List<MenuItems> MenuItems { get; set; } = new List<MenuItems>();
    }
    public class MenuItems
    {
        public int CategoryId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public bool IsVeg { get; set; }
        public int Extras { get; set; }
    }
    public class AddEventFieldsReq
    {
        public int UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public string FormTitle { get; set; } = "Registration Form";
        public List<FieldItem> Fields { get; set; } = new List<FieldItem>();
    }
    public class GetFoodCategoryResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<FoodCategories> Categories { get; set; }
    }
    public class FoodCategories
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }
    }

    public class FieldItem
    {
        public string FieldType { get; set; }
        public string Label { get; set; }
        public string Placeholder { get; set; } = string.Empty;
        public string Options { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }
    public class UploadMenuFilePOST
    {
        public long UserId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileBase64 { get; set; } = string.Empty;
    }
    public class GetMenuDetailsResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Categories> Categories { get; set; } = new List<Categories>();
    }
    public class Categories
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<FoodMenuItems> MenuItems { get; set; } = new List<FoodMenuItems>();
        public int TotalMenuItems { get; set; }
    }
    public class FoodMenuItems
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public bool IsVeg { get; set; }
        public int Extras { get; set; }
    }
    public class EventAccessModel
    {
        public int UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public string AccessGatewayMode { get; set; }
        public bool IsUnListed { get; set; }
    }
    public class GetDefaultRegistrationFormsResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<RegistrationFormSummary> RegistrationForms { get; set; } = new();
    }
    public class RegistrationFormSummary
    {
        public int FormId { get; set; }
        public string FormTitle { get; set; }
        public int TotalFields { get; set; }
    }
    public class GetFormFieldsResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<FormFieldDto> Fields { get; set; } = new();
    }
    public class FormFieldDto
    {
        public int FieldId { get; set; }
        public string FieldType { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }
    public class GetRegistrationFormResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public List<FormFieldDto> Fields { get; set; } = new();
    }
    public class AddFormEventFieldsReq
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string hashValue { get; set; }
        public List<EventFieldInput> Fields { get; set; } = new();
    }
    public class EventFieldInput
    {
        public string Label { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
    }
    public class GetRsvpDefaultCustomFormsResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<CustomFormSummary> CustomForms { get; set; } = new();
    }
    public class CustomFormSummary
    {
        public string FormTitle { get; set; }
        public int TotalFields { get; set; }
    }

    public class AddLandingPageReq { 
        public  int UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public string Logo { get; set; }
        public string PrimaryColour {  get; set; }
        public string AccentColour {  get; set; }
        public string BackGroundColour {  get; set; }
        public string Link {  get; set; }
        public string Description { get; set; }
    }
    public class FileUploadModel
    {
        public long UserId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ImageBase64 { get; set; } = string.Empty;
    }
    public class UploadImageResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
        public string ImageURL { get; set; } = "";
    }
    public class EventPageDetailsResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string Eventcategory { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VenueName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public List<string> ThingsToknow { get; set; } = new List<string>();
        public List<string> TermsConditions { get; set; } = new List<string>();
        public List<LandingEventDates> EventDates { get; set; } = new List<LandingEventDates>();
        public List<Speakers> Speakers { get; set; } = new List<Speakers>();
        public List<Tickets> Tickets { get; set; } = new List<Tickets>();
        public List<FAQS> FAQS { get; set; } = new List<FAQS>();
    }
    public class LandingEventDates
    {
        public int EventDateId { get; set; }
        public string Label { get; set; } = string.Empty;
        public List<EventSlots> EventSlots { get; set; } = new List<EventSlots>();
    }
    public class EventSlots
    {
        public int SlotId { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public string SlotName { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public string Speaker { get; set; } = string.Empty;
    }
    public class Speakers
    {
        public int SpeakerId { get; set; }
        public string SpeakerName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
    }
    public class Tickets
    {
        public int TicketTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    public class FAQS
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }

    public class AddRsvpCustomsReq
    {
        public int UserId { get; set; }
        public string hashValue { get; set; }
        public int EventId { get; set; }
        public bool IsRsvpEnabled {  get; set; }
        public List<CustomFields> Custom {  get; set; }
    }
    public class CustomFields
    {
        public int CustomId {  get; set; }
        public string CustomName {  get; set; }
        public bool IsRequired { get; set; }
    }
    public class GetRsvpCustomsResp
    {
        public int Status {  get; set; }
        public string Message {  get; set; }
        public bool IsRsvpEnabled { get; set; }
        public bool IsEnterprise {  get; set; }
        public List<CustomFields> Custom { get; set; }
    }
    public class GetRsvpTemplatesResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<CustomFormTemplate> CustomForms { get; set; } = new List<CustomFormTemplate>();
    }
    public class CustomFormTemplate
    {
        public string FormTitle { get; set; }
        public int TotalFields { get; set; }
    }
    public class GetRsvpTemplateFieldsResp
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<TemplateField> Fields { get; set; } = new List<TemplateField>();
    }
    public class TemplateField
    {
        public int FieldId { get; set; }
        public string FieldType { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }

    public class PublishEventReq
    {
        public int UserId { get; set; }
        public string hashValue { get; set; } = string.Empty;
        public int EventId { get; set; }
        public bool IsPublish { get; set; }
    }
    public class EventSummaryResp
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string EventCategory { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public string TotalPotential { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public int TotalTicketTiers { get; set; }
        public int Capacity { get; set; }
        public int Halls { get; set; }
        public string FoodServing { get; set; } = string.Empty;
        public int TotalCustomFields { get; set; }
        public int TotalAddons { get; set; }
        public int TotalPromoCodes { get; set; }
        public string Engagement { get; set; } = string.Empty;
        public string Refund { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public string Commission { get; set; } = string.Empty;
        public string PublishingFee { get; set; } = string.Empty;
        public string GuestCharges { get; set; } = string.Empty;
        public string DueAmount { get; set; } = string.Empty;
    }
}
