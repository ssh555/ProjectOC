using ExcelToJson;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (row[0] == null || row[0] == "")
            {
                return false;
            }
            // 0 -> ID
            this.ID = row[0];
            // 1 -> Sort
            this.Sort = int.Parse(row[1]);
            // 2 -> Category
            this.Category = (RecipeCategory)Enum.Parse(typeof(RecipeCategory), row[2]);
            // 3 -> Name
            this.Name = new TextContent.TextContent();
            this.Name.Chinese = row[3];
            this.Name.English = row[3];
            // 4 -> Raw
            this.Raw = Program.ParseFormula(row[4]);
            // 5 -> Product
            List<CompositeSystem.Formula> temp = Program.ParseFormula(row[5]);
            if (temp.Count >= 1)
            {
                this.Product = temp[0];
            }
            else
            {
                this.Product = new CompositeSystem.Formula();
            }
            // 6 -> TimeCost
            this.TimeCost = int.Parse(row[6]);
            // 7 -> ExpRecipe
            this.ExpRecipe = int.Parse(row[7]);
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
        Detector,
        Refine,
        Sawn,
        Textile,
        Aggregator,
        Processor,
        Kitchen,
        Calculus,
        Store,
        Reservoir,
        Incubator,
        EchoWheel,
        DiversionNode,
        LifeDiversion,
        Projector,
    }
}
