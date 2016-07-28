using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Providers
{
    internal interface IUserProvider
    {
        string IpAddress { get; }
        string UserId { get; }
        string UserName { get; }
    }
}
