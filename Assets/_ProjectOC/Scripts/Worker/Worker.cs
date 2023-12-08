using ML.Engine.FSM;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.MissionNS;
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
        /// 名字，随机生成
        /// </summary>
        public string Name = "";
        /// <summary>
        /// 职业类型
        /// </summary>
        public WorkType WorkType;
        /// <summary>
        /// 当前体力值
        /// </summary>
        public int APCurrent;
        /// <summary>
        /// 体力上限
        /// </summary>
        public int APMax;
        /// <summary>
        /// 体力工作阈值，判断工作状态刁民是否需要强制休息的阈值
        /// </summary>
        public int APWorkThreshold;
        /// <summary>
        /// 体力休息阈值，判断休息状态刁民是否需要主动进餐的阈值
        /// </summary>
        public int APRelaxThreshold;
        /// <summary>
        /// 刁民完成一项任务消耗的体力值
        /// </summary>
        public int APCost;
        /// <summary>
        /// 每次搬运结算消耗的体力值
        /// </summary>
        public int APCostTransport;
        /// <summary>
        /// 移动速度，单位为 m/s
        /// </summary>
        public float WalkSpeed;
        /// <summary>
        /// 当前负重
        /// </summary>
        public int BURCurrent;
        /// <summary>
        /// 负重上限
        /// </summary>
        public int BURMax;
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
        /// 技能
        /// </summary>
        public Dictionary<WorkType, WorkerAbility> Skills = new Dictionary<WorkType, WorkerAbility>();


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
                return TimeArrangement[GameManager.Instance.GetLocalManager<DispatchTimeManager>().CurrentTimeFrame];
            } 
        }
        /// <summary>
        /// 当前实际状态
        /// </summary>
        private Status status;
        /// <summary>
        /// 当前实际状态
        /// </summary>
        public Status Status
        {
            get => status;
            set
            {
                GameManager.Instance.GetLocalManager<MissionBroadCastManager>().UpdateWorkerStatusList(this, status, value);
                status = value;
            }
        }
        /// <summary>
        /// 是否在值班
        /// </summary>
        public bool IsOnDuty;
        /// <summary>
        /// 状态机控制器
        /// </summary>
        protected StateController stateController;
        /// <summary>
        /// 状态机
        /// </summary>
        protected WorkerStateMachine stateMachine;


        public Worker()
        {
            // TODO: 策划配置初始数值
            this.Name = "";
            this.WorkType = WorkType.Transport;
            this.APCurrent = 10;
            this.APMax = 10;
            this.APWorkThreshold = 0;
            this.APRelaxThreshold = 3;
            this.APCost = 1;
            this.APCostTransport = 1;
            this.WalkSpeed = 10;
            this.BURCurrent = 0;
            this.BURMax = 10;
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

            FeatureManager featureManager = GameManager.Instance.GetLocalManager<FeatureManager>();
            if (featureManager != null)
            {
                this.Features = featureManager.CreateFeatureForWorker();
            }
            foreach (Feature feature in this.Features)
            {
                feature.ApplyEffectToWorker(this);
            }
            this.Skills.Add(WorkType.Cook, new WorkerAbility(""));
            this.Skills.Add(WorkType.HandCraft, new WorkerAbility(""));
            this.Skills.Add(WorkType.Industry, new WorkerAbility(""));
            this.Skills.Add(WorkType.Science, new WorkerAbility(""));
            this.Skills.Add(WorkType.Magic, new WorkerAbility(""));
            this.Skills.Add(WorkType.Transport, new WorkerAbility(""));
            // TODO:从表里拿skill id创建skill
            foreach (WorkerAbility skill in this.Skills.Values)
            {
                skill.ApplyEffectToWorker(this);
            }

            // 初始化状态机
            stateController = new StateController(0);
            stateMachine = new WorkerStateMachine();
            stateController.SetStateMachine(stateMachine);
        }
        /// <summary>
        /// 修改经验值
        /// </summary>
        /// <param name="workType">经验类型</param>
        /// <param name="value">经验值</param>
        public void AlterExp(WorkType workType, int value)
        {
            this.Skills[workType].AlterExp(value);
            // TODO:技能升级时效果变化
        }

        /// <summary>
        /// 修改体力值
        /// </summary>
        /// <param name="value">体力值</param>
        public void AlterAP(int value)
        {

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
