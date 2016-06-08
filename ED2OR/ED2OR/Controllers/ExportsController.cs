using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.ViewModels;
using System.Threading.Tasks;
using System.Net.Http;
using ED2OR.Enums;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace ED2OR.Controllers
{
    public class ExportsController : BaseController
    {
        public async Task<ActionResult> Index()
        {

            var schools = await GetSchools();
            //if (schools != null)
            var schoolsCbs = (from s in schools
                                select new ExportsCheckbox
                                {
                                    Id = s.id,
                                    SchoolId = s.schoolId,
                                    Text = s.nameOfInstitution,
                                    Visible = true
                                }).ToList();

            var model = new ExportsViewModel();

            model.CriteriaSections = new List<ApiCriteriaSection>
            {
                new ApiCriteriaSection
                {
                    FilterCheckboxes = schoolsCbs,
                    SectionName = "Schools",
                    Level = 1
                },
                new ApiCriteriaSection
                {
                    FilterCheckboxes = GenerateCbList(),
                    SectionName = "Subjects",
                    Level = 2
                },
                new ApiCriteriaSection
                {
                    FilterCheckboxes = GenerateCbList(),
                    SectionName = "Courses",
                    Level = 3
                },
                new ApiCriteriaSection
                {
                    FilterCheckboxes = GenerateCbList(),
                    SectionName = "Sections",
                    Level = 3
                }
                ,new ApiCriteriaSection
                {
                    FilterCheckboxes = GenerateCbList(),
                    SectionName = "Teachers",
                    Level = 3
                }
            };

            return View(model);
        }

        public List<ExportsCheckbox> GenerateCbList()
        {
            return new List<ExportsCheckbox>
                {
                    new ExportsCheckbox
                    {
                        Id = "a",
                        Selected = true,
                        Text = "abc"
                    },
                     new ExportsCheckbox
                    {
                        Id = "b",
                        Selected = false,
                        Text = "def"
                    },
                      new ExportsCheckbox
                    {
                        Id = "c",
                        Selected = true,
                        Text = "ghi"
                    }
                };
        }

        public async Task<List<SchoolViewModel>> GetSchools()
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

                var schoolsResponse = await client.GetAsync(ApiEndPoints.ApiPrefix + ApiEndPoints.Schools);
                var schoolsJson = await schoolsResponse.Content.ReadAsStringAsync();
                var schoolsArray = JArray.Parse(schoolsJson);

                var schools = (from s in schoolsArray
                                select new SchoolViewModel
                                {
                                    id = (string)s["id"],
                                    schoolId = (string)s["schoolId"],
                                    nameOfInstitution = (string)s["nameOfInstitution"]
                                }).ToList();
                return schools;
            }
        }

        [HttpPost]
        public ActionResult Index(ExportsViewModel model)
        {
            return View(model);
        }
    }
}