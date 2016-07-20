using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EF2OR.Utils
{
    public class LocalAdminInfo
    {
        public string Username { get; set; }
        public bool InitialDatabaseConfigured { get; set; } = false;
    }
    public class PreConfigurationHelper
    {
        private static string path = "~/Config/preconfigurationInfo.dat";
        internal static LocalAdminInfo GetLocalAdminInfo(HttpContextBase context)
        {
            string mappedPath = context.Server.MapPath(path);
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(LocalAdminInfo));
            LocalAdminInfo objAdminInfo = null;
            if (System.IO.File.Exists(mappedPath))
            {
                try
                {
                    using (var stream = new System.IO.StreamReader(mappedPath))
                    {
                        objAdminInfo = serializer.Deserialize(stream) as LocalAdminInfo;
                    }
                }
                catch (Exception)
                {
                }
            }
            return objAdminInfo;
        }
        public static bool IsInitialSetup(HttpContextBase context)
        {
            bool isInitialSetup = false;
            var adminUser = PreConfigurationHelper.GetLocalAdminInfo(context);
            isInitialSetup = (adminUser == null) || (adminUser != null && adminUser.InitialDatabaseConfigured==false);
            return isInitialSetup;
        }

        internal static void SaveAdminUser(LocalAdminInfo adminUser, HttpContextBase context)
        {
            string mappedPath = context.Server.MapPath(path);
            System.IO.FileStream writer = null;
            if (!System.IO.File.Exists(mappedPath))
            {
                string directory = System.IO.Path.GetDirectoryName(mappedPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                writer = System.IO.File.Create(mappedPath);
            }
            else
                writer = System.IO.File.Open(mappedPath, System.IO.FileMode.Create);
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(LocalAdminInfo));
            using (writer)
            {
                serializer.Serialize(writer, adminUser);
                writer.Flush();
                writer.Close();
            }
        }
    }
}
