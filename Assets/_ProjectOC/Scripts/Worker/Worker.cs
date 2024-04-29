using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


namespace ProjectOC.WorkerNS
{
    [LabelText("����"), Serializable]
    public class Worker : SerializedMonoBehaviour, ML.Engine.Timer.ITickComponent, ML.PlayerCharacterNS.IAICharacter
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
        #endregion

        #region Data Property
        [LabelText("ʵ��ID"), ReadOnly]
        public string InstanceID;
        [LabelText("ְҵЧ�ʼӳ�"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, int> Eff = new Dictionary<WorkType, int>();
        [LabelText("����"), ReadOnly]
        public List<Feature> Features = new List<Feature>();
        public ML.Engine.InventorySystem.ItemIcon WorldIcon { get => GetComponentInChildren<ML.Engine.InventorySystem.ItemIcon>(); }
        #endregion

        #region WorkerContainer
        [ShowInInspector]
        public Dictionary<WorkerContainerType, IWorkerContainer> ContainerDict;
        private ML.Engine.Timer.CounterDownTimer timerForNoHome;
        private ML.Engine.Timer.CounterDownTimer TimerForNoHome
        {
            get
            {
                if (timerForNoHome == null)
                {
                    timerForNoHome = new ML.Engine.Timer.CounterDownTimer(60 * DestroyTimeForNoHome, false, false);
                    timerForNoHome.OnEndEvent += () =>
                    {
                        ManagerNS.LocalGameManager.Instance.WorkerManager.DeleteWorker(this);
                    };
                }
                return timerForNoHome;
            }
        }

        #region Property
        [LabelText("�Ƿ��������ڵ�"), ShowInInspector, ReadOnly]
        public bool HaveProNode => HasContainer(WorkerContainerType.Work);
        [LabelText("�Ƿ��в���"), ShowInInspector, ReadOnly]
        public bool HaveRestaurantSeat => HasContainer(WorkerContainerType.Relax);
        [LabelText("�Ƿ�����"), ShowInInspector, ReadOnly]
        public bool HaveHome => HasContainer(WorkerContainerType.Home);
        [LabelText("�Ƿ��������ڵ�ֵ��"), ShowInInspector, ReadOnly]
        public bool IsOnProNodeDuty { get { return HaveProNode && Status != Status.Relaxing && GetContainer(WorkerContainerType.Work).IsArrive; } }
        [LabelText("�����ڵ�"), ShowInInspector, ReadOnly]
        public ProNodeNS.ProNode ProNode => HasContainer(WorkerContainerType.Work) ? GetContainer(WorkerContainerType.Work) as ProNodeNS.ProNode : null;
        #endregion

        public IWorkerContainer GetContainer(WorkerContainerType type)
        {
            return ContainerDict[type];
        }

        public void SetContainer(IWorkerContainer container)
        {
            if (HaveTransport && container.GetContainerType() == WorkerContainerType.Work)
            {
                Transport.End();
            }
            if (container.GetContainerType() == WorkerContainerType.Home)
            {
                timerForNoHome?.End();
            }
            if (!container.HaveWorker || container.Worker.InstanceID == InstanceID)
            {
                ContainerDict[container.GetContainerType()] = container;
            }
        }

        public void RemoveContainer(WorkerContainerType type)
        {
            if (type == WorkerContainerType.Home && (timerForNoHome == null || timerForNoHome.IsStoped))
            {
                TimerForNoHome?.Start();
            }
            ContainerDict[type] = null;
        }

        public bool HasContainer(WorkerContainerType type)
        {
            IWorkerContainer container = ContainerDict[type];
            return container != null && !string.IsNullOrEmpty(container.GetUID());
        }
        #endregion

        #region Mono
        public void Init()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            Agent = GetComponent<NavMeshAgent>();

            foreach (WorkType workType in Enum.GetValues(typeof(WorkType)))
            {
                SkillExpRate.Add(workType, 100);
                Eff.Add(workType, 10);
                Skill.Add(workType, new Skill());
            }

            ContainerDict = new Dictionary<WorkerContainerType, IWorkerContainer>()
            {
                { WorkerContainerType.Work, null },
                { WorkerContainerType.Relax, null },
                { WorkerContainerType.Home, null }
            };

            if (!HasContainer(WorkerContainerType.Home))
            {
                TimerForNoHome.Start();
            }

            Features = ManagerNS.LocalGameManager.Instance.FeatureManager.CreateFeature();
            foreach (Feature feature in this.Features)
            {
                feature.ApplyFeature(this);
            }

            StateController = new ML.Engine.FSM.StateController(0);
            StateMachine = new WorkerStateMachine(this);
            StateController.SetStateMachine(StateMachine);

            ML.Engine.InventorySystem.ItemManager.Instance.AddItemIconObject("", transform, new Vector3(0, transform.GetComponent<CapsuleCollider>().height * 1.5f, 0),
                                        Quaternion.Euler(Vector3.zero), Vector3.one,
                                        (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
        }

        public void OnDestroy()
        {
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
            Transport?.End();
            foreach (IWorkerContainer container in ContainerDict.Values.ToArray())
            {
                container?.RemoveWorker();
            }
        }
        #endregion

        #region NavMesh
        public NavMeshAgent Agent = null;
        public float Threshold = 3f;
        [LabelText("Ѱ·Ŀ�ĵ�"), ShowInInspector, ReadOnly]
        public Vector3 Target { get; private set; }
        [LabelText("�Ƿ���Ѱ·"), ShowInInspector, ReadOnly]
        public bool HaveDestination { get; private set; } = false;
        [ShowInInspector, ReadOnly]
        public Vector3 LastPosition;
        public event Action<Worker> OnArriveEvent;
        private event Action<Worker> OnArriveDisposableEvent;

        public bool SetDestination(Vector3 target, Action<Worker> action = null, WorkerContainerType arriveType = WorkerContainerType.None)
        {
            ClearDestination();
            foreach (var kv in ContainerDict)
            {
                if (kv.Key != arriveType)
                {
                    kv.Value?.TempRemoveWorker();
                }
            }
            Agent.isStopped = false;
            if (Agent.SetDestination(target))
            {
                Target = target;
                HaveDestination = true;
                OnArriveDisposableEvent = action;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ClearDestination()
        {
            HaveDestination = false;
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

        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public void Tick(float deltatime)
        {
            // NavMesh
            if (HaveDestination && Vector3.Distance(transform.position, Target) < Threshold)
            {
                ClearDestination();
                OnArriveDisposableEvent?.Invoke(this);
                OnArriveEvent?.Invoke(this);
            }
            // World Icon
            bool lowMood = Mood < MoodThreshold;
            bool inSeq = ManagerNS.LocalGameManager.Instance.RestaurantManager.ContainWorker(this);
            string icon = lowMood ? "Tex2D_Worker_UI_LowMood" : "";
            icon = inSeq ? "Tex2D_Worker_UI_LowAP" : icon;
            icon = lowMood && inSeq ? "Tex2D_Worker_UI_LowAPMood" : icon;
            WorldIcon?.SetSprite(ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(icon));
        }
        #endregion

        #region State Status
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
        public Action<Status> OnStatusChangeEvent;
        [LabelText("״̬��������"), ShowInInspector, ReadOnly]
        protected ML.Engine.FSM.StateController StateController = null;
        [LabelText("״̬��"), ShowInInspector, ReadOnly]
        protected WorkerStateMachine StateMachine = null;
        #endregion

        #region Transport
        [LabelText("��ǰ����"), ShowInInspector, ReadOnly]
        public int WeightCurrent
        {
            get
            {
                int result = 0;
                foreach (ML.Engine.InventorySystem.Item item in this.TransportItems)
                {
                    result += item.Weight;
                }
                return result;
            }
        }
        [LabelText("����"), ReadOnly]
        public MissionNS.Transport Transport = null;
        [LabelText("�Ƿ��а���"), ShowInInspector, ReadOnly]
        public bool HaveTransport { get => Transport != null && !string.IsNullOrEmpty(Transport.ItemID); }

        [LabelText("������Ʒ"), ReadOnly]
        public List<ML.Engine.InventorySystem.Item> TransportItems = new List<ML.Engine.InventorySystem.Item>();
        #endregion

        #region AP Mood Skill Exp
        [LabelText("��ǰ����ֵ"), ReadOnly]
        public int APCurrent = 10;
        public Action<int> OnAPChangeEvent;
        [LabelText("��ǰ����"), ReadOnly]
        public int Mood = 100;
        public Action<int> OnMoodChangeEvent;
        [LabelText("����"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, Skill> Skill = new Dictionary<WorkType, Skill>();
        [LabelText("���ܾ����ȡ�ٶ�"), ShowInInspector, ReadOnly]
        public Dictionary<WorkType, int> SkillExpRate = new Dictionary<WorkType, int>();

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
            OnMoodChangeEvent?.Invoke(Mood);
        }
        public void AlterExp(WorkType workType, int value)
        {
            Skill[workType].AlterExp(value * SkillExpRate[workType]);
        }
        public void SettleTransport()
        {
            AlterAP(-1 * APCostTransport);
            AlterExp(WorkType.Transport, ExpTransport);
            AlterMood(-1 * MoodCost);
        }
        #endregion

        #region TimeStatus
        [LabelText("ÿ��ʱ�εİ���")]
        public TimeArrangement TimeArrangement = new TimeArrangement();
        [LabelText("��ǰʱ�εİ���"), ShowInInspector, ReadOnly]
        public TimeStatus CurTimeStatus
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
        public void SetTimeStatus(int time, TimeStatus timeStatus)
        {
            this.TimeArrangement[time] = timeStatus;
        }
        public void SetTimeStatusAll(TimeStatus timeStatus)
        {
            this.TimeArrangement.SetTimeStatusAll(timeStatus);
        }
        #endregion

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
                int stateX = (x.HaveProNode || x.HaveTransport) ? 1 : 0;
                int stateY = (y.HaveProNode || y.HaveTransport) ? 1 : 0;
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

        #region IAICharacter
        public int prefabIndex { get; } = 0;
        public ML.PlayerCharacterNS.ICharacterState State { get; set; }
        public ML.PlayerCharacterNS.IController Controller { get; set; }
        public void OnSpawn(ML.PlayerCharacterNS.IController controller) { }
        public void OnDespose(ML.PlayerCharacterNS.IController controller) { }
        #endregion
    }
}
