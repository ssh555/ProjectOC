using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ClanNS
{
    [LabelText("������������"), System.Serializable]
    public struct ClanConfig
    {
        [LabelText("�Ա�Ȩ��")]
        public List<GenderWeight> GenderWeight;
        [LabelText("��ȡ��Ȩ��")]
        public List<SexPreferenceWeight> SexPreferenceWeight;
        [LabelText("���������½�")]
        public int AgeLow;
        [LabelText("���������Ͻ�")]
        public int AgeHigh;
        [LabelText("����ȡֵ����")]
        public List<int> TalentM;
        [LabelText("���ܸ���ֵ�½�")]
        public double TalentA;
        [LabelText("���ܸ���ֵ�Ͻ�")]
        public double TalentB;
        [LabelText("������ֵ����ϵ���½�")]
        public double TalentC;
        [LabelText("������ֵ����ϵ���Ͻ�")]
        public double TalentD;
        [LabelText("����ƫ̬�ֲ�����")]
        public double TalentVar;
        [LabelText("����ƫ̬�ֲ�ƫ��ֵ")]
        public double TalentLambda;

        public ClanConfig(ClanConfig config)
        {
            GenderWeight = new List<GenderWeight>();
            GenderWeight.AddRange(config.GenderWeight);
            SexPreferenceWeight = new List<SexPreferenceWeight>();
            SexPreferenceWeight.AddRange(config.SexPreferenceWeight);
            AgeLow = config.AgeLow;
            AgeHigh = config.AgeHigh;
            TalentM = new List<int>();
            TalentM.AddRange(config.TalentM);
            TalentA = config.TalentA;
            TalentB = config.TalentB;
            TalentC = config.TalentC;
            TalentD = config.TalentD;
            TalentVar = config.TalentVar;
            TalentLambda = config.TalentLambda;
        }
    }

    [System.Serializable]
    public struct GenderWeight
    {
        [LabelText("�Ա�")]
        public Gender Gender;
        [LabelText("Ȩ��")]
        public int Weight;
    }
    [System.Serializable]
    public struct SexPreferenceWeight
    {
        [LabelText("��ȡ��")]
        public SexPreference SexPreference;
        [LabelText("Ȩ��")]
        public int Weight;
    }
}