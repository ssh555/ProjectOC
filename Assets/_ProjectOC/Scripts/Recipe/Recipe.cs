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
        public int Sort { get => LocalGameManager.Instance.RecipeManager.GetSort(ID); }
        [LabelText("��Ŀ"), ShowInInspector, ReadOnly]
        public RecipeCategory Category { get => LocalGameManager.Instance.RecipeManager.GetCategory(ID); }
        [LabelText("ԭ��"), ShowInInspector, ReadOnly]
        public List<Formula> Raw { get => LocalGameManager.Instance.RecipeManager.GetRaw(ID); }
        [LabelText("��Ʒ"), ShowInInspector, ReadOnly]
        public Formula Product { get => LocalGameManager.Instance.RecipeManager.GetProduct(ID); }
        [LabelText("��ƷID"), ShowInInspector, ReadOnly]
        public string ProductID { get => LocalGameManager.Instance.RecipeManager.GetProduct(ID).id; }
        [LabelText("��Ʒ����"), ShowInInspector, ReadOnly]
        public int ProductNum { get => LocalGameManager.Instance.RecipeManager.GetProduct(ID).num; }
        [LabelText("ʱ�����ģ�����1��������Ҫ������"), ShowInInspector, ReadOnly]
        public int TimeCost { get => LocalGameManager.Instance.RecipeManager.GetTimeCost(ID); }
        [LabelText("�䷽����"), ShowInInspector, ReadOnly]
        public int ExpRecipe { get => LocalGameManager.Instance.RecipeManager.GetExpRecipe(ID); }
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
