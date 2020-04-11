// Module_GUIUtils - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_GUIUtils;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_GUIUtils
    {
        // Fields

        // Methods
        public void AllButtonsState(GUI g, bool visible, bool clickable)
        {
            int i = 0;
            for (i = 0; i < g.ControlCount; i += 1)
            {
                Button btn = g.Controls[i].AsButton;
                if (btn != null)
                {
                    btn.Visible = visible;
                    btn.Visible = clickable;
                }
            }
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose GUIUtils methods so they can be used without instance prefix
        public static void AllButtonsState(GUI g, bool visible, bool clickable)
        {
            GUIUtils.AllButtonsState(g, visible, clickable);
        }


    }

    #endregion

}
