using System;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace ConsoleApplication1.Reports
{
    public static class AppInputStatReport
    {
        public static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        public static readonly Guid ReportDefId = new Guid("{0E05462C-4A4C-4729-972E-5074DB1DED4E}");
        public static readonly Guid ReportItemDefId = new Guid("{3C1A7B35-8300-4E4D-9BD9-F9AC0D4C81D6}");
        public static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        public static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        public static readonly Guid PersonalFileDefId = new Guid("{B9B0D237-CA2F-41A2-BC26-D1C83CE3907E}");

        public static void Build(IAppServiceProvider provider, IDataContext dataContext)
        {
            var query = new SqlQuery(provider, PersonalFileDefId/*AppDefId*/, Guid.Empty);
            query.AddAttribute("&OrgCode");
            query.AddAttribute("&OrgName");
            // query.AddAttribute("&Created", "cast({0} as date)");
            query.AddAttribute("&Created", SqlQuerySummaryFunction.Max);
            query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
            query.AddGroupAttributes(new[] {"&OrgCode", "&OrgName" /*, "&Created"*/});
            query.AddOrderAttribute("&OrgCode");

            using(var reader = new SqlQueryReader(dataContext, query))
            {
                reader.Open();
                while(reader.Read())
                {
                    var orgCode = !reader.IsDbNull(0) ? reader.GetString(0) : String.Empty;
                    var orgName = !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;
                    var created = !reader.IsDbNull(2) ? reader.GetDateTime(2).ToShortDateString() : String.Empty;
                    var count = !reader.IsDbNull(3) ? reader.GetInt32(3) : 0;

                    Console.WriteLine(@"{0};{1};{2};{3}", orgCode, orgName, created, count);
                }
                reader.Close();
            }
        }
    }
}
