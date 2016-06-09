using Newtonsoft.Json.Linq;

namespace ED2OR.ViewModels
{
    public class ApiResponse
    {
        public bool TokenExpired { get; set; }
        public JArray ResponseArray { get; set; }
    }
}