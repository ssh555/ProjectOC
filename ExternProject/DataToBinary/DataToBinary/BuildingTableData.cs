using ExcelToJson;
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
        public List<InventorySystem.Formula> raw;
        public string upgradeID;
        public TextContent.TextContent ItemDescription;
        public TextContent.TextContent EffectDescription;

        public string GetClassificationString()
        {
            return $"{category1}_{category2}_{category3}_{category4}";
        }

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0])) { return false; }
            // 0 -> ID
            id = Program.ParseString(row[0]);
            // 1 -> Sort
            sort = Program.ParseInt(row[1]);
            // 2 -> Name
            name = Program.ParseTextContent(row[2]);
            // 3 -> Icon
            icon = Program.ParseString(row[3]);
            // 4 -> Category1
            category1 = Program.ParseString(row[4]);
            // 5 -> Category2
            category2 = Program.ParseString(row[5]);
            // 6 -> Category3
            category3 = Program.ParseString(row[6]);
            // 7 -> Category4
            category4 = Program.ParseString(row[7]);
            // 8 -> ActorID
            actorID = Program.ParseString(row[8]);
            // 9 -> Raw
            raw = Program.ParseFormulaList(row[9]);
            // 10 -> UpgradeID
            upgradeID = Program.ParseString(row[10]);
            ItemDescription = Program.ParseTextContent(row[11]);
            EffectDescription = Program.ParseTextContent(row[12]);
            return true;
        }
    }
}
