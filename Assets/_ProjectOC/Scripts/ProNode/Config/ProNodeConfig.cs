using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ProNodeNS
{
    [LabelText("�����ڵ���������"), System.Serializable]
    public struct ProNodeConfig
    {
        [LabelText("��������Ч��(%)")]
        public int EffBase;
        [LabelText("���ȼ�")]
        public int LevelMax;
        [LabelText("������ߵĻ�������Ч��(%)")]
        public List<int> LevelUpgradeEff;
        [LabelText("��������_ֵ��")]
        public int InitAPCost_Duty;
        public ProNodeConfig(ProNodeConfig config)
        {
            EffBase = config.EffBase;
            LevelMax = config.LevelMax;
            LevelUpgradeEff = new List<int>();
            LevelUpgradeEff.AddRange(config.LevelUpgradeEff);
            InitAPCost_Duty = config.InitAPCost_Duty;
        }
    }
}
