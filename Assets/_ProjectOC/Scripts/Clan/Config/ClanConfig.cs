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

        public ClanConfig(ClanConfig config)
        {
            GenderWeight = new List<GenderWeight>();
            GenderWeight.AddRange(config.GenderWeight);
            SexPreferenceWeight = new List<SexPreferenceWeight>();
            SexPreferenceWeight.AddRange(config.SexPreferenceWeight);
            AgeLow = config.AgeLow;
            AgeHigh = config.AgeHigh;
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