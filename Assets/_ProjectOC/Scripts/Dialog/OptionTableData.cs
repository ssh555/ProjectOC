using System;
using System.Collections.Generic;

namespace ProjectOC.Dialog
{
    [System.Serializable]
    public struct OptionTableData:IComparable<OptionTableData>
    {
        public string ID;
        public List<OnePieceOption> Options;
        public struct OnePieceOption
        {
            public ML.Engine.TextContent.TextContent OptionText;
            public string NextID;

        }
        public int CompareTo(OptionTableData other)
        {
            int xIndex = int.Parse(ID.Split("_")[2]);
            int yIndex = int.Parse(other.ID.Split("_")[2]);
            return xIndex.CompareTo(yIndex);
        }
    }
}
