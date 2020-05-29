using System.Collections.Generic;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;

namespace Intersoft.CISSA.DataAccessLayer.Report
{
    public class ExcelStyles
    {
        public static Dictionary<TextStyle, CellStyle> CreateDefaultStyels(HSSFWorkbook hssfworkbook)
        {
            var styles = new Dictionary<TextStyle, CellStyle>();

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 16;
                font.Boldweight = 3000;
                font.FontName = "Arial";
                style.SetFont(font);
                
                //style.FillForegroundColor = HSSFColor.GREY_25_PERCENT.index;
                //style.FillPattern = FillPatternType.SOLID_FOREGROUND;

                styles.Add(TextStyle.Header1, style);
            }


            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 18;
                font.Boldweight = 2500;
                font.FontName = "Arial";
                style.SetFont(font);

                style.Alignment = HorizontalAlignment.CENTER;

                styles.Add(TextStyle.BigHeader, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 14;
                font.Boldweight = 2000;
                font.FontName = "Arial";
                style.SetFont(font);

                //style.FillForegroundColor = HSSFColor.GREY_25_PERCENT.index;
                //style.FillPattern = FillPatternType.SOLID_FOREGROUND;

                styles.Add(TextStyle.Header2, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 14;
                font.Boldweight = 2000;
                font.FontName = "Arial";
                style.SetFont(font);
                style.Alignment = HorizontalAlignment.CENTER;

                styles.Add(TextStyle.Header2Simple, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 12;
                font.Boldweight = 1000;
                font.FontName = "Arial";
                style.SetFont(font);

                //style.FillForegroundColor = HSSFColor.WHITE.index;
                //style.FillPattern =  FillPatternType.FINE_DOTS;
                //style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;

                styles.Add(TextStyle.Header3, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 12;
                font.Boldweight = 1000;
                font.FontName = "Arial";
                style.SetFont(font);
                style.Alignment = HorizontalAlignment.CENTER;

                styles.Add(TextStyle.Header3Simple, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 12;
                font.Boldweight = 1000;
                font.FontName = "Arial";
                style.SetFont(font);

                style.FillForegroundColor = HSSFColor.WHITE.index;
                style.FillPattern = FillPatternType.FINE_DOTS;
                style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;

                style.Alignment = HorizontalAlignment.CENTER;

                styles.Add(TextStyle.Header3Center, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 12;
                font.Boldweight = 1000;
                font.FontName = "Arial";
                style.SetFont(font);

                //style.FillForegroundColor = HSSFColor.WHITE.index;
                //style.FillPattern = HSSFCellStyle.LESS_DOTS;
                //style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;

                style.WrapText = true;

                styles.Add(TextStyle.Header3Wrap, style);
            }


            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);
                style.Alignment = HorizontalAlignment.LEFT;

                styles.Add(TextStyle.NormalText, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                font.IsItalic = true;

                style.SetFont(font);
                style.Alignment = HorizontalAlignment.LEFT;

                styles.Add(TextStyle.NormalTextItalic, style);
            }


            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);

                style.FillForegroundColor = HSSFColor.WHITE.index;
                style.FillPattern = FillPatternType.FINE_DOTS;
                style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;
                
                style.BorderTop = CellBorderType.THIN;
                style.BorderBottom = CellBorderType.THIN;
                style.BorderLeft = CellBorderType.THIN;
                style.BorderRight = CellBorderType.THIN;
                
                style.VerticalAlignment = VerticalAlignment.CENTER;
                
                style.BottomBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.TopBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.LeftBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.RightBorderColor = HSSFColor.GREY_50_PERCENT.index;

                style.WrapText = true;

                styles.Add(TextStyle.TableRowDark, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);

                style.FillForegroundColor = HSSFColor.WHITE.index;
                style.FillPattern = FillPatternType.FINE_DOTS;
                style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;

                style.Alignment = HorizontalAlignment.CENTER;
                style.VerticalAlignment = VerticalAlignment.TOP;

                styles.Add(TextStyle.TableRowDarkCenter, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);

                style.FillForegroundColor = HSSFColor.WHITE.index;
                style.FillPattern = FillPatternType.FINE_DOTS;
                style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;

                style.Alignment = HorizontalAlignment.RIGHT;
                style.VerticalAlignment = VerticalAlignment.TOP;

                DataFormat format = hssfworkbook.CreateDataFormat();
                style.DataFormat = format.GetFormat("# ### ### ##0.00");

                styles.Add(TextStyle.TableRowDarkMoney, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);

                style.FillForegroundColor = HSSFColor.WHITE.index;
                style.FillPattern = FillPatternType.FINE_DOTS;
                style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;

                style.Alignment = HorizontalAlignment.LEFT;
                style.VerticalAlignment = VerticalAlignment.TOP;
                style.WrapText = true;

                styles.Add(TextStyle.NormalDarkLeftTopWrap, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);

                style.FillForegroundColor = HSSFColor.WHITE.index;
                style.FillPattern = FillPatternType.SOLID_FOREGROUND;

                style.VerticalAlignment = VerticalAlignment.CENTER;
                
                style.BorderTop = CellBorderType.THIN;
                style.BorderBottom = CellBorderType.THIN;
                style.BorderLeft = CellBorderType.THIN;
                style.BorderRight = CellBorderType.THIN;

                style.BottomBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.TopBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.LeftBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.RightBorderColor = HSSFColor.GREY_50_PERCENT.index;

                style.WrapText = true;

                styles.Add(TextStyle.TableRowNormal, style);
            }


            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);

                style.WrapText = true;

                styles.Add(TextStyle.WrapText, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.Boldweight = 3000;
                font.FontName = "Arial";
                style.SetFont(font);

                styles.Add(TextStyle.BoldText, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.Boldweight = 3000;
                font.FontName = "Arial";
                style.Alignment = HorizontalAlignment.CENTER;
                style.SetFont(font);

                styles.Add(TextStyle.BoldTextAlignCenter, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.Alignment = HorizontalAlignment.CENTER;
                style.VerticalAlignment = VerticalAlignment.TOP;
                style.SetFont(font);

                styles.Add(TextStyle.NormalAlignCenter, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.Alignment = HorizontalAlignment.LEFT;
                style.VerticalAlignment = VerticalAlignment.TOP;
                style.SetFont(font);

                styles.Add(TextStyle.NormalLeftTop, style);
            }


            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.Alignment = HorizontalAlignment.LEFT;
                style.VerticalAlignment = VerticalAlignment.TOP;
                style.SetFont(font);

                style.WrapText = true;

                styles.Add(TextStyle.NormalLeftTopWrap, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.Alignment = HorizontalAlignment.RIGHT;
                style.VerticalAlignment = VerticalAlignment.TOP;
                style.SetFont(font);

                styles.Add(TextStyle.NormalRightTop, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                font.Boldweight = 3000;

                style.Alignment = HorizontalAlignment.RIGHT;
                style.VerticalAlignment = VerticalAlignment.TOP;
                style.SetFont(font);

                styles.Add(TextStyle.BoldRightTop, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                //font.Underline
                font.Color = HSSFColor.BLUE.index;

                style.SetFont(font);

                styles.Add(TextStyle.Hyperlink, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.Boldweight = 3000;
                font.FontName = "Arial";
                style.Alignment = HorizontalAlignment.CENTER;
                style.SetFont(font);

                styles.Add(TextStyle.TableHeader, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);
                style.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy");
                styles.Add(TextStyle.SysDateTime, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);
                style.DataFormat = HSSFDataFormat.GetBuiltinFormat("0");
                styles.Add(TextStyle.SysNumber, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);
                style.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");

                styles.Add(TextStyle.NormalPercent, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                style.SetFont(font);
                DataFormat format = hssfworkbook.CreateDataFormat();
                style.DataFormat = format.GetFormat("# ### ### ##0.00");
                styles.Add(TextStyle.SysMoney, style);
            }


            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                font.Boldweight = 3000;

                style.Alignment = HorizontalAlignment.RIGHT;
                style.VerticalAlignment = VerticalAlignment.TOP;
                style.SetFont(font);

                style.BorderTop = CellBorderType.THIN;
                style.BorderBottom = CellBorderType.THIN;
                style.BorderLeft = CellBorderType.THIN;
                style.BorderRight = CellBorderType.THIN;

                style.BottomBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.TopBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.LeftBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.RightBorderColor = HSSFColor.GREY_50_PERCENT.index;

                styles.Add(TextStyle.TableTotal, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.FontName = "Arial";
                font.Boldweight = 3000;

                style.Alignment = HorizontalAlignment.RIGHT;
                style.VerticalAlignment = VerticalAlignment.TOP;
                style.SetFont(font);
                
                style.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");

                style.BorderTop = CellBorderType.THIN;
                style.BorderBottom = CellBorderType.THIN;
                style.BorderLeft = CellBorderType.THIN;
                style.BorderRight = CellBorderType.THIN;

                style.BottomBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.TopBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.LeftBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.RightBorderColor = HSSFColor.GREY_50_PERCENT.index;

                styles.Add(TextStyle.TableTotalPercent, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 10;
                font.Boldweight = 3000;
                font.FontName = "Arial";

                style.Alignment = HorizontalAlignment.CENTER;
                style.VerticalAlignment = VerticalAlignment.CENTER;

                style.BorderTop = CellBorderType.THIN;
                style.BorderBottom = CellBorderType.THIN;
                style.BorderLeft = CellBorderType.THIN;
                style.BorderRight = CellBorderType.THIN;

                style.BottomBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.TopBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.LeftBorderColor = HSSFColor.GREY_50_PERCENT.index;
                style.RightBorderColor = HSSFColor.GREY_50_PERCENT.index;

                style.FillForegroundColor = HSSFColor.WHITE.index;
                style.FillPattern = FillPatternType.FINE_DOTS;
                style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;

                style.WrapText = true;

                style.SetFont(font);

                styles.Add(TextStyle.TableHeaderGreyCenterdBorder, style);
            }


            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 8;
                font.FontName = "Arial";
                style.SetFont(font);

                styles.Add(TextStyle.SmallText, style);
            }

            {
                Font font = hssfworkbook.CreateFont();
                CellStyle style = hssfworkbook.CreateCellStyle();

                font.Color = HSSFColor.BLACK.index;
                font.FontHeightInPoints = 8;
                font.FontName = "Arial";
                style.SetFont(font);

                style.Alignment = HorizontalAlignment.LEFT;
                style.VerticalAlignment = VerticalAlignment.TOP;
                style.WrapText = true;

                styles.Add(TextStyle.SmallTextWrap, style);
            }

            return styles;
        }
    }

    public enum TextStyle
    {
        BigHeader,
        Header1,
        Header2,
        Header2Simple,
        Header3,
        Header3Simple,
        Header3Wrap,
        Header3Center,
        NormalText,
        NormalTextItalic,
        NormalAlignCenter,
        NormalLeftTop,
        NormalRightTop,
        WrapText,
        BoldText,
        BoldTextAlignCenter,
        Hyperlink,
        TableRowNormal,
        TableRowDark,
        TableRowDarkCenter,
        TableHeader,
        SysDateTime,
        SysNumber,
        SysMoney,
        NormalLeftTopWrap,
        TableTotal,
        TableTotalPercent,
        TableHeaderGreyCenterdBorder,
        NormalDarkLeftTopWrap,
        NormalPercent,
        BoldRightTop,
        TableRowDarkMoney,
        SmallText,
        SmallTextWrap
    }
}