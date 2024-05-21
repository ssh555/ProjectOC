using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// 标明搬运的起点和终点，搬运优先级从高到低
    /// </summary>
    [LabelText("搬运类型")]
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