using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS 
{
    /// <summary>
    /// 刁民搬运状态
    /// </summary>
    public enum TransportState
    {
        [LabelText("空手行进")]
        EmptyHanded,
        [LabelText("持物行进")]
        HoldingObjects
    }
}