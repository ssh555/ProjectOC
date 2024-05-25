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

        [LabelText("��ֵ�ṩЧ�ʼӳ�")]
        public int CreatureOutputAddEff;
        [LabelText("���ֵ�ľ������x����Ʒ�½���ֵ")]
        public int CreatureOutputDescCount;
        [LabelText("���ֵ�ľ��������Ʒ�½���ֵ")]
        public int CreatureOutputDescValue;
        [LabelText("��ֵȡֵ����")]
        public List<int> OutputRanges;

        [LabelText("����ʱ��")]
        public int MineTimeCost;
        [LabelText("���ɾ���")]
        public int MineExp;
        [LabelText("���ɶѵ�����")]
        public int MineStackThreshold;
        [LabelText("���ɰ�����ֵ")]
        public int MineTransThreshold;

        [LabelText("UI������Ӧ����������ɫ")]
        public List<APBarColorConfig> APBarColorConfigs;
        [LabelText("UIѧϰ�ٶȶ�Ӧ��ͼ������")]
        public List<IconNumConfig> ExpRateIconNumConfigs;

        public ProNodeConfig(ProNodeConfig config)
        {
            EffBase = config.EffBase;
            LevelMax = config.LevelMax;
            LevelUpgradeEff = new List<int>();
            LevelUpgradeEff.AddRange(config.LevelUpgradeEff);
            InitAPCost_Duty = config.InitAPCost_Duty;

            CreatureOutputAddEff = config.CreatureOutputAddEff;
            CreatureOutputDescCount = config.CreatureOutputDescCount;
            CreatureOutputDescValue = config.CreatureOutputDescValue;
            OutputRanges = new List<int>();
            OutputRanges.AddRange(config.OutputRanges);

            MineTimeCost = config.MineTimeCost;
            MineExp = config.MineExp;
            MineStackThreshold = config.MineStackThreshold;
            MineTransThreshold = config.MineTransThreshold;

            APBarColorConfigs = new List<APBarColorConfig>();
            APBarColorConfigs.AddRange(config.APBarColorConfigs);
            ExpRateIconNumConfigs = new List<IconNumConfig>();
            ExpRateIconNumConfigs.AddRange(config.ExpRateIconNumConfigs);
        }
    }
    [System.Serializable]
    public struct APBarColorConfig
    {
        [LabelText("��ʼ����ֵ")]
        public int Start;
        [LabelText("��������ֵ")]
        public int End;
        [LabelText("��Ӧ��ɫ")]
        public UnityEngine.Color Color;
    }
    [System.Serializable]
    public struct IconNumConfig
    {
        [LabelText("��ʼ����ֵ")]
        public int Start;
        [LabelText("��������ֵ")]
        public int End;
        [LabelText("��Ӧ����")]
        public int Num;
    }
}
