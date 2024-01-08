using System;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;

namespace ML.Engine.InventorySystem
{
    /// <summary>
    /// 配方
    /// </summary>
    [System.Serializable]
    public class Recipe
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";

        #region 读表数据
        /// <summary>
        /// 序号用于排序
        /// </summary>
        public int Sort { get => RecipeManager.Instance.GetSort(ID); }

        /// <summary>
        /// 类目
        /// </summary>
        public RecipeCategory Category { get => RecipeManager.Instance.GetCategory(ID); }
        /// <summary>
        /// 原料
        /// </summary>
        public Dictionary<string, int> Raw { get => RecipeManager.Instance.GetRaw(ID); }
        /// <summary>
        /// 成品
        /// </summary>
        public Dictionary<string, int> Product { get => RecipeManager.Instance.GetProduct(ID); }
        /// <summary>
        /// 时间消耗，进行1次生产需要多少秒
        /// </summary>
        public int TimeCost { get => RecipeManager.Instance.GetTimeCost(ID); }
        /// <summary>
        /// 配方经验，该配方制作完成时可获得的经验值
        /// </summary>
        public int ExpRecipe { get => RecipeManager.Instance.GetExpRecipe(ID); }
        #endregion

        public Recipe(RecipeManager.RecipeTableJsonData config)
        {
            this.ID = config.ID;
        }

        public Recipe(Recipe recipe)
        {
            this.ID = recipe.ID;
        }

        public string GetProductID()
        {
            if (Product.Count == 1)
            {
                foreach (var kv in Product)
                {
                    return kv.Key;
                }
            }
            Debug.LogError("Recipe Product Num is Error");
            return "";
        }

        public int GetProductNum()
        {
            if (Product.Count == 1)
            {
                foreach (var kv in Product)
                {
                    return kv.Value;
                }
            }
            Debug.LogError("Recipe Product Num is Error");
            return 0;
        }

        public int GetRawNum(string itemID)
        {
            if (Raw.ContainsKey(itemID))
            {
                return Raw[itemID];
            }
            else
            {
                Debug.LogError($"Raw {itemID} is not exist in recipe {ID}");
                return 0;
            }
        }

        public Item Composite(IInventory inventory)
        {
            if (Product.Count == 1)
            {
                foreach (var kv in Product)
                {
                    CompositeManager.CompositionObjectType compObjType = CompositeManager.Instance.Composite(inventory, kv.Key, out var composition);
                    switch (compObjType)
                    {
                        case CompositeManager.CompositionObjectType.Item:
                            Item item = composition as Item;
                            if (item.Amount != GetProductNum())
                            {
                                Debug.LogError($"Recipe {ID} Product {kv.Key} Item Num is Error");
                            }
                            return item;
                        default:
                            Debug.LogError($"Recipe {ID} Product {kv.Key} Composite {compObjType}");
                            break;
                    }
                }
            }
            Debug.LogError("Recipe Product Num is Error");
            return null;
        }
    }
}
