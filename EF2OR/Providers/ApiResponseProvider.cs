using EF2OR.Enums;
using EF2OR.Models;
using EF2OR.Utils;
using EF2OR.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EF2OR.Providers
{
    public class ApiResponseProvider : IApiResponseProvider
    {
        internal static ApplicationDbContext db = new ApplicationDbContext();

        public ApiResponseProvider ()
        {
        }

        public async Task<JArray> GetApiData(string apiEndpoint, bool forceNew = false, string fields = null)
        {
            if (CommonUtils.ExistingResponses.ContainsKey(apiEndpoint) && !forceNew)
            {
                return CommonUtils.ExistingResponses[apiEndpoint];
            }

            var response = await GetApiResponse(apiEndpoint, fields);
            if (response.TokenExpired)
            {
                Providers.ApiResponseProvider.GetToken(true);
                response = await GetApiResponse(apiEndpoint, fields);
            }

            if (CommonUtils.ExistingResponses.ContainsKey(apiEndpoint))
            {
                CommonUtils.ExistingResponses.Remove(apiEndpoint);
            }

            CommonUtils.ExistingResponses.Add(apiEndpoint, response.ResponseArray);

            return response.ResponseArray;
        }

        public async Task<JArray> GetPagedApiData(string apiEndpoint, int offset, string fields = null)
        {
            var response = await GetPagedApiResponse(apiEndpoint, fields, offset);
            if (response.TokenExpired)
            {
                Providers.ApiResponseProvider.GetToken(true);
                response = await GetPagedApiResponse(apiEndpoint, fields, offset);
            }

            return response.ResponseArray;
        }

        public static TokenViewModel GetToken(bool forceNewToken = false)
        {
            if (forceNewToken || CommonUtils.HttpContextProvider.Current.Session["token"] == null || ((TokenViewModel)HttpContext.Current.Session["token"]).IsSuccessful == false)
            {
                var context = new ApplicationDbContext();
                var apiBaseUrl = context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiBaseUrl)?.SettingValue;
                var apiKey = context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiKey)?.SettingValue;
                var apiSecret = context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiSecret)?.SettingValue;
                CommonUtils.HttpContextProvider.Current.Session["token"] = GetToken(apiBaseUrl, apiKey, apiSecret);
                context.Dispose();
            }

            return (TokenViewModel)CommonUtils.HttpContextProvider.Current.Session["token"];
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

        private static string GetApiPrefix()
        {
            using (var context = new ApplicationDbContext())
            {
                return context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiPrefix)?.SettingValue;
            }
        }

        public async Task<ApiResponse> GetPagedApiResponse(string apiEndpoint, string fields, int offset)
        {
            var context = new ApplicationDbContext();
            var apiBaseUrl = context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiBaseUrl)?.SettingValue;

            var maxRecordLimit = 100;
            var fullUrl = GetApiPrefix() + apiEndpoint + "?limit=" + maxRecordLimit;
            if (!string.IsNullOrEmpty(fields))
            {
                fullUrl += "&fields=" + fields;
            }

            var tokenModel = GetToken();
            if (!tokenModel.IsSuccessful)
            {
                var ex = new EF2ORCustomException("There was a problem connecting to the API.  Make sure the connection can test properly in the Settings page.");
                throw ex;
            }

            var token = tokenModel.Token;

            return await GetPagedJArray(token, fullUrl, apiBaseUrl, offset);
        }

        public async Task<ApiResponse> GetApiResponse(string apiEndpoint, string fields)
        {
            var context = new ApplicationDbContext();
            var apiBaseUrl = context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiBaseUrl)?.SettingValue;
            var maxRecordLimit = 100;
            //var stopFetchingRecordsAt = 500;

            var fullUrl = GetApiPrefix() + apiEndpoint + "?limit=" + maxRecordLimit;
            if (!string.IsNullOrEmpty(fields))
            {
                fullUrl += "&fields=" + fields;
            }

            var tokenModel = GetToken();
            if (!tokenModel.IsSuccessful)
            {
                var ex = new EF2ORCustomException("There was a problem connecting to the API.  Make sure the connection can test properly in the Settings page.");
                throw ex;
            }

            var token = tokenModel.Token;

            return await GetFullJArray(token, fullUrl, apiBaseUrl, maxRecordLimit);
        }

        private async Task<ApiResponse> GetFullJArray(string token, string fullUrl, string apiBaseUrl, int maxRecordLimit)
        {

            var finalResponse = new JArray();
            
            using (var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
            {
                client.DefaultRequestHeaders.Authorization =
                       new AuthenticationHeaderValue("Bearer", token);

                var offset = 0;
                bool getMoreRecords = true;
                while (getMoreRecords)
                {
                    var apiResponse = await client.GetAsync(fullUrl + "&offset=" + offset);

                    if (apiResponse.IsSuccessStatusCode == false && apiResponse.ReasonPhrase == "Invalid token")
                    {
                        return new ApiResponse
                        {
                            TokenExpired = true,
                            ResponseArray = null
                        };
                    }

                    if (apiResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception(apiResponse.ReasonPhrase);
                    }

                    var responseJson = await apiResponse.Content.ReadAsStringAsync();
                    var responseArray = JArray.Parse(responseJson);

                    if (responseArray != null && responseArray.Count() > 0)
                    {
                        finalResponse = new JArray(finalResponse.Union(responseArray));
                        offset += maxRecordLimit;
                    }
                    else
                    {
                        getMoreRecords = false;
                    }

                    if (responseArray.Count() != maxRecordLimit)//  || finalResponse.Count() >= stopFetchingRecordsAt)
                    {
                        getMoreRecords = false;
                    }
                }
                return new ApiResponse
                {
                    TokenExpired = false,
                    ResponseArray = finalResponse
                };
            }
        }

        private async Task<ApiResponse> GetPagedJArray(string token, string fullUrl, string apiBaseUrl, int offset)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
            {
                client.DefaultRequestHeaders.Authorization =
                       new AuthenticationHeaderValue("Bearer", token);

                var apiResponse = await client.GetAsync(fullUrl + "&offset=" + offset);

                if (apiResponse.IsSuccessStatusCode == false && apiResponse.ReasonPhrase == "Invalid token")
                {
                    return new ApiResponse
                    {
                        TokenExpired = true,
                        ResponseArray = null
                    };
                }

                if (apiResponse.IsSuccessStatusCode == false)
                {
                    throw new Exception(apiResponse.ReasonPhrase);
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
    }
}
