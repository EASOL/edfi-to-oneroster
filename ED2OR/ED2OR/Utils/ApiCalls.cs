using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ED2OR.ViewModels;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using ED2OR.Enums;
using ED2OR.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace ED2OR.Utils
{
    public class ApiCalls
    {
        #region Variables
        private static readonly ApplicationDbContext db = new ApplicationDbContext();
        private static Dictionary<string, JArray> ExistingResponses = new Dictionary<string, JArray>();
        private static string UserId
        {
            get
            {
                var context = HttpContext.Current;
                return context.User.Identity.GetUserId();
            }
        }

        private static string GetApiPrefix()
        {
            using (var context = new ApplicationDbContext())
            {
               return context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiPrefix)?.SettingValue;
            }
        }

        #endregion

        #region TokenMethods
        //public async Task<ApiCallViewModel> GetToken()
        public static TokenViewModel GetToken(bool forceNewToken = false)
        {
            if (forceNewToken || HttpContext.Current.Session["token"] == null || ((TokenViewModel)HttpContext.Current.Session["token"]).IsSuccessful == false)
            {
                var context = new ApplicationDbContext();
                var apiBaseUrl = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiBaseUrl)?.SettingValue;
                var apiKey = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiKey)?.SettingValue;
                var apiSecret = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiSecret)?.SettingValue;
                HttpContext.Current.Session["token"] = GetToken(apiBaseUrl, apiKey, apiSecret);
                context.Dispose();
            }

            return (TokenViewModel)HttpContext.Current.Session["token"];
        }

        public static TokenViewModel GetToken(string apiBaseUrl, string apiKey, string apiSecret)
        {
            try
            {
                using (var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
                {
                    var authorizeResult = client.PostAsync(ApiEndPoints.OauthAuthorize,
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string,string>("Client_id",apiKey),
                        new KeyValuePair<string,string>("Response_type","code")
                    })).Result;
                    var codeJson = authorizeResult.Content.ReadAsStringAsync().Result;

                    if (!authorizeResult.IsSuccessStatusCode)
                    {
                        return GetApiCallViewModel(false, "Request returned \"" + authorizeResult.ReasonPhrase + "\"", "");
                    }

                    if (JObject.Parse(codeJson)["error"] != null)
                    {
                        return GetApiCallViewModel(false, JObject.Parse(codeJson)["error"].ToString(), "");
                    }

                    var code = JObject.Parse(codeJson)["code"].ToString();

                    var tokenRequestResult = client.PostAsync(ApiEndPoints.OauthGetToken,
                        new FormUrlEncodedContent(new[]
                        {
                    new KeyValuePair<string,string>("Client_id",apiKey),
                    new KeyValuePair<string,string>("Client_secret",apiSecret),
                    new KeyValuePair<string,string>("Code",code),
                    new KeyValuePair<string,string>("Grant_type","authorization_code")
                        })).Result;
                    var tokenJson = tokenRequestResult.Content.ReadAsStringAsync().Result;

                    if (JObject.Parse(tokenJson)["error"] != null)
                    {
                        return GetApiCallViewModel(false, JObject.Parse(tokenJson)["error"].ToString(), "");
                    }

                    return GetApiCallViewModel(true, "", JObject.Parse(tokenJson)["access_token"].ToString());
                }
            }
            catch (AggregateException ex)
            {
                return GetApiCallViewModel(false, ex.InnerException.InnerException.Message, "");
            }
            catch (UriFormatException ex)
            {
                return GetApiCallViewModel(false, "Invalid Url", "");
            }
            catch (Exception ex)
            {
                return GetApiCallViewModel(false, ex.Message, "");
            }
        }
        #endregion

        #region FilterMethods
        public static async Task PopulateFilterSection1(ExportsViewModel model, bool collapseAll = false)
        {
            var schools = await GetSchools();
            var schoolYears = await GetSchoolYears();
            var terms = await GetTerms();

            /////////////Load these two now because the API is already stored in the dictionary up top.  It'll be in the session for the user for later.  He'll get instant checkboxes
            await GetSubjects();
            await GetCourses();
            await GetTeachers();
            await GetSections();
            //////////////////////////////////////

            model.SchoolsCriteriaSection = new ApiCriteriaSection
            {
                FilterCheckboxes = schools,
                SectionName = "Schools",
                IsExpanded = collapseAll ? false : true
            };

            model.SchoolYearsCriteriaSection = new ApiCriteriaSection
            {
                FilterCheckboxes = schoolYears,
                SectionName = "School Years",
                IsExpanded = collapseAll ? false : true
            };

            model.TermsCriteriaSection = new ApiCriteriaSection
            {
                FilterCheckboxes = terms,
                SectionName = "Terms",
                IsExpanded = collapseAll ? false : true
            };
        }

        public static async Task<List<ExportsCheckbox>> GetSchools()
        {
            if (HttpContext.Current.Session["AllSchools"] == null)
            {
                var responseArray = await GetApiResponseArray(ApiEndPoints.Schools);
                var schools = (from s in responseArray
                               select new ExportsCheckbox
                               {
                                   Id = (string)s["id"],
                                   SchoolId = (string)s["id"],
                                   Text = (string)s["nameOfInstitution"],
                                   Visible = true
                               }).OrderBy(x => x.Text).ToList();

                HttpContext.Current.Session["AllSchools"] = schools;
            }

            return (List<ExportsCheckbox>)HttpContext.Current.Session["AllSchools"];
        }

        public static async Task<List<ExportsCheckbox>> GetSchoolYears()
        {
            if (HttpContext.Current.Session["AllSchoolYears"] == null)
            {
                var responseArray = await GetApiResponseArray(ApiEndPoints.SchoolYears, false, "id,uniqueSectionCode,academicSubjectDescriptor,sessionReference,courseOfferingReference,locationReference,schoolReference,staff");

                var schoolYearsStrings = (from s in responseArray
                                   select (string)s["sessionReference"]["schoolYear"]).Distinct();

                var schoolYears = (from s in schoolYearsStrings
                                   select new ExportsCheckbox
                              {
                                  Id = s,
                                  Text = s,
                                  Visible = true
                              }).OrderBy(x => x.Text).ToList();
                HttpContext.Current.Session["AllSchoolYears"] = schoolYears;
            }
            return (List<ExportsCheckbox>)HttpContext.Current.Session["AllSchoolYears"];
        }

        public static async Task<List<ExportsCheckbox>> GetTerms()
        {
            if (HttpContext.Current.Session["AllTerms"] == null)
            {
                var responseArray = await GetApiResponseArray(ApiEndPoints.SchoolYears);
                var termStrings = (from s in responseArray
                                          select (string)s["sessionReference"]["termDescriptor"]).Distinct();

                var terms = (from s in termStrings
                             select new ExportsCheckbox
                                   {
                                       Id = s,
                                       Text = s,
                                       Visible = true
                                   }).OrderBy(x => x.Text).ToList();
                HttpContext.Current.Session["AllTerms"] = terms;
            }
            return (List<ExportsCheckbox>)HttpContext.Current.Session["AllTerms"];
        }

        public static async Task<List<ExportsCheckbox>> GetSubjects()
        {
            if (HttpContext.Current.Session["AllSubjects"] == null)
            {
                var responseArray = await GetApiResponseArray(ApiEndPoints.Subjects);
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


                HttpContext.Current.Session["AllSubjects"] = subjects;
            }
            return (List<ExportsCheckbox>)HttpContext.Current.Session["AllSubjects"];
        }

        public static async Task<List<ExportsCheckbox>> GetCourses()
        {
            if (HttpContext.Current.Session["AllCourses"] == null)
            {
                var responseArray = await GetApiResponseArray(ApiEndPoints.Courses);
                var courses = (from s in responseArray
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

                HttpContext.Current.Session["AllCourses"] = courses;
            }
            return (List<ExportsCheckbox>)HttpContext.Current.Session["AllCourses"];
        }

        public static async Task<List<ExportsCheckbox>> GetTeachers()
        {
            if (HttpContext.Current.Session["AllTeachers"] == null)
            {
                var sectionsResponse = await GetApiResponseArray(ApiEndPoints.Sections);
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

                var staffResponse = await GetApiResponseArray(ApiEndPoints.Staff, false, "id,firstName,lastSurname");
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

                HttpContext.Current.Session["AllTeachers"] = teachers.OrderBy(x => x.Text).ToList();
            }
            return (List<ExportsCheckbox>)HttpContext.Current.Session["AllTeachers"];
        }

        public static async Task<List<ExportsCheckbox>> GetSections()
        {
            if (HttpContext.Current.Session["AllSections"] == null)
            {
                var responseArray = await GetApiResponseArray(ApiEndPoints.Sections);
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

                HttpContext.Current.Session["AllSections"] = sections;
            }
            return (List<ExportsCheckbox>)HttpContext.Current.Session["AllSections"];
        }

        #endregion

        #region ResultsMethods
        public static async Task<List<string>> GetTermDescriptors(bool forceNew = false)
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.Terms, forceNew);
            var terms = responseArray.Select(x => (string)x["sessionReference"]["termDescriptor"]).Distinct();
            return terms.ToList();
        }

        public static async Task<PreveiwJsonResults> GetJsonPreviews(FilterInputs inputs)
        {
            var dataResults = await GetDataResults(inputs);

            var previewModel = new PreveiwJsonResults();
            previewModel.Orgs = JsonConvert.SerializeObject(dataResults.Orgs);
            previewModel.Users = JsonConvert.SerializeObject(dataResults.Users);
            previewModel.Courses = JsonConvert.SerializeObject(dataResults.Courses);
            previewModel.Classes = JsonConvert.SerializeObject(dataResults.Classes);
            previewModel.Enrollments = JsonConvert.SerializeObject(dataResults.Enrollments);
            previewModel.AcademicSessions = JsonConvert.SerializeObject(dataResults.AcademicSessions);

            return previewModel;
        }

        public static async Task<DataResults> GetDataResults(FilterInputs inputs)
        {
            var dataResults = new DataResults();

            if (ExistingResponses.Count() > 0)
            {
                ExistingResponses.Clear(); //reset the global dictionary so we get fresh data
            }

            dataResults.Orgs = await GetCsvOrgs(inputs);
            dataResults.Users = await GetCsvUsers(inputs);
            dataResults.Courses = await GetCsvCourses(inputs);
            dataResults.Classes = await GetCsvClasses(inputs);
            dataResults.Enrollments = await GetCsvEnrollments(inputs);
            dataResults.AcademicSessions = await GetCsvAcademicSessions(inputs);

            ExistingResponses.Clear(); //reset the global dictionary so next time we get fresh data.  Also so it doesn't sit in memory.

            return dataResults;
        }

        public static async Task<List<CsvOrgs>> GetCsvOrgs(FilterInputs inputs)
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvOrgs);

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

        public static async Task<List<CsvUsers>> GetCsvUsers(FilterInputs inputs)
        {
            var enrollmentsResponse = await GetApiResponseArray(ApiEndPoints.CsvUsers);

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

            var studentsResponse = await GetApiResponseArray(ApiEndPoints.CsvUsersStudents);
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
                                   identifier = (string)s["studentUniqueId"],
                                   email = emailAddress,
                                   sms = mobileNumber,
                                   phone = mainTelephoneNumber,
                                   username = (string)s["loginId"]
                               }).ToList();


            var staffResponse = await GetApiResponseArray(ApiEndPoints.CsvUsersStaff);
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
                                   role = "student",
                                   userId = si.userId,
                                   givenName = si.givenName,
                                   familyName = si.familyName,
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
                                 role = "teacher",
                                 userId = si.userId,
                                 givenName = si.givenName,
                                 familyName = si.familyName,
                                 identifier = si.identifier,
                                 email = si.email,
                                 sms = si.sms,
                                 phone = si.phone,
                                 username = si.username
                             }).ToList();

            var distinctStudents = studentInfo.GroupBy(x => x.sourcedId).Select(group => group.First());
            var distinctStaff = staffInfo.GroupBy(x => x.sourcedId).Select(group => group.First());

            var studentsAndStaff = distinctStudents.Concat(distinctStaff);
            return studentsAndStaff.ToList();
        }

        public static async Task<List<CsvCourses>> GetCsvCourses(FilterInputs inputs)
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvCourses);
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

        public static async Task<List<CsvClasses>> GetCsvClasses(FilterInputs inputs)
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvClasses);
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

        public static async Task<List<CsvEnrollments>> GetCsvEnrollments(FilterInputs inputs)
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvEnrollments);
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

        public static async Task<List<CsvAcademicSessions>> GetCsvAcademicSessions(FilterInputs inputs)
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvAcademicSessions);

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

            context.Dispose();
            return enrollmentsList.ToList();
        }
        #endregion

        #region PrivateMethods
        private static async Task<JArray> GetApiResponseArray(string apiEndpoint, bool forceNew = false, string fields = null)
        {
            if (ExistingResponses.ContainsKey(apiEndpoint) && !forceNew)
            {
                return ExistingResponses[apiEndpoint];
            }

            var response = await GetApiResponse(apiEndpoint, fields);
            if (response.TokenExpired)
            {
                GetToken(true);
                response = await GetApiResponse(apiEndpoint, fields);
            }

            if (ExistingResponses.ContainsKey(apiEndpoint))
            {
                ExistingResponses.Remove(apiEndpoint);
            }

            ExistingResponses.Add(apiEndpoint, response.ResponseArray);

            return response.ResponseArray;
        }

        private static async Task<ApiResponse> GetApiResponse(string apiEndpoint, string fields)
        {
            var context = new ApplicationDbContext();
            //var stopFetchingRecordsAt = 500;
            var maxRecordLimit = 100;
            var fullUrl = GetApiPrefix() + apiEndpoint + "?limit=" + maxRecordLimit;

            if (!string.IsNullOrEmpty(fields))
            {
                fullUrl += "&fields=" + fields;
            }

            var tokenModel = GetToken();
            if (!tokenModel.IsSuccessful)
            {
                return null;
            }

            var token = tokenModel.Token;

            var apiBaseUrl = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiBaseUrl)?.SettingValue;

            var finalResponse = new JArray();
            using (var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
            {
                client.DefaultRequestHeaders.Authorization =
                       new AuthenticationHeaderValue("Bearer", token);

                var offset = 0;
                bool getMoreRecords = true;
                while (getMoreRecords)
                {
                    var apiResponse = await client.GetAsync(fullUrl + "&offset=" + offset);

                    if (apiResponse.IsSuccessStatusCode == false && apiResponse.ReasonPhrase == "Invalid token")
                    {
                        return new ApiResponse
                        {
                            TokenExpired = true,
                            ResponseArray = null
                        };
                    }

                    if (apiResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception(apiResponse.ReasonPhrase);
                    }

                    var responseJson = await apiResponse.Content.ReadAsStringAsync();
                    var responseArray = JArray.Parse(responseJson);

                    if (responseArray != null && responseArray.Count() > 0)
                    {
                        finalResponse = new JArray(finalResponse.Union(responseArray));
                        offset += maxRecordLimit;
                    }
                    else
                    {
                        getMoreRecords = false;
                    }

                    if (responseArray.Count() != maxRecordLimit)//  || finalResponse.Count() >= stopFetchingRecordsAt)
                    {
                        getMoreRecords = false;
                    }
                }

                return new ApiResponse
                {
                    TokenExpired = false,
                    ResponseArray = finalResponse
                };
            }
        }

        private static TokenViewModel GetApiCallViewModel(bool isSuccessful, string errorMsg, string token)
        {
            return new TokenViewModel
            {
                IsSuccessful = isSuccessful,
                ErrorMessage = errorMsg,
                Token = token
            };
        }
        #endregion
    }
}