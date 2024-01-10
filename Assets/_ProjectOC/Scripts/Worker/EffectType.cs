using Sirenix.OdinInspector;
namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ����Ч������
    /// </summary>
    public enum EffectType
    {
        [LabelText("None")]
        None,

        #region Set Int
        [LabelText("�����������")]
        AlterAPMax,

        [LabelText("��⿾����ٶ�")]
        AlterExpRate_Cook,
        [LabelText("�Ṥ�����ٶ�")]
        AlterExpRate_HandCraft,
        [LabelText("���������ٶ�")]
        AlterExpRate_Industry,
        [LabelText("���������ٶ�")]
        AlterExpRate_Magic,
        [LabelText("���˾����ٶ�")]
        AlterExpRate_Transport,
        [LabelText("�ɼ������ٶ�")]
        AlterExpRate_Collect,
        #endregion

        #region Offset Int
        [LabelText("�����������")]
        AlterBURMax,

        [LabelText("������Ч�ʼӳ�")]
        AlterEff_Cook,
        [LabelText("����ṤЧ�ʼӳ�")]
        AlterEff_HandCraft,
        [LabelText("�������Ч�ʼӳ�")]
        AlterEff_Industry,
        [LabelText("�������Ч�ʼӳ�")]
        AlterEff_Magic,
        [LabelText("�������Ч�ʼӳ�")]
        AlterEff_Transport,
        [LabelText("����ɼ�Ч�ʼӳ�")]
        AlterEff_Collect,
        #endregion

        #region Offset Float
        [LabelText("����ƶ��ٶ�")]
        AlterWalkSpeed,
        #endregion
    }
}