using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// �������˵������յ㣬�������ȼ��Ӹߵ���
    /// </summary>
    [LabelText("��������")]
    public enum MissionTransportType
    {
        [LabelText("None")]
        None = 0,
        [LabelText("�����ڵ㵽�ֿ�")]
        ProNode_Store,
        [LabelText("�����ڵ㵽����")]
        ProNode_Restaurant,
        [LabelText("�ֿ⵽�����ڵ�")]
        Store_ProNode,
        [LabelText("�ⲿ���ֿ�")]
        Outside_Store,
    }
}