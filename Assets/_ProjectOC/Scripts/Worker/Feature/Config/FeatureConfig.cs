using Sirenix.OdinInspector;

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
    }
}