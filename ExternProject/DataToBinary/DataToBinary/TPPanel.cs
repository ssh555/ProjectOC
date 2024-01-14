using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectOC.TechTree
{
    public class TechTreeManager
    {
        [System.Serializable]
        public struct TPPanel
        {
            public TextContent toptitle;
            public TextTip[] category;
            public KeyTip categorylast;
            public KeyTip categorynext;
            public TextContent lockedtitletip;
            public TextContent unlockedtitletip;
            public KeyTip inspector;
            public TextContent timecosttip;
            public KeyTip decipher;
            public KeyTip back;
        }
    }

}
