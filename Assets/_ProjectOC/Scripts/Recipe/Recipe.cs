using System.Collections.Generic;
using ML.Engine.InventorySystem.CompositeSystem;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;

namespace ML.Engine.InventorySystem
{
    [LabelText("�䷽"), System.Serializable]
    public struct Recipe
    {
        [LabelText("ID"), ReadOnly]
        public string ID;

        public bool IsValidRecipe => !string.IsNullOrEmpty(ID) && (LocalGameManager.Instance == null || LocalGameManager.Instance.RecipeManager.IsValidID(ID));
        #region ��������
        [LabelText("����"), ShowInInspector, ReadOnly]
        public int Sort => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetSort(ID) : 999;
        [LabelText("��Ŀ"), ShowInInspector, ReadOnly]
        public RecipeCategory Category => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetCategory(ID) : RecipeCategory.None;
        [LabelText("ԭ��"), ShowInInspector, ReadOnly]
        public List<Formula> Raw => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetRaw(ID) : new List<Formula>();
        [LabelText("��Ʒ"), ShowInInspector, ReadOnly]
        public Formula Product => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetProduct(ID) : default(Formula);
        [LabelText("��ƷID"), ShowInInspector, ReadOnly]
        public string ProductID => Product.id;
        [LabelText("��Ʒ����"), ShowInInspector, ReadOnly]
        public int ProductNum => Product.num;
        [LabelText("ʱ�����ģ�����1��������Ҫ������"), ShowInInspector, ReadOnly]
        public int TimeCost => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetTimeCost(ID) : 0;
        [LabelText("�䷽����"), ShowInInspector, ReadOnly]
        public int ExpRecipe => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetExpRecipe(ID) : 0;
        #endregion

        public Recipe(RecipeTableData config)
        {
            this.ID = config.ID;
        }

        public void ClearData()
        {
            this.ID = "";
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
