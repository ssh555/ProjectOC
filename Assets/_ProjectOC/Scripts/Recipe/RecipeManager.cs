using System.Collections.Generic;
using UnityEngine;
using ML.Engine.Manager;
using System.Collections;
using System;
using System.Linq;

namespace ML.Engine.InventorySystem
{
    public sealed class RecipeManager : Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private RecipeManager(){}

        private static RecipeManager instance;

        public static RecipeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RecipeManager();
                    GameManager.Instance.RegisterGlobalManager(instance);
                    instance.LoadTableData();
                }
                return instance;
            }
        }
        #endregion

        #region Load And Data
        /// <summary>
        /// �Ƿ��Ѽ���������
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        public Dictionary<RecipeCategory, List<string>> RecipeCategorys = new Dictionary<RecipeCategory, List<string>>();
        /// <summary>
        /// Recipe ���ݱ�
        /// </summary>
        private Dictionary<string, RecipeTableJsonData> RecipeTableDict = new Dictionary<string, RecipeTableJsonData>();

        [System.Serializable]
        public struct RecipeTableJsonData
        {
            public string ID;
            public int Sort;
            public RecipeCategory Category;
            public Dictionary<string, int> Raw;
            public Dictionary<string, int> Product;
            public int TimeCost;
            public int ExpRecipe;
        }
        public static ML.Engine.ABResources.ABJsonAssetProcessor<RecipeTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<RecipeTableJsonData[]>("Json/TableData", "RecipeTableData", (datas) =>
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
                }, null, "�䷽������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region Spawn
        /// <summary>
        /// ����id�����µ��䷽
        /// </summary>
        public Recipe SpawnRecipe(string id)
        {
            if (RecipeTableDict.TryGetValue(id, out RecipeTableJsonData row))
            {
                Recipe recipe = new Recipe(row);
                return recipe;
            }
            Debug.LogError("û�ж�ӦIDΪ " + id + " ���䷽");
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

        public Dictionary<string, int> GetRaw(string id)
        {
            if (!RecipeTableDict.ContainsKey(id))
            {
                return new Dictionary<string, int>();
            }
            return RecipeTableDict[id].Raw;
        }

        public Dictionary<string, int> GetProduct(string id)
        {
            if (!RecipeTableDict.ContainsKey(id))
            {
                return new Dictionary<string, int>();
            }
            return RecipeTableDict[id].Product;
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
