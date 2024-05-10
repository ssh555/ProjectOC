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
        [LabelText("��ʳʱ��(s)")]
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
        [LabelText("�ƶ��ٶ�(m/s)")]
        public float WalkSpeed;
        [LabelText("��������")]
        public int BURMax;
        [LabelText("���˾���ֵ")]
        public int ExpTransport;
        [LabelText("���ܳ�ʼ����Ч��(%)")]
        public int SkillEff;
        [LabelText("����ȼ�ӳ���ϵ")]
        public List<int> ExpToLevel;
        [LabelText("�ȼ�Ч��ӳ��_ֵ��")]
        public List<int> LevelToEff_Duty;
        [LabelText("�ȼ�Ч��ӳ��_����")]
        public List<int> LevelToEff_Transport;
        [LabelText("δ��������ʱ��(s)")]
        public float DestroyTimeForNoHome;
        [LabelText("���ܵȼ��ֲ���ֵ")]
        public float SkillStdMean;
        [LabelText("���ܵȼ��ֲ�����")]
        public float SkillStdDev;
        [LabelText("���ܵȼ��ֲ��Ͻ�")]
        public int SkillStdHighBound;
        [LabelText("���ܵȼ��ֲ��½�")]
        public int SkillStdLowBound;

        public WorkerConfig(WorkerConfig config)
        {
            APMax = config.APMax;
            APWorkThreshold = config.APWorkThreshold;
            APRelaxThreshold = config.APRelaxThreshold;
            APCost_Transport = config.APCost_Transport;
            EatTime = config.EatTime;
            EMMax = config.EMMax;
            EMLowThreshold = config.EMLowThreshold;
            EMHighThreshold = config.EMHighThreshold;
            EMLowEffect = config.EMLowEffect;
            EMHighEffect = config.EMHighEffect;
            EMCost = config.EMCost;
            EMRecover = config.EMRecover;
            WalkSpeed = config.WalkSpeed;
            BURMax = config.BURMax;
            ExpTransport = config.ExpTransport;
            SkillEff = config.SkillEff;
            ExpToLevel = new List<int>();
            ExpToLevel.AddRange(config.ExpToLevel);
            LevelToEff_Duty = new List<int>();
            LevelToEff_Duty.AddRange(config.LevelToEff_Duty);
            LevelToEff_Transport = new List<int>();
            LevelToEff_Transport.AddRange(config.LevelToEff_Transport);
            DestroyTimeForNoHome = config.DestroyTimeForNoHome;
            SkillStdMean = config.SkillStdMean;
            SkillStdDev = config.SkillStdDev;
            SkillStdHighBound = config.SkillStdHighBound;
            SkillStdLowBound = config.SkillStdLowBound;
        }
    }
}