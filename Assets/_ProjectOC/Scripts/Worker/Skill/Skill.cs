using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [LabelText("����"), System.Serializable]
    public struct Skill
    {
        [LabelText("��������"), ReadOnly]
        public SkillType SkillType;
        [LabelText("�ṩ�Ĺ���Ч��"), ReadOnly]
        public int Eff;
        [LabelText("����ѧϰ�ٶ�"), ReadOnly]
        public float ExpRate;
        [LabelText("��ǰ����ֵ"), ReadOnly]
        public int Exp;
        [LabelText("���ܵȼ�"), ReadOnly]
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

