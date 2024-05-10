using ExcelToJson;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct FeatureTableData : IGenData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;
        public string UpgradeID;
        public string ReduceID;
        public string ReverseID;
        public int Sort;
        public string Icon;
        public FeatureType Type;
        public string Condition;
        public List<string> TrueEffect;
        public List<string> FalseEffect;
        public string Event;
        public ML.Engine.TextContent.TextContent ItemDescription;
        public ML.Engine.TextContent.TextContent EffectsDescription;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0])) {  return false; }
            ID = Program.ParseString(row[0]);
            Name = Program.ParseTextContent(row[1]);
            UpgradeID = Program.ParseString(row[2]);
            ReduceID = Program.ParseString(row[3]);
            ReverseID = Program.ParseString(row[4]);
            Sort = Program.ParseInt(row[5]);
            Icon = Program.ParseString(row[6]);
            Type = Program.ParseEnum<FeatureType>(row[7]);
            Condition = Program.ParseString(row[8]);
            TrueEffect = Program.ParseStringList(row[9]);
            FalseEffect = Program.ParseStringList(row[10]);
            Event = Program.ParseString(row[11]);
            ItemDescription = Program.ParseTextContent(row[12]);
            EffectsDescription = Program.ParseTextContent(row[13]);
            return true;
        }
    }
    [System.Serializable]
    public enum FeatureType
    {
        None,
        Reverse,
        DeBuff,
        Buff,
        Race,
    }
}
