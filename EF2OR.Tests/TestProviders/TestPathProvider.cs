using EF2OR.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Tests.TestProviders
{
    internal class TestPathProvider : IPathProvider
    {

        string IPathProvider.MapPath(string path)
        {
            return System.IO.Path.Combine(Environment.CurrentDirectory, "FakeMappedPAth", path);
        }
    }
}
