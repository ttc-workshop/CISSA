using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    public static class Utils
    {
        public static string ExtractAttributePath(string attribute)
        {
            var i = attribute.LastIndexOf('.');
            return i > 0 ? attribute.Substring(0, i) : String.Empty;
        }

        public static string ExtractAttributePathRoot(string attribute)
        {
            var i = attribute.IndexOf('.');
            return i > 0 ? attribute.Substring(0, i) : String.Empty;
        }

        public static string ExtractAttributeSubPath(string attribute)
        {
            var i = attribute.IndexOf('.');
            return i > 0 ? attribute.Substring(i + 1, attribute.Length - (i + 1)) : attribute;
        }

        public static string ExtractAttributeName(string attribute)
        {
            var i = attribute.LastIndexOf('.');
            return i > 0 ? attribute.Substring(i + 1, attribute.Length - (i + 1)) : attribute;
        }

        public static bool AttributeHasPath(string attribute)
        {
            return attribute.IndexOf('.') > 0;
        }

        //        public static string Extract
    }
}
