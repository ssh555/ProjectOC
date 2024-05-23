using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS
{
    [LabelText("搬运优先级")]
    public enum TransportPriority
    {
        [LabelText("紧急")]
        Urgency = 0,
        [LabelText("常规")]
        Normal = 1,
        [LabelText("备选")]
        Alternative = 2
    }
}