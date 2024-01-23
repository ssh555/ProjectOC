using System;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;
using ProjectOC.ManagerNS;

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
        public int Sort { get => LocalGameManager.Instance.RecipeManager.GetSort(ID); }

        /// <summary>
        /// 类目
        /// </summary>
        public RecipeCategory Category { get => LocalGameManager.Instance.RecipeManager.GetCategory(ID); }
        /// <summary>
        /// 原料
        /// </summary>
        public List<Formula> Raw { get => LocalGameManager.Instance.RecipeManager.GetRaw(ID); }
        /// <summary>
        /// 成品
        /// </summary>
        public Formula Product { get => LocalGameManager.Instance.RecipeManager.GetProduct(ID); }
        /// <summary>
        /// 时间消耗，进行1次生产需要多少秒
        /// </summary>
        public int TimeCost { get => LocalGameManager.Instance.RecipeManager.GetTimeCost(ID); }
        /// <summary>
        /// 配方经验，该配方制作完成时可获得的经验值
        /// </summary>
        public int ExpRecipe { get => LocalGameManager.Instance.RecipeManager.GetExpRecipe(ID); }
        #endregion

        public Recipe(RecipeTableData config)
        {
            this.ID = config.ID;
        }

        public Recipe(Recipe recipe)
        {
            this.ID = recipe.ID;
        }

        public string GetProductID()
        {
            return Product.id ?? "";
        }

        public int GetProductNum()
        {
            return Product.num;
        }

        public int GetRawNum(string itemID)
        {
            foreach (Formula raw in Raw)
            {
                if (raw.id == itemID)
                {
                    return raw.num;
                }
            }
            Debug.LogError($"Raw {itemID} is not exist in recipe {ID}");
            return 0;
        }

        public Item Composite(IInventory inventory)
        {
            
            CompositeManager.CompositionObjectType compObjType = CompositeManager.Instance.Composite(inventory, Product.id, out var composition);
            switch (compObjType)
            {
                case CompositeManager.CompositionObjectType.Item:
                    Item item = composition as Item;
                    if (item.Amount != GetProductNum())
                    {
                        Debug.LogError($"Recipe {ID} Product {Product.id} Item Num is Error");
                    }
                    return item;
                default:
                    Debug.LogError($"Recipe {ID} Product {Product.id} Composite {compObjType}");
                    break;
            }
            Debug.LogError("Recipe Product Num is Error");
            return null;
        }
    }
}
