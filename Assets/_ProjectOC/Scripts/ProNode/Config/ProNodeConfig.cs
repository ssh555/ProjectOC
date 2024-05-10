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
