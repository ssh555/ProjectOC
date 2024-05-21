using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.DataNS
{
    public abstract class UniqueItemContainerOwner<T> : IContainerOwner<T> where T : ML.Engine.InventorySystem.Item
    {
        [LabelText("´æ´¢Êý¾Ý"), ReadOnly]
        public DataContainer<T> DataContainer { get; set; }

        #region Init Clear
        public void ClearData()
        {
            (this as MissionNS.IMissionObj).Clear();
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
        public void ChangeCapacity(int capacity)
        {
            var dict = DataContainer.ChangeCapacity(capacity, 1);
            foreach (var kv in dict)
            {
                if (kv.Item2 > 0) { ManagerNS.LocalGameManager.Instance.Player.GetInventory().AddItem(kv.Item1); }
            }
            (this as MissionNS.IMissionObj).UpdateTransport();
        }
        public void ChangeData(int index, T data)
        {
            var t = DataContainer.ChangeData(index, data);
            if (t.Item2 > 0) { ManagerNS.LocalGameManager.Instance.Player.GetInventory().AddItem(t.Item1); }
            (this as MissionNS.IMissionObj).UpdateTransport(t.Item1);
        }
        #endregion

        #region IMission
        public abstract Transform GetTransform();
        public abstract string GetUID();
        public abstract MissionNS.MissionObjType GetMissionObjType();
        public List<MissionNS.Transport> Transports { get; set; } = new List<MissionNS.Transport>();
        public List<MissionNS.MissionTransport> Missions { get; set; } = new List<MissionNS.MissionTransport>();
        public MissionNS.TransportPriority TransportPriority { get; set; } = MissionNS.TransportPriority.Normal;

        public int ChangeAmount(string id, int amount, DataOpType addType, DataOpType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false) { return 0; }
        public int GetAmount(string itemID, DataOpType type, bool needCanIn = false, bool needCanOut = false) { return 0; }
        public Dictionary<string, int> GetAmount(DataOpType type, bool needCanIn = false, bool needCanOut = false) { return new Dictionary<string, int>(); }
        public int ChangeAmount(ML.Engine.InventorySystem.Item item, DataOpType addType, DataOpType removeType, bool needCanIn = false, bool needCanOut = false) 
        {
            return item is T t ? DataContainer.ChangeAmount(t, 1, addType, removeType, needCanIn, needCanOut) : 0;
        }
        public int GetAmount(ML.Engine.InventorySystem.Item item, DataOpType type, bool needCanIn = false, bool needCanOut = false) 
        {
            return item is T t ? DataContainer.GetAmount(t, type, needCanIn, needCanOut) : 0;
        }
        public HashSet<ML.Engine.InventorySystem.Item> GetAmountForItem(DataOpType type, bool needCanIn = false, bool needCanOut = false) 
        {
            HashSet<ML.Engine.InventorySystem.Item> result = new HashSet<ML.Engine.InventorySystem.Item>();
            foreach (var kv in DataContainer.GetAmount(type, needCanIn, needCanOut))
            {
                if (kv.Item2 > 0) { result.Add(kv.Item1); }
            }
            return result;
        }
        #endregion

        #region IInventory
        public bool AddItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item is T t)
            {
                return DataContainer.ChangeAmount(t, 1, DataOpType.Storage, DataOpType.Empty) == item.Amount;
            }
            return false;
        }
        public bool RemoveItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item is T t)
            {
                return DataContainer.ChangeAmount(t, 1, DataOpType.Empty, DataOpType.Storage) == item.Amount;
            }
            return false;
        }
        public ML.Engine.InventorySystem.Item RemoveItem(ML.Engine.InventorySystem.Item item, int amount) { return null; }
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