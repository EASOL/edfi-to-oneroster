using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EF2OR.ViewModels
{
    public class PagedDataResults
    {
        public int TotalPages { get; set; }
        public int PagesEnumerated { get; set; }
        public int CurrentOffset { get; set; }
        public List<CsvAcademicSessions> AcademicSessions { get; set; }
    }
}