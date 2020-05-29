using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.BizServiceTests.FakeRepo
{
    class FakeDocRepository: IDocRepository
    {
        /// <summary>
        /// Сохраняет документ в БД
        /// </summary>
        /// <param name="document">Сохраняемый документ</param>
        /// <returns>Сохраненный дкоумент</returns>
        public Doc Save(Doc document)
        {
            return document;
        }

        /// <summary>
        /// Создает новый документ
        /// </summary>
        /// <param name="docDefId">Идентификатор описаниядокумента</param>
        /// <returns>Новый не инециализированный документ</returns>
        public Doc New(Guid docDefId)
        {
            return new Doc();
        }

        public Doc InitDocFrom(Doc doc, IStringParams prms)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Загружает документ из БД по идентификатору документа
        /// </summary>
        /// <param name="documentId">Идентификатор загружаемого документа</param>
        /// <returns>Загруженный документ</returns>
        public Doc LoadById(Guid documentId)
        {
            var doc = new Doc {Id = documentId};
            // doc.XXX     добавить инициализацию если необходимо

            return doc;
        }

        public Doc LoadById(Guid documentId, DateTime forDate)
        {
            var doc = new Doc { Id = documentId };
            // doc.XXX     добавить инициализацию если необходимо

            return doc;
        }

        /// <summary>
        /// Загружает список документов из БД по идентификатору формы
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <returns>Список загруженных документов</returns>
        public IList<Doc> LoadByFormId(Guid formId)
        {
            return new List<Doc>();
        }

        /// <summary>
        /// Удаляет документ из БД по идентификатору документа
        /// </summary>
        /// <param name="documentId">Идентификатор загружаемого документа</param>
        public void DeleteById(Guid documentId)
        {
            return;
        }

        public List<DocState> GetDocumentStates(Guid docId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Выполняет проверку переданного документа 
        /// </summary>
        /// <param name="document">Проверяемый документ</param>
        /// <returns>Проверенный и по возможности исправленный документ</returns>
        public Doc Check(Doc document)
        {
            return document;
        }

        public Doc CalculateAutoAttributes(Doc doc)
        {
            throw new NotImplementedException();
        }

        public List<Guid> Search(List<SearchParameter> parameters, LogicOperation logicOperation, DateTime forDate)
        {
            return new List<Guid>();
        }

        /// <summary>
        /// Загружает список документов на основании фильтра
        /// </summary>
        /// <param name="parameters">Список параметров</param>
        /// <param name="logicOperation">Логическая операция объединения параметров</param>
        /// <returns>Список найденых документов</returns>
        public List<Guid> Search(List<SearchParameter> parameters, LogicOperation logicOperation)
        {
            return new List<Guid>();
        }

        /// <summary>
        /// Получает текущее состояние документа для пользователя
        /// </summary>
        /// <param name="documentId">Идентификатор документа</param>
        /// <param name="dateTime">Дата на которую необходимо получить состояние документа</param>
        /// <returns>Состояние документа</returns>
        public DocState GetDocState(Guid documentId, DateTime dateTime)
        {
            return new DocState();
        }

        public void SetDocState(Guid documentId, Guid stateTypeId)
        {
            throw new NotImplementedException();
        }

        public void SetDocState(Doc document, Guid stateTypeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Устанавливает состояние документа для пользователя
        /// </summary>
        /// <param name="documentId">Идентификатор документа</param>
        /// <param name="stateTypeId">Идентификатор состояния</param>
        /// <returns></returns>
        public void SetDocState(Guid documentId, int stateTypeId)
        {
            return;
        }

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageCount">Кол-во страниц</param>
        /// <returns>Список документов</returns>
        public List<Guid> List(out int pageCount, Guid docDefId, int pageNo, int pageSize = 0)
        {
            pageCount = 1;
            return new List<Guid>();
        }

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        public List<Guid> List(out int count, Guid docDefId, int pageNo, int pageSize = 0, Doc filter = null, Guid? sortAttrId = null)
        {
            count = 20;
            return new List<Guid>();
        }

        public List<Guid> List(out int count, Guid docDefId, Guid docStateId, int pageNo, int pageSize, Doc filter, Guid? sortAttrId)
        {
            count = 2;
            return new List<Guid>();
        }

        public List<Guid> DocAttrList(out int count, Doc document, Guid attrDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttrId)
        {
            count = 1;
            return new List<Guid>();
        }

        public List<Guid> DocAttrList(out int count, Doc document, string attrDefName, int pageNo, int pageSize, Doc filter = null,
            Guid? sortAttrId = null)
        {
            throw new NotImplementedException();
        }

        public List<Guid> DocAttrList(out int count, Doc document, DocListAttribute attr, int pageNo, int pageSize, Doc filter = null,
            Guid? sortAttrId = null)
        {
            throw new NotImplementedException();
        }

        public List<Guid> DocAttrListById(out int count, Guid docId, Guid attrDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttrId)
        {
            count = 1;
            return new List<Guid>();
        }

        public Doc GetNestingDocument(Doc document, DocAttribute docAttr)
        {
            return document;
        }

        public bool DocIsStored(Doc document)
        {
            return true;
        }

        public bool DocExists(Guid docId)
        {
            throw new NotImplementedException();
        }

        public bool ExistsInDocList(Guid docId, Guid attrDocId, Guid attrDefId)
        {
            return false;
        }

        public Doc AddDocToList(Guid docId, Doc document, Guid attrDefId)
        {
            return document;
        }

        public Doc AddDocToList(Guid docId, Doc document, string attrName)
        {
            throw new NotImplementedException();
        }

        public Doc AddDocToList(Doc doc, Doc document, DocListAttribute attr)
        {
            throw new NotImplementedException();
        }

        public void RemoveDocFromList(Guid docId, Doc document, DocListAttribute attr)
        {
            throw new NotImplementedException();
        }

        public void ClearAttrDocList(Guid docId, Guid attrDefId)
        {
            throw new NotImplementedException();
        }

        public int CalcAttrDocListCount(Doc doc, DocListAttribute attr)
        {
            throw new NotImplementedException();
        }

        public double? CalcAttrDocListSum(Doc doc, DocListAttribute attr, string sumAttrName)
        {
            throw new NotImplementedException();
        }

        public object GetDocumentValue(Doc document, SystemIdent ident)
        {
            throw new NotImplementedException();
        }

        public BlobData GetBlobAttrData(Guid docId, AttrDef attrDef)
        {
            throw new NotImplementedException();
        }

        public BlobData GetBlobAttrData(Guid docId, Guid attrDefId)
        {
            throw new NotImplementedException();
        }

        public void SaveBlobAttrData(Guid docId, Guid attrDefId, byte[] data, string fileName)
        {
            throw new NotImplementedException();
        }

        public List<Guid> List(out int count, Guid docDefId, int docStateId, int pageNo, int pageSize, Doc filter, Guid? sortAttrId)
        {
            count = 20;
            return new List<Guid>();
        }

        public List<Guid> DocAttrList(out int count, Guid docId, Guid attrDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttrId)
        {
            count = 20;
            return new List<Guid>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
