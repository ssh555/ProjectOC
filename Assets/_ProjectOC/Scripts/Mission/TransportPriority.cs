using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// �������ȼ�
    /// </summary>
    public enum TransportPriority
    {
        [LabelText("����")]
        Urgency = 0,
        [LabelText("����")]
        Normal = 1,
        [LabelText("��ѡ")]
        Alternative = 2
    }
}