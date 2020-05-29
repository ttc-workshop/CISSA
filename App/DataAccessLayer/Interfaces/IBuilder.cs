namespace Intersoft.CISSA.DataAccessLayer.Interfaces
{
    public interface IBuilder<in TSource, out TResult> // BuilderFromModel , PrototypeBasedBuilder, ObjectBuilder, ModelBasedBuilder
    {
        TResult Build(TSource source);
    }

    public interface IBuilder<in TSource1, in TSource2, out TResult>
    {
        TResult Build(TSource1 source1, TSource2 source2);
    }

    public interface IBuilder<in TSource1, in TSource2, in TSource3, out TResult>
    {
        TResult Build(TSource1 source1, TSource2 source2, TSource3 source3);
    }

    // Существующие методы трансформации
    // 1. QueryDef => SqlQuery
    //  var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
    //  using (var query = sqlQueryBuilder.Build(queryDef)) ...
    //  var sbBuilder = provider.Get<ITransformer<QueryDef, SqlQuery>>();
    //  using (var query = sbBuilder.Build(queryDef)) ...
    // 2. SqlQuery => SqlQueryReader
    // 3. BizForm => SqlQuery
    // 4. QueryDef, BizForm => SqlQuery
    // 5. QueryDef, BizForm, BizForm => SqlQuery

    // 6. XlsGridDefBuilder : (BizForm, SqlQueryReader) => XlsDef
    // 7. XlsGridDefBuilder : (BizForm, IList<Guid>) => XlsDef
    // 8. XlsGridReportDefBuilder : (BizForm, SqlQueryReader) => XlsDef
    // 9. XlsGridReportDefBuilder : (BizForm, IList<Guid>) => XlsDef

    // 10. BizForm => IXlsFormDefBuilder
    // 11. (BizForm, Doc) => XlsDef

    // 12. XlsDefToWordDefConverter : XlsDef => WordDocDef

    // Новые методы трансформации
    //  1. DocDef => SqlQuery
    //  2. SqlQuery => XlsDef
    //  3. (BizForm, SqlQuery) => XlsDef
    //  4. (BizForm, QueryDef) => XlsDef
    //  5. ReportDef => XlsDef
    //  6. ReportDef => SqlQuery
}
