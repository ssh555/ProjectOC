using System.Collections.Generic;
using System.Linq;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public struct RecipeTableData
    {
        public string ID;
        public int Sort;
        public RecipeCategory Category;
        public TextContent.TextContent Name;
        public List<CompositeSystem.Formula> Raw;
        public CompositeSystem.Formula Product;
        public int TimeCost;
        public int ExpRecipe;
    }

    [System.Serializable]
    public sealed class RecipeManager : Manager.LocalManager.ILocalManager
    {
        public void OnRegister()
        {
            LoadTableData();
        }

        #region Load And Data
        public Dictionary<RecipeCategory, List<string>> RecipeCategorys = new Dictionary<RecipeCategory, List<string>>();
        private Dictionary<string, RecipeTableData> RecipeTableDict = new Dictionary<string, RecipeTableData>();
        public ML.Engine.ABResources.ABJsonAssetProcessor<RecipeTableData[]> ABJAProcessor;
        public void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<RecipeTableData[]>("OCTableData", "Recipe", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.RecipeTableDict.Add(data.ID, data);

                    if (!this.RecipeCategorys.ContainsKey(data.Category))
                    {
                        this.RecipeCategorys[data.Category] = new List<string>();
                    }
                    this.RecipeCategorys[data.Category].Add(data.ID);
                }
            }, "配方表数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion

        #region Spawn
        public Recipe SpawnRecipe(string id)
        {
            if (IsValidID(id))
            {
                return new Recipe(RecipeTableDict[id]);
            }
            return default(Recipe);
        }
        #endregion

        #region Getter
        public bool IsValidID(string id) { return !string.IsNullOrEmpty(id) ? RecipeTableDict.ContainsKey(id) : false; }

        public UnityEngine.Sprite GetRecipeIcon(string id) { return IsValidID(id) ? ItemManager.Instance.GetItemSprite(RecipeTableDict[id].Product.id) : null; }

        public List<string> GetRecipeIDsByCategory(RecipeCategory category)
        {
            List<string> result = new List<string>();
            if (RecipeCategorys.ContainsKey(category))
            {
                result.AddRange(RecipeCategorys[category]);
            }
            return result;
        }

        public List<string> SortRecipeIDs(List<string> recipeIDs)
        {
            List<string> resultes = new List<string>();
            if (recipeIDs != null)
            {
                List<System.Tuple<string, int>> temps = new List<System.Tuple<string, int>>();
                foreach (string id in recipeIDs)
                {
                    temps.Add(new System.Tuple<string, int>(id, GetSort(id)));
                }
                temps.Sort((t1, t2) => { return t1.Item2 != t2.Item2 ? t1.Item2.CompareTo(t2.Item2) : t1.Item1.CompareTo(t2.Item1); });
                foreach (var tuple in temps)
                {
                    resultes.Add(tuple.Item1);
                }
            }
            return resultes;
        }
        
        public string[] GetAllID() { return RecipeTableDict.Keys.ToArray(); }

        public string GetRecipeName(string id) { return IsValidID(id) ? RecipeTableDict[id].Name : null; }

        public int GetSort(string id) { return IsValidID(id) ? RecipeTableDict[id].Sort : int.MaxValue; }

        public RecipeCategory GetCategory(string id) { return IsValidID(id) ? RecipeTableDict[id].Category : RecipeCategory.None; }

        public List<CompositeSystem.Formula> GetRaw(string id)
        {
            List<CompositeSystem.Formula> result = new List<CompositeSystem.Formula>();
            if (IsValidID(id))
            {
                result.AddRange(RecipeTableDict[id].Raw);
            }
            return result;
        }

        public CompositeSystem.Formula GetProduct(string id) 
        { 
            return IsValidID(id) ? RecipeTableDict[id].Product : new CompositeSystem.Formula() { id = "", num = 0 }; 
        }
        public int GetTimeCost(string id) { return IsValidID(id) ? RecipeTableDict[id].TimeCost : 0; }
        public int GetExpRecipe(string id) { return IsValidID(id) ? RecipeTableDict[id].ExpRecipe : 0; }
        #endregion
    }
}
