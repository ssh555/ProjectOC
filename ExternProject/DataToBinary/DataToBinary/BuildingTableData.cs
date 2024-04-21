using ExcelToJson;
using System;
using System.Collections.Generic;

namespace ML.Engine.BuildingSystem
{
    [System.Serializable]
    public struct BuildingTableData : IGenData
    {
        public string id;
        public int sort;
        public TextContent.TextContent name;
        public string icon;
        public string category1;
        public string category2;
        public string category3;
        public string category4;
        public string actorID;
        public List<InventorySystem.CompositeSystem.Formula> raw;
        public string upgradeID;

        public string GetClassificationString()
        {
            return $"{category1}_{category2}_{category3}_{category4}";
        }

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.id = Program.ParseString(row[0]);
            // 1 -> Sort
            this.sort = Program.ParseInt(row[1]);
            // 2 -> Name
            this.name = Program.ParseTextContent(row[2]);
            // 3 -> Icon
            this.icon = Program.ParseString(row[3]);
            // 4 -> Category1
            this.category1 = Program.ParseString(row[4]);
            // 5 -> Category2
            this.category2 = Program.ParseString(row[5]);
            // 6 -> Category3
            this.category3 = Program.ParseString(row[6]);
            // 7 -> Category4
            this.category4 = Program.ParseString(row[7]);
            // 8 -> ActorID
            this.actorID = Program.ParseString(row[8]);
            // 9 -> Raw
            this.raw = Program.ParseFormulaList(row[9]);
            // 10 -> UpgradeID
            this.upgradeID = Program.ParseString(row[10]);
            return true;
        }
    }
}
