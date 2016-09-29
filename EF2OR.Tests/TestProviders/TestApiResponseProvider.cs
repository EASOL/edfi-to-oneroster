using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EF2OR.Providers;
using EF2OR.ViewModels;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EF2OR.Entities;
using EF2OR.Entities.EdFiOdsApi;
using SchoolsNS = EF2OR.Entities.EdFiOdsApi.Enrollment.Schools;
using StaffNS = EF2OR.Entities.EdFiOdsApi.Enrollment.Staffs;
using StudentNS = EF2OR.Entities.EdFiOdsApi.Enrollment.Students;
using SectionsNS = EF2OR.Entities.EdFiOdsApi.Enrollment.Sections;
using EF2OR.Models;
using EF2OR.Enums;
using System.Configuration;

namespace EF2OR.Tests.TestProviders
{
    public class TestApiResponseProvider : EF2OR.Providers.IApiResponseProvider
    {
        async Task<IEdFiOdsData> IApiResponseProvider.GetApiData<T>(string apiEndpoint, bool forceNew = false, string fields = null, Dictionary<string, string> filters = null)
        {
            bool foundMatch = false;
            T result = new T();
            if (apiEndpoint == "enrollment/schools" ||
                System.Text.RegularExpressions.Regex.IsMatch(input: apiEndpoint, pattern: string.Format("enrollment/schools/{0}/sections", @"\d*")))
            {
                var schools =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<List<SchoolsNS.Class1>>(EF2OR.Tests.Properties.Resources.Enrollment_Schools);
                if (typeof(T) == typeof(SchoolsNS.Schools))
                {
                    var tResult = result as SchoolsNS.Schools;
                    tResult.Property1 = schools.ToArray();
                }
                foundMatch = true;
            }
            if (apiEndpoint == "enrollment/staffs")
            {
                var staffs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StaffNS.Class1>>(EF2OR.Tests.Properties.Resources.Enrollment_Staffs);
                if (typeof(T) == typeof(StaffNS.Staffs))
                {
                    var tResult = result as StaffNS.Staffs;
                    tResult.Property1 = staffs.ToArray();
                }
                foundMatch = true;
            }
            if (apiEndpoint == "enrollment/students")
            {
                var students = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StudentNS.Class1>>(EF2OR.Tests.Properties.Resources.Enrollment_Students);
                if (typeof(T) == typeof(StudentNS.Students))
                {
                    var tResult = result as StudentNS.Students;
                    tResult.Property1 = students.ToArray();
                }
                foundMatch = true;
            }
            if (apiEndpoint == "enrollment/sections")
            {
                var sections = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SectionsNS.Class1>>(EF2OR.Tests.Properties.Resources.Enrollment_Sections);
                if (typeof(T) == typeof(SectionsNS.Sections))
                {
                    var tResult = result as SectionsNS.Sections;
                    tResult.Property1 = sections.ToArray();
                }
                foundMatch = true;
            }
            if (!foundMatch)
                Assert.Fail(string.Format("Unexpected Endpoint: {0}", apiEndpoint));
            await Task.Yield();
            return result;
        }

        string IApiResponseProvider.GetApiPrefix()
        {
            using (var context = new ApplicationDbContext())
            {
                return context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiPrefix)?.SettingValue;
            }
        }

        Task<JArray> IApiResponseProvider.GetCustomApiData(string customUrl)
        {
            throw new NotImplementedException();
        }

        private static int _checkboxPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["CheckboxPageSize"]);
        private static int _maxApiCallSize = Convert.ToInt32(ConfigurationManager.AppSettings["ApiMaxCallSize"]);
        private static int _dataPreviewPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["DataPreviewPageSize"]);

        async Task<JArray> IApiResponseProvider.GetPagedApiData(string apiEndpoint, int offset, string fields = null, Dictionary<string, string> filters = null)
        {
            bool foundMatch = false;
            JArray result = null;
            JArray apiResponse = null;
            if (apiEndpoint == "enrollment/schools")
            {
                apiResponse =
                    JArray.Parse(EF2OR.Tests.Properties.Resources.Enrollment_Schools);
                result = await Task.FromResult(apiResponse);
                foundMatch = true;
            }
            if (apiEndpoint == "enrollment/staffs" ||
                System.Text.RegularExpressions.Regex.IsMatch(input: apiEndpoint, pattern: string.Format("enrollment/schools/{0}/staffs", @"\d*")))
            {
                apiResponse =
                    JArray.Parse(EF2OR.Tests.Properties.Resources.Enrollment_Staffs);
                result = await Task.FromResult(apiResponse);
                foundMatch = true;
            }
            if (apiEndpoint == "enrollment/students")
            {
                apiResponse =
                    JArray.Parse(EF2OR.Tests.Properties.Resources.Enrollment_Students);
                result = await Task.FromResult(apiResponse);
                foundMatch = true;
            }
            if (apiEndpoint == "enrollment/sections" ||
                System.Text.RegularExpressions.Regex.IsMatch(input: apiEndpoint, pattern: string.Format("enrollment/schools/{0}/sections", @"\d*"))
                )
            {
                apiResponse =
                    JArray.Parse(EF2OR.Tests.Properties.Resources.Enrollment_Sections);
                result = await Task.FromResult(apiResponse);
                foundMatch = true;
            }
            if (!foundMatch)
                Assert.Fail(string.Format("Unexpected Endpoint: {0}", apiEndpoint));
            if (offset > 0 && result != null && result.Count > 0)
            {
                var offsetData = result.Skip(offset);
                result = JArray.FromObject(offsetData);
            }
            result = JArray.FromObject(result.Take(_maxApiCallSize));
            return result;
        }
    }
}
