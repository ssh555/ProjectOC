using ML.Engine.FSM;
using ML.Engine.InventorySystem;
using ML.Engine.Timer;
using ProjectOC.MissionNS;
using ProjectOC.ProNodeNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using ML.PlayerCharacterNS;
using UnityEngine;
using UnityEngine.AI;
using ProjectOC.ManagerNS;

namespace ProjectOC.WorkerNS
{
    [LabelText("刁民"), System.Serializable]
    public class Worker : SerializedMonoBehaviour, ITickComponent,IAICharacter
    {
        public string InstanceID;
        #region 策划配置项
        [LabelText("名字"), FoldoutGroup("配置")]
        public string Name = "Worker";
        [LabelText("性别"), FoldoutGroup("配置")]
        public Gender Gender = Gender.Male;
        [LabelText("当前体力值"), ReadOnly]
        public int APCurrent = 10;
        [LabelText("体力上限"), FoldoutGroup("配置")]
        public int APMax = 10;
        [LabelText("体力工作阈值"), FoldoutGroup("配置")]
        public int APWorkThreshold = 8;
        [LabelText("体力休息阈值"), FoldoutGroup("配置")]
        public int APRelaxThreshold = 9;
        [LabelText("完成一次任务消耗的体力值"), FoldoutGroup("配置")]
        public int APCost = 1;
        [LabelText("完成一次搬运消耗的体力值"), FoldoutGroup("配置")]
        public int APCostTransport = 1;
        [LabelText("移动速度"), FoldoutGroup("配置")]
        public float WalkSpeed = 10;
        [LabelText("当前心情"), ReadOnly]
        public int Mood = 100;
        [LabelText("心情最大值"), FoldoutGroup("配置")]
        public int MoodMax = 100;
        [LabelText("超过DestroyTime分钟，未绑定窝的刁民会被销毁"), FoldoutGroup("配置")]
        public float DestroyTimeForNoHome = 0.2f;
        [LabelText("当前负重"), ShowInInspector, ReadOnly]
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
        [LabelText("负重上限"), FoldoutGroup("配置")]
        public int BURMax = 100;
        [LabelText("搬运经验值"), FoldoutGroup("配置")]
        public int ExpTransport = 10;
        [LabelText("刁民类型"), FoldoutGroup("配置")]
        public WorkType WorkType;
        [LabelText("技能配置"), FoldoutGroup("配置"), ShowInInspector]
        public Dictionary<WorkType, string> SkillConfig = new Dictionary<WorkType, string>();
        [LabelText("技能"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, Skill> Skill = new Dictionary<WorkType, Skill>();
        [LabelText("技能经验获取速度"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, int> ExpRate = new Dictionary<WorkType, int>();
        [LabelText("职业效率加成"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, int> Eff = new Dictionary<WorkType, int>();
        #endregion

        [LabelText("特性"), ReadOnly]
        public List<Feature> Features = new List<Feature>();

        [LabelText("每个时段的安排"), FoldoutGroup("配置")]
        public TimeArrangement TimeArrangement = new TimeArrangement();

        [LabelText("当前时段的安排应该所处的状态"), ShowInInspector, ReadOnly]
        public TimeStatus CurTimeFrameStatus 
        { 
            get 
            {
                ManagerNS.DispatchTimeManager timeManager = ManagerNS.LocalGameManager.Instance.DispatchTimeManager;
                if (timeManager != null)
                {
                    return TimeArrangement[timeManager.CurrentHour];
                }
                return TimeStatus.Relax;
            } 
        }

        [LabelText("状态机控制器"), ShowInInspector, ReadOnly]
        protected StateController StateController = null;
        
        [LabelText("状态机"), ShowInInspector, ReadOnly]
        protected WorkerStateMachine StateMachine = null;

        public Status status;
        [LabelText("当前实际状态"), ShowInInspector, ReadOnly]
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
        public Action<int> APChangeAction;
        
        [LabelText("是否在值班"), ShowInInspector, ReadOnly]
        public bool IsOnDuty { get { return HasProNode && this.Status != Status.Relaxing && ProNode.IsWorkerArrive; } }

        [LabelText("生产节点"), ReadOnly]
        public ProNode ProNode = null;
        [LabelText("是否有生产节点"), ShowInInspector, ReadOnly]
        public bool HasProNode { get => this.ProNode != null && !string.IsNullOrEmpty(this.ProNode.UID); }
        [ShowInInspector, ReadOnly]
        public Vector3 LastPosition;
        
        [LabelText("搬运"), ReadOnly]
        public Transport Transport = null;
        [LabelText("是否有搬运"), ShowInInspector, ReadOnly]
        public bool HasTransport { get => this.Transport != null && !string.IsNullOrEmpty(this.Transport.ItemID); }
        [LabelText("搬运状态"), ShowInInspector, ReadOnly]
        public TransportState TransportState { get => (HasTransport && this.Transport.CurNum > 0) ? TransportState.HoldingObjects : TransportState.EmptyHanded; }

        [LabelText("搬运物品"), ReadOnly]
        public List<Item> TransportItems = new List<Item>();
        [LabelText("餐厅"), ReadOnly]
        public RestaurantNS.Restaurant Restaurant; 
        [LabelText("是否有餐厅"), ShowInInspector, ReadOnly]
        public bool HasRestaurant { get => Restaurant != null && !string.IsNullOrEmpty(Restaurant.UID); }

        private Building.WorkerHome home;
        [LabelText("窝"), ReadOnly]
        public Building.WorkerHome Home
        {
            get { return home; }
            set
            {
                home = value;
                if (home == null)
                {
                    if (TimerForNoHome == null)
                    {
                        TimerForNoHome = new CounterDownTimer(60 * DestroyTimeForNoHome, false, false);
                        TimerForNoHome.OnEndEvent += () =>
                        {
                            LocalGameManager.Instance.WorkerManager.DeleteWorker(this);
                        };
                    }
                    TimerForNoHome?.Start();
                }
                else
                {
                    TimerForNoHome?.End();
                }
            }
        }
        private CounterDownTimer TimerForNoHome;
        [LabelText("是否有窝"), ShowInInspector, ReadOnly]
        public bool HasHome { get => Home != null && !string.IsNullOrEmpty(Home.UID); }

        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        #endregion
        public NavMeshAgent Agent = null;
        public float Threshold = 2f;
        [ReadOnly]
        public Vector3 Target;
        public event Action<Worker> OnArrival;
        private event Action<Worker> OnArrivalDisposable;
        public bool HasDestination = false;
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
            }

            // 初始化状态机
            StateController = new StateController(0);
            StateMachine = new WorkerStateMachine(this);
            StateController.SetStateMachine(StateMachine);

            Home = null;
        }
        private void Awake()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }
        private void Start()
        {
            Agent = GetComponent<NavMeshAgent>();
            this.Init();
            this.enabled = false;
        }

        public void Tick(float deltatime)
        {
            if (HasDestination && Vector3.Distance(transform.position, Target) < Threshold)
            {
                ClearDestination();
                OnArrivalDisposable?.Invoke(this);
                OnArrival?.Invoke(this);
            }
        }

        public bool SetDestination(Vector3 target, Action<Worker> action = null)
        {
            ClearDestination();
            Agent.isStopped = false;
            if (Agent.SetDestination(target))
            {
                Target = target;
                HasDestination = true;
                OnArrivalDisposable = action;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ClearDestination()
        {
            HasDestination = false;
            if (Agent != null && Agent.enabled)
            {
                Agent.isStopped = true;
            }
        }

        public void RecoverLastPosition()
        {
            if (!Agent.enabled)
            {
                transform.position = LastPosition;
                Agent.enabled = true;
            }
        }

        public void ChangeProNode(ProNode proNode)
        {
            if (HasTransport)
            {
                this.Transport.End();
            }
            if (HasProNode)
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
                this.APChangeAction?.Invoke(this.APCurrent);
                return true;
            }
            return false;
        }

        public bool AlterMood(int value)
        {
            int mood = Mood + value;
            if (mood >= 0 && mood <= MoodMax)
            {
                Mood = mood;
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


        public void OnDestroy()
        {
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
            if (HasTransport)
            {
                this.Transport?.End();
            }
            if (HasProNode)
            {
                this.ProNode?.RemoveWorker();
            }
        }

        public int prefabIndex { get; } = 0;
        public ICharacterState State { get; set; }
        public IController Controller { get; set; }
        public void OnSpawn(IController controller)
        {
        }

        public void OnDespose(IController controller)
        {

        }

        public class Sort : IComparer<Worker>
        {
            public WorkType WorkType;

            public int Compare(Worker x, Worker y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (y == null)
                {
                    return -1;
                }
                int stateX = (x.HasProNode || x.HasTransport) ? 1 : 0;
                int stateY = (y.HasProNode || y.HasTransport) ? 1 : 0;
                if (stateX != stateY)
                {
                    return stateX.CompareTo(stateY);
                }
                int levelX = x.Skill[WorkType].Level;
                int levelY = y.Skill[WorkType].Level;
                if (levelX != levelY)
                {
                    return levelY.CompareTo(levelX);
                }
                return x.InstanceID.CompareTo(y.InstanceID);
            }
        }
    }
}
