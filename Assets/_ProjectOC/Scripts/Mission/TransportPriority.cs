using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// �������ȼ�
    /// </summary>
    public enum TransportPriority
    {
        [LabelText("����")]
        Normal = 0,
        [LabelText("����")]
        Urgency = 1,
        [LabelText("��ѡ")]
        Alternative = 2
    }
}