using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheelUI
    {
        [System.Serializable]
        public struct ResonanceWheelPanel
        {
            //topPart
            public TextContent toptitle;
            public KeyTip lastterm;
            public KeyTip nextterm;
            public TextContent HiddenBeastResonanceText;
            public TextContent SongofSeaBeastsText;

            //ring
            public KeyTip nextgrid;
            public TextContent Grid1Text, Grid2Text, Grid3Text, Grid4Text, Grid5Text;

            //ResonanceTarget
            public TextContent ResonanceTargetTitle;
            public TextContent RandomText;
            public TextContent SwitchTargetText;

            //ResonanceConsumpion
            public MultiTextContent ResonanceConsumpionTitle;
            public TextContent StartResonanceText;
            public TextContent StopResonanceText;

            //BotKeyTips
            public KeyTip back;
        }

    }
}
