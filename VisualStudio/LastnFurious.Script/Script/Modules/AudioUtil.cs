// Module_AudioUtil - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_AudioUtil;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_AudioUtil
    {
        // Fields

        // Methods
        public void StopAllAudio()
        {
            Game.StopAudio(eAudioTypeAmbientSound);
            Game.StopAudio(eAudioTypeMusic);
            Game.StopAudio(eAudioTypeSound);
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AudioUtil methods so they can be used without instance prefix
        public static void StopAllAudio()
        {
            AudioUtil.StopAllAudio();
        }


    }

    #endregion

}
