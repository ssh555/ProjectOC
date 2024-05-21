using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using ProjectOC.ManagerNS;

namespace ProjectOC.ProNodeNS
{
    [LabelText("�����ڵ�"), Serializable]
    public class ProNode: DataNS.ItemContainerOwner, WorkerNS.IWorkerContainer, WorkerNS.IEffectObj
    {
        #region Data
        [LabelText("�����ڵ㽨��"), ReadOnly]
        public WorldProNode WorldProNode;
        [ReadOnly]
        public string ID = "";
        [LabelText("�ȼ�"), ReadOnly]
        public int Level = 0;
        [LabelText("�������䷽"), ReadOnly]
        public ML.Engine.InventorySystem.Recipe Recipe;
        [LabelText("��������Ч��"), ReadOnly]
        public int EffBase;
        [LabelText("�ѻ�Ԥ����"), ReadOnly]
        public int StackReserve = 0;

        [LabelText("��������_ֵ��"), ReadOnly, ShowInInspector]
        private int InitAPCost_Duty;
        [LabelText("����ֵ����������"), ReadOnly, ShowInInspector]
        private int ModifyAPCost_Duty;
        [LabelText("ֵ���������ı���"), ReadOnly, ShowInInspector]
        private float FactorAPCost_Duty;
        [LabelText("����ֵ����������"), ReadOnly, ShowInInspector]
        public int RealAPCost_Duty => (int)(InitAPCost_Duty * FactorAPCost_Duty + ModifyAPCost_Duty);
        #endregion

        #region Timer
        protected ML.Engine.Timer.CounterDownTimer timerForProduce;
        /// <summary>
        /// ������ʱ����ʱ��Ϊ�䷽����һ�������ʱ��
        /// </summary>
        protected ML.Engine.Timer.CounterDownTimer TimerForProduce
        {
            get
            {
                if (timerForProduce == null)
                {
                    timerForProduce = new ML.Engine.Timer.CounterDownTimer(TimeCost, false, false);
                    timerForProduce.OnEndEvent += EndActionForProduce;
                    timerForProduce.OnUpdateEvent += UpdateActionForProduce;
                }
                return timerForProduce;
            }
        }
        protected ML.Engine.Timer.CounterDownTimer timerForMission;
        /// <summary>
        /// �����ʱ��
        /// </summary>
        protected ML.Engine.Timer.CounterDownTimer TimerForMission
        {
            get
            {
                if (timerForMission == null)
                {
                    timerForMission = new ML.Engine.Timer.CounterDownTimer(1f, true, false);
                    timerForMission.OnEndEvent += EndActionForMission;
                }
                return timerForMission;
            }
        }
        #endregion

        #region Property
        [LabelText("�Ƿ��������䷽"), ShowInInspector, ReadOnly]
        public bool HasRecipe => Recipe.IsValidRecipe;
        [LabelText("�ѻ�����"), ShowInInspector, ReadOnly]
        public int Stack => DataContainer?.GetAmount(Recipe.ProductID, DataNS.DataOpType.Storage) ?? 0;
        [LabelText("�ܶѻ�����"), ShowInInspector, ReadOnly]
        public int StackAll => DataContainer?.GetAmount(Recipe.ProductID, DataNS.DataOpType.StorageAll) ?? 0;
        [LabelText("������ID"), ShowInInspector, ReadOnly]
        public string ProductID => Recipe.ProductID;
        [LabelText("һ������������������"), ShowInInspector, ReadOnly]
        public int ProductNum => Recipe.ProductNum;
        [LabelText("�Ƿ���������"), ShowInInspector, ReadOnly]
        public bool IsOnRunning => timerForMission != null && !timerForMission.IsStoped;
        [LabelText("�Ƿ�����������Ʒ"), ShowInInspector, ReadOnly]
        public bool IsOnProduce => timerForProduce != null && !timerForProduce.IsStoped;
        [LabelText("�����ڵ�״̬"), ShowInInspector, ReadOnly]
        public ProNodeState State => HasRecipe ? (IsOnProduce ? ProNodeState.Production : ProNodeState.Stagnation) : ProNodeState.Vacancy;
        [LabelText("����Ч��"), PropertyTooltip("��λ%"), ShowInInspector, ReadOnly]
        public int Eff => HaveWorker ? EffBase + Worker.GetEff(ExpType) : EffBase;
        [LabelText("����һ������Ҫ��ʱ��"), ShowInInspector, ReadOnly]
        public int TimeCost => HasRecipe && Eff > 0 ? (int)Math.Ceiling((double)100 * Recipe.TimeCost / Eff) : 0;
        #endregion

        #region Table Property
        [LabelText("�����ڵ�����"), ShowInInspector, ReadOnly]
        public ProNodeType ProNodeType => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetProNodeType(ID) : ProNodeType.None;
        [LabelText("�����ڵ���Ŀ"), ShowInInspector, ReadOnly]
        public ML.Engine.InventorySystem.RecipeCategory Category => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetCategory(ID) : ML.Engine.InventorySystem.RecipeCategory.None;
        [LabelText("�����ڵ��ִ���䷽��Ŀ"), ShowInInspector, ReadOnly]
        public List<ML.Engine.InventorySystem.RecipeCategory> RecipeCategoryFilter => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetRecipeCategoryFilterd(ID) : new List<ML.Engine.InventorySystem.RecipeCategory>();
        [LabelText("��������"), ShowInInspector, ReadOnly]
        public WorkerNS.SkillType ExpType => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetExpType(ID) : WorkerNS.SkillType.None;
        [LabelText("�ѷ�������"), ShowInInspector, ReadOnly]
        public int StackMax => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetMaxStack(ID) : 0;
        [LabelText("�ѷ���ֵ��"), ShowInInspector, ReadOnly]
        public int StackThreshold => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetStackThreshold(ID) : 0;
        [LabelText("������ֵ��"), ShowInInspector, ReadOnly]
        public int RawThreshold => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetRawThreshold(ID) : 0;
        [LabelText("�Ƿ���Ҫ����"), ShowInInspector, FoldoutGroup("����")]
        public bool RequirePower => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetCanCharge(ID) : false;
        #endregion

        #region ProNode
        public ProNode(ProNodeTableData config)
        {
            ID = config.ID ?? "";
            (this as DataNS.IContainerOwner<string>).InitData(0, 0);
            EffBase = LocalGameManager.Instance.ProNodeManager.Config.EffBase;
            InitAPCost_Duty = LocalGameManager.Instance.ProNodeManager.Config.InitAPCost_Duty;
            DataContainer.OnDataChangeEvent += OnContainerDataChangeEvent;
        }

        public void Destroy()
        {
            RemoveRecipe();
            (this as WorkerNS.IWorkerContainer).RemoveWorker();
        }

        /// <summary>
        /// ��ʼ���������ڵ㣬���ʱ�򲢲�һ����ʼ������Ʒ
        /// </summary>
        public void StartRun()
        {
            TimerForMission.Start();
            StartProduce();
        }

        /// <summary>
        /// ȡ�����У����ʱ���ֹͣ������Ʒ
        /// </summary>
        public void StopRun()
        {
            if (IsOnRunning)
            {
                TimerForMission.End();
            }
            StopProduce();
        }

        /// <summary>
        /// ��ʼ������Ʒ
        /// </summary>
        public bool StartProduce()
        {
            if (CanWorking() && !IsOnProduce)
            {
                TimerForProduce.Reset(TimeCost);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ֹͣ������Ʒ
        /// </summary>
        public void StopProduce()
        {
            if (IsOnProduce)
            {
                timerForProduce?.End();
            }
        }

        /// <summary>
        /// �Ƿ���Կ�ʼ������Ʒ
        /// </summary>
        public bool CanWorking()
        {
            if (HasRecipe)
            {
                foreach (var kv in Recipe.Raw)
                {
                    if (DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) < kv.num) { return false; }
                }
                if (ProNodeType == ProNodeType.Mannul && !(HaveWorker && Worker.IsOnProNodeDuty && !Worker.HaveFeatSeat)) { return false; }
                if (StackAll >= StackMax * ProductNum) { return false; }
                if (RequirePower && WorldProNode != null && WorldProNode.PowerCount <= 0) { return false; }
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// ��ȡ�����ڵ�����������䷽
        /// </summary>
        public List<string> GetCanProduceRecipe()
        {
            List<string> result = new List<string>();
            foreach (var recipeCategory in RecipeCategoryFilter)
            {
                result.AddRange(LocalGameManager.Instance.RecipeManager.GetRecipeIDsByCategory(recipeCategory));
            }
            return LocalGameManager.Instance.RecipeManager.SortRecipeIDs(result);
        }

        public bool SetLevel(int level)
        {
            var config = LocalGameManager.Instance.ProNodeManager.Config;
            if (0 <= level && level <= config.LevelMax)
            {
                if (level > Level)
                {
                    for (int i = Level; i < level; i++)
                    {
                        EffBase += config.LevelUpgradeEff[i];
                    }
                }
                else if (level < Level)
                {
                    for (int i = Level; i > level; i--)
                    {
                        EffBase -= config.LevelUpgradeEff[i - 1];
                    }
                }
                Level = level;
                return true;
            }
            return false;
        }

        public bool ChangeRecipe(string recipeID)
        {
            lock (this)
            {
                RemoveRecipe();
                if (!string.IsNullOrEmpty(recipeID))
                {
                    var recipe = LocalGameManager.Instance.RecipeManager.SpawnRecipe(recipeID);
                    if (recipe.IsValidRecipe)
                    {
                        Recipe = recipe;
                        List<string> itemIDs = new List<string>() { recipe.ProductID };
                        List<int> dataCapacitys = new List<int>() { recipe.ProductNum * StackMax };
                        foreach (var raw in recipe.Raw)
                        {
                            itemIDs.Add(raw.id);
                            dataCapacitys.Add(raw.num * StackMax);
                        }
                        ResetData(itemIDs, dataCapacitys);
                        StartRun();
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        public void RemoveRecipe()
        {
            StopRun();
            ClearData();
            Recipe.ClearData();
        }
        #endregion

        #region Event
        public event Action OnDataChangeEvent;
        public event Action<double> OnProduceUpdateEvent;
        public event Action OnProduceEndEvent;

        protected void EndActionForMission()
        {
            int missionNum;
            foreach (var kv in Recipe.Raw)
            {
                missionNum = kv.num * RawThreshold - DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) - (this as MissionNS.IMissionObj<string>).GetMissionNum(kv.id, true);
                if (missionNum > 0)
                {
                    missionNum += kv.num * (StackMax - RawThreshold);
                    LocalGameManager.Instance.MissionManager.CreateTransportMission
                        (MissionNS.MissionTransportType.Store_ProNode, kv.id, missionNum, this, MissionNS.MissionInitiatorType.PutIn_Initiator);
                }
            }
            if (StackReserve > 0)
            {
                var missionType = ML.Engine.InventorySystem.ItemManager.Instance.GetItemType(ProductID) == ML.Engine.InventorySystem.ItemType.Feed ? 
                    MissionNS.MissionTransportType.ProNode_Restaurant : MissionNS.MissionTransportType.ProNode_Store;
                LocalGameManager.Instance.MissionManager.CreateTransportMission(missionType, ProductID, StackReserve, this, MissionNS.MissionInitiatorType.PutOut_Initiator);
                StackReserve = 0;
            }
        }

        protected void UpdateActionForProduce(double time) { OnProduceUpdateEvent?.Invoke(time); }

        protected void EndActionForProduce()
        {
            ML.Engine.InventorySystem.Item item = Recipe.Composite(this);
            AddItem(item);
            int needAssignNum = (this as MissionNS.IMissionObj<string>).GetNeedAssignNum(ProductID, false);
            if (Stack >= StackReserve + needAssignNum + StackThreshold * ProductNum)
            {
                StackReserve = Stack - needAssignNum;
            }
            if (ProNodeType == ProNodeType.Mannul)
            {
                Worker.SettleDuty(ExpType, Recipe.ExpRecipe, RealAPCost_Duty);
            }
            // ��һ������
            if (!StartProduce())
            {
                StopProduce();
            }
            OnProduceEndEvent?.Invoke();
        }

        protected void OnWorkerStatusChangeEvent(WorkerNS.Status status)
        {
            if (status != WorkerNS.Status.Relaxing)
            {
                StartProduce();
            }
            else
            {
                StopProduce();
            }
            OnDataChangeEvent?.Invoke();
        }

        protected void OnWorkerAPChangeEvent(int ap) { OnDataChangeEvent?.Invoke(); }

        protected void OnContainerDataChangeEvent()
        {
            int cur = StackReserve + (this as MissionNS.IMissionObj<string>).GetNeedAssignNum(ProductID, false) - Stack;
            if (cur > 0)
            {
                foreach (var mission in (this as MissionNS.IMissionObj<string>).GetMissions(ProductID, false))
                {
                    int needAssignNum = mission.NeedAssignNum;
                    int missionNum = mission.MissionNum;
                    if (cur > needAssignNum)
                    {
                        mission.ChangeMissionNum(missionNum - needAssignNum);
                        cur -= needAssignNum;
                    }
                    else
                    {
                        mission.ChangeMissionNum(missionNum - cur);
                        cur = 0;
                        break;
                    }
                }
            }
            if (cur > 0)
            {
                StackReserve -= cur;
            }
            StartProduce();
            OnDataChangeEvent?.Invoke(); 
        }
        #endregion

        #region ItemContainerOwner
        public override Transform GetTransform() { return WorldProNode.transform; }
        public override string GetUID() { return WorldProNode.InstanceID; }
        public override MissionNS.MissionObjType GetMissionObjType() { return MissionNS.MissionObjType.ProNode; }
        public void FastAdd() { for (int i = 1; i < DataContainer.GetCapacity(); i++) { FastAdd(i); } }
        #endregion

        #region IWorkerContainer
        public Action<WorkerNS.Worker> OnSetWorkerEvent { get; set; }
        public Action<bool, WorkerNS.Worker> OnRemoveWorkerEvent { get; set; }
        public WorkerNS.Worker Worker { get; set; }
        public bool IsArrive { get; set; }
        public bool HaveWorker => ProNodeType == ProNodeType.Mannul && Worker != null && !string.IsNullOrEmpty(Worker.ID);

        public WorkerNS.WorkerContainerType GetContainerType() { return WorkerNS.WorkerContainerType.Work; }

        public void OnArriveEvent(WorkerNS.Worker worker)
        {
            (this as WorkerNS.IWorkerContainer).OnArriveSetPosition(worker, WorldProNode.transform.position + new Vector3(0, 2f, 0));
            worker.ProNode.StartProduce();
        }

        public void OnPositionChange(Vector3 differ)
        {
            if (HaveWorker)
            {
                if (IsArrive)
                {
                    Worker.transform.position += differ;
                }
                else
                {
                    Worker.SetDestination(GetTransform().position, OnArriveEvent, GetContainerType());
                }
            }
            (this as MissionNS.IMissionObj<string>).OnPositionChangeTransport();
        }

        public void SetWorkerRelateData()
        {
            if (ProNodeType == ProNodeType.Mannul && Worker != null)
            {
                Worker.SetTimeStatusAll(WorkerNS.TimeStatus.Work_OnDuty);
                Worker.SetDestination(WorldProNode.transform.position, OnArriveEvent, GetContainerType());
                Worker.OnStatusChangeEvent += OnWorkerStatusChangeEvent;
                Worker.OnAPChangeEvent += OnWorkerAPChangeEvent;
            }
        }

        public void RemoveWorkerRelateData() 
        {
            if (ProNodeType == ProNodeType.Mannul)
            {
                StopProduce();
                if (HaveWorker)
                {
                    Worker.OnStatusChangeEvent -= OnWorkerStatusChangeEvent;
                    Worker.OnAPChangeEvent -= OnWorkerAPChangeEvent;
                }
            }
        }

        public bool TempRemoveWorker()
        {
            if (Worker != null && !IsOnProduce && IsArrive)
            {
                Worker.RecoverLastPosition();
                IsArrive = false;
                return true;
            }
            return false;
        }
        #endregion

        #region IEffectObj
        public List<WorkerNS.Effect> Effects { get; set; } = new List<WorkerNS.Effect>();
        public void ApplyEffect(WorkerNS.Effect effect)
        {
            if (effect.EffectType != WorkerNS.EffectType.AlterProNodeVariable) { Debug.Log("type != AlterProNodeVariable"); return; }
            bool flag = true;
            if (effect.ParamStr == "EffBase")
            {
                EffBase += effect.ParamInt;
            }
            else if (effect.ParamStr == "FactorAPCostDuty")
            {
                FactorAPCost_Duty += effect.ParamFloat;
            }
            else
            {
                flag = false;
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
        public void RemoveEffect(WorkerNS.Effect effect)
        {
            if (effect.EffectType != WorkerNS.EffectType.AlterProNodeVariable) { Debug.Log("type != AlterProNodeVariable"); return; }
            Effects.Remove(effect);
            if (effect.ParamStr == "EffBase")
            {
                EffBase -= effect.ParamInt;
            }
            else if (effect.ParamStr == "FactorAPCostDuty")
            {
                FactorAPCost_Duty -= effect.ParamFloat;
            }
        }
        #endregion
    }
}