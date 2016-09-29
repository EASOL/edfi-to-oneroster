using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Entities.EdFiOdsApi.Enrollment.Schools
{
    public class Schools : BaseEdFiOdsData
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public string id { get; set; }
        public int schoolId { get; set; }
        public string stateOrganizationId { get; set; }
        public string nameOfInstitution { get; set; }
        public string type { get; set; }
        public Address[] addresses { get; set; }
        public Educationorganizationcategory[] educationOrganizationCategories { get; set; }
        public object[] identificationCodes { get; set; }
        public Institutiontelephone[] institutionTelephones { get; set; }
        public object[] internationalAddresses { get; set; }
        public Schoolcategory[] schoolCategories { get; set; }
        public Gradelevel[] gradeLevels { get; set; }
        public Localeducationagencyreference localEducationAgencyReference { get; set; }
    }

    public class Localeducationagencyreference
    {
        public string id { get; set; }
        public int localEducationAgencyId { get; set; }
    }

    public class Address
    {
        public string addressType { get; set; }
        public string streetNumberName { get; set; }
        public string city { get; set; }
        public string stateAbbreviationType { get; set; }
        public string postalCode { get; set; }
    }

    public class Educationorganizationcategory
    {
        public string type { get; set; }
    }

    public class Institutiontelephone
    {
        public string institutionTelephoneNumberType { get; set; }
        public string telephoneNumber { get; set; }
    }

    public class Schoolcategory
    {
        public string type { get; set; }
    }

    public class Gradelevel
    {
        public string gradeLevelDescriptor { get; set; }
    }

}
