using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ProNodeNS
{
    [LabelText("�����ڵ���������"), System.Serializable]
    public struct ProNodeConfig
    {
        [LabelText("��������Ч��"), PropertyTooltip("��λ %")]
        public int EffBase;
        [LabelText("���ȼ�")]
        public int LevelMax;
        [LabelText("������ߵĻ�������Ч��")]
        public List<int> LevelUpgradeEff;
        [LabelText("��������_ֵ��")]
        public int InitAPCost_Duty;
    }
}
