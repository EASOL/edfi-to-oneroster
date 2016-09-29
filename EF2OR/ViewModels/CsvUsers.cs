using System.Collections.Generic;

namespace EF2OR.ViewModels
{
    public class CsvUsers
    {
        [OR10IncludeField]
        [OR11IncludeField]
        public string sourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string status { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string dateLastModified { get; set; }
        [OR11IncludeField]
        public string enabledUser { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string orgSourcedIds { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string role { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string username { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string userId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string givenName { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string familyName { get; set; }
        [OR11IncludeField]
        public string middleNames { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string identifier { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string email { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string sms { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string phone { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string agents { get; set; }
        [OR11IncludeField]
        public string grade { get; set; }
        [OR11IncludeField]
        public string password { get; set; }

        public string SchoolId { get; set; }
        public string SchoolYear { get; set; }
        public string Term { get; set; }
        public string Subject { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
        public List<string> Teachers { get; set; }
    }
}