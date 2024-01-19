using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using ML.Engine.InventorySystem.CompositeSystem;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public struct RecipeTableData
    {
        public string ID;
        public int Sort;
        public RecipeCategory Category;
        public TextContent.TextContent Name;
        public List<Formula> Raw;
        public Formula Product;
        public int TimeCost;
        public int ExpRecipe;
    }

    public sealed class RecipeManager : Manager.LocalManager.ILocalManager
    {
        #region Load And Data
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        public Dictionary<RecipeCategory, List<string>> RecipeCategorys = new Dictionary<RecipeCategory, List<string>>();
        /// <summary>
        /// Recipe 数据表
        /// </summary>
        private Dictionary<string, RecipeTableData> RecipeTableDict = new Dictionary<string, RecipeTableData>();

        public static ML.Engine.ABResources.ABJsonAssetProcessor<RecipeTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<RecipeTableData[]>("Binary/TableData", "Recipe", (datas) =>
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
                }, null, "配方表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region Spawn
        /// <summary>
        /// 根据id创建新的配方
        /// </summary>
        public Recipe SpawnRecipe(string id)
        {
            if (RecipeTableDict.TryGetValue(id, out RecipeTableData row))
            {
                Recipe recipe = new Recipe(row);
                return recipe;
            }
            Debug.LogError("没有对应ID为 " + id + " 的配方");
            return null;
        }
        #endregion

        #region Getter
        public List<string> GetRecipeIDsByCategory(RecipeCategory category)
        {
            List<string> result = new List<string>();
            if (RecipeCategorys.ContainsKey(category) && RecipeCategorys.ContainsKey(category) && RecipeCategorys[category] != null)
            {
                result.AddRange(RecipeCategorys[category]);
            }
            return result;
        }
        
        public string[] GetAllRecipeID()
        {
            return RecipeTableDict.Keys.ToArray();
        }

        public bool IsValidID(string id)
        {
            return RecipeTableDict.ContainsKey(id);
        }

        public int GetSort(string id)
        {
            if (!RecipeTableDict.ContainsKey(id))
            {
                return int.MaxValue;
            }
            return RecipeTableDict[id].Sort;
        }

        public RecipeCategory GetCategory(string id)
        {
            if (!RecipeTableDict.ContainsKey(id))
            {
                return RecipeCategory.None;
            }
            return RecipeTableDict[id].Category;
        }

        public List<Formula> GetRaw(string id)
        {
            List<Formula> result = new List<Formula>();
            if (RecipeTableDict.ContainsKey(id))
            {
                result.AddRange(RecipeTableDict[id].Raw);
            }
            return result;
        }

        public Formula GetProduct(string id)
        {
            if (RecipeTableDict.ContainsKey(id))
            {
                return RecipeTableDict[id].Product;
            }
            return new Formula();
        }

        public int GetTimeCost(string id)
        {
            if (!RecipeTableDict.ContainsKey(id))
            {
                return 1;
            }
            return RecipeTableDict[id].TimeCost;
        }

        public int GetExpRecipe(string id)
        {
            if (!RecipeTableDict.ContainsKey(id))
            {
                return 0;
            }
            return RecipeTableDict[id].ExpRecipe;
        }
        #endregion
    }
}
