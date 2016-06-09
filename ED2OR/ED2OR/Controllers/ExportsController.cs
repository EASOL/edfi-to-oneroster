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
using ED2OR.Utils;

namespace ED2OR.Controllers
{
    public class ExportsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var model = new ExportsViewModel();
            var schools = await ApiCalls.GetSchools();
            //if (schools != null)
            //{

            //}
            var schoolsCbs = (from s in schools
                            select new ExportsCheckbox
                            {
                                Id = s.id,
                                SchoolId = s.schoolId,
                                Text = s.nameOfInstitution,
                                Visible = true
                            }).ToList();


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

        

        [HttpPost]
        public ActionResult Index(ExportsViewModel model)
        {
            return View(model);
        }
    }
}