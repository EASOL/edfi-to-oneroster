using EF2OR.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using EF2OR.Enums;

namespace EF2OR.Utils
{
    public class CsvMethods
    {
        //http://stackoverflow.com/questions/1179816/best-practices-for-serializing-objects-to-a-custom-string-format-for-use-in-an-ou
        public async Task<byte[]> GetZipFile(List<string> schoolIds,
            //List<string> schoolYears,
            //List<string> terms,
            //List<string> subjects,
            //List<string> courses,
            List<string> teachers,
            List<string> sections,
            string oneRosterVersion)
        {
            var model = new DataResults();
            var inputs = new FilterInputs
            {
                Schools = schoolIds,
                //SchoolYears = schoolYears,
                //Terms = terms,
                //Subjects = subjects,
                //Courses = courses,
                Teachers = teachers,
                Sections = sections
            };
            model = await ApiCalls.GetDataResults(inputs, oneRosterVersion);

            var csvFilesDirectory = "~/CsvFiles";
            var csvDirectoryFullName = CommonUtils.PathProvider.MapPath(csvFilesDirectory);

            var directoryGuid = Guid.NewGuid().ToString();
            var tempDirectory = csvFilesDirectory + "/" + directoryGuid;
            var tempDirectoryFullName = CommonUtils.PathProvider.MapPath(tempDirectory);

            Directory.CreateDirectory(tempDirectoryFullName);
            WriteObjectToCsv(model.Orgs, tempDirectoryFullName, "orgs.csv", oneRosterVersion);
            WriteObjectToCsv(model.Users, tempDirectoryFullName, "users.csv", oneRosterVersion);
            WriteObjectToCsv(model.Courses, tempDirectoryFullName, "courses.csv", oneRosterVersion);
            WriteObjectToCsv(model.Classes, tempDirectoryFullName, "classes.csv", oneRosterVersion);
            WriteObjectToCsv(model.Enrollments, tempDirectoryFullName, "enrollments.csv", oneRosterVersion);
            WriteObjectToCsv(model.AcademicSessions, tempDirectoryFullName, "academicSessions.csv", oneRosterVersion);
            WriteObjectToCsv(model.Demographics, tempDirectoryFullName, "demographics.csv", oneRosterVersion);
            if (oneRosterVersion == OneRosterVersions.OR_1_1)
            {
                WriteObjectToCsv(model.Manifest, tempDirectoryFullName, "manifest.csv", oneRosterVersion);
            }

            var zipPath = Path.Combine(csvDirectoryFullName, directoryGuid + ".zip");

            var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\orgs.csv", "orgs.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\users.csv", "users.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\courses.csv", "courses.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\classes.csv", "classes.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\enrollments.csv", "enrollments.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\academicSessions.csv", "academicSessions.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\demographics.csv", "demographics.csv");
            if (oneRosterVersion == OneRosterVersions.OR_1_1)
            {
                zip.CreateEntryFromFile(tempDirectoryFullName + "\\manifest.csv", "manifest.csv");
            }
            zip.Dispose();

            var bytes = File.ReadAllBytes(zipPath); //if this eats memory there are other options: http://stackoverflow.com/questions/2041717/how-to-delete-file-after-download-with-asp-net-mvc
            Directory.Delete(tempDirectoryFullName, true);
            File.Delete(zipPath);
            return bytes;
        }

        private void WriteObjectToCsv<T>(List<T> inputList, string directoryPath, string fileName, string oneRosterVersion)
        {
            if (inputList == null) inputList = new List<T>();

            var filePath = Path.Combine(directoryPath, fileName);

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                IEnumerable<string> columnNames;
                if (oneRosterVersion == OneRosterVersions.OR_1_0)
                {
                    columnNames = typeof(T).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR10IncludeFieldAttribute))).Select(x => x.Name);
                }
                else
                {
                    columnNames = typeof(T).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR11IncludeFieldAttribute))).Select(x => x.Name);
                }
                var headerLine = string.Join(",", columnNames);
                headerLine = headerLine.Replace("__", ".");  //some column names have dots in them.  I put double underscores that should be replaced
                sw.WriteLine(headerLine);

                foreach (var rec in inputList)
                {
                    var newLine = new List<string>();
                    foreach (string prop in columnNames)
                    {
                        var stringVal = rec.GetType().GetProperty(prop).GetValue(rec, null)?.ToString() ?? "";
                        newLine.Add("\"" + stringVal + "\"");
                    }
                    sw.WriteLine(string.Join(",", newLine));
                }
            }
        }

        //private async Task<FileResult> GetZipFile()
        //{
        //    var csvFilesDirectory = "~/CsvFiles";
        //    var csvDirectoryFullName = CommonUtils.PathProviderMapPath(csvFilesDirectory);

        //    var directoryGuid = Guid.NewGuid().ToString();
        //    var tempDirectory = csvFilesDirectory + "/" + directoryGuid;
        //    var tempDirectoryFullName = CommonUtils.PathProvider.MapPath(tempDirectory);

        //    Directory.CreateDirectory(tempDirectoryFullName);
        //    WriteObjectToCsv(await ApiCalls.GetCsvOrgs(), tempDirectoryFullName, "orgs.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvUsers(), tempDirectoryFullName, "users.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvCourses(), tempDirectoryFullName, "courses.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvClasses(), tempDirectoryFullName, "classes.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvEnrollments(), tempDirectoryFullName, "enrollments.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvAcademicSessions(), tempDirectoryFullName, "academicSessions.csv");

        //    var zipPath = Path.Combine(csvDirectoryFullName, directoryGuid + ".zip");

        //    var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\orgs.csv", "orgs.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\users.csv", "users.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\courses.csv", "courses.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\classes.csv", "classes.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\enrollments.csv", "enrollments.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\academicSessions.csv", "academicSessions.csv");
        //    zip.Dispose();

        //    var bytes = System.IO.File.ReadAllBytes(zipPath); //if this eats memory there are other options: http://stackoverflow.com/questions/2041717/how-to-delete-file-after-download-with-asp-net-mvc
        //    Directory.Delete(tempDirectoryFullName, true);
        //    System.IO.File.Delete(zipPath);
        //    var downloadFileName = "EdFiExport_" + string.Format("{0:MM_dd_yyyy}", DateTime.Now) + ".zip";
        //    return File(bytes, "application/zip", downloadFileName);
        //}

    }
}