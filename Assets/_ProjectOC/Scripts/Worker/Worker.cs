using ML.Engine.FSM;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.MissionNS;
using ProjectOC.ProductionNodeNS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// 刁民
    /// </summary>
    [System.Serializable]
    public class Worker : MonoBehaviour
    {
        /// <summary>
        /// 刁民ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// 名字，TODO:随机生成
        /// </summary>
        public string Name = "";
        /// <summary>
        /// 职业类型
        /// </summary>
        public WorkType WorkType;
        /// <summary>
        /// 当前体力值
        /// </summary>
        public int APCurrent = 10;
        /// <summary>
        /// 体力上限
        /// </summary>
        public int APMax = 10;
        /// <summary>
        /// 体力工作阈值，判断工作状态刁民是否需要强制休息的阈值
        /// </summary>
        public int APWorkThreshold = 3;
        /// <summary>
        /// 体力休息阈值，判断休息状态刁民是否需要主动进餐的阈值
        /// </summary>
        public int APRelaxThreshold = 5;
        /// <summary>
        /// 刁民完成一项任务消耗的体力值
        /// </summary>
        public int APCost = 1;
        /// <summary>
        /// 每次搬运结算消耗的体力值
        /// </summary>
        public int APCostTransport = 1;
        /// <summary>
        /// 当前负重
        /// </summary>
        public int BURCurrent = 0;
        /// <summary>
        /// 负重上限
        /// </summary>
        public int BURMax = 100;
        /// <summary>
        /// 技能
        /// </summary>
        public Dictionary<WorkType, Skill> Skills = new Dictionary<WorkType, Skill>();

        #region 不进表
        /// <summary>
        /// 移动速度，单位为 m/s
        /// </summary>
        public float WalkSpeed = 10;
        /// <summary>
        /// 职业经验获取速度，单位为%
        /// </summary>
        public Dictionary<WorkType, int> ExpRate = new Dictionary<WorkType, int>();
        /// <summary>
        /// 职业效率加成，单位为%
        /// </summary>
        public Dictionary<WorkType, int> Eff = new Dictionary<WorkType, int>();
        /// <summary>
        /// 特性
        /// </summary>
        public List<Feature> Features = new List<Feature>();

        /// <summary>
        /// 每个时段的安排
        /// </summary>
        protected TimeArrangement TimeArrangement;
        /// <summary>
        /// 当前时段的安排应该所处的状态
        /// </summary>
        public TimeStatus CurTimeFrameStatus 
        { 
            get 
            {
                DispatchTimeManager timeManager = GameManager.Instance.GetLocalManager<DispatchTimeManager>();
                if (timeManager != null)
                {
                    return TimeArrangement[timeManager.CurrentTimeFrame];
                }
                return TimeStatus.None;
            } 
        }
        /// <summary>
        /// 当前实际状态
        /// </summary>
        public Status Status;
        /// <summary>
        /// 状态机控制器
        /// </summary>
        protected StateController StateController;
        /// <summary>
        /// 状态机
        /// </summary>
        protected WorkerStateMachine StateMachine;
        /// <summary>
        /// 是否在值班
        /// </summary>
        public bool IsOnDuty;
        public ProductionNode DutyProductionNode;
        public MissionTransport MissionTransport;
        #endregion
        public Worker()
        {
            this.ExpRate.Add(WorkType.Cook, 100);
            this.ExpRate.Add(WorkType.HandCraft, 100);
            this.ExpRate.Add(WorkType.Industry, 100);
            this.ExpRate.Add(WorkType.Science, 100);
            this.ExpRate.Add(WorkType.Magic, 100);
            this.ExpRate.Add(WorkType.Transport, 100);

            this.Eff.Add(WorkType.Cook, 10);
            this.Eff.Add(WorkType.HandCraft, 10);
            this.Eff.Add(WorkType.Industry, 10);
            this.Eff.Add(WorkType.Science, 10);
            this.Eff.Add(WorkType.Magic, 10);
            this.Eff.Add(WorkType.Transport, 50);

            this.Features = FeatureManager.Instance.CreateFeatureForWorker();
            foreach (Feature feature in this.Features)
            {
                feature.ApplyEffectToWorker(this);
            }
            // 初始化状态机
            StateController = new StateController(0);
            StateMachine = new WorkerStateMachine();
            StateController.SetStateMachine(StateMachine);
        }
        public void Init(WorkerManager.WorkerTableJsonData config)
        {
            this.ID = config.id;
            this.Name = config.name;
            this.WorkType = config.workType;
            this.APCurrent = config.apMax;
            this.APMax = config.apMax;
            this.APWorkThreshold = config.apWorkThreshold;
            this.APRelaxThreshold = config.apRelaxThreshold;
            this.APCost = config.apCost;
            this.APCostTransport = config.apCostTransport;
            this.BURMax = config.burMax;
            foreach (var kv in config.skillDict)
            {
                Skill skill = SkillManager.Instance.SpawnSkill(kv.Value);
                if (skill != null)
                {
                    skill.ApplyEffectToWorker(this);
                    this.Skills.Add(kv.Key, skill);
                }
            }
        }
        public void Init(Worker worker)
        {
            this.ID = worker.ID;
            this.Name = worker.Name;
            this.WorkType = worker.WorkType;
            this.APCurrent = worker.APCurrent;
            this.APMax = worker.APMax;
            this.APWorkThreshold = worker.APWorkThreshold;
            this.APRelaxThreshold = worker.APRelaxThreshold;
            this.APCost = worker.APCost;
            this.APCostTransport = worker.APCostTransport;
            this.WalkSpeed = worker.WalkSpeed;
            this.BURCurrent = worker.BURCurrent;
            this.BURMax = worker.BURMax;
            this.ExpRate = new Dictionary<WorkType, int>(worker.ExpRate);
            this.Eff = new Dictionary<WorkType, int>(worker.Eff);
            foreach (Feature feature in this.Features)
            {
                feature.RemoveEffectToWorker(this);
            }
            this.Features.Clear();
            foreach (Feature feature in worker.Features)
            {
                Feature featureNew = new Feature();
                featureNew.Init(feature);
                featureNew.ApplyEffectToWorker(this);
                this.Features.Add(featureNew);
            }
            foreach (Skill skill in this.Skills.Values)
            {
                skill.RemoveEffectToWorker(this);
            }
            this.Skills.Clear();
            foreach (var kv in worker.Skills)
            {
                Skill skill = new Skill();
                skill.Init(kv.Value);
                skill.ApplyEffectToWorker(this);
                this.Skills.Add(kv.Key, skill);
            }
            this.TimeArrangement.SetTimeArrangement(worker.TimeArrangement);
        }

        /// <summary>
        /// 修改经验值
        /// </summary>
        /// <param name="workType">经验类型</param>
        /// <param name="value">经验值</param>
        public void AlterExp(WorkType workType, int value)
        {
            this.Skills[workType].AlterExp(value);
        }
        /// <summary>
        /// 修改体力值
        /// </summary>
        /// <param name="value">体力值</param>
        public bool AlterAP(int value)
        {
            int ap = this.APCurrent + value;
            if (ap >= 0 && ap <= this.APMax)
            {
                this.APCurrent = ap;
                return true;
            }
            return false;
        }
        public void SetTimeStatus(int time, TimeStatus timeStatus)
        {
            this.TimeArrangement[time] = timeStatus;
        }
        public void SetTimeArrangement(Worker worker)
        {
            if (worker != null)
            {
                this.TimeArrangement.SetTimeArrangement(worker.TimeArrangement);
            }
        }
        public void SetTimeStatusAll(TimeStatus timeStatus)
        {
            this.TimeArrangement.SetTimeStatusAll(timeStatus);
        }
    }
}
