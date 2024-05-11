using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectOC.WorkerNS
{
    [LabelText("����"), Serializable]
    public class Worker : SerializedMonoBehaviour, ML.Engine.Timer.ITickComponent, ML.PlayerCharacterNS.IAICharacter, IEffectObj
    {
        #region Data
        #region Base
        [LabelText("ID"), ReadOnly, ShowInInspector]
        public string ID { get; private set; }
        [LabelText("����"), ReadOnly, ShowInInspector]
        public string Name { get; private set; }
        [LabelText("��Ŀ"), ReadOnly]
        public WorkerEchoNS.Category Category;
        [LabelText("�Ա�"), ReadOnly, ShowInInspector]
        public Gender Gender { get; private set; }
        #endregion
        #region AP
        [LabelText("��ǰ����ֵ"), ReadOnly, ShowInInspector]
        public int APCurrent { get; private set; }
        [LabelText("��������"), ReadOnly, ShowInInspector]
        public int APMax { get; private set; }
        [LabelText("����������ֵ"), ReadOnly, ShowInInspector]
        public int APWorkThreshold { get; private set; }
        [LabelText("������Ϣ��ֵ"), ReadOnly, ShowInInspector]
        public int APRelaxThreshold { get; private set; }
        [LabelText("��������_����"), ReadOnly, ShowInInspector]
        private int APCost_Transport;
        [LabelText("������������"), ReadOnly, ShowInInspector]
        private int ModifyAPCost;
        [LabelText("�������ı���"), ReadOnly, ShowInInspector]
        private float FactorAPCost = 1;
        [LabelText("���հ�����������"), ReadOnly, ShowInInspector]
        public int RealAPCost_Transport => (int)(APCost_Transport * FactorAPCost + ModifyAPCost);
        #endregion
        #region Food
        [LabelText("��ʳʱ��"), ReadOnly, ShowInInspector]
        private float EatTime;
        [LabelText("��ʳʱ�䱶��"), ReadOnly, ShowInInspector]
        private float FactorEatTime = 1;
        [LabelText("�����ʳʱ��"), ReadOnly, ShowInInspector]
        private float ModifyEatTime;
        [LabelText("���ս�ʳʱ��"), ReadOnly, ShowInInspector]
        public float RealEatTime => EatTime * FactorEatTime + ModifyEatTime;
        #endregion
        #region Mood
        [LabelText("��ǰ����ֵ"), ReadOnly, ShowInInspector]
        public int EMCurrent { get; private set; }
        [LabelText("�Ƿ񴥷�������Ч��"), ReadOnly, ShowInInspector]
        private bool HaveSetEMLowEffect;
        [LabelText("�Ƿ񴥷�������Ч��"), ReadOnly, ShowInInspector]
        private bool HaveSetEMHighEffect;
        [LabelText("����ֵ����"), ReadOnly, ShowInInspector]
        public int EMMax { get; private set; }
        [LabelText("��������ֵ"), ReadOnly, ShowInInspector]
        private int EMLowThreshold;
        [LabelText("��������ֵ"), ReadOnly, ShowInInspector]
        private int EMHighThreshold;
        [LabelText("������Ч��"), ReadOnly, ShowInInspector]
        private int EMLowEffect;
        [LabelText("������Ч��"), ReadOnly, ShowInInspector]
        private int EMHighEffect;
        [LabelText("����������"), ReadOnly, ShowInInspector]
        private int EMCost;
        [LabelText("����ָ���"), ReadOnly, ShowInInspector] 
        public int EMRecover { get; private set; }
        #endregion
        #region Speed
        [LabelText("�ƶ��ٶ�"), ReadOnly, ShowInInspector]
        private float WalkSpeed;
        [LabelText("�ƶ��ٶȱ���"), ReadOnly, ShowInInspector]
        private float FactorWalkSpeed = 1;
        [LabelText("�����ƶ��ٶ�"), ReadOnly, ShowInInspector]
        private float ModifyWalkSpeed;
        [LabelText("�����ƶ��ٶ�"), ReadOnly, ShowInInspector]
        public float RealWalkSpeed => WalkSpeed * FactorWalkSpeed + ModifyWalkSpeed;
        #endregion
        #region Weight
        [LabelText("��������"), ReadOnly, ShowInInspector]
        private int BURMax;
        [LabelText("�������ޱ���"), ReadOnly, ShowInInspector]
        private float FactorBURMax = 1;
        [LabelText("���⸺������"), ReadOnly, ShowInInspector]
        private int ModifyBURMax;
        [LabelText("���ո�������"), ReadOnly, ShowInInspector]
        public int RealBURMax => (int)(BURMax * FactorBURMax + ModifyBURMax) + GetEff(SkillType.Transport);
        #endregion
        #region Skill
        [LabelText("ȫ�ֹ���Ч��"), ReadOnly]
        public int Eff_AllSkill;
        [LabelText("���˾���ֵ"), ReadOnly, ShowInInspector]
        public int ExpTransport;
        [LabelText("����"), ShowInInspector, ReadOnly]
        public Dictionary<SkillType, Skill> Skill = new Dictionary<SkillType, Skill>();
        [LabelText("����"), ReadOnly]
        public List<Feature> Features = new List<Feature>();
        #endregion
        #region Time
        [LabelText("�Ƿ���԰���ʱ��"), ReadOnly, ShowInInspector]
        public bool CanArrange { get; private set; }
        [LabelText("�Ƿ���Է�תʱ��"), ReadOnly, ShowInInspector]
        public bool CanReverse { get; private set; }
        #endregion
        #endregion

        #region Init OnDestroy
        public void Init()
        {
            #region Init Data
            var config = ManagerNS.LocalGameManager.Instance.WorkerManager.Config;
            ID = ManagerNS.LocalGameManager.Instance.WorkerManager.GetOneNewWorkerID();
            Name = ManagerNS.LocalGameManager.Instance.WorkerManager.GetOneNewWorkerName();
            Gender = ManagerNS.LocalGameManager.Instance.WorkerManager.GetOneNewWorkerGender();
            APMax = config.APMax;
            APWorkThreshold = config.APWorkThreshold;
            APRelaxThreshold = config.APRelaxThreshold;
            APCost_Transport = config.APCost_Transport;
            AlterAP(APMax);
            EatTime = config.EatTime;
            EMMax = config.EMMax;
            EMLowThreshold = config.EMLowThreshold;
            EMHighThreshold = config.EMHighThreshold;
            EMLowEffect = config.EMLowEffect;
            EMHighEffect = config.EMHighEffect;
            EMCost = config.EMCost;
            EMRecover = config.EMRecover;
            AlterMood(EMMax);
            WalkSpeed = config.WalkSpeed;
            BURMax = config.BURMax;
            ExpTransport = config.ExpTransport;
            List<int> levels = ManagerNS.LocalGameManager.Instance.WorkerManager.GetSkillLevel();
            foreach (SkillType skillType in Enum.GetValues(typeof(SkillType)))
            {
                int level = skillType != SkillType.None ? levels[(int)skillType - 1] : 0;
                Skill.Add(skillType, new Skill(skillType, level));
            }
            Features = ManagerNS.LocalGameManager.Instance.FeatureManager.CreateFeature();
            foreach (Feature feature in Features)
            {
                feature.ApplyFeature(this);
            }
            CanArrange = true;
            CanReverse = false;
            #endregion

            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            Agent = GetComponent<NavMeshAgent>();
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
            StateController = new ML.Engine.FSM.StateController(0);
            StateMachine = new WorkerStateMachine(this);
            StateController.SetStateMachine(StateMachine);
            ML.Engine.InventorySystem.ItemManager.Instance.AddItemIconObject("", transform, 
                new Vector3(0, transform.GetComponent<CapsuleCollider>().height * 1.5f, 0),
                Quaternion.Euler(Vector3.zero), Vector3.one,
                ManagerNS.LocalGameManager.Instance.Player.currentCharacter.transform);
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

        #region IEffectObj
        public List<Effect> Effects { get; set; } = new List<Effect>();
        public void ApplyEffect(Effect effect)
        {
            if (effect.EffectType != EffectType.AlterWorkerVariable) { Debug.Log("type != AlterWorkerVariable"); return; }
            bool flag = true;
            if (effect.ParamStr == "APMax")
            {
                APMax += effect.ParamInt;
            }
            else if (effect.ParamStr == "CanArrange")
            {
                CanArrange = effect.ParamBool;
            }
            else if (effect.ParamStr == "CanReverse")
            {
                CanReverse = effect.ParamBool;
            }
            else if (effect.ParamStr == "Eff_AllSkill")
            {
                Eff_AllSkill += effect.ParamInt;
            }
            else if (effect.ParamStr == "EMCost")
            {
                EMCost += effect.ParamInt;
            }
            else if (effect.ParamStr == "EMRecover")
            {
                EMRecover += effect.ParamInt;
            }
            else if (effect.ParamStr == "FactorAPCost")
            {
                FactorAPCost += effect.ParamFloat;
            }
            else if (effect.ParamStr == "FactorEatTime")
            {
                FactorEatTime += effect.ParamFloat;
            }
            else if (effect.ParamStr == "FactorWalkSpeed")
            {
                FactorWalkSpeed += effect.ParamFloat;
            }
            else if (effect.ParamStr == "ModifyBURMax")
            {
                ModifyBURMax += effect.ParamInt;
            }
            else if (effect.ParamStr == "ModifyEatTime")
            {
                ModifyEatTime += effect.ParamFloat;
            }
            else if (effect.ParamStr == "ModifyWalkSpeed")
            {
                ModifyWalkSpeed += effect.ParamFloat;
            }
            else
            {
                flag = false;
            }

            if (!flag && effect.ParamStr.Contains('_') && Enum.TryParse(effect.ParamStr.Split('_')[1], out SkillType skillType))
            {
                flag = true;
                if (effect.ParamStr.StartsWith("Eff"))
                {
                    Skill[skillType].AlterEff(effect.ParamInt);
                }
                else if (effect.ParamStr.StartsWith("ExpRate"))
                {
                    Skill[skillType].AlterExpRate(effect.ParamInt);
                }
                else if (effect.ParamStr.StartsWith("LevelCurrent"))
                {
                    Skill[skillType].ChangeLevel(effect.ParamInt);
                }
                else
                {
                    flag = false;
                }
            }
            if (flag)
            {
                Effects.Add(effect);
            }
            else
            {
                Debug.Log($"ParamStr Error {effect.ParamStr}");
            }
        }
        public void RemoveEffect(Effect effect)
        {
            if (effect.EffectType != EffectType.AlterWorkerVariable) { Debug.Log("type != AlterWorkerVariable"); return; }
            Effects.Remove(effect);
            if (effect.ParamStr == "APMax")
            {
                APMax -= effect.ParamInt;
            }
            else if (effect.ParamStr == "CanArrange")
            {
                CanArrange = !effect.ParamBool;
            }
            else if (effect.ParamStr == "CanReverse")
            {
                CanReverse = !effect.ParamBool;
            }
            else if (effect.ParamStr == "Eff_AllSkill")
            {
                Eff_AllSkill -= effect.ParamInt;
            }
            else if (effect.ParamStr == "EMCost")
            {
                EMCost -= effect.ParamInt;
            }
            else if (effect.ParamStr == "EMRecover")
            {
                EMRecover -= effect.ParamInt;
            }
            else if (effect.ParamStr == "FactorAPCost")
            {
                FactorAPCost -= effect.ParamFloat;
            }
            else if (effect.ParamStr == "FactorEatTime")
            {
                FactorEatTime -= effect.ParamFloat;
            }
            else if (effect.ParamStr == "FactorWalkSpeed")
            {
                FactorWalkSpeed -= effect.ParamFloat;
            }
            else if (effect.ParamStr == "ModifyBURMax")
            {
                ModifyBURMax -= effect.ParamInt;
            }
            else if (effect.ParamStr == "ModifyEatTime")
            {
                ModifyEatTime -= effect.ParamFloat;
            }
            else if (effect.ParamStr == "ModifyWalkSpeed")
            {
                ModifyWalkSpeed -= effect.ParamFloat;
            }

            if (effect.ParamStr.Contains('_') && Enum.TryParse(effect.ParamStr.Split('_')[1], out SkillType skillType))
            {
                if (effect.ParamStr.StartsWith("Eff"))
                {
                    Skill[skillType].AlterEff(-effect.ParamInt);
                }
                else if (effect.ParamStr.StartsWith("ExpRate"))
                {
                    Skill[skillType].AlterExpRate(-effect.ParamInt);
                }
                else if (effect.ParamStr.StartsWith("LevelCurrent"))
                {
                    Skill[skillType].ChangeLevel(0);
                }
            }
        }
        #endregion

        #region AP Mood Skill
        public Action<int> OnAPChangeEvent;
        public Action<int> OnMoodChangeEvent;
        public void AlterAP(int value)
        {
            int current = APCurrent;
            current += value;
            current = current < 0 ? 0 : current;
            int max = APMax;
            current = current < max ? current : max;
            APCurrent = current;
            OnAPChangeEvent?.Invoke(APCurrent);
        }
        public void AlterMood(int value)
        {
            int current = EMCurrent;
            current += value;
            current = current < 0 ? 0 : current;
            int max = EMMax;
            current = current < max ? current : max;
            EMCurrent = current;
            EMCurrent = value;
            if (EMCurrent >= EMHighThreshold)
            {
                if (!HaveSetEMHighEffect)
                {
                    Eff_AllSkill += EMHighEffect;
                    HaveSetEMHighEffect = true;
                }
            }
            else
            {
                if (HaveSetEMHighEffect)
                {
                    Eff_AllSkill -= EMHighEffect;
                    HaveSetEMHighEffect = false;
                }
            }
            if (EMCurrent > EMLowThreshold)
            {
                if (HaveSetEMLowEffect)
                {
                    Eff_AllSkill -= EMLowEffect;
                    HaveSetEMLowEffect = false;
                }
            }
            else
            {
                if (!HaveSetEMLowEffect)
                {
                    Eff_AllSkill -= EMLowEffect;
                    HaveSetEMLowEffect = true;
                }
            }
            OnMoodChangeEvent?.Invoke(EMCurrent);
        }
        public void AlterExp(SkillType workType, int value)
        {
            Skill[workType].AlterExp(value);
        }
        public void SettleTransport()
        {
            AlterExp(SkillType.Transport, ExpTransport);
            AlterAP(-1 * RealAPCost_Transport);
            AlterMood(-1 * EMCost);
        }
        public void SettleDuty(SkillType expType, int exp, int apCost)
        {
            AlterExp(expType, exp);
            int realAPCost_Duty = (int)(FactorAPCost * apCost + ModifyAPCost);
            AlterAP(-1 * realAPCost_Duty);
            AlterMood(-1 * EMCost);
        }
        public int GetEff(SkillType type)
        {
            return Skill[type].GetEff() + (type != SkillType.Transport ? Eff_AllSkill : Eff_AllSkill / 10);
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
                foreach (var kv in TransportDict)
                {
                    if (ManagerNS.LocalGameManager.Instance != null)
                    {
                        int weight = ManagerNS.LocalGameManager.Instance.ItemManager?.GetWeight(kv.Key) ?? 0;
                        result += weight * kv.Value;
                    }
                }
                return result;
            }
        }
        [LabelText("����"), ReadOnly]
        public MissionNS.Transport Transport = null;
        [LabelText("�Ƿ��а���"), ShowInInspector, ReadOnly]
        public bool HaveTransport { get => Transport != null && !string.IsNullOrEmpty(Transport.ID); }

        [LabelText("������Ʒ"), ReadOnly]
        public Dictionary<string, int> TransportDict = new Dictionary<string, int>();
        #endregion

        #region TimeStatus
        [LabelText("ÿ��ʱ�εİ���")]
        public TimeArrangement TimeArrangement = new TimeArrangement(24);
        [LabelText("��ǰʱ�εİ���"), ShowInInspector, ReadOnly]
        public TimeStatus CurTimeStatus => ManagerNS.LocalGameManager.Instance != null ? 
            TimeArrangement.GetTimeStatus(ManagerNS.LocalGameManager.Instance.DispatchTimeManager.CurrentHour) : TimeStatus.Relax;
        public void SetTimeStatus(int time, TimeStatus timeStatus) { if (CanArrange) { TimeArrangement.SetTimeStatus(time, timeStatus); } }
        public void SetTimeStatusAll(TimeStatus timeStatus) { if (CanArrange) { TimeArrangement.SetTimeStatusAll(timeStatus); } }
        public void ReverseTimeStatusAll() { if (CanReverse) { TimeArrangement.ReverseTimeAll(); } }
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
                    timerForNoHome = new ML.Engine.Timer.CounterDownTimer(60 * ManagerNS.LocalGameManager.Instance.WorkerManager.Config.DestroyTimeForNoHome, false, false);
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
            if (!container.HaveWorker || container.Worker.ID == ID)
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
            if (ContainerDict != null && ContainerDict.ContainsKey(type))
            {
                IWorkerContainer container = ContainerDict[type];
                return container != null && !string.IsNullOrEmpty(container.GetUID());
            }
            return false;
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

        public bool SetDestination(Vector3 target, Action<Worker> action = null, WorkerContainerType arriveType = WorkerContainerType.None, float threshold = 3f)
        {
            ClearDestination();
            foreach (var key in ContainerDict.Keys.ToArray())
            {
                if (key != arriveType)
                {
                    ContainerDict[key]?.TempRemoveWorker();
                }
            }
            Threshold = threshold;
            Agent.isStopped = false;
            Agent.speed = RealWalkSpeed;
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
            bool inSeq = ManagerNS.LocalGameManager.Instance.RestaurantManager.ContainWorker(this);
            string icon = HaveSetEMLowEffect ? "Tex2D_Worker_UI_LowMood" : "";
            icon = inSeq ? "Tex2D_Worker_UI_LowAP" : icon;
            icon = HaveSetEMLowEffect && inSeq ? "Tex2D_Worker_UI_LowAPMood" : icon;
            GetComponentInChildren<ML.Engine.InventorySystem.ItemIcon>()?.SetSprite(ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(icon));
        }
        #endregion

        #region Sort
        public class SortForProNodeUI : IComparer<Worker>
        {
            public SkillType WorkType;

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
                int levelX = x.Skill[WorkType].LevelCurrent;
                int levelY = y.Skill[WorkType].LevelCurrent;
                if (levelX != levelY)
                {
                    return levelY.CompareTo(levelX);
                }
                return x.ID.CompareTo(y.ID);
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

        #region External
        /// <summary>
        /// 0 1 2 �ֱ������и�
        /// </summary>
        public int GetMoodStatu()
        {
            if(HaveSetEMLowEffect) return 0;
            if (HaveSetEMHighEffect) return 2;
            return 1;
        }
        /// <summary>
        /// 0 1 2 �ֱ������и�
        /// </summary>
        public int GetAPStatu()
        {
            if (APCurrent < APWorkThreshold) return 0;
            if (APCurrent >APRelaxThreshold) return 2;
            return 1;
        }
        #endregion
    }
}
