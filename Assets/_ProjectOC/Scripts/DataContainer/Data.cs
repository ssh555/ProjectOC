using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.DataNS
{
    [LabelText("存储数据"), System.Serializable]
    public struct Data
    {
        #region Private
        [LabelText("最大容量"), ShowInInspector, ReadOnly]
        private int MaxCapacity;
        [LabelText("实际存放量"), ShowInInspector, ReadOnly]
        private int Storage;
        [LabelText("实际空余量"), ShowInInspector, ReadOnly]
        private int Empty;
        [LabelText("预留存放量"), ShowInInspector, ReadOnly]
        private int StorageReserve;
        [LabelText("预留空余量"), ShowInInspector, ReadOnly]
        private int EmptyReserve;
        #endregion

        #region Public
        [LabelText("数据ID"), ReadOnly]
        public string ID;
        [LabelText("能否存入"), ReadOnly]
        public bool CanIn;
        [LabelText("能否取出"), ReadOnly]
        public bool CanOut;
        [LabelText("总存放量"), ShowInInspector, ReadOnly]
        public int StorageAll { get { return Storage + StorageReserve; } }
        [LabelText("总存放量"), ShowInInspector, ReadOnly]
        public bool HaveSetData { get { return !string.IsNullOrEmpty(ID); } }
        #endregion

        public Data(string id, int maxCapacity)
        {
            ID = id;
            MaxCapacity = maxCapacity;
            Storage = 0;
            Empty = maxCapacity;
            StorageReserve = 0;
            EmptyReserve = 0;
            CanIn = true;
            CanOut = true;
        }

        public void Clear()
        {
            ID = "";
            Empty += Storage + StorageReserve + EmptyReserve;
            Storage = 0;
            StorageReserve = 0;
            EmptyReserve = 0;
        }

        public int GetMaxCapacity()
        {
            return MaxCapacity;
        }

        public void SetMaxCapacity(int maxCapacity)
        {
            if (maxCapacity >= 0)
            {
                Empty += (maxCapacity - MaxCapacity);
                Empty = Empty < 0 ? 0 : Empty;
                MaxCapacity = maxCapacity;
            }
        }

        public int GetAmount(DataOpType type)
        {
            switch (type)
            {
                case DataOpType.Storage:
                    return Storage;
                case DataOpType.Empty:
                    return Empty;
                case DataOpType.StorageReserve:
                    return StorageReserve;
                case DataOpType.EmptyReserve:
                    return EmptyReserve;
            }
            return 0;
        }

        public void ChangeAmount(DataOpType type, int amount)
        {
            switch (type)
            {
                case DataOpType.Storage:
                    Storage += amount;
                    if (Storage < 0)
                    {
                        Debug.LogError($"Storage {Storage} < 0");
                    }
                    break;
                case DataOpType.StorageReserve:
                    StorageReserve += amount;
                    if (StorageReserve < 0)
                    {
                        Debug.LogError($"StorageReserve {StorageReserve} < 0");
                    }
                    break;
                case DataOpType.EmptyReserve:
                    EmptyReserve += amount;
                    EmptyReserve = EmptyReserve >= 0 ? EmptyReserve : 0;
                    break;
                case DataOpType.Empty:
                    Empty += amount;
                    Empty = Empty >= 0 ? Empty : 0;
                    break;
            }
        }
    }
}
