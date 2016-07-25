using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Models
{
    public class ErrorLog
    {
        public string Data { get; internal set; }
        [Key]
        public Guid ErrorId { get; set; }
        public string Message { get; internal set; }
    }

}
