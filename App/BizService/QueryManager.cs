using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.CISSA.BizService
{
    public partial class BizService
    {
        private readonly ISqlQueryBuilderFactory _sqlQueryBuilderFactory;
        private readonly ISqlQueryReaderFactory _sqlQueryReaderFactory;

        /// <summary>
        /// Возвращает список документов попадающих в запрос
        /// </summary>
        /// <param name="queryDef">Запрос на выборку данных</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество строк на странице</param>
        /// <returns>Список идентификаторв документов</returns>
        public List<Guid> GetDocList(QueryDef queryDef, int pageNo, int pageSize)
        {
            /*using (var query = new DocQuery(queryDef, DataContext))
            {
                return pageSize <= 0
                           ? query.All().ToList()
                           : query.Take(pageNo * pageSize, pageSize).ToList();
            }*/
            var sqb = _sqlQueryBuilderFactory.Create();
            using (var query = sqb.Build(queryDef))
            {
                query.AddAttribute("&Id");

                using (var reader = _sqlQueryReaderFactory.Create(query))
                {
                    reader.Open();
                    var i = reader.GetAttributeIndex("&Id");
                    var result = new List<Guid>();
                    while(reader.Read()) 
                        result.Add(reader.GetGuid(i));
                    return result;
                }
            }
        }

        /// <summary>
        /// Возвращает количество и список документов попадающих в запрос
        /// </summary>
        /// <param name="count">Количество документов попадающих под запрос в БД</param>
        /// <param name="queryDef">Запрос на выборку данных</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество строк на странице</param>
        /// <returns>Список идентификаторв документов</returns>        
        public List<Guid> GetDocListWithCount(out int count, QueryDef queryDef, int pageNo, int pageSize)
        {
            /*using (var query = new DocQuery(queryDef, DataContext))
            {
                count = query.Count();

                return pageSize <= 0
                           ? query.All().ToList()
                           : query.Take(pageNo * pageSize, pageSize).ToList();
            }*/
            var sqb = _sqlQueryBuilderFactory.Create();
            using (var query = sqb.Build(queryDef))
            {
                query.AddAttribute("&Id");

                using (var reader = _sqlQueryReaderFactory.Create(query))
                {
                    reader.Open();
                    count = reader.GetCount();

                    var i = reader.GetAttributeIndex("&Id");
                    var result = new List<Guid>();
                    while (reader.Read())
                        result.Add(reader.GetGuid(i));
                    return result;
                }
            }
        }

        /// <summary>
        /// Подсчитывает количество строк в запросе
        /// </summary>
        /// <param name="queryDef">Запрос на выборку данных</param>
        /// <returns>Количество строк</returns>
        public int GetQueryCount(QueryDef queryDef)
        {
            /*using (var query = new DocQuery(queryDef, DataContext))

                return query.Count();*/
            var sqb = _sqlQueryBuilderFactory.Create();
            using (var query = sqb.Build(queryDef))
            {
                using (var reader = _sqlQueryReaderFactory.Create(query))
                {
                    return reader.GetCount();
                }
            }
        }

        /// <summary>
        /// Формирует запрос из текущего документа
        /// </summary>
        /// <param name="document">Документ на основе которого будет формироваться структура запроса</param>
        /// <returns>Запрос на выборку данных из БД</returns>
        public QueryDef QueryFromDoc(Doc document)
        {
            using (var engine = new QueryEngine(DataContext))
            {
                return engine.QueryFromDoc(document);
            }
        }

        /// <summary>
        /// Формирует запрос из формы
        /// </summary>
        /// <param name="form">Форма на основе которого будет формироваться структура запроса</param>
        /// <returns>Запрос на выборку данных из БД</returns>        
        public QueryDef QueryFromForm(BizForm form)
        {
            using (var engine = new QueryEngine(DataContext))
                return engine.QueryFromForm(form);
        }

        /// <summary>
        /// Создает запрос на основе структуры и статуса документа
        /// </summary>
        /// <param name="docDefId">Идентификатор класса документа</param>
        /// <param name="docStateId">Статус документа, по которому будет производится выборка</param>
        /// <returns>Запрос на выборку данных из БД</returns>
        public QueryDef CreateQuery(Guid docDefId, Guid? docStateId)
        {
            var builder = new QueryBuilder(docDefId);

            if (docStateId != null) builder.Where("&state").Eq(docStateId);

            return builder.Def;
        }
    }
}