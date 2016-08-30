using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EF2OR.ViewModels;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using EF2OR.Enums;
using EF2OR.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace EF2OR.Utils
{
    public class ApiCalls
    {
        #region Variables
        private static readonly ApplicationDbContext db = new ApplicationDbContext();
        static ApiCalls()
        {
            Providers.ApiResponseProvider.db = ApiCalls.db;
            CommonUtils.ExistingResponses = new Dictionary<string, JArray>();
    }
        private static string UserId
        {
            get
            {
                return CommonUtils.UserProvider.UserId;
            }
        }

        #endregion


        #region FilterMethods
        public static async Task PopulateFilterSection1(ExportsViewModel model)
        {
            var schools = await GetSchools("id,nameOfInstitution");
            var schoolYears = await GetSchoolYears("id,uniqueSectionCode,academicSubjectDescriptor,sessionReference,courseOfferingReference,locationReference,schoolReference,staff");
            var terms = await GetTerms();

            /////////////Load these now because the API is already stored in the dictionary up top.  It'll be in the session for the user for later.  He'll get instant checkboxes
            //await GetSubjects();
            //await GetCourses();
            //await GetTeachers();
            //await GetSections();
            //////////////////////////////////////

            model.SchoolsCriteriaSection = new ApiCriteriaSection
            {
                FilterCheckboxes = schools,
                SectionName = "Schools",
                IsExpanded = true
            };

            model.SchoolYearsCriteriaSection = new ApiCriteriaSection
            {
                FilterCheckboxes = schoolYears,
                SectionName = "School Years",
                IsExpanded = true
            };

            model.TermsCriteriaSection = new ApiCriteriaSection
            {
                FilterCheckboxes = terms,
                SectionName = "Terms",
                IsExpanded = true
            };
        }

        public static async Task<List<ExportsCheckbox>> GetSchools(string fields = null)
        {
            if (CommonUtils.HttpContextProvider.Current.Session["AllSchools"] == null)
            {
                var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.Schools, false, fields);
                var schools = (from s in responseArray
                               select new ExportsCheckbox
                               {
                                   Id = (string)s["id"],
                                   SchoolId = (string)s["id"],
                                   Text = (string)s["nameOfInstitution"],
                                   Visible = true
                               }).OrderBy(x => x.Text).ToList();

                CommonUtils.HttpContextProvider.Current.Session["AllSchools"] = schools;
            }

            var allSchools = (List<ExportsCheckbox>)CommonUtils.HttpContextProvider.Current.Session["AllSchools"];
            allSchools.ForEach(c => c.Selected = false); // make sure all are unchecked first

            return allSchools;
        }

        public static async Task<List<ExportsCheckbox>> GetSchoolYears(string fields = null)
        {
            if (CommonUtils.HttpContextProvider.Current.Session["AllSchoolYears"] == null)
            {
                var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.SchoolYears, false, fields);

                var schoolYearsStrings = (from s in responseArray
                                          select (string)s["sessionReference"]["schoolYear"]).Distinct();

                var schoolYears = (from s in schoolYearsStrings
                                   select new ExportsCheckbox
                                   {
                                       Id = s,
                                       Text = s,
                                       Visible = true
                                   }).OrderBy(x => x.Text).ToList();
                CommonUtils.HttpContextProvider.Current.Session["AllSchoolYears"] = schoolYears;
            }

            var allSchoolYears = (List<ExportsCheckbox>)CommonUtils.HttpContextProvider.Current.Session["AllSchoolYears"];
            allSchoolYears.ForEach(c => c.Selected = false); // make sure all are unchecked first

            return allSchoolYears;
        }

        public static async Task<List<ExportsCheckbox>> GetTerms()
        {
            if (CommonUtils.HttpContextProvider.Current.Session["AllTerms"] == null)
            {
                var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.SchoolYears);
                var termStrings = (from s in responseArray
                                   select (string)s["sessionReference"]["termDescriptor"]).Distinct();

                var terms = (from s in termStrings
                             select new ExportsCheckbox
                             {
                                 Id = s,
                                 Text = s,
                                 Visible = true
                             }).OrderBy(x => x.Text).ToList();
                CommonUtils.HttpContextProvider.Current.Session["AllTerms"] = terms;
            }

            var allTerms = (List<ExportsCheckbox>)CommonUtils.HttpContextProvider.Current.Session["AllTerms"];
            allTerms.ForEach(c => c.Selected = false); // make sure all are unchecked first

            return allTerms;
        }

        public static async Task<List<ExportsCheckbox>> GetSubjects()
        {
            if (CommonUtils.HttpContextProvider.Current.Session["AllSubjects"] == null)
            {
                var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.Subjects);
                var subjects = (from s in responseArray
                                select new ExportsCheckbox
                                {
                                    Id = (string)s["academicSubjectDescriptor"],
                                    SchoolId = (string)s["schoolReference"]["id"],
                                    SchoolYear = (string)s["sessionReference"]["schoolYear"],
                                    Term = (string)s["sessionReference"]["termDescriptor"],
                                    Text = (string)s["academicSubjectDescriptor"],
                                    Visible = true
                                }).OrderBy(x => x.Text).ToList();


                CommonUtils.HttpContextProvider.Current.Session["AllSubjects"] = subjects;
            }

            var allSubjects = (List<ExportsCheckbox>)CommonUtils.HttpContextProvider.Current.Session["AllSubjects"];
            allSubjects.ForEach(c => c.Selected = false); // make sure all are unchecked first

            return allSubjects;
        }

        public static async Task<ApiCriteriaSection> GetCourses(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            int limit,
            int offset)
        {
            //var checkboxes = new List<ExportsCheckbox>();
            var model = new ApiCriteriaSection();
            if (CommonUtils.HttpContextProvider.Current.Session["CoursesModel"] != null)
            {
                model = (ApiCriteriaSection)CommonUtils.HttpContextProvider.Current.Session["CoursesModel"];
            }
            else
            {
                model = new ApiCriteriaSection
                {
                    SectionName = "Courses",
                    IsExpanded = true,
                    CurrentOffset = 0,
                    NumCheckBoxesToDisplay = limit,
                    FilterCheckboxes = new List<ExportsCheckbox>(),
                    AllCheckboxes = new List<ExportsCheckbox>()
                };

            //FilterCheckboxes(model.FilterCheckboxes, schoolIds, schoolYears, terms);

            while (model.FilterCheckboxes.Count() < limit && !model.AllDataReceived)
            {
                    var responseArray = await CommonUtils.ApiResponseProvider.GetPagedApiData(ApiEndPoints.Courses, 0);
                    var checkboxes = (from s in responseArray
                                  select new ExportsCheckbox
                                  {
                                      Id = (string)s["courseOfferingReference"]["localCourseCode"],
                                      SchoolId = (string)s["schoolReference"]["id"],
                                      SchoolYear = (string)s["sessionReference"]["schoolYear"],
                                      Term = (string)s["sessionReference"]["termDescriptor"],
                                      Text = (string)s["courseOfferingReference"]["localCourseCode"],
                                      Subject = (string)s["academicSubjectDescriptor"],
                                      Visible = true
                                  }).OrderBy(x => x.Text).ToList();

                    if (checkboxes.Count < 100)
                    {
                        model.AllDataReceived = true;
                    }

                    model.AllCheckboxes.AddRange(checkboxes);
                    model.FilterCheckboxes = FilterCheckboxes(model.AllCheckboxes, schoolIds, schoolYears, terms);
                }
            }

            CommonUtils.HttpContextProvider.Current.Session["CoursesModel"] = model;

            return model;


            //allCourses.ForEach(c => c.Selected = false); // make sure all are unchecked first

            //var ret = new ApiCriteriaSection
            //{
            //    SectionName = "Courses",
            //    IsExpanded = true,
            //    FilterCheckboxes = allCourses,
            //    CurrentOffset = 0,
            //    NumCheckBoxesToDisplay = 0
            //};

            //List < ExportsCheckbox >
        }

        private static List<ExportsCheckbox> FilterCheckboxes(List<ExportsCheckbox> allBoxes,
           List<string> schoolIds,
           List<string> schoolYears,
           List<string> terms)
        {
            bool allSchools = schoolIds == null || schoolIds.Count() == 0;
            bool allSchoolYears = schoolYears == null || schoolYears.Count() == 0;
            bool allTerms = terms == null || terms.Count() == 0;

            var filteredBoxes = new List<ExportsCheckbox>();
            filteredBoxes.AddRange(allBoxes);

            if (!allSchools)
            {
                filteredBoxes = filteredBoxes.Where(x =>
                schoolIds.Contains(x.SchoolId)).ToList();
            }

            if (!allSchoolYears)
            {
                filteredBoxes = filteredBoxes.Where(x =>
                schoolYears.Contains(x.SchoolYear)).ToList();
            }

            if (!allTerms)
            {
                filteredBoxes = filteredBoxes.Where(x =>
                terms.Contains(x.Term)).ToList();
            }

            filteredBoxes = filteredBoxes.GroupBy(x => x.Text).Select(group => group.First()).ToList();

            return filteredBoxes;
        }

        public static async Task<List<ExportsCheckbox>> GetTeachers()
        {
            if (CommonUtils.HttpContextProvider.Current.Session["AllTeachers"] == null)
            {
                var sectionsResponse = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.Sections);
                var sections = (from o in sectionsResponse
                                let staffs = o["staff"].Children()//.Select(x => (string)x["id"])
                                select new
                                {
                                    SchoolId = (string)o["schoolReference"]["id"],
                                    SchoolYear = (string)o["courseOfferingReference"]["schoolYear"],
                                    Term = (string)o["courseOfferingReference"]["termDescriptor"],
                                    Subject = (string)o["academicSubjectDescriptor"],
                                    Course = (string)o["courseOfferingReference"]["localCourseCode"],
                                    staffs = staffs
                                });

                var staffSections = from section in sections
                                    from staff in section.staffs
                                    select new
                                    {
                                        SchoolId = section.SchoolId,
                                        SchoolYear = section.SchoolYear,
                                        Term = section.Term,
                                        Subject = section.Subject,
                                        Course = section.Course,
                                        StaffId = (string)staff["id"]
                                    };

                var distinctStaffSections = staffSections.GroupBy(x => x.StaffId).Select(group => group.First());

                var staffResponse = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.Staff, false, "id,firstName,lastSurname");
                var staffInfo = from s in staffResponse
                                select new
                                {
                                    Id = (string)s["id"],
                                    Name = (string)s["firstName"] + " " + (string)s["lastSurname"]
                                };

                var teachers = (from ss in distinctStaffSections
                                from si in staffInfo.Where(x => x.Id == ss.StaffId).DefaultIfEmpty()
                                let teacherName = si == null ? "" : (si.Name ?? "")
                                select new ExportsCheckbox
                                {
                                    SchoolId = ss.SchoolId,
                                    SchoolYear = ss.SchoolYear,
                                    Term = ss.Term,
                                    Subject = ss.Subject,
                                    Course = ss.Course,
                                    Text = teacherName,
                                    Id = ss.StaffId
                                });

                CommonUtils.HttpContextProvider.Current.Session["AllTeachers"] = teachers.OrderBy(x => x.Text).ToList();
            }

            var allTeachers = (List<ExportsCheckbox>)CommonUtils.HttpContextProvider.Current.Session["AllTeachers"];
            allTeachers.ForEach(c => c.Selected = false); // make sure all are unchecked first

            return allTeachers;
        }

        public static async Task<List<ExportsCheckbox>> GetSections()
        {
            if (CommonUtils.HttpContextProvider.Current.Session["AllSections"] == null)
            {
                var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.Sections);
                var sections = (from s in responseArray
                                select new ExportsCheckbox
                                {
                                    Id = (string)s["uniqueSectionCode"], //or is it ["id"]??
                                    Course = (string)s["courseOfferingReference"]["localCourseCode"],
                                    SchoolId = (string)s["schoolReference"]["id"],
                                    SchoolYear = (string)s["sessionReference"]["schoolYear"],
                                    Term = (string)s["sessionReference"]["termDescriptor"],
                                    Text = (string)s["uniqueSectionCode"],
                                    Subject = (string)s["academicSubjectDescriptor"],
                                    Visible = true
                                }).OrderBy(x => x.Text).ToList();

                CommonUtils.HttpContextProvider.Current.Session["AllSections"] = sections;
            }

            var allSections = (List<ExportsCheckbox>)CommonUtils.HttpContextProvider.Current.Session["AllSections"];
            allSections.ForEach(c => c.Selected = false); // make sure all are unchecked first

            return allSections;
        }

        #endregion

        #region ResultsMethods
        public static async Task<List<string>> GetTermDescriptors(bool forceNew = false)
        {
            var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.TermDescriptors, forceNew);
            var terms = responseArray.Select(x => (string)x["description"]).Distinct();
            CommonUtils.ExistingResponses.Remove(ApiEndPoints.TermDescriptors);  //now we have one in there with only termDescriptor
            return terms.ToList();
        }

        public static async Task<PreveiwJsonResults> GetJsonPreviews(FilterInputs inputs, string oneRosterVersion)
        {
            var dataResults = await GetDataResults(inputs, oneRosterVersion);

            var previewModel = new PreveiwJsonResults();
            previewModel.Orgs = JsonConvert.SerializeObject(dataResults.Orgs);
            previewModel.Users = JsonConvert.SerializeObject(dataResults.Users);
            previewModel.Courses = JsonConvert.SerializeObject(dataResults.Courses);
            previewModel.Classes = JsonConvert.SerializeObject(dataResults.Classes);
            previewModel.Enrollments = JsonConvert.SerializeObject(dataResults.Enrollments);
            previewModel.AcademicSessions = JsonConvert.SerializeObject(dataResults.AcademicSessions);
            if (oneRosterVersion == OneRosterVersions.OR_1_1)
            {
                previewModel.Manifest = JsonConvert.SerializeObject(dataResults.Manifest);
            }

            return previewModel;
        }

        public static async Task<DataResults> GetDataResults(FilterInputs inputs, string oneRosterVersion)
        {
            var dataResults = new DataResults();

            if (CommonUtils.ExistingResponses.Count() > 0)
            {
                CommonUtils.ExistingResponses.Clear(); //reset the global dictionary so we get fresh data
            }

            dataResults.Orgs = await GetCsvOrgs(inputs);
            dataResults.Users = await GetCsvUsers(inputs);
            dataResults.Courses = await GetCsvCourses(inputs);
            dataResults.Classes = await GetCsvClasses(inputs);
            dataResults.Enrollments = await GetCsvEnrollments(inputs);
            dataResults.AcademicSessions = await GetCsvAcademicSessions(inputs);
            if (oneRosterVersion == OneRosterVersions.OR_1_1)
            {
                dataResults.Manifest = GetCsvManifest(DownloadTypes.bulk);
            }

            CommonUtils.ExistingResponses.Clear(); //reset the global dictionary so next time we get fresh data.  Also so it doesn't sit in memory.

            return dataResults;
        }

        private static async Task<List<CsvOrgs>> GetCsvOrgs(FilterInputs inputs)
        {
            var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.CsvOrgs);

            var context = new ApplicationDbContext();
            var identifierSetting = context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.OrgsIdentifier)?.SettingValue;
            bool blankIdentifier = identifierSetting == null || identifierSetting == OrgIdentifierSettings.blank;
            context.Dispose();

            var listOfObjects = (from o in responseArray
                                 select new CsvOrgs
                                 {
                                     sourcedId = (string)o["id"],
                                     name = (string)o["nameOfInstitution"],
                                     type = "school",
                                     identifier = blankIdentifier ? "" : (string)o["stateOrganizationId"],
                                     parentSourcedId = (string)o["localEducationAgencyReference"]["id"],
                                     SchoolId = (string)o["id"]
                                 });

            if (inputs != null && inputs.Schools != null)
            {
                listOfObjects = listOfObjects.Where(x => inputs.Schools.Contains(x.SchoolId));
            }
            return listOfObjects.ToList();
        }

        private static async Task<List<CsvUsers>> GetCsvUsers(FilterInputs inputs)
        {
            var enrollmentsResponse = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.CsvUsers);

            var enrollmentsList = (from o in enrollmentsResponse
                                   let students = o["students"].Children().Select(x => (string)x["id"])
                                   let staffs = o["staff"].Children().Select(x => (string)x["id"])
                                   let teachers = o["staff"].Children().Select(x => (string)x["id"])
                                   select new
                                   {
                                       students = students,
                                       staffs = staffs,
                                       SchoolId = (string)o["schoolReference"]["id"],
                                       SchoolYear = (string)o["courseOfferingReference"]["schoolYear"],
                                       Term = (string)o["courseOfferingReference"]["termDescriptor"],
                                       Subject = (string)o["academicSubjectDescriptor"],
                                       Course = (string)o["courseOfferingReference"]["localCourseCode"],
                                       Section = (string)o["uniqueSectionCode"],
                                       Teachers = teachers
                                   });

            if (inputs != null)
            {
                if (inputs.Schools != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Schools.Contains(x.SchoolId));

                if (inputs.SchoolYears != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.SchoolYears.Contains(x.SchoolYear));

                if (inputs.Terms != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Terms.Contains(x.Term));

                if (inputs.Subjects != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Subjects.Contains(x.Subject));

                if (inputs.Courses != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Courses.Contains(x.Course));

                if (inputs.Sections != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Sections.Contains(x.Section));

                if (inputs.Teachers != null)
                    enrollmentsList = enrollmentsList.Where(x => x.Teachers.Intersect(inputs.Teachers).Any());
            }
            enrollmentsList = enrollmentsList.ToList();

            var studentsResponse = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.CsvUsersStudents);
            var studentsResponseInfo = (from s in studentsResponse
                                        let mainTelephone = (s["telephones"] == null || s["telephones"].Count() == 0) ? null : s["telephones"].Children().FirstOrDefault(x => (string)x["orderOfPriority"] == "1")
                                        let mainTelephoneNumber = mainTelephone == null ? "" : (string)mainTelephone["telephoneNumber"]
                                        let emailAddress = (s["electronicMails"] == null || s["electronicMails"].Count() == 0) ? "" : (string)s["electronicMails"][0]["electronicMailAddress"] //TODO: just pick 0?.  or get based on electronicMailType field.
                                        let mobile = (s["telephones"] == null || s["telephones"].Count() == 0) ? null : s["telephones"].Children().FirstOrDefault(x => (string)x["telephoneNumberType"] == "Mobile")
                                        let mobileNumber = mobile == null ? "" : (string)mobile["telephoneNumber"]
                                        select new
                                        {
                                            id = (string)s["id"],
                                            userId = (string)s["studentUniqueId"],
                                            givenName = (string)s["firstName"],
                                            familyName = (string)s["lastSurname"],
                                            middleName = (string)s["middleName"],
                                            identifier = (string)s["studentUniqueId"],
                                            email = emailAddress,
                                            sms = mobileNumber,
                                            phone = mainTelephoneNumber,
                                            username = (string)s["loginId"]
                                        }).ToList();


            var staffResponse = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.CsvUsersStaff);
            var staffResponseInfo = (from s in staffResponse
                                     let mainTelephone = (s["telephones"] == null || s["telephones"].Count() == 0) ? null : s["telephones"].Children().FirstOrDefault(x => (string)x["orderOfPriority"] == "1")
                                     let mainTelephoneNumber = mainTelephone == null ? "" : (string)mainTelephone["telephoneNumber"]
                                     let emailAddress = (s["electronicMails"] == null || s["electronicMails"].Count() == 0) ? "" : (string)s["electronicMails"][0]["electronicMailAddress"] //TODO: just pick 0?.  or get based on electronicMailType field.
                                     let mobile = (s["telephones"] == null || s["telephones"].Count() == 0) ? null : s["telephones"].Children().FirstOrDefault(x => (string)x["telephoneNumberType"] == "Mobile")
                                     let mobileNumber = mobile == null ? "" : (string)mobile["telephoneNumber"]
                                     select new
                                     {
                                         id = (string)s["id"],
                                         userId = (string)s["staffUniqueId"],
                                         givenName = (string)s["firstName"],
                                         familyName = (string)s["lastSurname"],
                                         middleName = (string)s["middleName"],
                                         identifier = (string)s["staffUniqueId"],
                                         email = emailAddress,
                                         sms = mobileNumber,
                                         phone = mainTelephoneNumber,
                                         username = (string)s["loginId"]
                                     }).ToList();

            var studentInfo = (from e in enrollmentsList
                               from s in e.students
                               from si in studentsResponseInfo.Where(x => x.id == s)
                               select new CsvUsers
                               {
                                   sourcedId = s,
                                   orgSourcedIds = e.SchoolId,
                                   enabledUser = "TRUE",
                                   role = "student",
                                   userId = si.userId,
                                   givenName = si.givenName,
                                   familyName = si.familyName,
                                   middleNames = si.middleName,
                                   identifier = si.identifier,
                                   email = si.email,
                                   sms = si.sms,
                                   phone = si.phone,
                                   username = si.username
                               }).ToList();

            var staffInfo = (from e in enrollmentsList
                             from s in e.staffs
                             from si in staffResponseInfo.Where(x => x.id == s)
                             select new CsvUsers
                             {
                                 sourcedId = s,
                                 orgSourcedIds = e.SchoolId,
                                 enabledUser = "TRUE",
                                 role = "teacher",
                                 userId = si.userId,
                                 givenName = si.givenName,
                                 familyName = si.familyName,
                                 middleNames = si.middleName,
                                 identifier = si.identifier,
                                 email = si.email,
                                 sms = si.sms,
                                 phone = si.phone,
                                 username = si.username
                             }).ToList();

            var distinctStudents = studentInfo.GroupBy(x => new { x.sourcedId, x.SchoolId }).Select(group => group.First());
            var distinctStaff = staffInfo.GroupBy(x => new { x.sourcedId, x.SchoolId }).Select(group => group.First());

            var studentsAndStaff = distinctStudents.Concat(distinctStaff);
            return studentsAndStaff.ToList();
        }

        private static async Task<List<CsvCourses>> GetCsvCourses(FilterInputs inputs)
        {
            var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.CsvCourses);
            var enrollmentsList = (from o in responseArray
                                   let teachers = o["staff"].Children().Select(x => (string)x["id"])
                                   select new CsvCourses
                                   {
                                       sourcedId = (string)o["courseOfferingReference"]["id"],
                                       schoolYearId = (string)o["sessionReference"]["id"],
                                       title = (string)o["courseOfferingReference"]["localCourseCode"],
                                       courseCode = (string)o["courseOfferingReference"]["localCourseCode"],
                                       orgSourcedId = (string)o["schoolReference"]["id"],
                                       subjects = (string)o["academicSubjectDescriptor"],
                                       SchoolId = (string)o["schoolReference"]["id"],
                                       SchoolYear = (string)o["courseOfferingReference"]["schoolYear"],
                                       Term = (string)o["courseOfferingReference"]["termDescriptor"],
                                       Subject = (string)o["academicSubjectDescriptor"],
                                       Course = (string)o["courseOfferingReference"]["localCourseCode"],
                                       Section = (string)o["uniqueSectionCode"],
                                       Teachers = teachers
                                   });

            if (inputs != null)
            {
                if (inputs.Schools != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Schools.Contains(x.SchoolId));

                if (inputs.SchoolYears != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.SchoolYears.Contains(x.SchoolYear));

                if (inputs.Terms != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Terms.Contains(x.Term));

                if (inputs.Subjects != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Subjects.Contains(x.Subject));

                if (inputs.Courses != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Courses.Contains(x.Course));

                if (inputs.Sections != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Sections.Contains(x.Section));

                if (inputs.Teachers != null)
                    enrollmentsList = enrollmentsList.Where(x => x.Teachers.Intersect(inputs.Teachers).Any());
            }

            enrollmentsList = enrollmentsList.GroupBy(x => x.sourcedId).Select(group => group.First());

            return enrollmentsList.ToList();
        }

        private static async Task<List<CsvClasses>> GetCsvClasses(FilterInputs inputs)
        {
            var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.CsvClasses);
            var enrollmentsList = (from o in responseArray
                                   let teachers = o["staff"].Children().Select(x => (string)x["id"])
                                   select new CsvClasses
                                   {
                                       sourcedId = (string)o["id"],
                                       title = (string)o["uniqueSectionCode"],
                                       courseSourcedId = (string)o["courseOfferingReference"]["id"],
                                       classCode = (string)o["uniqueSectionCode"],
                                       classType = "scheduled",
                                       schoolSourcedId = (string)o["schoolReference"]["id"],
                                       termSourcedId = (string)o["sessionReference"]["id"],
                                       subjects = (string)o["academicSubjectDescriptor"],
                                       SchoolId = (string)o["schoolReference"]["id"],
                                       SchoolYear = (string)o["courseOfferingReference"]["schoolYear"],
                                       Term = (string)o["courseOfferingReference"]["termDescriptor"],
                                       Subject = (string)o["academicSubjectDescriptor"],
                                       Course = (string)o["courseOfferingReference"]["localCourseCode"],
                                       Section = (string)o["uniqueSectionCode"],
                                       Teachers = teachers
                                   });

            if (inputs != null)
            {
                if (inputs.Schools != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Schools.Contains(x.SchoolId));

                if (inputs.SchoolYears != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.SchoolYears.Contains(x.SchoolYear));

                if (inputs.Terms != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Terms.Contains(x.Term));

                if (inputs.Subjects != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Subjects.Contains(x.Subject));

                if (inputs.Courses != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Courses.Contains(x.Course));

                if (inputs.Sections != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Sections.Contains(x.Section));

                if (inputs.Teachers != null)
                    enrollmentsList = enrollmentsList.Where(x => x.Teachers.Intersect(inputs.Teachers).Any());
            }

            return enrollmentsList.ToList();
        }

        private static async Task<List<CsvEnrollments>> GetCsvEnrollments(FilterInputs inputs)
        {
            var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.CsvEnrollments);
            var enrollmentsList = (from o in responseArray
                                   let students = o["students"].Children()
                                   let staffs = o["staff"].Children()
                                   let teachers = o["staff"].Children().Select(x => (string)x["id"])
                                   select new
                                   {
                                       classSourcedId = (string)o["id"],
                                       schoolSourcedId = (string)o["schoolReference"]["id"],
                                       students = students,
                                       staffs = staffs,
                                       SchoolId = (string)o["schoolReference"]["id"],
                                       SchoolYear = (string)o["courseOfferingReference"]["schoolYear"],
                                       Term = (string)o["courseOfferingReference"]["termDescriptor"],
                                       Subject = (string)o["academicSubjectDescriptor"],
                                       Course = (string)o["courseOfferingReference"]["localCourseCode"],
                                       Section = (string)o["uniqueSectionCode"],
                                       Teachers = teachers
                                   });

            if (inputs != null)
            {
                if (inputs.Schools != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Schools.Contains(x.SchoolId));

                if (inputs.SchoolYears != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.SchoolYears.Contains(x.SchoolYear));

                if (inputs.Terms != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Terms.Contains(x.Term));

                if (inputs.Subjects != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Subjects.Contains(x.Subject));

                if (inputs.Courses != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Courses.Contains(x.Course));

                if (inputs.Sections != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Sections.Contains(x.Section));

                if (inputs.Teachers != null)
                    enrollmentsList = enrollmentsList.Where(x => x.Teachers.Intersect(inputs.Teachers).Any());
            }

            var studentInfo = (from e in enrollmentsList
                               from s in e.students
                               select new CsvEnrollments
                               {
                                   sourcedId = (string)s["studentSectionAssociation_id"],
                                   classSourcedId = e.classSourcedId,
                                   schoolSourcedId = e.schoolSourcedId,
                                   userSourcedId = (string)s["id"],
                                   role = "student"
                               });

            var staffInfo = (from e in enrollmentsList
                             from s in e.staffs
                             select new CsvEnrollments
                             {
                                 sourcedId = (string)s["staffSectionAssociation_id"],
                                 classSourcedId = e.classSourcedId,
                                 schoolSourcedId = e.schoolSourcedId,
                                 userSourcedId = (string)s["id"],
                                 role = "teacher"
                             });

            var allEnrollments = studentInfo.Concat(staffInfo).ToList();

            return allEnrollments;
        }

        private static async Task<List<CsvAcademicSessions>> GetCsvAcademicSessions(FilterInputs inputs)
        {
            var responseArray = await CommonUtils.ApiResponseProvider.GetApiData(ApiEndPoints.CsvAcademicSessions);

            var context = new ApplicationDbContext();
            var typeDictionary = context.AcademicSessionTypes.ToDictionary(t => t.TermDescriptor, t => t.Type);

            var enrollmentsList = (from o in responseArray
                                   let teachers = o["staff"].Children().Select(x => (string)x["id"])
                                   let termDescriptor = (string)o["sessionReference"]["termDescriptor"]
                                   let type = typeDictionary.ContainsKey(termDescriptor) ? typeDictionary[termDescriptor] : ""
                                   let startDate = (DateTime)o["sessionReference"]["beginDate"]
                                   let endDate = (DateTime)o["sessionReference"]["endDate"]
                                   select new CsvAcademicSessions
                                   {
                                       sourcedId = (string)o["sessionReference"]["id"],
                                       title = (string)o["sessionReference"]["schoolYear"] + " " + termDescriptor,
                                       type = type,
                                       startDate = startDate.ToString("yyyy-MM-dd"),
                                       endDate = endDate.ToString("yyyy-MM-dd"),
                                       SchoolId = (string)o["schoolReference"]["id"],
                                       SchoolYear = (string)o["courseOfferingReference"]["schoolYear"],
                                       Term = (string)o["courseOfferingReference"]["termDescriptor"],
                                       Subject = (string)o["academicSubjectDescriptor"],
                                       Course = (string)o["courseOfferingReference"]["localCourseCode"],
                                       Section = (string)o["uniqueSectionCode"],
                                       Teachers = teachers,
                                       schoolYear = (string)o["sessionReference"]["schoolYear"]
                                   });

            if (inputs != null)
            {
                if (inputs.Schools != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Schools.Contains(x.SchoolId));

                if (inputs.SchoolYears != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.SchoolYears.Contains(x.SchoolYear));

                if (inputs.Terms != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Terms.Contains(x.Term));

                if (inputs.Subjects != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Subjects.Contains(x.Subject));

                if (inputs.Courses != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Courses.Contains(x.Course));

                if (inputs.Sections != null)
                    enrollmentsList = enrollmentsList.Where(x => inputs.Sections.Contains(x.Section));

                if (inputs.Teachers != null)
                    enrollmentsList = enrollmentsList.Where(x => x.Teachers.Intersect(inputs.Teachers).Any());
            }

            enrollmentsList = enrollmentsList.GroupBy(x => x.sourcedId).Select(group => group.First());

            context.Dispose();
            return enrollmentsList.ToList();
        }

        private static List<CsvManifest> GetCsvManifest(string bulkOrDelta)
        {
            return new List<CsvManifest>
            {
                new CsvManifest { propertyName = "manifest.version", value = "1.0" },
                new CsvManifest { propertyName = "oneroster.version", value = "1.1" },
                new CsvManifest { propertyName = "file.academicSessions", value = bulkOrDelta },
                new CsvManifest { propertyName = "file.categories", value = "absent" },
                new CsvManifest { propertyName = "file.classes", value = bulkOrDelta },
                new CsvManifest { propertyName = "file.classResources", value = "absent" },
                new CsvManifest { propertyName = "file.courses", value = bulkOrDelta },
                new CsvManifest { propertyName = "file.courseResources", value = "absent" },
                new CsvManifest { propertyName = "file.demographics", value = "absent" },
                new CsvManifest { propertyName = "file.enrollments", value = bulkOrDelta },
                new CsvManifest { propertyName = "file.lineItems", value = "absent" },
                new CsvManifest { propertyName = "file.orgs", value = bulkOrDelta },
                new CsvManifest { propertyName = "file.resources", value = "absent" },
                new CsvManifest { propertyName = "file.results", value = "absent" },
                new CsvManifest { propertyName = "file.users", value = bulkOrDelta },
                new CsvManifest { propertyName = "source.systemName", value = "absent" },
                new CsvManifest { propertyName = "source.systemCode", value = "absent" }
            };
        }
        #endregion

        #region PrivateMethods

        #endregion
    }
}