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

        private static string apiPrefix = db.MappingSettings.FirstOrDefault(x => x.SettingName == MappingSettingNames.ApiPrefix)?.SettingValue;

        #endregion

        #region TokenMethods
        //public async Task<ApiCallViewModel> GetToken()
        public static TokenViewModel GetToken(bool forceNewToken = false)
        {
            if (forceNewToken || HttpContext.Current.Session["token"] == null || ((TokenViewModel)HttpContext.Current.Session["token"]).IsSuccessful == false)
            {
                var context = new ApplicationDbContext();
                var user = context.Users.FirstOrDefault(x => x.Id == UserId);
                HttpContext.Current.Session["token"] = GetToken(user.ApiBaseUrl, user.ApiKey, user.ApiSecret);
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
                               }).ToList();

                HttpContext.Current.Session["AllSchools"] = schools;
            }

            return (List<ExportsCheckbox>)HttpContext.Current.Session["AllSchools"];
        }

        public static async Task<List<ExportsCheckbox>> GetSchoolYears()
        {
            if (HttpContext.Current.Session["AllSchoolYears"] == null)
            {
                var responseArray = await GetApiResponseArray(ApiEndPoints.SchoolYears);

                var schoolYearsStrings = (from s in responseArray
                                   select (string)s["sessionReference"]["schoolYear"]).Distinct();

                var schoolYears = (from s in schoolYearsStrings
                                   select new ExportsCheckbox
                              {
                                  Id = s,
                                  Text = s,
                                  Visible = true
                              }).ToList();
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
                                   }).ToList();
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
                                }).ToList();


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
                                }).ToList();

                HttpContext.Current.Session["AllCourses"] = courses;
            }
            return (List<ExportsCheckbox>)HttpContext.Current.Session["AllCourses"];
        }

        public static async Task<List<ExportsCheckbox>> GetTeachers()
        {
            if (HttpContext.Current.Session["AllTeachers"] == null)
            {
                var responseArray = await GetApiResponseArray(ApiEndPoints.Teachers);
                var enrollmentsList = (from o in responseArray
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

                var teachers = (from e in enrollmentsList
                                from s in e.staffs
                                select new ExportsCheckbox
                                {
                                    SchoolId = e.SchoolId,
                                    SchoolYear = e.SchoolYear,
                                    Term = e.Term,
                                    Subject = e.Subject,
                                    Course = e.Course,
                                    Text = (string)s["firstName"] + " " + (string)s["lastSurname"],
                                    Id = (string)s["id"]
                                }).ToList();

                HttpContext.Current.Session["AllTeachers"] = teachers;
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
                                }).ToList();

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
            dataResults.Orgs = await GetCsvOrgs(inputs);
            dataResults.Users = await GetCsvUsers(inputs);
            dataResults.Courses = await GetCsvCourses(inputs);
            dataResults.Classes = await GetCsvClasses(inputs);
            dataResults.Enrollments = await GetCsvEnrollments(inputs);
            dataResults.AcademicSessions = await GetCsvAcademicSessions(inputs);
            return dataResults;
        }

        public static async Task<List<CsvOrgs>> GetCsvOrgs(FilterInputs inputs)
        {
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvOrgs);

            var context = new ApplicationDbContext();
            var identifierSetting = context.MappingSettings.FirstOrDefault(x => x.SettingName == MappingSettingNames.OrgsIdentifier)?.SettingValue;
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
            var responseArray = await GetApiResponseArray(ApiEndPoints.CsvUsers);

            var enrollmentsList = (from o in responseArray
                                   let students = o["students"].Children()
                                   let staffs = o["staff"].Children()
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

            var studentInfo = (from e in enrollmentsList
                               from s in e.students
                               let mainTelephone = s["telephones"].Children().FirstOrDefault(x => (string)x["orderOfPriority"] == "1")
                               let mainTelephoneNumber = mainTelephone == null ? "" : (string)mainTelephone["telephoneNumber"]
                               let emailAddress = s["electronicMails"].Children().Count() > 0 ? (string)s["electronicMails"][0]["electronicMailAddress"] : "" //TODO: just pick 0?.  or get based on electronicMailType field.
                               let mobile = s["telephones"].Children().FirstOrDefault(x => (string)x["telephoneNumberType"] == "Mobile")
                               let mobileNumber = mobile == null ? "" : (string)mobile["telephoneNumber"]
                               //let schoolAssociationsIds = s["schoolAssociations"] != null ? s["schoolAssociations"].Children().Select(x => (string)x["id"]) : null
                               //let orgIds = schoolAssociationsIds == null ? "" :
                               //    (
                               //     schoolAssociationsIds.Count() > 1 ? string.Join(", ", schoolAssociationsIds) : schoolAssociationsIds.FirstOrDefault()
                               //    )
                               select new CsvUsers
                               {
                                   sourcedId = (string)s["id"],
                                   //orgSourcedIds = orgIds,
                                   orgSourcedIds = e.SchoolId,
                                   role = "student",
                                   username = (string)s["loginId"],
                                   userId = (string)s["studentUniqueId"],
                                   givenName = (string)s["firstName"],
                                   familyName = (string)s["lastSurname"],
                                   identifier = (string)s["studentUniqueId"],
                                   email = emailAddress,
                                   sms = mobileNumber,
                                   phone = mainTelephoneNumber
                               });


            var staffInfo = (from e in enrollmentsList
                             from s in e.staffs
                             let mainTelephone = s["telephones"].Children().FirstOrDefault(x => (string)x["orderOfPriority"] == "1")
                             let mainTelephoneNumber = mainTelephone == null ? "" : (string)mainTelephone["telephoneNumber"]
                             let emailAddress = s["electronicMails"].Children().Count() > 0 ? (string)s["electronicMails"][0]["electronicMailAddress"] : "" //TODO: just pick 0?.  or get based on electronicMailType field.
                             let mobile = s["telephones"].Children().FirstOrDefault(x => (string)x["telephoneNumberType"] == "Mobile")
                             let mobileNumber = mobile == null ? "" : (string)mobile["telephoneNumber"]
                             //let schoolAssociationsIds = s["schoolAssociations"] != null ? s["schoolAssociations"].Children().Select(x => (string)x["id"]) : null
                             //let orgIds = schoolAssociationsIds == null ? "" :
                             //    (
                             //     schoolAssociationsIds.Count() > 1 ? string.Join(", ", schoolAssociationsIds) : schoolAssociationsIds.FirstOrDefault()
                             //    )
                             select new CsvUsers
                             {
                                 sourcedId = (string)s["id"],
                                 //orgSourcedIds = orgIds,
                                 orgSourcedIds = e.SchoolId,
                                 role = "teacher",
                                 username = (string)s["loginId"],
                                 userId = (string)s["staffUniqueId"],
                                 givenName = (string)s["firstName"],
                                 familyName = (string)s["lastSurname"],
                                 identifier = (string)s["staffUniqueId"],
                                 email = emailAddress,
                                 sms = mobileNumber,
                                 phone = mainTelephoneNumber
                             });

            var distinctStudents = studentInfo.GroupBy(x => x.sourcedId).Select(group => group.First());
            var distinctStaff = staffInfo.GroupBy(x => x.sourcedId).Select(group => group.First());
            var studentsAndStaff = distinctStudents.Concat(distinctStaff);

            return studentsAndStaff.ToList();
            /*
            This section was with the old Endpoints no longer used
            var responseArrayStudents = await GetApiResponseArray(ApiEndPoints.CsvUsersStudents);
            var listOfStudents = (from o in responseArrayStudents
                                  let mainTelephone = o["telephones"].Children().FirstOrDefault(x => (string)x["orderOfPriority"] == "1")
                                  let mainTelephoneNumber = mainTelephone == null ? "" : (string)mainTelephone["telephoneNumber"]
                                  let emailAddress = o["electronicMails"].Children().Count() > 0 ? (string)o["electronicMails"][0]["electronicMailAddress"] : "" //TODO: just pick 0?.  or get based on electronicMailType field.
                                  let mobile = o["telephones"].Children().FirstOrDefault(x => (string)x["telephoneNumberType"] == "Mobile")
                                  let mobileNumber = mobile == null ? "" : (string)mobile["telephoneNumber"]
                                  let schoolAssociationsIds = o["schoolAssociations"] != null ? o["schoolAssociations"].Children().Select(x => (string)x["id"]) : null
                                  let orgIds = schoolAssociationsIds == null ? "" :
                                      (
                                       schoolAssociationsIds.Count() > 1 ? string.Join(", ", schoolAssociationsIds) : schoolAssociationsIds.FirstOrDefault()
                                      )
                                  select new CsvUsers
                                  {
                                      sourcedId = (string)o["id"],
                                      orgSourcedIds = orgIds,
                                      role = "student",
                                      username = (string)o["loginId"],
                                      userId = (string)o["studentUniqueId"],
                                      givenName = (string)o["firstName"],
                                      familyName = (string)o["lastSurname"],
                                      identifier = (string)o["studentUniqueId"],
                                      email = emailAddress,
                                      sms = mobileNumber,
                                      phone = mainTelephoneNumber
                                  });

            var responseArrayStaff = await GetApiResponseArray(ApiEndPoints.CsvUsersStaff);
            var listOfStaff = (from o in responseArrayStaff
                               let mainTelephone = o["telephones"].Children().FirstOrDefault(x => (string)x["orderOfPriority"] == "1")
                               let mainTelephoneNumber = mainTelephone == null ? "" : (string)mainTelephone["telephoneNumber"]
                               let emailAddress = o["electronicMails"].Children().Count() > 0 ? (string)o["electronicMails"][0]["electronicMailAddress"] : "" //TODO: just pick 0?.  or get based on electronicMailType field.
                               let mobile = o["telephones"].Children().FirstOrDefault(x => (string)x["telephoneNumberType"] == "Mobile")
                               let mobileNumber = mobile == null ? "" : (string)mobile["telephoneNumber"]
                               let schoolAssociationsIds = o["schoolAssociations"] != null ? o["schoolAssociations"].Children().Select(x => (string)x["id"]) : null
                               let orgIds = schoolAssociationsIds == null ? "" :
                                   (
                                    schoolAssociationsIds.Count() > 1 ? string.Join(", ", schoolAssociationsIds) : schoolAssociationsIds.FirstOrDefault()
                                   )
                               select new CsvUsers
                               {
                                   sourcedId = (string)o["id"],
                                   orgSourcedIds = orgIds,
                                   role = "teacher",
                                   username = (string)o["loginId"],
                                   userId = (string)o["staffUniqueId"],
                                   givenName = (string)o["firstName"],
                                   familyName = (string)o["lastSurname"],
                                   identifier = (string)o["staffUniqueId"],
                                   email = emailAddress,
                                   sms = mobileNumber,
                                   phone = mainTelephoneNumber
                               });
            return listOfStudents.Concat(listOfStaff).ToList();*/
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

            var typeDictionary = db.AcademicSessionTypes.ToDictionary(t => t.TermDescriptor, t => t.Type);

            var enrollmentsList = (from o in responseArray
                                   let teachers = o["staff"].Children().Select(x => (string)x["id"])
                                   let termDescriptor = (string)o["sessionReference"]["termDescriptor"]
                                   let type = typeDictionary.ContainsKey(termDescriptor) ? typeDictionary[termDescriptor] : ""
                                   select new CsvAcademicSessions
                                   {
                                       sourcedId = (string)o["sessionReference"]["id"],
                                       title = (string)o["sessionReference"]["schoolYear"] + " " + termDescriptor,
                                       type = type,
                                       startDate = (string)o["sessionReference"]["beginDate"],
                                       endDate = (string)o["sessionReference"]["endDate"],
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
        #endregion

        #region PrivateMethods
        private static async Task<JArray> GetApiResponseArray(string apiEndpoint, bool forceNew = false)
        {
            if (ExistingResponses.ContainsKey(apiEndpoint) && !forceNew)
            {
                return ExistingResponses[apiEndpoint];
            }

            var response = await GetApiResponse(apiEndpoint);
            if (response.TokenExpired)
            {
                GetToken(true);
                response = await GetApiResponse(apiEndpoint);
            }

            if (ExistingResponses.ContainsKey(apiEndpoint))
            {
                ExistingResponses.Remove(apiEndpoint);
            }

            ExistingResponses.Add(apiEndpoint, response.ResponseArray);

            return response.ResponseArray;
        }

        private static async Task<ApiResponse> GetApiResponse(string apiEndpoint)
        {
            var context = new ApplicationDbContext();
            var stopFetchingRecordsAt = 250;
            var maxRecordLimit = 50;
            var fullUrl = apiPrefix + apiEndpoint + "?limit=" + maxRecordLimit;

            var tokenModel = GetToken();
            if (!tokenModel.IsSuccessful)
            {
                return null;
            }

            var token = tokenModel.Token;

            var apiBaseUrl = context.Users.FirstOrDefault(x => x.Id == UserId).ApiBaseUrl;

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

                    if (responseArray.Count() != maxRecordLimit  || finalResponse.Count() >= stopFetchingRecordsAt)
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