using System;
using System.IO;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cissa.Tests.ReportTest
{
    [TestClass]
    public class ExcelTemp
    {
        public class StringParams : IStringParams
        {
            public readonly Dictionary<string, string> Params = new Dictionary<string, string>();

            public string Get(string name)
            {
                string value;
                if (Params.TryGetValue(name, out value))
                    return value;
                return String.Empty;
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create())
            {
                var prms = new StringParams();

                prms.Params.Add("OrganizationMSEC", "МСЭК-1");

                var temp = new ExcelTemplateRepository(provider, Guid.Empty);

                using (var stream = temp.Generate("F:\\Temp\\StatTalonGrown.xls", prms))
                {
                    stream.Position = 0;
                    using (var file = new FileStream("f:\\temp\\outputGrowth.xls", FileMode.CreateNew))
                    {
                        stream.CopyTo(file);
                    }
                }
            }
        }
    }
}
