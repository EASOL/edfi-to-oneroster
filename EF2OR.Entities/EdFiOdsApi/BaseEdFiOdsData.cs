using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Entities.EdFiOdsApi
{
    public class BaseEdFiOdsData : IEdFiOdsData
    {
        public bool TokenExpired { get; set; }
    }
}
