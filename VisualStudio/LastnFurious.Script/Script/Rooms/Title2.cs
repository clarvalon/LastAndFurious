// Room_Title2 - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using static LastnFurious.Room_Title2;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Room_Title2 // 402
    {
        // Fields

        // Methods
        public override void room_Load()
        {
            StopAllAudio();
            DrawingSurface ds = Room.GetDrawingSurfaceForBackground();
            ds.DrawingColor = 15;
            ds.DrawString(0, system.ViewportHeight - GetFontHeight(eFontNotalot35Regular12), eFontNotalot35Regular12, GAME_VERSION);
            ds.Release();
        }

        public override void room_AfterFadeIn()
        {
            aCar_Theft_101.Play();

            // If a platform service has been set up (i.e. xbox live) then display 'Press A' to initiate login
            if (PlatformService.Configured)
            {
                DisplayGameMenu(eMenuLogin);
            }
            else
            {
                // Otherwise just show start menu
                DisplayGameMenu(eMenuStart);
            }
        }

        

        public override void room_Leave()
        {
            StopAllAudio();
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const string GAME_VERSION = "v1.0.5";


    }

    #endregion

}
