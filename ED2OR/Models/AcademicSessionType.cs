using System.ComponentModel.DataAnnotations;

namespace EF2OR.Models
{
    public class AcademicSessionType
    {
        [Key]
        public int AcademicSessionTypeId { get; set; }
        public string TermDescriptor { get; set; }
        public string Type { get; set; }
    }
}