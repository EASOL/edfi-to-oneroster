using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Entities.EdFiOdsApi.Enrollment.Staffs
{

    public class Staffs : BaseEdFiOdsData
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public string studentUniqueId;
        public string middleName;

        public string id { get; set; }
        public string staffUniqueId { get; set; }
        public string personalTitlePrefix { get; set; }
        public string firstName { get; set; }
        public string lastSurname { get; set; }
        public string loginId { get; set; }
        public string sexType { get; set; }
        public DateTime birthDate { get; set; }
        public bool hispanicLatinoEthnicity { get; set; }
        public object[] addresses { get; set; }
        public Electronicmail[] electronicMails { get; set; }
        public Identificationcode[] identificationCodes { get; set; }
        public object[] languages { get; set; }
        public object[] races { get; set; }
        public Telephone[] telephones { get; set; }
        public Classification[] classifications { get; set; }
    }

    public class  Telephone
    {
        public string telephoneNumber;
        public string telephoneNumberType;

        public string orderOfPriority { get; set; }
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

    public class Classification
    {
        public string classification { get; set; }
    }

}
