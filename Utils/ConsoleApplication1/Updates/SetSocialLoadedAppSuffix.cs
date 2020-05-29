﻿using System;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Organizations;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Updates
{
    public static class SetSocialLoadedAppSuffix
    {
        public static readonly Guid AppDocId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        public static readonly Guid PersonDocId = new Guid("{6F5B8A06-361E-4559-8A53-9CB480A9B16C}");

        public static void Start(IAppServiceProvider provider, IDataContext dataContext)
        {
            Console.WriteLine(@"Начало");
            SetSuffix(provider, dataContext);
        }

        public static void SetSuffix(IAppServiceProvider provider, IDataContext dataContext)
        {
            var query = new SqlQuery(provider, AppDocId, provider.GetCurrentUserId());
            query.JoinSource(query.Source, PersonDocId, SqlSourceJoinType.Inner, "Applicant");

            query.AddAttributes("&Id", "RegNo", "LastName");
            query.AddCondition(ExpressionOperation.And, AppDocId, "RegNo",
                               ConditionOperation.IsNotNull, null);
            query.AddCondition(ExpressionOperation.And, AppDocId, "RegNo",
                               ConditionOperation.NotLike, "%-s");
            query.AddCondition(ExpressionOperation.And, PersonDocId, "SOCIAL_ID",
                               ConditionOperation.IsNotNull, null);

            int i = 0;
            //using (var docRepo = new DocRepository())
            var docRepo = provider.Get<IDocRepository>();
            {
                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    var orgRepo = provider.Get<IOrgRepository>(); //new OrgRepository(docRepo.DataContext/*, Guid.Empty*/);

                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);
                        var regNo = reader.GetString(1);
                        var lastName = reader.GetString(2);
                        var now = DateTime.Now;

                        if (!regNo.EndsWith("-s"))
                        {
                            var doc = docRepo.LoadById(id);

                            doc["RegNo"] = regNo + "-s";

                            var orgInfo = doc.OrganizationId != null
                                              ? orgRepo.Get((Guid) doc.OrganizationId)
                                              : (OrgInfo) null;

                            docRepo.Save(doc);
                            Console.WriteLine(@"  {0}. {1}; {2}; {3}; {4} ms", i, regNo, lastName, orgInfo != null ? orgInfo.Name : "???", (DateTime.Now - now).TotalMilliseconds);
                            i++;
                        }
                    }
                }
            }
        }
    }
}
