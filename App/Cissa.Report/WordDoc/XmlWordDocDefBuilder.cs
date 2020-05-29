using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class XmlWordDocDefBuilder
    {
        public Stream Stream { get; private set; }
        public XmlWordDocDefBuilder(Stream stream)
        {
            Stream = stream;
        }
        public XmlWordDocDefBuilder(String xml)
        {
            Stream = new MemoryStream(GetBytes(xml));
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private readonly IDictionary<string, DataSet> _dataSets = new Dictionary<string, DataSet>();
        public IDictionary<string, DataSet> DataSets { get { return _dataSets;} }

        public void AddDataSet(string name, DataSet dataSet)
        {
            _dataSets.Add(name, dataSet);
        }

        private readonly IDictionary<string, Func<object, object>> _functions = new Dictionary<string, Func<object, object>>();
        public IDictionary<string, Func<object, object>> Functions { get { return _functions; } }

        public void AddFunction(string name, Func<object, object> func)
        {
            _functions.Add(name, func);
        }

        private readonly ContentStyle _style = new ContentStyle();

        public WordDocDef Build()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(Stream);

            var wordDoc = new WordDocDef();

            var root = xmlDoc.DocumentElement;
            if (root != null && String.Equals(root.Name, "Document", StringComparison.OrdinalIgnoreCase))
            {
                BuildWordDoc(wordDoc, root);
            }
            return wordDoc;
        }

        private void BuildWordDoc(WordDocDef wordDoc, XmlElement root)
        {
            var size = root.Attributes["size"];
            if (size != null)
            {
                if (String.Equals(size.Value, "A5", StringComparison.OrdinalIgnoreCase)) wordDoc.A5();
                if (String.Equals(size.Value, "A4", StringComparison.OrdinalIgnoreCase)) wordDoc.A4();
                if (String.Equals(size.Value, "A3", StringComparison.OrdinalIgnoreCase)) wordDoc.A3();
            }
            var orientation = root.Attributes["orientation"];
            if (orientation != null)
            {
                if (String.Equals(orientation.Value, "portrait", StringComparison.OrdinalIgnoreCase))
                    wordDoc.Portrait();
                if (String.Equals(orientation.Value, "landscape", StringComparison.OrdinalIgnoreCase))
                    wordDoc.Landscape();
            }

            float marginFloat;
            var margin = root.Attributes["left"];
            if (margin != null)
            {
                if (float.TryParse(margin.Value, out marginFloat))
                    wordDoc.MarginLeft = marginFloat;
            }
            margin = root.Attributes["top"];
            if (margin != null)
            {
                if (float.TryParse(margin.Value, out marginFloat))
                    wordDoc.MarginTop = marginFloat;
            }
            margin = root.Attributes["right"];
            if (margin != null)
            {
                if (float.TryParse(margin.Value, out marginFloat))
                    wordDoc.MarginRight = marginFloat;
            }
            margin = root.Attributes["bottom"];
            if (margin != null)
            {
                if (float.TryParse(margin.Value, out marginFloat))
                    wordDoc.MarginBottom = marginFloat;
            }

            SetSectionStyle(root);
            _paragraphCount = 0;
            BuildSections(wordDoc, root);
        }

        private int _paragraphCount;
        private void BuildSections(WordGroupDef wordDoc, XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Text)
                    {
                        if (_paragraphCount == 0) AddParagraph(wordDoc, child);
                        else AddText(wordDoc, child);
                    }
                    else if (child.NodeType == XmlNodeType.Element)
                    {
                        if (String.Equals(child.Name, "p", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_paragraphCount == 0) AddParagraph(wordDoc, child);
                            else AddText(wordDoc, child);
                        }
                        else if (String.Equals(child.Name, "center", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_paragraphCount == 0) AddParagraph(wordDoc, child);
                            else AddText(wordDoc, child);
                        }
                        else if (String.Equals(child.Name, "left", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_paragraphCount == 0) AddParagraph(wordDoc, child);
                            else AddText(wordDoc, child);
                        }
                        else if (String.Equals(child.Name, "right", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_paragraphCount == 0) AddParagraph(wordDoc, child);
                            else AddText(wordDoc, child);
                        }
                        else if (String.Equals(child.Name, "both", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_paragraphCount == 0) AddParagraph(wordDoc, child);
                            else AddText(wordDoc, child);
                        }
                        else if (String.Equals(child.Name, "table", StringComparison.OrdinalIgnoreCase))
                            AddTable(wordDoc, child);
                        else if (String.Equals(child.Name, "repeat", StringComparison.OrdinalIgnoreCase))
                            AddRepeatSectoin(wordDoc, child);
                            // TODO: Дописать для FIELD!!!!
                        else if (String.Equals(child.Name, "field", StringComparison.OrdinalIgnoreCase))
                        {
                            //var pd = wordDoc.AddParagraph(String.Empty, _style);
                            AddTextOrField(wordDoc, child);
                        }
                        else if (String.Equals(child.Name, "pagebreak", StringComparison.OrdinalIgnoreCase))
                        {
                            //wordDoc.AddItem(new PageBreak());
                            AddPageBreak(wordDoc, child);
                        }
                    }
                }
            }
        }

        private static void AddPageBreak(WordGroupDef wordDoc, XmlNode node)
        {
            var step = 0;
            var stepAttr = node.Attributes != null ? node.Attributes["step"] : null;
            if (stepAttr != null)
            {
                int.TryParse(stepAttr.Value, out step);
            }
            wordDoc.AddItem(new PageBreak{Step = step});
        }

        private readonly Stack<DataSet> _dataSetStack = new Stack<DataSet>();

        private DataSet FindDataSetByFieldName(string fieldName)
        {
            return _dataSetStack.FirstOrDefault(dataSet => dataSet.HasField(fieldName));
        }

        private void AddRepeatSectoin(WordGroupDef wordDoc, XmlNode node)
        {
            var dataSetAttr = node.Attributes != null ? node.Attributes["dataset"] : null;
            var dataSet = dataSetAttr != null ? _dataSets[dataSetAttr.Value] : _dataSets.First().Value;
            var reset = node.Attributes != null && node.Attributes["reset"] != null;

            var oldStyle = new ContentStyle(_style);
            try
            {
                SetSectionStyle(node);

                var section = new WordRepeatSectionDef(dataSet) {Style = _style};
                wordDoc.AddItem(section);
                _dataSetStack.Push(dataSet);
                try
                {
                    section.ResetDatas = reset;
                    BuildSections(section, node);
                }
                finally
                {
                    _dataSetStack.Pop();
                }
            }
            finally
            {
                _style.Assign(oldStyle);
            }
        }

        private void AddTable(WordGroupDef wordDoc, XmlNode table)
        {
            var oldStyle = new ContentStyle(_style);
            try
            {
                SetSectionStyle(table);

                var tableDef = new WordTableDef { Style = _style };
                wordDoc.AddItem(tableDef);
                var tableFullWidth = table.Attributes != null ? table.Attributes["fullwidth"] : null;
                tableDef.FitWidth = tableFullWidth != null;
                // var rowNo = 0;
                var repeatSection = true;
                foreach (XmlNode child in table.ChildNodes)
                {
                    if (String.Equals(child.Name, "tr", StringComparison.OrdinalIgnoreCase))
                    {
                        var tableSectionDef = repeatSection
                            ? tableDef.AddSection(new WordTableSectionDef())
                            : tableDef.Sections.LastOrDefault();

                        AddTableRow(tableSectionDef, child /*, ref rowNo*/);
                        repeatSection = false;

                    }
                    else if (String.Equals(child.Name, "repeat", StringComparison.OrdinalIgnoreCase))
                    {
                        AddTableRepeatSection(tableDef, child /*, ref rowNo*/);
                        repeatSection = true;
                    }
                }
            }
            finally
            {
                _style.Assign(oldStyle);
            }
        }

        private void AddTableRepeatSection(WordTableDef tableDef, XmlNode node)
        {
            var dataSetAttr = node.Attributes != null ? node.Attributes["dataset"] : null;
            var dataSet = dataSetAttr != null ? _dataSets[dataSetAttr.Value] : _dataSets.First().Value;
            var reset = node.Attributes != null && node.Attributes["reset"] != null;

            var oldStyle = new ContentStyle(_style);
            try
            {
                SetSectionStyle(node);

                var sectionDef =
                    tableDef.AddSection(new WordTableRepeatSectionDef(dataSet) {ResetDatas = reset, Style = _style});

                _dataSetStack.Push(dataSet);
                try
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (String.Equals(child.Name, "tr", StringComparison.OrdinalIgnoreCase))
                            AddTableRow(sectionDef, child /*, ref rowNo*/);
                    }
                }
                finally
                {
                    _dataSetStack.Pop();
                }
            }
            finally
            {
                _style.Assign(oldStyle);
            }
        }

        private void AddTableRow(WordTableSectionDef tableSectionDef, XmlNode row/*, ref int rowNo*/)
        {
            var oldStyle = new ContentStyle(_style);
            try
            {
                SetSectionStyle(row);
                var rowDef = tableSectionDef.AddRow(/*rowNo*/);
                rowDef.Style = _style;
                var colNo = 0;
                foreach (XmlNode child in row.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element &&
                        String.Equals(child.Name, "td", StringComparison.OrdinalIgnoreCase))
                        AddTableCell(rowDef, child, ref colNo);

                }
                //rowNo++;
            }
            finally
            {
                _style.Assign(oldStyle);
            }
        }

        private void AddTableCell(WordTableRowDef rowDef, XmlNode cell, ref int colNo)
        {
            var oldStyle = new ContentStyle(_style);
            try
            {
                SetSectionStyle(cell);
                var cellDef = rowDef.AddCell(colNo);
                cellDef.Style = _style;
                
                if (!cell.HasChildNodes)
                    cellDef.AddText(cell.InnerText, _style);
                else
                    foreach (XmlNode child in cell.ChildNodes)
                    {
                        AddTextOrField(cellDef, child);
                    }
                colNo++;
            }
            finally
            {
                _style.Assign(oldStyle);
            }
        }

        private WordParagraphDef AddParagraph(WordGroupDef wordDoc, XmlNode node)
        {
            _paragraphCount++;
            var oldStyle = new ContentStyle(_style);
            try
            {
                SetTextStyle(node);
                SetSectionStyle(node);

                var pd = new WordParagraphDef(String.Empty, _style);
                wordDoc.AddItem(pd);
                if (node.NodeType == XmlNodeType.Text)
                {
                    pd.AddItem(new WordParagraphTextDef(node.InnerText));
                }
                else if (node.NodeType == XmlNodeType.Element)
                {
                    if (!node.HasChildNodes)
                        pd.AddItem(new WordParagraphTextDef(node.InnerText) { Style = _style });
                    else
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            if (child.NodeType == XmlNodeType.Element &&
                                String.Equals(child.Name, "repeat", StringComparison.OrdinalIgnoreCase))
                            {
                                AddRepeatSectoin(pd, child);
                            }
                            else
                                AddText(pd, child);
                        }
                }
                return pd;
            }
            finally
            {
                _style.Assign(oldStyle);
                _paragraphCount--;
            }
        }

        private void AddText(WordGroupDef paraDef, XmlNode node)
        {
            var oldStyle = new ContentStyle(_style);
            try
            {
                SetTextStyle(node);
                SetSectionStyle(node);

                if (node.NodeType == XmlNodeType.Text)
                    paraDef.AddItem(new WordParagraphTextDef(node.InnerText) {Style = _style});
                else if (node.NodeType == XmlNodeType.Whitespace)
                    paraDef.AddItem(new WordParagraphTextDef(node.InnerText) { Style = _style });
                else if (node.NodeType == XmlNodeType.Element)
                {
                    if (!node.HasChildNodes)
                        paraDef.AddItem(new WordParagraphTextDef(node.InnerText) { Style = _style });
                    else if (node.NodeType == XmlNodeType.Element &&
                             String.Equals(node.Name, "field", StringComparison.OrdinalIgnoreCase))
                    {
                        var dataSetAttr = node.Attributes != null ? node.Attributes["dataset"] : null;
                        var dataSet =
                            (dataSetAttr != null
                                ? _dataSets[dataSetAttr.Value]
                                : FindDataSetByFieldName(node.InnerText.Trim()) ?? _dataSetStack.Peek()) as
                                SqlQueryDataSet;
                        var format = node.Attributes != null ? node.Attributes["format"] : null;

                        if (dataSet != null)
                            paraDef.AddItem(
                                new WordDataField(dataSet.CreateField(node.InnerText.Trim()),
                                    format != null ? format.Value : null) {Style = _style});
                    }
                    else
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            AddTextOrField(paraDef, child);
                        }
                }
            }
            finally
            {
                _style.Assign(oldStyle);
            }
        }

        private void AddTextOrField(WordGroupDef paraDef, XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Element &&
                String.Equals(node.Name, "field", StringComparison.OrdinalIgnoreCase))
            {
                var dataSetAttr = node.Attributes != null ? node.Attributes["dataset"] : null;
                var dataSet = (dataSetAttr != null ? _dataSets[dataSetAttr.Value] : FindDataSetByFieldName(node.InnerText.Trim()) ?? _dataSetStack.Peek()) as SqlQueryDataSet;
                var format = node.Attributes != null ? node.Attributes["format"] : null;
                var funcName = node.Attributes != null ? node.Attributes["func"] : null;
                Func<object, object> func = null;
                if (funcName != null)
                    Functions.TryGetValue(funcName.Value, out func);

                if (dataSet != null)
                    paraDef.AddItem(
                        new WordDataField(dataSet.CreateField(node.InnerText.Trim()),
                            format != null ? format.Value : null) {Style = _style, Func = func});
            }
            else 
                AddText(paraDef, node);
        }

        private void SetTextStyle(XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                if (String.Equals(node.Name, "b", StringComparison.OrdinalIgnoreCase))
                    _style.Bold();
                else if (String.Equals(node.Name, "i", StringComparison.OrdinalIgnoreCase))
                    _style.Italic();
                else if (String.Equals(node.Name, "u", StringComparison.OrdinalIgnoreCase))
                    _style.Underline();
                else if (String.Equals(node.Name, "center", StringComparison.OrdinalIgnoreCase))
                    _style.HAlign = HAlignment.Center;
                else if (String.Equals(node.Name, "left", StringComparison.OrdinalIgnoreCase))
                    _style.HAlign = HAlignment.Left;
                else if (String.Equals(node.Name, "right", StringComparison.OrdinalIgnoreCase))
                    _style.HAlign = HAlignment.Right;
                else if (String.Equals(node.Name, "both", StringComparison.OrdinalIgnoreCase))
                    _style.HAlign = HAlignment.FullWidth;
            }
        }

        // ReSharper disable once FunctionComplexityOverflow
        private void SetSectionStyle(XmlNode node)
        {
            if (node.Attributes != null)
            {
                if (node.Attributes["border"] != null)
                    _style.Borders = TableCellBorder.All;
                XmlAttribute attr = node.Attributes["fontsize"] ?? node.Attributes["fontdsize"];
                if (attr != null)
                    _style.FontDSize = short.Parse(attr.Value);
                attr = node.Attributes["color"] ?? node.Attributes["fontcolor"] ?? node.Attributes["forecolor"];
                if (attr != null)
                    _style.FontColor = short.Parse(attr.Value);
                attr = node.Attributes["font"] ?? node.Attributes["fontname"] ?? node.Attributes["font-family"] ?? node.Attributes["fontfamily"];
                if (attr != null)
                    _style.FontName = attr.Value;
                attr = node.Attributes["bgcolor"] ?? node.Attributes["backcolor"] ?? node.Attributes["background"] ?? node.Attributes["backgroundcolor"];
                if (attr != null)
                    _style.BgColor = short.Parse(attr.Value);
                attr = node.Attributes["wrap"] ?? node.Attributes["wraptext"];
                if (attr != null)
                    _style.WrapText = true;
                attr = node.Attributes["autoheight"];
                if (attr != null)
                    _style.AutoHeight = true;
                attr = node.Attributes["autowidth"];
                if (attr != null)
                    _style.AutoWidth = true;
                attr = node.Attributes["align"] ?? node.Attributes["alignment"] ?? node.Attributes["halign"];
                if (attr != null)
                {
                    if (String.Equals(attr.Value, "center", StringComparison.OrdinalIgnoreCase))
                        _style.HAlign = HAlignment.Center;
                    else if (String.Equals(attr.Value, "left", StringComparison.OrdinalIgnoreCase))
                        _style.HAlign = HAlignment.Left;
                    else if (String.Equals(attr.Value, "right", StringComparison.OrdinalIgnoreCase))
                        _style.HAlign = HAlignment.Right;
                    else if (String.Equals(attr.Value, "both", StringComparison.OrdinalIgnoreCase))
                        _style.HAlign = HAlignment.FullWidth;                   
                }
            }
        }
    }
}
