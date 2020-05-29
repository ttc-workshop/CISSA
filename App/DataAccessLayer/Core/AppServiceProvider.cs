using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using NPOI.HSSF.Record.Formula.Functions;
using Raven.Abstractions.Extensions;

namespace Intersoft.CISSA.DataAccessLayer.Core
{
    public class ServiceDefInfo
    {
        public object Service { get; private set; }
        public bool IsDependent { get; private set; }

        public ServiceDefInfo(object service, bool dependent)
        {
            Service = service;
            IsDependent = dependent;
        }
    }

    public class AppServiceProvider : IAppServiceProvider, IAppServiceProviderRegistrator
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<Type, Func<object, object>> _typeFactoryFuncs = new ConcurrentDictionary<Type, Func<object, object>>();
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<Type, Func<object, object, ServiceDefInfo>> _typeFactoryOneArgFuncs = new ConcurrentDictionary<Type, Func<object, object, ServiceDefInfo>>();
        public static ConcurrentDictionary<Type, Func<object, object>> TypeFactoryFuncs { get { return _typeFactoryFuncs; } }
        public static ConcurrentDictionary<Type, Func<object, object, ServiceDefInfo>> TypeFactory1ArgFuncs { get { return _typeFactoryOneArgFuncs; } }

        static AppServiceProvider()
        {
//            _typeFactoryFuncs.Add(typeof(IDataContext), GetDataContext);
//            _typeFactoryFuncs.Add(typeof(IUserDataProvider), GetUserDataProvider);
        }

        public static void SetServiceFactoryFunc(Type type, Func<object, object> func)
        {
            Func<object, object> f;
            if (_typeFactoryFuncs.ContainsKey(type))
                _typeFactoryFuncs.TryRemove(type, out f);
            _typeFactoryFuncs.TryAdd(type, func);
        }
        public static void SetServiceFactoryFunc(Type type, Func<object, object, ServiceDefInfo> func)
        {
            Func<object, object, ServiceDefInfo> f;
            if (_typeFactoryOneArgFuncs.ContainsKey(type))
                _typeFactoryOneArgFuncs.TryRemove(type, out f);
            _typeFactoryOneArgFuncs.TryAdd(type, func);
        }

        /*public IDataContext DataContext { get; private set; }
        public AppServiceProvider(IDataContext dataContext)
        {
            DataContext = dataContext;

            _services.Add(DataContext);
        }*/

        private readonly IList<object> _externalServices = new List<object>();
        public AppServiceProvider(params object[] services)
        {
            _serviceLock.AcquireWriterLock(LockTimeout);
            try
            {
                _externalServices.AddRange(services);
            }
            finally
            {
                _serviceLock.ReleaseWriterLock();
            }
        }

        /*private static object GetUserDataProvider(object appContext)
        {
            var provider = appContext as IUserDataProvider;
            if (provider == null)
                throw new ApplicationException("Cannot get IUserDataProvider");

            return provider;
        }*/

        private readonly IList<object> _noArgServices = new List<object>();
        //public IList<object> Services { get { return _noArgServices; } }

        private readonly IDictionary<object, object> _oneArgServices = new Dictionary<object, object>();
        private readonly ReaderWriterLock _serviceLock = new ReaderWriterLock();

        public void Dispose()
        {
            foreach (var disposable in _noArgServices.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            _noArgServices.Clear();
            foreach (var disposable in _oneArgServices.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            _oneArgServices.Clear();
        }

        public T Find<T>() where T: class
        {
            T service;
            _serviceLock.AcquireReaderLock(LockTimeout);
            try
            {
                service = _noArgServices.OfType<T>().FirstOrDefault();
                if (service != null) return service;

                service = _externalServices.OfType<T>().FirstOrDefault();
                if (service != null) return service;
            }
            finally
            {
                _serviceLock.ReleaseReaderLock();
            }

            service = this as T;
            if (service != null) return service;

            Func<object, object> serviceFunc;

            if (_typeFactoryFuncs.TryGetValue(typeof (T), out serviceFunc) && serviceFunc != null)
            {
                service = (T) serviceFunc.Invoke(this);
                if (service != null)
                {
                    _serviceLock.AcquireWriterLock(LockTimeout);
                    try
                    {
                        _noArgServices.Add(service);
                    }
                    finally
                    {
                        _serviceLock.ReleaseWriterLock();
                    }
                }
                return service;
            }

            return null; //default(T);
        }

        public const int LockTimeout = 500000;

        public T Find<T>(object arg) where T : class
        {
            _serviceLock.AcquireReaderLock(LockTimeout);
            try
            {
                if (arg != null)
                {
                    var service =
                        _oneArgServices.Where(p => p.Key is T && p.Value == arg)
                            .Select(p => p.Key as T)
                            .FirstOrDefault();
                    if (service != null) return service;
                }
                if (arg == null)
                {
                    var service = _noArgServices.OfType<T>().FirstOrDefault();
                    if (service != null) return service;

                    service = _externalServices.OfType<T>().FirstOrDefault();
                    if (service != null) return service;

                    service = this as T;
                    if (service != null) return service;
                }
            }
            finally
            {
                _serviceLock.ReleaseReaderLock();
            }
            
            Func<object, object, ServiceDefInfo> serviceFunc;
            if (_typeFactoryOneArgFuncs.TryGetValue(typeof (T), out serviceFunc) && serviceFunc != null)
            {
                var serviceDef = serviceFunc.Invoke(this, arg);
                if (serviceDef != null && serviceDef.Service != null)
                {
                    _serviceLock.AcquireWriterLock(LockTimeout);
                    try
                    {
                        if (serviceDef.IsDependent)
                            _oneArgServices.Add(serviceDef.Service, arg);
                        else
                            _noArgServices.Add(serviceDef.Service);
                    }
                    finally
                    {
                        _serviceLock.ReleaseWriterLock();
                    }
                    return (T) serviceDef.Service;
                }
            }
            return null; //default(T);
        }

        public T Get<T>() where T: class
        {
            var service = Find<T>();
            if (service != null) return service;

            throw new ApplicationException(String.Format("Service of \"{0}\" type not found!", typeof(T).Name));
        }
        public T Get<T>(object arg) where T : class
        {
            var service = Find<T>(arg);
            if (service != null) return service;

            throw new ApplicationException(String.Format("Service({1}) of \"{0}\" type not found!", typeof(T).Name, arg != null ? arg.GetType().Name : "null"));
        }

        public void AddService(object service)
        {
            _serviceLock.AcquireWriterLock(LockTimeout);
            try
            {
                _externalServices.Add(service);
            }
            finally
            {
                _serviceLock.ReleaseWriterLock();
            } 
        }

        public int GetServiceCount()
        {
            _serviceLock.AcquireReaderLock(LockTimeout);
            try
            {
                return _externalServices.Count + _noArgServices.Count + _oneArgServices.Count;
            }
            finally
            {
                _serviceLock.ReleaseReaderLock();
            }
        }
    }
}
