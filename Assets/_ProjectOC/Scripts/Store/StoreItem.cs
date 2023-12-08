namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库物品
    /// </summary>
    public struct StoreItem 
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemID;
        /// <summary>
        /// 物品数量
        /// </summary>
        public int Amount;
        public StoreItem(string itemID, int amount)
        {
            this.ItemID = itemID;
            this.Amount = amount;
        }
    }
}