using ExcelToJson;
using System.Collections.Generic;

namespace ProjectOC.Dialog
{
    [System.Serializable]
    public struct OptionTableData : IGenData
    {
        public string ID;
        public List<OnePieceOption> Options;
        public struct OnePieceOption
        {
            public ML.Engine.TextContent.TextContent OptionText;
            public string NextID;
        }

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            this.ID = Program.ParseString(row[0]);
            this.Options = new List<OnePieceOption>();
            for (int i = 0; i <= 2; i++)
            {
                Options.Add(new OnePieceOption() { OptionText = Program.ParseTextContent(row[2*i+1]), NextID = Program.ParseString(row[2*i+2]) });
            }
            return true;
        }
    }
}
