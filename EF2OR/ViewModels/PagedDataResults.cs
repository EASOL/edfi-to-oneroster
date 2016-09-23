using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EF2OR.ViewModels
{
    public class UsersPagedDataResults : PagedDataResults
    {
        public PagedDataResults SchoolsPagedDataResults { get; set; } = new PagedDataResults();
        public PagedDataResults SectionsPagedDataResults { get; set; } = new PagedDataResults();
        public PagedDataResults StudentsPagedDataResults { get; set; } = new PagedDataResults();
        public PagedDataResults StaffPagedDataResults { get; set; } = new PagedDataResults();
        public PagedDataResults EnrollmentsPagedDataResults { get; set; } = new PagedDataResults();
    }
    public class PagedDataResults
    {
        public int TotalPages { get; set; }
        public int PagesEnumerated { get; set; }
        public int CurrentOffset { get; set; }
        public FilterInputs Inputs { get; set; }
        public List<CsvOrgs> Orgs { get; set; }
        public List<CsvOrgs> OrgsCurrentPage { get; set; }
        public List<CsvUsers> Users { get; set; }
        public List<CsvUsers> UsersCurentPage { get; set; }
        public List<CsvClasses> Classes { get; set; }
        public List<CsvClasses> ClassesCurentPage { get; set; }
        public List<CsvCourses> Courses { get; set; }
        public List<CsvCourses> CoursesCurentPage { get; set; }
        public List<CsvEnrollments> Enrollments { get; set; }
        public List<CsvEnrollments> EnrollmentsCurentPage { get; set; }
        public List<CsvAcademicSessions> AcademicSessions { get; set; }
        public List<CsvAcademicSessions> AcademicSessionsCurrentPage { get; set; }
    }
}