using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// �ֿ�����ÿ���洢���ӵ�����
    /// </summary>
    [System.Serializable]
    public class StoreData
    {
        [LabelText("�洢��Item ID"), ReadOnly]
        public string ItemID = "";
        [LabelText("�ܴ����"), ShowInInspector, ReadOnly]
        public int StorageAll { get { return Storage + StorageReserve; } }
        [LabelText("ʵ�ʴ����"), ReadOnly]
        public int Storage;
        [LabelText("����ռ�ô����"), ReadOnly]
        public int StorageReserve;
        [LabelText("ʵ�ʿ�����"), ShowInInspector, ReadOnly]
        public int Empty 
        { 
            get
            {
                int emptyCapacity = MaxCapacity - StorageAll - EmptyReserve;
                return emptyCapacity > 0 ? emptyCapacity : 0;
            } 
        }
        [LabelText("����ռ�ÿ�����"), ReadOnly]
        public int EmptyReserve;
        [LabelText("�������"), ReadOnly]
        public int MaxCapacity;
        [LabelText("�����ܷ����"), ReadOnly]
        public bool CanIn = true;
        [LabelText("�����ܷ�ȡ��"), ReadOnly]
        public bool CanOut = true;
        public StoreData(string itemID, int maxCapacity)
        {
            ItemID = itemID;
            MaxCapacity = maxCapacity;
        }
    }
}