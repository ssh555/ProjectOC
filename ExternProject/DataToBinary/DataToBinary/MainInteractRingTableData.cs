using ExcelToJson;

namespace ProjectOC.MainInteract
{
    [System.Serializable]
    public struct MainInteractRingTableData : IGenData
    {
        public string ID;
        public int Sort;
        public string Name;
        public string Icon;
        public FunctionEnum Function;
        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0])) { return false; }
            ID = Program.ParseString(row[0]);
            Sort = Program.ParseInt(row[1]);
            Name = Program.ParseString(row[2]);
            Icon = Program.ParseString(row[3]);
            Function = Program.ParseEnum<FunctionEnum>(row[4]);
            return true;
        }
    }
    [System.Serializable]
    public enum FunctionEnum
    {
        Bag,
        Build,
        TechTree,
        ProManage,
        Order,
        MyClan,
        Friend,
        Option,
    }
}
