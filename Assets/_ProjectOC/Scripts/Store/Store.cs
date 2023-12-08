using System;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.MissionNS;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// �ֿ�
    /// </summary>
    [System.Serializable]
    public class Store
    {
        /// <summary>
        /// ͳһʹ�ý�����ʵ�����, -1 Ϊ invalid value
        /// </summary>
        public string UID;
        /// <summary>
        /// ID
        /// </summary>
        public string ID;
        /// <summary>
        /// �ֿ�����
        /// </summary>
        public StoreType Type;
        /// <summary>
        /// �ֿ�洢����
        /// </summary>
        public List<StoreData> StoreDatas = new List<StoreData>();
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
        public int Level = 1;
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
        public PriorityTransport PriorityTransport;
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
        public event Action<string, List<StoreData>> OnStoreCapacityChanged;

        public Store()
        {
            for (int i = 0; i < this.StoreCapacity; i++)
            {
                this.StoreDatas.Add(new StoreData("-1", this.StoreDataCapacity));
            }
        }
        public Store(string id)
        {
            this.ID = id;
            // TODO:����
            for (int i = 0; i < this.StoreCapacity; i++)
            {
                this.StoreDatas.Add(new StoreData("-1", this.StoreDataCapacity));
            }
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
                if (data.StorageCapacity == 0 && data.StorageCapacityReserved == 0)
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
                    if (data.EmptyCapacity >= item.Amount)
                    {
                        data.EmptyCapacity -= item.Amount;
                        data.StorageCapacity += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                    else
                    {
                        data.StorageCapacity += data.EmptyCapacity;
                        item.Amount -= data.EmptyCapacity;
                        data.EmptyCapacity = 0;
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
            int removeAmount = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.StorageCapacity >= amount)
                    {
                        data.EmptyCapacity += amount;
                        data.StorageCapacity -= amount;
                        removeAmount += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.EmptyCapacity += data.StorageCapacity;
                        removeAmount += data.StorageCapacity;
                        amount -= data.StorageCapacity;
                        data.StorageCapacity = 0;
                    }
                }
            }
            return new StoreItem(itemID, removeAmount);
        }

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

        /// <summary>
        /// ��������Worker���������Item
        /// </summary>
        /// <param name="item"></param>
        public StoreItem AddItemFromWorker(StoreItem item)
        {
            // TODO: ���������Ʒʱ���������ֿ��������ޡ�
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == item.ItemID)
                {
                    if (data.EmptyCapacityReserved >= item.Amount)
                    {
                        data.EmptyCapacityReserved -= item.Amount;
                        data.StorageCapacity += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                    else
                    {
                        data.StorageCapacity += data.EmptyCapacityReserved;
                        item.Amount -= data.EmptyCapacityReserved;
                        data.EmptyCapacityReserved = 0;
                    }
                }
            }
            // �������
            if (item.Amount > 0)
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == item.ItemID)
                    {
                        data.EmptyCapacityReserved -= item.Amount;
                        data.StorageCapacity += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                }
            }
            return item;
        }

        /// <summary>
        /// ȡ������Worker���������Item
        /// ����ֵΪ�Ƴ���Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public StoreItem RemoveItemToWorker(string itemID, int amount)
        {
            int removeAmount = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.StorageCapacityReserved >= amount)
                    {
                        data.StorageCapacityReserved -= amount;
                        data.EmptyCapacity += amount;
                        removeAmount += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.EmptyCapacity += data.StorageCapacityReserved;
                        removeAmount += data.StorageCapacityReserved;
                        amount -= data.StorageCapacityReserved;
                        data.StorageCapacityReserved = 0;
                    }
                }
            }
            return new StoreItem(itemID, removeAmount);
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
                    if (data.EmptyCapacity >= amount)
                    {
                        data.EmptyCapacity -= amount;
                        data.EmptyCapacityReserved += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.EmptyCapacityReserved += data.EmptyCapacity;
                        amount -= data.EmptyCapacity;
                        data.EmptyCapacity = 0;
                    }
                }
            }
            if (amount > 0)
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        data.EmptyCapacity -= amount;
                        data.EmptyCapacityReserved += amount;
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
                    if (data.StorageCapacity >= amount)
                    {
                        data.StorageCapacity -= amount;
                        data.StorageCapacityReserved += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.StorageCapacityReserved += data.StorageCapacity;
                        amount -= data.StorageCapacity;
                        data.StorageCapacity = 0;
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
                    result += data.StorageCapacity;
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
                    result += data.EmptyCapacity;
                }
            }
            return result;
        }

        // TODO: ���������ֿ�ʱ����������Ʒ������ұ�����
    }
}