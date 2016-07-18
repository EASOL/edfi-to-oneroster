using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ED2OR.Models
{
    public class ApplicationSetting
    {
        [Key]
        public int ApplicationSettingsId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}