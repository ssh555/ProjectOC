using Sirenix.OdinInspector;

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
        [LabelText("None")]
        None = 0,
        [LabelText("生产节点到仓库")]
        ProNode_Store,
        [LabelText("生产节点到餐厅")]
        ProNode_Restaurant,
        [LabelText("仓库到生产节点")]
        Store_ProNode,
        [LabelText("外部到仓库")]
        Outside_Store,
    }
}

