using ExcelToJson;
using System;
using System.Collections.Generic;

namespace ProjectOC.WorkerEchoNS
{
    [System.Serializable]
    public struct WorkerEchoTableData : IGenData
    {
        public string ID;
        public Category Category;
        public List<ML.Engine.InventorySystem.CompositeSystem.Formula> Raw;
        public int TimeCost;

        public bool GenData(string[] row)
        {
            if (row[0] == null || row[0] == "")
            {
                return false;
            }
            // 0 -> ID
            this.ID = row[0];
            // 1 -> Category
            this.Category = (Category)Enum.Parse(typeof(Category), row[1]);
            // 2 -> Raw
            this.Raw = Program.ParseFormula(row[2]);
            // 3 -> TimeCost
            this.TimeCost = int.Parse(row[3]);
            return true;
        }
    }
    [System.Serializable]
    public enum Category
    {
        None,
        Random,
        Cat,
        Deer,
        Fox,
        Rabbit,
        Dog,
        Seal,
    }
}
