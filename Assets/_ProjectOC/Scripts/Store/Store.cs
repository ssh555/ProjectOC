using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    [LabelText("�ֿ�"), Serializable]
    public class Store: MissionNS.IMissionObj, ML.Engine.InventorySystem.IInventory
    {
        #region Data
        [LabelText("����ֿ�"), ReadOnly]
        public WorldStore WorldStore;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldStore?.InstanceID ?? ""; } }
        [LabelText("�ֿ�����"), ReadOnly]
        public string Name = "";
        [LabelText("�ֿ�����"), ReadOnly]
        public ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 StoreType;
        [LabelText("�ֿ�洢����"), ReadOnly]
        public DataNS.DataContainer StoreDatas;
        [LabelText("�ֿ�ȼ�"), ReadOnly]
        public int Level;
        [LabelText("����Ƿ�������˲ֿ⽻��"), ReadOnly]
        public bool IsInteracting;
        [LabelText("����Icon��Ӧ��Item"), ReadOnly]
        public string WorldIconItemID;
        #endregion

        #region ����
        [LabelText("�ֿ����ȼ�"), FoldoutGroup("����")]
        public int LevelMax = 2;
        [LabelText("ÿ������Ĳֿ�����"), FoldoutGroup("����")]
        public List<int> LevelCapacity = new List<int>() { 2, 4, 8 };
        [LabelText("ÿ������Ĳֿ���������"), FoldoutGroup("����")]
        public List<int> LevelDataCapacity = new List<int>() { 50, 100, 200 };
        #endregion

        public Store(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType)
        {
            StoreDatas = new DataNS.DataContainer(LevelCapacity[Level], LevelDataCapacity[Level]);
            StoreType = storeType;
            Name = storeType.ToString();
        }

        public void Destroy()
        {
            foreach (MissionNS.Transport transport in Transports.ToArray())
            {
                if (transport.Target == this || !transport.ArriveSource)
                {
                    transport?.End();
                }
            }
            ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(StoreDatas.GetStorageAll());
        }

        public void OnPositionChange()
        {
            foreach (var transport in Transports.ToArray())
            {
                transport?.UpdateDestination();
            }
        }

        public bool SetLevel(int newLevel)
        {
            if (WorldStore.transform != null && 0 <= newLevel && newLevel <= LevelMax)
            {
                StoreDatas.ChangeCapacity(LevelCapacity[newLevel], LevelDataCapacity[newLevel]);
                Level = newLevel;
                UpdateTransport();
                return true;
            }
            return false;
        }

        public void UpdateTransport()
        {
            Dictionary<string, int> storageReserve = StoreDatas.GetDataAmount(DataNS.DataOpType.StorageReserve);
            Dictionary<string, int> emptyReserve = StoreDatas.GetDataAmount(DataNS.DataOpType.EmptyReserve);
            foreach (MissionNS.Transport transport in Transports.ToArray())
            {
                if (transport != null && storageReserve.ContainsKey(transport.ItemID))
                {
                    if (transport.Source == this && !transport.ArriveSource)
                    {
                        if (storageReserve[transport.ItemID] <= 0)
                        {
                            transport.End();
                        }
                        else
                        {
                            storageReserve[transport.ItemID] -= transport.SoureceReserveNum;
                        }
                    }
                    else if (transport.Target == this && emptyReserve[transport.ItemID] == 0)
                    {
                        transport.End();
                    }
                }
                else
                {
                    transport?.End();
                }
            }
        }

        public bool HaveItem(string itemID, bool needCanIn = false, bool needCanOut = false)
        {
            return StoreDatas.HaveSetData(itemID, needCanIn, needCanOut);
        }
        public int GetAmount(string itemID, DataNS.DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            return StoreDatas.GetDataAmount(itemID, type, needCanIn, needCanOut);
        }
        public void ChangeStoreData(int index, string itemID)
        {
            var t = StoreDatas.ChangeData(index, itemID);
            UpdateTransport();
            ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(t.Item1, t.Item2);
        }

        public class Sort : IComparer<Store>
        {
            public int Compare(Store x, Store y)
            {
                if (x == null || y == null)
                {
                    return (x == null).CompareTo((y==null));
                }
                int priorityX = (int)x.TransportPriority;
                int priorityY = (int)y.TransportPriority;
                if (priorityX != priorityY)
                {
                    return priorityX.CompareTo(priorityY);
                }
                return x.UID.CompareTo(y.UID);
            }
        }

        #region UI�ӿ�
        public void UIRemove(int index, int amount)
        {
            lock (this)
            {
                if (amount > 0 && StoreDatas.GetDataAmount(index, DataNS.DataOpType.Storage) >= amount)
                {
                    ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(StoreDatas.GetDataID(index), amount);
                    StoreDatas.ChangeAmount(index, amount, DataNS.DataOpType.Empty, DataNS.DataOpType.Storage);
                }
            }
        }
        public void UIFastAdd(int index)
        {
            lock (this)
            {
                if (StoreDatas.HaveSetData(index))
                {
                    var inventory = ManagerNS.LocalGameManager.Instance.Player.GetInventory();
                    string itemID = StoreDatas.GetDataID(index);
                    int amount = inventory.GetItemAllNum(itemID);
                    int empty = StoreDatas.GetDataAmount(index, DataNS.DataOpType.Empty);
                    amount = amount <= empty ? amount : empty;
                    if (inventory.RemoveItem(itemID, amount))
                    {
                        StoreDatas.ChangeAmount(index, amount, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty);
                    }
                }
            }
        }
        #endregion

        #region IMission�ӿ�
        [LabelText("�ֿ��Ӧ�İ���"), ReadOnly]
        public List<MissionNS.Transport> Transports = new List<MissionNS.Transport>();
        [LabelText("�������ȼ�"), ReadOnly]
        public MissionNS.TransportPriority TransportPriority = MissionNS.TransportPriority.Normal;
        public Transform GetTransform() { return WorldStore?.transform; }
        public MissionNS.TransportPriority GetTransportPriority() { return TransportPriority; }
        public string GetUID() { return UID; }
        public void AddTransport(MissionNS.Transport transport) { Transports.Add(transport); }
        public void RemoveTranport(MissionNS.Transport transport) { Transports.Remove(transport); }
        public bool PutIn(string itemID, int amount)
        {
            return StoreDatas.ChangeAmount(itemID, amount, DataNS.DataOpType.Storage, DataNS.DataOpType.EmptyReserve, exceed:true) == amount;
        }
        public int PutOut(string itemID, int amount)
        {
            return StoreDatas.ChangeAmount(itemID, amount, DataNS.DataOpType.Empty, DataNS.DataOpType.StorageReserve, complete:false);
        }
        public int ReservePutIn(string itemID, int amount, MissionNS.Transport transport)
        {
            transport.TargetReserveNum = StoreDatas.ChangeAmount(itemID, amount, DataNS.DataOpType.EmptyReserve, DataNS.DataOpType.Empty, exceed:true, needCanIn:true);
            return transport.TargetReserveNum;
        }
        public int ReservePutOut(string itemID, int amount, MissionNS.Transport transport)
        {
            transport.SoureceReserveNum = StoreDatas.ChangeAmount(itemID, amount, DataNS.DataOpType.StorageReserve, DataNS.DataOpType.Storage, complete:false, needCanOut: true);
            return transport.SoureceReserveNum;
        }
        public int RemoveReservePutIn(string itemID, int amount)
        {
            return StoreDatas.ChangeAmount(itemID, amount, DataNS.DataOpType.Empty, DataNS.DataOpType.EmptyReserve);
        }
        public int RemoveReservePutOut(string itemID, int amount)
        {
            return StoreDatas.ChangeAmount(itemID, amount, DataNS.DataOpType.Storage, DataNS.DataOpType.StorageReserve);
        }
        #endregion

        #region IInventory�ӿ�
        public bool AddItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                return StoreDatas.ChangeAmount(item.ID, item.Amount, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty) >= item.Amount;
            }
            return false;
        }

        public bool RemoveItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                return StoreDatas.ChangeAmount(item.ID, item.Amount, DataNS.DataOpType.Empty, DataNS.DataOpType.Storage) >= item.Amount;
            }
            return false;
        }

        public ML.Engine.InventorySystem.Item RemoveItem(ML.Engine.InventorySystem.Item item, int amount)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && amount > 0)
            {
                ML.Engine.InventorySystem.Item result = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItem(item.ID);
                result.Amount = StoreDatas.ChangeAmount(item.ID, item.Amount, DataNS.DataOpType.Empty, DataNS.DataOpType.Storage, complete : false);
                return result;
            }
            return null;
        }

        public bool RemoveItem(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && amount > 0)
            {
                return StoreDatas.ChangeAmount(itemID, amount, DataNS.DataOpType.Empty, DataNS.DataOpType.Storage) >= amount;
            }
            return false;
        }

        public int GetItemAllNum(string id)
        {
            return GetAmount(id, DataNS.DataOpType.Storage);
        }

        public ML.Engine.InventorySystem.Item[] GetItemList()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}