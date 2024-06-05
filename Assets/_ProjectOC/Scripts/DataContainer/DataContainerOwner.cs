using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.DataNS
{
    public abstract class DataContainerOwner : MissionNS.IMissionObj, ML.Engine.InventorySystem.IInventory
    {
        [LabelText("Êý¾ÝÈÝÆ÷"), ReadOnly]
        public DataContainer DataContainer;
        public bool ChangeDataAutoSort;

        #region Init Clear Reset
        public void InitData(int capacity, int dataCapacity)
        {
            DataContainer = new DataContainer(capacity, dataCapacity);
        }
        public void InitData(int capacity, List<int> dataCapacitys)
        {
            DataContainer = new DataContainer(capacity, dataCapacitys);
        }
        public void ClearData(bool clearSize=true)
        {
            (this as MissionNS.IMissionObj).Clear();
            if (DataContainer != null)
            {
                var dict = DataContainer.GetAmount(DataOpType.StorageAll);
                foreach (var kv in dict)
                {
                    kv.Key.AddToPlayerInventory(kv.Value);
                }
            }
            if (clearSize)
            {
                DataContainer.Clear();
            }
            else
            {
                DataContainer.ClearData();
            }
        }
        public void ResetData(List<IDataObj> datas, List<int> dataCapacity)
        {
            ClearData();
            DataContainer.Reset(datas, dataCapacity);
        }
        #endregion

        #region Str
        private const string str = "";
        #endregion

        #region Set
        public void ChangeCapacity(int capacity, int dataCapacity)
        {
            var dict = DataContainer.ChangeCapacity(capacity, dataCapacity);
            foreach (var kv in dict)
            {
                kv.Key.AddToPlayerInventory(kv.Value);
            }
            (this as MissionNS.IMissionObj).UpdateTransport();
        }
        public void ChangeCapacity(int capacity, List<int> dataCapacitys)
        {
            var dict = DataContainer.ChangeCapacity(capacity, dataCapacitys);
            foreach (var kv in dict)
            {
                kv.Key.AddToPlayerInventory(kv.Value);
            }
            (this as MissionNS.IMissionObj).UpdateTransport();
        }
        public void ChangeData(int index, IDataObj data, bool isAdd = true, bool updateTransport=true)
        {
            var tup = DataContainer.ChangeData(index, data, ChangeDataAutoSort);
            if (tup.Item1 != null)
            {
                if (isAdd) { tup.Item1.AddToPlayerInventory(tup.Item2); }
                if (updateTransport) { (this as MissionNS.IMissionObj).UpdateTransport(tup.Item1); }
            }
        }
        public void Remove(int index, int amount)
        {
            if (amount > 0 && DataContainer.HaveSetData(index) && DataContainer.GetAmount(index, DataOpType.Storage) >= amount)
            {
                amount = DataContainer.ChangeAmount(index, amount, DataOpType.Empty, DataOpType.Storage);
                DataContainer.GetData(index).AddToPlayerInventory(amount);
            }
        }
        public void FastRemove(int index)
        {
            if (DataContainer.HaveSetData(index))
            {
                int storage = DataContainer.GetAmount(index, DataOpType.Storage);
                DataContainer.GetData(index).AddToPlayerInventory(storage);
                DataContainer.ChangeAmount(index, storage, DataOpType.Empty, DataOpType.Storage);
            }
        }
        public void FastAdd(int index)
        {
            if (DataContainer.HaveSetData(index))
            {
                int empty = DataContainer.GetAmount(index, DataOpType.Empty);
                int amount = DataContainer.GetData(index).RemoveFromPlayerInventory(empty);
                DataContainer.ChangeAmount(index, amount, DataOpType.Storage, DataOpType.Empty);
            }
        }
        #endregion

        #region IMission
        public abstract Transform GetTransform();
        public abstract string GetUID();
        public abstract MissionNS.MissionObjType GetMissionObjType();
        public List<MissionNS.Transport> Transports { get; set; } = new List<MissionNS.Transport>();
        public List<MissionNS.MissionTransport> Missions { get; set; } = new List<MissionNS.MissionTransport>();
        public MissionNS.TransportPriority TransportPriority { get; set; } = MissionNS.TransportPriority.Normal;
        public int GetReservePutIn(IDataObj data) { return DataContainer.GetAmount(data, DataOpType.EmptyReserve); }
        public int GetReservePutOut(IDataObj data) { return DataContainer.GetAmount(data, DataOpType.StorageReserve); }
        public Dictionary<IDataObj, int> GetReservePutIn() { return DataContainer.GetAmount(DataOpType.EmptyReserve); }
        public Dictionary<IDataObj, int> GetReservePutOut() { return DataContainer.GetAmount(DataOpType.StorageReserve); }
        public abstract void PutIn(int index, IDataObj data, int amount);
        public int ReservePutIn(IDataObj data, int amount, bool reserveEmpty = false)
        {
            if (reserveEmpty && DataContainer.AddDataToEmptyIndex(data, needCanIn: true, needSort: ChangeDataAutoSort) < 0) { return 0; }
            return DataContainer.ChangeAmount(data, amount, DataOpType.EmptyReserve, DataOpType.Empty, needCanIn: true);
        }
        public int RemoveReservePutIn(IDataObj data, int amount, bool removeEmpty = false)
        {
            amount = DataContainer.ChangeAmount(data, amount, DataOpType.Empty, DataOpType.EmptyReserve);
            if (removeEmpty) { DataContainer.RemoveDataWithEmptyIndex(data, ChangeDataAutoSort); }
            return amount;
        }
        public int PutOut(IDataObj data, int amount, bool removeEmpty = false)
        {
            amount = DataContainer.ChangeAmount(data, amount, DataOpType.Empty, DataOpType.StorageReserve, complete: false);
            if (removeEmpty) { DataContainer.RemoveDataWithEmptyIndex(data, ChangeDataAutoSort); }
            return amount;
        }
        public int ReservePutOut(IDataObj data, int amount, bool recplaceData = false, MissionNS.Transport transport = null)
        {
            if (recplaceData && transport != null)
            {
                Dictionary<IDataObj, int> result = DataContainer.ChangeAmountForUniqueData(data?.GetDataID() ?? str, amount,
                    DataOpType.StorageReserve, DataOpType.Storage, complete: false, needCanOut: true);
                int cnt = 0;
                if (result.Count > 0)
                {
                    foreach (var kv in result)
                    {
                        transport.Data = kv.Key;
                        cnt += kv.Value;
                    }
                }
                return cnt;
            }
            else
            {
                return DataContainer.ChangeAmount(data, amount, DataOpType.StorageReserve, DataOpType.Storage, complete: false, needCanOut: true);
            }
        }
        public bool PutIn(IDataObj data, int amount)
        {
            return DataContainer.ChangeAmount(data, amount, DataOpType.Storage, DataOpType.EmptyReserve, exceed: true) == amount;
        }
        public int RemoveReservePutOut(IDataObj data, int amount)
        {
            return DataContainer.ChangeAmount(data, amount, DataOpType.Storage, DataOpType.StorageReserve);
        }
        #endregion

        #region IInventory
        public bool AddItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                if (item is IDataObj dataObj)
                {
                    return DataContainer.ChangeAmount(dataObj, item.Amount, DataOpType.Storage, DataOpType.Empty, true) == item.Amount;
                }
                else
                {
                    return DataContainer.ChangeAmount(item.ID, item.Amount, DataOpType.Storage, DataOpType.Empty, true) == item.Amount;
                }
            }
            return false;
        }
        public bool RemoveItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                if (item is IDataObj dataObj)
                {
                    return DataContainer.ChangeAmount(dataObj, item.Amount, DataOpType.Empty, DataOpType.Storage) == item.Amount;
                }
                else
                {
                    return DataContainer.ChangeAmount(item.ID, item.Amount, DataOpType.Empty, DataOpType.Storage) == item.Amount;
                }
            }
            return false;
        }
        public ML.Engine.InventorySystem.Item RemoveItem(ML.Engine.InventorySystem.Item item, int amount)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && amount > 0)
            {
                ML.Engine.InventorySystem.Item result = (ML.Engine.InventorySystem.Item)item.Clone();
                if (item is IDataObj dataObj)
                {
                    result.Amount = DataContainer.ChangeAmount(dataObj, amount, DataOpType.Empty, DataOpType.Storage, complete: false);
                }
                else
                {
                    result.Amount = DataContainer.ChangeAmount(item.ID, amount, DataOpType.Empty, DataOpType.Storage, complete: false);
                }
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
                result.AddRange(kv.Key.ConvertToItem(kv.Value));
            }
            return result.ToArray();
        }
        #endregion
    }
}