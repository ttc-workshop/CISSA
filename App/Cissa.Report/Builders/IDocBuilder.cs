using System;
using System.Drawing;
using Intersoft.Cissa.Report.Styles;

namespace Intersoft.Cissa.Report.Builders
{
    public class Alignment
    {
        public HAlignment Horizontal { get; set; }
        public VAlignment Vertical { get; set; }
    }

    public interface IStyleBuilder
    {
        IStyleBuilder Bold(bool enable = true);
        IStyleBuilder Underline(bool enable = true);
        IStyleBuilder Italic(bool enable = true);
        IStyleBuilder HAlign(HAlignment alignment);
        IStyleBuilder VAlign(VAlignment alignment);
        IStyleBuilder FontSizeDelta(int delta);
        IStyleBuilder FontSize(int size);
        IStyleBuilder FontColor(Color color);
        IStyleBuilder BackgroundColor(Color color);
    }

    public interface ITableStyleBuilder : IStyleBuilder
    {
        ITableStyleBuilder Borders();
        ITableStyleBuilder BorderColor(Color color);
    }

    public interface IContentBuilder
    {
        void BeginStyle(Action<IStyleBuilder> styleAction);
        void EndStyle();
    }

    public interface ITextBuilder : IContentBuilder
    {
        void Write(string text);
        void WriteLine(string text = null);
    }

    public interface IImageBuilder : IContentBuilder
    {
        void AddImage(Image image);
    }

    public class TableOptions
    {
        bool AutoWidth { get; set; }
    }
    public interface ITableBuilder : IContentBuilder
    {
        void AddRow(Action<IRangeBuilder> rowBuilder);
        void AddColumn(Action<IRangeBuilder> builderAction);
    }

    public interface ICellBuilder
    {
        void AddText(params string[] texts);
        void AddText(string format, params object[] args);
        void AddInt(params int[] values);
        void AddFloat(params float[] values);
        void AddDateTime(params DateTime[] values);
        void AddCurrency(params decimal[] values);
    }
    public interface IRangeBuilder
    {
        void AddCell(Action<ICellBuilder> builderAction);
        void AddTable(Action<ITableBuilder> builderAction);
        void AddRow(Action<IRangeBuilder> builderAction);
        void AddColumn(Action<IRangeBuilder> builderAction);
    }

    public interface IDocumentBuilder : IContentBuilder
    {
        void AddText(Action<ITextBuilder> builderAction);
        void AddImage(Action<IImageBuilder> builderAction);
        void AddTable(Action<ITableBuilder> builderAction);
    }



}
