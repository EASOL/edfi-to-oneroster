using System.ComponentModel.DataAnnotations;

namespace ED2OR.Models
{
    public class AcademicSessionType
    {
        [Key]
        public int AcademicSessionTypeId { get; set; }
        public string TermDescriptor { get; set; }
        public string Type { get; set; }
    }
}