using System;
using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    [LabelText("养殖仓库"), Serializable]
    public class CreatureStore : IStore
    {
        [LabelText("养殖生物ID"), ReadOnly, ShowInInspector]
        public string CreatureItemID { get; private set; }

        public CreatureStore(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType, int level) : base(storeType, level, false)
        {
            CreatureItemID = "";
            InitData(15, 1);
            ChangeDataAutoSort = true;
        }
        public void ChangeCreature(string itemID)
        {
            if (ManagerNS.LocalGameManager.Instance.ItemManager.GetItemType(itemID) == ML.Engine.InventorySystem.ItemType.Creature)
            {
                if (CreatureItemID != itemID) { ClearData(); }
                CreatureItemID = itemID;
            }
        }
        public void FastAdd()
        {
            if (!string.IsNullOrEmpty(CreatureItemID))
            {
                foreach (var item in ManagerNS.LocalGameManager.Instance.Player.GetInventory().GetItemList())
                {
                    if (item.ID == CreatureItemID && item is ML.Engine.InventorySystem.CreatureItem creature)
                    {
                        int index = DataContainer.GetEmptyIndex();
                        if (index >= 0)
                        {
                            ChangeData(index, creature);
                            DataContainer.ChangeAmount(creature, 1, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty);
                        }
                    }
                }
            }
        }
    }
}