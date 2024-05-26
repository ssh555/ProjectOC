using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ProNodeNS
{
    [LabelText("生产节点配置数据"), System.Serializable]
    public struct ProNodeConfig
    {
        [LabelText("基础生产效率(%)")]
        public int EffBase;
        [LabelText("最大等级")]
        public int LevelMax;
        [LabelText("升级提高的基础生产效率(%)")]
        public List<int> LevelUpgradeEff;
        [LabelText("体力消耗_值班")]
        public int InitAPCost_Duty;

        [LabelText("产值提供效率加成")]
        public int CreatureOutputAddEff;
        [LabelText("活度值耗尽后产出x次物品下降产值")]
        public int CreatureOutputDescCount;
        [LabelText("活度值耗尽后产出物品下降产值")]
        public int CreatureOutputDescValue;
        [LabelText("产值取值区间")]
        public List<int> OutputRanges;

        [LabelText("开采时间")]
        public int MineTimeCost;
        [LabelText("开采经验")]
        public int MineExp;
        [LabelText("开采堆叠上限")]
        public int MineStackThreshold;
        [LabelText("开采搬运阈值")]
        public int MineTransThreshold;

        [LabelText("UI体力对应的体力条颜色")]
        public List<APBarColorConfig> APBarColorConfigs;
        [LabelText("UI学习速度对应的图标数量")]
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
        [LabelText("初始体力值")]
        public int Start;
        [LabelText("结束体力值")]
        public int End;
        [LabelText("对应颜色")]
        public UnityEngine.Color Color;
    }
    [System.Serializable]
    public struct IconNumConfig
    {
        [LabelText("初始体力值")]
        public int Start;
        [LabelText("结束体力值")]
        public int End;
        [LabelText("对应数量")]
        public int Num;
    }
}
