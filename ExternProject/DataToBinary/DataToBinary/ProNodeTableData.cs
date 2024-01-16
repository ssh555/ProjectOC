using ExcelToJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ProjectOC.ProNodeNS
{
    [System.Serializable]
    public struct ProNodeTableData : IGenData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;
        public ProNodeType Type;
        public ML.Engine.InventorySystem.RecipeCategory Category;
        public List<ML.Engine.InventorySystem.RecipeCategory> RecipeCategoryFiltered;
        public WorkerNS.WorkType ExpType;
        public int Stack;
        public int StackThreshold;
        public int RawThreshold;

        public bool GenData(string[] row)
        {
            if (row[0] == null || row[0] == "")
            {
                return false;
            }
            // 0 -> ID
            this.ID = row[0];
            // 1 -> Name
            this.Name = new ML.Engine.TextContent.TextContent();
            this.Name.Chinese = row[1];
            this.Name.English = row[1];
            // 2 -> Type
            this.Type = (ProNodeType)Enum.Parse(typeof(ProNodeType), row[2]);
            // 3 -> Category
            this.Category = (ML.Engine.InventorySystem.RecipeCategory)Enum.Parse(typeof(ML.Engine.InventorySystem.RecipeCategory), row[3]);
            // 4 -> RecipeCategoryFiltered
            this.RecipeCategoryFiltered = new List<ML.Engine.InventorySystem.RecipeCategory>();
            foreach (string str in row[4].Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList())
            {
                this.RecipeCategoryFiltered.Add((ML.Engine.InventorySystem.RecipeCategory)Enum.Parse(typeof(ML.Engine.InventorySystem.RecipeCategory), str));
            }
            // 5 -> ExpType
            if (!string.IsNullOrEmpty(row[5]))
            {
                this.ExpType = (WorkerNS.WorkType)Enum.Parse(typeof(WorkerNS.WorkType), row[5]);
            }
            // 6 -> Stack
            this.Stack = int.Parse(row[6]);
            // 7 -> StackThreshold
            this.StackThreshold = int.Parse(row[7]);
            // 8 -> RawThreshold
            this.RawThreshold = int.Parse(row[8]);
            return true;
        }
    }
    public enum ProNodeType
    {
        None,
        Auto,
        Mannul,
    }
}
