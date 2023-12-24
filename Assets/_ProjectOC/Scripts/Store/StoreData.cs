namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库里面每个存储格子的数据
    /// </summary>
    [System.Serializable]
    public class StoreData
    {
        /// <summary>
        /// 存储的Item的ID，-1为无效值，即空
        /// 只有当此容器存储量为0时，才能更改ItemID
        /// </summary>
        public string ItemID;
        public int StorageAll { get { return Storage + StorageReserved; } }
        /// <summary>
        /// 实际存放量
        /// </summary>
        public int Storage;
        /// <summary>
        /// 预留存放量
        /// 任务占用的取出量，仅搬运Worker可取出将其变为PlayerEmptyCapacity
        /// </summary>
        public int StorageReserved;
        /// <summary>
        /// 实际空余量
        /// </summary>
        public int Empty 
        { 
            get
            {
                int emptyCapacity = MaxCapacity - StorageAll - EmptyReserved;
                return emptyCapacity > 0 ? emptyCapacity : 0;
            } 
        }
        /// <summary>
        /// 预留空余量
        /// 任务占用的存放量，仅搬运Worker可存入将其变为PlayerStoreCapacity
        /// </summary>
        public int EmptyReserved;
        /// <summary>
        /// 最大容量
        /// 由仓库的Level控制
        /// </summary>
        public int MaxCapacity;
        public StoreData()
        {
            this.ItemID = "";
            this.MaxCapacity = 0;
            this.Storage = 0;
            this.StorageReserved = 0;
            this.EmptyReserved = 0;
        }
        public StoreData(string itemID, int maxCapacity)
        {
            this.ItemID = itemID;
            this.MaxCapacity = maxCapacity;
            this.Storage = 0;
            this.StorageReserved = 0;
            this.EmptyReserved = 0;
        }
        public StoreData(string itemID, int storage, int storageReserved, int emptyReserved, int maxCapacity)
        {
            this.ItemID = itemID;
            this.Storage = storage;
            this.StorageReserved = storageReserved;
            this.EmptyReserved = emptyReserved;
            this.MaxCapacity = maxCapacity;
        }
    }
}