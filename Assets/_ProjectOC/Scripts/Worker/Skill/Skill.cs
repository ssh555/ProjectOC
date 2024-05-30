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
        public int ExpRate;
        [LabelText("当前经验值"), ReadOnly]
        public int Exp;
        [LabelText("技能等级"), ReadOnly]
        public int LevelCurrent;
        
        public Skill(SkillType type, int level=0)
        {
            SkillType = type;
            ExpRate = 100;
            Eff = ManagerNS.LocalGameManager.Instance.WorkerManager.Config.SkillEff;
            Eff = type != SkillType.Transport ? Eff : Eff / 10;
            Exp = 0;
            LevelCurrent = 0;
            ChangeLevel(level);
        }
        public Skill AlterEff(int value)
        {
            Eff += value;
            return this;
        }
        public Skill AlterExpRate(int value)
        {
            ExpRate += value;
            return this;
        }
        public Skill AlterExp(int value)
        {
            int cur = (int)(Exp + value * ExpRate / 100f);
            cur = cur >= 0 ? cur : 0;
            List<int> expToLevel = ManagerNS.LocalGameManager.Instance.WorkerManager.Config.ExpToLevel;
            Exp = System.Math.Min(cur, expToLevel[expToLevel.Count - 1]);
            cur = 0;
            for (int i = 0; i < expToLevel.Count; i++)
            {
                if (Exp >= expToLevel[i]) { cur = i + 1; }
                else { break; }
            }
            LevelCurrent = cur;
            return this;
        }
        public Skill ChangeLevel(int level)
        {
            List<int> expToLevel = ManagerNS.LocalGameManager.Instance.WorkerManager.Config.ExpToLevel;
            if (0 <= level && level <= expToLevel.Count)
            {
                Exp = level > 0 ? expToLevel[level - 1] : 0;
                LevelCurrent = level;
            }
            return this;
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

