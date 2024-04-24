using ML.Engine.FSM;
using ML.Engine.InventorySystem;
using ML.Engine.Timer;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace ProjectOC.WorkerNS
{
    [LabelText("����"), Serializable]
    public class Worker : SerializedMonoBehaviour, ITickComponent, ML.PlayerCharacterNS.IAICharacter
    {
        #region Config
        [LabelText("����"), FoldoutGroup("����")]
        public string Name = "Worker";
        [LabelText("��������"), FoldoutGroup("����")]
        public WorkType WorkType;
        [LabelText("�Ա�"), FoldoutGroup("����")]
        public Gender Gender = Gender.Male;
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
        [LabelText("�������ֵ"), FoldoutGroup("����")]
        public int MoodMax = 100;
        [LabelText("������ֵ"), FoldoutGroup("����")]
        public int MoodThreshold = 50;
        [LabelText("���һ���������ĵ�����ֵ"), FoldoutGroup("����")]
        public int MoodCost = 10;
        [LabelText("��������"), FoldoutGroup("����")]
        public int WeightMax = 100;
        [LabelText("���˾���ֵ"), FoldoutGroup("����")]
        public int ExpTransport = 10;
        [LabelText("�ƶ��ٶ�"), FoldoutGroup("����")]
        public float WalkSpeed = 10;
        [LabelText("����DestroyTime���ӣ�δ���ѵĵ���ᱻ����"), FoldoutGroup("����")]
        public float DestroyTimeForNoHome = 1f;
        [LabelText("��������"), FoldoutGroup("����"), ShowInInspector]
        public Dictionary<WorkType, string> SkillConfig = new Dictionary<WorkType, string>();
        #endregion

        #region Data
        [LabelText("ʵ��ID"), ReadOnly]
        public string InstanceID;
        [LabelText("��ǰ����ֵ"), ReadOnly]
        public int APCurrent = 10;
        [LabelText("��ǰ����"), ReadOnly]
        public int Mood = 100;

        [LabelText("����"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, Skill> Skill = new Dictionary<WorkType, Skill>();
        [LabelText("���ܾ����ȡ�ٶ�"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, int> ExpRate = new Dictionary<WorkType, int>();
        [LabelText("ְҵЧ�ʼӳ�"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, int> Eff = new Dictionary<WorkType, int>();
        [LabelText("����"), ReadOnly]
        public List<Feature> Features = new List<Feature>();
        [LabelText("ÿ��ʱ�εİ���")]
        public TimeArrangement TimeArrangement = new TimeArrangement();
        #endregion

        #region Property
        [LabelText("��ǰ����"), ShowInInspector, ReadOnly]
        public int WeightCurrent
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
        [LabelText("��ǰʱ�εİ���"), ShowInInspector, ReadOnly]
        public TimeStatus CurTimeFrameStatus
        {
            get
            {
                ManagerNS.DispatchTimeManager timeManager = ManagerNS.LocalGameManager.Instance?.DispatchTimeManager;
                if (timeManager != null)
                {
                    return TimeArrangement[timeManager.CurrentHour];
                }
                return TimeStatus.Relax;
            }
        }

        public Status status;
        [LabelText("��ǰʵ��״̬"), ShowInInspector, ReadOnly]
        public Status Status
        {
            get { return status; }
            set
            {
                status = value;
                OnStatusChangeEvent?.Invoke(status);
            }
        }
        [LabelText("�Ƿ���ֵ��"), ShowInInspector, ReadOnly]
        public bool IsOnDuty { get { return HasProNode && Status != Status.Relaxing && WorkPlace.IsArrive; } }
        [LabelText("�����ڵ�"), ShowInInspector, ReadOnly]
        public ProNodeNS.ProNode ProNode => WorkPlace!= null ? WorkPlace as ProNodeNS.ProNode : null;
        [LabelText("�Ƿ��������ڵ�"), ShowInInspector, ReadOnly]
        public bool HasProNode => HasContainer(WorkerContainerType.Work);
        [LabelText("�Ƿ��в���"), ShowInInspector, ReadOnly]
        public bool HasRestaurant => HasContainer(WorkerContainerType.Relax);
        public ItemIcon WorldIcon { get => GetComponentInChildren<ItemIcon>(); }
        #endregion

        #region StateMachine
        [LabelText("״̬��������"), ShowInInspector, ReadOnly]
        protected StateController StateController = null;
        [LabelText("״̬��"), ShowInInspector, ReadOnly]
        protected WorkerStateMachine StateMachine = null;
        #endregion

        #region Event
        public Action<Status> OnStatusChangeEvent;
        public Action<int> OnAPChangeEvent;
        #endregion
       
        [ShowInInspector, ReadOnly]
        public Vector3 LastPosition;
        
        [LabelText("����"), ReadOnly]
        public MissionNS.Transport Transport = null;
        [LabelText("�Ƿ��а���"), ShowInInspector, ReadOnly]
        public bool HasTransport { get => Transport != null && !string.IsNullOrEmpty(Transport.ItemID); }
       
        [LabelText("������Ʒ"), ReadOnly]
        public List<Item> TransportItems = new List<Item>();
        

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
                            ManagerNS.LocalGameManager.Instance.WorkerManager.DeleteWorker(this);
                        };
                        TimerForNoHome?.Start();
                    }
                }
                else
                {
                    TimerForNoHome?.End();
                }
            }
        }
        private CounterDownTimer TimerForNoHome;
        [LabelText("�Ƿ�����"), ShowInInspector, ReadOnly]
        public bool HasHome => HasContainer(WorkerContainerType.Home);


        #region Container
        public IWorkerContainer WorkPlace;
        public IWorkerContainer RelaxPlace;
        public IWorkerContainer HomePlace;
        public Dictionary<WorkerContainerType, IWorkerContainer> ContainerDict;

        public IWorkerContainer GetContainer(WorkerContainerType type)
        {
            return ContainerDict[type];
        }

        public void SetContainer(IWorkerContainer container)
        {
            if (HasTransport && container.GetContainerType() == WorkerContainerType.Work)
            {
                Transport.End();
            }
            ContainerDict[container.GetContainerType()] = container;
        }

        public void RemoveContainer(WorkerContainerType type)
        {
            ContainerDict[type] = null;
        }

        public bool HasContainer(WorkerContainerType type)
        {
            IWorkerContainer container = ContainerDict[type];
            return container != null && !string.IsNullOrEmpty(container.GetUID());
        }
        #endregion

        #region NavMesh
        public NavMeshAgent Agent = null;
        public float Threshold = 2f;
        public Vector3 Target;
        public event Action<Worker> OnArrivalEvent;
        private event Action<Worker> OnArrivalDisposableEvent;
        public bool HasDestination = false;

        public bool SetDestination(Vector3 target, Action<Worker> action = null)
        {
            ClearDestination();
            Agent.isStopped = false;
            if (Agent.SetDestination(target))
            {
                Target = target;
                HasDestination = true;
                OnArrivalDisposableEvent = action;
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
        #endregion

        #region Mono
        private void Start()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            Agent = GetComponent<NavMeshAgent>();
            this.Init();
            this.enabled = false;
        }
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

            ContainerDict = new Dictionary<WorkerContainerType, IWorkerContainer>()
            {
                { WorkerContainerType.Work, WorkPlace },
                { WorkerContainerType.Relax, RelaxPlace },
                { WorkerContainerType.Home, HomePlace }
            };

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

            if (!HasHome)
            {
                Home = null;
            }

            ItemManager.Instance.AddItemIconObject("", this.transform, new Vector3(0, this.transform.GetComponent<CapsuleCollider>().height * 1.5f, 0),
                                        Quaternion.Euler(Vector3.zero), Vector3.one,
                                        (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
        }
        public void OnDestroy()
        {
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
            if (HasTransport)
            {
                this.Transport?.End();
            }
            foreach (IWorkerContainer container in ContainerDict.Values)
            {
                container?.RemoveWorker();
            }
        }
        #endregion

        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public void Tick(float deltatime)
        {
            if (HasDestination && Vector3.Distance(transform.position, Target) < Threshold)
            {
                ClearDestination();
                OnArrivalDisposableEvent?.Invoke(this);
                OnArrivalEvent?.Invoke(this);
            }
            bool lowMood = Mood < MoodThreshold;
            bool inSeq = ManagerNS.LocalGameManager.Instance.RestaurantManager.ContainWorker(this);
            string icon = lowMood ? "LowMood" : "";
            icon = inSeq ? "LowAP" : icon;
            icon = lowMood && inSeq ? "LowAPMood" : icon;
            WorldIcon?.SetSprite(ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(icon));
        }
        #endregion

        public void AlterExp(WorkType workType, int value)
        {
            this.Skill[workType].AlterExp(value * ExpRate[workType]);
        }

        public void AlterAP(int value)
        {
            APCurrent += value;
            APCurrent = APCurrent < 0 ? 0 : APCurrent;
            APCurrent = APCurrent > APMax ? APMax : APCurrent;
            OnAPChangeEvent?.Invoke(APCurrent);
        }

        public void AlterMood(int value)
        {
            Mood += value;
            Mood = Mood < 0 ? 0 : Mood;
            Mood = Mood > MoodMax ? MoodMax : Mood;
        }

        /// <summary>
        /// ������˽���
        /// </summary>
        public void SettleTransport()
        {
            AlterAP(-1 * APCostTransport);
            AlterExp(WorkType.Transport, ExpTransport);
            AlterMood(-1 * MoodCost);
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

        #region Sort
        public class SortForProNodeUI : IComparer<Worker>
        {
            public WorkType WorkType;

            public int Compare(Worker x, Worker y)
            {
                if (x == null || y == null)
                {
                    return (x == null).CompareTo((y == null));
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
        #endregion

        #region ML.PlayerCharacterNS.IAICharacter
        public int prefabIndex { get; } = 0;
        public ML.PlayerCharacterNS.ICharacterState State { get; set; }
        public ML.PlayerCharacterNS.IController Controller { get; set; }
        public void OnSpawn(ML.PlayerCharacterNS.IController controller) { }
        public void OnDespose(ML.PlayerCharacterNS.IController controller) { }
        #endregion
    }
}
