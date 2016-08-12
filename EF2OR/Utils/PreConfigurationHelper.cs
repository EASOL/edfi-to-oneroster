using System;
using System.IO;
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
            string mappedPath = CommonUtils.PathProvider.MapPath(path);
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

        internal static bool IsInitialSetup(HttpContext context)
        {
            bool isInitialSetup = false;
            var adminUser = PreConfigurationHelper.GetLocalAdminInfo(context);
            isInitialSetup = (adminUser == null) || (adminUser != null && adminUser.InitialDatabaseConfigured == false);
            return isInitialSetup;
        }

        private static LocalAdminInfo GetLocalAdminInfo(object context)
        {
            string mappedPath = CommonUtils.PathProvider.MapPath(path);
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
            string mappedPath = CommonUtils.PathProvider.MapPath(path);
            System.IO.FileStream writer = null;
            if (!System.IO.File.Exists(mappedPath))
            {
                string directory = System.IO.Path.GetDirectoryName(mappedPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                writer = File.Create(mappedPath);
            }
            else
                writer = File.Open(mappedPath, System.IO.FileMode.Create);
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
