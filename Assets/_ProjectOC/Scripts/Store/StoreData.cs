using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// �ֿ�����ÿ���洢���ӵ�����
    /// ֻ�е��������洢��Ϊ0ʱ�����ܸ���ItemID
    /// </summary>
    [System.Serializable]
    public class StoreData
    {
        [LabelText("�洢��Item ID")]
        public string ItemID;
        [LabelText("�ܴ����")]
        public int StorageAll { get { return Storage + StorageReserve; } }
        [LabelText("ʵ�ʴ����")]
        public int Storage;
        [LabelText("����ռ�ô����")]
        public int StorageReserve;
        [LabelText("ʵ�ʿ�����")]
        public int Empty 
        { 
            get
            {
                int emptyCapacity = MaxCapacity - StorageAll - EmptyReserve;
                return emptyCapacity > 0 ? emptyCapacity : 0;
            } 
        }
        [LabelText("����ռ�ÿ�����")]
        public int EmptyReserve;
        [LabelText("�������")]
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