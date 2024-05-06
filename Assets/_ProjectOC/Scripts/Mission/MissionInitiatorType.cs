using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS
{
    [LabelText("搬运发起者类型")]
    public enum MissionInitiatorType
    {
        [LabelText("None")]
        None = 0,
        [LabelText("放入发起者")]
        PutIn_Initiator,
        [LabelText("取出发起者")]
        PutOut_Initiator,
    }
}