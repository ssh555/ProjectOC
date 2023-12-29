namespace ProjectOC.MissionNS
{
    /// <summary>
    /// 搬运类型，表明了搬运的起点和终点
    /// 搬运优先级从高到低
    /// 1从生产节点至仓库
    /// 2从仓库至生产节点
    /// 3从外部至仓库
    /// </summary>
    public enum MissionTransportType
    {
        None = 0,
        /// <summary>
        /// 生产节点到仓库
        /// </summary>
        ProductionNode_Store = 1,
        /// <summary>
        /// 仓库到生产节点
        /// </summary>
        Store_ProductionNode = 2,
        /// <summary>
        /// 外部到仓库
        /// </summary>
        Outside_Store = 3,
    }
}

