using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace meitubikanSite.Controllers
{
    public class ControllerHelper
    {
        public static string NormalizeString(string str)
        {
            return str == null ? null : str.ToLower().Trim();
        }
    }
}