using System;
using System.Collections.Generic;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ProjectOC.MissionNS;
using ProjectOC.Player;
using Sirenix.OdinInspector;
using UnityEngine;


namespace ProjectOC.StoreNS
{
    [LabelText("�ֿ�"), Serializable]
    public class Store: IMissionObj, IInventory
    {
        #region ��ǰ����
        [LabelText("����ֿ�"), ReadOnly]
        public WorldStore WorldStore;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldStore?.InstanceID ?? ""; } }
        [LabelText("�ֿ�����"), ReadOnly]
        public string Name = "";
        [LabelText("�ֿ�����"), ReadOnly]
        public StoreType StoreType;
        [LabelText("�ֿ�洢����"), ReadOnly]
        public List<StoreData> StoreDatas = new List<StoreData>();
        [LabelText("�ֿ��Ӧ�İ���"), ReadOnly]
        public List<Transport> Transports = new List<Transport>();
        [LabelText("�ֿ�ȼ�"), ReadOnly]
        public int Level;
        [LabelText("�������ȼ�"), ReadOnly]
        public TransportPriority TransportPriority = TransportPriority.Normal;
        [LabelText("����Ƿ�������˲ֿ⽻��"), ReadOnly]
        public bool IsInteracting;
        [LabelText("����Icon��Ӧ��Item"), ReadOnly]
        public string WorldIconItemID;
        #endregion

        #region ����
        [LabelText("�ֿ����ȼ�"), FoldoutGroup("����")]
        public int LevelMax = 2;
        [LabelText("ÿ������Ĳֿ�����"), FoldoutGroup("����")]
        public List<int> LevelStoreCapacity = new List<int>() { 2, 4, 8 };
        [LabelText("ÿ������Ĳֿ���������"), FoldoutGroup("����")]
        public List<int> LevelStoreDataCapacity = new List<int>() { 50, 100, 200 };
        #endregion

        #region Property
        /// <summary>
        /// �ֿ�����
        /// </summary>
        public int StoreCapacity { get { return this.LevelStoreCapacity[this.Level]; } }
        /// <summary>
        /// �ֿ����ݵ�����
        /// </summary>
        public int StoreDataCapacity { get { return this.LevelStoreDataCapacity[this.Level]; } }
        #endregion

        public event Action OnStoreDataChangeAction;

        public Store(StoreType storeType)
        {
            for (int i = 0; i < this.StoreCapacity; i++)
            {
                this.StoreDatas.Add(new StoreData("", this.StoreDataCapacity));
            }
            this.StoreType = storeType;
            this.Name = storeType.ToString();
        }

        public void Destroy()
        {
            List<Transport> transports = new List<Transport>();
            transports.AddRange(Transports);
            foreach (Transport transport in transports)
            {
                if (transport.Target == this || !transport.ArriveSource)
                {
                    transport?.End();
                }
            }
            this.Transports.Clear();
            // ���ѷŵĳ�Ʒ���زģ�ȫ����������ұ���
            List<Item> items = new List<Item>();
            foreach (StoreData data in StoreDatas)
            {
                if (ItemManager.Instance.IsValidItemID(data.ItemID) && data.StorageAll > 0)
                {
                    items.AddRange(ItemManager.Instance.SpawnItems(data.ItemID, data.StorageAll));
                }
            }
            items = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory.AddItem(items);
            foreach (Item item in items)
            {
                #pragma warning disable CS4014
                ItemManager.Instance.SpawnWorldItem(item, WorldStore.transform.position, WorldStore.transform.rotation);
                #pragma warning restore CS4014
            }
        }

        public void OnPositionChange()
        {
            foreach (var transport in Transports)
            {
                transport?.UpdateDestination();
            }
        }

        /// <summary>
        /// �޸ĵȼ�
        /// </summary>
        public bool SetLevel(int newLevel)
        {
            if (WorldStore.transform != null && newLevel > this.Level && newLevel <= LevelMax)
            {
                int newStoreCapacity = LevelStoreCapacity[newLevel];
                int newStoreDataCapacity = LevelStoreDataCapacity[newLevel];
                for (int i = 0; i < newStoreCapacity - StoreCapacity; i++)
                {
                    this.StoreDatas.Add(new StoreData("", newStoreDataCapacity));
                }
                foreach (StoreData storeData in StoreDatas)
                {
                    storeData.MaxCapacity = newStoreDataCapacity;
                }
                Level = newLevel;
                return true;
            }
            return false;
        }

        /// <summary>
        /// �޸Ĳֿ�洢����Ʒ
        /// </summary>
        /// <param name="index">�ڼ����洢����</param>
        /// <param name="itemID">�µ���ƷID</param>
        public bool ChangeStoreData(int index, string itemID)
        {
            if (itemID == null)
            {
                itemID = "";
            }
            if (0 <= index && index < this.StoreCapacity)
            {
                StoreData data = this.StoreDatas[index];

                int storageReserve = GetDataNum(data.ItemID, DataType.StorageReserve) - data.StorageReserve;
                int emptyReserve = GetDataNum(data.ItemID, DataType.EmptyReserve) - data.EmptyReserve;
                foreach (Transport transport in Transports)
                {
                    if (transport != null && transport.ItemID == data.ItemID)
                    {
                        if (transport.Source == this && !transport.ArriveSource)
                        {
                            if (storageReserve <= 0)
                            {
                                transport.End();
                            }
                            else
                            {
                                storageReserve -= transport.SoureceReserveNum;
                            }
                        }
                        else if (transport.Target == this && emptyReserve == 0)
                        {
                            transport.End();
                        }
                    }
                }

                // ���ѷŵ���Ʒ��ȫ����������ұ���
                List<Item> items = new List<Item>();
                if (ItemManager.Instance.IsValidItemID(data.ItemID) && data.Storage > 0)
                {
                    items.AddRange(ItemManager.Instance.SpawnItems(data.ItemID, data.Storage));
                }
                items = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory.AddItem(items);
                if (items != null)
                {
                    foreach (Item item in items)
                    {
#pragma warning disable CS4014
                        ItemManager.Instance.SpawnWorldItem(item, WorldStore.transform.position, WorldStore.transform.rotation);
#pragma warning restore CS4014
                    }
                }

                data.Clear();
                data.ItemID = itemID;

                OnStoreDataChangeAction?.Invoke();
                return true;
            }
            return false;
        }


        #region ���ݷ���
        /// <summary>
        /// �ֿ��Ƿ��и���Ʒ
        /// </summary>
        public bool IsStoreHaveItem(string itemID, bool needCanIn = false, bool needCanOut = false)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in StoreDatas)
                {
                    if (data.ItemID == itemID && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public enum DataType
        {
            Storage,
            StorageReserve,
            EmptyReserve,
            Empty
        }
        public int GetDataNum(string itemID, DataType dataType, bool needCanIn = false, bool needCanOut = false)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in StoreDatas)
                {
                    if (data.ItemID == itemID && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                    {
                        result += data.GetNum(dataType);
                    }
                }
            }
            return result;
        }
        public bool CheckCanChangeData(DataType addType, DataType removeType, bool exceed=false)
        {
            if (exceed && (removeType != DataType.Empty || removeType != DataType.EmptyReserve))
            {
                return false;
            }
            switch (addType) 
            {
                case DataType.Storage:
                    return removeType == DataType.Empty || removeType == DataType.StorageReserve || removeType == DataType.EmptyReserve;
                case DataType.StorageReserve: 
                    return removeType == DataType.Storage;
                case DataType.EmptyReserve:
                    return removeType == DataType.Empty;
                case DataType.Empty:
                    return removeType == DataType.Storage || removeType == DataType.StorageReserve || removeType == DataType.EmptyReserve;
                default:
                    return false;
            }
        }
        /// <summary>
        /// �����޸ĳɹ�������
        /// </summary>
        public int ChangeData(string itemID, int amount, DataType addType, DataType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(itemID) && amount > 0 && CheckCanChangeData(addType, removeType, exceed))
                {
                    int removeNum = GetDataNum(itemID, removeType, needCanIn, needCanOut);
                    if (!exceed && removeNum == 0 && (!complete || removeNum <amount))
                    {
                        return 0;
                    }
                    int num = amount;
                    StoreData temp = null;
                    foreach (StoreData data in this.StoreDatas)
                    {
                        if (data.ItemID == itemID && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                        {
                            if (temp == null)
                            {
                                temp = data;
                            }
                            int cur = data.GetNum(removeType);
                            cur = cur <= num ? cur : num;
                            data.ChangeData(addType, cur);
                            data.ChangeData(removeType, -cur);
                            num -= cur;
                            if (num <= 0)
                            {
                                break;
                            }
                        }
                    }
                    if (exceed && num > 0 && temp != null)
                    {
                        temp.ChangeData(addType, num);
                        temp.ChangeData(removeType, -num);
                        num = 0;
                    }
                    OnStoreDataChangeAction?.Invoke();
                    return amount - num;
                }
                return 0;
            }
        }
        #endregion

        public class Sort : IComparer<Store>
        {
            public int Compare(Store x, Store y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (y == null)
                {
                    return -1;
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
        public void UIRemove(StoreData storeData, int amount)
        {
            lock (this)
            {
                if (storeData != null && amount > 0)
                {
                    if (ItemManager.Instance.IsValidItemID(storeData.ItemID) && storeData.Storage >= amount)
                    {
                        List<Item> items = ItemManager.Instance.SpawnItems(storeData.ItemID, amount);
                        var inventory = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory;
                        foreach (Item item in items)
                        {
                            int itemAmount = item.Amount;
                            if (inventory.AddItem(item))
                            {
                                storeData.ChangeData(DataType.Storage, -itemAmount);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        public void UIFastAdd(StoreData storeData)
        {
            lock (this)
            {
                if (storeData != null)
                {
                    if (ItemManager.Instance.IsValidItemID(storeData.ItemID))
                    {
                        var inventory = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory;
                        int amount = inventory.GetItemAllNum(storeData.ItemID);
                        amount = amount >= storeData.Empty ? storeData.Empty : amount;
                        if (inventory.RemoveItem(storeData.ItemID, amount))
                        {
                            storeData.ChangeData(DataType.Storage, amount);
                        }
                    }
                }
            }
        }
        public bool UIChangeStoreData(int index, string itemID)
        {
            return ChangeStoreData(index, itemID);
        }
        #endregion

        #region IMission�ӿ�
        public Transform GetTransform()
        {
            return WorldStore?.transform;
        }
        public TransportPriority GetTransportPriority()
        {
            return this.TransportPriority;
        }
        public string GetUID()
        {
            return this.UID;
        }
        public void AddTransport(Transport transport)
        {
            this.Transports.Add(transport);
        }
        public void RemoveTranport(Transport transport)
        {
            this.Transports.Remove(transport);
        }
        public bool PutIn(string itemID, int amount)
        {
            return ChangeData(itemID, amount, DataType.Storage, DataType.EmptyReserve, exceed:true) >= amount;
        }
        public int PutOut(string itemID, int amount)
        {
            return ChangeData(itemID, amount, DataType.Empty, DataType.StorageReserve, complete:false);
        }
        public int ReservePutIn(string itemID, int amount, Transport transport)
        {
            transport.TargetReserveNum = ChangeData(itemID, amount, DataType.EmptyReserve, DataType.Empty, exceed:true, needCanIn:true);
            return transport.TargetReserveNum;
        }
        public int ReservePutOut(string itemID, int amount, Transport transport)
        {
            transport.SoureceReserveNum = ChangeData(itemID, amount, DataType.StorageReserve, DataType.Storage, complete:false, needCanOut: true);
            return transport.SoureceReserveNum;
        }
        public int RemoveReservePutIn(string itemID, int amount)
        {
            return ChangeData(itemID, amount, DataType.Empty, DataType.EmptyReserve);
        }
        public int RemoveReservePutOut(string itemID, int amount)
        {
            return ChangeData(itemID, amount, DataType.Storage, DataType.StorageReserve);
        }
        #endregion

        #region IInventory�ӿ�
        public bool AddItem(Item item)
        {
            if (item != null)
            {
                return ChangeData(item.ID, item.Amount, DataType.Storage, DataType.Empty) >= item.Amount;
            }
            return false;
        }

        public bool RemoveItem(Item item)
        {
            if (item != null)
            {
                return ChangeData(item.ID, item.Amount, DataType.Empty, DataType.Storage) >= item.Amount;
            }
            return false;
        }

        public Item RemoveItem(Item item, int amount)
        {
            if (item != null)
            {
                Item result = ItemManager.Instance.SpawnItem(item.ID);
                result.Amount = ChangeData(item.ID, item.Amount, DataType.Empty, DataType.Storage, complete : false);
                return result;
            }
            return null;
        }

        public bool RemoveItem(string itemID, int amount)
        {
            return ChangeData(itemID, amount, DataType.Empty, DataType.Storage) >= amount;
        }

        public int GetItemAllNum(string id)
        {
            return GetDataNum(id, DataType.Storage);
        }

        public Item[] GetItemList()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}