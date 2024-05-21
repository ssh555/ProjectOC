using Sirenix.OdinInspector;

namespace ProjectOC.DataNS
{
    [LabelText("�洢����"), System.Serializable]
    public struct Data<T>
    {
        #region Data
        private T data;
        [LabelText("����ID"), ShowInInspector, ReadOnly]
        private string id;
        [LabelText("�ܷ����"), ShowInInspector, ReadOnly]
        private bool canIn;
        [LabelText("�ܷ�ȡ��"), ShowInInspector, ReadOnly]
        private bool canOut;
        [LabelText("�������"), ShowInInspector, ReadOnly]
        private int MaxCapacity;
        [LabelText("ʵ�ʴ����"), ShowInInspector, ReadOnly]
        private int Storage;
        [LabelText("ʵ�ʿ�����"), ShowInInspector, ReadOnly]
        private int Empty;
        [LabelText("Ԥ�������"), ShowInInspector, ReadOnly]
        private int StorageReserve;
        [LabelText("Ԥ��������"), ShowInInspector, ReadOnly]
        private int EmptyReserve;
        #endregion

        #region Property
        public string ID => id;
        public bool CanIn => canIn;
        public bool CanOut => canOut;
        [LabelText("�ܴ����"), ShowInInspector, ReadOnly]
        public int StorageAll => Storage + StorageReserve;
        [LabelText("�ܴ����"), ShowInInspector, ReadOnly]
        public bool HaveSetData => !string.IsNullOrEmpty(id);
        #endregion

        public Data(T data, int maxCapacity)
        {
            if (data is string str)
            {
                id = str;
            }
            else if (data is IDataObj dataObj)
            {
                id = dataObj.GetDataID();
            }
            else
            {
                id = data.GetHashCode().ToString();
            }
            this.data = data;
            canIn = true;
            canOut = true;
            MaxCapacity = maxCapacity;
            Storage = 0;
            Empty = maxCapacity;
            StorageReserve = 0;
            EmptyReserve = 0;
        }
        public Data(int maxCapacity)
        {
            id = "";
            data = default(T);
            canIn = true;
            canOut = true;
            MaxCapacity = maxCapacity;
            Storage = 0;
            Empty = maxCapacity;
            StorageReserve = 0;
            EmptyReserve = 0;
        }

        #region Get
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
                case DataOpType.MaxCapacity:
                    return MaxCapacity;
                case DataOpType.StorageAll:
                    return Storage + StorageReserve;
            }
            return 0;
        }
        public T GetData() { return data; }
        #endregion

        #region Set
        public void Reset()
        {
            id = "";
            data = default(T);
            Storage = 0;
            Empty = MaxCapacity;
            StorageReserve = 0;
            EmptyReserve = 0;
        }
        public void ChangeData(T data)
        {
            Reset();
            SetData(data);
        }

        private void SetData(T data)
        {
            if (data is string str)
            {
                id = str;
            }
            else if (data is IDataObj dataObj)
            {
                id = dataObj.GetDataID();
            }
            else
            {
                id = data.GetHashCode().ToString();
            }
            this.data = data;
        }

        public void ChangeCanIn(bool canIn) { this.canIn = canIn; }
        public void ChangeCanOut(bool canOut) { this.canOut = canOut; }
        public void ChangeAmount(DataOpType type, int amount)
        {
            switch (type)
            {
                case DataOpType.Storage:
                    Storage += amount;
                    break;
                case DataOpType.StorageReserve:
                    StorageReserve += amount;
                    break;
                case DataOpType.EmptyReserve:
                    EmptyReserve += amount;
                    EmptyReserve = EmptyReserve >= 0 ? EmptyReserve : 0;
                    break;
                case DataOpType.Empty:
                    Empty += amount;
                    Empty = Empty >= 0 ? Empty : 0;
                    break;
                case DataOpType.MaxCapacity:
                    if (amount > 0)
                    {
                        Empty = (amount - Storage - StorageReserve - EmptyReserve);
                        Empty = Empty >= 0 ? Empty : 0;
                        MaxCapacity = amount;
                    }
                    break;
            }
        }
        #endregion
    }
}