using ExcelToJson;
using System;
using System.Collections.Generic;

namespace ML.Engine.BuildingSystem
{
    [System.Serializable]
    public struct BuildingTableData : IGenData
    {
        public string id;
        public TextContent.TextContent name;
        public string icon;
        public string category1;
        public string category2;
        public string category3;
        public string category4;
        public string actorID;
        public List<InventorySystem.CompositeSystem.Formula> raw;
        public List<InventorySystem.CompositeSystem.Formula> upgradeRaw;

        public bool GenData(string[] row)
        {
            if (row[0] == null || row[0] == "")
            {
                return false;
            }
            // 0 -> id
            this.id = row[0];
            // 1 -> name
            this.name = new TextContent.TextContent();
            this.name.Chinese = row[1];
            this.name.English = row[1];
            // 2 -> icon
            this.icon = row[2];
            // 3 -> category1
            this.category1 = row[3];
            // 4 -> category2
            this.category2 = row[4];
            // 5 -> category3
            this.category3 = row[5];
            // 6 -> category4
            this.category4 = row[6];
            // 7 -> actorID
            this.actorID = !string.IsNullOrEmpty(row[7]) ? row[7] : "";
            // 8 -> raw
            this.raw = Program.ParseFormula(row[8]);
            // 9 -> upgradeRaw
            this.upgradeRaw = Program.ParseFormula(row[9]);
            return true;
        }
    }
}
