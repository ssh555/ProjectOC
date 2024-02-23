using ML.Engine.FSM;
using ML.Engine.InventorySystem;
using ProjectOC.MissionNS;
using ProjectOC.ProNodeNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// 刁民
    /// </summary>
    [System.Serializable]
    public class Worker : MonoBehaviour
    {
        #region 策划配置项
        [LabelText("名字")]
        public string Name = "Worker";
        [LabelText("性别")]
        public Gender Gender = Gender.None;
        [LabelText("当前体力值")]
        public int APCurrent = 10;
        [LabelText("体力上限")]
        public int APMax = 10;
        [LabelText("体力工作阈值")]
        public int APWorkThreshold = 8;
        [LabelText("体力休息阈值")]
        public int APRelaxThreshold = 9;
        [LabelText("完成一次任务消耗的体力值")]
        public int APCost = 1;
        [LabelText("完成一次搬运消耗的体力值")]
        public int APCostTransport = 1;
        [LabelText("移动速度")]
        public float WalkSpeed = 10;
        [LabelText("当前负重")]
        public int BURCurrent
        {
            get
            {
                int result = 0;
                foreach (Item item in this.TransportItems)
                {
                    result += item.Weight;
                }
                return result;
            }
        }
        [LabelText("负重上限")]
        public int BURMax = 100;
        [LabelText("搬运经验值")]
        public int ExpTransport = 10;
        [LabelText("刁民类型")]
        public WorkType WorkType;
        public Dictionary<WorkType, string> SkillConfig = new Dictionary<WorkType, string>();
        [LabelText("技能")]
        public Dictionary<WorkType, Skill> Skill = new Dictionary<WorkType, Skill>();
        [LabelText("技能经验获取速度")]
        public Dictionary<WorkType, int> ExpRate = new Dictionary<WorkType, int>();
        [LabelText("职业效率加成")]
        public Dictionary<WorkType, int> Eff = new Dictionary<WorkType, int>();
        #endregion

        [LabelText("特性")]
        public List<Feature> Features = new List<Feature>();

        [LabelText("每个时段的安排")]
        public TimeArrangement TimeArrangement = new TimeArrangement();

        [LabelText("当前时段的安排应该所处的状态")]
        public TimeStatus CurTimeFrameStatus 
        { 
            get 
            {
                ManagerNS.DispatchTimeManager timeManager = ManagerNS.LocalGameManager.Instance.DispatchTimeManager;
                if (timeManager != null)
                {
                    return TimeArrangement[timeManager.CurrentTimeFrame];
                }
                //Debug.LogError("DispatchTimeManager is Null");
                return TimeStatus.None;
            } 
        }

        [LabelText("状态机控制器")]
        protected StateController StateController = null;
        
        [LabelText("状态机")]
        protected WorkerStateMachine StateMachine = null;

        public Status status;
        [LabelText("当前实际状态")]
        public Status Status
        {
            get { return status; }
            set 
            {
                status = value;
                StatusChangeAction?.Invoke(status);
            }
        }

        public Action<Status> StatusChangeAction;
        
        [LabelText("是否在值班")]
        public bool IsOnDuty { get { return this.ProNode != null && this.Status != Status.Relaxing && ArriveProNode; } }
        
        [LabelText("生产节点")]
        public ProNode ProNode = null;
        
        [LabelText("是否到达生产节点")]
        public bool ArriveProNode = false;
        
        [LabelText("搬运")]
        public Transport Transport = null;
        
        [LabelText("搬运物品")]
        public List<Item> TransportItems = new List<Item>();

        private NavMeshAgent Agent = null;
        public float Threshold = 2f;
        public Transform Target = null;
        private event Action<Worker> OnArrival;
        public bool HasArrived = false;
        public void Init()
        {
            this.ExpRate.Add(WorkType.None, 0);
            this.ExpRate.Add(WorkType.Cook, 100);
            this.ExpRate.Add(WorkType.HandCraft, 100);
            this.ExpRate.Add(WorkType.Industry, 100);
            this.ExpRate.Add(WorkType.Magic, 100);
            this.ExpRate.Add(WorkType.Transport, 100);
            this.ExpRate.Add(WorkType.Collect, 100);

            this.Eff.Add(WorkType.None, 0);
            this.Eff.Add(WorkType.Cook, 10);
            this.Eff.Add(WorkType.HandCraft, 10);
            this.Eff.Add(WorkType.Industry, 10);
            this.Eff.Add(WorkType.Magic, 10);
            this.Eff.Add(WorkType.Transport, 50);
            this.Eff.Add(WorkType.Collect, 10);

            this.Skill.Add(WorkType.None, new Skill());
            this.Skill.Add(WorkType.Cook, new Skill());
            this.Skill.Add(WorkType.HandCraft, new Skill());
            this.Skill.Add(WorkType.Industry, new Skill());
            this.Skill.Add(WorkType.Magic, new Skill());
            this.Skill.Add(WorkType.Transport, new Skill());
            this.Skill.Add(WorkType.Collect, new Skill());

            this.Features = ManagerNS.LocalGameManager.Instance.FeatureManager.CreateFeature();
            foreach (Feature feature in this.Features)
            {
                feature.ApplyFeature(this);
            }

            foreach (var kv in SkillConfig)
            {
                Skill skill = ManagerNS.LocalGameManager.Instance.SkillManager.SpawnSkill(kv.Value);
                if (skill != null)
                {
                    skill.ApplySkill(this);
                    this.Skill[kv.Key] = skill;
                }
                else
                {
                    //Debug.LogError($"Worker {Name} Skill {kv.Value} is Null");
                }
            }

            // 初始化状态机
            StateController = new StateController(0);
            StateMachine = new WorkerStateMachine(this);
            StateController.SetStateMachine(StateMachine);
        }

        public void Start()
        {
            Agent = GetComponent<NavMeshAgent>();
            this.Init();
            //this.enabled = false;
        }

        public void Update()
        {
            if (Target != null && !HasArrived && Vector3.Distance(transform.position, Target.position) < Threshold)
            {
                HasArrived = true;
                OnArrival?.Invoke(this);
            }
        }

        public bool SetDestination(Transform target, Action<Worker> action = null, bool isClearAction=true)
        {
            this.Target = target;
            if (Target != null)
            {
                if (Agent.SetDestination(Target.position))
                {
                    HasArrived = false;
                    if (isClearAction)
                    {
                        OnArrival = action;
                    }
                    else
                    {
                        OnArrival += action;
                    }
                    return true;
                }
            }
            return false;
        }

        public void ClearDestination()
        {
            Target = null;
            OnArrival = null;
            HasArrived = false;
        }

        public void ChangeProNode(ProNode proNode)
        {
            if (this.Transport != null)
            {
                this.Transport.End();
            }
            if (this.ProNode != null)
            {
                this.ProNode.RemoveWorker();
            }
            this.ProNode = proNode;
        }


        /// <summary>
        /// 修改经验值
        /// </summary>
        /// <param name="workType">经验类型</param>
        /// <param name="value">经验值</param>
        public void AlterExp(WorkType workType, int value)
        {
            this.Skill[workType].AlterExp(value * ExpRate[workType]);
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

        /// <summary>
        /// 结算搬运奖励
        /// </summary>
        public void SettleTransport()
        {
            this.AlterAP(-1 * APCostTransport);
            this.AlterExp(WorkType.Transport, ExpTransport);
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
