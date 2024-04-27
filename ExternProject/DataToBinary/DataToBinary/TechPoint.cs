using ExcelToJson;
using System.Collections.Generic;

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
        public List<string> EventStrings;


        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Category
            this.Category = Program.ParseEnum<TechPointCategory>(row[1], "Other");
            // 2 -> Name
            this.Name = Program.ParseTextContent(row[2]);
            // 3 -> Icon
            this.Icon = Program.ParseString(row[3]);
            // 4 -> ItemDescription
            this.Description = Program.ParseTextContent(row[4]);
            // 5 -> Grid
            var sgrid = row[5].Split(',');
            this.Grid = new int[2] { Program.ParseInt(sgrid[0]), Program.ParseInt(sgrid[1]) };
            // 6 -> IsUnlocked
            this.IsUnlocked = Program.ParseBool(row[6]);
            // 7 -> UnlockRecipe
            this.UnLockRecipe = Program.ParseStringList(row[7]).ToArray();
            // 8 -> UnlockBuild
            this.UnLockBuild = Program.ParseStringList(row[8]).ToArray();
            // 9 -> PrePoint
            this.PrePoint = Program.ParseStringList(row[9]).ToArray();
            // 10 -> ItemCost
            this.ItemCost = Program.ParseFormulaList(row[10]).ToArray();
            // 11 -> TimeCost
            this.TimeCost = Program.ParseInt(row[11]);
            // 12 -> Event
            this.EventStrings = Program.ParseStringList(row[12]);
            return true;
        }
    }
}
