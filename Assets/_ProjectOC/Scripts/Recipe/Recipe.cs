using System;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;

namespace ML.Engine.InventorySystem
{
    [LabelText("配方"), System.Serializable]
    public class Recipe
    {
        [LabelText("ID"), ReadOnly]
        public string ID = "";

        #region 读表数据
        [LabelText("排序"), ShowInInspector, ReadOnly]
        public int Sort
        { 
            get
            {
                if(LocalGameManager.Instance)
                {
                    return LocalGameManager.Instance.RecipeManager.GetSort(ID);

                }
                return -1;
            }
        }
        [LabelText("类目"), ShowInInspector, ReadOnly]
        public RecipeCategory Category
        {
            get
            {
                if (LocalGameManager.Instance)
                {
                    return LocalGameManager.Instance.RecipeManager.GetCategory(ID);

                }
                return 0;
            }
        }
        [LabelText("原料"), ShowInInspector, ReadOnly]
        public List<Formula> Raw
        {
            get
            {
                if (LocalGameManager.Instance)
                {
                    return LocalGameManager.Instance.RecipeManager.GetRaw(ID);

                }
                return null;
            }
        }
        [LabelText("成品"), ShowInInspector, ReadOnly]
        public Formula Product
        {
            get
            {
                if (LocalGameManager.Instance)
                {
                    return LocalGameManager.Instance.RecipeManager.GetProduct(ID);

                }
                return default(Formula);
            }
        }
        [LabelText("成品ID"), ShowInInspector, ReadOnly]
        public string ProductID
        {
            get
            {
                if (LocalGameManager.Instance)
                {
                    return LocalGameManager.Instance.RecipeManager.GetProduct(ID).id;

                }
                return null;
            }
        }
        [LabelText("成品数量"), ShowInInspector, ReadOnly]
        public int ProductNum
        {
            get
            {
                if (LocalGameManager.Instance)
                {
                    return LocalGameManager.Instance.RecipeManager.GetProduct(ID).num;

                }
                return 0;
            }
        }
        [LabelText("时间消耗，进行1次生产需要多少秒"), ShowInInspector, ReadOnly]
        public int TimeCost
        {
            get
            {
                if (LocalGameManager.Instance)
                {
                    return LocalGameManager.Instance.RecipeManager.GetTimeCost(ID);

                }
                return 0;
            }
        }
        [LabelText("配方经验"), ShowInInspector, ReadOnly]
        public int ExpRecipe
        {
            get
            {
                if (LocalGameManager.Instance)
                {
                    return LocalGameManager.Instance.RecipeManager.GetExpRecipe(ID);

                }
                return 0;
            }
        }
        #endregion

        public Recipe(RecipeTableData config)
        {
            this.ID = config.ID;
        }

        public int GetRawNum(string itemID)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (Formula raw in Raw)
                {
                    if (raw.id == itemID)
                    {
                        return raw.num;
                    }
                }
            }
            return 0;
        }

        public Item Composite(IInventory inventory)
        {
            CompositeManager.CompositionObjectType compObjType = CompositeManager.Instance.Composite(inventory, ID, out var composition);
            switch (compObjType)
            {
                case CompositeManager.CompositionObjectType.Item:
                    Item item = composition as Item;
                    return item;
                default:
                    break;
            }
            return null;
        }
    }
}
