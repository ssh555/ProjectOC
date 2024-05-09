using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [LabelText("������������"), System.Serializable]
    public struct WorkerConfig
    {
        [LabelText("��������")]
        public int APMax;
        [LabelText("����������ֵ")]
        public int APWorkThreshold;
        [LabelText("������Ϣ��ֵ")]
        public int APRelaxThreshold;
        [LabelText("��������_����")]
        public int APCost_Transport;
        [LabelText("��ʳʱ��")]
        public float EatTime;
        [LabelText("����ֵ����")]
        public int EMMax;
        [LabelText("��������ֵ")]
        public int EMLowThreshold;
        [LabelText("��������ֵ")]
        public int EMHighThreshold;
        [LabelText("������Ч��")]
        public int EMLowEffect;
        [LabelText("������Ч��")]
        public int EMHighEffect;
        [LabelText("����������")]
        public int EMCost;
        [LabelText("����ָ���")]
        public int EMRecover;
        [LabelText("�ƶ��ٶ�")]
        public float WalkSpeed;
        [LabelText("��������")]
        public int BURMax;
        [LabelText("���˾���ֵ")]
        public int ExpTransport;
        [LabelText("���ܳ�ʼ����Ч��")]
        public int SkillEff;
        [LabelText("����ȼ�ӳ���ϵ")]
        public List<int> ExpToLevel;
        [LabelText("�ȼ�Ч��ӳ��_ֵ��")]
        public List<int> LevelToEff_Duty;
        [LabelText("�ȼ�Ч��ӳ��_����")]
        public List<int> LevelToEff_Transport;
        [LabelText("δ��������ʱ��")]
        public float DestroyTimeForNoHome;
        [LabelText("���ܵȼ��ֲ���ֵ")]
        public float SkillStdMean;
        [LabelText("���ܵȼ��ֲ�����")]
        public float SkillStdDev;
        [LabelText("���ܵȼ��ֲ��Ͻ�")]
        public int SkillStdHighBound;
        [LabelText("���ܵȼ��ֲ��½�")]
        public int SkillStdLowBound;
    }
}