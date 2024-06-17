using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ClanNS
{
    [LabelText("TAG"), System.Serializable]
    public struct PersonalityTAG
    {
        public string ID;
        public bool IsActive;
        public PersonalityTAGTableData Data => (ManagerNS.LocalGameManager.Instance != null) ?
            ManagerNS.LocalGameManager.Instance.ClanManager.GetTAG(ID) : default(PersonalityTAGTableData);
        public string Name => Data.Name ?? "";
        public string Desc => Data.Description ?? "";
        public TAGType Type => Data.Type;
        public int Level => Data.Level;

        public bool CheckCondition(Clan clan)
        {
            PersonalityTAGTableData data = ManagerNS.LocalGameManager.Instance.ClanManager.GetTAG(ID);
            if (clan.Wisdom >= data.Wisdom && clan.Combat >= data.Combat && clan.Resilience >= data.Resilience)
            {
                var thinking = clan.PersonalityDict[PersonalityType.Thinking];
                var social = clan.PersonalityDict[PersonalityType.Social];
                var basis = clan.PersonalityDict[PersonalityType.Basis];
                if (thinking.Value >= data.ThinkingLow && thinking.Value <= data.ThinkingHigh &&
                    social.Value >= data.SocialLow && social.Value <= data.SocialHigh &&
                    basis.Value >= data.BasisLow && basis.Value <= data.BasisHigh)
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
            PersonalityTAGTableData data = ManagerNS.LocalGameManager.Instance.ClanManager.GetTAG(ID);
            switch (type)
            {
                case PersonalityType.Thinking:
                    return (data.ThinkingLow, data.ThinkingHigh);
                case PersonalityType.Social:
                    return (data.SocialLow, data.SocialHigh);
                case PersonalityType.Basis:
                    return (data.BasisLow, data.BasisHigh);
            }
            return (0, 0);
        }

        public class Sort : IComparer<PersonalityTAG>
        {
            public int Compare(PersonalityTAG x, PersonalityTAG y)
            {
                PersonalityTAGTableData dataX = ManagerNS.LocalGameManager.Instance.ClanManager.GetTAG(x.ID);
                PersonalityTAGTableData dataY = ManagerNS.LocalGameManager.Instance.ClanManager.GetTAG(y.ID);
                if (dataX.Level != dataY.Level)
                {
                    return dataY.Level.CompareTo(dataX.Level);
                }
                if (dataX.Type != dataY.Type)
                {
                    return dataX.Type.CompareTo(dataY.Type);
                }
                return x.ID.CompareTo(y.ID);
            }
        }
    }
}