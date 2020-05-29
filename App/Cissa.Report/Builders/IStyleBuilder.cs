using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intersoft.Cissa.Report.Styles
{
    public enum HAlignment
    {
        None,
        Left,
        Center,
        Right,
        FullWidth
    }

    public enum VAlignment
    {
        None,
        Top,
        Middle,
        Bottom
    }

    [Flags]
    public enum CellBorder
    {
        None = 0,
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        All = 15
    }

    public class StyleBuilder
    {
    }
}
