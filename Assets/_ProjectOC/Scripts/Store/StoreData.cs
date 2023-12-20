namespace ProjectOC.StoreNS
{
    /// <summary>
    /// �ֿ�����ÿ���洢���ӵ�����
    /// </summary>
    [System.Serializable]
    public class StoreData
    {
        /// <summary>
        /// �洢��Item��ID��-1Ϊ��Чֵ������
        /// ֻ�е��������洢��Ϊ0ʱ�����ܸ���ItemID
        /// </summary>
        public string ItemID;
        public int StorageAll { get { return Storage + StorageReserved; } }
        /// <summary>
        /// ʵ�ʴ����
        /// </summary>
        public int Storage;
        /// <summary>
        /// Ԥ�������
        /// ����ռ�õ�ȡ������������Worker��ȡ�������ΪPlayerEmptyCapacity
        /// </summary>
        public int StorageReserved;
        /// <summary>
        /// ʵ�ʿ�����
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
        /// Ԥ��������
        /// ����ռ�õĴ������������Worker�ɴ��뽫���ΪPlayerStoreCapacity
        /// </summary>
        public int EmptyReserved;
        /// <summary>
        /// �������
        /// �ɲֿ��Level����
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