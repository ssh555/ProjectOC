using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.DataNS
{
    public abstract class ItemContainerOwner : MissionNS.IMissionObj, ML.Engine.InventorySystem.IInventory
    {
        [LabelText("´æ´¢Êý¾Ý"), ReadOnly]
        public DataContainer DataContainer;

        #region Init Clear
        public void InitData(int capacity, int dataCapacity)
        {
            DataContainer = new DataContainer(capacity, dataCapacity);
        }
        public void InitData(List<string> ids, List<int> dataCapacitys)
        {
            DataContainer = new DataContainer(ids, dataCapacitys);
        }
        public void ClearData()
        {
            (this as MissionNS.IMissionObj).Clear();
            if (DataContainer != null)
            {
                ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(DataContainer.GetAmount(DataOpType.StorageAll));
            }
        }
        public void ResetData(List<string> ids, List<int> dataCapacity)
        {
            ClearData();
            InitData(ids, dataCapacity);
        }
        #endregion

        #region Set
        public void ChangeCapacity(int capacity, int dataCapacity)
        {
            var dict = DataContainer.ChangeCapacity(capacity, dataCapacity);
            ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(dict);
            (this as MissionNS.IMissionObj).UpdateTransport();
        }

        public void ChangeCapacity(int capacity, List<int> dataCapacitys)
        {
            var dict = DataContainer.ChangeCapacity(capacity, dataCapacitys);
            ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(dict);
            (this as MissionNS.IMissionObj).UpdateTransport();
        }

        public void ChangeData(int index, string itemID)
        {
            string oldID = DataContainer.GetID(index);
            var t = DataContainer.ChangeData(index, itemID);
            ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(t.Item1, t.Item2);
            (this as MissionNS.IMissionObj).UpdateTransport(oldID);
        }

        public void Remove(int index, int amount)
        {
            if (amount > 0 && DataContainer.GetAmount(index, DataOpType.Storage) >= amount)
            {
                amount = DataContainer.ChangeAmount(index, amount, DataOpType.Empty, DataOpType.Storage);
                ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(DataContainer.GetID(index), amount);
            }
        }

        public void FastAdd(int index)
        {
            if (DataContainer.HaveSetData(index))
            {
                string itemID = DataContainer.GetID(index);
                int empty = DataContainer.GetAmount(index, DataOpType.Empty);
                int amount = ManagerNS.LocalGameManager.Instance.Player.InventoryCostItems(itemID, empty, false);
                DataContainer.ChangeAmount(index, amount, DataOpType.Storage, DataOpType.Empty);
            }
        }
        #endregion

        #region IMission
        public abstract Transform GetTransform();
        public abstract string GetUID();
        public abstract MissionNS.MissionObjType GetMissionObjType();
        public MissionNS.MissionObjType MissionObjType { get; set; }
        public List<MissionNS.Transport> Transports { get; set; } = new List<MissionNS.Transport>();
        public List<MissionNS.MissionTransport> Missions { get; set; } = new List<MissionNS.MissionTransport>();
        public MissionNS.TransportPriority TransportPriority { get; set; } = MissionNS.TransportPriority.Normal;

        public int ChangeAmount(string id, int amount, DataOpType addType, DataOpType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false)
        {
            return DataContainer.ChangeAmount(id, amount, addType, removeType, exceed, complete, needCanIn, needCanOut);
        }
        public int GetAmount(string itemID, DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            return DataContainer.GetAmount(itemID, type, needCanIn, needCanOut);
        }
        public Dictionary<string, int> GetAmount(DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            return DataContainer.GetAmount(type, needCanIn, needCanOut);
        }
        #endregion

        #region IInventory
        public bool AddItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                return DataContainer.ChangeAmount(item.ID, item.Amount, DataOpType.Storage, DataOpType.Empty) == item.Amount;
            }
            return false;
        }

        public bool RemoveItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                return DataContainer.ChangeAmount(item.ID, item.Amount, DataOpType.Empty, DataOpType.Storage) == item.Amount;
            }
            return false;
        }

        public ML.Engine.InventorySystem.Item RemoveItem(ML.Engine.InventorySystem.Item item, int amount)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && amount > 0)
            {
                ML.Engine.InventorySystem.Item result = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItem(item.ID);
                result.Amount = DataContainer.ChangeAmount(item.ID, item.Amount, DataOpType.Empty, DataOpType.Storage, complete: false);
                return result;
            }
            return null;
        }

        public bool RemoveItem(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && amount > 0)
            {
                return DataContainer.ChangeAmount(itemID, amount, DataOpType.Empty, DataOpType.Storage) == amount;
            }
            return false;
        }

        public int GetItemAllNum(string id)
        {
            return DataContainer.GetAmount(id, DataOpType.Storage);
        }

        public ML.Engine.InventorySystem.Item[] GetItemList()
        {
            var result = new List<ML.Engine.InventorySystem.Item>();
            foreach (var kv in DataContainer.GetAmount(DataOpType.Storage))
            {
                result.AddRange(ManagerNS.LocalGameManager.Instance.ItemManager.SpawnItems(kv.Key, kv.Value));
            }
            return result.ToArray();
        }
        #endregion
    }
}
