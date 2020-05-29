using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Intersoft.Cissa.Report.Context;
using Intersoft.Cissa.Report.Defs;
using Intersoft.Cissa.Report.Styles;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Providers;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.BizService
{
    public partial class BizService
    {
        public ITemplateReportGeneratorProvider ReportGeneratorProvider { get; private set; }

        /// <summary>
        /// Формирует выходной документ из шаблона на основе документа и данных рабочего контекста
        /// </summary>
        /// <param name="document">Документ, из которого берутся данные для подстановки в шаблон</param>
        /// <param name="contextData">Контекст бизнес процесса, из которого берутся данные для подстановки в шаблон</param>
        /// <param name="fileName">Наименование файла - шаблона выходного документа в форматах (Excel, PDF)</param>
        /// <returns>Массив двоичных данных представляющих собой выходной файл</returns>
        public byte[] GenerateFromTemplate(Doc document, WorkflowContextData contextData, string fileName)
        {
            var context = new WorkflowContext(contextData, Provider);

            var generator = ReportGeneratorProvider.FindForTemplate(fileName);

            if (generator != null)
            {
                using (var stream = generator.Generate(document, fileName, context))
                {
                    using (var buf = new MemoryStream())
                    {
                        stream.Position = 0;
                        stream.CopyTo(buf);
                        return buf.GetBuffer();
                    }
                }
            }
            throw new ApplicationException(String.Format("Неизвестный тип файла \"{0}\"", fileName));
        }

        /// <summary>
        /// Формирует Excel документ из формы и запроса выборки данных 
        /// </summary>
        /// <param name="form">Форма, задающая оформление данных в выходном документе</param>
        /// <param name="queryDef">Запрос для выборки данных из БД</param>
        /// <returns>Массив двоичных данных тела Excel файла</returns>
        public byte[] ExcelFromQuery(BizForm form, QueryDef queryDef)
        {
            var sqb = _sqlQueryBuilderFactory.Create();
            using (var sqlQuery = sqb.Build(queryDef, form, null, null))
            {
                sqlQuery.AddAttribute("&Id");

                using (var sqlReader = _sqlQueryReaderFactory.Create(sqlQuery)/*new SqlQueryReader(DataContext, sqlQuery)*/)
                {
                    var defBuilder = new XlsGridDefBuilder(Provider, form, sqlReader);

                    using (var def = defBuilder.BuildFromBizForm())
                    {
                        var builder = new XlsBuilder(def);
                        using (var workbook = builder.Build())
                        {
                            using (var stream = new MemoryStream())
                            {
                                workbook.Write(stream);

                                return stream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Формирует Excel документ из формы и списка документов
        /// </summary>
        /// <param name="form">Форма, задающая оформление данных в выходном документе</param>
        /// <param name="docIdList">Список идентификаторов документов, которые необходимо вывести в файл</param>
        /// <returns>Массив двоичных данных тела Excel файла</returns>
        public byte[] ExcelFromDocIdList(BizForm form, List<Guid> docIdList)
        {
            var defBuilder = new XlsGridDefBuilder(DataContext, form, docIdList, CurrentUserId);

            using (var def = defBuilder.BuildFromBizForm())
            {
                var builder = new XlsBuilder(def);
                using (var workbook = builder.Build())
                {
                    using (var stream = new MemoryStream())
                    {
                        workbook.Write(stream);

                        return stream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Формирует Excel документ из формы и критериев выборки
        /// </summary>
        /// <param name="form">Форма, задающая оформление данных в выходном документе</param>
        /// <param name="docStateId">Идентификатор статуса документа в качестве критерия выборки документов из БД</param>
        /// <param name="filter">Форма фильтра с данными для ограничения выборки документов</param>
        /// <returns>Массив двоичных данных тела Excel файла</returns>
        public byte[] ExcelFromFilter(BizForm form, Guid? docStateId, Doc filter)
        {
            var sqb = _sqlQueryBuilderFactory.Create();
            using (var query = sqb.Build(form, docStateId, null, null))
            {
                SqlQueryExBuilder.AddDocConditions(query, query.Source, filter);

                using (var reader = _sqlQueryReaderFactory.Create(query))
                {
                    var defBuilder = new XlsGridDefBuilder(Provider, form, reader);
                    using (var def = defBuilder.BuildFromBizForm())
                    {
                        var builder = new XlsBuilder(def);
                        using (var workbook = builder.Build())
                        {
                            using (var stream = new MemoryStream())
                            {
                                workbook.Write(stream);

                                return stream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Формирует Excel документ из формы и критериев выборки
        /// </summary>
        /// <param name="form">Форма, задающая оформление данных в выходном документе</param>
        /// <param name="docStateId">Идентификатор статуса документа в качестве критерия выборки документов из БД</param>
        /// <param name="filter">Форма фильтра с данными для ограничения выборки документов</param>
        /// <returns>Массив двоичных данных тела Excel файла</returns>
        public byte[] ExcelFromFormFilter(BizForm form, Guid? docStateId, BizForm filter)
        {
            var sqb = _sqlQueryBuilderFactory.Create();

            using (var query = sqb.Build(form, docStateId, filter, null))
            {
                using (var reader = _sqlQueryReaderFactory.Create(query))
                {
                    var defBuilder = new XlsGridDefBuilder(Provider, form, reader);
                    using (var def = defBuilder.BuildFromBizForm())
                    {
                        var builder = new XlsBuilder(def);
                        using (var workbook = builder.Build())
                        {
                            using (var stream = new MemoryStream())
                            {
                                workbook.Write(stream);

                                return stream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        public byte[] ExcelFromDocumentListForm(Guid documentId, BizDocumentListForm docListForm)
        {
            var form = docListForm.TableForm;
            if (form == null && docListForm.FormId != null)
            {
                form = FormRepo.GetTableForm((Guid) docListForm.FormId);
            }
            if (form == null)
                throw new ApplicationException("Табличная форма не указана!");

            var sqb = _sqlQueryBuilderFactory.Create();

            if (docListForm.AttributeDefId != null)
            {
                using (var query = sqb.BuildAttrList(form, documentId, (Guid) docListForm.AttributeDefId, null, null))
                {
                    query.WithNoLock = true;

                    using (var reader = _sqlQueryReaderFactory.Create(query))
                    {
                        var defBuilder = new XlsGridDefBuilder(Provider, form, reader);
                        using (var def = defBuilder.BuildFromBizForm())
                        {
                            var builder = new XlsBuilder(def);
                            using (var workbook = builder.Build())
                            {
                                using (var stream = new MemoryStream())
                                {
                                    workbook.Write(stream);

                                    return stream.ToArray();
                                }
                            }
                        }
                    }
                }
            }

            if (docListForm.FormAttributeDefId != null)
            {
                using (var query = sqb.BuildRefList(form, documentId, (Guid)docListForm.FormAttributeDefId, null, null))
                {
                    using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(DataContext, query))
                    {
                        var defBuilder = new XlsGridDefBuilder(Provider, form, reader);
                        using (var def = defBuilder.BuildFromBizForm())
                        {
                            var builder = new XlsBuilder(def);
                            using (var workbook = builder.Build())
                            {
                                using (var stream = new MemoryStream())
                                {
                                    workbook.Write(stream);

                                    return stream.ToArray();
                                }
                            }
                        }
                    }
                }
            }

            throw new ApplicationException("Не могу сформировать Excel файл! Атрибут не указан!");
        }

        public byte[] ExcelFromDetailForm(BizForm form, Doc doc)
        {
            var factory = Provider.Get<IXlsFormDefBuilderFactory>();

            var defBuilder = factory.Create(form); 
            using (var def = defBuilder.Build(doc))
            {
                def.Style.Borders = TableCellBorder.All;

                var builder = new XlsBuilder(def);
                using (var workbook = builder.Build())
                {
                    using (var stream = new MemoryStream())
                    {
                        workbook.Write(stream);

                        return stream.ToArray();
                    }
                }
            }
        }

        public TableReportContext CreateTableReport(Guid docDefId)
        {
            var docDef = DocDefRepo.DocDefById(docDefId);
            var docDefRelations = DocDefRepo.GetDocDefRelations(docDefId);

            // var sourceItemId = Guid.NewGuid();
            var def = ReportDefHelper.Create(docDef);
            var context = new TableReportContext
            {
                Def = def,
                SourceRelations =
                    new List<ReportSourceRelations>(new[]
                    {
                        new ReportSourceRelations
                        {
                            SourceId = def.SourceId,
                            Relations = new List<DocDefRelation>(docDefRelations)
                        }
                    })
            };

            return context;
        }

        public TableReportContext DeserializeTableReport(byte[] data)
        {
            try
            {
                ReportDef def;

                var s = Encoding.UTF8.GetString(data).TrimEnd('\0');
                var pos = s.IndexOf("<", StringComparison.Ordinal);
                if (pos > 0) s = s.Substring(pos);

                //using (var stream = new StringStream(data))
                {
                    using (var streamReader = new StringReader(s/*, Encoding.UTF8*/))
                    {
                        var settings = new XmlReaderSettings
                        {
                            CheckCharacters = false
                        };
                        using (var reader = XmlTextReader.Create(streamReader, settings))
                        {
                            var serializer = new XmlSerializer(typeof (ReportDef));
                            def = (ReportDef) serializer.Deserialize(reader);
                        }
                    }
                }
                var relations = def.Sources.Select(sourceDef =>
                    new ReportSourceRelations
                    {
                        SourceId = sourceDef.Id,
                        Relations = new List<DocDefRelation>(DocDefRepo.GetDocDefRelations(sourceDef.DocDef.Id))
                    }).ToList();

                var context = new TableReportContext
                {
                    Def = def,
                    SourceRelations = relations
                };

                return context;
            }
            catch (Exception e)
            {
                try
                {
                    var fn = Logger.GetLogFileName("ReportManagerError");
                    Logger.OutputLog(fn, e, "DeserializeReportDef Error");
                }
                catch
                {
                    ;
                }
                throw;
            }
        }

        public byte[] SerializeTableReport(TableReportContext context)
        {
            try
            {
                //string s;
                var serializer = new XmlSerializer(typeof (ReportDef));
                using (var stream = new MemoryStream())
                {
                    context.Def.Check();

                    var settings = new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        Indent = true,
                        NewLineOnAttributes = true,
                    };

                    using (var writer = XmlTextWriter.Create(stream, settings)/*new StreamWriter(stream, Encoding.UTF8)*/)
                    {
                        serializer.Serialize(writer, context.Def); //(stream, context.Def);
                        stream.Position = 0;
                        return stream.GetBuffer();
                        /*
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            s = reader.ReadToEnd();
                        }*/
                    }
                }
                //return s;
            }
            catch (Exception e)
            {
                try
                {
                    var fn = Logger.GetLogFileName("ReportManagerError");
                    Logger.OutputLog(fn, e, "SerializeReportDef Error");
                }
                catch
                {
                    ;
                }
                throw;
            }
        }

        public TableReportContext JoinTableReportSource(TableReportContext context, Guid masterSourceId, Guid docDefId, Guid attrDefId)
        {
            var joinDef = context.Def.JoinDocDefSource(masterSourceId, DocDefRepo.DocDefById(docDefId), attrDefId);

            var docDefNames = DocDefRepo.GetDocDefNames();
            var relations =
                DocDefRepo.GetDocDefRelations(docDefId)
                    .Where(
                        r => docDefNames.Any(dd => dd.Id == r.DocDefId) && docDefNames.Any(dd => dd.Id == r.RefDocDefId))
                    .ToList();

            if (context.SourceRelations == null) context.SourceRelations = new List<ReportSourceRelations>();
            context.SourceRelations.Add(
                new ReportSourceRelations
                {
                    SourceId = joinDef.SourceId,
                    Relations = relations
                });

            return context;
        }

        public TableReportContext RemoveTableReportSource(TableReportContext context, Guid sourceId)
        {
            context.Def.RemoveSource(sourceId);
            return context;
        }

        public TableReportContext AddTableReportColumn(TableReportContext context, Guid sourceId, Guid attrDefId)
        {
            context.Def.AddColumn(sourceId, attrDefId);
            return context;
        }

        public TableReportContext AddTableReportColumns(TableReportContext context, Guid sourceId, Guid[] attrDefIds)
        {
            foreach (var attrDefId in attrDefIds)
                context.Def.AddColumn(sourceId, attrDefId);

            return context;
        }

        public TableReportContext RemoveTableReportColumn(TableReportContext context, Guid columnId)
        {
            context.Def.RemoveColumn(columnId);
            return context;
        }

        public TableReportContext AddTableReportParamCondition(TableReportContext context, Guid sourceId, Guid attrDefId,
            CompareOperation operation, object value, string paramCaption)
        {
            context.Def.AddParamCondition(sourceId, attrDefId, operation, value, paramCaption);
            return context;
        }

        public TableReportContext AddTableReportAttributeCondition(TableReportContext context, Guid leftSourceId, Guid leftAttrId,
            CompareOperation operation, Guid rightSourceId, Guid rightAttrId)
        {
            context.Def.AddAttributeCondition(leftSourceId, leftAttrId, operation, rightSourceId, rightAttrId);
            return context;
        }

        public TableReportContext AddTableReportCondition(TableReportContext context, Guid leftSourceId, Guid leftAttrId, ReportConditionDefType type = ReportConditionDefType.Param)
        {
            var conditionDef = context.Def.AddCondition(leftSourceId, leftAttrId, CompareOperation.Equal, type);
            if (type == ReportConditionDefType.Param)
            {
                var attr = context.Def.GetAttribute(conditionDef.LeftAttribute);
                var attrDef = attr as AttrDef;
                var sysAttrDef = attr as ReportSourceSystemAttributeDef;

                if (attrDef != null && attrDef.Type.Id == (short) CissaDataType.Enum && attrDef.EnumDefType != null)
                {
                    var rightPart = conditionDef.RightPart as ReportConditionRightParamDef;
                    if (rightPart != null)
                    {
                        var provider = Provider.Get<IEnumRepository>();
                        rightPart.Values = new List<EnumValue>(provider.GetEnumItems(attrDef.EnumDefType.Id));
                    }
                }
                else if (sysAttrDef != null && sysAttrDef.Ident == SystemIdent.State)
                {
                    var sourceDef = context.Def.GetSourceDef(leftSourceId);
                    var rightPart = conditionDef.RightPart as ReportConditionRightParamDef;
                    if (rightPart != null)
                    {
                        var provider = Provider.Get<IDocDefStateListProvider>();
                        rightPart.Values = provider.Get(sourceDef.DocDef.Id).Select(dst => new EnumValue{Id = dst.Id, Value = dst.Name}).ToList();
                    }
                }
            }
            return context;
        }

        public TableReportContext AddTableReportExpCondition(TableReportContext context)
        {
            context.Def.AddExpConditionDef();
            return context;
        }

        public TableReportContext RemoveTableReportCondition(TableReportContext context, Guid conditionId)
        {
            context.Def.RemoveCondition(conditionId);
            return context;
        }

        public byte[] ExecuteTableReport(TableReportContext context)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    // DONE: Реализовать генерацию отчета
                    var xlsDefBuilder = Provider.Get<IBuilder<ReportDef, XlsDef>>();
                    using (var xlsDef = xlsDefBuilder.Build(context.Def))
                    {
                        var builder = new XlsBuilder(xlsDef);
                        using (var workbook = builder.Build())
                        {
                            workbook.Write(stream);

                            return stream.ToArray();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    var fn = Logger.GetLogFileName("ReportManagerError");
                    Logger.OutputLog(fn, e, "ExecuteTableReport Error");
                }
                catch
                {
                    
                }
                throw;
            }
        }

        /// <summary>
        /// Получает файл по имени
        /// </summary>
        /// <param name="fileName">Имя файла находящегося на сервере</param>
        /// <returns>Массив двоичных данных тела файла</returns>
        public byte[] GetFile(string fileName)
        {
            using (var stream = new MemoryStream())
            {
                using (var file = new FileStream(fileName, FileMode.Open))
                {
                    file.CopyTo(stream);

                    return stream.GetBuffer();
                }
            }
        }
    }
}