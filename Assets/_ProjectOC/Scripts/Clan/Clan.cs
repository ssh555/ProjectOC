using ML.Engine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ClanNS
{
    [LabelText("����"), System.Serializable]
    public class Clan : NPC
    {
        [LabelText("��֢�׶�"), ReadOnly]
        public int Level;
        #region Talent
        [LabelText("ѧʶ֪��"), ReadOnly]
        public int Wisdom;
        [LabelText("ս������"), ReadOnly]
        public int Combat;
        [LabelText("��������"), ReadOnly]
        public int Resilience;
        #endregion

        #region Personality
        [LabelText("�Ը�"), ReadOnly, ShowInInspector]
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
            // TODO: ��ʼ��Wisdom��Combat��Resilience
            // TODO: ��ʼ��PersonalityDict
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
                                        new UIManager.SideBarUIData($"�����Ա{Name}��������TAG{tag.Name}", null, null));
                return true;
            }
            return false;
        }
        public override void RemoveTAG(PersonalityTAG tag)
        {
            TAGS.Remove(tag);
            ML.Engine.Manager.GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI,
                                        new UIManager.SideBarUIData($"�����Ա{Name}ʧȥ����TAG{tag.Name}", null, null));
        }

        #region Bed
        [LabelText("ӵ�еĴ�"), ReadOnly, System.NonSerialized]
        public ClanBed Bed;
        [LabelText("�Ƿ�ӵ�д�"), ShowInInspector, ReadOnly]
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