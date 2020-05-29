using System;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace ConsoleApplication1.Tests
{
    public class SocialFundUnloader
    {
        // Document Defs Id
        public static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        public static readonly Guid ReportDefId = new Guid("{0E05462C-4A4C-4729-972E-5074DB1DED4E}");
        public static readonly Guid ReportItemDefId = new Guid("{3C1A7B35-8300-4E4D-9BD9-F9AC0D4C81D6}");
        public static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        public static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        public static readonly Guid PersonDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");

        public static readonly Guid FirstMayUsrOrgId = new Guid("{34DDCAF2-EB08-48E7-894A-29C929D62C83}");

        public static void GetSql()
        {
            var qd = new QueryBuilder(AppDefId);
            qd.Where("Applicant").Include("PIN").IsNull();

            using (var query = SqlQueryBuilder.Build(qd))
            {
                query.AddAttribute(query.Source, "&Id");
                query.AddAttributes("LastName", "FirstName", "MiddleName", "BirthDate", "Sex", "PassportNo", "PassportDate", "Education");
                query.AddAttributes("ZipCode", "Town", "Street", "House", "Apartment", "HomePhone", "Category");

                query.AndCondition("&OrgId", ConditionOperation.Equal, FirstMayUsrOrgId);

                using (var reader = new SqlQueryReader(query))
                {
                    var sql = reader.GetSql();

                    Console.Write(sql);
                }
            }
        }
    }
}
