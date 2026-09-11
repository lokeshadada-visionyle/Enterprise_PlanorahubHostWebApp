using Microsoft.AspNetCore.Identity.Data;
using Planora_EnterproseHostWebApp.Models;
using Planora_EnterproseHostWebApp.Pages.Dashboard;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using LoginRequest = Planora_EnterproseHostWebApp.Models.LoginRequest;

namespace Planora_EnterproseHostWebApp
{
    public class CommonHelper
    {
        string SERVICE_URL = "https://jobnyle.azurewebsites.net/WorkNyleAPI/EventMGUnifiedAPI.svc";

        #region Helper_Methods
        public string GetSHA265(string text)
        {
            byte[] hashValue;
            byte[] message = System.Text.Encoding.UTF8.GetBytes(text);

            string hex = "";

            SHA256 hashString = new SHA256CryptoServiceProvider();
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private T _download_serialized_json_data<T>(string url) where T : new()
        {
            try
            {
                using (var w = new WebClient())
                {
                    var json_data = string.Empty;

                    w.Headers[HttpRequestHeader.ContentType] = "application/json";

                    w.Headers.Add("VenderId", "Event_Version_0.0001");
                    w.Headers.Add("VenderPwd", "afgftFrgjhiyRHThhfiohihdipdoioxdioxFFHFYhihppojfstrssfyhiiihgvchoodfjkuhkgx");

                    json_data = w.DownloadString(url);

                    Debug.WriteLine("Response: " + json_data);

                    return !string.IsNullOrEmpty(json_data) ? JsonSerializer.Deserialize<T>(json_data, _jsonOptions) : new T();
                }
            }
            catch (WebException wex)
            {
                var statusCode = "unknown";
                var body = "(no response body)";

                if (wex.Response is HttpWebResponse httpResponse)
                {
                    statusCode = ((int)httpResponse.StatusCode) + " " + httpResponse.StatusCode;

                    using (var stream = httpResponse.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        body = reader.ReadToEnd();
                    }
                }

                Debug.WriteLine("WebException calling " + url);
                Debug.WriteLine("Status: " + statusCode);
                Debug.WriteLine("Body: " + body);

                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unexpected error calling " + url + ": " + ex.Message);
                throw;
            }
        }

        private T _serialized_json_data<T>(string url, string indata) where T : new()
        {
            try
            {
                using (var w = new WebClient())
                {
                    var json_data = string.Empty;

                    w.Headers[HttpRequestHeader.ContentType] = "application/json";

                    // Add headers
                    w.Headers.Add("VenderId", "Event_Version_0.0001");
                    w.Headers.Add("VenderPwd", "afgftFrgjhiyRHThhfiohihdipdoioxdioxFFHFYhihppojfstrssfyhiiihgvchoodfjkuhkgx");

                    json_data = w.UploadString(url, "POST", indata);

                    return !string.IsNullOrEmpty(json_data) ? JsonSerializer.Deserialize<T>(json_data, _jsonOptions) : new T();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string BuildQuery(params (string key, string value)[] pairs)
        {
            var parts = new System.Collections.Generic.List<string>();
            foreach (var p in pairs)
                parts.Add($"{p.key}={System.Uri.EscapeDataString(p.value)}");
            return string.Join("&", parts);
        }

        #endregion

        #region Login

        public LoginResponse Login(string email, string password)
        {
            string hashValue = GetSHA265(email);
            var request = new LoginRequest
            {
                Email = email,
                Password = password,
                hashValue = hashValue,
            };
            string url = SERVICE_URL + "/Ent_EnterpriseLogin";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<LoginResponse>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        #endregion

        #region CretaeEvent
        public GetEventTypeResp EventType(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEventTypeSettings?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetEventTypeResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public GetEventCategoryResp GetEventCategory(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/GetEventCategory?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetEventCategoryResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response AddEventDetails(AddBasicDetailsReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddBasicDetailsReq
            {
                UserId = req.UserId,
                description = req.description,
                EventCategory = req.EventCategory,
                EventName = req.EventName,
                EventType = req.EventType,
                hashValue = hashValue,
                TagLine = req.TagLine,
                TimeZone = req.TimeZone,
                CityId = req.CityId,
                FileName = req.FileName,
                ImageBase64 =req.ImageBase64
            };
            string url = SERVICE_URL + "/Ent_AddEvent";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventDetails GetEventDetailsById(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            var request = new 
            {
                UserId = userId,
                EventId = eventId,
                hashValue = hashValue,
            };
            string url = SERVICE_URL + "/Ent_GetEventById";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<EventDetails>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response UpdateEventDetails(UpdateEventDetailsRequest req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new UpdateEventDetailsRequest
            {
                UserId = req.UserId,
                description = req.description,
                EventCategory = req.EventCategory,
                EventName = req.EventName,
                EventType = req.EventType,
                hashValue = hashValue,
                TagLine = req.TagLine,
                TimeZone = req.TimeZone,
                CityId = req.CityId,
                EventId = req.EventId,
                ImageUrl = req.ImageUrl,
            };
            string url = SERVICE_URL + "/Ent_UpdateEvent";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public AddThingToKnowResp AddThingsToKnow(AddThingToKnowReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddThingToKnowReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                ImageURL = req.ImageURL,
                Things = req.Things,
            };
            string url = SERVICE_URL + "/Ent_AddThingsToKnow";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<AddThingToKnowResp>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public AddThingToKnowResp UpdateThingsToKnow(AddThingToKnowReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new UpdateThingToKnowReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                ImageURL = req.ImageURL,
                Things = req.Things,
            };
            string url = SERVICE_URL + "/HostUpdateThingsToKnow";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<AddThingToKnowResp>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public ThingsToKnowResponse GetEventThinksToKnowById(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/HostGetThingsToKnowByEventId?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<ThingsToKnowResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public UploadCoverImageResp UploadCoverImage(UploadCoverImageReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new UploadCoverImageReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                FileName = req.FileName,
                ImageBase64 = req.ImageBase64
            };
            string url = SERVICE_URL + "/UploadCoverImage";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<UploadCoverImageResp>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetCitiesResp GetCityResp(long userId,string search)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EvenSearchtId", search),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/GetCities?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetCitiesResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public Response AddVenueAndHall(AddVenueAndHallReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddVenueAndHallReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                Venues = req.Venues,
            };
            string url = SERVICE_URL + "/Ent_AddVenueAndHall";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response AddEventDateAndSlots(AddEventDateSlotsReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddEventDateSlotsReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventDates = req.EventDates,
                EventId = req.EventId,
            };
            string url = SERVICE_URL + "/Ent_AddEventDateAndSlots";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetVenueAndHallResp GetVenueHallsResp(GenVenueHallReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new GenVenueHallReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
            };
            string url = SERVICE_URL + "/Ent_GetVenueAndHall";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<GetVenueAndHallResp>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetHallByVenueIdResp GetVenueHallsBYIdResp(long userId,int eventId, int venueId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("VenueId", venueId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetHallByVenueId?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetHallByVenueIdResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response AddSpeaker(AddSpeakerReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddSpeakerReq
            {
                UserId = req.UserId,
                EventId = req.EventId,
                hashValue = hashValue,
                Ent_Speaker = req.Ent_Speaker,
            };
            string url = SERVICE_URL + "/Ent_AddSpeaker";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public SpeakerResponseModel GetSpearkersResp(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_Speakers?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<SpeakerResponseModel>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public AddEventPolicyResp AddEventPolicyReq(AddEventPolicyReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddEventPolicyReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                IsPollsEnabled = req.IsPollsEnabled,
                CanGuestAttactPhoto = req.CanGuestAttactPhoto,
                IsFeedbackEnabled = req.IsFeedbackEnabled,
                Description = req.Description,
                FAQs = req.FAQs,
            };
            string url = SERVICE_URL + "/AddEventPolicy";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<AddEventPolicyResp>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventFAQResponse GetEventFaqResp(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/HostGetEventPolicy?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventFAQResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public UpdateEventFAQRequest UpdateEventPolicyReq(UpdateEventFAQRequest req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new UpdateEventFAQRequest
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                IsPollsEnabled = req.IsPollsEnabled,
                CanGuestAttactPhoto = req.CanGuestAttactPhoto,
                IsFeedbackEnabled = req.IsFeedbackEnabled,
                Description = req.Description,
                FAQs = req.FAQs,
            };
            string url = SERVICE_URL + "/HostUpdateEventPolicy";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<UpdateEventFAQRequest>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public GetEventCurrencyResp GetEventCurrency(long userId,int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEventCurrency?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetEventCurrencyResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response AddTicketTypeTierReq(CreateTicketTypeRequest req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new CreateTicketTypeRequest
            {
                UserId = req.UserId,
                EventId = req.EventId,
                hashValue = hashValue,
                TicketTypes = req.TicketTypes,
                
            };
            string url = SERVICE_URL + "/Ent_AddTicketTypeTiers";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public UploadMembersListResp UploadMembersList(long userId, int eventId, string fileName, string fileBase64)
        {
            string hashValue = GetSHA265(userId.ToString());
            var request = new UploadMembersListReq
            {
                UserId = userId,
                EventId = eventId,
                hashValue = hashValue,
                FileName = fileName,
                FileBase64 = fileBase64
            };
            string url = SERVICE_URL + "/Ent_UploadMembersList";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<UploadMembersListResp>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventDateAndSlotsResp GetEventDateAndSlots(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEventDateAndSlots?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventDateAndSlotsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public Response AddAddons(AddAddonsReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddAddonsReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                AddOns = req.AddOns,
            };
            string url = SERVICE_URL + "/Ent_AddEventAddOn";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public AddEventPolicyResp AddCouponsReq(AddPromoCodeReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddPromoCodeReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                PromoCodes = req.PromoCodes,
            };
            string url = SERVICE_URL + "/Ent_AddPromoCode";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<AddEventPolicyResp>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetDiscountExposureResp GetDiscountExposure(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetDiscountExposure?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetDiscountExposureResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public TicketTypeResp GetTicketType(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/ZoneDropdown?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<TicketTypeResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public Response AddFoodDeatils(AddFoodDetailsReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddFoodDetailsReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                IsFoodEnabled = req.IsFoodEnabled,
                RequireScan = false,
                ServingType = req.ServingType,
            };
            string url = SERVICE_URL + "/Ent_AddFoodDetails";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response SaveFoodAvaliablity(SaveFoodAvailabilityReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new SaveFoodAvailabilityReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                AvailabilityScope = req.AvailabilityScope,
                EventDates = req.EventDates,
            };
            string url = SERVICE_URL + "/Ent_SaveFoodAvailability";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response AddMenuItems(AddMenuItemReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddMenuItemReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                MenuItems = req.MenuItems,
            };
            string url = SERVICE_URL + "/Ent_AddMenuItem";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response UploadFoodMenu(UploadMenuFilePOST req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new UploadMenuFilePOST
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                FileName = req.FileName,
                FileBase64 = req.FileBase64,
            };
            string url = SERVICE_URL + "/UploadMenuFile";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetMenuDetailsResponse GetMenuDetails(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetMenuItems?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetMenuDetailsResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetFoodCategoryResp GetFoodMenuCategory(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetFoodCategories?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetFoodCategoryResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public Response AddRegistrationFormReq(AddEventFieldsReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());

            // Populate SortOrder and fallback defaults for each field
            if (req.Fields != null)
            {
                for (int i = 0; i < req.Fields.Count; i++)
                {
                    req.Fields[i].SortOrder = i + 1;
                    req.Fields[i].Placeholder ??= string.Empty;
                    req.Fields[i].Options ??= string.Empty;
                }
            }

            var request = new AddEventFieldsReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                FormTitle = string.IsNullOrWhiteSpace(req.FormTitle) ? "Custom Registration Form" : req.FormTitle,
                Fields = req.Fields
            };

            string url = SERVICE_URL + "/Ent_CreateCustomForm";
            string jsonData = JsonSerializer.Serialize(request);

            Debug.WriteLine("URL: " + url);
            Debug.WriteLine("Payload: " + jsonData);

            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));

            return response;
        }
        public Response AddAccessRegistration(EventAccessModel req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new EventAccessModel
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                AccessGatewayMode = req.AccessGatewayMode,
                IsUnListed = req.IsUnListed,
            };
            string url = SERVICE_URL + "/Ent_SetAccessGateway";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetDefaultRegistrationFormsResp GetDefaultRegistrationForms(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                //("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetDefaultRegistrationForms?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetDefaultRegistrationFormsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetFormFieldsResp GetDefaultRegistrationFormsById(long userId, int eventId, int formId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("FormId", formId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetDefaultRegistrationFormsById?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetFormFieldsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetRegistrationFormResp GetRegistrationForm(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetRegistrationForm?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetRegistrationFormResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetRsvpTemplatesResp GetRsvpDefaultCustomForms(int userId,int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetRsvpDefaultCustomForms?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetRsvpTemplatesResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetRsvpTemplateFieldsResp GetDefaultRsvpFormsByTitle(int userId,int eventId, string formTitle)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("FormTitle", formTitle.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetDefaultRsvpFormsByTitle?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetRsvpTemplateFieldsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response AddLadndingPage(AddLandingPageReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddLandingPageReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                AccentColour = req.AccentColour,
                BackGroundColour = req.BackGroundColour,
                Description = req.Description,
                Link = req.Link,
                Logo = req.Logo,
                PrimaryColour = req.PrimaryColour,
            };
            string url = SERVICE_URL + "/Ent_AddLandingPage";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public UploadImageResp UploadBrandingImage(FileUploadModel req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new FileUploadModel
            {
                UserId = req.UserId,
                hashValue = hashValue,
                FileName = req.FileName,
                ImageBase64 = req.ImageBase64
            };
            string url = SERVICE_URL + "/Ent_UploadLogo";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<UploadImageResp>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventPageDetailsResp EventLandingPageDetailsResp(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_EventDetailsForBrandingPage?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventPageDetailsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public Response AddRsvpCustoms(AddRsvpCustomsReq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new AddRsvpCustomsReq
            {
                UserId = req.UserId,
                hashValue = hashValue,
                EventId = req.EventId,
                Custom = req.Custom,
                IsRsvpEnabled = req.IsRsvpEnabled,
            };
            string url = SERVICE_URL + "/AddRsvpCustoms";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public GetRsvpCustomsResp GetRsvpCustoms(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/HostGetRsvpCustoms?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GetRsvpCustomsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public EventSummaryResp GetEventDetailsSummary(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEventDetails?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventSummaryResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response PublishEvent(int userId, int eventId, bool isPublish)
        {
            string hashValue = GetSHA265(userId.ToString());

            var request = new PublishEventReq
            {
                UserId = userId,
                hashValue = hashValue,
                EventId = eventId,
                IsPublish = isPublish
            };

            string url = SERVICE_URL + "/Ent_PublishEvent";
            string jsonData = JsonSerializer.Serialize(request);

            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);

            var response = _serialized_json_data<Response>(url, jsonData);

            Debug.WriteLine(JsonSerializer.Serialize(response));

            return response;
        }
        #endregion

        #region HostEvents
        public HostDashboardResp GetHostDashboard(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_EventDashboard?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<HostDashboardResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public HostDashboardWeelkyResp GetHostDashboardWeekly(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_WeeklyRevenue?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<HostDashboardWeelkyResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public LiveEventsResp GetLiveEvent(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_LiveEvents?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<LiveEventsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public MyEventResponse GetMyEvents(long userId, int pageNo = 1, int pageSize = 10,string status = "All",string search="")
        {
            string hashValue = GetSHA265(userId.ToString());    
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("PageNo", pageNo.ToString()),
                ("PageSize", pageSize.ToString()),
                ("Search", search),
                ("Status", status.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetMyEvents?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<MyEventResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        //public EventsHubResp GetHostDashboard(long userId,int pageNo,int pageSize,string status,string search)
        //{
        //    string hashValue = GetSHA265(userId.ToString());
        //    string query = BuildQuery(
        //        ("UserId", userId.ToString()),
        //        ("PageNo", pageNo.ToString()),
        //        ("PageSize", pageSize.ToString()),
        //        ("Status", status),
        //        ("Search", search),
        //        ("hashValue", hashValue));
        //    string url = SERVICE_URL + "/Ent_GetMyEventsHub?" + query;
        //    Debug.WriteLine(url);
        //    var response = _download_serialized_json_data<EventsHubResp>(url);
        //    Debug.WriteLine(JsonSerializer.Serialize(response));
        //    return response;
        //}
        public EventHubOverviewResp GetEventHubOverView(long userId, int eventId,int soltId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", soltId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEventOverview?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventHubOverviewResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubDatesAndSlotsResp GetEventHubDateAndSlot(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEventDatesAndSlotsDash?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventHubDatesAndSlotsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubGuestResp GetEventHubGuestResp(long userId, int eventId, int sloId,int ticketTypeId = 0,bool isCheckIn = false)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", sloId.ToString()),
                ("TicketTypeId", ticketTypeId.ToString()),
                ("isCheckIn", isCheckIn.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEventGuests?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventHubGuestResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventTicketTypeResp GetEventHubGuestTicketTypeResp(long userId, int eventId, int sloId, int ticketTypeId = 0, bool isCheckIn = false)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", sloId.ToString()),
                ("TicketTypeId", ticketTypeId.ToString()),
                ("isCheckIn", isCheckIn.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetTicketTypesBySlotId?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventTicketTypeResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubGuestRegistrationResp GetEventHubGuestRegistrationResp(long userId, int eventId, int sloId, int ticketTypeId = 0, bool isComplete = false,string search = "")
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", sloId.ToString()),
                ("TicketTypeId", ticketTypeId.ToString()),
                ("IsComplete", isComplete.ToString()),
                ("Search", search.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEventRegistrations?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventHubGuestRegistrationResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubRsvpGuestResp GetEventHubGuestRsvpResp(long userId, int eventId, int sloId, int ticketTypeId = 0, string search = "")
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", sloId.ToString()),
                ("TicketTypeId", ticketTypeId.ToString()),
                ("Search", search.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_RsvpGuestList?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventHubRsvpGuestResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubRsvpResponseResp GetEventHubGuestRsvpResponseResp(long userId, int eventId, int sloId,string search = "", string resp = "")
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", sloId.ToString()),
                ("Response", resp),
                ("Search", search.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_RsvpGuestResponsesList?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventHubRsvpResponseResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubSeatStatusResponse GetEventHubSeatingResp(long userId, int eventId, int sloId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", sloId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEventSeating?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventHubSeatStatusResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public SeatResponseModel GetEventHubSeatinLayoutResp(long userId, int eventId, int sloId,int ticketTypeId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", sloId.ToString()),
                ("TicketTypeId", ticketTypeId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_ViewSeatingLayout?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<SeatResponseModel>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubFoodMenuResponse GetEventHubFoodMenuResp(long userId, int eventId, int sloId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", sloId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_ViewSeatingLayout?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventHubFoodMenuResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response UpdateReadyToOrder(EventHubFoodReadyToOrderreq req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());
            var request = new EventHubFoodReadyToOrderreq
            {
                UserId = req.UserId,
                EventId = req.EventId,
                hashValue = hashValue,
                SLotId = req.SLotId,
                Ready = req.Ready
            };
            string url = SERVICE_URL + "/Ent_ReadyToOrder";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubServingLogResponse GetEventHubServingListResp(long userId, int eventId, int sloId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", sloId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetServingList?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventHubServingLogResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubVendorResponse GetVendorsAndStaffListResp(long userId, int eventId, int slotId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("hashValue", hashValue));

            string url = SERVICE_URL + "/Ent_GetVendorsAndStaff?" + query;
            Debug.WriteLine(url);

            var response = _download_serialized_json_data<EventHubVendorResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response RegenerateVendorPin(long userId, int eventId, int vendorId)
        {
            string hashValue = GetSHA265(userId.ToString());

            var payload = new
            {
                UserId = userId,
                hashValue = hashValue,
                EventId = eventId,
                VendorId = vendorId
            };

            string url = SERVICE_URL + "/Ent_RegenerateVendorPin";
            Debug.WriteLine(url);
            string jsonData = JsonSerializer.Serialize(payload);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventResponse GetEventListResp(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));

            string url = SERVICE_URL + "/Ent_GetEvent?" + query;
            Debug.WriteLine(url);

            var response = _download_serialized_json_data<EventResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response AddVendorSatff(AddVendorRequest req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());

            var payload = new AddVendorRequest
            {
                UserId = req.UserId,
                EventId = req.EventId,
                hashValue = hashValue,
                Name = req.Name,
                PhoneNo = req.PhoneNo,
                Role = req.Role,
                VendorId = req.VendorId,
                Pin = req.Pin,
                TicketTypeId = req.TicketTypeId,
                WholeEvent = req.WholeEvent,
                SlotIds = req.SlotIds ?? new List<int>()
            };

            string url = SERVICE_URL + "/Ent_AddVendorOrStaff";
            string jsonData = JsonSerializer.Serialize(payload);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response UpdateVendor(EditVendorRequest req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());

            var payload = new EditVendorRequest
            {
                UserId = req.UserId,
                EventId = req.EventId,
                hashValue = hashValue,
                VendorId = req.VendorId,
                VendorName = req.VendorName,
                PhoneNo = req.PhoneNo,
                Pin = req.Pin,
                WholeEvent = req.WholeEvent,
                SlotIds = req.SlotIds ?? new List<int>()
            };

            string url = SERVICE_URL + "/Ent_EditVendor";
            string jsonData = JsonSerializer.Serialize(payload);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response RemoveVendor(long userId, int eventId, int vendorId)
        {
            string hashValue = GetSHA265(userId.ToString());

            var payload = new
            {
                UserId = userId,
                hashValue = hashValue,
                EventId = eventId,
                VendorId = vendorId
            };

            string url = SERVICE_URL + "/Ent_RemoveVendor";
            string jsonData = JsonSerializer.Serialize(payload);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubPollResponse GetEventHubPollResp(long userId, int eventId, int slotId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("hashValue", hashValue));

            string url = SERVICE_URL + "/Ent_GetPolls?" + query;
            Debug.WriteLine(url);

            var response = _download_serialized_json_data<EventHubPollResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public Response AddPollsRequest(AddPollRequest req)
        {
            string hashValue = GetSHA265(req.UserId.ToString());

            var payload = new AddPollRequest
            {
                UserId = req.UserId,
                EventId = req.EventId,
                hashValue = hashValue,
                Options = req.Options,
                Question = req.Question,
                SlotId = req.SlotId,
                VisibleTo = req.VisibleTo,
            };

            string url = SERVICE_URL + "/Ent_AddPoll";
            string jsonData = JsonSerializer.Serialize(payload);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubBroadcastResponse GetEventHubBroadCastResp(long userId, int eventId, int slotId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("hashValue", hashValue));

            string url = SERVICE_URL + "/Ent_GetBroadcastsHistory?" + query;
            Debug.WriteLine(url);

            var response = _download_serialized_json_data<EventHubBroadcastResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubSalesResponse GetEventHubSalesResp(long userId, int eventId,int slotId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("hashValue", hashValue));

            string url = SERVICE_URL + "/Ent_GetTotalSales?" + query;
            Debug.WriteLine(url);

            var response = _download_serialized_json_data<EventHubSalesResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubInsightResponse GetEventHubInsightsResp(long userId, int eventId, int slotId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("hashValue", hashValue));

            string url = SERVICE_URL + "/Ent_GetEventInsights?" + query;
            Debug.WriteLine(url);

            var response = _download_serialized_json_data<EventHubInsightResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubPollAnalyticsResponse GetEventHubSalesSummaryResp(long userId, int eventId,int slotId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("hashValue", hashValue));

            string url = SERVICE_URL + "/Ent_GetPollsSummary?" + query;
            Debug.WriteLine(url);

            var response = _download_serialized_json_data<EventHubPollAnalyticsResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        public EventHubAddonReportResponse GetEventHubAddonsPromoResp(long userId, int eventId, int slotId,string search = "")
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("Search",search.ToString()),
                ("hashValue", hashValue));

            string url = SERVICE_URL + "/Ent_GetAddonsDashboard?" + query;
            Debug.WriteLine(url);

            var response = _download_serialized_json_data<EventHubAddonReportResponse>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        #endregion

        #region SalesAndRevenueEvents
        public SalesAndRevenueResp GetSalesAndRevenue(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetSalesAndRevenue?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<SalesAndRevenueResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public SalesAndRevenueByEventResp GetSalesAndRevenueByEvent(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetSalesAndRevenueByEvent?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<SalesAndRevenueByEventResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public SalesAndRevenueByDaysResp GetSalesAndRevenueByDays(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetSalesAndRevenueByDays?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<SalesAndRevenueByDaysResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public EventRefundsResp GetEventRefunds(long userId, int eventId)
        {
            string hashValue = GetSHA265(userId.ToString());
            var request = new EventRefundsReq
            {
                UserId = userId,
                EventId = eventId,
                hashValue = hashValue,
            };
            string url = SERVICE_URL + "/Ent_GetEventRefunds";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<EventRefundsResp>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        #endregion

        #region profile/settings
        public EnterpriseResp GetEnterprise(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEnterprise?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EnterpriseResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public Response UpdateEnterprise(long userId, string userName, string companyName,
    string joinedOn, string email, string phoneNo, string website, string settlementCurrency)
        {
            string hashValue = GetSHA265(userId.ToString());
            var request = new UpdateEnterpriseReq
            {
                UserId = userId,
                hashValue = hashValue,
                UserName = userName,
                CompanyName = companyName,
                JoinedOn = joinedOn,
                Email = email,
                PhoneNo = phoneNo,
                Website = website,
                SettlementCurrency = settlementCurrency,
            };
            string url = SERVICE_URL + "/Ent_UpdateEnterprise";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        #endregion

        #region MyPlan
        public EnterprisePlanDashResp GetEnterprisePlanDash(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEnterprisePlanDash?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EnterprisePlanDashResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public EnterprisePackageDetailsResp GetEnterprisePackageDetails(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEnterprisePackageDetails?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EnterprisePackageDetailsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public EnterpriseChannelCreditsDashResp GetEnterpriseChannelCreditsDash(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEnterpriseChannelCreditsDash?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EnterpriseChannelCreditsDashResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public EnterpriseBillingResp GetEnterpriseBilling(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEnterpriseBilling?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EnterpriseBillingResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public EnterpriseAuditReportResp GetEnterpriseAuditReport(long userId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetEnterpriseAuditReport?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EnterpriseAuditReportResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        #endregion

        #region checckin
        public GuestCheckInsDashboardResp GetGuestCheckInsDashboard(long userId, int eventId, int slotId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_GetGuestCheckInsDashboard?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<GuestCheckInsDashboardResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public Response ManualCheckIn(long userId, int eventId, int slotId, string reference)
        {
            string hashValue = GetSHA265(userId.ToString());
            var request = new ManualCheckInReq
            {
                UserId = userId,
                hashValue = hashValue,
                EventId = eventId,
                SlotId = slotId,
                Reference = reference,
            };
            string url = SERVICE_URL + "/Ent_ManualCheckIn";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public ManualCheckInGuestsResp GetManualCheckInGuests(long userId, int eventId, int slotId, int eventDateId, int ticketTypeId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("EventDateId", eventDateId.ToString()),
                ("TicketTypeId", ticketTypeId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_ManualCheckInGuests?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<ManualCheckInGuestsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public Response CheckInGuest(long userId, int eventId, int slotId, int guestId)
        {
            string hashValue = GetSHA265(userId.ToString());
            var request = new CheckInGuestsReq
            {
                UserId = userId,
                hashValue = hashValue,
                EventId = eventId,
                SlotId = slotId,
                GuestId = guestId,
            };
            string url = SERVICE_URL + "/Ent_CheckInGuests";
            string jsonData = JsonSerializer.Serialize(request);
            Debug.WriteLine(url);
            Debug.WriteLine(jsonData);
            var response = _serialized_json_data<Response>(url, jsonData);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public SeatingLayoutResp GetSeatingLayout(long userId, int eventId, int slotId, int eventDateId, int ticketTypeId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("EventDateId", eventDateId.ToString()),
                ("TicketTypeId", ticketTypeId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_SeatingLayout?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<SeatingLayoutResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }

        public EventScanLogsResp GetEventScanLogs(long userId, int eventId, int slotId)
        {
            string hashValue = GetSHA265(userId.ToString());
            string query = BuildQuery(
                ("UserId", userId.ToString()),
                ("EventId", eventId.ToString()),
                ("SlotId", slotId.ToString()),
                ("hashValue", hashValue));
            string url = SERVICE_URL + "/Ent_EventScanLogs?" + query;
            Debug.WriteLine(url);
            var response = _download_serialized_json_data<EventScanLogsResp>(url);
            Debug.WriteLine(JsonSerializer.Serialize(response));
            return response;
        }
        #endregion
    }
}
