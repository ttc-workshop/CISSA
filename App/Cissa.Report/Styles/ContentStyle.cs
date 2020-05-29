using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace Intersoft.Cissa.Report.Styles
{
    [Flags]
    public enum TableCellBorder
    {
        None = 0,
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        All = 15
    }

    public class ContentStyle : IComparable
    {
        public string FontName { get; set; }
        public short FontDSize { get; set; }

        private FontStyle _fontStyle;
        private bool _hasFontStyle;
        public FontStyle FontStyle
        {
            get { return _fontStyle; }
            set 
            { 
                _fontStyle = value;
                _hasFontStyle = true;
            }
        }

        private bool _hasFontColor;
        private short _fontColor;
        public short FontColor
        {
            get { return _fontColor; }
            set
            {
                _fontColor = value;
                _hasFontColor = true;
            }
        }

        private bool _hasBgColor;
        private short _bgColor;
        public short BgColor
        {
            get { return _bgColor; }
            set 
            { 
                _bgColor = value;
                _hasBgColor = true;
            }
        }

        public HAlignment HAlign { get; set; }
        public VAlignment VAlign { get; set; }

        private TableCellBorder _borders;
        public TableCellBorder Borders
        {
            get { return _borders; }
            set 
            { 
                _borders = value;
                HasBorders = true;
            }
        }
        public bool HasBorders { get; private set; }

        private short _borderColor;
        public short BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                HasBorderColor = true;
            }
        }
        public bool HasBorderColor { get; private set; }

        public bool? WrapText { get; set; }
        public bool? AutoWidth { get; set; }
        public bool? AutoHeight { get; set; }

        public bool HasFontName()
        {
            return !String.IsNullOrEmpty(FontName);
        }

        public bool HasFontDSize()
        {
            return FontDSize != 0;
        }

        public bool HasFontStyle()
        {
            return _hasFontStyle;
        }

        public bool HasFontColor()
        {
            return _hasFontColor;
        }

        public bool HasBgColor()
        {
            return _hasBgColor;
        }

        public bool HasValues()
        {
            return HasFontName() || HasFontStyle() || HasFontDSize() || HasFontColor() || HasBgColor() ||
                   Borders != TableCellBorder.None || HAlign != HAlignment.None || VAlign != VAlignment.None;
        }

        public ContentStyle Bold(bool enable = true)
        {
            if (enable)
                FontStyle |= FontStyle.Bold;
            else
                if (FontStyle.HasFlag(FontStyle.Bold))  FontStyle -= FontStyle.Bold;

            return this;
        }
        public ContentStyle Italic(bool enable = true)
        {
            if (enable)
                FontStyle |= FontStyle.Italic;
            else
                if (FontStyle.HasFlag(FontStyle.Italic)) FontStyle -= FontStyle.Italic;
            return this;
        }
        public ContentStyle Underline(bool enable = true)
        {
            if (enable)
                FontStyle |= FontStyle.Underline;
            else
                if (FontStyle.HasFlag(FontStyle.Underline)) FontStyle -= FontStyle.Underline;
            return this;
        }

        public void Assign(ContentStyle style)
        {
            if (style == null) return;

            FontName = style.FontName;
            FontDSize = style.FontDSize;
            _fontStyle = style.FontStyle;
            _hasFontStyle = style._hasFontStyle;
            _fontColor = style.FontColor;
            _hasFontColor = style._hasFontColor;
            _bgColor = style.BgColor;
            _hasBgColor = style._hasBgColor;
            HAlign = style.HAlign;
            VAlign = style.VAlign;
            _borders = style.Borders;
            HasBorders = style.HasBorders;
            _borderColor = style.BorderColor;
            HasBorderColor = style.HasBorderColor;

            WrapText = style.WrapText;
            AutoWidth = style.AutoWidth;
            AutoHeight = style.AutoHeight;
        }

        public void Merge(ContentStyle style)
        {
            if (style == null) return;

            if (style.HasFontName())
                FontName = style.FontName;
            if (style.HasFontDSize())
                FontDSize = style.FontDSize;
            if (style.HasFontStyle())
                FontStyle = style.FontStyle;
            if (style.HasFontColor())
                FontColor = style.FontColor;
            if (style.HasBgColor())
                BgColor = style.BgColor;
            if (style.HAlign != HAlignment.None)
                HAlign = style.HAlign;
            if (style.VAlign != VAlignment.None)
                VAlign = style.VAlign;
            if (style.HasBorders)
                Borders = style.Borders;
            if (style.HasBorderColor)
                BorderColor = style.BorderColor;
            if (style.WrapText != null)
                WrapText = style.WrapText;
            if (style.AutoWidth != null)
                AutoWidth = style.AutoWidth;
            if (style.AutoHeight != null)
                AutoHeight = style.AutoHeight;
        }

        public void MulMerge(ContentStyle style)
        {
            if (style == null) return;

            if (style.HasFontName())
                FontName = style.FontName;
            if (style.HasFontDSize())
            {
                if (FontDSize == 0 || (Math.Abs(FontDSize) < Math.Abs(style.FontDSize)))
                    FontDSize = style.FontDSize;
            }
            if (style.HasFontStyle())
                FontStyle |= style.FontStyle;
            if (style.HasFontColor())
                FontColor = style.FontColor;
            if (style.HasBgColor())
                BgColor = style.BgColor;
            if (style.HAlign != HAlignment.None)
                HAlign = style.HAlign;
            if (style.VAlign != VAlignment.None)
                VAlign = style.VAlign;
            if (style.HasBorders)
                Borders = style.Borders;
            if (style.HasBorderColor)
                BorderColor = style.BorderColor;
            if (style.WrapText != null)
                WrapText = style.WrapText;
            if (style.AutoWidth != null)
                AutoWidth = style.AutoWidth;
            if (style.AutoHeight != null)
                AutoHeight = style.AutoHeight;
        }

        public ContentStyle() { }
        public ContentStyle(ContentStyle style)
        {
            if (style != null) Assign(style);
        }

        public static ContentStyle MergeStyles(ContentStyle style1, ContentStyle style2)
        {
            var mergedStyle = new ContentStyle(style1);
            mergedStyle.MulMerge(style2);
            return mergedStyle;
        }

        public int CompareTo(object obj)
        {
            var s = obj as ContentStyle;
            if (s == null) return -1;

            if (s.HasValues() != HasValues()) return -1;

            if (s.HasFontName() != HasFontName()) return -1;
            if (s.HasFontName() && String.Compare(s.FontName, FontName, true, CultureInfo.InvariantCulture) != 0)
                return -1;
            if (s.HasBgColor() != HasBgColor()) return -1;
            if (s.HasBgColor() && (s.BgColor != BgColor)) return -1;
            if (s.HasFontColor() != HasFontColor()) return -1;
            if (s.HasFontColor() && (s.FontColor != FontColor)) return -1;
            if (s.HasFontDSize() != HasFontDSize()) return -1;
            if (s.HasFontDSize() && (s.FontDSize != FontDSize)) return -1;
            if (s.HasFontStyle() != HasFontStyle()) return -1;
            if (s.HasFontStyle() && (s.FontStyle != FontStyle)) return -1;

            if (s.HasBorders != HasBorders) return -1;
            if (s.Borders != Borders) return -1;
            if (s.HasBorderColor && (s.BorderColor != BorderColor)) return -1;

            if (s.AutoHeight != AutoHeight || s.AutoWidth != AutoWidth) return -1;
            if (s.HAlign != HAlign || s.VAlign != VAlign || s.WrapText != WrapText) return -1;

            return 0;
        }
    }

    public class ContentStyleEqualityComparer : EqualityComparer<ContentStyle>
    {
        public override bool Equals(ContentStyle x, ContentStyle y)
        {
            if (x == null || y == null) return false;
            return x.CompareTo(y) == 0;
        }

        public override int GetHashCode(ContentStyle obj)
        {
            return 1;
        }
    }
}
 