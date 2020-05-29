using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Updates
{
    public class CreateDistrictNoForBankPaymentRegistry
    {
        protected static readonly Guid PaymentDefId = new Guid("{68667FBB-C149-4FB3-93AD-1BBCE3936B6E}");
        protected static readonly Guid DistrictPaymentRegistryDefId = new Guid("{ADF1D21A-5FCE-4F42-8889-D0714DDF7967}");
        protected static readonly Guid PaymentRegistryDefId = new Guid("{B3BB3306-C3B4-4F67-98BF-B015DEEDEFFF}");
        protected static readonly Guid DistrictNoPaymentRegistryDefId = new Guid("{DB434DEC-259F-4563-9213-301D9E38753D}");
        protected static readonly Guid AssignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");

        public static void CreateDistrictNoBankPaymentRegistries(IAppServiceProvider provider)
        {
            var pqb = new QueryBuilder(PaymentDefId);
            //pqb.Where("Registry_DistrictNo").IsNull().And("Registry").IsNotNull();

            var sqlBuilder = provider.Get<ISqlQueryBuilder>();
            var docRepo = provider.Get<IDocRepository>();
            using (var query = sqlBuilder.Build(pqb.Def))
            {
                var assignmentSrc = query.JoinSource(query.Source, AssignmentDefId, SqlSourceJoinType.Inner, "Assignment");
                var districtRegistrySrc = query.JoinSource(query.Source, DistrictPaymentRegistryDefId,
                    SqlSourceJoinType.Inner, "Registry");

                query.AddAttribute("&Id");
                query.AddAttribute(query.Source, "Registry");
                query.AddAttribute(districtRegistrySrc, "District");
                query.AddAttribute(districtRegistrySrc, "BankPaymentRegistry");
                query.AddAttribute(assignmentSrc, "No");
                query.AddAttribute(assignmentSrc, "Amount");
                query.AddAttribute(query.Source, "Registry_DistrictNo");

                var sqlReaderBuilder = provider.Get<ISqlQueryReaderFactory>();
                var rows = new DataTable();
                using (var reader = sqlReaderBuilder.Create(query))
                {
                    reader.Open();
                    reader.Fill(rows);
                }
                Console.WriteLine(@"Количество выплат: " + rows.Rows.Count);

                var districtNoRegistries = new List<Doc>();
                var districtRegistries = new List<Doc>();
                foreach (DataRow row in rows.Rows)
                {
                    var id = row[0] as Guid? ?? Guid.Empty;
                    var districtRegistryId = row[1] as Guid? ?? Guid.Empty;
                    var districtId = row[2] as Guid? ?? Guid.Empty;
                    var registryId = row[3] as Guid? ?? Guid.Empty;
                    var no = row[4] as int? ?? 0;
                    var amount = row[5] as decimal? ?? 0m;
                    var distNoRegistryId = row[6] as Guid? ?? Guid.Empty;

                    if (districtRegistryId != Guid.Empty && districtId != Guid.Empty && registryId != Guid.Empty &&
                        no != 0)
                    {
                        var dReg = districtRegistries.FirstOrDefault(d => d.Id == districtRegistryId);
                        if (dReg == null && districtRegistryId != Guid.Empty)
                        {
                            dReg = docRepo.LoadById(districtRegistryId);
                            if (dReg.DocDef.Id == DistrictPaymentRegistryDefId)
                                districtRegistries.Add(dReg);
                            else
                                Console.WriteLine(@"Bad:  No: {0}; Amount: {4}; DistRegId: '{1}'; DistId: '{2}'; RegId: '{3}'", no, districtRegistryId, districtId, registryId, amount);
                        }
                        else if (dReg == null)
                            Console.WriteLine(@"BadR: No: {0}; Amount: {4}; DistRegId: '{1}'; DistId: '{2}'; RegId: '{3}'", no, districtRegistryId, districtId, registryId, amount);

                        var dnReg =
                            districtNoRegistries.FirstOrDefault(d => d.Id == distNoRegistryId);

                        if (dnReg == null && distNoRegistryId != Guid.Empty)
                        {
                            dnReg = docRepo.LoadById(distNoRegistryId);
                            districtNoRegistries.Add(dnReg);
                            dnReg["DistrictRegistry"] = districtRegistryId;
                            dnReg["Registry"] = registryId;
                            dnReg["ApplicationCount"] = 0;
                            dnReg["TotalAmount"] = 0m;
                        }

                        if (dnReg == null && distNoRegistryId == Guid.Empty)
                        {
                            dnReg =
                                districtNoRegistries.FirstOrDefault(
                                    d =>
                                        ((Guid?) d["DistrictRegistry"] ?? Guid.Empty) == districtRegistryId &&
                                        ((Guid?) d["Registry"] ?? Guid.Empty) == registryId &&
                                        ((int?) d["No"] ?? 0) == no);

                            if (dnReg != null)
                            {
                                var payment = docRepo.LoadById(id);
                                payment["Registry_DistrictNo"] = dnReg.Id;
                                docRepo.Save(payment);
                                Console.WriteLine(@"New:  No: {0}; Amount: {4}; DistRegId: '{1}'; DistId: '{2}'; RegId: '{3}'", no, districtRegistryId, districtId, registryId, amount);
                            }
                        }

                        if (dnReg == null)
                        {
                            dnReg = docRepo.New(DistrictNoPaymentRegistryDefId);

                            dnReg["DistrictRegistry"] = districtRegistryId;
                            dnReg["Registry"] = registryId;
                            dnReg["No"] = no;
                            dnReg["ApplicationCount"] = 1;
                            dnReg["TotalAmount"] = amount;
                            districtNoRegistries.Add(dnReg);
                            docRepo.Save(dnReg);
                            var payment = docRepo.LoadById(id);
                            payment["Registry_DistrictNo"] = dnReg.Id;
                            docRepo.Save(payment);
                            Console.WriteLine(@"New:  No: {0}; Amount: {4}; DistRegId: '{1}'; DistId: '{2}'; RegId: '{3}'", no, districtRegistryId, districtId, registryId, amount);
                        }
                        else
                        {
                            dnReg["ApplicationCount"] = ((int?) dnReg["ApplicationCount"] ?? 0) + 1;
                            dnReg["TotalAmount"] = ((decimal?) dnReg["TotalAmount"] ?? 0m) + amount;
                        }
                    }
                    else
                    {
                        Console.WriteLine(@"Skip: No: {0}; Amount: {4}; DistRegId: '{1}'; DistId: '{2}'; RegId: '{3}'", no, districtRegistryId, districtId, registryId, amount);
                    }
                }
                Console.WriteLine(@"Количество реестров на выплату: {0}, {1}", districtNoRegistries.Count, districtRegistries.Count);
                districtNoRegistries.ForEach(d => docRepo.Save(d));

                var qb = new QueryBuilder(DistrictNoPaymentRegistryDefId);
                using (var q = sqlBuilder.Build(qb.Def))
                {
                    
                }
            }
        }

        public class Payment
        {
            public Guid Id { get; set; }
            public Guid Registry { get; set; }
            public decimal Amount { get; set; }
            public int No { get; set; }

        }
    }
}
