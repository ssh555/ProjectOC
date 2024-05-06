using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using ProjectOC.ManagerNS;

namespace ProjectOC.ProNodeNS
{
    [LabelText("生产节点"), Serializable]
    public class ProNode: DataNS.ItemContainerOwner, WorkerNS.IWorkerContainer
    {
        #region Data
        [LabelText("生产节点建筑"), ReadOnly]
        public WorldProNode WorldProNode;
        [LabelText("建筑实例ID"), ShowInInspector, ReadOnly]
        public string UID { get { return WorldProNode?.InstanceID ?? ""; } }
        [ReadOnly]
        public string ID = "";
        [LabelText("等级"), ReadOnly]
        public int Level = 0;
        [LabelText("生产的配方"), ReadOnly]
        public ML.Engine.InventorySystem.Recipe Recipe;
        [LabelText("基础生产效率"), ReadOnly]
        public int EffBase;
        [LabelText("堆积预留量"), ReadOnly]
        public int StackReserve = 0;
        #endregion

        #region Timer
        protected ML.Engine.Timer.CounterDownTimer timerForProduce;
        /// <summary>
        /// 生产计时器，时间为配方生产一次所需的时间
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
        /// 任务计时器
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
        [LabelText("是否有生产配方"), ShowInInspector, ReadOnly]
        public bool HasRecipe => Recipe.IsValidRecipe;
        [LabelText("堆积数量"), ShowInInspector, ReadOnly]
        public int Stack => DataContainer?.GetAmount(Recipe.ProductID, DataNS.DataOpType.Storage) ?? 0;
        [LabelText("总堆积数量"), ShowInInspector, ReadOnly]
        public int StackAll => DataContainer?.GetAmount(Recipe.ProductID, DataNS.DataOpType.StorageAll) ?? 0;
        [LabelText("生产物ID"), ShowInInspector, ReadOnly]
        public string ProductID => Recipe.ProductID;
        [LabelText("一次生产的生产物数量"), ShowInInspector, ReadOnly]
        public int ProductNum => Recipe.ProductNum;
        [LabelText("是否正在运行"), ShowInInspector, ReadOnly]
        public bool IsOnRunning => timerForMission != null && !timerForMission.IsStoped;
        [LabelText("是否正在制作物品"), ShowInInspector, ReadOnly]
        public bool IsOnProduce => timerForProduce != null && !timerForProduce.IsStoped;
        [LabelText("生产节点状态"), ShowInInspector, ReadOnly]
        public ProNodeState State => HasRecipe ? (IsOnProduce ? ProNodeState.Production : ProNodeState.Stagnation) : ProNodeState.Vacancy;
        [LabelText("生产效率"), PropertyTooltip("单位%"), ShowInInspector, ReadOnly]
        public int Eff => HaveWorker ? EffBase + Worker.Eff[ExpType] : EffBase;
        [LabelText("生产一次所需要的时间"), ShowInInspector, ReadOnly]
        public int TimeCost => HasRecipe && Eff > 0 ? (int)Math.Ceiling((double)100 * Recipe.TimeCost / Eff) : 0;
        #endregion

        #region Table Property
        [LabelText("生产节点类型"), ShowInInspector, ReadOnly]
        public ProNodeType ProNodeType => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetProNodeType(ID) : ProNodeType.None;
        [LabelText("生产节点类目"), ShowInInspector, ReadOnly]
        public ML.Engine.InventorySystem.RecipeCategory Category => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetCategory(ID) : ML.Engine.InventorySystem.RecipeCategory.None;
        [LabelText("生产节点可执行配方类目"), ShowInInspector, ReadOnly]
        public List<ML.Engine.InventorySystem.RecipeCategory> RecipeCategoryFilter => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetRecipeCategoryFilterd(ID) : new List<ML.Engine.InventorySystem.RecipeCategory>();
        [LabelText("经验类型"), ShowInInspector, ReadOnly]
        public WorkerNS.WorkType ExpType => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetExpType(ID) : WorkerNS.WorkType.None;
        [LabelText("堆放上限数"), ShowInInspector, ReadOnly]
        public int StackMax => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetMaxStack(ID) : 0;
        [LabelText("堆放阈值数"), ShowInInspector, ReadOnly]
        public int StackThreshold => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetStackThreshold(ID) : 0;
        [LabelText("需求阈值数"), ShowInInspector, ReadOnly]
        public int RawThreshold => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetRawThreshold(ID) : 0;
        [LabelText("是否需要供电"), ShowInInspector, FoldoutGroup("配置")]
        public bool RequirePower => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetCanCharge(ID) : false;
        #endregion

        #region ProNode
        public ProNode(ProNodeTableData config)
        {
            ID = config.ID ?? "";
            EffBase = LocalGameManager.Instance.ProNodeManager.EffBase;
            InitData(0 , 0);
            DataContainer.OnDataChangeEvent += OnContainerDataChangeEvent;
        }

        public void Destroy()
        {
            DataContainer.OnDataChangeEvent -= OnContainerDataChangeEvent;
            RemoveRecipe();
            (this as WorkerNS.IWorkerContainer).RemoveWorker();
        }

        /// <summary>
        /// 开始运行生产节点，这个时候并不一定开始制作物品
        /// </summary>
        public void StartRun()
        {
            TimerForMission.Start();
            StartProduce();
        }

        /// <summary>
        /// 取消运行，这个时候会停止制作物品
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
        /// 开始制作物品
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
        /// 停止制作物品
        /// </summary>
        public void StopProduce()
        {
            if (IsOnProduce)
            {
                timerForProduce?.End();
            }
        }

        /// <summary>
        /// 是否可以开始制作物品
        /// </summary>
        public bool CanWorking()
        {
            if (HasRecipe)
            {
                foreach (var kv in Recipe.Raw)
                {
                    if (DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) < kv.num) { return false; }
                }
                if (ProNodeType == ProNodeType.Mannul && !(HaveWorker && Worker.IsOnProNodeDuty)) { return false; }
                if (DataContainer.GetAmount(ProductID, DataNS.DataOpType.StorageAll) >= StackMax * ProductNum) { return false; }
                if (RequirePower && WorldProNode.PowerCount <= 0) { return false; }
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// 获取生产节点可以生产的配方
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
            if (0 <= level && level <= LocalGameManager.Instance.ProNodeManager.LevelMax)
            {
                if (level > Level)
                {
                    for (int i = Level; i < level; i++)
                    {
                        EffBase += LocalGameManager.Instance.ProNodeManager.LevelUpgradeEff[i];
                    }
                }
                else if (level < Level)
                {
                    for (int i = Level; i > level; i--)
                    {
                        EffBase -= LocalGameManager.Instance.ProNodeManager.LevelUpgradeEff[i - 1];
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
                missionNum = kv.num * RawThreshold - DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) - (this as MissionNS.IMissionObj).GetMissionAssignNum(kv.id, true);
                if (missionNum > 0)
                {
                    missionNum += kv.num * (StackMax - RawThreshold);
                    LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionNS.MissionTransportType.Store_ProNode, kv.id, missionNum, this, MissionNS.MissionInitiatorType.PutIn_Initiator);
                }
            }
            missionNum = StackReserve - (this as MissionNS.IMissionObj).GetMissionAssignNum(ProductID, false);
            if (missionNum > 0)
            {
                var missionType = ML.Engine.InventorySystem.ItemManager.Instance.GetItemType(ProductID) == ML.Engine.InventorySystem.ItemType.Feed ? 
                    MissionNS.MissionTransportType.ProNode_Restaurant : MissionNS.MissionTransportType.ProNode_Store;
                LocalGameManager.Instance.MissionManager.CreateTransportMission(missionType, ProductID, missionNum, this, MissionNS.MissionInitiatorType.PutOut_Initiator);
            }
        }

        protected void UpdateActionForProduce(double time) { OnProduceUpdateEvent?.Invoke(time); }

        protected void EndActionForProduce()
        {
            ML.Engine.InventorySystem.Item item = Recipe.Composite(this);
            AddItem(item);
            if (Stack >= StackReserve + StackThreshold * ProductNum)
            {
                StackReserve = Stack;
            }
            if (ProNodeType == ProNodeType.Mannul)
            {
                Worker.AlterExp(ExpType, Recipe.ExpRecipe);
                Worker.AlterAP(-1 * Worker.APCost);
                Worker.AlterMood(-1 * Worker.MoodCost);
            }
            // 下一次生产
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
            if (StackReserve > StackAll)
            {
                StackReserve = StackAll;
                int cur = StackReserve;
                foreach (var mission in Missions.ToArray())
                {
                    if (cur > mission.MissionNum)
                    {
                        cur -= mission.MissionNum;
                    }
                    else
                    {
                        mission.ChangeMissionNum(cur);
                        cur = 0;
                    }
                }
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
        public bool HaveWorker => ProNodeType == ProNodeType.Mannul && Worker != null && !string.IsNullOrEmpty(Worker.InstanceID);

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
            (this as MissionNS.IMissionObj).UpdateTransport();
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
    }
}