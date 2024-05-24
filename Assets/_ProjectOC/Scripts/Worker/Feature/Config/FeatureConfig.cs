using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [LabelText("词条配置数据"), System.Serializable]
    public struct FeatureConfig
    {
        [LabelText("词条传授消耗物品ID")]
        public string FeatTransCostItemID;
        [LabelText("词条传授消耗物品数量")]
        public int FeatTransCostItemNum;
        [LabelText("词条传授消等待时间(s)")]
        public int FeatTransTime;

        [LabelText("词条升级消耗物品ID")]
        public string FeatUpCostItemID;
        [LabelText("词条升级消耗物品数量")]
        public int FeatUpCostItemNum;
        [LabelText("词条升级消等待时间(s)")]
        public int FeatUpTime;

        [LabelText("词条降级消耗物品ID")]
        public string FeatDownCostItemID;
        [LabelText("词条降级消耗物品数量")]
        public int FeatDownCostItemNum;
        [LabelText("词条降级消等待时间(s)")]
        public int FeatDownTime;

        [LabelText("词条反转消耗物品ID")]
        public string FeatReverseCostItemID;
        [LabelText("词条反转消耗物品数量")]
        public int FeatReverseCostItemNum;
        [LabelText("词条反转消等待时间(s)")]
        public int FeatReverseTime;

        [LabelText("词条删除消耗物品ID")]
        public string FeatDelCostItemID;
        [LabelText("词条删除消耗物品数量")]
        public int FeatDelCostItemNum;
        [LabelText("词条删除消等待时间(s)")]
        public int FeatDelTime;

        [LabelText("词条上限权重")]
        public List<int> FeatureMax;
        [LabelText("增减词条权重")]
        public List<int> FeatureOdds;

        public FeatureConfig(FeatureConfig config)
        {
            FeatTransCostItemID = config.FeatTransCostItemID;
            FeatTransCostItemNum = config.FeatTransCostItemNum;
            FeatTransTime = config.FeatTransTime;
            FeatUpCostItemID = config.FeatUpCostItemID;
            FeatUpCostItemNum = config.FeatUpCostItemNum;
            FeatUpTime = config.FeatUpTime;
            FeatDownCostItemID = config.FeatDownCostItemID;
            FeatDownCostItemNum = config.FeatDownCostItemNum;
            FeatDownTime = config.FeatDownTime;
            FeatReverseCostItemID = config.FeatReverseCostItemID;
            FeatReverseCostItemNum = config.FeatReverseCostItemNum;
            FeatReverseTime = config.FeatReverseTime;
            FeatDelCostItemID = config.FeatDelCostItemID;
            FeatDelCostItemNum = config.FeatDelCostItemNum;
            FeatDelTime = config.FeatDelTime;
            FeatureMax = new List<int>();
            FeatureMax.AddRange(FeatureMax);
            FeatureOdds = new List<int>();
            FeatureOdds.AddRange(FeatureOdds);
        }
    }
}