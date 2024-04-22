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
    [LabelText("����"), System.Serializable]
    public class Worker : SerializedMonoBehaviour, ITickComponent,IAICharacter
    {
        public string InstanceID;
        #region �߻�������
        [LabelText("����"), FoldoutGroup("����")]
        public string Name = "Worker";
        [LabelText("�Ա�"), FoldoutGroup("����")]
        public Gender Gender = Gender.Male;
        [LabelText("��ǰ����ֵ"), ReadOnly]
        public int APCurrent = 10;
        [LabelText("��������"), FoldoutGroup("����")]
        public int APMax = 10;
        [LabelText("����������ֵ"), FoldoutGroup("����")]
        public int APWorkThreshold = 8;
        [LabelText("������Ϣ��ֵ"), FoldoutGroup("����")]
        public int APRelaxThreshold = 9;
        [LabelText("���һ���������ĵ�����ֵ"), FoldoutGroup("����")]
        public int APCost = 1;
        [LabelText("���һ�ΰ������ĵ�����ֵ"), FoldoutGroup("����")]
        public int APCostTransport = 1;
        [LabelText("�ƶ��ٶ�"), FoldoutGroup("����")]
        public float WalkSpeed = 10;
        [LabelText("��ǰ����"), ReadOnly]
        public int Mood = 100;
        [LabelText("�������ֵ"), FoldoutGroup("����")]
        public int MoodMax = 100;
        [LabelText("����DestroyTime���ӣ�δ���ѵĵ���ᱻ����"), FoldoutGroup("����")]
        public float DestroyTimeForNoHome = 0.2f;
        [LabelText("��ǰ����"), ShowInInspector, ReadOnly]
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
        [LabelText("��������"), FoldoutGroup("����")]
        public int BURMax = 100;
        [LabelText("���˾���ֵ"), FoldoutGroup("����")]
        public int ExpTransport = 10;
        [LabelText("��������"), FoldoutGroup("����")]
        public WorkType WorkType;
        [LabelText("��������"), FoldoutGroup("����"), ShowInInspector]
        public Dictionary<WorkType, string> SkillConfig = new Dictionary<WorkType, string>();
        [LabelText("����"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, Skill> Skill = new Dictionary<WorkType, Skill>();
        [LabelText("���ܾ����ȡ�ٶ�"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, int> ExpRate = new Dictionary<WorkType, int>();
        [LabelText("ְҵЧ�ʼӳ�"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, int> Eff = new Dictionary<WorkType, int>();
        #endregion

        [LabelText("����"), ReadOnly]
        public List<Feature> Features = new List<Feature>();

        [LabelText("ÿ��ʱ�εİ���"), FoldoutGroup("����")]
        public TimeArrangement TimeArrangement = new TimeArrangement();

        [LabelText("��ǰʱ�εİ���Ӧ��������״̬"), ShowInInspector, ReadOnly]
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

        [LabelText("״̬��������"), ShowInInspector, ReadOnly]
        protected StateController StateController = null;
        
        [LabelText("״̬��"), ShowInInspector, ReadOnly]
        protected WorkerStateMachine StateMachine = null;

        public Status status;
        [LabelText("��ǰʵ��״̬"), ShowInInspector, ReadOnly]
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
        
        [LabelText("�Ƿ���ֵ��"), ShowInInspector, ReadOnly]
        public bool IsOnDuty { get { return HasProNode && this.Status != Status.Relaxing && ProNode.IsWorkerArrive; } }

        [LabelText("�����ڵ�"), ReadOnly]
        public ProNode ProNode = null;
        [LabelText("�Ƿ��������ڵ�"), ShowInInspector, ReadOnly]
        public bool HasProNode { get => this.ProNode != null && !string.IsNullOrEmpty(this.ProNode.UID); }
        [ShowInInspector, ReadOnly]
        public Vector3 LastPosition;
        
        [LabelText("����"), ReadOnly]
        public Transport Transport = null;
        [LabelText("�Ƿ��а���"), ShowInInspector, ReadOnly]
        public bool HasTransport { get => this.Transport != null && !string.IsNullOrEmpty(this.Transport.ItemID); }
        [LabelText("����״̬"), ShowInInspector, ReadOnly]
        public TransportState TransportState { get => (HasTransport && this.Transport.CurNum > 0) ? TransportState.HoldingObjects : TransportState.EmptyHanded; }

        [LabelText("������Ʒ"), ReadOnly]
        public List<Item> TransportItems = new List<Item>();
        [LabelText("����"), ReadOnly]
        public RestaurantNS.Restaurant Restaurant; 
        [LabelText("�Ƿ��в���"), ShowInInspector, ReadOnly]
        public bool HasRestaurant { get => Restaurant != null && !string.IsNullOrEmpty(Restaurant.UID); }

        private Building.WorkerHome home;
        [LabelText("��"), ReadOnly]
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
        [LabelText("�Ƿ�����"), ShowInInspector, ReadOnly]
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

            // ��ʼ��״̬��
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
        /// �޸ľ���ֵ
        /// </summary>
        /// <param name="workType">��������</param>
        /// <param name="value">����ֵ</param>
        public void AlterExp(WorkType workType, int value)
        {
            this.Skill[workType].AlterExp(value * ExpRate[workType]);
        }

        /// <summary>
        /// �޸�����ֵ
        /// </summary>
        /// <param name="value">����ֵ</param>
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
        /// ������˽���
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
