using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EF2OR.ViewModels
{
    public class SettingsViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Ed-Fi API Server API Base Url")]
        public string ApiBaseUrl { get; set; }

        [Display(Name = "Ed-Fi API Key")]
        public string ApiKey { get; set; }

        [Display(Name = "Ed-Fi API Secret")]
        public string ApiSecret { get; set; }

        public string ApiPrefix { get; set; }
        public string OrgsIdentifier { get; set; }

        public List<AcademicSessionTypeViewModel> AcademicSessionTypes { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Default OneRoster Version")]
        public string DefaultOneRosterVersion { get; set;}

        public InitialSetup DatabaseSettings { get; set; } = new InitialSetup();
    }

    public class AcademicSessionTypeViewModel
    {
        public int Id { get; set; }
        public string TermDescriptor { get; set; }
        public string Type { get; set; }
    }
}