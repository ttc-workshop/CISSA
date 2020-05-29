using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsTableFormControlAdjustInfo : XlsColumnItemAdjustInfo
    {
        public Guid Id { get; private set; }
        public string Caption { get; private set; }
        public int CaptionSize { get; private set; }
        public int ValueSize { get; private set; }
//        public XlsControlLayoutType Layout { get; private set; }
//        public int ColSpan { get; internal set; }
//        public int CaptionColSpan { get; internal set; }
//        public int DividerColSpan { get; internal set; }
//        public int ColumnNo { get; internal set; }
//        public int CaptionColumnNo { get; internal set; }
//        public int DividerColumnNo { get; internal set; }

        public XlsTableFormControlAdjustInfo(BizControl column) : base(column)
        {
            Control = column;
            Id = column.Id;
            Caption = column.Caption;

            CaptionSize = GetMaxWordLength(Caption);
            ValueSize = GetDataTypeSize(GetDataType(column));
//            if (isTable)
//            {
                Size = Math.Max(ValueSize, CaptionSize);
//            }
//            else
//            {
//                CaptionSize = Math.Max(30, CaptionSize);
//                Size = CaptionSize + ValueSize;
//            }
        }

        internal IEnumerable<XlsFormControlSizeInfo> GetControlGreatThen(int size, int prevSize, int prevNo)
        {
            var result = prevSize;
            var i = prevNo; result += Math.Max(CaptionSize, ValueSize);
            if (result > size)
                yield return new XlsFormControlSizeInfo(i, this, result);

            /*switch (Layout)
            {
                case XlsControlLayoutType.Vertical:
                    result += Math.Max(CaptionSize, ValueSize);
                    if (result > size)
                        yield return new XlsFormControlSizeInfo(i, this, result, XlsColumnPartType.Value);
                    break;
                case XlsControlLayoutType.Horizontal:
                    result += XlsFormColumnAdjuster.PanelDivideSize;
                    if (result > size)
                        yield return new XlsFormControlSizeInfo(i, this, result, XlsColumnPartType.Divider);
                    else
                    {
                        result += CaptionSize;
                        if (result > size)
                            yield return new XlsFormControlSizeInfo(i, this, result, XlsColumnPartType.Caption);
                        else
                        {
                            result += ValueSize;
                            if (result > size)
                                yield return new XlsFormControlSizeInfo(i + 1, this, result, XlsColumnPartType.Value);
                        }
                    }
                    break;
                default:
                    result += ValueSize;
                    if (result > size)
                        yield return new XlsFormControlSizeInfo(i, this, result, XlsColumnPartType.Value);
                    break;
            }*/
        }

        public bool AdjustColumn(int columnNo, int size, int prevSize, int prevNo)
        {
            var total = prevSize;
            total += Math.Max(CaptionSize, ValueSize);
            if (total == size)
            {
                ColumnNo = columnNo;
                ColSpan = columnNo - prevNo;
                return true;
            }
            /*switch (Layout)
            {
                case XlsControlLayoutType.Vertical:
                    total += Math.Max(CaptionSize, ValueSize);
                    if (total == size)
                    {
                        ColumnNo = columnNo;
                        ColSpan = columnNo - prevNo;
                        return true;
                    }
                    break;
                case XlsControlLayoutType.Horizontal:
                    total += XlsFormColumnAdjuster.PanelDivideSize;
                    if (total == size)
                    {
                        DividerColumnNo = columnNo;
                        DividerColSpan = columnNo - prevNo;
                        return true;
                    }
                    else
                    {
                        total += CaptionSize;
                        if (total == size)
                        {
                            CaptionColumnNo = columnNo;
                            CaptionColSpan = columnNo - prevNo - 1;
                            return true;
                        }
                        else
                        {
                            total += ValueSize;
                            if (total == size)
                            {
                                ColumnNo = columnNo;
                                ColSpan = columnNo - prevNo - 2;
                                return true;
                            }
                        }
                    }
                    break;
                default:
                    total += ValueSize;
                    if (total == size)
                    {
                        ColumnNo = columnNo;
                        ColSpan = columnNo - prevNo;
                        return true;
                    }
                    break;
            }*/
            return false;
        }

        public static BaseDataType GetDataType(BizControl control)
        {
            if (control is BizEditInt) return BaseDataType.Int;
            if (control is BizEditText) return BaseDataType.Text;
            if (control is BizEditBool) return BaseDataType.Bool;
            if (control is BizEditCurrency) return BaseDataType.Currency;
            if (control is BizEditDateTime) return BaseDataType.DateTime;
            if (control is BizEditFloat) return BaseDataType.Float;
            if (control is BizComboBox) return BaseDataType.Text;
            return BaseDataType.Text;
        }

        public static int GetDataTypeSize(BaseDataType dataType)
        {
            switch (dataType)
            {
                case BaseDataType.Int:
                    return IntegerColumnWidth;
                case BaseDataType.DateTime:
                    return DateColumnWidth;
                case BaseDataType.Bool:
                    return BooleanColumnWidth;
                case BaseDataType.Currency:
                    return CurrencyColumnWidth;
                case BaseDataType.Float:
                    return FloatColumnWidth;
                default:
                    return TextColumnWidth;
            }
        }
    }
}