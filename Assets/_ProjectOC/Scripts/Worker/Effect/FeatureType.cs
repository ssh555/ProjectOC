using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ��������
    /// ���ϵ��µ�˳��Ϊ���塢���桢���桢��������
    /// </summary>
    public enum FeatureType
    {
        None,
        [LabelText("��ת����")]
        Reverse,
        [LabelText("��������")]
        DeBuff,
        [LabelText("��������")]
        Buff,
        [LabelText("��������")]
        Race,
    }
}

