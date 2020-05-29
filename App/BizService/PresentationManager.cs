using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Languages;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.BizService
{
    public partial class BizService
    {
        private IFormRepository FormRepo { get; set; }
        private ILanguageRepository LangRepo { get; set; }

        private readonly IComboBoxEnumProvider _comboBoxValueProvider;

        public BizService(IFormRepository formRepo, string currentUserName)
        {
            FormRepo = formRepo;
            CurrentUserName = currentUserName;
        }

        /// <summary>
        /// Загружает любую форму по идентификатору
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Загруженная форма</returns>
        public BizForm GetAnyForm(Guid formId, int languageId = 0)
        {
            return FormRepo.GetForm(formId, languageId);
        }

        /// <summary>
        /// Загружает детальную форму по идентификатору
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Загруженная форма</returns>
        //[SmartCache(TimeOutSeconds = 600)]
        public BizDetailForm GetDetailForm(Guid formId, int languageId = 0)
        {
            return FormRepo.GetDetailForm(formId, languageId);
        }

        public BizDetailForm GetDetailFormWithData(Guid formId, Guid docId, int languageId)
        {
            return FormRepo.GetDetailFormWithData(formId, docId, languageId);
        }
        /// <summary>
        /// Загружает табличную форму по идентификатору формы
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Загруженная табличная форма</returns>
        //[SmartCache(TimeOutSeconds = 600)]
        public BizTableForm GetGridForm(Guid formId, int languageId = 0)
        {
            return FormRepo.GetTableForm(formId, languageId);
        }

        /// <summary>
        /// Записывает значения атрибутов документа в форму
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="document">Документ</param>
        /// <returns>Форма с данными</returns>
        public BizForm SetFormDoc(BizForm form, Doc document)
        {
            return (BizForm) FormRepo.SetFormDoc(form, document);
        }

        /// <summary>
        /// Записывает значения из формы в документ
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="document">Документ</param>
        /// <returns>Измененный документ</returns>
        public Doc GetFormDoc(BizForm form, Doc document)
        {
            return FormRepo.GetFormDoc(form, document);
        }

/*
        [Trace("Presentation")]
        public List<BizControl> GetTableFormRows(BizTableForm form, List<Guid> docIds)
        {
            return FormRepo.GetTableFormRows(form, docIds);
        }
*/
        /// <summary>
        /// Загружает главную форму
        /// </summary>
        /// <returns>Главная форма</returns>
        public BizForm GetMainForm()
        {
            var mainForm = new BizForm { Children = new List<BizControl>() };

/*
            var btn = new BizButton
                          {
                              Caption = "Test form open",
                              Id = Guid.Parse("3EECC26D-160D-4471-A886-5289B818798F"),
                              ActionId = Guid.Parse("B88FE6CA-D327-44C5-BC84-37F47D85FB4A"),
                              ActionType = BizButtonActionType.Form
                          };

            mainForm.Children.Add(btn);

            var em = new cissaEntities();

            foreach (var table in em.Object_Defs.OfType<Table_Form>().Where(f => f.Deleted == null || f.Deleted == false))
            {
                btn = new BizButton
                          {
                              Caption = table.Full_Name,
                              Id = Guid.NewGuid(),
                              ActionId = table.Id,
                              ActionType = BizButtonActionType.Form
                          };
                mainForm.Children.Add(btn);
            }

            foreach (var process in em.Object_Defs.OfType<Workflow_Process>().Where(p => p.Deleted == null || p.Deleted == false))
            {
                btn = new BizButton
                          {
                              Caption = process.Name,
                              Id = Guid.NewGuid(),
                              ProcessId = process.Id,
                              ActionType = BizButtonActionType.BizProcess
                          };
                mainForm.Children.Add(btn);
            }
*/
            return mainForm;
        }

/*
        /// <summary>
        /// Выполняет бизнес операцию
        /// </summary>
        /// <param name="actionId">Идентификатор безнес операции</param>
        /// <returns>Бизнес результат операции</returns>
        public BizResult ExecuteBizAction(Guid actionId)
        {
            throw new NotImplementedException();
        }
*/

        /// <summary>
        /// Возвращает дерево меню для пользователя
        /// </summary>
        /// <param name="languageId">Код языка</param>
        /// <returns>Список меню</returns>
        //[SmartCache(TimeOutSeconds = 3600)]
        public IList<BizMenu> GetMenus(int languageId = 0)
        {
            return FormRepo.GetMenus(languageId);
        }

        /// <summary>
        /// Возвращает список языков
        /// </summary>
        /// <returns>Список меню</returns>
        public IList<LanguageType> GetLanguages()
        {
            //using (var langRepo = new LanguageRepository(DataContext))
            return LangRepo.Load();
        }

        /// <summary>
        /// Переводит форму на указанный язык
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Форма</returns>
        public BizForm TranslateForm(BizForm form, int languageId)
        {
            FormRepo.TranslateForm(form, languageId);

            return form;
        }

        /// <summary>
        /// Переводит список меню на заданный язык
        /// </summary>
        /// <param name="menus">Список меню</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Список меню</returns>
        public List<BizMenu> TranslateMenus(List<BizMenu> menus, int languageId)
        {
            FormRepo.TranslateMenus(menus, languageId);

            return menus;
        }

        /// <summary>
        /// Переводит список пользовательских действий на заданный язык
        /// </summary>
        /// <param name="userActions">Список пользовательских действий</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Список пользовательских действий</returns>
        public List<UserAction> TranslateUserActions(List<UserAction> userActions, int languageId)
        {
            WorkflowRepo.TranslateUserActions(userActions, languageId);

            return userActions;
        }

        /// <summary>
        /// Возвращает список строк с данными табличной формы с учетом критерием фильтра
        /// </summary>
        /// <param name="count">Возвращает количество строк данных в БД</param>
        /// <param name="form">Табличная форма, для которой нужны строки данных</param>
        /// <param name="docStateId">Статус документов, отображаемые в табличной форме</param>
        /// <param name="filter">Форма фильтра с данными</param>
        /// <param name="sortAttrs">Список атрибутов с условиями сортировки</param>
        /// <param name="pageNo">Номер отображаемой страницы</param>
        /// <param name="pageSize">Количество отображаемых строк в таблице</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        public List<BizControl> GetTableFormRows(out int count, BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return FormRepo.GetTableFormRows(out count, form, docStateId, filter, sortAttrs, pageNo, pageSize);
        }
        public List<BizControl> GetTableFormRowData(BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return FormRepo.GetTableFormRows(form, docStateId, filter, sortAttrs, pageNo, pageSize);
        }
        public int GetTableFormRowCount(BizForm form, Guid? docStateId, BizForm filter)
        {
            return FormRepo.GetTableFormRowCount(form, docStateId, filter);
        }

        /// <summary>
        /// Возвращает список строк табличной формы с данными, получаемыми из запроса выборки
        /// </summary>
        /// <param name="count">Количество строк попадаемых в выборку</param>
        /// <param name="form">Табличная форма</param>
        /// <param name="def">Запрос выборки</param>
        /// <param name="sortAttrs">Список атрибутов сотрировки</param>
        /// <param name="pageNo">Номер отображаемой страницы в табличной форме</param>
        /// <param name="pageSize">Количество отображаемых строк в форме</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        public List<BizControl> GetTableFormRowsFromQuery(out int count, BizForm form, QueryDef def, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return FormRepo.GetTableFormRows(out count, form, def, sortAttrs, pageNo, pageSize);
        }
        public List<BizControl> GetTableFormRowsFromFilterQuery(out int count, BizForm form, QueryDef def, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return FormRepo.GetTableFormRows(out count, form, def, filter, sortAttrs, pageNo, pageSize);
        }
        public List<BizControl> GetTableFormRowDataFromQuery(BizForm form, QueryDef def, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return FormRepo.GetTableFormRows(form, def, sortAttrs, pageNo, pageSize);
        }

        public List<BizControl> GetTableFormRowDataFromFilterQuery(BizForm form, QueryDef def, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo,
            int pageSize)
        {
            return FormRepo.GetTableFormRows(form, def, filter, sortAttrs, pageNo, pageSize);
        }

        public int GetTableFormRowCountFromQuery(BizForm form, QueryDef def)
        {
            return FormRepo.GetTableFormRowCount(form, def);
        }

        public int GetTableFormRowCountFromFilterQuery(BizForm form, QueryDef def, BizForm filter)
        {
            return FormRepo.GetTableFormRowCount(form, def, filter);
        }

        /// <summary>
        /// Возвращает список строк табличной формы с данными, формируемые из списка документов
        /// </summary>
        /// <param name="form">Табличная форма</param>
        /// <param name="docIds">Список идентификаторов документов, которые необходимо высветить в таблице</param>
        /// <param name="sortAttrs">Список атрибутов сортировки строк</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество строк в странице</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        public List<BizControl> GetTableFormRowsFromList(BizForm form, IEnumerable<Guid> docIds,
                                          IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return FormRepo.GetTableFormRows(form, docIds, sortAttrs, pageNo, pageSize);
        }

        /// <summary>
        /// Возвращает список строк табличной формы с данными, формируемые из списочного атрибута документа
        /// </summary>
        /// <param name="count">Количество документов хранящихся в списочном атрибуте</param>
        /// <param name="form">Таличная форма</param>
        /// <param name="docId">Идентификатор документа, содержащего списочный атрибут</param>
        /// <param name="attrDefId">Идентификатор списочного атрибута</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество отображаемых строк в таблице</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        public List<BizControl> GetDocListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            return FormRepo.GetDocListTableFormRows(out count, form, docId, attrDefId, pageNo, pageSize);
        }
        public List<BizControl> GetDocListTableFormRowData(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            return FormRepo.GetDocListTableFormRows(form, docId, attrDefId, pageNo, pageSize);
        }
        public int GetDocListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId)
        {
            return FormRepo.GetDocListTableFormRowCount(form, docId, attrDefId);
        }

        /// <summary>
        /// Возвращает список строк табличной формы с данными, формируемые из списка документов ссылающихся на указанный документ
        /// </summary>
        /// <param name="count">Количество документов попадающих под запрос</param>
        /// <param name="form">Табличная форма</param>
        /// <param name="docId">Идентификатор документа</param>
        /// <param name="attrDefId">Идентификатор ссылочного атрибута</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество строк на странице</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        public List<BizControl> GetRefListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            return FormRepo.GetRefListTableFormRows(out count, form, docId, attrDefId, pageNo, pageSize);
        }
        public List<BizControl> GetRefListTableFormRowData(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            return FormRepo.GetRefListTableFormRows(form, docId, attrDefId, pageNo, pageSize);
        }
        public int GetRefListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId)
        {
            return FormRepo.GetRefListTableFormRowCount(form, docId, attrDefId);
        }

        public IList<ModelMessage> GetFormErrors(BizForm form, IList<ModelMessage> errors)
        {
            return FormRepo.GetFormErrors(form, errors);
        }

        public BizForm SetFormOptions(BizForm form, IDictionary<Guid, IList<BizControlOption>> formOptions)
        {
            if (form != null && formOptions != null)
            {
                IList<BizControlOption> options;
                
                if (formOptions.TryGetValue(form.Id, out options))
                    return FormRepo.SetFormOptions(form, options);
            }
            return form;
        }

        public BizForm SetFormControlOptions(BizForm form, IList<BizControlOption> formOptions)
        {
            if (form != null && formOptions != null)
            {
                return FormRepo.SetFormOptions(form, formOptions);
            }
            return form;
        }

        public byte[] GetImage(Guid formId, Guid imageId, int height = 0, int width = 0)
        {
            var form = FormRepo.GetForm(formId);
            var finder = new ControlFinder(form);
            var image = finder.Find(imageId) as BizImage;

            if (image == null || image.ImageBytes == null)
                return null;

            if (width <= 0 && height <= 0) return image.ImageBytes;

            using (var ms = new MemoryStream(image.ImageBytes))
            {
                using (var ms2 = new MemoryStream())
                {
                    ResizeImage(height, width, ms, ms2);
                    ms2.Position = 0;
                    return ms2.ToArray();
                }
            }
        }

        public IList<EnumValue> GetFormComboBoxValues(BizForm form, BizComboBox comboBox)
        {
            // return FormRepo.GetFormComboBoxValueList(form, comboBox);
            return _comboBoxValueProvider.GetFormComboBoxValues(form, comboBox);
        }

        private void ResizeImage(int height, int width, Stream fromStream, Stream toStream)
        {
            using (var image = Image.FromStream(fromStream))
            {
                var fH = (double)((height > 0 && image.Height > height) ? height / image.Height : 1);
                var fW = (double)((width > 0 && image.Width > width) ? width / image.Width : 1);
                ResizeImage(Math.Min(fH, fW), image, toStream);
            }
        }
        private static void ResizeImage(double scaleFactor, Image image, Stream toStream)
        {
            var newWidth = (int)(image.Width * scaleFactor);
            var newHeight = (int)(image.Height * scaleFactor);
            using (var thumbnailBitmap = new Bitmap(newWidth, newHeight))
            {
                using (var thumbnailGraph = Graphics.FromImage(thumbnailBitmap))
                {
                    thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                    thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                    thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                    thumbnailGraph.DrawImage(image, imageRectangle);

                    thumbnailBitmap.Save(toStream, image.RawFormat);
                }
            }
        }
       
        public bool ThumbnailCallback()
        {
            return false;
        }
    }
}