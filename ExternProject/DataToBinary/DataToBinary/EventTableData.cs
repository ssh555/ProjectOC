using ExcelToJson;

namespace ML.Engine.Event
{
    [System.Serializable]
    public struct EventTableData : IGenData
    {
        public string ID;
        public string Parameter;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Parameter
            this.Parameter = Program.ParseString(row[1]);
            return true;
        }
    }


    public enum CheckType
    {
        CheckBagItem = 0,
        CheckBuild,
        CheckWorkerEMCurrent
    }
}
