using ExcelToJson;

namespace ML.Engine.InventorySystem
{
    public struct CreatureTableData : IGenData
    {
        public string ID;
        public string ItemID;
        public string ProRecipeID;
        public string BreRecipeID;
        public Formula Discard;
        public int Activity;
        public bool Sextype;
        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0])) { return false; }
            ID = Program.ParseString(row[0]);
            ItemID = Program.ParseString(row[1]);
            ProRecipeID = Program.ParseString(row[2]);
            BreRecipeID = Program.ParseString(row[3]);
            Discard = Program.ParseFormula(row[4]);
            Activity = Program.ParseInt(row[5]);
            Sextype = Program.ParseBool(row[6]);
            return true;
        }
    }
}
