using Sirenix.OdinInspector;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// �������ͣ������˰��˵������յ�
    /// �������ȼ��Ӹߵ���
    /// 1�������ڵ����ֿ�
    /// 2�Ӳֿ��������ڵ�
    /// 3���ⲿ���ֿ�
    /// </summary>
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

