using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Entities.EdFiOdsApi
{

    public class TermDescriptors: BaseEdFiOdsData
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public string id { get; set; }
        public int termDescriptorId { get; set; }
        public string _namespace { get; set; }
        public string codeValue { get; set; }
        public string shortDescription { get; set; }
        public string description { get; set; }
        public string termType { get; set; }
        public string _etag { get; set; }
    }

}
