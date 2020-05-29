using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Security;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class FormRepository : IFormRepository, IDisposable
    {
        private IAppServiceProvider Provider { get; set; }
        public IDataContext DataContext { get; private set; }
        private readonly bool _ownDataContext;

        public Guid UserId { get; private set; }
        public IControlFactory Factory { get; private set; }
        private readonly ILanguageRepository _langRepo;

        private readonly IDocRepository _docRepo;
        private readonly IDocDefRepository _defRepo;
        private readonly IAttributeRepository _attrRepo;
        private readonly IUserRepository _userRepo;
        private readonly IOrgRepository _orgRepo;
        private readonly IPermissionRepository _permissionRepo;
        private readonly IDocStateRepository _docStateRepo;
        private readonly IEnumRepository _enumRepo;
        private readonly ISqlQueryReaderFactory _sqlQueryReaderFactory;
        private readonly ISqlQueryBuilderFactory _sqlQueryBuilderFactory;
        private readonly IComboBoxEnumProvider _comboBoxValueProvider;

        public FormRepository(IDataContext dataContext, IDocRepository docRepo, Guid userId,
            IDocDefRepository defRepo = null, ILanguageRepository langRepo = null, IUserRepository userRepo = null)
        {
            if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else
                DataContext = dataContext;

            var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = providerFactory.Create();

            Factory = Provider.Get<IControlFactory>(DataContext);

            _docRepo = docRepo ?? Provider.Get<IDocRepository>();
            _defRepo = defRepo ?? Provider.Get<IDocDefRepository>();
            _langRepo = langRepo ?? Provider.Get<ILanguageRepository>();
            _userRepo = userRepo ?? Provider.Get<IUserRepository>();
            _orgRepo = Provider.Get<IOrgRepository>();
            _attrRepo = Provider.Get<IAttributeRepository>();
            _permissionRepo = Provider.Get<IPermissionRepository>();
            _docStateRepo = Provider.Get<IDocStateRepository>();
            _enumRepo = Provider.Get<IEnumRepository>();

            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            _comboBoxValueProvider = Provider.Get<IComboBoxEnumProvider>();
            
            /*Factory = new ControlFactory(DataContext, userId);

            _docRepo = docRepo ?? new DocRepository(DataContext);
            _defRepo = defRepo ?? new DocDefRepository(DataContext);
            _langRepo = langRepo ?? new LanguageRepository(DataContext);
            _userRepo = userRepo ?? new UserRepository(DataContext);
            _orgRepo = /*_userRepo.OrgRepo ??#1# new OrgRepository(DataContext);
            _attrRepo = new AttributeRepository(/*DataContext#1#);
            _permissionRepo = new PermissionRepository(DataContext);
            _docStateRepo = new DocStateRepository(DataContext);
            _enumRepo = new EnumRepository(DataContext);*/

            
            UserId = userId;
        }

        public FormRepository(IDataContext dataContext, Guid userId) : this(dataContext, null, userId) {}

//        public FormRepository(Guid userId) : this(null, userId) {}
        public FormRepository(IDataContext dataContext) : this(dataContext, Guid.Empty) { }
        public FormRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            Provider = provider;
            DataContext = dataContext; //provider.Get<IDataContext>();
            // var userDataProvider = provider.Get<IUserDataProvider>();
            UserId = Provider.GetCurrentUserId();

            Factory = Provider.Get<IControlFactory>(DataContext);

            _docRepo = Provider.Get<IDocRepository>();
            _defRepo = Provider.Get<IDocDefRepository>();
            _langRepo = Provider.Get<ILanguageRepository>();
            _userRepo = Provider.Get<IUserRepository>();
            _orgRepo = Provider.Get<IOrgRepository>();
            _attrRepo = Provider.Get<IAttributeRepository>();
            _permissionRepo = Provider.Get<IPermissionRepository>();
            _docStateRepo = Provider.Get<IDocStateRepository>();
            _enumRepo = Provider.Get<IEnumRepository>();

            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            _comboBoxValueProvider = Provider.Get<IComboBoxEnumProvider>();
        }

        private bool _permissionReady;
        private readonly PermissionSet _permissions = new PermissionSet();

        private PermissionSet Permissions
        {
            get 
            { 
                if (_permissionReady) return _permissions;
                if (!UserId.Equals(Guid.Empty))
                {
                    _permissions.UnionWith(_permissionRepo.GetUserPermissions(UserId));
                }
                _permissionReady = true;
                return _permissions;
            }
        }

        public static readonly ObjectCache<string> DetailFormCache = new ObjectCache<string>();
        public static readonly ObjectCache<string> TableFormCache = new ObjectCache<string>();
        // private readonly static object GetFormLock = new object();
        private static readonly ReaderWriterLock DetailFormCacheLock = new ReaderWriterLock();
        private static readonly ReaderWriterLock TableFormCacheLock = new ReaderWriterLock();
        private const int LockTimeout = 600000;

        public static void ClearCaches()
        {
            MenuCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                MenuCache.Clear();
            }
            finally
            {
                MenuCacheLock.ReleaseWriterLock();
            }
            DetailFormCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                DetailFormCache.Clear();
            }
            finally
            {
                DetailFormCacheLock.ReleaseWriterLock();
            }
            TableFormCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                TableFormCache.Clear();
            }
            finally
            {
                TableFormCacheLock.ReleaseWriterLock();
            }
        }

        private static IList<BizForm> _metadataForms;
        private static BizForm GetMetadataForm(Guid formId)
        {
            if (_metadataForms == null)
            {
                _metadataForms = new List<BizForm>(MetadataForms.GetOrganizationForms());
            }
            return _metadataForms.FirstOrDefault(f => f.Id == formId);
        }

        /// <summary>
        /// Получает форму для редактирования или создания документа
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <returns>Форма</returns>
        public BizForm GetForm(Guid formId)
        {
            /*lock (GetFormLock)
            {
                var cached = FormCache.Find(formId);
                if (cached != null)
                {
                    var fm = (BizForm)CheckFormPermissions(DeserializeControl(cached.CachedObject));
                    InitializeForm(fm);
                    return fm;
                }

                string xml;
                var metaForm = GetMetadataForm(formId);
                if (metaForm != null)
                {
                    xml = SerializeControl(metaForm);

                    FormCache.Add(xml, formId);

                    metaForm = (BizForm) CheckFormPermissions(metaForm);
                    InitializeForm(metaForm);

                    return metaForm;
                }

                var en = DataContext.GetEntityDataContext().Entities;
                var form = en.Object_Defs.OfType<Form>().FirstOrDefault(f => f.Id == formId);
                if (form != null)
                {
                    var frm = Factory.Create(form);

                    xml = SerializeControl(frm);
                    
                    FormCache.Add(xml, formId);

                    frm = CheckFormPermissions(frm);
                    InitializeForm((BizForm)frm);

                    return frm as BizDetailForm;
                }

                var tableForm = en.Object_Defs.OfType<Table_Form>().FirstOrDefault(f => f.Id == formId);
                if (tableForm != null)
                {
                    var frm = Factory.Create(tableForm);

                    xml = SerializeControl(frm);
                    
                    FormCache.Add(xml, formId);

                    frm = CheckFormPermissions(frm);
                    InitializeForm(frm as BizForm);

                    return frm as BizTableForm;
                }*/
            var form = FindForm(formId);

            if (form == null)
                throw new ApplicationException(string.Format("Форма с идентификатором {0} не существует.", formId));

            return form;
        }

        private void InitializeForm(BizForm frm)
        {
            frm.LanguageId = 0;

            var helper = new FormHelper(Provider, frm);
            helper.InitDependency();

            if (frm is BizDetailForm && frm.DocumentDefId != null)
            {
                var docDef = _defRepo.DocDefById((Guid) frm.DocumentDefId);
                InitializeControls(frm, frm, docDef);
            }
            else
            {
                var form = frm as BizTableForm;
                if(form != null && form.DocumentDefId != null)
                {
                    if (form.Children == null)
                        form.Children = new List<BizControl>();

                    if (form.Children.Count == 0)
                    {
                        if (form.FormId != null || form.FilterFormId != null)
                        {
                            var detailForm = GetDetailForm((Guid) (form.FormId ?? form.FilterFormId));

                            foreach (var child in detailForm.Children.Where(c => !(c is BizDocumentListForm)))
                                form.Children.Add(CloneControl(child));
                        }
                    }
                    var docDef = _defRepo.DocDefById((Guid) form.DocumentDefId);
                    InitializeControls(form, form, docDef);
                    if (form.PageSize == 0)
                        form.PageSize = 10;
                }
            }
        }

        public BizDetailForm FindDetailForm(Guid formId)
        {
            // lock (GetFormLock)
            DetailFormCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cached = DetailFormCache.Find(formId);
                if (cached != null)
                {
                    var fm = CheckFormPermissions(DeserializeControl(cached.CachedObject));
                    InitializeForm(fm as BizForm);
                    return (BizDetailForm) fm;
                }

                string xml;
                var metaForm = GetMetadataForm(formId) as BizDetailForm;
                if (metaForm != null)
                {
                    xml = SerializeControl(metaForm);

                    var mlc = DetailFormCacheLock.UpgradeToWriterLock(LockTimeout);
                    try
                    {
                        DetailFormCache.Add(xml, formId);
                    }
                    finally
                    {
                        DetailFormCacheLock.DowngradeFromWriterLock(ref mlc);
                    }
                    metaForm = (BizDetailForm) CheckFormPermissions(metaForm);
                    InitializeForm(metaForm);

                    return metaForm;
                }
                BizControl frm;
                var lc = DetailFormCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cached = DetailFormCache.Find(formId);
                    if (cached == null)
                    {
                        var en = DataContext.GetEntityDataContext().Entities;
                        var form = en.Object_Defs.OfType<Form>().FirstOrDefault(f => f.Id == formId);

                        if (form == null) return null;

                        frm = Factory.Create(form);

                        xml = SerializeControl(frm);

                        DetailFormCache.Add(xml, formId);
                    }
                    else
                        frm = DeserializeControl(cached.CachedObject);
                }
                finally
                {
                    DetailFormCacheLock.DowngradeFromWriterLock(ref lc);
                }

                frm = CheckFormPermissions(frm);
                InitializeForm(frm as BizForm);

                return frm as BizDetailForm;
            }
            finally
            {
                DetailFormCacheLock.ReleaseReaderLock();
            }
        }

        public BizTableForm FindTableForm(Guid formId)
        {
            // lock (GetFormLock)
            TableFormCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cached = TableFormCache.Find(formId);
                if (cached != null)
                {
                    var fm = CheckFormPermissions(DeserializeControl(cached.CachedObject));
                    InitializeForm(fm as BizTableForm);
                    return (BizTableForm) fm;
                }

                string xml;
                var metaForm = GetMetadataForm(formId) as BizTableForm;
                if (metaForm != null)
                {
                    xml = SerializeControl(metaForm);

                    var mlc = TableFormCacheLock.UpgradeToWriterLock(LockTimeout);
                    try
                    {
                        TableFormCache.Add(xml, formId);
                    }
                    finally
                    {
                        TableFormCacheLock.DowngradeFromWriterLock(ref mlc);
                    }

                    metaForm = (BizTableForm) CheckFormPermissions(metaForm);
                    InitializeForm(metaForm);

                    return metaForm;
                }
                BizControl frm;
                var lc = TableFormCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cached = TableFormCache.Find(formId);
                    if (cached == null)
                    {
                        var en = DataContext.GetEntityDataContext().Entities;
                        var form = en.Object_Defs.OfType<Table_Form>().FirstOrDefault(f => f.Id == formId);
                        if (form == null) return null;

                        // var tableForm = (BizTableForm) CheckFormPermissions(ControlFactory.Create(form));

                        frm = Factory.Create(form);

                        xml = SerializeControl(frm);

                        TableFormCache.Add(xml, formId);
                    }
                    else
                        frm = DeserializeControl(cached.CachedObject);
                }
                finally
                {
                    TableFormCacheLock.DowngradeFromWriterLock(ref lc);
                }
                frm = CheckFormPermissions(frm);
                InitializeForm(frm as BizForm);

                return frm as BizTableForm;
            }
            finally
            {
                TableFormCacheLock.ReleaseReaderLock();
            }
        }

        public BizForm FindForm(Guid formId)
        {
            var detailtForm = FindDetailForm(formId);
            if (detailtForm != null) return detailtForm;

            var tableForm = FindTableForm(formId);
            return tableForm;
        }

        /// <summary>
        /// Получает форму для редактирования или создания документа
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Форма</returns>
        public BizForm GetForm(Guid formId, int languageId)
        {
            var form = GetForm(formId);

            if (form != null && languageId != 0) TranslateForm(form, languageId);

            return form;
        }

        /// <summary>
        /// Получает форму для редактирования или создания документа
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <returns>Форма</returns>
        public BizDetailForm GetDetailForm(Guid formId)
        {
            var form = FindDetailForm(formId);

            if (form == null)
                throw new ApplicationException(String.Format("Форма с идентификатором {0} не существует.", formId));

            return form;
        }

        public BizDetailForm GetDetailForm(Guid formId, int languageId)
        {
            var form = GetDetailForm(formId);

            if (form != null && languageId != 0) TranslateForm(form, languageId);

            return form;
        }

        /// <summary>
        /// Получает табличную форму
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <returns>Табличная форма</returns>
        public BizTableForm GetTableForm(Guid formId)
        {
            var form = FindTableForm(formId);

            if (form == null)
                throw new ApplicationException(String.Format("Формы с идентификатором {0} не существует.", formId));

            return form;
        }

        public BizTableForm GetTableForm(Guid formId, int languageId)
        {
            var form = GetTableForm(formId);

            if (form != null && languageId != 0) TranslateForm(form, languageId);

            return form;
        }

        /// <summary>
        /// Возвращает детальную форму с данными
        /// </summary>
        /// <param name="formId">Id формы</param>
        /// <param name="documentId">Документ</param>
        /// <param name="languageId">Язык</param>
        /// <returns></returns>
        public BizDetailForm GetDetailFormWithData(Guid formId, Guid documentId, int languageId = 0)
        {
            var form = GetDetailForm(formId, languageId);

            var doc = _docRepo.LoadById(documentId);

            if (doc != null)
                SetFormDoc(form, doc);

            return form;
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(form, docStateId, filter, sortAttrs, UserId, DataContext))
            using (var query = sqb.Build(form, docStateId, filter, sortAttrs))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo*pageSize;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    count = reader.GetCount();
                    var hasPageSize = pageSize > 0;

                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;
                        rows.Add(SetFormDoc(formRow, reader));

                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }
        public List<BizControl> GetTableFormRows(BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(form, docStateId, filter, sortAttrs, UserId, DataContext))
            using (var query = sqb.Build(form, docStateId, filter, sortAttrs))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo * pageSize;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    var hasPageSize = pageSize > 0;

                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;
                        rows.Add(SetFormDoc(formRow, reader));

                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }
        public int GetTableFormRowCount(BizForm form, Guid? docStateId, BizForm filter)
        {
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(form, docStateId, filter, null, UserId, DataContext))
            using (var query = sqb.Build(form, docStateId, filter, null))
            {
                //            query.UserId = UserId;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    return reader.GetCount();
                }
            }
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, QueryDef def, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(def, form, sortAttrs, UserId, DataContext))
            using (var query = sqb.Build(def, form, sortAttrs))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo*pageSize;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    count = reader.GetCount();

                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, QueryDef def, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo,
            int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(def, form, filter, sortAttrs, UserId, DataContext))
            using (var query = sqb.Build(def, form, filter, sortAttrs))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo * pageSize;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    count = reader.GetCount();

                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }

        public List<BizControl> GetTableFormRows(BizForm form, QueryDef def, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(def, form, sortAttrs, UserId, DataContext))
            using (var query = sqb.Build(def, form, sortAttrs))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo * pageSize;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }
        public List<BizControl> GetTableFormRows(BizForm form, QueryDef def, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(def, form, filter, sortAttrs, UserId, DataContext))
            using (var query = sqb.Build(def, form, filter, sortAttrs))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo * pageSize;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }

        public int GetTableFormRowCount(BizForm form, QueryDef def)
        {
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(def, form, null, UserId, DataContext))
            using (var query = sqb.Build(def, form, null))
            {
                //            query.UserId = UserId;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    return reader.GetCount();
                }
            }
        }
        public int GetTableFormRowCount(BizForm form, QueryDef def, BizForm filter)
        {
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(def, form, filter, null, UserId, DataContext))
            using (var query = sqb.Build(def, form, filter, null))
            {
                //            query.UserId = UserId;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    return reader.GetCount();
                }
            }
        }

        public List<BizControl> GetTableFormRows(BizForm form, IEnumerable<Guid> docIds, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(form, docIds, sortAttrs, UserId, DataContext))
            using (var query = sqb.Build(form, docIds, sortAttrs))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo*pageSize;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }

        public List<BizControl> GetTableFormRows(BizForm form, IEnumerable<Doc> docs, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var i = 0;
            foreach (var doc in docs)
            {
                if (i > pageNo*pageSize)
                {
                    var formRow = CloneControl(form) as BizForm;

                    rows.Add(SetFormDoc(formRow, doc));
                }
                i++;
                if (pageSize > 0 && i > (pageNo + 1)*pageSize) break;
            }
            return rows;
        }

        public List<BizControl> GetDocListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.BuildAttrList(form, docId, attrDefId, null, null, UserId, DataContext))
            using (var query = sqb.BuildAttrList(form, docId, attrDefId, null, null))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo*pageSize;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    count = reader.GetCount();

                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }
        public List<BizControl> GetDocListTableFormRows(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.BuildAttrList(form, docId, attrDefId, null, null, UserId, DataContext))
            using (var query = sqb.BuildAttrList(form, docId, attrDefId, null, null))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo * pageSize;
                query.WithNoLock = true;

                //using (var reader = new SqlQueryReader(DataContext, query)) 
                // DONE: Найти решение по механизму создания запросов и выборки данных. Вывести создание SqlQueryReader во внешний класс - SqlQueryReaderFactory
                using (var reader = _sqlQueryReaderFactory.Create(query))
                {
                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }
        public int GetDocListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId)
        {
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.BuildAttrList(form, docId, attrDefId, null, null, UserId, DataContext))
            using (var query = sqb.BuildAttrList(form, docId, attrDefId, null, null))
            {
                //            query.UserId = UserId;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    return reader.GetCount();
                }
            }
        }

        public List<BizControl> GetRefListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.BuildRefList(form, docId, attrDefId, null, null, UserId, DataContext))
            using (var query = sqb.BuildRefList(form, docId, attrDefId, null, null))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo*pageSize;
                query.WithNoLock = true;

                //using (var reader = new SqlQueryReader(DataContext, query))
                using (var reader = _sqlQueryReaderFactory.Create(query))
                {
                    count = reader.GetCount();

                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }
        public List<BizControl> GetRefListTableFormRows(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.BuildRefList(form, docId, attrDefId, null, null, UserId, DataContext))
            using (var query = sqb.BuildRefList(form, docId, attrDefId, null, null))
            {
                //            query.UserId = UserId;
                query.TopNo = pageSize;
                query.SkipNo = pageNo * pageSize;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                {
                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }
        public int GetRefListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId)
        {
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.BuildRefList(form, docId, attrDefId, null, null, UserId, DataContext))
            using (var query = sqb.BuildRefList(form, docId, attrDefId, null, null))
            {
                //            query.UserId = UserId;
                query.WithNoLock = true;

                using (var reader = _sqlQueryReaderFactory.Create(query)) // new SqlQueryReader(DataContext, query))
                {
                    return reader.GetCount();
                }
            }
        }

        public List<BizMenu> GetMenus(int languageId)
        {
            var menus = GetMenus();

            if (languageId != 0)
            {
                foreach (var menu in menus)
                {
                    TranslateControl(menu, languageId);
                }
            }
            return menus;
        }

        public void TranslateForm(BizForm form, int languageId)
        {
            if (form == null || form.LanguageId == languageId) return;
            TranslateControl(form, languageId);
            form.LanguageId = languageId;
        }

        public void TranslateMenus(List<BizMenu> menus, int languageId)
        {
            if (menus == null) return;
            foreach (var menu in menus)
            {
                TranslateControl(menu, languageId);
            }
        }

        public IList<ModelMessage> GetFormErrors(BizForm form, IList<ModelMessage> errors)
        {
            if (form == null) return errors;

            if (form.DocumentDefId == null) return errors;

            var docDef = _defRepo.DocDefById((Guid) form.DocumentDefId);
            var result = new ModelMessageBuilder();
            var controlFinder = new ControlFinder(form);

            foreach (var errorMsg in errors)
            {
                if (errorMsg.Key == Guid.Empty && String.IsNullOrEmpty(errorMsg.Name))
                    result.AddMessage(errorMsg.Key, errorMsg.Message);
                else if (controlFinder.Find(errorMsg.Key) != null)
                    result.AddMessage(errorMsg.Key, errorMsg.Message);
                else
                {
                    var attr = docDef.Attributes.FirstOrDefault(a => a.Id == errorMsg.Key);

                    if (attr == null && !String.IsNullOrEmpty(errorMsg.Name))
                        attr =
                            docDef.Attributes.FirstOrDefault(
                                a =>
                                    String.Equals(a.Name, errorMsg.Name, StringComparison.OrdinalIgnoreCase));

                    if (attr != null)
                    {
                        var control =
                            controlFinder.FirstOrDefault(c =>
                                c is BizDataControl &&
                                (((BizDataControl) c).AttributeDefId == attr.Id ||
                                 String.Equals(((BizDataControl)c).AttributeName, attr.Name,
                                     StringComparison.OrdinalIgnoreCase)));

                        if (control != null)
                            result.AddMessage(control.Id, errorMsg.Message);
                    }
                }
            }
            return result.Messages;
        }

        public BizForm SetFormOptions(BizForm form, IList<BizControlOption> options)
        {
            if (form == null || options == null) return form;

            var controlFinder = new ControlFinder(form);
            var hasAttributes = false;

            foreach (var option in options)
            {
                if (option.Id != Guid.Empty)
                {
                    var control = controlFinder.Find(option.Id);

                    if (control != null)
                    {
                        control.Options = option.Flags;
                    }
                }
                else if (!String.IsNullOrEmpty(option.AttributeName))
                    hasAttributes = true;
            }

            if (hasAttributes)
            {
                if (form.DocumentDefId == null) return form;
                var docDef = _defRepo.DocDefById((Guid) form.DocumentDefId);

                foreach (var option in options)
                {
                    if (!String.IsNullOrEmpty(option.AttributeName))
                    {
                        var attr = docDef.Attributes.FirstOrDefault(a => a.Id == option.Id);

                        if (attr == null && !String.IsNullOrEmpty(option.AttributeName))
                            attr =
                                docDef.Attributes.FirstOrDefault(
                                    a =>
                                        String.Equals(a.Name, option.AttributeName, StringComparison.OrdinalIgnoreCase));
                        if (attr != null)
                        {
                            var control =
                                controlFinder.FirstOrDefault(c =>
                                    c is BizDataControl &&
                                    (((BizDataControl)c).AttributeDefId == attr.Id ||
                                     String.Equals(((BizDataControl)c).AttributeName, attr.Name,
                                         StringComparison.OrdinalIgnoreCase)));

                            if (control != null)
                                control.Options = option.Flags;
                        }

                    }
                }
            }
            return form;
        }
		/*
        public IList<EnumValue> GetFormComboBoxValueList(BizForm form, BizComboBox comboBox)
        {
            var list = new List<EnumValue>();
            // TODO: Проверить!!!
            var attrDef = GetComboBoxAttrDef(form, comboBox);
            if (attrDef == null) return list;

            if (attrDef.Type.Id == (short) CissaDataType.Doc && attrDef.DocDefType != null)
            {
                var detailDocDef = _defRepo.DocDefById(attrDef.DocDefType.Id);

                AttrDef detailAttrDef;
                if (comboBox.DetailAttributeId != null)
                    detailAttrDef = detailDocDef.Attributes.FirstOrDefault(ad => ad.Id == comboBox.DetailAttributeId);
                else
                {
                    detailAttrDef =
                        detailDocDef.Attributes.FirstOrDefault(
                            ad =>
                                String.Equals(ad.Name, comboBox.DetailAttributeName, StringComparison.OrdinalIgnoreCase))
                        ??
                        detailDocDef.Attributes.FirstOrDefault(ad => ad.Type.Id == (int) CissaDataType.Text);
                }
                if (detailAttrDef == null) return list;

                var sqlQueryBuilder = _sqlQueryBuilderFactory.Create();
                using (var query = sqlQueryBuilder.Build(comboBox, detailDocDef.Id))
                {
                    query.AddAttribute("&Id");
                    query.AddAttribute(detailAttrDef.Id);
                    query.AddOrderAttribute(detailAttrDef.Id);
                    using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(context, query))
                    {
                        while (reader.Read())
                        {
                            var detail = !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;

                            list.Add(new EnumValue
                            {
                                Id = reader.GetGuid(0),
                                Value = detail,
                                DefaultValue = detail
                            });
                        }
                    }                    
                }
            }
            return list;
        }

        private AttrDef GetComboBoxAttrDef(BizForm form, BizComboBox comboBox)
        {
            var finder = new FormHelper(Provider, form);
            var docDef = finder.FindAttributeDocDef(comboBox.AttributeDefId);
            if (docDef != null)
                return docDef.Attributes.FirstOrDefault(a => a.Id == comboBox.AttributeDefId);

            return null;
        }
		*/
        private void TranslateControl(BizControl control, int languageId)
        {
            if (control.LanguageId != languageId)
            {
                control.Caption = GetTranslateCaption(control, languageId);

                if (control is BizComboBox)
                {
                    if (((BizComboBox)control).Items != null)
                    {
                        // DONE: Exclude EnumRepository direct reference
                        //using (var enumRepo = new EnumRepository(DataContext))
                        _enumRepo.TranslateEnumItems(((BizComboBox) control).Items, languageId);
                    }
                }
                else if (control is BizDocumentControl)
                {
                    var docControl = (BizDocumentControl) control;

                    if (docControl.DocForm != null)
                        TranslateControl(docControl.DocForm, languageId);
                }
                else if (control is BizDocumentListForm)
                {
                    var docListForm = (BizDocumentListForm) control;

                    if (docListForm.TableForm != null)
                        TranslateControl(docListForm.TableForm, languageId);
                }
            }

            if (control.Children != null)
                foreach (var child in control.Children)
                {
                    TranslateControl(child, languageId);
                }

            control.LanguageId = languageId;
        }

        private string GetTranslateCaption(BizControl control, int languageId)
        {
            if (languageId != 0)
            {
                var caption = _langRepo.GetTranslation(control.Id, languageId);

                if (caption != null)
                {
                    return caption;
                }
            }
            return control.DefaultCaption;
        }

        private static string SerializeControl(BizControl control)
        {
            var serializer = new XmlSerializer(typeof (BizControl));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, control);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

        private static BizControl DeserializeControl(string data)
        {
            using (var read = new StringReader(data))
            {
                var serializer = new XmlSerializer(typeof (BizControl));
                using (var reader = new XmlTextReader(read))
                {
                    return (BizControl) serializer.Deserialize(reader);
                }
            }
        }

        /// <summary>
        /// Загружает документ на форму
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="document">Документ</param>
        /// <returns>Форма с данными</returns>
        public BizControl SetFormDoc(BizControl form, Doc document)
        {
            var bizForm = form as BizForm;
            if (bizForm != null)
                bizForm.DocumentId = document.Id;

            if (form is BizDetailForm)
            {
                var dform = (BizDetailForm) form;

                dform.OrganizationId = document.OrganizationId;
                dform.PositionId = document.PositionId;
                dform.UserId = document.UserId;
                dform.Created = document.CreationTime;

                var userInfo = _userRepo.FindUserInfo(document.UserId);
                if (userInfo != null)
                    dform.UserFullName = String.Format("{0} {1} {2}", userInfo.LastName, userInfo.FirstName,
                                                       userInfo.MiddleName);
                else
                    dform.UserFullName = "[Не указан]";
                dform.OrganizationName = document.OrganizationId != null
                                             ? _orgRepo.GetOrgName((Guid) document.OrganizationId)
                                             : "[Не указана]";
                dform.PositionName = document.PositionId != null
                                         ? _orgRepo.GetOrgPositionName((Guid) document.PositionId)
                                         : "[Не указана]";
                dform.DocumentState = _docRepo.GetDocState(document.Id);
            }

            foreach (var child in form.Children)
            {
                SetControlData((BizForm) form, child, document, form.LanguageId);
            }

            return form;
        }

        /// <summary>
        /// Загружает документ на форму
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="reader">Ридер</param>
        /// <returns>Форма с данными</returns>
        public BizForm SetFormDoc(BizForm form, SqlQueryReader reader)
        {
            var source = reader.Query.Source;

            var attrIndex = reader.TryGetAttributeIndex(source, "&Id");
            if (attrIndex >= 0)
                form.DocumentId = !reader.IsDbNull(attrIndex) ? reader.GetGuid(attrIndex) : Guid.Empty;

            if (form is BizDetailForm)
            {
                var dform = (BizDetailForm)form;

                attrIndex = reader.TryGetAttributeIndex(source, "&OrgId");
                if (attrIndex >= 0)
                {
                    dform.OrganizationId = !reader.IsDbNull(attrIndex) ? reader.GetGuid(attrIndex) : Guid.Empty;
                    dform.OrganizationName = dform.OrganizationId != Guid.Empty
                                                 ? _orgRepo.GetOrgName((Guid) dform.OrganizationId)
                                                 : "[Не указана]";
                }

//                dform.PositionId = document.PositionId;
                attrIndex = reader.TryGetAttributeIndex(source, "&UserId");
                if (attrIndex >= 0)
                {
                    dform.UserId = !reader.IsDbNull(attrIndex) ? reader.GetGuid(attrIndex) : Guid.Empty;
                    var userInfo = _userRepo.FindUserInfo(dform.UserId);
                    if (userInfo != null)
                        dform.UserFullName = String.Format("{0} {1} {2}", userInfo.LastName, userInfo.FirstName,
                                                           userInfo.MiddleName);
                    else
                        dform.UserFullName = "[Не указан]";
                }
                attrIndex = reader.TryGetAttributeIndex(source, "&Created");
                if (attrIndex >= 0)
                    dform.Created = !reader.IsDbNull(attrIndex) ? reader.GetDateTime(attrIndex) : DateTime.MinValue;


//                dform.PositionName = document.PositionId != null
//                                         ? orgRepo.GetOrgPositionName((Guid)document.PositionId)
//                                         : "[Не указана]";

                attrIndex = reader.TryGetAttributeIndex(source, "&State");
                if (attrIndex >= 0)
                {
                    var state = !reader.IsDbNull(attrIndex) ? reader.GetGuid(attrIndex) : Guid.Empty;
                    if (state != Guid.Empty)
                    {
                        //using (var docStateRepo = new DocStateRepository(DataContext))
                        dform.DocumentState = new DocState {Type = _docStateRepo.LoadById(state)};
                    }
                }
            }

            foreach (var child in form.Children)
            {
                SetControlData(form, child, reader, source);
            }

            return form;
        }

        /// <summary>
        /// Сохраняет данные на форме в документ
        /// </summary>
        /// <param name="form">Форма с данными</param>
        /// <param name="document">Документ</param>
        /// <returns>Документ</returns>
        public Doc GetFormDoc(BizControl form, Doc document)
        {
            foreach (var child in form.Children)
            {
                GetControlData(child, document);
            }

            return document;
        }

        /// <summary>
        /// Записывает значение атрибута документа в контрол
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="control">Контрол</param>
        /// <param name="document">Документ</param>
        /// <returns></returns>
        public void SetControlData(BizForm form, BizControl control, Doc document)
        {
            AttributeBase attr = null;
            Doc nestingDoc = document;

            var dataControl = control as BizDataControl;
            if (dataControl != null)
            {
                if (document != null)
                {
                    if (dataControl.AttributeDefId != null)
                    {
                        attr = _attrRepo.GetAttributeById((Guid) dataControl.AttributeDefId, document);
                    } 
                    else if (control is BizEditDateTime || control is BizEditText)
                    {
                        ((BizEdit) control).
                            ObjectValue = _docRepo.GetDocumentValue(document, ((BizEdit) control).Ident);
                    }
                }
                var comboBox = control as BizComboBox;
                if (comboBox != null && attr == null)
                {
                    if (dataControl.AttributeDefId == null && !String.IsNullOrEmpty(dataControl.AttributeName))
                    {
                        comboBox.ObjectValue = _docRepo.GetDocumentValue(document, comboBox.Ident);
                    }
                    // DONE: Инициализировать текущее значение КомбоБокса
                        // Factory.InitComboBoxItems(((BizComboBox) control), attr != null ? attr.AttrDef : null);
                        // comboBox.DetailText = _comboBoxValueProvider.GetComboBoxDetailValue(comboBox, null);
                    comboBox.Items =
                        new List<EnumValue>(_comboBoxValueProvider.GetFormComboBoxValues(form, comboBox
                            /*, attr != null ? attr.AttrDef : null*/));
                }

                if (attr != null)
                {
                    if (control is BizDocumentControl)
                    {
                        dataControl.ObjectValue = attr.ObjectValue;

                        var docControl = (BizDocumentControl)control;

                        if (docControl.Children != null && docControl.Children.Count > 0)
                        {
                            nestingDoc = _docRepo.GetNestingDocument(document, (DocAttribute)attr);
                        }
                        else
                        {
                            if (docControl.DocForm == null && docControl.FormId != null)
                                docControl.DocForm = GetForm((Guid)docControl.FormId);

                            if (docControl.DocForm != null && attr.ObjectValue != null)
                            {
                                //Doc controlDoc = docRepo.GetNestingDocument(document, (Guid)attr.ObjectValue, attr.Created);
                                var controlDoc = _docRepo.GetNestingDocument(document, (DocAttribute)attr);
                                //docRepo.LoadById((Guid) attr.ObjectValue, attr.Created);
                                SetFormDoc(docControl.DocForm, controlDoc);
                            }
                        }
                    }
                    else if (control is BizDocumentListForm)
                    {
                        //((BizDataControl)control).ObjectValue = attr.ObjectValue;

                        var docList = (BizDocumentListForm)control;

                        docList.DocDefId = attr.AttrDef.DocDefType != null ? attr.AttrDef.DocDefType.Id : (Guid?)null;
                        docList.DocumentId = document.Id;

                        if (docList.TableForm == null && docList.FormId != null)
                            docList.TableForm =
                                GetTableForm((Guid)docList.FormId);
                        if (docList.TableForm != null)
                        {
                            /*int pc = 1;

                            docList.DocList = attrRepo.GetAttributeDocList(out pc, document.Id, attr.AttrDef.Id,
                                                                           docList.TableForm.PageNo, docList.TableForm.PageSize);
                            docList.TableForm.PageCount = pc; */
                        }
                    }
                    else if (control is BizTableColumn)
                    {
                        var column = (BizTableColumn)control;

                        if (attr is DocAttribute && attr.ObjectValue != null)
                        {
                            nestingDoc = _docRepo.GetNestingDocument(document, (DocAttribute)attr);
                            column.Document = nestingDoc;
                        }
                    }
                    else if (attr is OrganizationAttribute && control is BizEditText)
                    {
                        if (attr.ObjectValue != null)
                        {
                            string orgName;
                            if (_orgRepo.TryGetOrgName((Guid) attr.ObjectValue, out orgName))
                                ((BizEditText) control).Value = orgName;
                        }
                        else ((BizEditText)control).Value = String.Empty;
                    }
                    else if (attr is DocumentStateAttribute && control is BizEditText)
                    {
                        if (attr.ObjectValue != null)
                        {
                            var info = _docStateRepo.TryLoadById((Guid) attr.ObjectValue);
                            ((BizEditText) control).Value = info != null ? info.Name : String.Empty;
                        }
                        else ((BizEditText) control).Value = String.Empty;
                    }
                    else if (attr is BlobAttribute)
                    {
                        if (control is BizEditFile)
                        {
                            ((BizEditFile) control).DocumentId = document.Id;
                            ((BizEditFile) control).FileName = ((BlobAttribute) attr).FileName;
                            ((BizEditFile) control).Empty = !((BlobAttribute) attr).HasValue;
                        }
                        else if (control is BizDataImage)
                        {
                            ((BizDataImage) control).DocumentId = document.Id;
                            ((BizDataImage) control).FileName = ((BlobAttribute) attr).FileName;
                        }
                    }
                    else
                    {
                        dataControl.ObjectValue = attr.ObjectValue;
                        dataControl.DocNotNull = attr.AttrDef.IsNotNull;

                        var combo = control as BizComboBox;
                        if (combo != null)
                            // DONE: Инициализировать текущее значение КомбоБокса
                            // Factory.InitComboBoxItems(combo, attr.AttrDef);
                            // combo.DetailText = _comboBoxValueProvider.GetComboBoxDetailValue(combo, attr.AttrDef);
                            // comboBox.Items = new List<EnumValue>(_comboBoxValueProvider.GetFormComboBoxValues(comboBox, attr.AttrDef));
                            comboBox.Items =
                                new List<EnumValue>(_comboBoxValueProvider.GetFormComboBoxValues(form, comboBox, attr.AttrDef));
                    }

                }
                else if (control is BizDocumentListForm && ((BizDocumentListForm) control).FormId != null)
                {
                    var docList = (BizDocumentListForm) control;

                    if (docList.TableForm == null)
                        docList.TableForm = GetTableForm((Guid) docList.FormId);

                    if (docList.TableForm.DocumentDefId != null)
                    {
                        var nestingDocDef = _defRepo.DocDefById((Guid) docList.TableForm.DocumentDefId);

                        var nestingAttr =
                            nestingDocDef.Attributes.FirstOrDefault(a => a.Id == docList.AttributeDefId);

                        if (nestingAttr == null && !String.IsNullOrEmpty(docList.AttributeName))
                            nestingAttr = nestingDocDef.Attributes.FirstOrDefault(
                                a => String.Equals(a.Name, docList.AttributeName, StringComparison.OrdinalIgnoreCase));

                        if (nestingAttr != null || docList.FormAttributeDefId != null)
                        {
                            docList.DocDefId = docList.TableForm.DocumentDefId;
                            docList.DocumentId = document != null ? document.Id : (Guid?) null;
                        }
                    }
                }
                else
                {
                    dataControl.ObjectValue = null;

                    if (control is BizDocumentControl)
                    {
                        var docControl = (BizDocumentControl)control;

                        if (docControl.Children != null && docControl.Children.Count > 0)
                            nestingDoc = null;
                        else
                        {
                            if (docControl.DocForm != null) docControl.DocForm = null;
                        }
                    }
                    else if (control is BizTableColumn)
                    {
                        nestingDoc = null;
                    }
                }
            }

            if (control != null && control.Children != null)
                foreach (var child in control.Children)
                {
                    SetControlData(form, child, nestingDoc);
                }
        }

        /// <summary>
        /// Записывает значение атрибута документа в контрол
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="control">Контрол</param>
        /// <param name="document">Документ</param>
        /// <param name="languageId">Язык</param>
        /// <returns></returns>
        public void SetControlData(BizForm form, BizControl control, Doc document, int languageId)
        {
            AttributeBase attr = null;
            Doc nestingDoc = document;

            var dataControl = control as BizDataControl;
            if (dataControl != null)
            {
                if (document != null)
                {
                    if (dataControl.AttributeDefId != null)
                    {
                        attr = _attrRepo.GetAttributeById((Guid) dataControl.AttributeDefId, document);
                    }
                    else if (control is BizEditDateTime || control is BizEditText)
                    {
                        ((BizEdit) control).
                            ObjectValue = _docRepo.GetDocumentValue(document, ((BizEdit) control).Ident);
                        return;
                    }
                }
                var comboBox = control as BizComboBox;
                if (comboBox != null && attr == null)
                {
                    if (dataControl.AttributeDefId == null && !String.IsNullOrEmpty(dataControl.AttributeName))
                    {
                        comboBox.ObjectValue = _docRepo.GetDocumentValue(document, comboBox.Ident);
                    }
                    comboBox.Items = new List<EnumValue>(_comboBoxValueProvider.GetFormComboBoxValues(form, comboBox/*, attr != null ? attr.AttrDef : null*/));
                }

                if (attr != null)
                {
                    if (control is BizDocumentControl)
                    {
                        dataControl.ObjectValue = attr.ObjectValue;

                        var docControl = (BizDocumentControl)control;

                        if (docControl.Children != null && docControl.Children.Count > 0)
                        {
                            nestingDoc = _docRepo.GetNestingDocument(document, (DocAttribute)attr);
                        }
                        else
                        {
                            if (docControl.DocForm == null && docControl.FormId != null)
                                docControl.DocForm = GetForm((Guid)docControl.FormId, languageId);

                            if (docControl.DocForm != null && attr.ObjectValue != null)
                            {
                                //Doc controlDoc = docRepo.GetNestingDocument(document, (Guid)attr.ObjectValue, attr.Created);
                                var controlDoc = _docRepo.GetNestingDocument(document, (DocAttribute)attr);
                                //docRepo.LoadById((Guid) attr.ObjectValue, attr.Created);
                                SetFormDoc(docControl.DocForm, controlDoc);
                            }
                        }
                    }
                    else if (control is BizDocumentListForm)
                    {
                        //((BizDataControl)control).ObjectValue = attr.ObjectValue;

                        var docList = (BizDocumentListForm)control;

                        docList.DocDefId = attr.AttrDef.DocDefType != null ? attr.AttrDef.DocDefType.Id : (Guid?)null;
                        docList.DocumentId = document.Id;

                        if (docList.TableForm == null && docList.FormId != null)
                            docList.TableForm =
                                GetTableForm((Guid)docList.FormId, languageId);
                        if (docList.TableForm != null)
                        {
                            /*int pc = 1;

                            docList.DocList = attrRepo.GetAttributeDocList(out pc, document.Id, attr.AttrDef.Id,
                                                                           docList.TableForm.PageNo, docList.TableForm.PageSize);
                            docList.TableForm.PageCount = pc; */
                        }
                    }
                    else if (control is BizTableColumn)
                    {
                        var column = (BizTableColumn)control;

                        if (attr is DocAttribute && attr.ObjectValue != null)
                        {
                            nestingDoc = _docRepo.GetNestingDocument(document, (DocAttribute)attr);
                            column.Document = nestingDoc;
                        }
                    }
                    else if (attr is OrganizationAttribute && control is BizEditText)
                    {
                        if (attr.ObjectValue != null)
                        {
                            string orgName;
                            if (_orgRepo.TryGetOrgName((Guid)attr.ObjectValue, out orgName))
                                ((BizEditText)control).Value = orgName;
                        }
                        else ((BizEditText)control).Value = String.Empty;
                    }
                    else if (attr is DocumentStateAttribute && control is BizEditText)
                    {
                        if (attr.ObjectValue != null)
                        {
                            //using (var docStateRepo = new DocStateRepository(DataContext))

                            var info = _docStateRepo.TryLoadById((Guid) attr.ObjectValue);
                            ((BizEditText) control).Value = info != null ? info.Name : String.Empty;
                        }
                        else ((BizEditText)control).Value = String.Empty;
                    }
                    else if (attr is BlobAttribute)
                    {
                        if (control is BizEditFile)
                        {
                            ((BizEditFile) control).DocumentId = document.Id;
                            ((BizEditFile) control).FileName = ((BlobAttribute) attr).FileName;
                            ((BizEditFile)control).Empty = !((BlobAttribute)attr).HasValue;
                        }
                        else if (control is BizDataImage)
                        {
                            ((BizDataImage) control).DocumentId = document.Id;
                            ((BizDataImage) control).FileName = ((BlobAttribute) attr).FileName;
                        }
                    }
                    else
                    {
                        dataControl.ObjectValue = attr.ObjectValue;
                        dataControl.DocNotNull = attr.AttrDef.IsNotNull;

                        if (control is BizComboBox)
                            //Factory.InitComboBoxItems(((BizComboBox)control), attr.AttrDef);
                            /*((BizComboBox) control).DetailText =
                                _comboBoxValueProvider.GetComboBoxDetailValue(form, (BizComboBox) control/*, attr.AttrDef#1#);*/
                            comboBox.Items =
                                new List<EnumValue>(_comboBoxValueProvider.GetFormComboBoxValues(form, comboBox, attr.AttrDef));
                    }
                }
                else if (control is BizDocumentListForm && ((BizDocumentListForm)control).FormId != null/* &&
                    ((BizDocumentListForm)control).FormAttributeDefId != null*/)
                {
                    var docList = (BizDocumentListForm)control;

                    if (docList.TableForm == null)
                        docList.TableForm = GetTableForm((Guid)docList.FormId, languageId);

                    if (docList.TableForm.DocumentDefId != null)
                    {
                        //var defRepo = new DocDefRepository(DataContext);
                        var nestingDocDef = _defRepo.DocDefById((Guid)docList.TableForm.DocumentDefId);

                        var nestingAttr =
                            nestingDocDef.Attributes.FirstOrDefault(
                                a => a.Id == docList.AttributeDefId);

                        if (nestingAttr == null && !String.IsNullOrEmpty(docList.AttributeName))
                            nestingAttr = nestingDocDef.Attributes.FirstOrDefault(
                                a => String.Equals(a.Name, docList.AttributeName, StringComparison.OrdinalIgnoreCase));

                        if (nestingAttr != null || docList.FormAttributeDefId != null)
                        {
                            docList.DocDefId = docList.TableForm.DocumentDefId;
                            docList.DocumentId = document != null ? document.Id : (Guid?) null;
                        }
                    }
                }
                else
                {
                    dataControl.ObjectValue = null;

                    if (control is BizDocumentControl)
                    {
                        var docControl = (BizDocumentControl)control;

                        if (docControl.Children != null && docControl.Children.Count > 0)
                            nestingDoc = null;
                        else
                        {
                            if (docControl.DocForm != null) docControl.DocForm = null;
                        }
                    }
                    else if (control is BizTableColumn)
                    {
                        nestingDoc = null;
                    }
                }
            }

            if (control != null && control.Children != null)
                foreach (var child in control.Children)
                {
                    SetControlData(form, child, nestingDoc, languageId);
                }
        }

        /// <summary>
        /// Записывает значение атрибута документа в контрол
        /// </summary>
        /// <param name="control">Контрол</param>
        /// <param name="reader">Ридер данных</param>
        /// <param name="source">Источник данных в запросе</param>
        /// <param name="form">Форма</param>
        /// <returns></returns>
        public void SetControlData(BizForm form, BizControl control, SqlQueryReader reader, SqlQuerySource source)
        {
            object attrValue = null;
            AttrDef attr = null;
            var target = source;

            var box = control as BizDataControl;
            if (box != null)
            {
                if (box.AttributeDefId != null)
                {
                    var attrIndex = reader.TryGetAttributeIndex(source, (Guid) box.AttributeDefId);
                    attrValue = attrIndex >= 0 ? !reader.IsDbNull(attrIndex) ? reader.GetValue(attrIndex) : null : null;
                    var qAttr = reader.Query.FindAttribute(source, (Guid) box.AttributeDefId);
                    attr = qAttr != null && qAttr.Def != null ? qAttr.Def : null;
                }
                else if (control is BizEditVar || control is BizEditText || control is BizEditDateTime)
                {
                    var attrIndex = reader.TryGetAttributeIndex(source, box.AttributeName);
                    attrValue = attrIndex >= 0 ? !reader.IsDbNull(attrIndex) ? reader.GetValue(attrIndex) : null : null;
                    if (attrValue != null && attrIndex >= 0)
                    {
                        SystemIdent attrIdent;
                        if (SystemIdentConverter.TryConvert(box.AttributeName, out attrIdent))
                            switch (attrIdent)
                            {
                                case SystemIdent.OrgName:
                                case SystemIdent.OrgCode:
                                    attrValue = ((Guid) attrValue) != Guid.Empty
                                        ? _orgRepo.GetOrgName((Guid) attrValue)
                                        : "[Не указана]";
                                    break;
                                case SystemIdent.State:
                                    if (((Guid) attrValue) != Guid.Empty)
                                    {
                                        //using (var docStateRepo = new DocStateRepository(DataContext))

                                        var docStateType = _docStateRepo.LoadById((Guid) attrValue);
                                        if (docStateType != null)
                                            attrValue = docStateType.Name;
                                    }
                                    break;
                                case SystemIdent.UserName:
                                    var userInfo = _userRepo.FindUserInfo((Guid) attrValue);
                                    if (userInfo != null)
                                        attrValue = String.Format("{0} {1} {2}", userInfo.LastName, userInfo.FirstName,
                                            userInfo.MiddleName);
                                    else
                                        attrValue = "[Не указан]";
                                    break;
                            }
                    }
                    var qAttr = reader.Query.FindAttribute(source, box.AttributeName);
                    attr = qAttr != null && qAttr.Def != null ? qAttr.Def : null;
                }
                else if (control is BizEditSysIdent)
                {
                    var attrIndex = reader.TryGetAttributeIndex(source, box.AttributeName);
                    attrValue = attrIndex >= 0 ? !reader.IsDbNull(attrIndex) ? reader.GetValue(attrIndex) : null : null;
                    var qAttr = reader.Query.FindAttribute(source, box.AttributeName);
                    attr = qAttr != null && qAttr.Def != null ? qAttr.Def : null;
                }

                if (attrValue != null)
                {
                    if (control is BizDocumentControl)
                    {
                        box.ObjectValue = attrValue;
                        target = reader.Query.FindAttributeTargetSource(source, (Guid) box.AttributeDefId);
                    }
                    else if (control is BizDocumentListForm)
                    {
                    }
                    else if (control is BizTableColumn)
                    {
                        target = reader.Query.FindAttributeTargetSource(source, (Guid) box.AttributeDefId);
                    }
                    else if (control is BizComboBox)
                    {
                        ((BizComboBox) control).ObjectValue = attrValue;
                        // Factory.InitComboBoxItems(((BizComboBox)control), attr);
                        /*((BizComboBox) control).DetailText =
                            _comboBoxValueProvider.GetComboBoxDetailValue((BizComboBox) control, attr);*/
                        if (attr != null)
                            ((BizComboBox) control).Items =
                                new List<EnumValue>(_comboBoxValueProvider.GetFormComboBoxValues(form,
                                    (BizComboBox) control,
                                    attr));
                        else
                            ((BizComboBox) control).Items =
                                new List<EnumValue>(_comboBoxValueProvider.GetFormComboBoxValues(form,
                                    (BizComboBox) control));
                    }
                    else if (attr != null && attr.Type.Id == (short) CissaDataType.Organization && control is BizEditText)
                    {
                        string orgName;
                        if (_orgRepo.TryGetOrgName((Guid) attrValue, out orgName))
                            ((BizEditText) control).Value = orgName;
                    }
                    else if (attr != null && attr.Type.Id == (short) CissaDataType.DocumentState &&
                             control is BizEditText)
                    {
                        //using (var docStateRepo = new DocStateRepository(DataContext))

                        var info = _docStateRepo.TryLoadById((Guid) attrValue);
                        ((BizEditText) control).Value = info != null ? info.Name : String.Empty;
                    }
                    else if (attr != null && attr.Type.Id == (short) CissaDataType.Blob)
                    {
                        if (control is BizEditFile)
                        {
                            if (form.DocumentId != null)
                                ((BizEditFile) control).DocumentId = form.DocumentId ?? Guid.Empty;
                            ((BizEditFile) control).FileName = attrValue.ToString();
                        }
                        else if (control is BizDataImage)
                        {
                            if (form.DocumentId != null)
                                ((BizDataImage) control).DocumentId = form.DocumentId ?? Guid.Empty;
                            ((BizDataImage) control).FileName = attrValue.ToString();
                        }
                    }
                    else
                        box.ObjectValue = attrValue;
                }
                else
                {
                    box.ObjectValue = null;
                    if (box is BizDocumentControl)
                    {
                        if (box.AttributeDefId != null)
                            target = reader.Query.FindAttributeTargetSource(source, (Guid) box.AttributeDefId);
                    }
                    else if (box is BizTableColumn)
                    {
                        if (box.AttributeDefId != null)
                            target = reader.Query.FindAttributeTargetSource(source, (Guid) box.AttributeDefId);
                    }
                }
            }

            if (control != null && control.Children != null)
                foreach (var child in control.Children)
                {
                    SetControlData(form, child, reader, target);
                }
        }

        /// <summary>
        /// Записывает значение контрола в атрибут документа
        /// </summary>
        /// <param name="control">Контрол</param>
        /// <param name="document">Документ</param>
        /// <returns></returns>
        public void GetControlData(BizControl control, Doc document)
        {
            if (document == null) return;
            AttributeBase attr = null;
            //var attrRepo = new AttributeRepository(DataContext);
            var nestingDoc = document;

            var @var = control as BizDataControl;
            if (@var != null && !(control is BizDocumentListForm))
            {
                if (@var.AttributeDefId != null)
                {
                    attr = _attrRepo.GetAttributeById((Guid)@var.AttributeDefId, document);
                }
                else if (control is BizEditVar)
                {
                    attr =
                        document.Attributes.FirstOrDefault(
                            a => String.Equals(a.AttrDef.Name, ((BizEditVar)control).AttributeName, StringComparison.OrdinalIgnoreCase));
                }
/*
                else if (control is BizComboBox && ((BizComboBox)control).AttributeDefId != null)
                {
                    attr = attrRepo.GetAttributeById((Guid)((BizComboBox)control).AttributeDefId, document);
                }
                else if (control is BizDocumentControl && ((BizDocumentControl)control).AttributeDefId != null)
                {
                    attr = attrRepo.GetAttributeById((Guid)((BizDocumentControl)control).AttributeDefId, document);
                }
                else if (control is BizDocumentListForm && ((BizDocumentListForm)control).AttributeDefId != null)
                {
                    attr = attrRepo.GetAttributeById((Guid)((BizDocumentListForm)control).AttributeDefId, document);
                }
*/
                if (control is BizDocumentControl && attr is DocAttribute /*&& ControlsHaveValue(@var.Children)*/)
                {
                    var docAttr = (DocAttribute)attr;

                    if (docAttr.Document == null && docAttr.Value != null)
                    {
                        nestingDoc = _docRepo.GetNestingDocument(document, docAttr);
                    }
                    else
                    {
                        if (ControlsHaveValue(@var.Children))
                        {
                            if (docAttr.Document == null && docAttr.AttrDef.DocDefType != null)
                                nestingDoc = docAttr.Document = _docRepo.New(docAttr.AttrDef.DocDefType.Id);
                            else
                                nestingDoc = docAttr.Document;
                        }
                        else
                            nestingDoc = null;
                    }
                }
                else if (attr != null &&
                         /*!(attr is OrganizationAttribute) && !(attr is DocumentStateAttribute) &&*/ !(attr is DocListAttribute))
                    attr.ObjectValue = @var.ObjectValue;
            }

            if (control.Children != null)
                foreach (var child in control.Children)
                {
                    GetControlData(child, nestingDoc);
                }
        }

        private static bool ControlsHaveValue(IEnumerable<BizControl> controls)
        {
            if (controls == null) return false;
            foreach (var child in controls)
            {
                if (child is BizDataControl && ((BizDataControl)child).AttributeDefId != null &&
                    ((BizDataControl)child).ObjectValue != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Возвращает список форм со строковыми данными для отображения табличных форм
        /// </summary>
        /// <param name="form">Табличная форма</param>
        /// <param name="docIds">Список Ид документов</param>
        /// <returns>Список форм со строковыми данными</returns>
        public List<BizControl> GetTableFormRows(BizTableForm form, List<Guid> docIds)
        {
            return (from docId in docIds
                select _docRepo.LoadById(docId)
                into doc
                let formRow = CloneControl(form) as BizForm
                select SetFormDoc(formRow, doc)).ToList();
        }

        /// <summary>
        /// Возвращает список форм со строковыми данными для отображения табличных форм
        /// </summary>
        /// <param name="form">Табличная форма</param>
        /// <param name="filter">Форма фильтра с данными</param>
        /// <param name="pageSize">Кол-во строк в странице</param>
        /// <param name="pageNo">Номер текущей страницы</param>
        /// <returns>Список форм со строковыми данными</returns>
        public List<BizControl> GetTableFormRows(BizTableForm form, BizForm filter, int pageSize, int pageNo)
        {
            var rows = new List<BizControl>();
            var sqb = _sqlQueryBuilderFactory.Create();
            //using (var query = SqlQueryExBuilder.Build(form.DocumentDefId ?? Guid.Empty, filter, null, UserId, DataContext))
            using (var query = sqb.Build(form.DocumentDefId ?? Guid.Empty, filter, null))
            {
                query.WithNoLock = true;
                query.TopNo = pageSize;
                query.SkipNo = pageNo*pageSize;

                using (var reader = _sqlQueryReaderFactory.Create(query)) // new SqlQueryReader(DataContext, query))
                {
                    var hasPageSize = pageSize > 0;
                    while (reader.Read())
                    {
                        var formRow = CloneControl(form) as BizForm;

                        rows.Add(SetFormDoc(formRow, reader));
                        if (hasPageSize)
                        {
                            pageSize--;
                            if (pageSize <= 0) break;
                        }
                    }
                }
            }
            return rows;
        }

        private BizControl CloneControl(BizControl control)
        {
            return DeserializeControl(SerializeControl(control));

            //if (control is BizTableForm)
            //{
            //    return AssignForm(new BizTableForm
            //                          {
            //                              FilterFormId = ((BizTableForm) control).FilterFormId,
            //                              FormId = ((BizTableForm) control).FormId,
            //                              PageSize = ((BizTableForm) control).PageSize
            //                          }, (BizTableForm) control);
            //}
            //if (control is BizDetailForm)
            //    return AssignForm(new BizDetailForm
            //                          {
            //                              LayoutId = ((BizDetailForm) control).LayoutId
            //                          }, (BizDetailForm) control);
            //if (control is BizDocumentControl)
            //    return AssignDataControl(new BizDocumentControl()
            //                                 {
            //                                     FormId = ((BizDocumentControl) control).FormId,
            //                                     DocForm = ((BizDocumentControl) control).DocForm,
            //                                     Value = ((BizDocumentControl) control).Value
            //                                 }, (BizDocumentControl) control);
            //if (control is BizDocumentListForm)
            //    return AssignDataControl(new BizDocumentListForm
            //                                 {
            //                                     FormId = ((BizDocumentListForm) control).FormId,
            //                                     TableForm = ((BizDocumentListForm) control).TableForm,
            //                                     DocDefId = ((BizDocumentListForm) control).DocDefId,
            //                                     DocumentId = ((BizDocumentListForm) control).DocumentId,
            //                                     DocList = ((BizDocumentListForm) control).DocList
            //                                 }, (BizDocumentListForm) control);
            //if (control is BizPanel)
            //    return AssignControl(new BizPanel
            //                             {
            //                                 LayoutId = ((BizPanel) control).LayoutId
            //                             }, (BizPanel) control);
            //if (control is BizTableColumn)
            //    return AssignDataControl(new BizTableColumn
            //                                 {
            //                                     Value = ((BizTableColumn) control).Value,
            //                                     Document = ((BizTableColumn) control).Document
            //                                 }, (BizTableColumn) control);
            //if (control is BizEditText)
            //    return AssignEdit(new BizEditText
            //                          {
            //                              Value = ((BizEditText) control).Value
            //                          }, (BizEditText) control);
            //if (control is BizEditInt)
            //    return AssignEdit(new BizEditInt
            //                          {
            //                              Value = ((BizEditInt) control).Value
            //                          }, (BizEditInt) control);
            //if (control is BizEditFloat)
            //    return AssignEdit(new BizEditFloat
            //                          {
            //                              Value = ((BizEditFloat) control).Value
            //                          }, (BizEditFloat) control);
            //if (control is BizEditDateTime)
            //    return AssignEdit(new BizEditDateTime
            //                          {
            //                              Value = ((BizEditDateTime) control).Value
            //                          }, (BizEditDateTime) control);
            //if (control is BizEditCurrency)
            //    return AssignEdit(new BizEditCurrency
            //                          {
            //                              Value = ((BizEditCurrency) control).Value
            //                          }, (BizEditCurrency) control);
            //if (control is BizEditBool)
            //    return AssignEdit(new BizEditBool
            //                          {
            //                              Value = ((BizEditBool) control).Value
            //                          }, (BizEditBool) control);
            //if (control is BizComboBox)
            //    return AssignDataControl(new BizComboBox
            //                                 {
            //                                     Value = ((BizComboBox) control).Value,
            //                                     IsRadio = ((BizComboBox) control).IsRadio,
            //                                     Items = ((BizComboBox) control).Items,
            //                                     Rows = ((BizComboBox) control).Rows
            //                                 }, (BizComboBox) control);
            //if (control is BizMenu)
            //    return AssignControl(new BizMenu
            //                             {
            //                                 DocStateId = ((BizMenu) control).DocStateId,
            //                                 FormId = ((BizMenu) control).FormId,
            //                                 ProcessId = ((BizMenu) control).ProcessId
            //                             }, control);

            //throw new ApplicationException(String.Format("Не могу клонировать визуальный элемент \"{0}\"", control.GetType().ToString()));
        }

       /* private BizControl AssignEdit(BizEdit target, BizEdit source)
        {
            target.MaxValue = source.MaxValue;
            target.MinValue = source.MinValue;
            target.MaxLength = source.MaxLength;
            target.Rows = source.Rows;
            target.Cols = source.Cols;

            return AssignDataControl(target, source);
        }

        private BizControl AssignDataControl(BizDataControl target, BizDataControl source)
        {
            target.AttributeDefId = source.AttributeDefId;
            target.ReadOnly = source.ReadOnly;
            return AssignControl(target, source);
        }

        private BizControl AssignForm(BizForm target, BizForm source)
        {
            target.DocumentDefId = source.DocumentDefId;
            target.LanguageId = source.LanguageId;
            return AssignControl(target, source);
        }

        private BizControl AssignControl(BizControl target, BizControl source)
        {
            target.Caption = source.Caption;
            target.Id = source.Id;
            target.Invisible = source.Invisible;
            target.Name = source.Name;
            target.Operation = source.Operation;
            target.Permissions = source.Permissions;
            target.Style = source.Style;
            target.Title = source.Title;
            if (source.Children != null && source.Children.Count > 0)
            {
                if (target.Children == null)
                    target.Children = new List<BizControl>();

                foreach (var child in source.Children)
                {
                    target.Children.Add(CloneControl(child));
                }
            }
            return target;
        }*/

        /// <summary>
        ///  Инициализирует детальную форму, в зависимости от данных документа??
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="document">Документ</param>
        /// <returns>Форму</returns>
        public BizForm InitializeFormContext(BizForm form, Doc document)
        {
            var script = GetFormScript(form);
            return form;
        }

        public void InitializeControls(BizForm form, BizControl control, DocDef def)
        {
            var tableForm = control as BizTableForm;
            if (tableForm != null)
            {
                if (tableForm.Children == null || tableForm.Children.Count == 0)
                {
                    if (tableForm.FormId != null || tableForm.FilterFormId != null)
                    {
                        var detailForm = GetDetailForm((Guid) (tableForm.FormId ?? tableForm.FilterFormId));
                        tableForm.Children = new List<BizControl>(
                            detailForm.Children.Select(CloneControl));
                    }
                }
            }
            else if (control is BizComboBox)
            {
                var attrDef = ((BizComboBox) control).AttributeDefId != null
                                  ? def.Attributes.FirstOrDefault(
                                      a => a.Id == ((BizComboBox) control).AttributeDefId)
                                  : null;

                // Factory.InitComboBoxItems((BizComboBox) control, attrDef);
                /*((BizComboBox) control).DetailText = _comboBoxValueProvider.GetComboBoxDetailValue(
                    (BizComboBox) control, attrDef);*/
                ((BizComboBox) control).Items = new List<EnumValue>(_comboBoxValueProvider.GetFormComboBoxValues(form, (BizComboBox) control, attrDef));
            }
            else if (control is BizTableColumn && control.Children != null && control.Children.Count > 0)
            {
                var attrDef = ((BizTableColumn)control).AttributeDefId != null
                                  ? def.Attributes.FirstOrDefault(
                                      a => a.Id == ((BizTableColumn)control).AttributeDefId)
                                  : null;
                if (attrDef != null && attrDef.DocDefType != null)
                {
                    var nestDef = _defRepo.DocDefById(attrDef.DocDefType.Id);

                    foreach (var child in control.Children)
                        InitializeControls(form, child, nestDef);
                    return;
                }
            }
            else if (control is BizDocumentControl)
            {
                var docControl = (BizDocumentControl) control;

                var attrDef = docControl.AttributeDefId != null
                                  ? def.Attributes.FirstOrDefault(
                                      a => a.Id == docControl.AttributeDefId)
                                  : null;
                if (attrDef != null && attrDef.DocDefType != null)
                {
                    var nestDef = _defRepo.DocDefById(attrDef.DocDefType.Id);

                    if ((control.Children == null || control.Children.Count == 0) && (docControl.DocForm != null || docControl.FormId != null))
                    {
                        if (docControl.DocForm == null && docControl.FormId != null)
                            docControl.DocForm = GetForm((Guid) docControl.FormId);

                        if (docControl.DocForm != null)
                            control.Children = new List<BizControl>(
                                docControl.DocForm.Children.Select(CloneControl));
                    }
                    if (control.Children != null)
                    {
                        foreach (var child in control.Children)
                            InitializeControls(form, child, nestDef);
                    }
                    if (docControl.DocForm != null && docControl.DocForm.Children != null)
                        foreach (var child in docControl.DocForm.Children)
                            InitializeControls(form, child, nestDef);
                    return;
                }
            }
            else if (control is BizDocumentListForm)
            {
                var list = (BizDocumentListForm) control;

                if (list.AttributeDefId == null && list.FormAttributeDefId == null)
                {
                    if (list.TableForm == null && list.FormId != null)
                        list.TableForm = GetTableForm((Guid) list.FormId);

                    if (list.TableForm != null && list.TableForm.DocumentDefId != null)
                    {
                        //var defRepo = new DocDefRepository(DataContext, UserId);

                        var docDef = _defRepo.DocDefById((Guid) list.TableForm.DocumentDefId);
                        //var ascendants = new List<Guid>();
                        var descendants = new List<Guid>();
                        //ascendants.AddRange(_defRepo.GetAncestors(def));
                        descendants.AddRange(_defRepo.GetDocDefDescendant(docDef.Id));

                        foreach(var attr in 
                            def.Attributes.Where(a => 
                                (a.Type.Id == (short)CissaDataType.Doc || a.Type.Id == (short)CissaDataType.DocList) &&
                                    a.DocDefType != null))
                        {
                            if (descendants.Contains(attr.DocDefType.Id))
                            {
                                list.AttributeDefId = attr.Id;
                                list.AttributeName = attr.Name;

                                break;
                            }
                        }

                        if (list.AttributeDefId == null)
                        {
                            descendants.Clear();
                            descendants.AddRange(_defRepo.GetDocDefDescendant(def.Id));

                            foreach (var attr in
                                docDef.Attributes.Where(a =>
                                    (a.Type.Id == (short)CissaDataType.Doc || a.Type.Id == (short)CissaDataType.DocList) &&
                                        a.DocDefType != null))
                            {
                                if (descendants.Contains(attr.DocDefType.Id))
                                {
                                    list.FormAttributeDefId = attr.Id;
//                                    list.AttributeName = attr.Name;
                                    break;
                                }
                            }
                        }
                        if (control.Children != null)
                            foreach (var child in control.Children)
                                InitializeControls(form, child, docDef);
                        if (list.TableForm.Children != null)
                            foreach (var child in list.TableForm.Children)
                                InitializeControls(form, child, docDef);
                        return;
                    }
                }
            }

            if (control.Children != null)
                foreach (var child in control.Children)
                    InitializeControls(form, child, def);
        }

        public string GetFormScript(BizForm form)
        {
            var frm = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Form>().FirstOrDefault(f => f.Id == form.Id);
            return frm != null ? frm.Script : String.Empty;
        }

        public readonly static IDictionary<string, IList<BizMenu>> MenuCache = new Dictionary<string, IList<BizMenu>>();
        // private readonly static object BizMenuLocker = new object();
        private static readonly ReaderWriterLock MenuCacheLock = new ReaderWriterLock();

        /// <summary>
        /// Возвращает дерево меню
        /// </summary>
        /// <returns>Список меню</returns>
        public List<BizMenu> GetMenus()
        {
            // lock (BizMenuLocker)
            MenuCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var returnMenus = new List<BizMenu>();
                {
                    if (MenuCache.ContainsKey(DataContext.Name))
                    {
                        var loadedMenus = MenuCache[DataContext.Name];

                        if (loadedMenus != null && loadedMenus.Count > 0)
                        {
                            returnMenus.AddRange(
                                loadedMenus.Select(
                                    menu => (BizMenu) CheckControlPermissions(CloneControl(menu), Permissions))
                                    .Where(bm => bm != null));

                            return returnMenus;
                        }
                    }

                    var lc = MenuCacheLock.UpgradeToWriterLock(LockTimeout);
                    try
                    {
                        if (MenuCache.ContainsKey(DataContext.Name)) // еще раз убедиться, для избежания дублирования выполнения
                        {
                            var loadedMenus = MenuCache[DataContext.Name];

                            if (loadedMenus != null && loadedMenus.Count > 0)
                            {
                                returnMenus.AddRange(
                                    loadedMenus.Select(
                                        menu => (BizMenu)CheckControlPermissions(CloneControl(menu), Permissions))
                                        .Where(bm => bm != null));

                                return returnMenus;
                            }
                        }

                        var menus =
                            DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Menu>()
                                .Where(m => m.Parent_Id == null && (m.Deleted == null || m.Deleted == false) &&
                                            (m.Invisible == null || m.Invisible == false))
                                .OrderBy(m => m.Order_Index).ToList();


                        MenuCache.Remove(DataContext.Name);
                        var menuList = new List<BizMenu>();
                        MenuCache.Add(DataContext.Name, menuList);

                        foreach (var menu in menus)
                        {
                            var bm = new BizMenu
                            {
                                Id = menu.Id,
                                Caption = menu.Full_Name,
                                DefaultCaption = menu.Full_Name,
                                Name = menu.Name,
                                DocStateId = menu.State_Type_Id,
                                FormId = menu.Form_Id,
                                ProcessId = menu.Process_Id,
                                Children = new List<BizControl>(),
                                Permissions = _permissionRepo.GetObjectDefPermissions(menu.Id),
                                LanguageId = 0
                            };

                            LoadChildMenus(bm, bm.Id);

                            menuList.Add((BizMenu) CloneControl(bm));

                            bm = (BizMenu) CheckControlPermissions(bm, Permissions);
                            if (bm == null) continue;

                            returnMenus.Add(bm);
                        }
                        /*MetadataForms.GetControlPanelMenus().ForEach(m =>
                        {
                            MenuCache.Add(m);
                            var mm = (BizMenu)CheckControlPermissions(m, Permissions);
                            if (mm != null) returnMenus.Add(mm);
                        });*/
                    }
                    finally
                    {
                        MenuCacheLock.DowngradeFromWriterLock(ref lc);
                    }
                }
                return returnMenus;
            }
            finally
            {
                MenuCacheLock.ReleaseReaderLock();
            }
        }

        public void HideControls(BizControl ctrl, List<string> controlNames)
        {
            if (controlNames == null) return;

            if (controlNames.FirstOrDefault(s => String.Equals(s, ctrl.Name, StringComparison.OrdinalIgnoreCase)) != null)
                ctrl.Invisible = true;

            if (ctrl is BizDocumentControl && ((BizDocumentControl)ctrl).DocForm != null)
                HideControls(((BizDocumentControl)ctrl).DocForm, controlNames);
            if (ctrl is BizDocumentListForm && ((BizDocumentListForm)ctrl).TableForm != null)
                HideControls(((BizDocumentListForm)ctrl).TableForm, controlNames);

            if (ctrl.Children != null)
                foreach (var child in ctrl.Children) HideControls(child, controlNames);
        }

        public void ShowControls(BizControl ctrl, List<string> controlNames)
        {
            if (controlNames == null) return;

            if (controlNames.FirstOrDefault(s => String.Equals(s, ctrl.Name, StringComparison.OrdinalIgnoreCase)) != null)
                ctrl.Invisible = false;

            if (ctrl is BizDocumentControl && ((BizDocumentControl)ctrl).DocForm != null)
                ShowControls(((BizDocumentControl)ctrl).DocForm, controlNames);
            if (ctrl is BizDocumentListForm && ((BizDocumentListForm)ctrl).TableForm != null)
                ShowControls(((BizDocumentListForm)ctrl).TableForm, controlNames);

            if (ctrl.Children != null)
                foreach (var child in ctrl.Children) ShowControls(child, controlNames);
        }

        public void SetReadOnlyControls(BizControl ctrl, bool value, List<string> controlNames)
        {
            if (controlNames == null) return;

            if (ctrl is BizDataControl &&
                controlNames.FirstOrDefault(s => String.Equals(s, ctrl.Name, StringComparison.OrdinalIgnoreCase)) != null)
                ((BizDataControl) ctrl).ReadOnly = true; 

            if (ctrl is BizDocumentControl && ((BizDocumentControl)ctrl).DocForm != null)
                SetReadOnlyControls(((BizDocumentControl)ctrl).DocForm, value, controlNames);
            if (ctrl is BizDocumentListForm && ((BizDocumentListForm)ctrl).TableForm != null)
                SetReadOnlyControls(((BizDocumentListForm)ctrl).TableForm, value, controlNames);

            if (ctrl.Children != null)
                foreach (var child in ctrl.Children) SetReadOnlyControls(child, value, controlNames);
        }

        private void LoadChildMenus(BizMenu menu, Guid menuId)
        {
            var children =
                DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Menu>()
                    .Where(m => m.Parent_Id == menuId && (m.Deleted == null || m.Deleted == false) &&
                                (m.Invisible == null || m.Invisible == false))
                    .OrderBy(m => m.Order_Index);

            foreach (var child in children)
            {
                var childMenu = new BizMenu
                                    {
                                        Id = child.Id,
                                        Caption = child.Full_Name,
                                        DefaultCaption = child.Full_Name,
                                        Name = child.Name,
                                        DocStateId = child.State_Type_Id,
                                        FormId = child.Form_Id,
                                        ProcessId = child.Process_Id,
                                        Children = new List<BizControl>(),
                                        Permissions = _permissionRepo.GetObjectDefPermissions(child.Id),
                                        LanguageId = 0
                                    };

                LoadChildMenus(childMenu, childMenu.Id);

                menu.Children.Add(childMenu);
            }
        }

        public bool ControlPermitted(BizControl control, PermissionSet permissions)
        {
            if (control == null) return false;

            if (permissions == null) return true;

            return permissions.IsSupersetOf(control.Permissions);
        }

        public BizControl CheckFormPermissions(BizControl form)
        {
            var frm = CheckControlPermissions(form, Permissions);

            if (frm == null)
            {
                throw new ApplicationException(
                    string.Format("Недостаточно прав для доступа к форме."));
            }
            if (frm is BizTableForm && Permissions != null)
            {
                var tableForm = (BizTableForm) frm;
                tableForm.AllowAddNew = tableForm.AddNewPermissionId == null ||
                                        Permissions.Contains((Guid) tableForm.AddNewPermissionId);
                tableForm.AllowOpen = tableForm.OpenPermissionId == null ||
                                        Permissions.Contains((Guid)tableForm.OpenPermissionId);
            }
            else if (frm is BizDetailForm)
            {
                var detailForm = (BizDetailForm)frm;

                detailForm.AllowDelete = detailForm.CanDelete;
                detailForm.AllowEdit = detailForm.CanEdit;
            }
            return frm;
        }

        public BizControl CheckControlPermissions(BizControl control, PermissionSet permissions)
        {
            if (control == null) return null;

            //if (control.Permissions == null) return control;
            if (permissions == null) permissions = new PermissionSet();
                /*{
                    return control.Permissions != null && control.Permissions.Items.Count > 0 ? null : control;
                }*/

            if (!permissions.IsSupersetOf(control.Permissions)) return null;

            if (control.Children != null)
            {
                var removedControls = new List<BizControl>();

                foreach (var child in control.Children)
                {
                    if (!permissions.IsSupersetOf(child.Permissions))
                        removedControls.Add(child);
                    else
                        CheckControlPermissions(child, permissions);
                }

                foreach (var removing in removedControls)
                    control.Children.Remove(removing);
            }
            if (control.QueryItems != null)
            {
                var removedItems = new List<QueryItemDefData>();

                foreach (var item in control.QueryItems)
                {
                    if (!permissions.IsSupersetOf(item.Permissions))
                        removedItems.Add(item);
                    else
                        CheckQueryItemPermissions(item, permissions);
                }
                foreach (var removing in removedItems)
                    control.QueryItems.Remove(removing);
            }

            return control;
        }

        public QueryItemDefData CheckQueryItemPermissions(QueryItemDefData item, PermissionSet permissions)
        {
            var initItem = CheckQueryItemChildrenPermissions(item, permissions);

            return initItem;
        }

        public QueryItemDefData CheckQueryItemChildrenPermissions(QueryItemDefData item, PermissionSet permissions)
        {
            if (item == null) return null;

            if (permissions == null) permissions = new PermissionSet();
/*
            {
                return item.Permissions.Items.Count > 0 ? null : item;
            }
*/
            if (!permissions.IsSupersetOf(item.Permissions)) return null;

            if (item.Items == null) return item;

            var removedItems = new List<QueryItemDefData>();

            foreach (var child in item.Items)
            {
                if (!permissions.IsSupersetOf(child.Permissions))
                    removedItems.Add(child);
                else
                    CheckQueryItemChildrenPermissions(child, permissions);
            }

            foreach (var removing in removedItems)
                item.Items.Remove(removing);

            return item;
        }

        /*private static IEnumerable<AttributeBase> GetDocAttributes(BizControl control)
        {
            var returnAttributes = new List<AttributeBase>();

/*
            if (control is BizEditText)
            {
                var edittxt = (BizEditText) control;
                returnAttributes.Add(edittxt.Attribute);
            }

            if (control is BizEditCurrency)
            {
                var edittxt = (BizEditCurrency)control;
                returnAttributes.Add(edittxt.Attribute);
            }

            if (control is BizEditFloat)
            {
                var edittxt = (BizEditFloat)control;
                returnAttributes.Add(edittxt.Attribute);
            }

            if (control is BizEditInt)
            {
                var edittxt = (BizEditInt)control;
                returnAttributes.Add(edittxt.Attribute);
            }

            if (control is BizEditDateTime)
            {
                var editDateTime = (BizEditDateTime)control;
                returnAttributes.Add(editDateTime.Attribute);
            }

            if (control is BizEditBool)
            {
                var editBool = (BizEditBool)control;
                returnAttributes.Add(editBool.Attribute);
            }

            if (control is BizComboBox)
            {
                var combo = (BizComboBox) control;
                returnAttributes.Add(combo.Attribute);
            }

            if (control.Children != null && control.Children.Count() > 0)
            {
                returnAttributes.AddRange(
                    control.Children.SelectMany(GetDocAttributes));
            }
#1#

            return returnAttributes;
        }*/

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
                    Logger.OutputLog(e, "FormRepository.Dispose");
                    throw;
                }
            }
        }

        ~FormRepository()
        {
            if (_ownDataContext && DataContext != null)
                try 
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "FormRepository.Finalize");
                    throw;
                }
        }
    }
}
