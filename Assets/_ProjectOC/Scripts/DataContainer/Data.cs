using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.DataNS
{
    [LabelText("�洢����"), System.Serializable]
    public struct Data
    {
        #region Data
        [LabelText("����ID"), ReadOnly]
        public string ID;
        [LabelText("�������"), ReadOnly]
        public int MaxCapacity;
        [LabelText("ʵ�ʴ����"), ReadOnly]
        public int Storage;
        [LabelText("ʵ�ʿ�����"), ReadOnly]
        public int Empty;
        [LabelText("Ԥ�������"), ReadOnly]
        public int StorageReserve;
        [LabelText("Ԥ��������"), ReadOnly]
        public int EmptyReserve;
        [LabelText("�ܷ����"), ReadOnly]
        public bool CanIn;
        [LabelText("�ܷ�ȡ��"), ReadOnly]
        public bool CanOut;
        #endregion

        #region Property
        [LabelText("�ܴ����"), ShowInInspector, ReadOnly]
        public int StorageAll { get { return Storage + StorageReserve; } }
        [LabelText("�ܴ����"), ShowInInspector, ReadOnly]
        public bool HaveSetData { get { return !string.IsNullOrEmpty(ID); } }
        #endregion

        public Data(string itemID, int maxCapacity)
        {
            ID = itemID;
            MaxCapacity = maxCapacity;
            Storage = 0;
            Empty = 0;
            StorageReserve = 0;
            EmptyReserve = 0;
            CanIn = true;
            CanOut = true;
        }

        public void Clear()
        {
            ID = "";
            Storage = 0;
            Empty = 0;
            StorageReserve = 0;
            EmptyReserve = 0;
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
