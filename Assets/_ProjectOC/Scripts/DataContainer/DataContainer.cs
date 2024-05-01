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
        [NonSerialized]
        private Dictionary<string, HashSet<int>> IndexDict;
        public Action OnDataChangeEvent;
        public DataContainer(int capacity, int dataCapacity)
        {
            if(capacity < 0  || dataCapacity < 0) { return; }
            Datas = new Data[capacity];
            for (int i = 0; i < capacity; i++)
            {
                Datas[i] = new Data("", dataCapacity);
            }
            IndexDict = new Dictionary<string, HashSet<int>>();
            OnDataChangeEvent?.Invoke();
        }
        public DataContainer(List<string> ids, List<int> dataCapacitys)
        {
            Datas = new Data[ids.Count];
            if (dataCapacitys.Count >= ids.Count)
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    Datas[i] = new Data(ids[i], dataCapacitys[i]);
                }
            }
            IndexDict = new Dictionary<string, HashSet<int>>();
            UpdateIndexDict();
            OnDataChangeEvent?.Invoke();
        }
        private void UpdateIndexDict()
        {
            IndexDict.Clear();
            for (int i = 0; i < Datas.Length; i++)
            {
                Data data = Datas[i];
                if (data.HaveSetData)
                {
                    if (!IndexDict.ContainsKey(data.ID))
                    {
                        IndexDict[data.ID] = new HashSet<int>();
                    }
                    IndexDict[data.ID].Add(i);
                }
            }
        }
        #endregion

        #region Get
        public int GetCapacity() { return Datas?.Length ?? 0; }
        public bool IsValidIndex(int index) { return 0 <= index && index < Datas.Length; }
        public string GetID(int index) { return IsValidIndex(index) ? Datas[index].ID : ""; }
        public bool GetCanIn(int index) { return IsValidIndex(index) ? Datas[index].CanIn : false; }
        public bool GetCanOut(int index) { return IsValidIndex(index) ? Datas[index].CanOut : false; }
        public bool HaveSetData(int index) { return IsValidIndex(index) && Datas[index].HaveSetData; }
        public int GetAmount(int index, DataOpType type) { return IsValidIndex(index) ? Datas[index].GetAmount(type) : 0; }
        public List<Data> GetDatas() { return Datas.ToList(); }

        public bool CheckDataOpType(DataOpType addType, DataOpType removeType, bool exceed = false)
        {
            if (exceed && removeType != DataOpType.Empty && removeType != DataOpType.EmptyReserve)
            {
                return false;
            }
            switch (addType)
            {
                case DataOpType.Storage:
                    return removeType != DataOpType.Storage;
                case DataOpType.StorageReserve:
                    return removeType == DataOpType.Storage;
                case DataOpType.EmptyReserve:
                    return removeType == DataOpType.Empty;
                case DataOpType.Empty:
                    return removeType != DataOpType.Empty;
                default:
                    return false;
            }
        }

        public List<int> GetIndexs(string id, bool needSort = false)
        {
            if (!string.IsNullOrEmpty(id) && IndexDict.ContainsKey(id))
            {
                List<int> indexs = IndexDict[id].ToList();
                if (needSort)
                {
                    indexs.Sort((x, y) => x.CompareTo(y));
                }
                return indexs;
            }
            return new List<int>();
        }

        public bool HaveSetData(string id, bool needCanIn = false, bool needCanOut = false)
        {
            if (!string.IsNullOrEmpty(id))
            {
                foreach (int index in GetIndexs(id))
                {
                    if (Datas[index].HaveSetData && Datas[index].ID == id && (!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HaveData(string id, DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            foreach (Data data in Datas)
            {
                if (data.ID == id && data.GetAmount(type) > 0 && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HaveAnyData(DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            foreach (Data data in Datas)
            {
                if (data.HaveSetData && data.GetAmount(type) > 0 && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                {
                    return true;
                }
            }
            return false;
        }

        public int GetAmount(string id, DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            int result = 0;
            foreach (int index in GetIndexs(id))
            {
                if (Datas[index].HaveSetData && Datas[index].ID == id && (!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
                {
                    result += Datas[index].GetAmount(type);
                }
            }
            return result;
        }

        public Dictionary<string, int> GetAmount(DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach (Data data in Datas)
            {
                if (data.HaveSetData && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                {
                    int amount = data.GetAmount(type);
                    if (amount > 0)
                    {
                        if (!result.ContainsKey(data.ID))
                        {
                            result[data.ID] = 0;
                        }
                        result[data.ID] += amount;
                    }
                }
            }
            return result;
        }
        #endregion

        #region Set
        public Dictionary<string, int> ChangeCapacity(int capacity, int dataCapacity)
        {
            lock (this)
            {
                Dictionary<string, int> dict = new Dictionary<string, int>();
                if (capacity < 0 || dataCapacity < 0) { return dict; }
                Data[] newDatas = new Data[capacity];
                for (int i = 0; i < capacity; i++)
                {
                    if (!IsValidIndex(i)) { break; }
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
                    newDatas[i].ChangeAmount(DataOpType.MaxCapacity, dataCapacity);
                }
                Datas = newDatas;
                UpdateIndexDict();
                OnDataChangeEvent?.Invoke();
                return dict;
            }
        }

        public Dictionary<string, int> ChangeCapacity(int capacity, List<int> dataCapacitys)
        {
            lock (this)
            {
                Dictionary<string, int> dict = new Dictionary<string, int>();
                if (capacity < 0 || dataCapacitys.Count < capacity) { return dict; }
                Data[] newDatas = new Data[capacity];
                for (int i = 0; i < capacity; i++)
                {
                    if (!IsValidIndex(i)) { break; }
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
                    if (dataCapacitys[i] < 0) { continue; }
                    if (newDatas[i].StorageAll > dataCapacitys[i])
                    {
                        if (!dict.ContainsKey(newDatas[i].ID))
                        {
                            dict[newDatas[i].ID] = 0;
                        }
                        int remove = newDatas[i].StorageAll - dataCapacitys[i];
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
                    newDatas[i].ChangeAmount(DataOpType.MaxCapacity, dataCapacitys[i]);
                }
                Datas = newDatas;
                UpdateIndexDict();
                OnDataChangeEvent?.Invoke();
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
            lock (this)
            {
                id = id ?? "";
                Tuple<string, int> result = new Tuple<string, int>("", 0);
                if (IsValidIndex(index))
                {
                    if (Datas[index].StorageAll > 0)
                    {
                        result = new Tuple<string, int>(Datas[index].ID, Datas[index].StorageAll);
                        IndexDict[Datas[index].ID].Remove(index);
                    }
                    Datas[index].ChangeID(id);
                    if (!IndexDict.ContainsKey(id))
                    {
                        IndexDict[id] = new HashSet<int>();
                    }
                    IndexDict[id].Add(index);
                    OnDataChangeEvent?.Invoke();
                }
                return result;
            }
        }

        /// <summary>
        /// 返回修改成功的数量
        /// </summary>
        public int ChangeAmount(string id, int amount, DataOpType addType, DataOpType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(id) && amount > 0 && CheckDataOpType(addType, removeType, exceed))
                {
                    int removeNum = GetAmount(id, removeType, needCanIn, needCanOut);
                    if (!exceed && removeNum == 0 && (!complete || removeNum < amount)) { return 0; }
                    int num = amount;
                    int temp = -1;
                    foreach (int index in GetIndexs(id, true))
                    {
                        if (Datas[index].ID == id && (!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
                        {
                            temp = temp == -1 ? index : temp;
                            int cur = Datas[index].GetAmount(removeType);
                            cur = cur <= num ? cur : num;
                            Datas[index].ChangeAmount(addType, cur);
                            Datas[index].ChangeAmount(removeType, -cur);
                            num -= cur;
                            if (num <= 0) { break; }
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

        public int ChangeAmount(int index, int amount, DataOpType addType, DataOpType removeType, bool exceed = false, bool complete = true)
        {
            lock (this)
            {
                if (IsValidIndex(index) && Datas[index].HaveSetData && amount > 0 && CheckDataOpType(addType, removeType, exceed))
                {
                    int remove = GetAmount(index, removeType);
                    if (!exceed && remove == 0 && (!complete || remove < amount)) { return 0; }
                    int num = Datas[index].GetAmount(removeType);
                    num = !exceed && num <= amount ? num : amount;
                    Datas[index].ChangeAmount(addType, num);
                    Datas[index].ChangeAmount(removeType, -num);
                    OnDataChangeEvent?.Invoke();
                    return num;
                }
                return 0;
            }
        }

        public void ChangeCanIn(int index, bool canIn) { if (IsValidIndex(index)) { Datas[index].ChangeCanIn(canIn); } }
        public void ChangeCanOut(int index, bool canOut) { if (IsValidIndex(index)) { Datas[index].ChangeCanOut(canOut); } }
        #endregion
    }
}
