using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ED2OR.Utils
{
    public class PreConfigurationHelper
    {
        public static bool IsInitialSetup()
        {
            bool isInitialSetup = false;
            using (Models.AdminDBModels.AdminDBEntities ctx = new Models.AdminDBModels.AdminDBEntities())
            {
                var userRow = ctx.AdminUsers.Where(p=>p.Username=="admin").FirstOrDefault();
                isInitialSetup = !(userRow != null && !string.IsNullOrWhiteSpace(userRow.Password));
            }
            return isInitialSetup;
        }
    }
}
