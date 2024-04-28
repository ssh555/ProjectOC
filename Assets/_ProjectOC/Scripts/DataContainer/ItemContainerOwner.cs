using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.DataNS
{
    public abstract class ItemContainerOwner : MissionNS.IMissionObj, ML.Engine.InventorySystem.IInventory
    {
        [LabelText("´æ´¢Êý¾Ý"), ReadOnly]
        public DataContainer ItemDatas;

        #region Init Clear
        public void InitData(int capacity, int dataCapacity)
        {
            ItemDatas = new DataContainer(capacity, dataCapacity);
        }
        public void InitData(List<string> ids, List<int> dataCapacitys)
        {
            ItemDatas = new DataContainer(ids, dataCapacitys);
        }
        public void ClearData()
        {
            (this as MissionNS.IMissionObj).Clear();
            if (ItemDatas != null)
            {
                ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(ItemDatas.GetStorageAll());
            }
        }
        public void ResetData(List<string> ids, List<int> dataCapacity)
        {
            ClearData();
            InitData(ids, dataCapacity);
        }
        #endregion

        #region Get
        public int GetCapacity()
        {
            return ItemDatas?.GetCapacity() ?? 0;
        }
        public bool HaveSetItem(string itemID, bool needCanIn = false, bool needCanOut = false)
        {
            return ItemDatas.HaveSetData(itemID, needCanIn, needCanOut);
        }
        public int GetAmount(string itemID, DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            return ItemDatas.GetAmount(itemID, type, needCanIn, needCanOut);
        }
        public int GetAmount(int index, DataOpType type)
        {
            return ItemDatas.GetAmount(index, type);
        }
        #endregion

        #region Set
        public void ChangeCapacity(int capacity, int dataCapacity)
        {
            var dict = ItemDatas.ChangeCapacity(capacity, dataCapacity);
            ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(dict);
            (this as MissionNS.IMissionObj).UpdateTransport();
        }

        public void ChangeCapacity(int capacity, List<int> dataCapacitys)
        {
            var dict = ItemDatas.ChangeCapacity(capacity, dataCapacitys);
            ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(dict);
            (this as MissionNS.IMissionObj).UpdateTransport();
        }

        public void ChangeData(int index, string itemID)
        {
            string oldID = ItemDatas.GetID(index);
            var t = ItemDatas.ChangeData(index, itemID);
            ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(t.Item1, t.Item2);
            (this as MissionNS.IMissionObj).UpdateTransport(oldID);
        }

        public void Remove(int index, int amount)
        {
            if (amount > 0 && ItemDatas.GetAmount(index, DataOpType.Storage) >= amount)
            {
                ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(ItemDatas.GetID(index), amount);
                ItemDatas.ChangeAmount(index, amount, DataOpType.Empty, DataOpType.Storage);
            }
        }

        public void FastAdd(int index)
        {
            if (ItemDatas.HaveSetData(index))
            {
                string itemID = ItemDatas.GetID(index);
                int empty = ItemDatas.GetAmount(index, DataOpType.Empty);
                int amount = ManagerNS.LocalGameManager.Instance.Player.InventoryCostItems(itemID, empty, false);
                ItemDatas.ChangeAmount(index, amount, DataOpType.Storage, DataOpType.Empty);
            }
        }
        #endregion

        #region IMission
        public abstract Transform GetTransform();
        public abstract string GetUID();
        public List<MissionNS.Transport> Transports { get; set; } = new List<MissionNS.Transport>();
        public List<MissionNS.MissionTransport> Missions { get; set; } = new List<MissionNS.MissionTransport>();
        public MissionNS.TransportPriority TransportPriority { get; set; } = MissionNS.TransportPriority.Normal;

        public bool PutIn(string itemID, int amount)
        {
            return ItemDatas.ChangeAmount(itemID, amount, DataOpType.Storage, DataOpType.EmptyReserve, exceed: true) == amount;
        }
        public int PutOut(string itemID, int amount)
        {
            return ItemDatas.ChangeAmount(itemID, amount, DataOpType.Empty, DataOpType.StorageReserve, complete: false);
        }
        public int ReservePutIn(string itemID, int amount)
        {
            return ItemDatas.ChangeAmount(itemID, amount, DataOpType.EmptyReserve, DataOpType.Empty, exceed: true, needCanIn: true);
        }
        public int ReservePutOut(string itemID, int amount)
        {
            return ItemDatas.ChangeAmount(itemID, amount, DataOpType.StorageReserve, DataOpType.Storage, complete: false, needCanOut: true);
        }
        public int RemoveReservePutIn(string itemID, int amount)
        {
            return ItemDatas.ChangeAmount(itemID, amount, DataOpType.Empty, DataOpType.EmptyReserve);
        }
        public int RemoveReservePutOut(string itemID, int amount)
        {
            return ItemDatas.ChangeAmount(itemID, amount, DataOpType.Storage, DataOpType.StorageReserve);
        }
        public int GetReservePutIn(string itemID) { return ItemDatas.GetAmount(itemID, DataOpType.EmptyReserve); }
        public int GetReservePutOut(string itemID) { return ItemDatas.GetAmount(itemID, DataOpType.StorageReserve); }
        public Dictionary<string, int> GetReservePutIn() { return ItemDatas.GetAmount(DataOpType.EmptyReserve); }
        public Dictionary<string, int> GetReservePutOut() { return ItemDatas.GetAmount(DataOpType.StorageReserve); }
        #endregion

        #region IInventory
        public bool AddItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                return ItemDatas.ChangeAmount(item.ID, item.Amount, DataOpType.Storage, DataOpType.Empty) == item.Amount;
            }
            return false;
        }

        public bool RemoveItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                return ItemDatas.ChangeAmount(item.ID, item.Amount, DataOpType.Empty, DataOpType.Storage) == item.Amount;
            }
            return false;
        }

        public ML.Engine.InventorySystem.Item RemoveItem(ML.Engine.InventorySystem.Item item, int amount)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && amount > 0)
            {
                ML.Engine.InventorySystem.Item result = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItem(item.ID);
                result.Amount = ItemDatas.ChangeAmount(item.ID, item.Amount, DataOpType.Empty, DataOpType.Storage, complete: false);
                return result;
            }
            return null;
        }

        public bool RemoveItem(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && amount > 0)
            {
                return ItemDatas.ChangeAmount(itemID, amount, DataOpType.Empty, DataOpType.Storage) == amount;
            }
            return false;
        }

        public int GetItemAllNum(string id)
        {
            return GetAmount(id, DataOpType.Storage);
        }

        public ML.Engine.InventorySystem.Item[] GetItemList()
        {
            var result = new List<ML.Engine.InventorySystem.Item>();
            foreach (var kv in ItemDatas.GetStorageAll())
            {
                result.AddRange(ManagerNS.LocalGameManager.Instance.ItemManager.SpawnItems(kv.Key, kv.Value));
            }
            return result.ToArray();
        }
        #endregion
    }
}
