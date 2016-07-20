using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EF2OR.Tests.Utils
{
    internal class FakeHttpSession : HttpSessionStateBase
    {
        internal static Dictionary<string, object> m_Session = new Dictionary<string, object>();
        public override object this[string name]
        {
            get
            {
                return m_Session[name];
            }
            set
            {
                m_Session[name] = value;
            }
        }
    }

    internal class SessionHelper
    {
        public static FakeHttpSession Session { get; set; }
        internal static void Initialize()
        {
            Session = new FakeHttpSession();
        }
    }
}
