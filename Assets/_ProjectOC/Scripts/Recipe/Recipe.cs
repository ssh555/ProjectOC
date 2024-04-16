using System;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;

namespace ML.Engine.InventorySystem
{
    [LabelText("�䷽"), System.Serializable]
    public class Recipe
    {
        [LabelText("ID"), ReadOnly]
        public string ID = "";

        #region ��������
        [LabelText("����"), ShowInInspector, ReadOnly]
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
        [LabelText("��Ŀ"), ShowInInspector, ReadOnly]
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
        [LabelText("ԭ��"), ShowInInspector, ReadOnly]
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
        [LabelText("��Ʒ"), ShowInInspector, ReadOnly]
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
        [LabelText("��ƷID"), ShowInInspector, ReadOnly]
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
        [LabelText("��Ʒ����"), ShowInInspector, ReadOnly]
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
        [LabelText("ʱ�����ģ�����1��������Ҫ������"), ShowInInspector, ReadOnly]
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
        [LabelText("�䷽����"), ShowInInspector, ReadOnly]
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
