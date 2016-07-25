using System.ComponentModel.DataAnnotations;

namespace EF2OR.Models
{
    public class ApplicationSetting
    {
        [Key]
        public int ApplicationSettingsId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}