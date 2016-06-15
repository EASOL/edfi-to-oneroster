using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ED2OR.ViewModels
{
    public class SchoolViewModel
    {
        public string id { get; set; }
        public string schoolId { get; set; }
        public string nameOfInstitution { get; set; }
    }

    public class SubjectsViewModel
    {
        public string id { get; set; }
        public string schoolId { get; set; }
        public string acedemicSubjectDescriptor { get; set; }
    }
}