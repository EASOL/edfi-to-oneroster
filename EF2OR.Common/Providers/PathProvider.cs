using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Common.Providers
{
    public class PathProvider : IPathProvider
    {
        public string MapPath(string path)
        {
            return System.Web.HttpContext.Current.Server.MapPath(path);
        }
    }
}
