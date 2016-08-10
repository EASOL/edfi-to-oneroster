using EF2OR.Utils;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Providers
{
    internal class WebUserProvider : IUserProvider
    {
        string IUserProvider.IpAddress
        {
            get
            {
                return CommonUtils.HttpContextProvider.Current.Request.UserHostAddress;
            }

        }

        string IUserProvider.UserId
        {
            get
            {
                var context = CommonUtils.HttpContextProvider.Current;
                return context.User.Identity.GetUserId();
            }
        }

        string IUserProvider.UserName
        {
            get
            {
                return CommonUtils.HttpContextProvider.Current.User.Identity.Name;
            }
        }
    }
}
