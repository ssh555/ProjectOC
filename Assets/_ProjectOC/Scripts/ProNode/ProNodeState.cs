using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [LabelText("�����ڵ�״̬")]
    public enum ProNodeState
    {
        /// <summary>
        /// δѡ��������
        /// </summary>
        [LabelText("����")]
        Vacancy = 0,
        /// <summary>
        /// ���������������ڸ�or��Ʒ����or�زĲ��㣬δ��������
        /// </summary>
        [LabelText("ͣ����")]
        Stagnation = 1,
        /// <summary>
        /// �������������������
        /// </summary>
        [LabelText("������")]
        Production = 2
    }
}
