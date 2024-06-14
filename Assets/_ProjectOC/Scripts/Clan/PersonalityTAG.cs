using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ClanNS
{
    [LabelText("TAG"), System.Serializable]
    public struct PersonalityTAG
    {
        public string ID;
        public string Name;
        public string Desc;
        public TAGType Type;
        public int Level;

        public int Wisdom;
        public int Combat;
        public int Resilience;
        public int ThinkingLow;
        public int ThinkingHigh;
        public int SocialLow;
        public int SocialHigh;
        public int BasisLow;
        public int BasisHigh;

        public bool IsActive;

        public bool CheckCondition(Clan clan)
        {
            if (clan.Wisdom >= Wisdom && clan.Combat >= Combat && clan.Resilience >= Resilience)
            {
                var thinking = clan.PersonalityDict[PersonalityType.Thinking];
                var social = clan.PersonalityDict[PersonalityType.Social];
                var basis = clan.PersonalityDict[PersonalityType.Basis];
                if (thinking.Value >= ThinkingLow && thinking.Value <= ThinkingHigh &&
                    social.Value >= SocialLow && social.Value <= SocialHigh &&
                    basis.Value >= BasisLow && basis.Value <= BasisHigh)
                {
                    return true;
                }
            }
            return false;
        }
        public PersonalityTAG ChangeActive(bool isActive)
        {
            IsActive = isActive;
            return this;
        }
        public (int, int) GetPersonality(PersonalityType type)
        {
            switch (type)
            {
                case PersonalityType.Thinking:
                    return (ThinkingLow, ThinkingHigh);
                case PersonalityType.Social:
                    return (SocialLow, SocialHigh);
                case PersonalityType.Basis:
                    return (BasisLow, BasisHigh);
            }
            return (0, 0);
        }

        public class Sort : IComparer<PersonalityTAG>
        {
            public int Compare(PersonalityTAG x, PersonalityTAG y)
            {
                if (x.Level != y.Level)
                {
                    return y.Level.CompareTo(x.Level);
                }
                if (x.Type != y.Type)
                {
                    return x.Type.CompareTo(y.Type);
                }
                return x.ID.CompareTo(y.ID);
            }
        }
    }
}