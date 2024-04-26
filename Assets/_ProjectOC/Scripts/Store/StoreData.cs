using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    [LabelText("�ֿ�洢����"), System.Serializable]
    public struct StoreData
    {
        [LabelText("�洢��Item ID"), ReadOnly]
        public string ItemID;
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
        public bool CanIn;
        [LabelText("�����ܷ�ȡ��"), ReadOnly]
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