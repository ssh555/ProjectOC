using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheel_sub1
    {
        [System.Serializable]
        public struct ResonanceWheel_sub1Struct
        {
            //BeastInfo
            public TextContent Stamina;
            public TextContent Speed;

            public TextContent Cook;
            public TextContent HandCraft;
            public TextContent Industry;
            public TextContent Magic;
            public TextContent Transport;
            public TextContent Collect;

            public KeyTip expel;
            public KeyTip receive;

            //BotKeyTips
            public KeyTip back;
        }

    }
}
