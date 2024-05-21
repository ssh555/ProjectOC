using System;

namespace ProjectOC.Dialog
{
    [System.Serializable]
    public struct DialogTableData:IComparable<DialogTableData>
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Content;
        public string CharacterID;
        public ML.Engine.TextContent.TextContent Name;
        public string Audio;
        public string MoodID;
        public string ActionID;
        public string NextID;
        public string OptionID;
        public string BHasOption;

        public int CompareTo(DialogTableData other)
        {
            int xIndex = int.Parse(ID.Split("_")[2]);
            int yIndex = int.Parse(other.ID.Split("_")[2]);
            return xIndex.CompareTo(yIndex);
        }
    }
}