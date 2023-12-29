using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using ProjectOC.MissionNS;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// �ֿ�
    /// </summary>
    [System.Serializable]
    public class Store: IMission
    {
        public WorldStore WorldStore;
        public string UID { get { return WorldStore?.InstanceID ?? ""; } }
        public string ID;
        public TextContent Name;
        /// <summary>
        /// �ֿ�����
        /// </summary>
        public StoreType Type;
        /// <summary>
        /// �ֿ�洢����
        /// </summary>
        public List<StoreData> StoreDatas = new List<StoreData>();
        public List<Transport> Transports = new List<Transport>();

        /// <summary>
        /// �ֿ��������ֿ��ܷŶ�������Ʒ
        /// </summary>
        public int StoreCapacity
        {
            get
            {
                return this.LevelStoreCapacity[this.Level - 1];
            }
        }
        /// <summary>
        /// �ֿ����ݵ�������������Ʒ�����洢����
        /// </summary>
        public int StoreDataCapacity
        {
            get
            {
                return this.LevelStoreDataCapacity[this.Level - 1];
            }
        }
        private int level = 1;
        /// <summary>
        /// �ֿ�ȼ�
        /// </summary>
        public int Level 
        {
            get { return level; }
            set 
            {
                int newLevel = value;
                if (newLevel >= 1 && newLevel<=LevelMax)
                {
                    int newStoreCapacity = LevelStoreCapacity[newLevel];
                    int newStoreDataCapacity = LevelStoreDataCapacity[newLevel];
                    List<StoreItem> temp = new List<StoreItem>();
                    if (newStoreCapacity >= StoreCapacity)
                    {
                        for (int i=0; i<newStoreCapacity-StoreCapacity;i++)
                        {
                            this.StoreDatas.Add(new StoreData("", newStoreDataCapacity));
                        }
                    }
                    else
                    {
                        for (int i = 0; i <  StoreCapacity - newStoreCapacity; i++)
                        {
                            StoreData storeData = this.StoreDatas[this.StoreDatas.Count - 1];
                            if (ItemSpawner.Instance.IsValidItemID(storeData.ItemID) && storeData.StorageAll > 0)
                            {
                                temp.Add(new StoreItem(storeData.ItemID, storeData.StorageAll));
                            }
                            this.StoreDatas.RemoveAt(this.StoreDatas.Count - 1);
                        }
                    }
                    if (newStoreDataCapacity >= StoreDataCapacity)
                    {
                        foreach (StoreData storeData in this.StoreDatas)
                        {
                            storeData.MaxCapacity = newStoreDataCapacity;
                        }
                    }
                    else
                    {
                        foreach (StoreData storeData in this.StoreDatas)
                        {
                            int removeAmount = storeData.MaxCapacity - newStoreDataCapacity;
                            if (storeData.Storage > removeAmount)
                            {
                                storeData.Storage -= removeAmount;
                                temp.Add(new StoreItem(storeData.ItemID, removeAmount));
                            }
                            else
                            {
                                temp.Add(new StoreItem(storeData.ItemID, storeData.Storage));
                                storeData.Storage = 0;
                            }
                            storeData.MaxCapacity = newStoreDataCapacity;
                        }
                    }
                    // TODO:����temp���ɳ�������
                    this.level = value;
                }
            } 
        }
        /// <summary>
        /// �ֿ����ȼ�
        /// </summary>
        public int LevelMax = 3;
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
        public TransportPriority TransportPriority;
        /// <summary>
        /// ����Ƿ�������˲ֿ⽻��
        /// ֻҪ���������ĳһ���ֿ���н������ͽ�������Ϊtrue,��������ʱ���ܿ��Ǵ���Ϊtrue�Ĳֿ�
        /// </summary>
        public bool IsInteracting;
        /// <summary>
        /// �ֿ�洢�����仯ʱ����
        /// �ֿ����������롢�Ƴ�
        /// ����Ϊ��ǰ�ֿ��<ID, List<StoreData>>
        /// </summary>
        //public event Action<string, List<StoreData>> OnStoreCapacityChanged;

        public Store(StoreManager.StoreTableJsonData config)
        {
            for (int i = 0; i < this.StoreCapacity; i++)
            {
                this.StoreDatas.Add(new StoreData("", this.StoreDataCapacity));
            }
            this.ID = config.id;
            this.Name = config.name;
            this.Type = config.type;
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
                if (data.Storage == 0 && data.StorageReserved == 0)
                {
                    this.StoreDatas[index] = new StoreData(itemID, this.StoreDataCapacity);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Player����Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public StoreItem AddItemFromPlayer(StoreItem item)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == item.ItemID)
                {
                    if (data.Empty >= item.Amount)
                    {
                        data.Storage += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                    else
                    {
                        item.Amount -= data.Empty;
                        data.Storage += data.Empty;
                    }
                }
            }
            return item;
        }
        /// <summary>
        /// Playerȡ��Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns>ȡ�����Ľ��</returns>
        public StoreItem RemoveItemToPlayer(string itemID, int amount)
        {
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
            return new StoreItem(itemID, amount);
        }

        /// <summary>
        /// ��������Worker���������Item
        /// </summary>
        /// <param name="item"></param>
        public StoreItem AddItemFromWorker(StoreItem item)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == item.ItemID)
                {
                    if (data.EmptyReserved >= item.Amount)
                    {
                        data.EmptyReserved -= item.Amount;
                        data.Storage += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                    else
                    {
                        data.Storage += data.EmptyReserved;
                        item.Amount -= data.EmptyReserved;
                        data.EmptyReserved = 0;
                    }
                }
            }
            return item;
        }

        /// <summary>
        /// ȡ������Worker���������Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public StoreItem RemoveItemToWorker(string itemID, int amount)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.StorageReserved >= amount)
                    {
                        data.StorageReserved -= amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        amount -= data.StorageReserved;
                        data.StorageReserved = 0;
                    }
                }
            }
            return new StoreItem(itemID, amount);
        }

        /// <summary>
        /// ������Ԥ���������
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public StoreItem ReserveEmptyCapacityToWorker(string itemID, int amount)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.Empty >= amount)
                    {
                        data.EmptyReserved += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        amount -= data.Empty;
                        data.EmptyReserved += data.Empty;
                    }
                }
            }
            if (amount > 0)
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        data.EmptyReserved += amount;
                        amount = 0;
                        break;
                    }
                }
            }
            return new StoreItem(itemID, amount);
        }
        /// <summary>
        /// ������Ԥ��ȡ������
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public StoreItem ReserveStorageCapacityToWorker(string itemID, int amount)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.Storage >= amount)
                    {
                        data.Storage -= amount;
                        data.StorageReserved += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.StorageReserved += data.Storage;
                        amount -= data.Storage;
                        data.Storage = 0;
                    }
                }
            }
            return new StoreItem(itemID, amount);
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
        public bool IsStoreHaveItemStorage(string itemID, int amount)
        {
            return GetItemStorageCapacity(itemID) > amount;
        }
        /// <summary>
        /// �ֿ��Ƿ��ܴ���ָ�������ĸ���Ʒ
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <returns></returns>
        public bool IsStoreHaveItemEmpty(string itemID, int amount)
        {
            return GetItemEmptyCapacity(itemID) > amount;
        }
        /// <summary>
        /// �ֿ����ж��������ĸ���Ʒ
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <returns></returns>
        public int GetItemStorageCapacity(string itemID)
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
        /// <summary>
        /// �ֿ����ܴ�Ŷ��������ĸ���Ʒ
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <returns></returns>
        public int GetItemEmptyCapacity(string itemID)
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

        #region TODO
        // TODO: ���������ֿ�ʱ����������Ʒ������ұ�����
        /// <summary>
        /// ����棬������������Ʒ�ڱ����Ͳֿ�֮��ת��
        /// �治�ɳ����ֿ��λ
        /// </summary>
        public void NormalAdd(Player.PlayerCharacter player, string itemID, int amount)
        {
            // TODO: �ȴ������ӿ�
        }
        /// <summary>
        /// ����ȡ��������������Ʒ�ڱ����Ͳֿ�֮��ת��
        /// ȡ���ɳ����ֿ����ͱ�����λ�Ľ�Сֵ
        /// </summary>
        public void NormalRemove(Player.PlayerCharacter player, string itemID, int amount)
        {
            // TODO: �ȴ������ӿ�
        }
        /// <summary>
        /// ��ݴ�ţ�����ұ����пɴ���ڸòֿ����Ʒȫ��ת�����ֿ��У�
        /// �ֿ��λ����ʱ����������ʣ������ڱ����У�
        /// </summary>
        public void FastAdd(Player.PlayerCharacter player)
        {
            // TODO: �ȴ������ӿ�
        }
        /// <summary>
        /// ���ȡ�� ������λ����ʱ����������ʣ������ڲֿ��С�
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        public void FastRemove(Player.PlayerCharacter player, string itemID, int amount)
        {
            // TODO: �ȴ������ӿ�
        }


        #endregion

        #region IMission�ӿ�
        Transform IMission.GetTransform()
        {
            throw new NotImplementedException();
        }

        TransportPriority IMission.GetTransportPriority()
        {
            throw new NotImplementedException();
        }

        string IMission.GetUID()
        {
            throw new NotImplementedException();
        }

        void IMission.AddTransport(Transport transport)
        {
            throw new NotImplementedException();
        }

        void IMission.RemoveTranport(Transport transport)
        {
            throw new NotImplementedException();
        }

        void IMission.AddMissionTranport(MissionTransport mission)
        {
            throw new NotImplementedException();
        }

        void IMission.RemoveMissionTranport(MissionTransport mission)
        {
            throw new NotImplementedException();
        }

        bool IMission.PutIn(string itemID, int amount)
        {
            throw new NotImplementedException();
        }

        int IMission.PutOut(string itemID, int amount)
        {
            throw new NotImplementedException();
        }

        int IMission.GetItemAmount(string itemID)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}