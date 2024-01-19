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
        public TechPointCategory Category;
        public ML.Engine.TextContent.TextContent Name;
        public string Icon;
        public ML.Engine.TextContent.TextContent Description;
        public int[] Grid;
        public bool IsUnlocked;
        public string[] UnLockRecipe;
        public string[] UnLockBuild;
        public string[] PrePoint;
        public ML.Engine.InventorySystem.CompositeSystem.Formula[] ItemCost;
        public int TimeCost;

        /// <summary>
        /// ��Row��һ���ַ�����������ת��Ϊ�����ݶ����ʵ������
        /// </summary>
        /// <param name="row">Excel����һ��ʵ������</param>
        /// <returns>�Ƿ����ɹ�</returns>
        public bool GenData(string[] row)
        {
            if (row[0] == null || row[0] == "")
            {
                return false;
            }

            // 0 -> ID
            this.ID = row[0];
            // 1 -> Category
            this.Category = (TechPointCategory)Enum.Parse(typeof(TechPointCategory), row[1]);
            // 2 -> Name
            this.Name = new ML.Engine.TextContent.TextContent();
            this.Name.Chinese = row[2];
            this.Name.English = row[2];
            // 3 -> Icon
            this.Icon = row[3];
            // 4 -> ����
            this.Description = new ML.Engine.TextContent.TextContent();
            this.Description.Chinese = row[4];
            this.Description.English = row[4];
            // 5 -> Grid
            var sgrid = row[5].Split(',');
            this.Grid = new int[2] { int.Parse(sgrid[0]), int.Parse(sgrid[1]) };
            // 6 -> IsUnlocked
            this.IsUnlocked = row[6] == "True";
            // 7 -> �䷽
            if (!string.IsNullOrEmpty(row[7]))
            {
                this.UnLockRecipe = row[7].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }
            else
            {
                this.UnLockRecipe = new List<string>().ToArray();
            }
            // 8 -> ������
            if (!string.IsNullOrEmpty(row[8]))
            {
                this.UnLockBuild = row[8].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }
            else
            {
                this.UnLockBuild = new List<string>().ToArray();
            }
            // 9 -> ǰ�ÿƼ���
            if (!string.IsNullOrEmpty(row[9]))
            {
                this.PrePoint = row[9].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }
            else
            {
                this.PrePoint = new List<string>().ToArray();
            }
            // 10 -> ItemCost
            this.ItemCost = Program.ParseFormula(row[10]).ToArray();
            // 11 -> TimeCost
            this.TimeCost = !string.IsNullOrEmpty(row[11]) ? int.Parse(row[11]) : 0;
            return true;
        }
    }
}
