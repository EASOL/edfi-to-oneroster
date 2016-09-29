using EF2OR.Entities;
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
using SchoolsNS = EF2OR.Entities.EdFiOdsApi.Enrollment.Schools;
using SectionsNS = EF2OR.Entities.EdFiOdsApi.Enrollment.Sections;
using StaffsNS = EF2OR.Entities.EdFiOdsApi.Enrollment.Staffs;
using StudentsNS = EF2OR.Entities.EdFiOdsApi.Enrollment.Students;
using EdFiOdsApiNS = EF2OR.Entities.EdFiOdsApi;
using System.Net;

namespace EF2OR.Providers
{
    public class ApiResponseProvider : IApiResponseProvider
    {
        internal static ApplicationDbContext db = new ApplicationDbContext();

        private class PageInfo
        {
            public int Offset { get; set; }
        }

        public ApiResponseProvider()
        {
        }

        public async Task<EdFiOdsApiNS.IEdFiOdsData> GetApiData<T>(string apiEndpoint, bool forceNew = false, string fields = null, 
            Dictionary<string, string> filters = null) where T : class, EdFiOdsApiNS.IEdFiOdsData, new()
        {
            int millisecondsToKeepAlive = 3600000;
            ServicePointManager.SetTcpKeepAlive(true, millisecondsToKeepAlive, millisecondsToKeepAlive);
            if (CommonUtils.ExistingResponses.ContainsKey(apiEndpoint) && !forceNew)
            {
                return CommonUtils.ExistingResponses[apiEndpoint];
            }

            var response = await GetApiResponse<T>(apiEndpoint, fields);
            if (response.TokenExpired)
            {
                Providers.ApiResponseProvider.GetToken(true);
                response = await GetApiResponse<T>(apiEndpoint, fields);
            }

            if (CommonUtils.ExistingResponses.ContainsKey(apiEndpoint))
            {
                CommonUtils.ExistingResponses.Remove(apiEndpoint);
            }

            CommonUtils.ExistingResponses.Add(apiEndpoint, response);

            return response;
        }

        public async Task<JArray> GetPagedApiData(string apiEndpoint, int offset, string fields = null, Dictionary<string,string> filters = null)
        {
            var response = await GetPagedApiResponse(apiEndpoint, fields, offset, filters);
            if (response.TokenExpired)
            {
                Providers.ApiResponseProvider.GetToken(true);
                response = await GetPagedApiResponse(apiEndpoint, fields, offset, filters);
            }

            return response.ResponseArray;
        }

        public async Task<JArray> GetCustomApiData(string customUrl)
        {
            var response = await GetCustomApiResponse(customUrl);
            if (response.TokenExpired)
            {
                Providers.ApiResponseProvider.GetToken(true);
                response = await GetCustomApiResponse(customUrl);
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

        public string GetApiPrefix()
        {
            using (var context = new ApplicationDbContext())
            {
                return context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiPrefix)?.SettingValue;
            }
        }

        private async Task<ApiResponse> GetCustomApiResponse(string customUrl)
        {
            var tokenModel = GetToken();
            if (!tokenModel.IsSuccessful)
            {
                var ex = new EF2ORCustomException("There was a problem connecting to the API.  Make sure the connection can test properly in the Settings page.");
                throw ex;
            }

            var token = tokenModel.Token;

            var context = new ApplicationDbContext();
            var apiBaseUrl = context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiBaseUrl)?.SettingValue;
            using (var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
            {
                client.DefaultRequestHeaders.Authorization =
                       new AuthenticationHeaderValue("Bearer", token);

                var apiResponse = await client.GetAsync(customUrl);

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

        private async Task<ApiResponse> GetPagedApiResponse(string apiEndpoint, string fields, int offset, Dictionary<string,string> filters = null)
        {
            var context = new ApplicationDbContext();
            var apiBaseUrl = context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiBaseUrl)?.SettingValue;

            var maxRecordLimit = 100;
            var fullUrl = GetApiPrefix() + apiEndpoint + "?limit=" + maxRecordLimit;
            if (!string.IsNullOrEmpty(fields))
            {
                fullUrl += "&fields=" + fields;
            }
            if (filters != null)
            {
                foreach (var item in filters)
                {
                    fullUrl += string.Format("&{0}={1}", item.Key, item.Value);
                }
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

        public async Task<EdFiOdsApiNS.IEdFiOdsData> GetApiResponse<T>(string apiEndpoint, string fields) where T : class, EdFiOdsApiNS.IEdFiOdsData, new()
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
            await Task.Yield();
            return GetFullJArray<T>(token, fullUrl, apiBaseUrl, maxRecordLimit);
        }


        private EdFiOdsApiNS.IEdFiOdsData GetFullJArray<T>(string token, string fullUrl, string apiBaseUrl, int maxRecordLimit) where T : class, EdFiOdsApiNS.IEdFiOdsData, new()
        {
            var offset = 0;
            bool getMoreRecords = true;
            List<ApiResponse> lstInvalidTokenResponses = new List<ApiResponse>();
            List<Exception> lstExceptions = new List<Exception>();
            //JArray finalResponse = new JArray();
            List<EdFiOdsApiNS.IEdFiOdsData> lstJsonResponses = new List<EdFiOdsApiNS.IEdFiOdsData>();
            Object SyncObject = new Object();
            Action<PageInfo, Type> actSinglePageRequest = (requestOffset, entityType) =>
            {
                using (var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
                {
                    client.DefaultRequestHeaders.Authorization =
                           new AuthenticationHeaderValue("Bearer", token);
                    Task<HttpResponseMessage> requestTask = client.GetAsync(fullUrl + "&offset=" + requestOffset.Offset);
                    requestTask.Wait();
                    YieldTime(500);
                    var apiResponse = requestTask.Result;
                    lock (lstInvalidTokenResponses)
                    if (apiResponse.IsSuccessStatusCode == false && apiResponse.ReasonPhrase == "Invalid token")
                        {
                            lstInvalidTokenResponses.Add(new ApiResponse
                            {
                                TokenExpired = true,
                                ResponseArray = null
                            });
                        }
                    lock (lstExceptions)
                    if (apiResponse.IsSuccessStatusCode == false)
                        {
                            lstExceptions.Add(new Exception(apiResponse.ReasonPhrase));
                        }

                    Task<string> contentTask = apiResponse.Content.ReadAsStringAsync();
                    contentTask.Wait();
                    YieldTime(500);
                    var responseJson = contentTask.Result;
                    var responseArray = JArray.Parse(responseJson);

                    lock (SyncObject)
                    if (responseArray != null && responseArray.Count() > 0)
                        {
                            EdFiOdsApiNS.IEdFiOdsData entityToInsert = null;
                            if (entityType == typeof(EdFiOdsApiNS.TermDescriptors))
                            {
                                var tResult = Newtonsoft.Json.JsonConvert.DeserializeObject<EdFiOdsApiNS.Class1[]>(responseJson);
                                entityToInsert = new EdFiOdsApiNS.TermDescriptors()
                                {
                                    Property1 = tResult
                                };
                            }
                            if (entityType == typeof(SchoolsNS.Schools))
                            {
                                var tResult = Newtonsoft.Json.JsonConvert.DeserializeObject<SchoolsNS.Class1[]>(responseJson);
                                entityToInsert = new SchoolsNS.Schools()
                                {
                                    Property1 = tResult
                                };
                            }
                            if (entityType == typeof(SectionsNS.Sections))
                            {
                                var tResult = Newtonsoft.Json.JsonConvert.DeserializeObject<SectionsNS.Class1[]>(responseJson);
                                entityToInsert = new SectionsNS.Sections()
                                {
                                    Property1 = tResult
                                };
                            }
                            if (entityType == typeof(StaffsNS.Staffs))
                            {
                                var tResult = Newtonsoft.Json.JsonConvert.DeserializeObject<StaffsNS.Class1[]>(responseJson);
                                entityToInsert = new StaffsNS.Staffs()
                                {
                                    Property1 = tResult
                                };
                            }
                            if (entityType == typeof(StudentsNS.Students))
                            {
                                var tResult = Newtonsoft.Json.JsonConvert.DeserializeObject<StudentsNS.Class1[]>(responseJson);
                                entityToInsert = new StudentsNS.Students()
                                {
                                    Property1 = tResult
                                };
                            }
                            if (entityToInsert != null)
                            {
                                lstJsonResponses.Add(entityToInsert);
                            }
                            else
                            {
                                throw new Exception(string.Format("No matching entity type for {0}", entityType.FullName));
                            }
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
            };
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            Type type = typeof(T);
            while (getMoreRecords)
            {
                List<Action> actionsToExecute = new List<Action>(Properties.Settings.Default.ThreadQty);
                for (int i=0; i < Properties.Settings.Default.ThreadQty; i++)
                {
                    var pi = new PageInfo { Offset = offset + i*100 };
                    Action actionToAdd = () => actSinglePageRequest(pi, type);
                    actionsToExecute.Add(actionToAdd);
                };
                Parallel.Invoke(actionsToExecute.ToArray());
                offset += maxRecordLimit * actionsToExecute.Count();
                YieldTime(1000);
                actionsToExecute.Clear();
            }
            stopWatch.Stop();
            var ellapsed = stopWatch.Elapsed;
            //JArray finalResponse = new JArray();
            //while (lstJsonResponses.Count > 0)
            //{
            //    var firstItem = lstJsonResponses[0];
            //    var jArrayElement = JArray.Parse(firstItem);
            //    while (jArrayElement.Count > 0)
            //    {
            //        finalResponse.Add(jArrayElement[0]);
            //        jArrayElement.RemoveAt(0);
            //    }
            //    lstJsonResponses.RemoveAt(0);
            //}
            //    while (getMoreRecords)
            //    {
            //        var apiResponse = await client.GetAsync(fullUrl + "&offset=" + offset);

            //        if (apiResponse.IsSuccessStatusCode == false && apiResponse.ReasonPhrase == "Invalid token")
            //        {
            //            return new ApiResponse
            //            {
            //                TokenExpired = true,
            //                ResponseArray = null
            //            };
            //        }

            //        if (apiResponse.IsSuccessStatusCode == false)
            //        {
            //            throw new Exception(apiResponse.ReasonPhrase);
            //        }

            //        var responseJson = await apiResponse.Content.ReadAsStringAsync();
            //        var responseArray = JArray.Parse(responseJson);

            //        if (responseArray != null && responseArray.Count() > 0)
            //        {
            //            finalResponse = new JArray(finalResponse.Union(responseArray));
            //            offset += maxRecordLimit;
            //        }
            //        else
            //        {
            //            getMoreRecords = false;
            //        }

            //        if (responseArray.Count() != maxRecordLimit)//  || finalResponse.Count() >= stopFetchingRecordsAt)
            //        {
            //            getMoreRecords = false;
            //        }
            //    }
            T result = new T();
            try
            {
                if (typeof(T) == typeof(SchoolsNS.Schools))
                {
                    var termDescriptorsResult = lstJsonResponses.Cast<SchoolsNS.Schools>();
                    var tResult = result as SchoolsNS.Schools;
                    tResult.Property1 = termDescriptorsResult.SelectMany(p => p.Property1).ToArray();
                }
                if (typeof(T) == typeof(SectionsNS.Sections))
                {
                    var termDescriptorsResult = lstJsonResponses.Cast<SectionsNS.Sections>();
                    var tResult = result as SectionsNS.Sections;
                    tResult.Property1 = termDescriptorsResult.SelectMany(p => p.Property1).ToArray();
                }
                if (typeof(T) == typeof(StaffsNS.Staffs))
                {
                    var termDescriptorsResult = lstJsonResponses.Cast<StaffsNS.Staffs>();
                    var tResult = result as StaffsNS.Staffs;
                    tResult.Property1 = termDescriptorsResult.SelectMany(p => p.Property1).ToArray();
                }
                if (typeof(T) == typeof(StudentsNS.Students))
                {
                    var termDescriptorsResult = lstJsonResponses.Cast<StudentsNS.Students>();
                    var tResult = result as StudentsNS.Students;
                    tResult.Property1 = termDescriptorsResult.SelectMany(p => p.Property1).ToArray();
                }
                if (typeof(T) == typeof(EdFiOdsApiNS.TermDescriptors))
                {
                    var termDescriptorsResult = lstJsonResponses.Cast<EdFiOdsApiNS.TermDescriptors>();
                    var tResult = result as EdFiOdsApiNS.TermDescriptors;
                    tResult.Property1 = termDescriptorsResult.SelectMany(p => p.Property1).ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        private void YieldTime(int millisecondsToYield)
        {
            System.Threading.Thread.Sleep(millisecondsToYield);
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
