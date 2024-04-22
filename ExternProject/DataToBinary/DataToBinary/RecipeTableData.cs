using ExcelToJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public struct RecipeTableData : IGenData
    {
        public string ID;
        public int Sort;
        public RecipeCategory Category;
        public TextContent.TextContent Name;
        public List<CompositeSystem.Formula> Raw;
        public CompositeSystem.Formula Product;
        public int TimeCost;
        public int ExpRecipe;
        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Sort
            this.Sort = Program.ParseInt(row[1]);
            // 2 -> Category
            this.Category = Program.ParseEnum<RecipeCategory>(row[2]);
            // 3 -> Name
            this.Name = Program.ParseTextContent(row[3]);
            // 4 -> Raw
            this.Raw = Program.ParseFormulaList(row[4]);
            // 5 -> Product
            this.Product = Program.ParseFormula(row[5]);
            // 6 -> TimeCost
            this.TimeCost = Program.ParseInt(row[6]);
            // 7 -> ExpRecipe
            this.ExpRecipe = Program.ParseInt(row[7]);
            return true;
        }
    }
    [System.Serializable]
    public enum RecipeCategory
    {
        None,
        WaterCollector,
        WaterPurifier,
        SeedBox,
        Integrator,
        SeedPlot,
        DiveStation,
        Refine,
        Handwork,
        Pharmacy,
        Liquor,
        Aggregator,
        Processor,
        Kitchen,
        CreaturePro,
        CreatureBreed,
        Calculus
    }
}
