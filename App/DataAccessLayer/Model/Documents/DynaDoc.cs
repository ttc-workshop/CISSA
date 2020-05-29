using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    public class DynaDoc: DynamicObject, IDisposable
    {
        private IDataContext _dataContext;
//        private readonly bool _ownDataContext;
        public IDataContext DataContext
        {
            get
            {
                var multiDc = _dataContext as IMultiDataContext;
                if (multiDc != null) return multiDc.GetDocumentContext;
                return _dataContext;
            }
            private set { _dataContext = value; }
        }

        private readonly IDocRepository _docRepo;
        private readonly IDocStateRepository _docStateRepo;
        private readonly IUserRepository _userRepo;

        public Guid Id
        {
            get { return Doc.Id; }
            set { Doc.Id = value; }
        }

        public DocDef DocDef
        {
            get { return Doc.DocDef; } 
            set { Doc.DocDef = value; }
        }

        public DateTime CreationTime
        {
            get { return Doc.CreationTime; }
            set { Doc.CreationTime = value; }
        }

        public Doc Doc { get; private set; }
        public Guid UserId { get; private set; }

        public IAppServiceProvider Provider { get; private set; }

        private readonly bool _ownProvider = false;

        public DynaDoc(Doc doc, Guid userId, IAppServiceProvider provider)
        {
            if (doc == null)
                throw new ApplicationException("Не могу создать динамический документ. Документ не передан!");
            Doc = doc;

            Provider = provider;
            DataContext = provider.Get<IDataContext>();

            if (userId == Guid.Empty)
            {
                //var userData = Provider.Get<IUserDataProvider>();
                UserId = Provider.GetCurrentUserId();
            }
            else
                UserId = userId;

            _docRepo = Provider.Get<IDocRepository>();
            _docStateRepo = Provider.Get<IDocStateRepository>();
            _userRepo = Provider.Get<IUserRepository>();
        }

        public DynaDoc(Doc doc, IAppServiceProvider provider) : this(doc, Guid.Empty, provider) {}

        public DynaDoc(Doc doc, Guid userId, IDataContext dataContext)
        {
            if (doc == null)
                throw new ApplicationException("Не могу создать динамический документ. Документ не передан!");
            Doc = doc;

            var factory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = factory.Create();
            _ownProvider = true;

            if (dataContext == null)
            {
                DataContext = Provider.Get<IDataContext>();  //new DataContext();
                // _ownDataContext = true;
            }
            else
                DataContext = dataContext;

            UserId = userId;

            _docRepo = Provider.Get<IDocRepository>(); // new DocRepository(DataContext, UserId);
            _docStateRepo = Provider.Get<IDocStateRepository>(); // new DocStateRepository(DataContext);
            _userRepo = Provider.Get<IUserRepository>(); // new UserRepository(DataContext);
        }

//        public DynaDoc(Doc doc) : this(doc, Guid.Empty, null) {}
        public DynaDoc(Doc doc, Guid userId)
        {
            if (doc == null)
                throw new ApplicationException("Не могу создать динамический документ. Документ не передан!");
            Doc = doc;

            var factory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = factory.Create();
            _ownProvider = true;

            DataContext = Provider.Get<IDataContext>(); 

            if (userId == Guid.Empty)
            {
                // var userData = Provider.Get<IUserDataProvider>();
                UserId = Provider.GetCurrentUserId();
            }
            else
                UserId = userId;

            _docRepo = Provider.Get<IDocRepository>();
            _docStateRepo = Provider.Get<IDocStateRepository>();
            _userRepo = Provider.Get<IUserRepository>();
        }

        public DynaDoc(Guid docId, Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = factory.Create();
            _ownProvider = true;

            if (dataContext == null)
            {
                DataContext = Provider.Get<IDataContext>(); //new DataContext();
                // _ownDataContext = true;
            }
            else
                DataContext = dataContext;

            UserId = userId;

            _docRepo = Provider.Get<IDocRepository>();
            _docStateRepo = Provider.Get<IDocStateRepository>();
            _userRepo = Provider.Get<IUserRepository>();

            Doc = _docRepo.LoadById(docId);
        }

        public DynaDoc(Guid docId, Guid userId, IAppServiceProvider provider)
        {
            Provider = provider;
            if (userId == Guid.Empty)
            {
                // var userData = Provider.Get<IUserDataProvider>();
                UserId = provider.GetCurrentUserId();
            }
            else
                UserId = userId;

            _docRepo = Provider.Get<IDocRepository>();
            _docStateRepo = Provider.Get<IDocStateRepository>();
            _userRepo = Provider.Get<IUserRepository>();

            Doc = _docRepo.LoadById(docId);
        }

        public DynaDoc(Guid docId, IAppServiceProvider provider) : this(docId, Guid.Empty, provider) { }

        public DynaDoc(Guid docId, Guid userId) : this(docId, userId, (IDataContext) null) { }
//        public DynaDoc(Guid docId) : this(docId, Guid.Empty, null) { }

        /// <summary>
        /// Создает новый экземпляр документа
        /// </summary>
        /// <param name="docDefId">Идентификатор класса документа</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Документ</returns>
        public static DynaDoc CreateNew(Guid docDefId, Guid userId)
        {
            //using (var docRepo = new DocRepository(userId))

            //return new DynaDoc(/*docRepo.New(*/docDefId/*)*/, userId);

            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create())
            {
                var docRepo = provider.Get<IDocRepository>();

                return new DynaDoc(docRepo.New(docDefId), userId);
            }
        }

        public static DynaDoc CreateNew(Guid docDefId, Guid userId, IAppServiceProvider provider)
        {
            var docRepo = provider.Get<IDocRepository>();

            return new DynaDoc(docRepo.New(docDefId), userId, provider);
        }

        public DynaDoc CreateNew(Guid docDefId)
        {
            return new DynaDoc(_docRepo.New(docDefId), UserId, (IAppServiceProvider) (_ownProvider ? null : Provider));
        }

        /// <summary>
        /// Возвращает вложенный документ на который ссылается атрибут
        /// </summary>
        /// <param name="docAttrName">Наименование атрибута</param>
        /// <returns>Вложенный документ</returns>
        public Doc GetAttrDoc(string docAttrName)
        {
            //OutputLog("GetAttrDoc");
            var attr = Doc.Get<DocAttribute>(docAttrName);

            return _docRepo.GetNestingDocument(Doc, attr);
        }

        public Doc GetAttrDoc(DocAttribute attr)
        {
            //OutputLog("GetAttrDoc");
            return _docRepo.GetNestingDocument(Doc, attr);
        }

        public List<Guid> GetAttrDocIdList(string attrDocListName)
        {
            return GetAttrDocIdList(attrDocListName, (Doc)null);
        }

        public List<Guid> GetAttrDocIdList(DocListAttribute attr)
        {
            return GetAttrDocIdList(attr, null);
        }

        public List<Guid> GetAttrDocIdList(string attrDocListName, Doc filter)
        {
            int count;

            return _docRepo.DocAttrList(out count, Doc, attrDocListName, 0, 0, filter);
        }

        public List<Guid> GetAttrDocIdList(DocListAttribute attr, Doc filter)
        {
            int count;

            return _docRepo.DocAttrList(out count, Doc, attr, 0, 0, filter);
        }

        public List<Guid> GetAttrDocIdList(string attrDocListName, IDictionary<string, object> filter)
        {
            var attr = Doc.Get<DocListAttribute>(attrDocListName);

            if (attr.AttrDef.DocDefType == null)
                throw new ApplicationException(String.Format("Атрибут \"{0}\" не ссылается на класс документа", attr.AttrDef.Name));

            Doc filterDoc = null;

            if (filter != null)
            {
                filterDoc = _docRepo.New(attr.AttrDef.DocDefType.Id);

                foreach(var cond in filter)
                    filterDoc[cond.Key] = cond.Value;
            }

            return GetAttrDocIdList(attrDocListName, filterDoc);
        }

        public IEnumerable<Doc> GetAttrDocList(DocListAttribute attr)
        {
            return new DocList(Provider, GetAttrDocIdList(attr), UserId);
        }

        public IEnumerable<Doc> GetAttrDocList(string attrDocListName)
        {
            return new DocList(Provider, GetAttrDocIdList(attrDocListName), UserId);
        }

        public EnumValue GetAttrEnum(string attrEnumName)
        {
            var attr = Doc.Get<EnumAttribute>(attrEnumName);

            //OutputLog("GetAttrEnum");
            return GetAttrEnum(attr);
        }

        public EnumValue GetAttrEnum(EnumAttribute attr)
        {
            //OutputLog("GetAttrEnum");
            if (attr.Value != null)
            {
                using (var enumRepo = new EnumRepository(DataContext))
                    return enumRepo.GetValue((Guid) attr.Value);

                /*var item = DataContext.ObjectDefs.OfType<Enum_Item>().FirstOrDefault(i => i.Id == attr.Value);)

                if (item != null)
                    return new EnumValue
                               {
                                   Id = item.Id,
                                   Code = item.Name,
                                   Value = item.Full_Name
                               };*/
            }
            return null;
        }

        protected DocState SetDocStateType(Guid stateId, IDataContext dataContext)
        {
            if (IsStored())
            {
                _docRepo.SetDocState(Doc, stateId);
            }

            if (Doc.State != null && Doc.State.Id == stateId) return Doc.State;

            //var state = dataContext.ObjectDefs.OfType<Document_State_Type>().First(s => s.Id == stateId);
            //using (var docStateRepo = new DocStateRepository(DataContext))

            var state = _docStateRepo.LoadById(stateId);

            //using (var userRepo = new UserRepository(dataContext))

            var userInfo = _userRepo.GetUserInfo(UserId);

            if (state != null)
                Doc.State = new DocState
                {
                    Id = Guid.NewGuid(),
                    Type = new DocStateType
                    {
                        Id = stateId,
                        Name = state.Name ?? "",
                        ReadOnly = state.ReadOnly
                    },
                    WorkerId = UserId,
                    Created = DateTime.Now, //state.Created,
                    WorkerName =
                        userInfo != null
                            ? String.Format("{0} {1} {2}", userInfo.LastName, userInfo.FirstName,
                                userInfo.MiddleName)
                            : ""
                };
            return Doc.State;
        }

        public DocState SetDocStateType(Guid stateId)
        {
            return SetDocStateType(stateId, DataContext);
        }

        public DocState SetDocStateType(string stateName)
        {
            //using (var docStateRepo = new DocStateRepository(DataContext))

            var stateId = _docStateRepo.GetDocStateTypeId(stateName);

            return SetDocStateType(stateId, DataContext);
        }

        public bool IsStored()
        {
            //OutputLog("IsStored");
            return _docRepo.DocIsStored(Doc);
//            return DataContext.Documents.FirstOrDefault(d => d.Id == Id) != null;
        }

        public bool IsNew()
        {
            //OutputLog("IsNew");
            return Doc.IsNew;
        }

        public void Save()
        {
            //lock (DocRepository.DocSaveLock)
                _docRepo.Save(Doc);
        }

        public void AssignFrom(Doc source)
        {
            //OutputLog("AssignFrom");
            _docRepo.AssignDocTo(source, Doc);
        }

        public void AssignFrom(DynaDoc source)
        {
            //OutputLog("AssignFrom");
            _docRepo.AssignDocTo(source.Doc, Doc);
        }

        public void AssignTo(Doc target)
        {
            //OutputLog("AssignTo");
            _docRepo.AssignDocTo(Doc, target);
        }

        public void AssignTo(DynaDoc target)
        {
            //OutputLog("AssignTo");
            _docRepo.AssignDocTo(Doc, target.Doc);
        }

        public DynaDoc Clone()
        {
            //OutputLog("Clone");
            var clone = CreateNew(Doc.DocDef.Id, UserId, Provider);

            AssignTo(clone);

            return clone;
        }

        public void AddDocToList(DocListAttribute attr, Doc doc)
        {
            _docRepo.AddDocToList(doc, Doc, attr);
        }

        public void AddDocToList(DocListAttribute attr, DynaDoc doc)
        {
            _docRepo.AddDocToList(doc.Doc, Doc, attr);
        }

        public void AddDocToList(string attrDefName, Doc doc)
        {
            AddDocToList(Doc.Get<DocListAttribute>(attrDefName), doc);
        }

        public void AddDocToList(string attrDefName, DynaDoc doc)
        {
            AddDocToList(Doc.Get<DocListAttribute>(attrDefName), doc);
        }

        public void RemoveDocFromList(DocListAttribute attr, Doc doc)
        {
            _docRepo.RemoveDocFromList(doc.Id, Doc, attr);
        }

        public void RemoveDocFromList(DocListAttribute attr, DynaDoc doc)
        {
            _docRepo.RemoveDocFromList(doc.Id, Doc, attr);
        }

        public void RemoveDocFromList(string attrDefName, Doc doc)
        {
            RemoveDocFromList(Doc.Get<DocListAttribute>(attrDefName), doc);
        }

        public void RemoveDocFromList(string attrDefName, DynaDoc doc)
        {
            RemoveDocFromList(Doc.Get<DocListAttribute>(attrDefName), doc);
        }

        public void RemoveDocFromList(DocListAttribute attr, Guid docId)
        {
            _docRepo.RemoveDocFromList(docId, Doc, attr);
        }

        public void RemoveDocFromList(string attrDefName, Guid docId)
        {
            RemoveDocFromList(Doc.Get<DocListAttribute>(attrDefName), docId);
        }

        public void ClearDocAttr(DocAttribute attr)
        {
            //OutputLog("ClearDocAttr");
            attr.Value = null;
        }

        public void ClearDocAttr(string attrDefName)
        {
            var attr = Doc.Get<DocAttribute>(attrDefName);

            ClearDocAttr(attr);
        }

        public void ClearDocAttrList(DocListAttribute attr)
        {
            if (attr.AddedDocIds != null)
                attr.AddedDocIds.Clear();
            if (attr.AddedDocs != null)
                attr.AddedDocs.Clear();

            _docRepo.ClearAttrDocList(Id, attr.AttrDef.Id);
/*
            var list = GetAttrDocIdList(attr);

            //OutputLog("ClearDocAttrList");
            foreach (var docId in list)
            {
                _docRepo.RemoveDocFromList(docId, Doc, attr);
            }
*/
        }

        public void ClearDocAttrList(string attrDefName)
        {
            var attr = Doc.Get<DocListAttribute>(attrDefName);

            ClearDocAttrList(attr);
        }

        public int CalcAttrDocListCount(DocListAttribute attr)
        {
//            OutputLog("CalcAttrDocListCount");
            return _docRepo.CalcAttrDocListCount(Doc, attr);
        }

        public int CalcAttrDocListCount(string attrDefName)
        {
            var attr = Doc.Get<DocListAttribute>(attrDefName);

            return CalcAttrDocListCount(attr);
        }

        public double CalcAttrDocListSum(DocListAttribute attr, string sumAttrName)
        {
//            OutputLog("CalcAttrDocListSum");
            return _docRepo.CalcAttrDocListSum(Doc, attr, sumAttrName) ?? 0;
        }

        public double CalcAttrDocListSum(string attrDefName, string sumAttrName)
        {
            var attr = Doc.Get<DocListAttribute>(attrDefName);

            return CalcAttrDocListSum(attr, sumAttrName);
        }

        public Doc FirstAttrListDoc(DocListAttribute attr, Func<Doc, bool> func)
        {
//            OutputLog("FirstAttrListDoc");
            return GetAttrDocList(attr).FirstOrDefault(func);
        }

        public Doc LastAttrListDoc(DocListAttribute attr, Func<Doc, bool> func)
        {
//            OutputLog("LastAttrListDoc");
            return GetAttrDocList(attr).LastOrDefault(func);
        }

        public DocState State
        {
            get { return Doc.State ?? (Doc.State = _docRepo.GetDocState(Id)); }
        }

        public bool InState(string stateName)
        {
            var state = State;

            return (state != null && state.Type != null) &&
                   String.Equals(state.Type.Name, stateName, StringComparison.OrdinalIgnoreCase);
        }

        public bool InState(Guid stateTypeId)
        {
            var state = State;

            if (state == null && stateTypeId == Guid.Empty) return true;

            return state != null && state.Type != null && state.Type.Id == stateTypeId;
        }

        public bool InStates(params Guid[] stateTypes)
        {
            var state = State;

            if (state == null)
                return stateTypes.Any(stateTypeId => stateTypeId == Guid.Empty);

            return (state.Type != null) && 
                stateTypes.Where(stateTypeId => state.Type.Id == stateTypeId).Any();
        }

        public bool InStates(params string[] stateNames)
        {
            var state = State;

            if (state == null)
                return stateNames.Any(String.IsNullOrEmpty);

            return (/*state != null && */state.Type != null) &&
                stateNames.Any(stateName => String.Equals(state.Type.Name, stateName, StringComparison.OrdinalIgnoreCase));
        }

        public void CheckInState(string stateName)
        {
            if (!InState(stateName))
                throw new ApplicationException(String.Format("Документ не находится в состоянии \"{0}\"", stateName));
        }

        public void CheckInState(Guid stateTypeId)
        {
            if (!InState(stateTypeId))
            {
                //using (var docStateRepo = new DocStateRepository(DataContext))

                var state = _docStateRepo.LoadById(stateTypeId);
                throw new ApplicationException(String.Format("Документ не находится в состоянии \"{0}\"", state.Name));
            }
        }

        public void CheckNotInState(string stateName)
        {
            if (InState(stateName))
                throw new ApplicationException(String.Format("Документ не находится в состоянии \"{0}\"", stateName));
        }

        public void CheckNotInState(Guid stateTypeId)
        {
            if (InState(stateTypeId))
            {
                //using (var docStateRepo = new DocStateRepository(DataContext))

                var state = _docStateRepo.LoadById(stateTypeId);
                throw new ApplicationException(String.Format("Документ не находится в состоянии \"{0}\"", state.Name));
            }
        }

        public void CheckInStates(params Guid[] stateTypes)
        {
            if (!InStates(stateTypes))
                throw new ApplicationException(String.Format("Документ не находится в одном из состояний \"{0}\"", stateTypes));
        }

        public void CheckInStates(params string[] stateNames)
        {
            if (!InStates(stateNames))
                throw new ApplicationException(String.Format("Документ не находится в одном из состояний \"{0}\"", stateNames.ToString()));
        }

        public bool CanBeChanged()
        {
            var state = State;

            if (state != null && state.Type.ReadOnly)
                return false;

            return true;
        }

        public AttributeBase FindAttributeByName(string attributeName)
        {
            var query = from attr in Doc.Attributes
                        where attr.AttrDef.Name.ToUpper() == attributeName.ToUpper()
                        select attr;

            return query.FirstOrDefault();
        }

        public AttributeBase GetAttributeByName(string attributeName)
        {
            var attr = FindAttributeByName(attributeName);

            if (attr == null)
                throw new ApplicationException(
                    string.Format("Атрибута с именем {0} не существует.", attributeName));

            return attr;
        }

        public T Get<T>(string name) where T : AttributeBase
        {
            var attr = GetAttributeByName(name);

            var attr1 = attr as T;
            if (attr1 != null) return attr1;

            throw new Exception(String.Format("Ошибка в типе атрибута \"{0}\"", name));
        }

        public T Find<T>(string name) where T : AttributeBase
        {
            var attr = FindAttributeByName(name);

            if (attr == null || attr is T) return (T) attr;

            return null;
        }

        public object this[string attributeName]
        {
            get
            {
                var attribute = GetAttributeByName(attributeName);

                return attribute.ObjectValue;
            }
            set
            {
                var attribute = GetAttributeByName(attributeName);

                attribute.ObjectValue = value;
            }
        }

        public object GetValue(string name)
        {
            var attr = GetAttributeByName(name);

            if (attr != null) return attr.ObjectValue;

            throw new Exception(String.Format("Атрибут \"{0}\" не найден", name));
        }

        public T GetValue<T>(string name)
        {
            var value = GetValue(name);

            if (value is T) return (T) value;

            throw new Exception(String.Format("Ошибка в типе атрибута \"{0}\"", name));
        }

        public T FindValue<T>(string name)
        {
            var attr = GetAttributeByName(name);

            if (attr != null) return (T) attr.ObjectValue;

            return (T)(object)null;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Doc[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = Doc[binder.Name];
            return true;
        }
/*
        private void OutputLog(string methodName)
        {
            try
            {
                using (var writer = new StreamWriter(Logger.GetLogFileName("DynaDoc"), true))
                {
                    writer.WriteLine("{0}: User Id: \"{1}\"; method: \"{2}\"", DateTime.Now, UserId, methodName);
                }
            }
            catch
            {
            }
        }*/

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var query = from atr in Doc.Attributes
                        select atr.AttrDef.Name;

            return query.ToList();
        }

        public void Dispose()
        {
            try
            {
                /*if (_ownDataContext && DataContext != null)
                {
                    DataContext.Dispose();
                    DataContext = null;
                }*/
                if (_ownProvider && Provider != null)
                {
                    Provider.Dispose();
                    Provider = null;
                }
            }
            catch (Exception e)
            {
                Logger.OutputLog(e, "DynaDoc.Dispose");
                throw;
            }
        }
/*
        ~DynaDoc()
        {
            if (_ownDataContext && _dataContext != null)
                try
                {
                    _dataContext.Dispose();
                }
                catch (Exception e)
                {
                    DataContext.OutputLog(e, "DynaDoc.Finalize");
                    throw;
                }
        }*/
    }
}
