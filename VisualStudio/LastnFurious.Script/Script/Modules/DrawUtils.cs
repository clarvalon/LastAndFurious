// Module_DrawUtils - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_DrawUtils;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_DrawUtils
    {
        // Fields

        // Methods
        public void ExtensionMethod_DrawFrame(DrawingSurface thisItem, int x1, int y1, int x2, int y2)
        {
            thisItem.DrawLine(x1, y1, x2, y1);
            thisItem.DrawLine(x2, y1, x2, y2);
            thisItem.DrawLine(x2, y2, x1, y2);
            thisItem.DrawLine(x1, y2, x1, y1);
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {

    }

    #endregion

    #region Extension Methods Wrapper (AGS workaround)

    public static partial class ExtensionMethods
    {
        public static void DrawFrame(this DrawingSurface thisItem, int x1, int y1, int x2, int y2)
        {
            GlobalBase.DrawUtils.ExtensionMethod_DrawFrame(thisItem, x1, y1, x2, y2);
        }

    }

    #endregion

}
