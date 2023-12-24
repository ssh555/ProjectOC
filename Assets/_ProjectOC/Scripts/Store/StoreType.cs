namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库类型
    /// </summary>
    public enum StoreType
    {
        /// <summary>
        /// 储物箱：可储存任何物品的基础仓库
        /// </summary>
        Normal,
        /// <summary>
        /// 恒温箱：可调节温度的仓库，保证某些特殊物品不会坏掉
        /// </summary>
        Incubator
    }
}