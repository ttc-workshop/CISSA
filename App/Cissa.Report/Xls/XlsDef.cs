using System;
using System.Collections.Generic;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;
using Intersoft.CISSA.DataAccessLayer.Model.Context;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsDef: IDisposable
    {
        private readonly List<XlsArea> _areas = new List<XlsArea>();
        public List<XlsArea> Areas { get { return _areas; } }
        
        private readonly ContentStyle _style = new ContentStyle();
        public ContentStyle Style
        {
            get { return _style; }
            set { _style.Assign(value); }
        }

        public float DefaultRowHeight = 0;
        public IDictionary<int, int> ColumnWidths { get; set; }

        public XlsArea AddArea()
        {
            var area = new XlsArea();
            Areas.Add(area);
            return area;
        }

        public XlsGrid AddGrid(DataSet dataSet)
        {
            var grid = new XlsGrid(dataSet);
            Areas.Add(grid);
            grid.ShowAllBorders(true);
            return grid;
        }

        public void WriteTo(XlsWriter writer)
        {
            var oldStyle = writer.SetStyle(Style);
            try
            {
                if (DefaultRowHeight > 0)
                    writer.DefaultRowHeight = DefaultRowHeight;

                if (ColumnWidths != null)
                    foreach (var columnWidth in ColumnWidths)
                    {
                        writer.SetColumnWidth(columnWidth.Key - 1, columnWidth.Value * 256);
                    }
                foreach (var area in Areas)
                {
                    area.WriteTo(writer);
                }
                writer.ApplyColumnAutoSize();
            }
            catch (Exception e)
            {
                Logger.OutputLog("XlsDef", e, "Excel Report Writing");
                throw;
            }
            finally
            {
                writer.Style = oldStyle;
            }
        }

        public void Dispose()
        {
            foreach (var area in Areas)
            {
                area.Dispose();
            };
        }
    }
}
