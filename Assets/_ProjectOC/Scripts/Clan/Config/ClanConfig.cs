using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ClanNS
{
    [LabelText("氏族配置数据"), System.Serializable]
    public struct ClanConfig
    {
        [LabelText("性别权重")]
        public List<GenderWeight> GenderWeight;
        [LabelText("性取向权重")]
        public List<SexPreferenceWeight> SexPreferenceWeight;
        [LabelText("年龄区间下界")]
        public int AgeLow;
        [LabelText("年龄区间上界")]
        public int AgeHigh;
        [LabelText("才能取值区间")]
        public List<int> TalentM;
        [LabelText("才能个体值下界")]
        public double TalentA;
        [LabelText("才能个体值上界")]
        public double TalentB;
        [LabelText("才能数值乘算系数下界")]
        public double TalentC;
        [LabelText("才能数值乘算系数上界")]
        public double TalentD;
        [LabelText("才能偏态分布方差")]
        public double TalentVar;
        [LabelText("才能偏态分布偏度值")]
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
        [LabelText("性别")]
        public Gender Gender;
        [LabelText("权重")]
        public int Weight;
    }
    [System.Serializable]
    public struct SexPreferenceWeight
    {
        [LabelText("性取向")]
        public SexPreference SexPreference;
        [LabelText("权重")]
        public int Weight;
    }
}