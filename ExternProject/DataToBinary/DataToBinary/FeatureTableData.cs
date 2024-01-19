using ExcelToJson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct FeatureTableData : IGenData
    {
        public string ID;
        public string IDExclude;
        public int Sort;
        public ML.Engine.TextContent.TextContent Name;
        public string Icon;
        public FeatureType Type;
        public List<Tuple<string, string>> Effects;
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
            // 1 -> IDExclude
            this.IDExclude = !string.IsNullOrEmpty(row[1]) ? row[1] : "";
            // 2 -> Sort
            this.Sort = int.Parse(row[2]);
            // 3 -> Name
            this.Name = new ML.Engine.TextContent.TextContent();
            this.Name.Chinese = row[3];
            this.Name.English = row[3];
            // 4 -> Icon
            this.Icon = !string.IsNullOrEmpty(row[4]) ? row[4] : "";
            // 5 -> Type
            this.Type = (FeatureType)Enum.Parse(typeof(FeatureType), row[5]);
            // 6 -> Effects
            this.Effects = new List<Tuple<string, string>>();
            if (!string.IsNullOrEmpty(row[6]))
            {
                foreach (string str in row[6].Split(';').Where(x => !string.IsNullOrEmpty(x)))
                {
                    string[] s = str.Split(',');
                    this.Effects.Add(new Tuple<string, string>(s[0], s[1]));
                }
            }
            // 7 -> ItemDescription
            this.ItemDescription = new ML.Engine.TextContent.TextContent();
            this.ItemDescription.Chinese = !string.IsNullOrEmpty(row[7]) ? row[7] : "";
            this.ItemDescription.English = !string.IsNullOrEmpty(row[7]) ? row[7] : "";
            // 8  -> EffectsDescription
            this.EffectsDescription = new ML.Engine.TextContent.TextContent();
            this.EffectsDescription.Chinese = !string.IsNullOrEmpty(row[8]) ? row[8] : "";
            this.EffectsDescription.English = !string.IsNullOrEmpty(row[8]) ? row[8] : "";
            return true;
        }
    }
    [System.Serializable]
    public enum FeatureType
    {
        Race,
        Buff,
        DeBuff,
        None,
    }
}
