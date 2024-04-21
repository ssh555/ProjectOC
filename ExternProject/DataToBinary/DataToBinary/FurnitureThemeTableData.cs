using ExcelToJson;
using System.Collections.Generic;


namespace ML.Engine.BuildingSystem
{
    [System.Serializable]
    public struct FurnitureThemeTableData : IGenData
    {
        public string ID;
        public int Sort;
        public TextContent.TextContent Name;
        public string Icon;
        public List<string> BuildID;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Sort
            this.Sort = Program.ParseInt(row[1]);
            // 2 -> Name
            this.Name = Program.ParseTextContent(row[2]);
            // 3 -> Icon
            this.Icon = Program.ParseString(row[3]);
            // 4 -> BuildID
            this.BuildID = Program.ParseStringList(row[4]);
            return true;
        }
    }
}
