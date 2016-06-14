using System.Collections.Generic;

namespace ED2OR.ViewModels
{
    public class CsvEnrollments
    {
        public string sourcedId { get; set; }
        public string classSourcedId { get; set; }
        public string schoolSourcedId { get; set; }
        public string userSourcedId { get; set; }
        public string role { get; set; }
        public string status { get; set; }
        public IEnumerable<string> studentIds { get; set; } //does not appear in csv
        public IEnumerable<string> staffIds { get; set; } //does not appear in csv
    }
}