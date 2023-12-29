using System.Collections.Generic;
using UnityEngine;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using Newtonsoft.Json;
using System.Collections;
using static ProjectOC.StoreNS.StoreManager;

namespace ProjectOC.ProductionNodeNS
{
    public sealed class RecipeManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private RecipeManager()
        {
            RecipeCategorys.Add(ItemCategory.Food, new List<string>());
            RecipeCategorys.Add(ItemCategory.Crop, new List<string>());
            RecipeCategorys.Add(ItemCategory.Cloth, new List<string>());
            RecipeCategorys.Add(ItemCategory.Metal, new List<string>());
            RecipeCategorys.Add(ItemCategory.Mineral, new List<string>());
        }

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

        public Dictionary<ItemCategory, List<string>> RecipeCategorys = new Dictionary<ItemCategory, List<string>>();
        /// <summary>
        /// 基础Recipe数据表
        /// </summary>
        private Dictionary<string, RecipeTableJsonData> RecipeTableDict = new Dictionary<string, RecipeTableJsonData>();

        /// <summary>
        /// 是否是有效的配方ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidID(string id)
        {
            return this.RecipeTableDict.ContainsKey(id);
        }
        public List<string> GetRecipeIDsByCategory(ItemCategory category)
        {
            List<string> result = new List<string>();
            if (RecipeCategorys.ContainsKey(category) && RecipeCategorys[category] != null)
            {
                result.AddRange(RecipeCategorys[category]);
            }
            return result;
        }

        /// <summary>
        /// 根据id创建新的配方
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Recipe SpawnRecipe(string id)
        {
            if (RecipeTableDict.ContainsKey(id))
            {
                RecipeTableJsonData row = this.RecipeTableDict[id];
                Recipe recipe = new Recipe();
                recipe.Init(row);
                return recipe;
            }
            Debug.LogError("没有对应ID为 " + id + " 的配方");
            return null;
        }

        #region to-do : 需读表导入所有所需的 Recipe 数据
        [System.Serializable]
        public struct RecipeTableJsonData
        {
            public string id;
            public ItemCategory category;
            public Dictionary<string, int> rawItems;
            public string productItem;
            public int timeCost;
            public int expRecipe;
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
                        this.RecipeTableDict.Add(data.id, data);
                        if (!this.RecipeCategorys.ContainsKey(data.category))
                        {
                            this.RecipeCategorys[data.category] = new List<string>();
                        }
                        this.RecipeCategorys[data.category].Add(data.id);
                    }
                }, null, "配方表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion
    }
}
