using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Updates
{
    public static class FixLoadedSocialApplication
    {
        public static readonly Guid AppDocId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        public static readonly Guid PersonDocId = new Guid("{6F5B8A06-361E-4559-8A53-9CB480A9B16C}");
        public static readonly Guid AssignmentDocId = new Guid("{5D599CE4-76C5-4894-91CC-4EB3560196CE}");

        public static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        public static void Start(IAppServiceProvider provider, IDataContext dataContext)
        {
            Console.WriteLine(@"Начало");
            FixApplications2(provider, dataContext);
        }

        public static void FixApplications(IAppServiceProvider provider, IDataContext dataContext)
        {
            var query = new SqlQuery(provider, AppDocId, provider.GetCurrentUserId());
            var assignmentSource = query.JoinSource(query.Source, AssignmentDocId, SqlSourceJoinType.Inner, "Assignments");

            query.AddAttributes("&Id", "RegNo", "&OrgName");
            query.AddAttribute(assignmentSource, "EffectiveDate");
            query.AddAttribute(assignmentSource, "ExpiryDate");
            query.AddAttribute(assignmentSource, "IsUnlimited");
            query.AddAttribute(assignmentSource, "Amount");
            query.AddCondition(ExpressionOperation.And, AppDocId, "&State",
                               ConditionOperation.Equal, ApprovedStateId);
            query.AddCondition(ExpressionOperation.And, AppDocId, "AssignFrom",
                               ConditionOperation.IsNull, null);
            query.AddCondition(ExpressionOperation.Or, AppDocId, "AssignTo",
                               ConditionOperation.IsNull, null);
            query.AddCondition(ExpressionOperation.Or, AppDocId, "PaymentSum",
                               ConditionOperation.IsNull, null);

            int i = 0;
            // using (var docRepo = new DocRepository())
            var docRepo = provider.Get<IDocRepository>();
            {
                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    // var orgRepo = new OrgRepository(docRepo.DataContext /*, Guid.Empty*/);
                    var orgRepo = provider.Get<IOrgRepository>();

                    var sql = reader.GetSql();
                    Console.WriteLine(sql);
                    
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);
                        var regNo = reader.GetString(1);
                        var orgId = reader.GetGuid(2);
                        var effectiveDate = reader.IsDbNull(3) ? (DateTime?) null : reader.GetDateTime(3);
                        var expiryDate = reader.IsDbNull(4) ? (DateTime?) null : reader.GetDateTime(4);
                        var isUnlimited = reader.IsDbNull(5) ? (bool?) null : reader.GetBoolean(5);
                        var paymentSum = reader.IsDbNull(6) ? (decimal?) null : reader.GetDecimal(6);
                        var now = DateTime.Now;

                        var app = docRepo.LoadById(id);
                        if (app["AssignFrom"] == null && effectiveDate != null)
                            app["AssignFrom"] = effectiveDate;
                        if (app["AssignTo"] == null && expiryDate != null)
                            app["AssignTo"] = expiryDate;
                        if (app["IsUnlimited"] == null && isUnlimited != null)
                            app["IsUnlimited"] = isUnlimited;
                        if (app["PaymentSum"] == null && paymentSum != null)
                            app["PaymentSum"] = docRepo.CalcAttrDocListSum(app, app.Get<DocListAttribute>("Assignments"), "Amount");

                        docRepo.Save(app);
                        var orgInfo = orgRepo.Get(orgId);

                        Console.WriteLine(@"  {0}. {1}; {2}; {3} ms", i, regNo, orgInfo != null ? orgInfo.Name : "???", (DateTime.Now - now).TotalMilliseconds);
                        i++;
                    }
                }
            }
        }

        public static void FixApplications2(IAppServiceProvider provider, IDataContext dataContext)
        {
            // using (var docRepo = new DocRepository())
            var docRepo = provider.Get<IDocRepository>();
            {
                using (var connection = new SqlConnection(dataContext.StoreConnection.ConnectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = SelectSql;
                        command.Connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            int i = 0;
                            while (reader.Read())
                            {
                                var id = reader.GetGuid(0);
                                var regNo = reader.GetString(1);
                                var orgName = reader.GetString(2);
                                var assignFrom = reader.IsDBNull(3) ? (DateTime?) null : reader.GetDateTime(3);
                                var assignTo = reader.IsDBNull(4) ? (DateTime?) null : reader.GetDateTime(4);
                                var paymentSum = reader.IsDBNull(5) ? (decimal?) null : reader.GetDecimal(5);
                                var effectiveDate = reader.IsDBNull(6) ? (DateTime?) null : reader.GetDateTime(6);
                                var expiryDate = reader.IsDBNull(7) ? (DateTime?) null : reader.GetDateTime(7);
                                var isUnlimited = reader.IsDBNull(8) ? (bool?) null : reader.GetBoolean(8);
                                var amount = reader.IsDBNull(9) ? (decimal?) null : reader.GetDecimal(9);
                                var now = DateTime.Now;

                                var app = docRepo.LoadById(id);
                                if (assignFrom == null && effectiveDate != null)
                                    app["AssignFrom"] = effectiveDate;
                                if (assignTo == null && expiryDate != null)
                                    app["AssignTo"] = expiryDate;
                                if (app["IsUnlimited"] == null && isUnlimited != null)
                                    app["IsUnlimited"] = isUnlimited;
                                if (paymentSum == null && amount != null)
                                {
                                    paymentSum = Convert.ToDecimal(docRepo.CalcAttrDocListSum(app, app.Get<DocListAttribute>("Assignments"), "Amount"));
                                    app["PaymentSum"] = paymentSum;
                                }

                                docRepo.Save(app);
                                var s = String.Format("- {0}. {1}; {2}; \"{3}\"; {4} ms", i, regNo, orgName, id,
                                                      (DateTime.Now - now).TotalMilliseconds);
                                Console.WriteLine(s);
                                OutputLog(s);
                                i++;
                            }
                        }
                    }
                }
            }
        }

        public static void OutputLog(string msg)
        {
            try
            {
                using (var writer = new StreamWriter("c:\\distr\\cissa\\FixSocialApplicationDates.log", true))
                {
                    writer.WriteLine(@"{0} {1}", DateTime.Now, msg);
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        private const string SelectSql = @"SELECT
	[SocialApplication].[Id1],
	[SocialApplication].[RegNo],
	(select Full_Name from Object_Defs 
	 where Id = [SocialApplication].[OrgId]) as OrgName,
	[SocialApplication].[AssignFrom],
	[SocialApplication].[AssignTo],
	[SocialApplication].[PaymentSum],
	[Assignment].[EffectiveDate],
	[Assignment].[ExpiryDate],
	[Assignment].[IsUnlimited],
	[Assignment].[Amount]
FROM
	(SELECT
		d.Id,
		d.Id as [Id1],
		[a2].[Value] as [RegNo],
		d.Organization_Id as [OrgId],
		dla4.Value as [Assignments],
		ds5.State_Type_Id as [State],
		[a6].[Value] as [AssignFrom],
		[a7].[Value] as [AssignTo],
		[a8].[Value] as [PaymentSum]
	 FROM
		Documents d WITH(NOLOCK)
		INNER JOIN Document_States ds5 WITH(NOLOCK) ON (ds5.Document_Id = d.Id and ds5.Expired = '99991231' and ds5.State_Type_Id = '{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}')
		LEFT OUTER JOIN Text_Attributes a2 WITH(NOLOCK) on (a2.Document_Id = d.Id and a2.Def_Id = 'f4d191e9-64f5-4d3d-9f6d-04c8171cb692' and a2.Expired = '99991231')
		LEFT OUTER JOIN DocumentList_Attributes dla4 WITH(NOLOCK)
			ON (dla4.Document_Id = d.Id AND dla4.Def_Id = '39e1960a-9593-4532-aa79-fbcba7d24bf3' AND dla4.Expired = '99991231') 
		LEFT OUTER JOIN Date_Time_Attributes a6 WITH(NOLOCK) on (a6.Document_Id = d.Id and a6.Def_Id = 'dba721e2-411d-46f0-b016-b1dfe0071731' and a6.Expired = '99991231')
		LEFT OUTER JOIN Date_Time_Attributes a7 WITH(NOLOCK) on (a7.Document_Id = d.Id and a7.Def_Id = '0261ac3c-1ddb-4d94-a5d8-733733f8905e' and a7.Expired = '99991231')
		LEFT OUTER JOIN Currency_Attributes a8 WITH(NOLOCK) on (a8.Document_Id = d.Id and a8.Def_Id = '516dc981-99a6-4e12-a64c-c6039a91279e' and a8.Expired = '99991231')
	 WHERE
		d.Def_Id = '04d25808-6de9-42f5-8855-6f68a94a224c'
	) as [SocialApplication]
	INNER JOIN 
		(SELECT
			d.Id,
			[a1].[Value] as [EffectiveDate],
			[a2].[Value] as [ExpiryDate],
			[a3].[Value] as [IsUnlimited],
			[a4].[Value] as [Amount]
		 FROM
			Documents d WITH(NOLOCK)
			LEFT OUTER JOIN Date_Time_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '3b693c49-13e8-4f40-8418-2c869916bbc0' and a1.Expired = '99991231')
			LEFT OUTER JOIN Date_Time_Attributes a2 WITH(NOLOCK) on (a2.Document_Id = d.Id and a2.Def_Id = '6f1d627c-1367-4506-824e-641be98e8db2' and a2.Expired = '99991231')
			LEFT OUTER JOIN Boolean_Attributes a3 WITH(NOLOCK) on (a3.Document_Id = d.Id and a3.Def_Id = '65d097b3-dc93-4100-ad2b-bea99dbb564f' and a3.Expired = '99991231')
			LEFT OUTER JOIN Currency_Attributes a4 WITH(NOLOCK) on (a4.Document_Id = d.Id and a4.Def_Id = '2457d787-ede7-4100-8cb4-086517782c1c' and a4.Expired = '99991231')
		 WHERE
			d.Def_Id = '5d599ce4-76c5-4894-91cc-4eb3560196ce'
		) as [Assignment] on [Assignment].Id = [SocialApplication].[Assignments]
WHERE
	[SocialApplication].[AssignFrom] is null and
	 (([SocialApplication].[AssignTo] is null and [Assignment].[ExpiryDate] is not null) or
	  [SocialApplication].[PaymentSum] is null)";
    }
}
