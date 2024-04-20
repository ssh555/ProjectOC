using ExcelToJson;
using System;
using System.Collections.Generic;
using System.Linq;


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
        public int MaxStack;
        public int StackThreshold;
        public int RawThreshold;
        public bool CanCharge;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Name
            this.Name = Program.ParseTextContent(row[1]);
            // 2 -> Type
            this.Type = Program.ParseEnum<ProNodeType>(row[2]);
            // 3 -> Category
            this.Category = Program.ParseEnum<ML.Engine.InventorySystem.RecipeCategory>(row[3]);
            // 4 -> RecipeCategoryFiltered
            this.RecipeCategoryFiltered = Program.ParseEnumList<ML.Engine.InventorySystem.RecipeCategory>(row[4]);
            // 5 -> ExpType
            this.ExpType = Program.ParseEnum<WorkerNS.WorkType>(row[5]);
            // 6 -> Stack
            this.MaxStack = Program.ParseInt(row[6]);
            // 7 -> StackThreshold
            this.StackThreshold = Program.ParseInt(row[7]);
            // 8 -> RawThreshold
            this.RawThreshold = Program.ParseInt(row[8]);
            // 9 -> CanCharge
            this.CanCharge = Program.ParseBool(row[9]);
            return true;
        }
    }
    [System.Serializable]
    public enum ProNodeType
    {
        None,
        Auto,
        Mannul,
    }
}
