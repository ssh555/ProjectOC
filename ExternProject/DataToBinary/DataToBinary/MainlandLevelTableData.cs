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
        public ML.Engine.TextContent.TextContent[] ConditionTexts;
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
            this.ConditionTexts = Program.ParseTextContentList(row[5]).ToArray();
            this.Events = Program.ParseStringList(row[6]).ToArray();
            this.EventTexts = Program.ParseTextContentList(row[7]).ToArray();
            return true;
        }
    }
}