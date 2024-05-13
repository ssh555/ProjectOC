using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [LabelText("技能"), System.Serializable]
    public struct Skill
    {
        [LabelText("技能类型"), ReadOnly]
        public SkillType SkillType;
        [LabelText("提供的工作效率"), ReadOnly]
        public int Eff;
        [LabelText("技能学习速度"), ReadOnly]
        public float ExpRate;
        [LabelText("当前经验值"), ReadOnly]
        public int Exp;
        [LabelText("技能等级"), ReadOnly]
        public int LevelCurrent;
        
        public Skill(SkillType type, int level=0)
        {
            SkillType = type;
            ExpRate = 1;
            Eff = ManagerNS.LocalGameManager.Instance.WorkerManager.Config.SkillEff;
            Eff = type != SkillType.Transport ? Eff : Eff / 10;
            Exp = 0;
            LevelCurrent = 0;
            ChangeLevel(level);
        }
        public void AlterEff(int value)
        {
            Eff += value;
        }
        public void AlterExpRate(int value)
        {
            ExpRate += value;
        }
        public void AlterExp(int value)
        {
            int cur = (int)(Exp + value * ExpRate);
            cur = cur >= 0 ? cur : 0;
            cur = cur <= 10000 ? cur : 10000;
            Exp = cur;
            List<int> ExpToLevel = ManagerNS.LocalGameManager.Instance.WorkerManager.Config.ExpToLevel;
            cur = 0;
            for (int i = 0; i < ExpToLevel.Count; i++)
            {
                if (Exp >= ExpToLevel[i])
                {
                    cur = i + 1;
                }
                else
                {
                    break;
                }
            }
            LevelCurrent = cur;
        }
        public void ChangeLevel(int level)
        {
            if (0 <= level && level <= 10)
            {
                Exp = level > 0 ? ManagerNS.LocalGameManager.Instance.WorkerManager.Config.ExpToLevel[level - 1] : 0;
                LevelCurrent = level;
            }
        }
        public int GetEff()
        {
            List<int> LevelToEff = SkillType != SkillType.Transport ? 
                ManagerNS.LocalGameManager.Instance.WorkerManager.Config.LevelToEff_Duty : 
                ManagerNS.LocalGameManager.Instance.WorkerManager.Config.LevelToEff_Transport;
            return Eff + LevelToEff[LevelCurrent];
        }
    }
}

