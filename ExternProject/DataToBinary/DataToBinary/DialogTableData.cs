using ExcelToJson;

namespace ProjectOC.Dialog
{
    [System.Serializable]
    public struct DialogTableData : IGenData
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
        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            this.ID = Program.ParseString(row[0]);
            this.Content = Program.ParseTextContent(row[1]);
            this.CharacterID = Program.ParseString(row[2]);
            this.Name = Program.ParseTextContent(row[3]);
            this.Audio = Program.ParseString(row[4]);
            this.MoodID = Program.ParseString(row[5]);
            this.ActionID = Program.ParseString(row[6]);
            this.NextID = Program.ParseString(row[7]); ;
            this.OptionID = Program.ParseString(row[8]); ;
            this.BHasOption = Program.ParseString(row[9]);
            return true;
        }
    }
}
