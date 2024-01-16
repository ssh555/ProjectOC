using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataToBinary
{
    [System.Serializable]
    public struct ResonanceWheelPanel
    {
        public TextContent toptitle;
        public TextTip[] itemtype;
        public KeyTip lastterm;
        public KeyTip nextterm;
        public KeyTip nextgrid;
    }
}
