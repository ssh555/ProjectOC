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
        [LabelText("存储数据"), ReadOnly, ShowInInspector]
        private Data[] Datas;
        private Dictionary<string, HashSet<int>> IndexDict;
        public event Action OnDataChangeEvent;
        public DataContainer(int capacity, int dataCapacity)
        {
            if(capacity < 0  || dataCapacity < 0) { return; }
            Datas = new Data[capacity];
            for (int i = 0; i < capacity; i++)
            {
                Datas[i] = new Data(dataCapacity);
            }
            IndexDict = new Dictionary<string, HashSet<int>>();
        }
        public DataContainer(int capacity, List<int> dataCapacitys)
        {
            if (capacity < 0 || dataCapacitys == null || dataCapacitys.Count < capacity) { return; }
            Datas = new Data[capacity];
            for (int i = 0; i < capacity; i++)
            {
                Datas[i] = new Data(dataCapacitys[i]);
            }
            IndexDict = new Dictionary<string, HashSet<int>>();
        }
        public void Reset(List<IDataObj> datas, List<int> dataCapacitys)
        {
            if (datas == null || dataCapacitys == null || dataCapacitys.Count < datas.Count) { return; }
            Array.Resize(ref Datas, datas.Count);
            for (int i = 0; i < datas.Count; i++)
            {
                Datas[i] = new Data(datas[i], dataCapacitys[i]);
            }
            ResetIndexDict();
            OnDataChangeEvent?.Invoke();
        }
        public void ClearData()
        {
            for (int i = 0; i < Datas.Length; i++)
            {
                Datas[i].Reset();
            }
            ResetIndexDict();
            OnDataChangeEvent?.Invoke();
        }
        public void Clear()
        {
            Array.Resize(ref Datas, 0);
            ResetIndexDict();
            OnDataChangeEvent?.Invoke();
        }
        private void ResetIndexDict()
        {
            HashSet<string> keys = new HashSet<string>(IndexDict.Keys);
            foreach (var key in keys)
            {
                IndexDict[key].Clear();
            }
            for (int i = 0; i < GetCapacity(); i++)
            {
                Data data = Datas[i];
                if (data.HaveSetData)
                {
                    if (!IndexDict.ContainsKey(data.ID))
                    {
                        IndexDict[data.ID] = new HashSet<int>();
                    }
                    IndexDict[data.ID].Add(i);
                    keys.Remove(data.ID);
                }
            }
            foreach (var key in keys)
            {
                IndexDict.Remove(key);
            }
        }
        #endregion

        #region Str
        private const string str = "";
        #endregion

        #region Get
        public int GetCapacity() { return Datas?.Length ?? 0; }
        public bool IsValidIndex(int index) { return Datas != null && 0 <= index && index < Datas.Length; }
        public string GetID(int index) { return IsValidIndex(index) ? Datas[index].ID : str; }
        public IDataObj GetData(int index) { return IsValidIndex(index) ? Datas[index].GetData() : null; }
        public bool GetCanIn(int index) { return IsValidIndex(index) && Datas[index].CanIn; }
        public bool GetCanOut(int index) { return IsValidIndex(index) && Datas[index].CanOut; }
        public bool HaveSetData(int index) { return IsValidIndex(index) && Datas[index].HaveSetData; }
        public int GetAmount(int index, DataOpType type) { return IsValidIndex(index) ? Datas[index].GetAmount(type) : 0; }
        public Data[] GetDatas() { return Datas; }
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
        public List<int> GetIndexs(IDataObj data, bool needSort = false)
        {
            string id = data?.GetDataID() ?? str;
            if (!string.IsNullOrEmpty(id) && IndexDict.ContainsKey(id))
            {
                HashSet<int> indexs = new HashSet<int>(IndexDict[id]);
                indexs.RemoveWhere(index => !data.DataEquales(Datas[index].GetData()));
                List<int> result = indexs.ToList();
                if (needSort) { result.Sort((x, y) => x.CompareTo(y)); }
                return result;
            }
            return new List<int>();
        }
        public List<int> GetIndexs(string id, bool needSort = false)
        {
            if (!string.IsNullOrEmpty(id) && IndexDict.ContainsKey(id))
            {
                List<int> result = IndexDict[id].ToList();
                if (needSort) { result.Sort((x, y) => x.CompareTo(y)); }
                return result;
            }
            return new List<int>();
        }
        public bool HaveSetData(IDataObj data, bool needCanIn = false, bool needCanOut = false)
        {
            foreach (int index in GetIndexs(data))
            {
                if (Datas[index].HaveSetData && (!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
                {
                    return true;
                }
            }
            return false;
        }
        public bool HaveAnyData(DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            for (int i = 0; i < GetCapacity(); i++)
            {
                if (Datas[i].HaveSetData && Datas[i].GetAmount(type) > 0 && (!needCanIn || Datas[i].CanIn) && (!needCanOut || Datas[i].CanOut))
                {
                    return true;
                }
            }
            return false;
        }
        public int GetAmount(IDataObj data, DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            int result = 0;
            foreach (int index in GetIndexs(data))
            {
                if (Datas[index].HaveSetData && (!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
                {
                    result += Datas[index].GetAmount(type);
                }
            }
            return result;
        }
        public int GetAmount(string id, DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            int result = 0;
            foreach (int index in GetIndexs(id))
            {
                if (Datas[index].HaveSetData && (!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
                {
                    result += Datas[index].GetAmount(type);
                }
            }
            return result;
        }
        public IDataObj GetData(string id, DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            foreach (int index in GetIndexs(id))
            {
                if (Datas[index].HaveSetData && Datas[index].GetAmount(type) > 0 && (!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
                {
                    return Datas[index].GetData();
                }
            }
            return null;
        }
        public Dictionary<IDataObj, int> GetAmount(DataOpType type, bool needCanIn = false, bool needCanOut = false)
        {
            Dictionary<IDataObj, int> result = new Dictionary<IDataObj, int>();
            for (int i = 0; i < GetCapacity(); i++)
            {
                if (Datas[i].HaveSetData && (!needCanIn || Datas[i].CanIn) && (!needCanOut || Datas[i].CanOut))
                {
                    int amount = Datas[i].GetAmount(type);
                    if (amount > 0)
                    {
                        IDataObj key = Datas[i].GetData();
                        if (!result.ContainsKey(key))
                        {
                            result.Add(key, 0);
                        }
                        result[key] += amount;
                    }
                }
            }
            return result;
        }
        public int GetEmptyIndex(bool needCanIn = false, bool needCanOut = false)
        {
            for (int i = 0; i < GetCapacity(); i++)
            {
                if (!Datas[i].HaveSetData && (!needCanIn || Datas[i].CanIn) && (!needCanOut || Datas[i].CanOut))
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion

        #region Set
        #region Capacity
        public Dictionary<IDataObj, int> ChangeCapacity(int capacity, int dataCapacity)
        {
            lock (this)
            {
                Dictionary<IDataObj, int> result = new Dictionary<IDataObj, int>();
                if (capacity < 0 || dataCapacity < 0) { return result; }
                int oldCapacity = GetCapacity();
                for (int i = capacity; i < oldCapacity; i++)
                {
                    if (Datas[i].StorageAll > 0)
                    {
                        IDataObj key = Datas[i].GetData();
                        if (!result.ContainsKey(key))
                        {
                            result[key] = 0;
                        }
                        result[key] += Datas[i].StorageAll;
                    }
                }
                Array.Resize(ref Datas, capacity);
                for (int i = oldCapacity; i < capacity; i++)
                {
                    Datas[i] = new Data(dataCapacity);
                }
                for (int i = 0; i < capacity; i++)
                {
                    if (Datas[i].StorageAll > dataCapacity)
                    {
                        int remove = Datas[i].StorageAll - dataCapacity;
                        IDataObj key = Datas[i].GetData();
                        if (!result.ContainsKey(key))
                        {
                            result[key] = 0;
                        }
                        result[key] += remove;
                        int storage = Datas[i].GetAmount(DataOpType.Storage);
                        if (storage >= remove)
                        {
                            Datas[i].ChangeAmount(DataOpType.Storage, -remove);
                        }
                        else
                        {
                            Datas[i].ChangeAmount(DataOpType.Storage, -storage);
                            Datas[i].ChangeAmount(DataOpType.StorageReserve, storage - remove);
                        }
                    }
                    Datas[i].ChangeAmount(DataOpType.MaxCapacity, dataCapacity);
                }
                ResetIndexDict();
                OnDataChangeEvent?.Invoke();
                return result;
            }
        }
        public Dictionary<IDataObj, int> ChangeCapacity(int capacity, List<int> dataCapacitys)
        {
            lock (this)
            {
                Dictionary<IDataObj, int> result = new Dictionary<IDataObj, int>();
                if (capacity < 0 || dataCapacitys == null || dataCapacitys.Count < capacity) { return result; }
                int oldCapacity = GetCapacity();
                for (int i = capacity; i < oldCapacity; i++)
                {
                    if (Datas[i].StorageAll > 0)
                    {
                        IDataObj key = Datas[i].GetData();
                        if (!result.ContainsKey(key))
                        {
                            result[key] = 0;
                        }
                        result[key] += Datas[i].StorageAll;
                    }
                }
                Array.Resize(ref Datas, capacity);
                for (int i = oldCapacity; i < capacity; i++)
                {
                    Datas[i] = new Data(dataCapacitys[i]);
                }
                for (int i = 0; i < capacity; i++)
                {
                    if (dataCapacitys[i] < 0) { continue; }
                    if (Datas[i].StorageAll > dataCapacitys[i])
                    {
                        int remove = Datas[i].StorageAll - dataCapacitys[i];
                        IDataObj key = Datas[i].GetData();
                        if (!result.ContainsKey(key))
                        {
                            result[key] = 0;
                        }
                        result[key] += remove;
                        int storage = Datas[i].GetAmount(DataOpType.Storage);
                        if (storage >= remove)
                        {
                            Datas[i].ChangeAmount(DataOpType.Storage, -remove);
                        }
                        else
                        {
                            Datas[i].ChangeAmount(DataOpType.Storage, -storage);
                            Datas[i].ChangeAmount(DataOpType.StorageReserve, storage - remove);
                        }
                    }
                    Datas[i].ChangeAmount(DataOpType.MaxCapacity, dataCapacitys[i]);
                }
                ResetIndexDict();
                OnDataChangeEvent?.Invoke();
                return result;
            }
        }
        public void AddCapacity(int addCapacity, int dataCapacity)
        {
            lock (this)
            {
                if (addCapacity < 0 || dataCapacity < 0) { return; }
                int oldCapacity = GetCapacity();
                Array.Resize(ref Datas, oldCapacity + addCapacity);
                for (int i = 0; i < addCapacity; i++)
                {
                    Datas[oldCapacity+i] = new Data(dataCapacity);
                }
                OnDataChangeEvent?.Invoke();
            }
        }
        public void AddCapacity(int addCapacity, List<int> dataCapacitys)
        {
            lock (this)
            {
                if (addCapacity < 0 || dataCapacitys == null || dataCapacitys.Count < addCapacity) { return; }
                int oldCapacity = GetCapacity();
                Array.Resize(ref Datas, oldCapacity + addCapacity);
                for (int i = 0; i < addCapacity; i++)
                {
                    Datas[oldCapacity+i] = new Data(dataCapacitys[i]);
                }
                OnDataChangeEvent?.Invoke();
            }
        }
        #endregion

        #region Change
        public int AddDataToEmptyIndex(IDataObj data, bool needCanIn = false, bool needCanOut = false, bool needSort = false)
        {
            lock (this)
            {
                int index = GetEmptyIndex(needCanIn, needCanOut);
                if (index >= 0 && !Datas[index].HaveSetData)
                {
                    ChangeData(index, data, needSort);
                }
                return index;
            }
        }

        public void RemoveDataWithEmptyIndex(IDataObj data, bool needSort = false)
        {
            foreach (int index in GetIndexs(data))
            {
                if (GetAmount(index, DataOpType.StorageAll) == 0 && GetAmount(index, DataOpType.EmptyReserve) == 0)
                {
                    ChangeData(index, null, needSort);
                }
            }
        }
        /// <summary>
        /// 修改存储的数据
        /// </summary>
        /// <param name="index">第几个存储数据</param>
        /// <param name="id">新的数据ID</param>
        public (IDataObj, int) ChangeData(int index, IDataObj data, bool needSort=false)
        {
            lock (this)
            {
                string id = data?.GetDataID() ?? str;
                (IDataObj, int) result = (null, 0);
                if (IsValidIndex(index))
                {
                    if (Datas[index].HaveSetData)
                    {
                        string key = Datas[index].ID;
                        IndexDict[key].Remove(index);
                        if (IndexDict[key].Count == 0)
                        {
                            IndexDict.Remove(key);
                        }
                    }
                    result = (Datas[index].GetData(), Datas[index].StorageAll);
                    Datas[index].ChangeData(data);
                    if (Datas[index].HaveSetData)
                    {
                        if (!IndexDict.ContainsKey(id))
                        {
                            IndexDict[id] = new HashSet<int>();
                        }
                        IndexDict[id].Add(index);
                    }
                    if (needSort)
                    {
                        Array.Sort(Datas, new SortForData());
                    }
                    OnDataChangeEvent?.Invoke();
                }
                return result;
            }
        }
        /// <summary>
        /// 返回修改成功的数量
        /// </summary>
        public int ChangeAmount(IDataObj data, int amount, DataOpType addType, DataOpType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false)
        {
            lock (this)
            {
                string id = data.GetDataID() ?? str;
                if (!string.IsNullOrEmpty(id) && amount > 0 && CheckDataOpType(addType, removeType, exceed))
                {
                    int removeNum = GetAmount(data, removeType, needCanIn, needCanOut);
                    if (!exceed && removeNum == 0 && (!complete || removeNum < amount)) { return 0; }
                    int num = amount;
                    int temp = -1;
                    foreach (int index in GetIndexs(data, true))
                    {
                        if ((!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
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
                        if ((!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
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
        public Dictionary<IDataObj, int> ChangeAmountForUniqueData(string id, int amount, DataOpType addType, DataOpType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false)
        {
            lock (this)
            {
                Dictionary<IDataObj, int> result = new Dictionary<IDataObj, int>();
                if (!string.IsNullOrEmpty(id) && amount > 0 && CheckDataOpType(addType, removeType, exceed))
                {
                    int removeNum = GetAmount(id, removeType, needCanIn, needCanOut);
                    if (!exceed && removeNum == 0 && (!complete || removeNum < amount)) { return result; }
                    int num = amount;
                    int temp = -1;
                    foreach (int index in GetIndexs(id, true))
                    {
                        if ((!needCanIn || Datas[index].CanIn) && (!needCanOut || Datas[index].CanOut))
                        {
                            temp = temp == -1 ? index : temp;
                            int cur = Datas[index].GetAmount(removeType);
                            cur = cur <= num ? cur : num;
                            Datas[index].ChangeAmount(addType, cur);
                            Datas[index].ChangeAmount(removeType, -cur);
                            num -= cur;
                            result.Add(Datas[index].GetData(), cur);
                            if (num <= 0) { break; }
                        }
                    }
                    if (exceed && num > 0 && temp >= 0)
                    {
                        Datas[temp].ChangeAmount(addType, num);
                        Datas[temp].ChangeAmount(removeType, -num);
                        result.Add(Datas[temp].GetData(), num);
                        num = 0;
                    }
                    OnDataChangeEvent?.Invoke();
                }
                return result;
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
        #endregion
    }
}