using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.Tests
{
    public class GetClassMethods
    {
        public class MethodInfo
        {
            public string NamespaceName { get; set; }
            public string ClassName { get; set; }
            public string MethodName { get; set; }
        }

        public List<MethodInfo> GetMethodInfoList(string path)
        {
            var pathType = Type.GetType(path);
            return GetMethodInfoList(pathType);
        }

        public List<MethodInfo> GetMethodInfoList(Type pathType)
        {
            var list = new List<MethodInfo>();

            if (pathType.IsClass)
            {
                foreach(var m in pathType.GetMethods())
                {
                    list.AddRange(GetMethodInfoList(m.GetType()));
                }
            }
            else if (pathType.IsPublic)
            {
                list.Add(new MethodInfo{NamespaceName = pathType.Namespace});
            }
            return list;
        }
    }
}
