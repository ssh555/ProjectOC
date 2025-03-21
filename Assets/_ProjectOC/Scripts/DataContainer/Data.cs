using Sirenix.OdinInspector;

namespace ProjectOC.DataNS
{
    [LabelText("存储数据"), System.Serializable]
    public struct Data
    {
        #region Data
        [LabelText("数据"), ShowInInspector, ReadOnly]
        private IDataObj data;
        [LabelText("数据ID"), ShowInInspector, ReadOnly]
        private string id;
        [LabelText("能否存入"), ShowInInspector, ReadOnly]
        private bool canIn;
        [LabelText("能否取出"), ShowInInspector, ReadOnly]
        private bool canOut;
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
        [LabelText("溢出空余量"), ShowInInspector, ReadOnly]
        private int RemoveEmpty;
        #endregion

        #region Str
        private const string str = "";
        #endregion

        #region Property
        public string ID => id;
        public bool CanIn => canIn;
        public bool CanOut => canOut;
        [LabelText("总存放量"), ShowInInspector, ReadOnly]
        public int StorageAll => Storage + StorageReserve;
        [LabelText("是否设置数据"), ShowInInspector, ReadOnly]
        public bool HaveSetData => !string.IsNullOrEmpty(id);
        #endregion

        #region Constructor
        public Data(IDataObj data, int maxCapacity)
        {
            id = data?.GetDataID() ?? str;
            this.data = data;
            canIn = true;
            canOut = true;
            MaxCapacity = maxCapacity;
            Storage = 0;
            Empty = maxCapacity;
            StorageReserve = 0;
            EmptyReserve = 0;
            RemoveEmpty = 0;
        }
        public Data(int maxCapacity)
        {
            id = str;
            data = null;
            canIn = true;
            canOut = true;
            MaxCapacity = maxCapacity;
            Storage = 0;
            Empty = maxCapacity;
            StorageReserve = 0;
            EmptyReserve = 0;
            RemoveEmpty = 0;
        }
        #endregion

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
        public IDataObj GetData() { return data; }
        #endregion

        #region Set
        public void Reset()
        {
            id = str;
            data = null;
            Storage = 0;
            Empty = MaxCapacity;
            StorageReserve = 0;
            EmptyReserve = 0;
            RemoveEmpty = 0;
        }
        public void ChangeData(IDataObj data)
        {
            Reset();
            SetData(data);
        }
        private void SetData(IDataObj data)
        {
            id = data?.GetDataID() ?? str;
            this.data = data;
        }

        public void ChangeCanIn(bool canIn) { this.canIn = canIn; }
        public void ChangeCanOut(bool canOut) { this.canOut = canOut; }
        public void ChangeAmount(DataOpType type, int amount)
        {
            int empty = 0;
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
                    break;
                case DataOpType.Empty:
                    if (RemoveEmpty > 0 && amount > 0)
                    {
                        if (amount > RemoveEmpty)
                        {
                            amount -= RemoveEmpty;
                            RemoveEmpty = 0;
                        }
                        else
                        {
                            RemoveEmpty -= amount;
                            amount = 0;
                        }
                    }
                    empty = Empty + amount;
                    if (empty < 0)
                    {
                        RemoveEmpty -= empty;
                        Empty = 0;
                    }
                    else
                    {
                        Empty = empty;
                    }
                    break;
                case DataOpType.MaxCapacity:
                    if (amount > 0)
                    {
                        empty = amount - Storage - StorageReserve - EmptyReserve - RemoveEmpty;
                        if (empty < 0)
                        {
                            RemoveEmpty = -1 * empty;
                            Empty = 0;
                        }
                        else
                        {
                            Empty = empty;
                        }
                        MaxCapacity = amount;
                    }
                    break;
            }
        }
        #endregion
    }

    public class SortForData : System.Collections.Generic.IComparer<Data>
    {
        public int Compare(Data x, Data y)
        {
            var xData = x.GetData();
            var yData = y.GetData();
            if (xData == null || yData == null)
            {
                return (xData == null).CompareTo((yData == null));
            }
            return xData.DataCompareTo(yData);
        }
    }
}