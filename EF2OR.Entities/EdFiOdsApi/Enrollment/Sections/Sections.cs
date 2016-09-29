using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Entities.EdFiOdsApi.Enrollment.Sections
{
    public class Sections: BaseEdFiOdsData
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
    }

    public class Student
    {
        public string studentSectionAssociation_id { get; set; }
        public string id { get; set; }
        public string studentUniqueId { get; set; }
    }


}
