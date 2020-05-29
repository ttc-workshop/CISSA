using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Reports
{
    [DataContract]
    public class BizTableReport
    {
        public BizTableReport()
        {
            ReportDetails = new List<BizReportDetail>();
            ReportHeaders = new List<string>();
            ReportFooters = new List<string>();
            PageHeaders = new List<string>();
            PageFooters = new List<string>();
        }

        public Guid Id { get; set; }
        public DocDef DocDef { get; set; }
        public List<Doc> Docs { get; set; }

        public List<BizReportDetail> ReportDetails { get; set; }

        public List<string> ReportHeaders { get; set; }

        public List<string> ReportFooters { get; set; }

        public List<string> PageHeaders { get; set; }

        public List<string> PageFooters { get; set; }
    }
}