using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectOC.DataNS 
{
    [LabelText("数据容器"), Serializable]
    public class DataContainer
    {
        #region Data
        [LabelText("存储数据"), ReadOnly]
        private Data[] Datas;
        private Dictionary<string, HashSet<int>> DataIndexDict;
        /// <summary>
        /// 能存多少个存储数据
        /// </summary>
        [LabelText("容量"), ShowInInspector, ReadOnly]
        public int Capacity { get; private set; }
        /// <summary>
        /// 存储数据里能存多少个数据
        /// </summary>
        [LabelText("存储数据的容量"), ShowInInspector, ReadOnly]
        public int DataCapacity { get; private set; }
        /// <summary>
        /// 数据变化事件
        /// </summary>
        public event Action OnDataChangeEvent;
        #endregion

        #region DataContainer
        public DataContainer(int capacity, int dataCapacity)
        {
            Datas = new Data[capacity];
            for (int i = 0; i < capacity; i++)
            {
                Datas[i] = new Data("", dataCapacity);
            }
            DataIndexDict = new Dictionary<string, HashSet<int>>();
        }

        private void UpdateIndexDict()
        {
            DataIndexDict.Clear();
            for (int i = 0; i < Datas.Length; i++)
            {
                Data data = Datas[i];
                if (data.HaveSetData)
                {
                    if (!DataIndexDict.ContainsKey(data.ID))
                    {
                        DataIndexDict[data.ID] = new HashSet<int>();
                    }
                    DataIndexDict[data.ID].Add(i);
                }
            }
        }

        public Dictionary<string, int> ChangeCapacity(int capacity, int dataCapacity)
        {
            lock (this)
            {
                Dictionary<string, int> dict = new Dictionary<string, int>();
                if (capacity > 0 && dataCapacity > 0)
                {
                    Data[] newDatas = new Data[capacity];
                    for (int i = 0; i < capacity; i++)
                    {
                        if (!IsValidIndex(i))
                        {
                            break;
                        }
                        newDatas[i] = Datas[i];
                    }
                    for (int i = capacity; i < Datas.Length; i++)
                    {
                        if (Datas[i].StorageAll > 0)
                        {
                            if (!dict.ContainsKey(Datas[i].ID))
                            {
                                dict.Add(Datas[i].ID, 0);
                            }
                            dict[Datas[i].ID] += Datas[i].StorageAll;
                        }
                    }

                    for (int i = 0; i < capacity; i++)
                    {
                        if (newDatas[i].StorageAll > dataCapacity)
                        {
                            if (!dict.ContainsKey(newDatas[i].ID))
                            {
                                dict[newDatas[i].ID] = 0;
                            }
                            int remove = newDatas[i].StorageAll - dataCapacity;
                            dict[newDatas[i].ID] += remove;

                            int storage = newDatas[i].GetAmount(DataOpType.Storage);
                            if (storage >= remove)
                            {
                                newDatas[i].ChangeAmount(DataOpType.Storage, -remove);
                            }
                            else
                            {
                                newDatas[i].ChangeAmount(DataOpType.Storage, -storage);
                                newDatas[i].ChangeAmount(DataOpType.StorageReserve, storage - remove);
                            }
                        }
                        newDatas[i].MaxCapacity = dataCapacity;
                    }
                    Datas = newDatas;
                    UpdateIndexDict();
                    OnDataChangeEvent?.Invoke();
                }
                return dict;
            }
        }

        /// <summary>
        /// 修改存储的数据
        /// </summary>
        /// <param name="index">第几个存储数据</param>
        /// <param name="id">新的数据ID</param>
        public Tuple<string, int> ChangeData(int index, string id)
        {
            id = id ?? "";
            Tuple<string, int> result = new Tuple<string, int>("", 0);
            if (IsValidIndex(index) && Datas[index].HaveSetData)
            {
                result = new Tuple<string, int>(Datas[index].ID, Datas[index].StorageAll);
                DataIndexDict[Datas[index].ID].Remove(index);
                Datas[index].Clear();
                Datas[index].ID = id;
                if (!DataIndexDict.ContainsKey(id))
                {
                    DataIndexDict[id] = new HashSet<int>();
                }
                DataIndexDict[id].Add(index);
                OnDataChangeEvent?.Invoke();
            }
            return result;
        }
        #endregion

        #region Get
        public bool IsValidIndex(int index)
        {
            return 0 <= index && index < Datas.Length;
        }

        public Data GetData(int index)
        {
            return IsValidIndex(index) ? Datas[index] : default(Data);
        }

        public List<Data> GetAllDatas()
        {
            return Datas.ToList();
        }

        public Dictionary<string, int> GetAllDatasStorage()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (Data data in Datas)
            {
                if (data.StorageAll > 0)
                {
                    if (!dict.ContainsKey(data.ID))
                    {
                        dict[data.ID] = 0;
                    }
                    dict[data.ID] += data.StorageAll;
                }
            }
            return dict;
        }

        public int GetDataAmount(string id, DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(id) && DataIndexDict.ContainsKey(id))
            {
                foreach (int index in DataIndexDict[id])
                {
                    Data data = Datas[index];
                    if (data.ID == id && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                    {
                        result += data.GetAmount(type);
                    }
                }
            }
            return result;
        }

        private List<int> GetIndexs(string id)
        {
            if (!string.IsNullOrEmpty(id) && DataIndexDict.ContainsKey(id))
            {
                List<int> indexs = DataIndexDict[id].ToList();
                indexs.Sort((x, y) => x.CompareTo(y));
                return indexs;
            }
            return new List<int>();
        }
        #endregion

        #region Set
        /// <summary>
        /// 是否有该物品
        /// </summary>
        public bool IsStoreHaveItem(string itemID, bool needCanIn = false, bool needCanOut = false)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (Data data in Datas)
                {
                    if (data.ID == itemID && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
       
        
        public bool CheckCanChangeData(DataOpType addType, DataOpType removeType, bool exceed = false)
        {
            if (exceed && removeType != DataOpType.Empty && removeType != DataOpType.EmptyReserve)
            {
                return false;
            }
            switch (addType)
            {
                case DataOpType.Storage:
                    return removeType == DataOpType.Empty || removeType == DataOpType.StorageReserve || removeType == DataOpType.EmptyReserve;
                case DataOpType.StorageReserve:
                    return removeType == DataOpType.Storage;
                case DataOpType.EmptyReserve:
                    return removeType == DataOpType.Empty;
                case DataOpType.Empty:
                    return removeType == DataOpType.Storage || removeType == DataOpType.StorageReserve || removeType == DataOpType.EmptyReserve;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 返回修改成功的数量
        /// </summary>
        public int ChangeAmount(string itemID, int amount, DataOpType addType, DataOpType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(itemID) && amount > 0 && CheckCanChangeData(addType, removeType, exceed))
                {
                    int removeNum = GetDataAmount(itemID, removeType, needCanIn, needCanOut);
                    if (!exceed && removeNum == 0 && (!complete || removeNum < amount))
                    {
                        return 0;
                    }
                    int num = amount;
                    int temp = -1;
                    for (int i = 0; i < Datas.Length; i++)
                    {
                        if (Datas[i].ID == itemID && (!needCanIn || Datas[i].CanIn) && (!needCanOut || Datas[i].CanOut))
                        {
                            if (temp >= 0)
                            {
                                temp = i;
                            }
                            int cur = Datas[i].GetAmount(removeType);
                            cur = cur <= num ? cur : num;
                            Datas[i].ChangeAmount(addType, cur);
                            Datas[i].ChangeAmount(removeType, -cur);
                            num -= cur;
                            if (num <= 0)
                            {
                                break;
                            }
                        }
                    }
                    if (exceed && num > 0 && temp >= 0)
                    {
                        Datas[temp].ChangeAmount(addType, num);
                        Datas[temp].ChangeAmount(removeType, -num);
                        num = 0;
                    }
                    OnDataChangeEvent?.Invoke();
                    return amount - num;
                }
                return 0;
            }
        }
        #endregion
    }
}
