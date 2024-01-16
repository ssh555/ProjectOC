using ExcelToJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Engine.BuildingSystem
{
    [System.Serializable]
    public struct BuildingUpgradeTableData : IGenData
    {
        public string id;
        public TextContent.TextContent name;
        public List<Tuple<string, int>> upgradeRaw;

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
            this.upgradeRaw = new List<Tuple<string, int>>();
            foreach (var str in row[2].Split(';').Where(x => !string.IsNullOrEmpty(x)))
            {
                string[] s = str.Split(',');
                this.upgradeRaw.Add(new Tuple<string, int>(s[0], int.Parse(s[1])));
            }
            return true;
        }
    }
}
