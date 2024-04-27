using ExcelToJson;
using System.Collections.Generic;

namespace ML.Engine.Event
{
    [System.Serializable]
    public struct ConditionTableData : IGenData
    {
        public string ID;
        public TextContent.TextContent Name;
        public CheckType CheckType;
        public List<string> Param1;
        public List<int> Param2;
        public List<float> Param3;
        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Name
            this.Name = Program.ParseTextContent(row[1]);
            // 2 -> CheckType
            this.CheckType = Program.ParseEnum<CheckType>(row[2]);
            // 3 -> Param1
            this.Param1 = Program.ParseStringList(row[3]);
            // 4 -> Param2
            this.Param2 = Program.ParseIntList(row[4]);
            // 5 -> Param3
            this.Param3 = Program.ParseFloatList(row[5]);
            return true;
        }
    }
}
