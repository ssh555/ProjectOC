using System;
using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    [LabelText("养殖仓库"), Serializable]
    public class CreatureStore : IStore
    {
        [LabelText("养殖生物ID"), ReadOnly, ShowInInspector]
        public string CreatureItemID { get; private set; }

        public CreatureStore(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType) : base(storeType)
        {
            InitData(15, 1);
            DataContainer.OnDataChangeEvent += DataChangeSort;
        }

        public void DataChangeSort()
        {
            DataContainer.SortDataContainer();
        }

        public void ChangeCreature(string itemID)
        {
            if (ManagerNS.LocalGameManager.Instance.ItemManager.GetItemType(itemID) == ML.Engine.InventorySystem.ItemType.Creature)
            {
                if (CreatureItemID != itemID)
                {
                    ClearData();
                }
                CreatureItemID = itemID;
            }
        }
    }
}