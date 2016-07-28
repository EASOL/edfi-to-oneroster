using EF2OR.Providers;
using EF2OR.Tests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Tests.TestProviders
{
    internal class TestUserProvider : IUserProvider
    {
        string IUserProvider.IpAddress
        {
            get
            {
                return "-1";
            }
        }

        string IUserProvider.UserId
        {
            get
            {
                return AuthenticationHelper.TestUser.Id;
            }
        }

        string IUserProvider.UserName
        {
            get
            {
                return EF2OR.Tests.Utils.AuthenticationHelper.TESTUSER_USERNAME;
            }
        }
    }
}
