using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [LabelText("������������"), System.Serializable]
    public struct FeatureConfig
    {
        [LabelText("��������������ƷID")]
        public string FeatTransCostItemID;
        [LabelText("��������������Ʒ����")]
        public int FeatTransCostItemNum;
        [LabelText("�����������ȴ�ʱ��(s)")]
        public int FeatTransTime;

        [LabelText("��������������ƷID")]
        public string FeatUpCostItemID;
        [LabelText("��������������Ʒ����")]
        public int FeatUpCostItemNum;
        [LabelText("�����������ȴ�ʱ��(s)")]
        public int FeatUpTime;

        [LabelText("��������������ƷID")]
        public string FeatDownCostItemID;
        [LabelText("��������������Ʒ����")]
        public int FeatDownCostItemNum;
        [LabelText("�����������ȴ�ʱ��(s)")]
        public int FeatDownTime;

        [LabelText("������ת������ƷID")]
        public string FeatReverseCostItemID;
        [LabelText("������ת������Ʒ����")]
        public int FeatReverseCostItemNum;
        [LabelText("������ת���ȴ�ʱ��(s)")]
        public int FeatReverseTime;

        [LabelText("����ɾ��������ƷID")]
        public string FeatDelCostItemID;
        [LabelText("����ɾ��������Ʒ����")]
        public int FeatDelCostItemNum;
        [LabelText("����ɾ�����ȴ�ʱ��(s)")]
        public int FeatDelTime;

        [LabelText("��������Ȩ��")]
        public List<int> FeatureMax;
        [LabelText("��������Ȩ��")]
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