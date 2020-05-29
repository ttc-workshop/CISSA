using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.BizService
{
    public partial class BizService
    {
        private IDocRepository DocRepo { get; set; }
        private IEnumRepository EnumRepo { get; set; }

        private IDocDefRepository DocDefRepo { get; set; }

        public BizService(IDocRepository docRepository, string currentUserName)
        {
            CurrentUserName = currentUserName;
            DocRepo = docRepository;
        }
        
        /// <summary>
        /// Загружает документ по идентификатору
        /// </summary>
        /// <param name="documentId">Идентификатор загружаемого документа</param>
        /// <returns>Загруженный документ</returns>
        public Doc DocumentLoad(Guid documentId)
        {
            return DocRepo.LoadById(documentId);
        }

        /// <summary>
        /// Созадает новый документ по идентификатору типа документа
        /// </summary>
        /// <param name="docDefId">Идентификатор типа документа</param>
        /// <returns>Новый не инициализированный документ</returns>
        public Doc DocumentNew(Guid docDefId)
        {
            return DocRepo.New(docDefId);
        }

        public Doc DocumentInit(Doc doc, WorkflowContextData data)
        {
            return DocRepo.InitDocFrom(doc, data);
        }

        /// <summary>
        /// Сохраняет документ
        /// </summary>
        /// <param name="doc">Сохраняемый документ</param>
        /// <returns>Сохраненный документ</returns>
        public Doc DocumentSave(Doc doc)
        {
            //lock(DocRepository.DocSaveLock)
                return DocRepo.Save(doc);
        }

        /// <summary>
        /// Удаляет документ
        /// </summary>
        /// <param name="documentId">Идентификатор удаляемого документа</param>
        public void DocumentDelete(Guid documentId)
        {
            DocRepo.DeleteById(documentId);
        }

        /// <summary>
        /// Возвращает историю состояний документа
        /// </summary>
        /// <param name="docId">Идентификатор документа</param>
        /// <returns>Список состояний документа</returns>
        public List<DocState> DocumentStateList(Guid docId)
        {
            return DocRepo.GetDocumentStates(docId);
        }

        /// <summary>
        /// Загружает справочник
        /// </summary>
        /// <param name="enumId">Идентификатор справочника</param>
        /// <returns>Список значений справочника</returns>
        public List<EnumValue> GetEnumList(Guid enumId)
        {
            return new List<EnumValue>(EnumRepo.GetEnumItems(enumId));
        }

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageCount">Кол-во страниц</param>
        /// <returns>Список документов</returns>
        public List<Guid> DocumentList(out int pageCount, Guid docDefId, int pageNo, int pageSize = 0)
        {
            return DocRepo.List(out pageCount, docDefId, pageNo, pageSize);
        }

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttr">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        public List<Guid> DocumentFilterList(out int count, Guid docDefId, int pageNo, int pageSize = 0, Doc filter = null, Guid? sortAttr = null)
        {
            return DocRepo.List(out count, docDefId, pageNo, pageSize, filter, sortAttr);
        }

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="docStateId">Статус документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttr">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        public List<Guid> DocumentStateFilterList(out int count, Guid docDefId, Guid docStateId, int pageNo, int pageSize, Doc filter, Guid? sortAttr)
        {
            return DocRepo.List(out count, docDefId, docStateId, pageNo, pageSize, filter, sortAttr);
        }

        /// <summary>
        /// Загружает список документов из атрибута
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="document">Документ</param>
        /// <param name="attrDefId">Класс атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="filter">Фильтр</param>
        /// <param name="sortAttr">Атрибут сортировки</param>
        /// <returns>Список документов</returns>
        public List<Guid> DocAttrList(out int count, Doc document, Guid attrDefId, int pageNo, int pageSize, Doc filter = null,
                               Guid? sortAttr = null)
        {
            return DocRepo.DocAttrList(out count, document, attrDefId, pageNo, pageSize, filter, sortAttr);
        }

        /// <summary>
        /// Загружает список документов из атрибута
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docId">ID Документа</param>
        /// <param name="attrDefId">Класс атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="filter">Фильтр</param>
        /// <param name="sortAttr">Атрибут сортировки</param>
        /// <returns>Список документов</returns>
        public List<Guid> DocAttrListById(out int count, Guid docId, Guid attrDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttr)
        {
            return DocRepo.DocAttrListById(out count, docId, attrDefId, pageNo, pageSize, filter, sortAttr);
        }

        /// <summary>
        /// Возвращает вложенный документ
        /// </summary>
        /// <param name="document">Текущий документ</param>
        /// <param name="docAttr">Атрибут ссылающийся на вложенный документ</param>
        /// <returns>Вложенный документ</returns>
        public Doc GetNestingDocument(Doc document, DocAttribute docAttr)
        {
            return DocRepo.GetNestingDocument(document, docAttr);
        }

        /// <summary>
        /// Проверяет сохранен ли документ в БД
        /// </summary>
        /// <param name="document">Документ</param>
        /// <returns>Истина - сохранен</returns>
        public bool DocIsStored(Doc document)
        {
            return DocRepo.DocIsStored(document);
        }

        /// <summary>
        /// Проверяет содержится ли документ в списке
        /// </summary>
        /// <param name="docId">Идентификатор документа, который нужно проверить на вхождение в список</param>
        /// <param name="attrDocId">Идентификатор документа которому принадлежит список</param>
        /// <param name="attrDefId">Идентификатор атрибута-списка</param>
        /// <returns>Истина - имеется в списке</returns>
        public bool ExistsInDocList(Guid docId, Guid attrDocId, Guid attrDefId)
        {
            return DocRepo.ExistsInDocList(docId, attrDocId, attrDefId);
        }

        /// <summary>
        /// Добавить документ в список
        /// </summary>
        /// <param name="docId">Идентификатор документа, который необходимо добавить в список</param>
        /// <param name="document">Документ содержащий список</param>
        /// <param name="attrDefId">Идентификатор списка</param>
        /// <returns>Текущий документ</returns>
        public Doc AddDocToList(Guid docId, Doc document, Guid attrDefId)
        {
            return DocRepo.AddDocToList(docId, document, attrDefId);
        }

        public BlobData GetDocBlob(Guid docId, Guid attrDefId)
        {
            var imageData = DocRepo.GetBlobAttrData(docId, attrDefId);

            return imageData;
        }

        public BlobData GetDocImage(Guid docId, Guid attrDefId, int height = 0, int width = 0)
        {
            var imageData = DocRepo.GetBlobAttrData(docId, attrDefId);

            if (imageData == null)
                return null;

            if (width <= 0 && height <= 0) return imageData;

            using (var ms = new MemoryStream(imageData.Data))
            {
                /*using (var img = Image.FromStream(ms))
                {
                    var fH = (double)((height > 0 && img.Height > height) ? height / img.Height : 1);
                    var fW = (double)((width > 0 && img.Width > width) ? width / img.Width : 1);
                    var factor = Math.Min(fH, fW);
                    var abort = new Image.GetThumbnailImageAbort(ThumbnailCallback);
                    using (var thumbnail =
                        img.GetThumbnailImage((int)(img.Width * factor), (int)(img.Height * factor), abort,
                        IntPtr.Zero))
                    {*/
                using (var ms2 = new MemoryStream())
                {
                    // thumbnail.Save(ms2, ImageFormat.Jpeg);
                    ResizeImage(height, width, ms, ms2);
                    ms2.Position = 0;
                    return new BlobData {Data = ms.ToArray(), FileName = imageData.FileName};
                }
                 /*}
                }*/
            }
        }

        public void SaveDocImage(Guid docId, Guid attrDefId, byte[] data, string fileName)
        {
            DocRepo.SaveBlobAttrData(docId, attrDefId, data, fileName);
        }

        public List<DocDefName> GetDocDefNames()
        {
            return new List<DocDefName>(DocDefRepo.GetDocDefNames());
        }
    }
}