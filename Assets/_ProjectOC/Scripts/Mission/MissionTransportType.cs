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
        None = 0,
        /// <summary>
        /// �����ڵ㵽�ֿ�
        /// </summary>
        ProductionNode_Store = 1,
        /// <summary>
        /// �ֿ⵽�����ڵ�
        /// </summary>
        Store_ProductionNode = 2,
        /// <summary>
        /// �ⲿ���ֿ�
        /// </summary>
        Outside_Store = 3,
    }
}

