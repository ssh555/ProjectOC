namespace ProjectOC.StoreNS
{
    /// <summary>
    /// �ֿ�����ÿ���洢���ӵ�����
    /// </summary>
    public class StoreData
    {
        /// <summary>
        /// �洢��Item��ID��-1Ϊ��Чֵ������
        /// ֻ�е��������洢��Ϊ0ʱ�����ܸ���ItemID
        /// </summary>
        public string ItemID;
        //public int StorageCapacityAll { get { return StorageCapacity + StorageCapacityReserve; } }
        //public int EmptyCapacityAll { get { return EmptyCapacity + EmptyCapacityReserved; } }
        /// <summary>
        /// ʵ�ʴ����
        /// </summary>
        public int StorageCapacity;
        /// <summary>
        /// Ԥ�������
        /// ����ռ�õ�ȡ������������Worker��ȡ�������ΪPlayerEmptyCapacity
        /// </summary>
        public int StorageCapacityReserved;
        /// <summary>
        /// ʵ�ʿ�����
        /// </summary>
        public int EmptyCapacity;
        /// <summary>
        /// Ԥ��������
        /// ����ռ�õĴ������������Worker�ɴ��뽫���ΪPlayerStoreCapacity
        /// </summary>
        public int EmptyCapacityReserved;
        /// <summary>
        /// �������
        /// �ɲֿ��Level����
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