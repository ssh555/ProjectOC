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