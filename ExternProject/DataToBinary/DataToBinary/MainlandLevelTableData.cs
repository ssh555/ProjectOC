using ExcelToJson;

namespace ProjectOC.LandMassExpand
{
    [System.Serializable]
    public struct MainlandLevelTableData : IGenData
    {
        public string ID;
        public int Level;
        public ML.Engine.TextContent.TextContent LevelText;
        public bool IsMax;
        public string[] Conditions;
        public string[] Events;
        public ML.Engine.TextContent.TextContent[] EventTexts;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            this.ID = Program.ParseString(row[0]);
            this.Level = Program.ParseInt(row[1]);
            this.LevelText = Program.ParseTextContent(row[2]);
            this.IsMax = Program.ParseBool(row[3]);
            this.Conditions = Program.ParseStringList(row[4]).ToArray();
            this.Events = Program.ParseStringList(row[5]).ToArray();
            this.EventTexts = Program.ParseTextContentList(row[6]).ToArray();
            return true;
        }
    }
}