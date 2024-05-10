using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [LabelText("隐兽配置数据"), System.Serializable]
    public struct WorkerConfig
    {
        [LabelText("体力上限")]
        public int APMax;
        [LabelText("体力工作阈值")]
        public int APWorkThreshold;
        [LabelText("体力休息阈值")]
        public int APRelaxThreshold;
        [LabelText("体力消耗_搬运")]
        public int APCost_Transport;
        [LabelText("进食时间(s)")]
        public float EatTime;
        [LabelText("心情值上限")]
        public int EMMax;
        [LabelText("低心情阈值")]
        public int EMLowThreshold;
        [LabelText("高心情阈值")]
        public int EMHighThreshold;
        [LabelText("低心情效果")]
        public int EMLowEffect;
        [LabelText("高心情效果")]
        public int EMHighEffect;
        [LabelText("心情消耗量")]
        public int EMCost;
        [LabelText("心情恢复量")]
        public int EMRecover;
        [LabelText("移动速度(m/s)")]
        public float WalkSpeed;
        [LabelText("负重上限")]
        public int BURMax;
        [LabelText("搬运经验值")]
        public int ExpTransport;
        [LabelText("技能初始工作效率(%)")]
        public int SkillEff;
        [LabelText("经验等级映射关系")]
        public List<int> ExpToLevel;
        [LabelText("等级效率映射_值班")]
        public List<int> LevelToEff_Duty;
        [LabelText("等级效率映射_搬运")]
        public List<int> LevelToEff_Transport;
        [LabelText("未绑定窝销毁时间(s)")]
        public float DestroyTimeForNoHome;
        [LabelText("技能等级分布均值")]
        public float SkillStdMean;
        [LabelText("技能等级分布方差")]
        public float SkillStdDev;
        [LabelText("技能等级分布上界")]
        public int SkillStdHighBound;
        [LabelText("技能等级分布下界")]
        public int SkillStdLowBound;

        public WorkerConfig(WorkerConfig config)
        {
            APMax = config.APMax;
            APWorkThreshold = config.APWorkThreshold;
            APRelaxThreshold = config.APRelaxThreshold;
            APCost_Transport = config.APCost_Transport;
            EatTime = config.EatTime;
            EMMax = config.EMMax;
            EMLowThreshold = config.EMLowThreshold;
            EMHighThreshold = config.EMHighThreshold;
            EMLowEffect = config.EMLowEffect;
            EMHighEffect = config.EMHighEffect;
            EMCost = config.EMCost;
            EMRecover = config.EMRecover;
            WalkSpeed = config.WalkSpeed;
            BURMax = config.BURMax;
            ExpTransport = config.ExpTransport;
            SkillEff = config.SkillEff;
            ExpToLevel = new List<int>();
            ExpToLevel.AddRange(config.ExpToLevel);
            LevelToEff_Duty = new List<int>();
            LevelToEff_Duty.AddRange(config.LevelToEff_Duty);
            LevelToEff_Transport = new List<int>();
            LevelToEff_Transport.AddRange(config.LevelToEff_Transport);
            DestroyTimeForNoHome = config.DestroyTimeForNoHome;
            SkillStdMean = config.SkillStdMean;
            SkillStdDev = config.SkillStdDev;
            SkillStdHighBound = config.SkillStdHighBound;
            SkillStdLowBound = config.SkillStdLowBound;
        }
    }
}