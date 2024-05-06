using ExcelToJson;
using System.Collections.Generic;

namespace ProjectOC.WorkerEchoNS
{
    [System.Serializable]
    public struct WorkerEchoTableData : IGenData
    {
        public string ID;
        public Category Category;
        public List<ML.Engine.InventorySystem.Formula> Raw;
        public int TimeCost;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Category
            this.Category = Program.ParseEnum<Category>(row[1]);
            // 2 -> Raw
            this.Raw = Program.ParseFormulaList(row[2]);
            // 3 -> TimeCost
            this.TimeCost = Program.ParseInt(row[3]);
            return true;
        }
    }
    [System.Serializable]
    public enum Category
    {
        None,
        Random,
        CookWorker,
        HandCraftWorker,
        IndustryWorker,
        MagicWorker,
        TransportWorker,
        CollectWorker
    }
}
