using System;
using System.Linq;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    public static class DocHelper
    {
        public static Doc Assign(this Doc source, Doc result)
        {
            if (source == null) return null;

            if (result == null || result.DocDef == null || result.DocDef.Attributes == null)
                return source;

            foreach (var attr in result.Attributes)
            {
                var sourceAttr = source.Attributes.FirstOrDefault(a => a.AttrDef.Id == attr.AttrDef.Id && a.AttrDef.Type.Id == attr.AttrDef.Type.Id) ??
                                    source.Attributes.FirstOrDefault(a => String.Equals(a.AttrDef.Name, attr.AttrDef.Name, StringComparison.OrdinalIgnoreCase) && a.AttrDef.Type.Id == attr.AttrDef.Type.Id);

                if (sourceAttr != null)
                    sourceAttr.ObjectValue = attr.ObjectValue;
            }
            return source;
        }

        public static bool AttrIsSame(this Doc source, Doc doc, string attrName)
        {
            if (source == null || doc == null) return false;

            var attr1 = source[attrName];
            var attr2 = doc[attrName];
            if (attr1 == null && attr2 == null) return true;
            if (attr1 == null || attr2 == null) return false;

            return attr1.Equals(attr2);
        }

/*        public static bool AttrIsSame(this Doc source, Doc doc, string attrName, IEqualityComparer comparer, Func<object, object> prepareFunc)
        {
            if (source == null || doc == null) return false;

            var attr1 = source[attrName];
            var attr2 = doc[attrName];

            if (prepareFunc != null)
            {
                attr1 = prepareFunc(attr1);
                attr2 = prepareFunc(attr2);
            }
            return comparer.Equals(attr1, attr2);
        }*/

        public static bool AttrIsSame(this Doc source, Doc doc, string attrName, Func<object, object, bool> compareFunc, Func<object, object> prepareFunc)
        {
            if (source == null || doc == null) return false;

            var attr1 = source[attrName];
            var attr2 = doc[attrName];

            if (prepareFunc != null)
            {
                attr1 = prepareFunc(attr1);
                attr2 = prepareFunc(attr2);
            }
            return compareFunc(attr1, attr2);
        }

        public static bool AttrIsSame(this Doc source, Doc doc, params string[] attrNames)
        {
            if (source == null || doc == null) return false;

            return attrNames.All(attrName => AttrIsSame(source, doc, attrName));
        }

/*        public static bool AttrIsSame(this Doc source, Doc doc, string[] attrNames, IEqualityComparer comparer, Func<object, object> prepareFunc)
        {
            if (source == null || doc == null) return false;

            return attrNames.All(attrName => AttrIsSame(source, doc, attrName, comparer, prepareFunc));
        }*/

        public static bool AttrIsSame(this Doc source, Doc doc, string[] attrNames, Func<object, object, bool> compareFunc, Func<object, object> prepareFunc)
        {
            if (source == null || doc == null) return false;

            return attrNames.All(attrName => AttrIsSame(source, doc, attrName, compareFunc, prepareFunc));
        }
    }
}
