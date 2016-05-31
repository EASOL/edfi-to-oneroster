using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.Models;
using ED2OR.ViewModels;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;

namespace ED2OR.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext db = new ApplicationDbContext();

        public ApiCallViewModel GetToken()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            return GetToken(user.ApiBaseUrl, user.ApiKey, user.ApiSecret);
        }

        //public async Task<bool> TestConnection()
        //public async Task<List<Student>> GetStudents(string schoolId)
        public ApiCallViewModel GetToken(string apiBaseUrl, string apiKey, string apiSecret)
        {
            var callResult = new ApiCallViewModel();

            using (var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
            {
                try
                {
                    var authorizeResult = client.PostAsync("oauth/authorize",
                    new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string,string>("Client_id",apiKey),
                            new KeyValuePair<string,string>("Response_type","code")
                    })).Result;
                    var codeJson = authorizeResult.Content.ReadAsStringAsync().Result;

                    if (JObject.Parse(codeJson)["error"] != null)
                    {
                        callResult.IsSuccessful = false;
                        callResult.ErrorMessage = JObject.Parse(codeJson)["error"].ToString();
                        return callResult;
                    }
                    var code = JObject.Parse(codeJson)["code"].ToString();

                    var tokenRequestResult = client.PostAsync("oauth/token",
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
                        callResult.IsSuccessful = false;
                        callResult.ErrorMessage = JObject.Parse(tokenJson)["error"].ToString();
                        return callResult;
                    }

                    callResult.IsSuccessful = true;
                    callResult.Token = JObject.Parse(tokenJson)["access_token"].ToString();
                    return callResult;
                }
                catch(Exception ex)
                {
                    callResult.IsSuccessful = false;
                    callResult.ErrorMessage = ex.InnerException.InnerException.Message;
                    return callResult;
                }
            }
        }
    }
}