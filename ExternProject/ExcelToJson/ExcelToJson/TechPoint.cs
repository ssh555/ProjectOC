using ExcelToJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectOC.TechTree
{
    [System.Serializable]
    public class TechPoint : IGenData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;
        public int[] Grid;
        public TechPointCategory Category;
        public string Icon;
        public ML.Engine.TextContent.TextContent Description;
        public bool IsUnlocked;
        public string[] UnLockRecipe;
        public string[] UnLockBuild;
        public string[] PrePoint;
        public ML.Engine.InventorySystem.CompositeSystem.Formula[] ItemCost;
        public int TimeCost;

        /// <summary>
        /// 将Row的一行字符串数组数据转换为此数据对象的实际数据
        /// </summary>
        /// <param name="row">Excel表格的一行实际数据</param>
        /// <returns>是否填充成功</returns>
        public bool GenData(string[] row)
        {
            if (row[0] == null || row[0] == "")
            {
                return false;
            }

            // 0 -> ID
            this.ID = row[0];
            // 1 -> Category
            this.Category = (ProjectOC.TechTree.TechPointCategory)Enum.Parse(typeof(ProjectOC.TechTree.TechPointCategory), row[1]);
            // 2 -> Name
            this.Name = new ML.Engine.TextContent.TextContent();
            this.Name.Chinese = row[2];
            this.Name.English = row[2];
            // 3 -> Icon
            this.Icon = row[3];
            // 4 -> 描述
            this.Description = new ML.Engine.TextContent.TextContent();
            this.Description.Chinese = row[4];
            this.Description.English = row[4];
            // 5 -> Grid
            var sgrid = row[5].Split(',');
            this.Grid = new int[2] { int.Parse(sgrid[0]), int.Parse(sgrid[1]) };
            // 6 -> IsUnlocked
            this.IsUnlocked = row[6] != "0";
            // 7 -> 配方
            this.UnLockRecipe = row[7].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            // 8 -> 建筑物
            this.UnLockBuild = row[8].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            // 9 -> 前置科技点
            this.PrePoint = row[9].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            // 10 -> ItemCost
            this.ItemCost = Program.ParseFormula(row[10]).ToArray();
            // 11 -> TimeCost
            this.TimeCost = int.Parse(row[11]);
            return true;
        }
    }
}
