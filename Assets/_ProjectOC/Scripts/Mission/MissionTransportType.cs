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
        ProNode_Store = 1,
        [LabelText("�ֿ⵽�����ڵ�")]
        Store_ProNode = 2,
        [LabelText("�ⲿ���ֿ�")]
        Outside_Store = 3,
    }
}

