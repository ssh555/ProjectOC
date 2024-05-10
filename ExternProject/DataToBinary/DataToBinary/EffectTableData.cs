using ExcelToJson;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct EffectTableData : IGenData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;
        public EffectType Type;
        public string Param1;
        public int Param2;
        public float Param3;
        public bool Param4;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0])) { return false; }
            ID = Program.ParseString(row[0]);
            Name = Program.ParseTextContent(row[1]);
            Type = Program.ParseEnum<EffectType>(row[2]);
            Param1 = Program.ParseString(row[3]);
            Param2 = Program.ParseInt(row[4]);
            Param3 = Program.ParseFloat(row[5]);
            Param4 = Program.ParseBool(row[6]);
            return true;
        }
    }
    [System.Serializable]
    public enum EffectType
    {
        None,
        AlterWorkerVariable,
        AlterProNodeVariable,
        AlterEchoVariable
    }
}
