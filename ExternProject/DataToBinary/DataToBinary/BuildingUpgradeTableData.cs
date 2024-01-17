using ExcelToJson;
using System;
using System.Collections.Generic;

namespace ML.Engine.BuildingSystem
{
    [System.Serializable]
    public struct BuildingUpgradeTableData : IGenData
    {
        public string id;
        public TextContent.TextContent name;
        public List<ML.Engine.InventorySystem.CompositeSystem.Formula> upgradeRaw;

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
            // 2 -> upgradeRaw
            this.upgradeRaw = Program.ParseFormula(row[2]);
            return true;
        }
    }
}
