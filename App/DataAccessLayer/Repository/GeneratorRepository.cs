using System;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public static class GeneratorRepository
    {
        public static readonly object Locker = new object();

        public static Int64 GetNewId(Guid orgId, Guid docDefId)
        {
            // DOТУ: Переделать!!!
            // using (var dataContext = new DataContext())
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create())
            {
                var mdc = provider.Get<IMultiDataContext>();

                return GetNewId(mdc, orgId, docDefId);
            }
        }

        public static Int64 GetNewId(IDataContext dataContext, Guid orgId, Guid docDefId)
        {
            var generator = new DocumentNumberGenerator(dataContext);
            return generator.GetNewId(orgId, docDefId);

            /*lock (Locker)
            {
                long id = 1;

                /*var mdc = dataContext as IMultiDataContext;
                var edc = mdc != null
                    ? mdc.GetDocumentContext.GetEntityDataContext()
                    : dataContext.GetEntityDataContext();
                var en = edc.Entities;#1#
                dataContext.BeginTransaction();
                try
                {
                    /*var gen =
                        en.Generators.FirstOrDefault(g => g.Organization_Id == orgId && g.Document_Def_Id == docDefId);

                    if (gen == null)
                    {
                        en.AddToGenerators(new Generator
                                               {
                                                   Organization_Id = orgId,
                                                   Document_Def_Id = docDefId,
                                                   Value = 1
                                               });
                    }
                    else
                    {
                        id = gen.Value ?? 1;
                        gen.Value = (gen.Value ?? 1) + 1;
                    }
                    edc.SaveChanges();
                    dataContext.Commit();#1#
                    using (var command = dataContext.CreateCommand(SelectSql))
                    {
                        AddParamWithValue(command, "@OrgId", orgId);
                        AddParamWithValue(command, "@DefId", docDefId);

                        var value = command.ExecuteScalar();

                        if (value == null)
                        {
                            id = (int) (value ?? 0) + 1;

                            using (var newValue = dataContext.CreateCommand(InsertSql))
                            {
                                AddParamWithValue(newValue, "@OrgId", orgId);
                                AddParamWithValue(newValue, "@DefId", docDefId);
                                AddParamWithValue(newValue, "@Value", id);

                                newValue.ExecuteNonQuery();
                            }
                        }
                    }
                    return id;
                }
                catch (Exception e)
                {
                    dataContext.Rollback();
                    Logger.OutputLog(e, "GeneratorRepository.GetNewId");
                    throw;
                }
            }*/
        }
    }
}
