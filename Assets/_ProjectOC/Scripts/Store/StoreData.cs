using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    [LabelText("仓库存储数据"), System.Serializable]
    public struct StoreData
    {
        [LabelText("存储的Item ID"), ReadOnly]
        public string ItemID;
        [LabelText("总存放量"), ShowInInspector, ReadOnly]
        public int StorageAll { get { return Storage + StorageReserve; } }
        [LabelText("实际存放量"), ReadOnly]
        public int Storage;
        [LabelText("任务占用存放量"), ReadOnly]
        public int StorageReserve;
        [LabelText("实际空余量"), ShowInInspector, ReadOnly]
        public int Empty 
        { 
            get
            {
                int emptyCapacity = MaxCapacity - StorageAll - EmptyReserve;
                return emptyCapacity > 0 ? emptyCapacity : 0;
            } 
        }
        [LabelText("任务占用空余量"), ReadOnly]
        public int EmptyReserve;
        [LabelText("最大容量"), ReadOnly]
        public int MaxCapacity;
        [LabelText("刁民能否存入"), ReadOnly]
        public bool CanIn;
        [LabelText("刁民能否取出"), ReadOnly]
        public bool CanOut;

        public StoreData(string itemID, int maxCapacity)
        {
            ItemID = itemID;
            MaxCapacity = maxCapacity;
            CanIn = true;
            CanOut = true;

            Storage = 0;
            StorageReserve = 0;
            EmptyReserve = 0;
        }

        public bool HaveItem()
        {
            return !string.IsNullOrEmpty(ItemID) && ManagerNS.LocalGameManager.Instance.ItemManager.IsValidItemID(ItemID);
        }

        public void ClearData()
        {
            this.ItemID = "";
            this.Storage = 0;
            this.StorageReserve = 0;
            this.EmptyReserve = 0;
        }

        public int GetNum(Store.DataType dataType)
        {
            switch (dataType)
            {
                case Store.DataType.Storage:
                    return Storage;
                case Store.DataType.StorageReserve:
                    return StorageReserve;
                case Store.DataType.EmptyReserve:
                    return EmptyReserve;
                case Store.DataType.Empty:
                    return Empty;
            }
            return 0;
        }

        public void ChangeData(Store.DataType dataType, int num)
        {
            switch (dataType)
            {
                case Store.DataType.Storage:
                    Storage += num;
                    break;
                case Store.DataType.StorageReserve:
                    StorageReserve += num;
                    break;
                case Store.DataType.EmptyReserve:
                    EmptyReserve += num;
                    EmptyReserve = EmptyReserve >= 0 ? EmptyReserve : 0;
                    break;
                case Store.DataType.Empty: 
                    break;
            }
        }
    }
}