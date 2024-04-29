using ExcelToJson;

namespace ProjectOC.Dialog
{
    [System.Serializable]
    public struct OptionTableData : IGenData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Optiontext1;
        public string OptionNextID1;
        public ML.Engine.TextContent.TextContent Optiontext2;
        public string OptionNextID2;
        public ML.Engine.TextContent.TextContent Optiontext3;
        public string OptionNextID3;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            this.ID = Program.ParseString(row[0]);
            this.Optiontext1 = Program.ParseTextContent(row[1]);
            this.OptionNextID1 = Program.ParseString(row[2]);
            this.Optiontext2 = Program.ParseTextContent(row[3]);
            this.OptionNextID2 = Program.ParseString(row[4]);
            this.Optiontext3 = Program.ParseTextContent(row[5]);
            this.OptionNextID3 = Program.ParseString(row[6]);
            return true;
        }
    }
}
