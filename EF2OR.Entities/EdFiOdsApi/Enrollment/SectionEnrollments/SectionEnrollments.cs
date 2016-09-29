using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Entities.EdFiOdsApi.Enrollment.SectionEnrollments
{

    public class Rootobject
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public string id { get; set; }
        public string uniqueSectionCode { get; set; }
        public string sequenceOfCourse { get; set; }
        public string educationalEnvironmentType { get; set; }
        public string availableCredits { get; set; }
        public string academicSubjectDescriptor { get; set; }
        public Sessionreference sessionReference { get; set; }
        public Classperiodreference classPeriodReference { get; set; }
        public Courseofferingreference courseOfferingReference { get; set; }
        public Locationreference locationReference { get; set; }
        public Schoolreference schoolReference { get; set; }
        public Staff[] staff { get; set; }
        public Student[] students { get; set; }
    }

    public class Sessionreference
    {
        public string id { get; set; }
        public string schoolId { get; set; }
        public string schoolYear { get; set; }
        public string termDescriptor { get; set; }
        public string sessionName { get; set; }
        public string beginDate { get; set; }
        public string endDate { get; set; }
        public string totalInstructionalDays { get; set; }
    }

    public class Classperiodreference
    {
        public string id { get; set; }
        public string schoolId { get; set; }
        public string name { get; set; }
    }

    public class Courseofferingreference
    {
        public string id { get; set; }
        public string localCourseCode { get; set; }
        public string schoolId { get; set; }
        public string schoolYear { get; set; }
        public string termDescriptor { get; set; }
    }

    public class Locationreference
    {
        public string id { get; set; }
        public string schoolId { get; set; }
        public string classroomIdentificationCode { get; set; }
    }

    public class Schoolreference
    {
        public string id { get; set; }
        public string schoolId { get; set; }
    }

    public class Staff
    {
        public string staffSectionAssociation_id { get; set; }
        public string id { get; set; }
        public string staffUniqueId { get; set; }
        public string personalTitlePrefix { get; set; }
        public string firstName { get; set; }
        public string lastSurname { get; set; }
        public string sexType { get; set; }
        public string birthDate { get; set; }
        public string hispanicLatinoEthnicity { get; set; }
        public Address[] addresses { get; set; }
        public Electronicmail[] electronicMails { get; set; }
        public Identificationcode[] identificationCodes { get; set; }
        public Language[] languages { get; set; }
        public Race[] races { get; set; }
        public Telephone[] telephones { get; set; }
        public Classification[] classifications { get; set; }
    }

    public class Address
    {
        public string addressType { get; set; }
        public string streetNumberName { get; set; }
        public string city { get; set; }
        public string stateAbbreviationType { get; set; }
        public string postalCode { get; set; }
        public string nameOfCounty { get; set; }
    }

    public class Electronicmail
    {
        public string electronicMailType { get; set; }
        public string electronicMailAddress { get; set; }
    }

    public class Identificationcode
    {
        public string staffIdentificationSystemDescriptor { get; set; }
        public string identificationCode { get; set; }
    }

    public class Language
    {
        public string languageDescriptor { get; set; }
    }

    public class Race
    {
        public string raceType { get; set; }
    }

    public class Telephone
    {
        public string telephoneNumberType { get; set; }
        public string orderOfPriority { get; set; }
        public string telephoneNumber { get; set; }
    }

    public class Classification
    {
        public string classification { get; set; }
    }

    public class Student
    {
        public string studentSectionAssociation_id { get; set; }
        public string id { get; set; }
        public string studentUniqueId { get; set; }
        public string personalTitlePrefix { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastSurname { get; set; }
        public string sexType { get; set; }
        public string birthDate { get; set; }
        public string hispanicLatinoEthnicity { get; set; }
        public string economicDisadvantaged { get; set; }
        public string limitedEnglishProficiencyDescriptor { get; set; }
        public Address1[] addresses { get; set; }
        public Characteristic[] characteristics { get; set; }
        public Cohortyear[] cohortYears { get; set; }
        public Electronicmail1[] electronicMails { get; set; }
        public Identificationcode1[] identificationCodes { get; set; }
        public Indicator[] indicators { get; set; }
        public Language1[] languages { get; set; }
        public Programparticipation[] programParticipations { get; set; }
        public Race1[] races { get; set; }
        public Telephone1[] telephones { get; set; }
        public Schoolassociation[] schoolAssociations { get; set; }
    }

    public class Address1
    {
        public string addressType { get; set; }
        public string streetNumberName { get; set; }
        public string city { get; set; }
        public string stateAbbreviationType { get; set; }
        public string postalCode { get; set; }
        public string nameOfCounty { get; set; }
    }

    public class Characteristic
    {
        public string studentCharacteristicDescriptor { get; set; }
    }

    public class Cohortyear
    {
        public string cohortYearType { get; set; }
        public string schoolYear { get; set; }
    }

    public class Electronicmail1
    {
        public string electronicMailType { get; set; }
        public string electronicMailAddress { get; set; }
    }

    public class Identificationcode1
    {
        public string assigningOrganizationIdentificationCode { get; set; }
        public string studentIdentificationSystemDescriptor { get; set; }
        public string identificationCode { get; set; }
    }

    public class Indicator
    {
        public string indicatorName { get; set; }
        public string indicator { get; set; }
    }

    public class Language1
    {
        public string languageDescriptor { get; set; }
    }

    public class Programparticipation
    {
        public string programType { get; set; }
        public string beginDate { get; set; }
        public string endDate { get; set; }
    }

    public class Race1
    {
        public string raceType { get; set; }
    }

    public class Telephone1
    {
        public string telephoneNumberType { get; set; }
        public string orderOfPriority { get; set; }
        public string telephoneNumber { get; set; }
    }

    public class Schoolassociation
    {
        public string id { get; set; }
        public string gradeLevel { get; set; }
    }

}
