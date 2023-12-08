namespace ProjectOC.ProductionNodeNS
{
    /// <summary>
    /// 生产节点状态
    /// </summary>
    public enum ProductionNodeState
    {
        /// <summary>
        /// 空置：未选择生产项；
        /// </summary>
        Vacancy = 0,
        /// <summary>
        /// 停滞中：已有生产项，因刁民不在岗or成品堆满or素材不足，未在生产中；
        /// </summary>
        Stagnation = 1,
        /// <summary>
        /// 生产中：已有生产项，正在生产中；
        /// </summary>
        Production = 2
    }
}
