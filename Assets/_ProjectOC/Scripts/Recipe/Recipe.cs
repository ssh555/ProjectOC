using System.Collections.Generic;
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
        [LabelText("ԭ��"), ShowInInspector, ReadOnly]
        public List<Formula> Raw => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetRaw(ID) : new List<Formula>();
        [LabelText("��Ʒ"), ShowInInspector, ReadOnly]
        public Formula Product => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetProduct(ID) : new Formula() { id = "" };
        [LabelText("��ƷID"), ShowInInspector, ReadOnly]
        public string ProductID => Product.id;
        [LabelText("��Ʒ����"), ShowInInspector, ReadOnly]
        public int ProductNum => Product.num;
        [LabelText("ʱ�����ģ�����1��������Ҫ������"), ShowInInspector, ReadOnly]
        public int TimeCost => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetTimeCost(ID) : 0;
        [LabelText("�䷽����"), ShowInInspector, ReadOnly]
        public int ExpRecipe => LocalGameManager.Instance != null ? LocalGameManager.Instance.RecipeManager.GetExpRecipe(ID) : 0;
        #endregion

        public Recipe(RecipeTableData config) { ID = config.ID; }
        public void ClearData() { ID = ""; }
        public int GetRawNum(string itemID)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (var raw in Raw)
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
            List<Formula> adds = new List<Formula>();
            foreach (var formula in Raw)
            {
                if(inventory.RemoveItem(formula.id, formula.num))
                {
                    adds.Add(formula);
                }
                else
                {
                    foreach (var add in adds)
                    {
                        inventory.AddItem(ItemManager.Instance.SpawnItems(add.id, add.num));
                    }
                    return null;
                }
            }
            Item item = ItemManager.Instance.SpawnItem(ProductID);
            item.Amount = ProductNum;
            return item;
        }
    }
}
