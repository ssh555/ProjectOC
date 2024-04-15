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
        /// <summary>
        /// ����
        /// </summary>
        public int Sort { get => LocalGameManager.Instance.RecipeManager.GetSort(ID); }
        /// <summary>
        /// ��Ŀ
        /// </summary>
        public RecipeCategory Category { get => LocalGameManager.Instance.RecipeManager.GetCategory(ID); }
        /// <summary>
        /// ԭ��
        /// </summary>
        public List<Formula> Raw { get => LocalGameManager.Instance.RecipeManager.GetRaw(ID); }
        /// <summary>
        /// ��Ʒ
        /// </summary>
        public Formula Product { get => LocalGameManager.Instance.RecipeManager.GetProduct(ID); }
        /// <summary>
        /// ��ƷID
        /// </summary>
        public string ProductID { get => LocalGameManager.Instance.RecipeManager.GetProduct(ID).id; }
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public int ProductNum { get => LocalGameManager.Instance.RecipeManager.GetProduct(ID).num; }
        /// <summary>
        /// ʱ�����ģ�����1��������Ҫ������
        /// </summary>
        public int TimeCost { get => LocalGameManager.Instance.RecipeManager.GetTimeCost(ID); }
        /// <summary>
        /// �䷽����
        /// </summary>
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
