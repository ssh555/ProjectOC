using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS
{
    [LabelText("���˷���������")]
    public enum MissionInitiatorType
    {
        [LabelText("None")]
        None = 0,
        [LabelText("���뷢����")]
        PutIn_Initiator,
        [LabelText("ȡ��������")]
        PutOut_Initiator,
    }
}