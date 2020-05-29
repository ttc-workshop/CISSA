using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Languages;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class LanguageRepository : ILanguageRepository
    {
        public IDataContext DataContext { get; private set; }
//        private readonly bool _ownDataContext;

        public LanguageRepository(IDataContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            /*{
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else*/
                DataContext = dataContext;
        }
//        public LanguageRepository() : this((IDataContext) null) { }

        public LanguageRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            DataContext = dataContext;  //provider.Get<IDataContext>();
        }

        public static IList<LanguageType> LanguageCache;
        private static bool _languagesLoaded;
        private const int LockTimeout = 500000;
        private static readonly ReaderWriterLock LanguageCacheLock = new ReaderWriterLock();
        public static IDictionary<int, ObjectCache<String>> LangTranslationCaches = new Dictionary<int, ObjectCache<string>>();
        // private static readonly object LangTranslationCacheLock = new object();
        private static readonly ReaderWriterLock LangTranslationCacheLock = new ReaderWriterLock();

        public IList<LanguageType> Load()
        {
            LanguageCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                if (!_languagesLoaded)
                {
                    var lc = LanguageCacheLock.UpgradeToWriterLock(LockTimeout);
                    try
                    {
                        if (!_languagesLoaded)
                        {
                            // DONE: Добавить инициализацию поля CultureName
                            LanguageCache =
                                DataContext.GetEntityDataContext()
                                    .Entities.Languages.Select(
                                        l => new LanguageType {Id = l.Id, Name = l.Name, CultureName = l.Culture_Name})
                                    .ToList();
                            _languagesLoaded = true;
                        }
                    }
                    finally
                    {
                        LanguageCacheLock.DowngradeFromWriterLock(ref lc);
                    }
                }
                return
                    LanguageCache.Select(i =>
                        new LanguageType
                        {
                            Id = i.Id,
                            Name = i.Name,
                            CultureName = i.CultureName
                        })
                        .ToList();
            }
            finally
            {
                LanguageCacheLock.ReleaseReaderLock();
            }
        }

        public string GetTranslation(Guid defId, int languageId)
        {
            // lock (LangTranslationCacheLock)
            LangTranslationCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                ObjectCache<String> cache;
                if (LangTranslationCaches.TryGetValue(languageId, out cache))
                {
                    var item = cache.Find(defId);
                    if (item != null)
                        return item.CachedObject;
                }

                var lc = LangTranslationCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    if (LangTranslationCaches.TryGetValue(languageId, out cache)) // 
                    {
                        var item = cache.Find(defId);
                        if (item != null)
                            return item.CachedObject;
                    }

                    var s =
                        DataContext.GetEntityDataContext()
                            .Entities.Object_Def_Translations.Where(
                                t => t.Def_Id == defId && t.Language_Id == languageId)
                            .Select(
                                t => t.Data_Text).FirstOrDefault();

                    if (!LangTranslationCaches.ContainsKey(languageId))
                        LangTranslationCaches.Add(languageId, cache = new ObjectCache<string>());

                    if (cache != null) cache.Add(s, defId);

                    return s;
                }
                finally
                {
                    LangTranslationCacheLock.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                LangTranslationCacheLock.ReleaseReaderLock();
            }
        }

        public static void ClearCache()
        {
            // lock(LangTranslationCacheLock)
            LangTranslationCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                LangTranslationCaches.Clear();
            }
            finally
            {
                LangTranslationCacheLock.ReleaseWriterLock();
            }
        }
/*
        public void Dispose()
        {
            if (_ownDataContext && DataContext != null)
            {
                try
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog("LangRepo", e, "LanguageRepository.Dispose");
                    throw;
                }
            }
        }

        ~LanguageRepository()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "LanguageRepository.Finalize");
                    throw;
                }
        }*/
    }
}
