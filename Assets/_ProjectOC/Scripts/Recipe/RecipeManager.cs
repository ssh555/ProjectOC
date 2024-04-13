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

    [System.Serializable]
    public sealed class RecipeManager : Manager.LocalManager.ILocalManager
    {
        public void OnRegister()
        {
            LoadTableData();
        }

        #region Load And Data
        /// <summary>
        /// �Ƿ��Ѽ���������
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        public Dictionary<RecipeCategory, List<string>> RecipeCategorys = new Dictionary<RecipeCategory, List<string>>();
        /// <summary>
        /// Recipe ���ݱ�
        /// </summary>
        private Dictionary<string, RecipeTableData> RecipeTableDict = new Dictionary<string, RecipeTableData>();

        public ML.Engine.ABResources.ABJsonAssetProcessor<RecipeTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<RecipeTableData[]>("OC/Json/TableData", "Recipe", (datas) =>
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
            }, "�䷽������");
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
            return null;
        }
        #endregion

        #region Getter
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
                List<Tuple<string, int>> temps = new List<Tuple<string, int>>();
                foreach (string id in recipeIDs)
                {
                    temps.Add(new Tuple<string, int>(id, GetSort(id)));
                }
                temps.Sort((t1, t2) => { return t1.Item2 != t2.Item2 ? t1.Item2.CompareTo(t2.Item2) : t1.Item1.CompareTo(t2.Item1); });
                foreach (var tuple in temps)
                {
                    resultes.Add(tuple.Item1);
                }
            }
            return resultes;
        }
        
        public string[] GetAllID()
        {
            return RecipeTableDict.Keys.ToArray();
        }

        public bool IsValidID(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return RecipeTableDict.ContainsKey(id);
            }
            return false;
        }

        public int GetSort(string id)
        {
            if (IsValidID(id))
            {
                return RecipeTableDict[id].Sort;
            }
            return int.MaxValue;
        }

        public RecipeCategory GetCategory(string id)
        {
            if (IsValidID(id))
            {
                return RecipeTableDict[id].Category;
            }
            return RecipeCategory.None;
        }

        public List<Formula> GetRaw(string id)
        {
            List<Formula> result = new List<Formula>();
            if (IsValidID(id))
            {
                result.AddRange(RecipeTableDict[id].Raw);
            }
            return result;
        }

        public Formula GetProduct(string id)
        {
            if (IsValidID(id))
            {
                return RecipeTableDict[id].Product;
            }
            return new Formula() { id = "", num = 0 };
        }

        public int GetTimeCost(string id)
        {
            if (IsValidID(id))
            {
                return RecipeTableDict[id].TimeCost;
            }
            return 1;
        }

        public int GetExpRecipe(string id)
        {
            if (IsValidID(id))
            {
                return RecipeTableDict[id].ExpRecipe;
            }
            return 0;
        }
        #endregion
    }
}
