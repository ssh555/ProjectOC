namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ��������Ч������
    /// Setǰ׺Ϊֱ�Ӹ�ֵ
    /// Offsetǰ׺Ϊ����
    /// </summary>
    public enum EffectType
    {
        /// <summary>
        /// �����������
        /// </summary>
        Set_int_APMax = 0,
        /// <summary>
        /// ����ƶ��ٶ�
        /// </summary>
        Offset_float_WalkSpeed = 1,
        /// <summary>
        /// �����������
        /// </summary>
        Offset_int_BURMax = 2,

        /// <summary>
        /// ������Ч�ʼӳ�
        /// </summary>
        Offset_int_Eff_Cook = 101,
        /// <summary>
        /// ����ֹ�Ч�ʼӳ�
        /// </summary>
        Offset_int_Eff_HandCraft = 102,
        /// <summary>
        /// ����ع�Ч�ʼӳ�
        /// </summary>
        Offset_int_Eff_Industry = 103,
        /// <summary>
        /// �����ѧЧ�ʼӳ�
        /// </summary>
        Offset_int_Eff_Science = 104,
        /// <summary>
        /// ���ħ��Ч�ʼӳ�
        /// </summary>
        Offset_int_Eff_Magic = 105,
        /// <summary>
        /// �������Ч�ʼӳ�
        /// </summary>
        Offset_int_Eff_Transport = 106,

        /// <summary>
        /// �����⿾����ȡ�ٶ�
        /// </summary>
        Set_int_ExpRate_Cook = 201,
        /// <summary>
        /// ����ֹ������ȡ�ٶ�
        /// </summary>
        Set_int_ExpRate_HandCraft = 202,
        /// <summary>
        /// ����ع������ȡ�ٶ�
        /// </summary>
        Set_int_ExpRate_Industry = 203,
        /// <summary>
        /// �����ѧ�����ȡ�ٶ�
        /// </summary>
        Set_int_ExpRate_Science = 204,
        /// <summary>
        /// ���ħ�������ȡ�ٶ�
        /// </summary>
        Set_int_ExpRate_Magic = 205,
        /// <summary>
        /// ������˾����ȡ�ٶ�
        /// </summary>
        Set_int_ExpRate_Transport = 206,
    }
}