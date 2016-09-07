using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Entities.EdFiOdsApi.Enrollment.Students
{

    public class Students: BaseEdFiOdsData
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public string id { get; set; }
        public string studentUniqueId { get; set; }
        public string personalTitlePrefix { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastSurname { get; set; }
        public string loginId { get; set; }
        public string sexType { get; set; }
        public string birthDate { get; set; }
        public string hispanicLatinoEthnicity { get; set; }
        public string economicDisadvantaged { get; set; }
        public string limitedEnglishProficiencyDescriptor { get; set; }
        public Address[] addresses { get; set; }
        public Characteristic[] characteristics { get; set; }
        public Cohortyear[] cohortYears { get; set; }
        public Electronicmail[] electronicMails { get; set; }
        public Identificationcode[] identificationCodes { get; set; }
        public Indicator[] indicators { get; set; }
        public Language[] languages { get; set; }
        public Programparticipation[] programParticipations { get; set; }
        public Race[] races { get; set; }
        public Telephone[] telephones { get; set; }
        public Schoolassociation[] schoolAssociations { get; set; }
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

    public class Characteristic
    {
        public string studentCharacteristicDescriptor { get; set; }
    }

    public class Cohortyear
    {
        public string cohortYearType { get; set; }
        public string schoolYear { get; set; }
    }

    public class Electronicmail
    {
        public string electronicMailType { get; set; }
        public string electronicMailAddress { get; set; }
    }

    public class Identificationcode
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

    public class Language
    {
        public string languageDescriptor { get; set; }
    }

    public class Programparticipation
    {
        public string programType { get; set; }
        public string beginDate { get; set; }
        public string endDate { get; set; }
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

    public class Schoolassociation
    {
        public string id { get; set; }
        public string gradeLevel { get; set; }
    }

}
