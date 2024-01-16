using ExcelToJson;
using ML.Engine.BuildingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    [System.Serializable]
    public class CompositionTableData : IGenData
    {
        /// <summary>
        /// 合成对象 -> Item | 建筑物 ID 引用
        /// </summary>
        public string id;
        /// <summary>
        /// 合成物名称
        /// </summary>
        public TextContent.TextContent name;
        /// <summary>
        /// 标签分级
        /// 1级|2级|3级
        /// </summary>
        public string[] tag;
        /// <summary>
        /// 合成公式
        /// 没有 num 则默认为 1
        /// num 目前仅限 item
        /// num = <1,2> | <1,2>
        /// 1 -> ID 
        /// 2 -> Num
        /// </summary>
        public Formula[] formula;
        /// <summary>
        /// 一次可合成数量
        /// </summary>
        public int compositionnum;
        public string texture2d;
        public List<string> usage;

        public CompositionTableData(RecipeTableData data)
        {
            this.id = data.Product[0].Item1;
            this.compositionnum = data.Product[0].Item2;
            this.name = data.Name;
            List<Formula> temp = new List<Formula>();
            foreach (var tuple in data.Raw)
            {
                temp.Add(new Formula { id = tuple.Item1, num = tuple.Item2} );
            }
            formula = temp.ToArray();

            this.tag = new List<string>().ToArray();
            this.texture2d = "";
            this.usage = new List<string>();
        }
        public CompositionTableData(BuildingTableData data)
        {
            this.id = data.id;
            this.name = data.name;
            this.tag = new string[] { data.category1, data.category2, data.category3 };
            List<Formula> temp = new List<Formula>();
            foreach (var tuple in data.raw)
            {
                temp.Add(new Formula { id=tuple.Item1, num=tuple.Item2});
            }
            this.formula = temp.ToArray();
            this.compositionnum = 1;
            this.texture2d = data.icon;
            this.usage = new List<string>();
        }
        public CompositionTableData(BuildingUpgradeTableData data)
        {
            this.id = data.id;
            this.name = data.name;
            List<Formula> temp = new List<Formula>();
            foreach (var tuple in data.upgradeRaw)
            {
                temp.Add(new Formula { id = tuple.Item1, num = tuple.Item2 });
            }
            this.formula = temp.ToArray();
            this.compositionnum = 1;
            this.tag = new List<string>().ToArray();
            this.texture2d = "";
            this.usage = new List<string>();
        }
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
            // 2 -> tag
            this.tag = row[2].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            // 3 -> formula
            List<Formula> temp = new List<Formula>();
            foreach (var str in row[3].Split(';').Where(x => !string.IsNullOrEmpty(x)))
            {
                string[] s = str.Split(',');
                temp.Add(new Formula { id = s[0], num = int.Parse(s[1]) });
            }
            formula = temp.ToArray();
            // 4 -> compositionnum
            this.compositionnum = int.Parse(row[4]);
            // 5 -> texture2d
            this.texture2d = row[5];
            // 6 -> usage
            this.usage = row[6].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();
            return true;
        }
    }
}
