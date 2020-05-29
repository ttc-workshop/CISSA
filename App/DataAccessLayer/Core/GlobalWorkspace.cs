using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Reflection;

namespace Intersoft.CISSA.DataAccessLayer.Core
{
    public static class GlobalWorkspace
    {
        private static readonly object Locker = new object();
        private static MetadataWorkspace _workSpace = null;

        public static MetadataWorkspace WorkSpace
        {
            get
            {
                lock (Locker)
                {
                    if (_workSpace == null)
                    {
                        const string assemblyQualifiedName = @"Intersoft.CISSA.DataAccessLayer";
                        Type anyModelType = Type.GetType(assemblyQualifiedName);
                        Assembly modelAssembly = Assembly.GetAssembly(anyModelType);

                        Assembly[] assemblys = { modelAssembly };
                        _workSpace = new MetadataWorkspace(new[] { @"res://*/Model.Data.DataModel.*" }, assemblys);
                    }
                }
                return _workSpace;
            }
        }
    }
}
