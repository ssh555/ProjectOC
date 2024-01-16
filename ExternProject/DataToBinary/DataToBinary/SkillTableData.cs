using ExcelToJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct SkillTableData : IGenData
    {
        public string ID;
        public int Sort;
        public string Icon;
        public WorkType AbilityType;
        public List<string> Effects;
        public ML.Engine.TextContent.TextContent ItemDescription;
        public ML.Engine.TextContent.TextContent EffectsDescription;

        public bool GenData(string[] row)
        {
            if (row[0] == null || row[0] == "")
            {
                return false;
            }
            // 0 -> ID
            this.ID = row[0];
            // 1 -> Sort
            this.Sort = int.Parse(row[1]);
            // 2 -> Icon
            this.Icon = row[2];
            // 3 -> AbilityType
            this.AbilityType = (WorkType)Enum.Parse(typeof(WorkType), row[3]);
            // 4 -> Effects
            this.Effects = row[4].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();
            // 5 -> ItemDescription
            this.ItemDescription = new ML.Engine.TextContent.TextContent();
            this.ItemDescription.Chinese = row[5];
            this.ItemDescription.English = row[5];
            // 6  -> EffectsDescription
            this.EffectsDescription = new ML.Engine.TextContent.TextContent();
            this.EffectsDescription.Chinese = row[6];
            this.EffectsDescription.English = row[6];
            return true;
        }
    }
}
