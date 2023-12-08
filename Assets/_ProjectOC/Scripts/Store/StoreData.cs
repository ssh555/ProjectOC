namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库里面每个存储格子的数据
    /// </summary>
    public class StoreData
    {
        /// <summary>
        /// 存储的Item的ID，-1为无效值，即空
        /// 只有当此容器存储量为0时，才能更改ItemID
        /// </summary>
        public string ItemID;
        //public int StorageCapacityAll { get { return StorageCapacity + StorageCapacityReserve; } }
        //public int EmptyCapacityAll { get { return EmptyCapacity + EmptyCapacityReserved; } }
        /// <summary>
        /// 实际存放量
        /// </summary>
        public int StorageCapacity;
        /// <summary>
        /// 预留存放量
        /// 任务占用的取出量，仅搬运Worker可取出将其变为PlayerEmptyCapacity
        /// </summary>
        public int StorageCapacityReserved;
        /// <summary>
        /// 实际空余量
        /// </summary>
        public int EmptyCapacity;
        /// <summary>
        /// 预留空余量
        /// 任务占用的存放量，仅搬运Worker可存入将其变为PlayerStoreCapacity
        /// </summary>
        public int EmptyCapacityReserved;
        /// <summary>
        /// 最大容量
        /// 由仓库的Level控制
        /// </summary>
        public int MaxCapacity;
        public StoreData()
        {
            this.ItemID = "-1";
            this.MaxCapacity = 0;
            this.StorageCapacity = 0;
            this.StorageCapacityReserved = 0;
            this.EmptyCapacity = 0;
            this.EmptyCapacityReserved = 0;
        }
        public StoreData(string itemID, int maxCapacity)
        {
            this.ItemID = itemID;
            this.MaxCapacity = maxCapacity;
            this.StorageCapacity = 0;
            this.StorageCapacityReserved = 0;
            this.EmptyCapacity = maxCapacity;
            this.EmptyCapacityReserved = 0;
        }
        public StoreData(string itemID, int playerStoreCapacity, int reserveStorageCapacity, 
            int playerEmptyCapacity, int reservedEmptyCapacity, int maxCapacity)
        {
            this.ItemID = itemID;
            this.StorageCapacity = playerStoreCapacity;
            this.StorageCapacityReserved = reserveStorageCapacity;
            this.EmptyCapacity = playerEmptyCapacity;
            this.EmptyCapacityReserved = reservedEmptyCapacity;
            this.MaxCapacity = maxCapacity;
        }
    }
}