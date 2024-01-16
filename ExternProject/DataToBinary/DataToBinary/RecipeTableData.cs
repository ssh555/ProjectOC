using ExcelToJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public struct RecipeTableData : IGenData
    {
        public string ID;
        public int Sort;
        public RecipeCategory Category;
        public TextContent.TextContent Name;
        public List<Tuple<string, int>> Raw;
        public List<Tuple<string, int>> Product;
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
            this.Raw = new List<Tuple<string, int>>();
            foreach (string str in row[4].Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList())
            {
                string[] s = str.Split(',');
                this.Raw.Add(new Tuple<string, int>(s[0], int.Parse(s[1])));
            }
            // 5 -> Product
            this.Product = new List<Tuple<string, int>>();
            foreach (string str in row[5].Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList())
            {
                string[] s = str.Split(',');
                this.Product.Add(new Tuple<string, int>(s[0], int.Parse(s[1])));
            }
            // 6 -> Stack
            this.TimeCost = int.Parse(row[6]);
            // 7 -> StackThreshold
            this.ExpRecipe = int.Parse(row[7]);
            return true;
        }
    }
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
