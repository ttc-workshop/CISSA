using System;
using System.Collections.Generic;
using System.Drawing;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsGridDefBuilder
    {
        private IAppServiceProvider Provider { get; set; }
        public DocFormDataSet DataSet { get; private set; }
        public BizForm Form { get; private set; }
        public Guid UserId { get; private set; }

        public SqlQueryDataSet SqlDataSet { get; set; }

        /*public XlsGridDefBuilder(BizForm form, IEnumerable<Guid> docs, Guid userId)
        {
            Form = form;
            UserId = userId;
            DataSet = new DocFormDataSet(docs, form, userId);
        }*/

        public XlsGridDefBuilder(IDataContext dataContext, BizForm form, IEnumerable<Guid> docs, Guid userId)
        {
            Form = form;
            UserId = userId;
            DataSet = new DocFormDataSet(dataContext, docs, form, userId);
        }

        public XlsGridDefBuilder(IAppServiceProvider provider, BizForm form, SqlQueryReader reader)
        {
            Provider = provider;
            Form = form;
            UserId = provider.GetCurrentUserId();
            SqlDataSet = new SqlQueryDataSet(Provider, reader);
        }

        public XlsDef BuildFromBizForm()
        {
            return Build();
        }

        public XlsDef Build()
        {
            var def = new XlsDef();
            try
            {
                def.Style.FontName = "Arial Narrow";
                var title = def.AddArea().AddRow().AddText(Form.Caption);
                title.Style.FontDSize = 2;
                title.Style.FontStyle = FontStyle.Bold;
                title.Style.FontColor = IndexedColors.DARK_BLUE.Index; // 18;
                title.Style.HAlign = HAlignment.Center;
                title.Style.AutoHeight = true;

                def.AddArea().AddEmptyRow();

                var hRow = def.AddArea().AddRow();
                hRow.ShowAllBorders(true);
                hRow.Style.FontStyle = FontStyle.Bold;
                hRow.Style.HAlign = HAlignment.Center;
                hRow.Style.BgColor = IndexedColors.BLUE_GREY.Index; //48;
                hRow.Style.FontColor = IndexedColors.WHITE.Index;
                hRow.Style.WrapText = true;

                var ds = ((DataSet) DataSet ?? SqlDataSet);
                var dRow = def.AddGrid(ds).AddRow();
                dRow.ShowAllBorders(true);
                dRow.Style.AutoWidth = true;

                if (Form.Children != null)
                    foreach (var control in Form.Children)
                    {
                        AddControlBand(hRow, dRow, control);
                    }
                title.ColSpan = dRow.GetCols();

                return def;
            }
            catch
            {
                def.Dispose();
                throw;
            }
        }

        protected void AddControlBand(XlsGroup band, XlsRow gridRow, BizControl control)
        {
            if (control.Invisible) return;

            if (control is BizPanel
                /*control is BizTableColumn ||
                control is BizDocumentControl || control is BizDocumentListForm*/)
            {
                var node = new XlsTextNode(control.Caption);
                band.AddGroup(node);

                foreach (var child in control.Children)
                {
                    AddControlBand(node, gridRow, child);
                }
            }
            else if (control is BizTableColumn ||
                     control is BizDocumentControl || control is BizDocumentListForm)
            {
                if (control.Children != null)
                {
                    foreach (var child in control.Children)
                    {
                        AddControlBand(band, gridRow, child);
                    }
                }
            }
            else if (control is BizDataControl)
                AddControlColumn(band, gridRow, control);
            else
                if (control.Children != null)
                {
                    foreach (var child in control.Children)
                    {
                        AddControlBand(band.AddGroup(new XlsTextNode(child.Caption)), gridRow, child);
                    }
                }
        }

        protected void AddControlColumn(XlsGroup band, XlsRow gridRow, BizControl control)
        {
            if (control.Invisible) return;

            band.AddGroup(new XlsTextNode(control.Caption));
            if (DataSet != null)
                gridRow.AddDataField(new DocFormDataSetField(DataSet, control));
            else if (SqlDataSet != null)
            {
                if (!(control is BizDataControl)) return;

                var attr = SqlDataSet.Reader.Query.FindAttribute(((BizDataControl) control).AttributeDefId ?? Guid.Empty) ??
                           SqlDataSet.Reader.Query.FindAttribute(((BizDataControl)control).AttributeName);
                if (attr != null)
                    gridRow.AddDataField(new SqlQueryDataSetField(SqlDataSet, attr, control));
            }
        }
    }
}
