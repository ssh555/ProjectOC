namespace ProjectOC.OrderNS
{
    /// <summary>
    /// 订单类型
    /// </summary>
    public enum OrderType
    {
        None,
        /// <summary>
        /// 常规订单
        /// </summary>
        Regular,
        /// <summary>
        /// 紧急订单
        /// </summary>
        Urgent,
        /// <summary>
        /// 特殊订单
        /// </summary>
        Special
    }
}