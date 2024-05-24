using ExcelToJson;
using System.Collections.Generic;

namespace ProjectOC.MineSystem
{
    [System.Serializable]
    public struct MineralTableData : IGenData
    {
        public string ID;
        public string Icon;
        public List<ML.Engine.InventorySystem.Formula> MineEff;
        public int MineNum;
        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0])) { return false; }
            ID = Program.ParseString(row[0]);
            Icon = Program.ParseString(row[1]);
            MineEff = Program.ParseFormulaList(row[2]);
            MineNum = Program.ParseInt(row[3]);
            return true;
        }
    }
}