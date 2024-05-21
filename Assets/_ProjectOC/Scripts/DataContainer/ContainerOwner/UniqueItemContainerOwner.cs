using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.DataNS
{
    public abstract class UniqueItemContainerOwner<T> : IContainerOwner<T> where T : ML.Engine.InventorySystem.Item
    {
        public bool IsUnique => true;
        [LabelText("´æ´¢Êý¾Ý"), ReadOnly]
        public DataContainer<T> DataContainer { get; set; }

        #region Clear
        public void ClearData()
        {
            (this as MissionNS.IMissionObj<T>).Clear();
            if (DataContainer != null)
            {
                foreach (var kv in DataContainer.GetAmount(DataOpType.StorageAll))
                {
                    if (kv.Item2 > 0) { ManagerNS.LocalGameManager.Instance.Player.GetInventory().AddItem(kv.Item1); }
                }
            }
        }
        #endregion

        #region Set
        public Dictionary<T, int> ListToDict(List<(T, int)> list)
        {
            Dictionary<T, int> result = new Dictionary<T, int>();
            foreach (var t in list)
            {
                if (!result.ContainsKey(t.Item1) && t.Item2 == 1)
                {
                    result.Add(t.Item1, t.Item2);
                }
                else
                {
                    Debug.Log($"UniqueItemContainerOwner Error Is HashCode Error {!result.ContainsKey(t.Item1)}");
                }
            }
            return result;
        }
        public void ChangeCapacity(int capacity)
        {
            var dict = DataContainer.ChangeCapacity(capacity, 1);
            foreach (var kv in dict)
            {
                if (kv.Item2 > 0) { ManagerNS.LocalGameManager.Instance.Player.GetInventory().AddItem(kv.Item1); }
            }
            (this as MissionNS.IMissionObj<T>).UpdateTransport();
        }
        public void ChangeData(int index, T data)
        {
            var t = DataContainer.ChangeData(index, data);
            if (t.Item2 > 0) { ManagerNS.LocalGameManager.Instance.Player.GetInventory().AddItem(t.Item1); }
            (this as MissionNS.IMissionObj<T>).UpdateTransport(t.Item1);
        }
        #endregion

        #region IMission
        public abstract Transform GetTransform();
        public abstract string GetUID();
        public abstract MissionNS.MissionObjType GetMissionObjType();
        public List<MissionNS.Transport<T>> Transports { get; set; } = new List<MissionNS.Transport<T>>();
        public List<MissionNS.MissionTransport<T>> Missions { get; set; } = new List<MissionNS.MissionTransport<T>>();
        public MissionNS.TransportPriority TransportPriority { get; set; } = MissionNS.TransportPriority.Normal;

        public int ChangeAmount(T data, int amount, DataOpType addType, DataOpType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false)
        {
            if (amount != 1) { return 0; }
            return DataContainer.ChangeAmount(data, amount, addType, removeType, exceed, complete, needCanIn, needCanOut);
        }
        public int GetAmount(T data, DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            return DataContainer.GetAmount(data, type, needCanIn, needCanOut);
        }
        public Dictionary<T, int> GetAmount(DataOpType type, bool needCanIn, bool needCanOut)
        {
            return ListToDict(DataContainer.GetAmount(type, needCanIn, needCanOut));
        }
        #endregion

        #region IInventory
        public bool AddItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item is T data)
            {
                return DataContainer.ChangeAmount(data, 1, DataOpType.Storage, DataOpType.Empty) == item.Amount;
            }
            return false;
        }
        public bool RemoveItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount == 1 && item is T data)
            {
                return DataContainer.ChangeAmount(data, 1, DataOpType.Empty, DataOpType.Storage) == item.Amount;
            }
            return false;
        }
        public ML.Engine.InventorySystem.Item RemoveItem(ML.Engine.InventorySystem.Item item, int amount) 
        {
            if (amount == 1)
            {
                item.Amount = 1;
                RemoveItem(item);
            }
            return null; 
        }
        public bool RemoveItem(string itemID, int amount) { return false; }
        public int GetItemAllNum(string id) { return 0; }
        public ML.Engine.InventorySystem.Item[] GetItemList()
        {
            var result = new List<ML.Engine.InventorySystem.Item>();
            foreach (var kv in DataContainer.GetAmount(DataOpType.Storage))
            {
                if (kv.Item2 > 0) { result.Add(kv.Item1); }
            }
            return result.ToArray();
        }
        #endregion
    }
}