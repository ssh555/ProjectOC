using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库里面每个存储格子的数据
    /// </summary>
    [System.Serializable]
    public class StoreData
    {
        [LabelText("存储的Item ID"), ReadOnly]
        public string ItemID = "";
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
        public bool CanIn = true;
        [LabelText("刁民能否取出"), ReadOnly]
        public bool CanOut = true;
        public StoreData(string itemID, int maxCapacity)
        {
            ItemID = itemID;
            MaxCapacity = maxCapacity;
        }
    }
}