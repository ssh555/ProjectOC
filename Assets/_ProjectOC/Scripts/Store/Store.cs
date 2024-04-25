using System;
using System.Collections.Generic;
using ML.Engine.InventorySystem;
using Sirenix.OdinInspector;
using UnityEngine;


namespace ProjectOC.StoreNS
{
    [LabelText("�ֿ�"), Serializable]
    public class Store: MissionNS.IMissionObj, IInventory
    {
        #region ��ǰ����
        [LabelText("����ֿ�"), ReadOnly]
        public WorldStore WorldStore;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldStore?.InstanceID ?? ""; } }
        [LabelText("�ֿ�����"), ReadOnly]
        public string Name = "";
        [LabelText("�ֿ�����"), ReadOnly]
        public ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 StoreType;
        [LabelText("�ֿ�洢����"), ReadOnly]
        public StoreData[] StoreDatas;
        [LabelText("�ֿ��Ӧ�İ���"), ReadOnly]
        public List<MissionNS.Transport> Transports = new List<MissionNS.Transport>();
        [LabelText("�ֿ�ȼ�"), ReadOnly]
        public int Level;
        [LabelText("�������ȼ�"), ReadOnly]
        public MissionNS.TransportPriority TransportPriority = MissionNS.TransportPriority.Normal;
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
        [LabelText("�ֿ�����"), ShowInInspector, ReadOnly]
        public int StoreCapacity => Level < LevelStoreCapacity.Count ? LevelStoreCapacity[Level] : 0;
        [LabelText("�ֿ����ݵ�����"), ShowInInspector, ReadOnly]
        public int StoreDataCapacity => Level<LevelStoreDataCapacity.Count? LevelStoreDataCapacity[Level] : 0;
        #endregion

        public event Action OnDataChangeEvent;

        public Store(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType)
        {
            int capacity = StoreCapacity;
            int dataCapacity = StoreDataCapacity;
            StoreDatas = new StoreData[capacity];
            for (int i = 0; i < capacity; i++)
            {
                StoreDatas[i] = new StoreData("", dataCapacity);
            }
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
            // ���ѷŵĳ�Ʒ���زģ�ȫ����������ұ���
            List<Item> items = new List<Item>();
            foreach (StoreData data in StoreDatas)
            {
                if (ItemManager.Instance.IsValidItemID(data.ItemID) && data.StorageAll > 0)
                {
                    items.AddRange(ItemManager.Instance.SpawnItems(data.ItemID, data.StorageAll));
                }
            }
            (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(items);
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
                int newCapacity = LevelStoreCapacity[newLevel];
                int newDataCapacity = LevelStoreDataCapacity[newLevel];
                StoreData[] newStoreDatas = new StoreData[newCapacity];
                for (int i = 0; i < StoreDatas.Length; i++)
                {
                    newStoreDatas[i] = StoreDatas[i];
                }
                StoreDatas = newStoreDatas;
                for (int i = 0; i < StoreDatas.Length; i++)
                {
                    StoreDatas[i].MaxCapacity = newDataCapacity;
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
            itemID = itemID ?? "";
            if (0 <= index && index < StoreCapacity)
            {
                if (StoreDatas[index].HaveItem())
                {
                    int storageReserve = GetDataNum(StoreDatas[index].ItemID, DataType.StorageReserve) - StoreDatas[index].StorageReserve;
                    int emptyReserve = GetDataNum(StoreDatas[index].ItemID, DataType.EmptyReserve) - StoreDatas[index].EmptyReserve;
                    foreach (MissionNS.Transport transport in Transports)
                    {
                        if (transport != null && transport.ItemID == StoreDatas[index].ItemID)
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
                    if (StoreDatas[index].Storage > 0)
                    {
                        items.AddRange(ItemManager.Instance.SpawnItems(StoreDatas[index].ItemID, StoreDatas[index].Storage));
                    }
                    (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(items);
                }

                StoreDatas[index].ClearData();
                StoreDatas[index].ItemID = itemID;
                OnDataChangeEvent?.Invoke();
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
                    StoreData temp = default(StoreData);
                    foreach (StoreData data in this.StoreDatas)
                    {
                        if (data.ItemID == itemID && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                        {
                            if (!temp.HaveItem())
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
                    if (exceed && num > 0 && temp.HaveItem())
                    {
                        temp.ChangeData(addType, num);
                        temp.ChangeData(removeType, -num);
                        num = 0;
                    }
                    OnDataChangeEvent?.Invoke();
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
                if (0 <= index && index < StoreDatas.Length && StoreDatas[index].HaveItem() && amount > 0 && StoreDatas[index].Storage >= amount)
                {
                    List<Item> items = ItemManager.Instance.SpawnItems(StoreDatas[index].ItemID, amount);
                    (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(items);
                    StoreDatas[index].ChangeData(DataType.Storage, -amount);
                }
            }
        }
        public void UIFastAdd(int index)
        {
            lock (this)
            {
                if (0 <= index && index < StoreDatas.Length && StoreDatas[index].HaveItem())
                {
                    var inventory = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory;
                    int amount = inventory.GetItemAllNum(StoreDatas[index].ItemID);
                    amount = amount <= StoreDatas[index].Empty ? amount : StoreDatas[index].Empty;
                    if (inventory.RemoveItem(StoreDatas[index].ItemID, amount))
                    {
                        StoreDatas[index].ChangeData(DataType.Storage, amount);
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
        public Transform GetTransform() { return WorldStore?.transform; }
        public MissionNS.TransportPriority GetTransportPriority() { return TransportPriority; }
        public string GetUID() { return UID; }
        public void AddTransport(MissionNS.Transport transport) { Transports.Add(transport); }
        public void RemoveTranport(MissionNS.Transport transport) { Transports.Remove(transport); }
        public bool PutIn(string itemID, int amount)
        {
            return ChangeData(itemID, amount, DataType.Storage, DataType.EmptyReserve, exceed:true) >= amount;
        }
        public int PutOut(string itemID, int amount)
        {
            return ChangeData(itemID, amount, DataType.Empty, DataType.StorageReserve, complete:false);
        }
        public int ReservePutIn(string itemID, int amount, MissionNS.Transport transport)
        {
            transport.TargetReserveNum = ChangeData(itemID, amount, DataType.EmptyReserve, DataType.Empty, exceed:true, needCanIn:true);
            return transport.TargetReserveNum;
        }
        public int ReservePutOut(string itemID, int amount, MissionNS.Transport transport)
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