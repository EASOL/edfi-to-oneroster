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

namespace EF2OR.Tests.TestProviders
{
    public class TestApiResponseProvider : EF2OR.Providers.IApiResponseProvider
    {
        async Task<IEdFiOdsData> IApiResponseProvider.GetApiData<T>(string apiEndpoint, bool forceNew = false, string fields = null, Dictionary<string, string> filters = null)
        {
            T result = new T();
            switch (apiEndpoint)
            {
                case "enrollment/schools":
                    var schools = 
                        Newtonsoft.Json.JsonConvert.DeserializeObject<List<SchoolsNS.Class1>>(EF2OR.Tests.Properties.Resources.Enrollment_Schools);
                    if (typeof(T) == typeof(SchoolsNS.Schools))
                    {
                        var tResult = result as SchoolsNS.Schools;
                        tResult.Property1 = schools.ToArray();
                    }
                    break;
                case "enrollment/staffs":
                    var staffs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StaffNS.Class1>>(EF2OR.Tests.Properties.Resources.Enrollment_Staffs);
                    if (typeof(T) == typeof(StaffNS.Staffs))
                    {
                        var tResult = result as StaffNS.Staffs;
                        tResult.Property1 = staffs.ToArray();
                    }
                    break;
                case "enrollment/students":
                    var students = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StudentNS.Class1>>(EF2OR.Tests.Properties.Resources.Enrollment_Students);
                    if (typeof(T) == typeof(StudentNS.Students))
                    {
                        var tResult = result as StudentNS.Students;
                        tResult.Property1 = students.ToArray();
                    }
                    break;
                case "enrollment/sections":
                    var sections = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SectionsNS.Class1>>(EF2OR.Tests.Properties.Resources.Enrollment_Sections);
                    if (typeof(T) == typeof(SectionsNS.Sections))
                    {
                        var tResult = result as SectionsNS.Sections;
                        tResult.Property1 = sections.ToArray();
                    }
                    break;
                default:
                    Assert.Fail(string.Format("Unexpected Endpoint: {0}", apiEndpoint));
                    break;
            }
            await Task.Yield();
            return result;
        }

        string IApiResponseProvider.GetApiPrefix()
        {
            throw new NotImplementedException();
        }

        Task<JArray> IApiResponseProvider.GetCustomApiData(string customUrl)
        {
            throw new NotImplementedException();
        }

        Task<JArray> IApiResponseProvider.GetPagedApiData(string apiEndpoint, int offset, string fields = null, Dictionary<string, string> filters = null)
        {
            throw new NotImplementedException();
        }
    }
}
