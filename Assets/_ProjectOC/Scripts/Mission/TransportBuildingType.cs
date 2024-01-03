using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// 搬运建筑类型
    /// </summary>
    public enum TransportBuildingType
    {
        [LabelText("None")]
        None = 0,
        [LabelText("仓库")]
        Store = 1,
        [LabelText("生产节点")]
        ProNode = 2,
        [LabelText("场景物品")]
        WorldItem = 3
    }
}

