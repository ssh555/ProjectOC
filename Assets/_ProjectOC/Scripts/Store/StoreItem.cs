namespace ProjectOC.StoreNS
{
    /// <summary>
    /// �ֿ���Ʒ
    /// </summary>
    [System.Serializable]
    public struct StoreItem 
    {
        /// <summary>
        /// ��ƷID
        /// </summary>
        public string ItemID;
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public int Amount;
        public StoreItem(string itemID, int amount)
        {
            this.ItemID = itemID;
            this.Amount = amount;
        }
    }
}