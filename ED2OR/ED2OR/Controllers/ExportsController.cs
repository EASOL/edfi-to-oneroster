using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.ViewModels;

namespace ED2OR.Controllers
{
    public class ExportsController : BaseController
    {
        public ActionResult Index()
        {
            var model = new ExportsViewModel();

            model.CriteriaSections = new List<ApiCriteriaSection>
            {
                new ApiCriteriaSection
                {
                    FilterCheckboxes = GenerateCbList(),
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
                        Id = 1,
                        Selected = true,
                        Text = "abc"
                    },
                     new ExportsCheckbox
                    {
                        Id = 2,
                        Selected = false,
                        Text = "def"
                    },
                      new ExportsCheckbox
                    {
                        Id = 3,
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