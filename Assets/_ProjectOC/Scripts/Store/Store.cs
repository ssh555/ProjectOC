using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.InventorySystem;
using ProjectOC.MissionNS;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// �ֿ�
    /// </summary>
    [System.Serializable]
    public class Store: IMission, IInventory
    {
        public WorldStore WorldStore;
        public string UID { get { return WorldStore?.InstanceID ?? ""; } }
        [LabelText("�ֿ�����")]
        public string Name = "";
        [LabelText("�ֿ�����")]
        public StoreType StoreType;
        [LabelText("�ֿ�洢����")]
        public List<StoreData> StoreDatas = new List<StoreData>();
        [LabelText("�ֿ��Ӧ�İ���")]
        public List<Transport> Transports = new List<Transport>();

        /// <summary>
        /// �ֿ��������ֿ��ܷŶ�������Ʒ
        /// </summary>
        public int StoreCapacity
        {
            get
            {
                return this.LevelStoreCapacity[this.Level];
            }
        }
        /// <summary>
        /// �ֿ����ݵ�������������Ʒ�����洢����
        /// </summary>
        public int StoreDataCapacity
        {
            get
            {
                return this.LevelStoreDataCapacity[this.Level];
            }
        }
        /// <summary>
        /// �ֿ�ȼ�
        /// </summary>
        public int Level { get; private set; }
        /// <summary>
        /// �ֿ����ȼ�
        /// </summary>
        public int LevelMax = 2;
        /// <summary>
        /// ÿ������ֿ�Ĵ洢��������
        /// </summary>
        public List<int> LevelStoreCapacity = new List<int>() { 2, 4, 8 };
        /// <summary>
        /// ÿ������ֿⵥ�����ӵ���������
        /// </summary>
        public List<int> LevelStoreDataCapacity = new List<int>() { 50, 100, 200 };
        /// <summary>
        /// �������ȼ�
        /// </summary>
        public TransportPriority TransportPriority = TransportPriority.Normal;
        /// <summary>
        /// ����Ƿ�������˲ֿ⽻��
        /// ֻҪ���������ĳһ���ֿ���н������ͽ�������Ϊtrue,��������ʱ���ܿ��Ǵ���Ϊtrue�Ĳֿ�
        /// </summary>
        public bool IsInteracting;

        public Store(StoreType storeType)
        {
            for (int i = 0; i < this.StoreCapacity; i++)
            {
                this.StoreDatas.Add(new StoreData("", this.StoreDataCapacity));
            }
            this.StoreType = storeType;
            this.Name = storeType.ToString();
        }

        /// <summary>
        /// �޸ĵȼ�
        /// </summary>
        public bool SetLevel(int newLevel)
        {
            if (WorldStore.transform != null && newLevel >= 0 && newLevel <= LevelMax)
            {
                int newStoreCapacity = LevelStoreCapacity[newLevel];
                int newStoreDataCapacity = LevelStoreDataCapacity[newLevel];
                Dictionary<string, int> temp = new Dictionary<string, int>();
                if (newStoreCapacity >= StoreCapacity)
                {
                    for (int i = 0; i < newStoreCapacity - StoreCapacity; i++)
                    {
                        this.StoreDatas.Add(new StoreData("", newStoreDataCapacity));
                    }
                }
                else
                {
                    for (int i = 0; i < StoreCapacity - newStoreCapacity; i++)
                    {
                        StoreData storeData = StoreDatas[StoreDatas.Count - 1];
                        if (ItemSpawner.Instance.IsValidItemID(storeData.ItemID) && storeData.StorageAll > 0)
                        {
                            temp.Add(storeData.ItemID, storeData.StorageAll);
                        }
                        StoreDatas.RemoveAt(StoreDatas.Count - 1);
                    }
                }
                if (newStoreDataCapacity >= StoreDataCapacity)
                {
                    foreach (StoreData storeData in StoreDatas)
                    {
                        storeData.MaxCapacity = newStoreDataCapacity;
                    }
                }
                else
                {
                    foreach (StoreData storeData in StoreDatas)
                    {
                        int removeAmount = storeData.MaxCapacity - newStoreDataCapacity;
                        if (storeData.Storage > removeAmount)
                        {
                            storeData.Storage -= removeAmount;
                            temp.Add(storeData.ItemID, removeAmount);
                        }
                        else
                        {
                            temp.Add(storeData.ItemID, storeData.Storage);
                            storeData.Storage = 0;
                        }
                        storeData.MaxCapacity = newStoreDataCapacity;
                    }
                }
                // ����temp���ɳ�������
                foreach (var kv in temp)
                {
                    List<Item> items = ItemSpawner.Instance.SpawnItems(kv.Key, kv.Value);
                    foreach (Item item in items)
                    {
                        ItemSpawner.Instance.SpawnWorldItem(item, WorldStore.transform.position, WorldStore.transform.rotation);
                    }
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
        /// <returns></returns>
        public bool ChangeStoreData(int index, string itemID)
        {
            if (0 <= index && index < this.StoreCapacity)
            {
                StoreData data = this.StoreDatas[index];
                if (data.Storage == 0 && data.StorageReserve == 0 && data.EmptyReserve == 0)
                {
                    this.StoreDatas[index] = new StoreData(itemID, this.StoreDataCapacity);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ������Ԥ���������
        /// </summary>
        public int ReserveEmptyToWorker(string itemID, int amount)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.Empty >= amount)
                    {
                        data.EmptyReserve += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        amount -= data.Empty;
                        data.EmptyReserve += data.Empty;
                    }
                }
            }
            if (amount > 0)
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        data.EmptyReserve += amount;
                        amount = 0;
                        break;
                    }
                }
            }
            return amount;
        }
        /// <summary>
        /// ������Ԥ��ȡ������
        /// </summary>
        public int ReserveStorageToWorker(string itemID, int amount)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.Storage >= amount)
                    {
                        data.Storage -= amount;
                        data.StorageReserve += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.StorageReserve += data.Storage;
                        amount -= data.Storage;
                        data.Storage = 0;
                    }
                }
            }
            return amount;
        }

        /// <summary>
        /// �ֿ��Ƿ��и���Ʒ
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <returns></returns>
        public bool IsStoreHaveItem(string itemID)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID == itemID)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// �ֿ��Ƿ���ָ�������ĸ���Ʒ
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <returns></returns>
        public bool IsStoreHaveStorage(string itemID, int amount)
        {
            return GetStoreStorage(itemID) > amount;
        }
        /// <summary>
        /// �ֿ��Ƿ��ܴ���ָ�������ĸ���Ʒ
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <returns></returns>
        public bool IsStoreHaveEmpty(string itemID, int amount)
        {
            return GetStoreEmpty(itemID) > amount;
        }
        public int GetStoreStorageAll(string itemID)
        {
            int result = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID == itemID)
                {
                    result += data.StorageAll;
                }
            }
            return result;
        }
        /// <summary>
        /// �ֿ����ж��������ĸ���Ʒ
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <returns></returns>
        public int GetStoreStorage(string itemID)
        {
            int result = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID == itemID)
                {
                    result += data.Storage;
                }
            }
            return result;
        }
        public int GetStoreStorageReserve(string itemID)
        {
            int result = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID == itemID)
                {
                    result += data.StorageReserve;
                }
            }
            return result;
        }
        /// <summary>
        /// �ֿ����ܴ�Ŷ��������ĸ���Ʒ
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <returns></returns>
        public int GetStoreEmpty(string itemID)
        {
            int result = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID == itemID)
                {
                    result += data.Empty;
                }
            }
            return result;
        }
        public int GetStoreEmptyReserve(string itemID)
        {
            int result = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID == itemID)
                {
                    result += data.EmptyReserve;
                }
            }
            return result;
        }

        #region TODO
        // TODO: ���������ֿ�ʱ����������Ʒ������ұ�����
        /// <summary>
        /// ��ݴ�ţ�����ұ����пɴ���ڸòֿ����Ʒȫ��ת�����ֿ��У�
        /// �ֿ��λ����ʱ����������ʣ������ڱ����У�
        /// </summary>
        public void FastAdd(Player.PlayerCharacter player)
        {
            
        }
        /// <summary>
        /// ���ȡ�� ������λ����ʱ����������ʣ������ڲֿ��С�
        /// </summary>
        public void FastRemove(Player.PlayerCharacter player, string itemID, int amount)
        {
            
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
        public void AddMissionTranport(MissionTransport mission) {}
        public void RemoveMissionTranport(MissionTransport mission) {}
        public bool PutIn(string itemID, int amount)
        {
            if (amount >= 0)
            {
                StoreData temp = null;
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        if (temp == null)
                        {
                            temp = data;
                        }
                        if (data.EmptyReserve >= amount)
                        {
                            data.EmptyReserve -= amount;
                            data.Storage += amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            data.Storage += data.EmptyReserve;
                            amount -= data.EmptyReserve;
                            data.EmptyReserve = 0;
                        }
                    }
                }
                if (amount > 0 && temp != null)
                {
                    temp.Storage += amount;
                    temp.EmptyReserve = 0;
                    amount = 0;
                }
                return amount == 0;
            }
            return false;
        }
        /// <summary>
        /// ����ȡ��������
        /// </summary>
        public int PutOut(string itemID, int amount)
        {
            if (amount > 0)
            {
                int result = amount;
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        if (data.StorageReserve >= result)
                        {
                            data.StorageReserve -= result;
                            result = 0;
                            break;
                        }
                        else
                        {
                            result -= data.StorageReserve;
                            data.StorageReserve = 0;
                        }
                    }
                }
                return amount - result;
            }
            return 0;
        }
        #endregion

        #region IInventory�ӿ�
        public bool AddItem(Item item)
        {
            int amount = item.Amount;
            if (GetStoreEmpty(item.ID) < amount || amount < 0)
            {
                return false;
            }
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == item.ID)
                {
                    if (data.Empty >= amount)
                    {
                        data.Storage += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        amount -= data.Empty;
                        data.Storage += data.Empty;
                    }
                }
            }
            return true;
        }

        public bool RemoveItem(Item item)
        {
            int amount = item.Amount;
            if (GetStoreStorage(item.ID) < amount || amount < 0)
            {
                return false;
            }
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == item.ID)
                {
                    if (data.Storage >= amount)
                    {
                        data.Storage -= amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        amount -= data.Storage;
                        data.Storage = 0;
                    }
                }
            }
            return true;
        }

        public Item RemoveItem(Item item, int amount)
        {
            int oldAmount = amount;
            if (amount > 0)
            {
                if (GetStoreStorage(item.ID) >= amount)
                {
                    foreach (StoreData data in this.StoreDatas)
                    {
                        if (data.ItemID != "" && data.ItemID == item.ID)
                        {
                            if (data.Storage >= amount)
                            {
                                data.Storage -= amount;
                                amount = 0;
                                break;
                            }
                            else
                            {
                                amount -= data.Storage;
                                data.Storage = 0;
                            }
                        }
                    }
                }
            }
            Item result = ItemSpawner.Instance.SpawnItem(item.ID);
            int newAmount = oldAmount - amount;
            result.Amount = newAmount;
            if (item.Amount != newAmount)
            {
                Debug.LogError($"Item Amount Error ItemAmount: {result.Amount} Amount: {newAmount}");
            }
            return result;
        }

        public bool RemoveItem(string itemID, int amount)
        {
            if (GetStoreStorage(itemID) < amount || amount < 0)
            {
                return false;
            }
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.Storage >= amount)
                    {
                        data.Storage -= amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        amount -= data.Storage;
                        data.Storage = 0;
                    }
                }
            }
            return true;
        }

        public int GetItemAllNum(string id)
        {
            return GetStoreStorageAll(id);
        }
        #endregion
    }
}