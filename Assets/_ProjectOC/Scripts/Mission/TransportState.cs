using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS 
{
    [LabelText("刁民搬运状态")]
    public enum TransportState
    {
        [LabelText("空手行进")]
        EmptyHanded,
        [LabelText("持物行进")]
        HoldingObjects
    }
}