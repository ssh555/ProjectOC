using ML.Engine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ClanNS
{
    [LabelText("氏族"), System.Serializable]
    public class Clan : NPC
    {
        [LabelText("病症阶段"), ReadOnly]
        public int Level;
        #region Talent
        [LabelText("学识知慧"), ReadOnly]
        public int Wisdom;
        [LabelText("战斗技巧"), ReadOnly]
        public int Combat;
        [LabelText("心理韧性"), ReadOnly]
        public int Resilience;
        #endregion

        #region Personality
        [LabelText("性格"), ReadOnly, ShowInInspector]
        public Dictionary<PersonalityType, Personality> PersonalityDict;
        #endregion

        public Clan() : base()
        {
            PersonalityDict = new Dictionary<PersonalityType, Personality>
            {
                { PersonalityType.Thinking, new Personality() },
                { PersonalityType.Social, new Personality() },
                { PersonalityType.Basis, new Personality() }
            };
            // TODO: 初始化Wisdom，Combat，Resilience
            // TODO: 初始化PersonalityDict
        }
        public int GetTalent(TalentType type)
        {
            switch (type)
            {
                case TalentType.Wisdom:
                    return Wisdom;
                case TalentType.Combat:
                    return Combat;
                case TalentType.Resilience:
                    return Resilience;
                default:
                    return 0;
            }
        }
        public void ChangeTalent(TalentType type, int value)
        {
            int temp = GetTalent(type) + value;
            temp = System.Math.Min(temp, 99);
            temp = System.Math.Max(temp, 0);
            switch (type)
            {
                case TalentType.Wisdom: 
                    Wisdom = temp; 
                    break;
                case TalentType.Combat: 
                    Combat = temp; 
                    break;
                case TalentType.Resilience: 
                    Resilience = temp; 
                    break;
            }
        }

        public void ChangePersonality(PersonalityType type, int value)
        {
            PersonalityDict[type] = PersonalityDict[type].ChangeValue(value);
        }

        public void RefreshATG()
        {
            for (int i = 0; i < TAGS.Count; i++)
            {
                bool condition = TAGS[i].CheckCondition(this);
                if (!TAGS[i].IsActive && condition)
                {
                    TAGS[i] = TAGS[i].ChangeActive(true);
                }
                else if (TAGS[i].IsActive && !condition)
                {
                    TAGS[i] = TAGS[i].ChangeActive(false);
                }
            }
        }
        public bool CheckPersonalityTAG(PersonalityType type, PersonalityTAG tag)
        {
            var personality = PersonalityDict[type];
            var tagLowHigh = tag.GetPersonality(type);
            int tagLow = tagLowHigh.Item1;
            int tagHigh = tagLowHigh.Item2;
            bool flag1 = personality.Low >= tagLow && personality.Low <= tagHigh;
            bool flag2 = tagLow >= personality.Low && tagLow <= personality.High;
            return flag1 || flag2;
        }
        public bool CheckPersonalityTAG(PersonalityTAG tag)
        {
            return CheckPersonalityTAG(PersonalityType.Thinking, tag) &&
                CheckPersonalityTAG(PersonalityType.Social, tag) &&
                CheckPersonalityTAG(PersonalityType.Basis, tag);
        }
        public override bool AddTAG(PersonalityTAG tag)
        {
            if (CheckPersonalityTAG(tag))
            {
                TAGS.Add(tag);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI,
                                        new UIManager.SideBarUIData($"氏族成员{Name}新增特征TAG{tag.Name}", null, null));
                return true;
            }
            return false;
        }
        public override void RemoveTAG(PersonalityTAG tag)
        {
            TAGS.Remove(tag);
            ML.Engine.Manager.GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI,
                                        new UIManager.SideBarUIData($"氏族成员{Name}失去特征TAG{tag.Name}", null, null));
        }

        #region Bed
        [LabelText("拥有的床"), ReadOnly, System.NonSerialized]
        public ClanBed Bed;
        [LabelText("是否拥有床"), ShowInInspector, ReadOnly]
        public bool HasBed { get { return Bed != null && !string.IsNullOrEmpty(Bed.InstanceID); } }

        public Clan(string id, string name)
        {
            ID = id;
            Name = name;
        }

        public class SortForBed : IComparer<Clan>
        {
            public int Compare(Clan x, Clan y)
            {
                if (x == null || y == null)
                {
                    return (x == null).CompareTo((y == null));
                }
                bool hasBedX = x.HasBed;
                bool hasBedY = y.HasBed;
                if (hasBedX != hasBedY)
                {
                    return hasBedX.CompareTo(hasBedY);
                }
                return x.ID.CompareTo(y.ID);
            }
        }
        #endregion
    }
}