using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Utils
{
    public static class DataConverter
    {
        public static MethodInfo GetTryParseMethod(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
            var parameterTypes = new Type[] { typeof(string), type.MakeByRefType() };

            return type.GetMethod("TryParse", bindingFlags, null, parameterTypes, null);
        }

        public static bool TryParse(this string s, Type type, out object result)
        {
            result = null;

            var method = GetTryParseMethod(type);
            if (method == null)
                throw new ApplicationException(String.Format("Метод конвертации для типа {0} не найден", type.Name));

            var parameters = new object[] { s, null };
            var success = (bool) method.Invoke(null, parameters);
            if (success)
                result = parameters[1];
            return success;
        }

        public static bool TryParse<T1, T2>(this T1 value, out T2 result)
        {
            result = default(T2);
            object tempResult;
            
            var v = (object) value;
            if (v == null) return true;

            var success = TryParse(v.ToString(), typeof(T2), out tempResult);
            if (success)
                result = (T2)tempResult;
            return success;
        }
    }
}
