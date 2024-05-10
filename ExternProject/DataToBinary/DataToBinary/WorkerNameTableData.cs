using ExcelToJson;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct WorkerNameTableData : IGenData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0])) { return false; }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Name
            this.Name = Program.ParseTextContent(row[1]);
            return true;
        }
    }
}