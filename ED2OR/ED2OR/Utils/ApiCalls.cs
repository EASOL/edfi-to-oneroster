using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ED2OR.ViewModels;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using ED2OR.Enums;
using ED2OR.Models;
using Microsoft.AspNet.Identity;

namespace ED2OR.Utils
{
    public class ApiCalls
    {
        private static readonly ApplicationDbContext db = new ApplicationDbContext();

        private static string UserId
        {
            get
            {
                var context = HttpContext.Current;
                return context.User.Identity.GetUserId();
            }
        }

        //public async Task<ApiCallViewModel> GetToken()
        public static TokenViewModel GetToken(bool forceNewToken = false)
        {
            if (forceNewToken || HttpContext.Current.Session["token"] == null)
            {
                var user = db.Users.FirstOrDefault(x => x.Id == UserId);
                HttpContext.Current.Session["token"] = GetToken(user.ApiBaseUrl, user.ApiKey, user.ApiSecret);
            }

            return (TokenViewModel)HttpContext.Current.Session["token"];
        }

        public static TokenViewModel GetToken(string apiBaseUrl, string apiKey, string apiSecret)
        {
            try
            {
                using (var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
                {
                    var authorizeResult = client.PostAsync(ApiEndPoints.OauthAuthorize,
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string,string>("Client_id",apiKey),
                        new KeyValuePair<string,string>("Response_type","code")
                    })).Result;
                    var codeJson = authorizeResult.Content.ReadAsStringAsync().Result;

                    if (!authorizeResult.IsSuccessStatusCode)
                    {
                        return GetApiCallViewModel(false, "Request returned \"" + authorizeResult.ReasonPhrase + "\"", "");
                    }

                    if (JObject.Parse(codeJson)["error"] != null)
                    {
                        return GetApiCallViewModel(false, JObject.Parse(codeJson)["error"].ToString(), "");
                    }

                    var code = JObject.Parse(codeJson)["code"].ToString();

                    var tokenRequestResult = client.PostAsync(ApiEndPoints.OauthGetToken,
                        new FormUrlEncodedContent(new[]
                        {
                    new KeyValuePair<string,string>("Client_id",apiKey),
                    new KeyValuePair<string,string>("Client_secret",apiSecret),
                    new KeyValuePair<string,string>("Code",code),
                    new KeyValuePair<string,string>("Grant_type","authorization_code")
                        })).Result;
                    var tokenJson = tokenRequestResult.Content.ReadAsStringAsync().Result;

                    if (JObject.Parse(tokenJson)["error"] != null)
                    {
                        return GetApiCallViewModel(false, JObject.Parse(tokenJson)["error"].ToString(), "");
                    }

                    return GetApiCallViewModel(true, "", JObject.Parse(tokenJson)["access_token"].ToString());
                }
            }
            catch (AggregateException ex)
            {
                return GetApiCallViewModel(false, ex.InnerException.InnerException.Message, "");
            }
            catch (UriFormatException ex)
            {
                return GetApiCallViewModel(false, "Invalid Url", "");
            }
            catch (Exception ex)
            {
                return GetApiCallViewModel(false, ex.Message, "");
            }
        }

        public static async Task<List<SchoolViewModel>> GetSchools()
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.Schools);
            var schools = (from s in responseArray
                           select new SchoolViewModel
                            {
                                id = (string)s["id"],
                                schoolId = (string)s["schoolId"],
                                nameOfInstitution = (string)s["nameOfInstitution"]
                            }).ToList();
            return schools;
        }

        public static async Task<List<CsvOrgs>> GetCsvOrgs()
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvOrgs);
            var listOfObjects = (from o in responseArray
                        select new CsvOrgs
                        {
                            sourcedId = (string)o["id"],
                            status ="active",
                            name = (string)o["nameOfInstitution"],
                            type = "school",
                            identifier = ""
                            //parentSourcedId = (string)o["xxxxxxx"]  //doesnt exist in API yet
                        }).ToList();
            return listOfObjects;
        }

        public static async Task<List<CsvOrgs>> GetCsvUsers()
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvOrgs);
            var listOfObjects = (from o in responseArray
                        select new CsvOrgs
                        {
                            sourcedId = (string)o["id"],
                        }).ToList();
            return listOfObjects;
        }

        public static async Task<List<CsvCourses>> GetCsvCourses()
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvOrgs);
            var listOfObjects = (from o in responseArray
                                 select new CsvCourses
                                 {
                                     sourcedId = (string)o["id"],
                                 }).ToList();
            return listOfObjects;
        }

        public static async Task<List<CsvClasses>> GetCsvClasses()
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvOrgs);
            var listOfObjects = (from o in responseArray
                                 select new CsvClasses
                                 {
                                     sourcedId = (string)o["id"],
                                 }).ToList();
            return listOfObjects;
        }

        public static async Task<List<CsvEnrollments>> GetCsvEnrollments()
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvOrgs);
            var listOfObjects = (from o in responseArray
                                 select new CsvEnrollments
                                 {
                                     sourcedId = (string)o["id"],
                                 }).ToList();
            return listOfObjects;
        }

        public static async Task<List<CsvAcademicSessions>> GetCsvAcademicSessions()
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvOrgs);
            var listOfObjects = (from o in responseArray
                                 select new CsvAcademicSessions
                                 {
                                     sourcedId = (string)o["id"],
                                 }).ToList();
            return listOfObjects;
        }

        private static async Task<JArray> GetApiResponseArray(string apiEndpoint)
        {
            var response = await GetApiResponse(apiEndpoint);
            if (response.TokenExpired)
            {
                GetToken(true);
                response = await GetApiResponse(apiEndpoint);
            }
            return response.ResponseArray;
        }

        private static async Task<ApiResponse> GetApiResponse(string apiEndpoint)
        {
            var tokenModel = GetToken();
            if (!tokenModel.IsSuccessful)
            {
                return null;
            }

            var token = tokenModel.Token;
            var apiBaseUrl = db.Users.FirstOrDefault(x => x.Id == UserId).ApiBaseUrl;

            using (var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
            {
                client.DefaultRequestHeaders.Authorization =
                       new AuthenticationHeaderValue("Bearer", token);

                var apiResponse = await client.GetAsync(ApiEndPoints.ApiPrefix + apiEndpoint);

                if (apiResponse.IsSuccessStatusCode == false && apiResponse.ReasonPhrase == "Invalid token")
                {
                    return new ApiResponse
                    {
                        TokenExpired = true,
                        ResponseArray = null
                    };
                }

                var responseJson = await apiResponse.Content.ReadAsStringAsync();
                var responseArray = JArray.Parse(responseJson);

                return new ApiResponse
                {
                    TokenExpired = false,
                    ResponseArray = responseArray
                };
            }
        }

        public void GetFileResult(int templateId)
        {

        }

        private static TokenViewModel GetApiCallViewModel(bool isSuccessful, string errorMsg, string token)
        {
            return new TokenViewModel
            {
                IsSuccessful = isSuccessful,
                ErrorMessage = errorMsg,
                Token = token
            };
        }
    }
}