using System.ComponentModel.DataAnnotations;

namespace EF2OR.ViewModels
{
    public class InitialSetup
    {
        [Display(Name = "Database Server", Description ="Name of IP of Database Server")]
        public string DatabaseServer { get; set; }
        [Display(Name = "Database Name", Description = "Database Name/Initial Catalog")]
        public string DatabaseName { get; set; }
        [Display(Name = "Database User Id", Description = "Id of the user for the connectionstring")]
        public string DatabaseUserId { get; set; }
        [Display(Name = "Database Password", Description = "Password of IP of Database Server")]
        public string DatabaseUserPassword { get; set; }
        [Display(Name = "Application Name", Description = "Name that will used to identity your application into the Database Engine")]
        public string DatabaseApplicationName { get; set; }
        //[Display(Name = "Application Admin Password", Description = "Admin Password generated first time application was run")]
        //public string ApplicationAdminPassword { get; set; }
        [Display(Name = "Use Integrated Security", Description = "Whether or not Integrated Security will be used")]
        public bool IntegratedSecuritySSPI { get; set; }
    }
}
