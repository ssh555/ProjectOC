using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库里面每个存储格子的数据
    /// 只有当此容器存储量为0时，才能更改ItemID
    /// </summary>
    [System.Serializable]
    public class StoreData
    {
        [LabelText("存储的Item ID")]
        public string ItemID;
        [LabelText("总存放量")]
        public int StorageAll { get { return Storage + StorageReserve; } }
        [LabelText("实际存放量")]
        public int Storage;
        [LabelText("任务占用存放量")]
        public int StorageReserve;
        [LabelText("实际空余量")]
        public int Empty 
        { 
            get
            {
                int emptyCapacity = MaxCapacity - StorageAll - EmptyReserve;
                return emptyCapacity > 0 ? emptyCapacity : 0;
            } 
        }
        [LabelText("任务占用空余量")]
        public int EmptyReserve;
        [LabelText("最大容量")]
        public int MaxCapacity;
        public StoreData()
        {
            this.ItemID = "";
            this.MaxCapacity = 0;
            this.Storage = 0;
            this.StorageReserve = 0;
            this.EmptyReserve = 0;
        }
        public StoreData(string itemID, int maxCapacity)
        {
            this.ItemID = itemID;
            this.MaxCapacity = maxCapacity;
            this.Storage = 0;
            this.StorageReserve = 0;
            this.EmptyReserve = 0;
        }
    }
}