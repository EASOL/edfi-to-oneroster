using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ED2OR.ViewModels
{
    public class InitialSetup
    {
        public string DatabaseServer { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUserId { get; set; }
        public string DatabaseUserPassword { get; set; }
        public string DatabaseApplicationName { get; set; }
        public string ApplicationAdminPassword { get; set; }
        public bool IntegratedSecuritySSPI { get; set; }
    }
}
