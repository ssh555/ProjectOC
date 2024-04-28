using ProjectOC.WorkerNS;
using ML.Engine.InventorySystem;
using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using ProjectOC.ManagerNS;

namespace ProjectOC.ProNodeNS
{
    [LabelText("生产节点"), Serializable]
    public class ProNode: DataNS.ItemContainerOwner, IWorkerContainer
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
        public Recipe Recipe;
        [LabelText("基础生产效率"), ReadOnly]
        public int EffBase;
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
        public int Stack => DataContainer.GetAmount(Recipe.ProductID, DataNS.DataOpType.Storage);
        [LabelText("总堆积数量"), ShowInInspector, ReadOnly]
        public int StackAll => DataContainer.GetAmount(Recipe.ProductID, DataNS.DataOpType.StorageAll);
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
        public RecipeCategory Category => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetCategory(ID) : RecipeCategory.None;
        [LabelText("生产节点可执行配方类目"), ShowInInspector, ReadOnly]
        public List<RecipeCategory> RecipeCategoryFilter => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetRecipeCategoryFilterd(ID) : new List<RecipeCategory>();
        [LabelText("经验类型"), ShowInInspector, ReadOnly]
        public WorkType ExpType => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetExpType(ID) : WorkType.None;
        [LabelText("堆放上限数"), ShowInInspector, ReadOnly]
        public int StackMax => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetMaxStack(ID) : 0;
        [LabelText("堆放阈值数"), ShowInInspector, ReadOnly]
        public int StackThreshold => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetStackThreshold(ID) : 0;
        [LabelText("需求阈值数"), ShowInInspector, ReadOnly]
        public int RawThreshold => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetRawThreshold(ID) : 0;
        [LabelText("是否需要供电"), ShowInInspector, FoldoutGroup("配置")]
        public bool RequirePower => LocalGameManager.Instance != null ? LocalGameManager.Instance.ProNodeManager.GetCanCharge(ID) : false;
        #endregion

        #region Init Destroy
        public ProNode(ProNodeTableData config)
        {
            ID = config.ID ?? "";
            EffBase = LocalGameManager.Instance.ProNodeManager.EffBase;
        }

        public void Destroy()
        {
            StopRun();
            RemoveRecipe();
            (this as IWorkerContainer).RemoveWorker();
        }
        #endregion

        #region Get
        /// <summary>
        /// 获取生产节点可以生产的配方
        /// </summary>
        public List<string> GetCanProduceRecipe()
        {
            List<string> result = new List<string>();
            foreach (RecipeCategory recipeCategory in RecipeCategoryFilter)
            {
                result.AddRange(LocalGameManager.Instance.RecipeManager.GetRecipeIDsByCategory(recipeCategory));
            }
            return LocalGameManager.Instance.RecipeManager.SortRecipeIDs(result);
        }
        /// <summary>
        /// 获取已经分配任务的物品数量
        /// </summary>
        /// <param name="isIn">true表示放入，false表示取出</param>
        protected int GetAssignNum(string itemID, bool isIn = true)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (MissionNS.MissionTransport mission in (this as MissionNS.IMissionObj).Missions.ToArray())
                {
                    if (mission != null && mission.ID == itemID)
                    {
                        if ((isIn && mission.Type == MissionNS.MissionTransportType.Store_ProNode) ||
                            (!isIn && (mission.Type == MissionNS.MissionTransportType.ProNode_Store || mission.Type == MissionNS.MissionTransportType.ProNode_Restaurant)))
                        {
                            result += mission.MissionNum;
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region Set
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
                    Recipe recipe = LocalGameManager.Instance.RecipeManager.SpawnRecipe(recipeID);
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

        #region Method
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
                missionNum = kv.num * RawThreshold - DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) - GetAssignNum(kv.id, true);
                if (missionNum > 0)
                {
                    missionNum += kv.num * (StackMax - RawThreshold);
                    LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionNS.MissionTransportType.Store_ProNode, kv.id, missionNum, this);
                }
            }
            missionNum = Stack - GetAssignNum(ProductID, false);
            if (missionNum >= StackThreshold * ProductNum)
            {
                var missionType = ItemManager.Instance.GetItemType(ProductID) == ItemType.Feed ? MissionNS.MissionTransportType.ProNode_Restaurant : MissionNS.MissionTransportType.ProNode_Store;
                LocalGameManager.Instance.MissionManager.CreateTransportMission(missionType, ProductID, missionNum, this);
            }
        }

        protected void UpdateActionForProduce(double time) { OnProduceUpdateEvent?.Invoke(time); }

        protected void EndActionForProduce()
        {
            // 结算
            Item item = Recipe.Composite(this);
            AddItem(item);
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
            OnDataChangeEvent?.Invoke();
            OnProduceEndEvent?.Invoke();
        }

        protected void OnWorkerStatusChangeEvent(Status status)
        {
            if (status != Status.Relaxing)
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
        #endregion

        #region ItemContainerOwner
        public override Transform GetTransform() { return WorldProNode.transform; }
        public override string GetUID() { return WorldProNode.InstanceID; }
        public void FastAdd()
        {
            for (int i = 0; i < DataContainer.GetCapacity() - 1; i++) { FastAdd(i); }
        }
        #endregion

        #region IWorkerContainer
        public Action<Worker> OnSetWorkerEvent { get; set; }
        public Action<bool, Worker> OnRemoveWorkerEvent { get; set; }
        public Worker Worker { get; set; }
        public bool IsArrive { get; set; }
        public bool HaveWorker => ProNodeType == ProNodeType.Mannul && Worker != null && !string.IsNullOrEmpty(Worker.InstanceID);

        public WorkerContainerType GetContainerType() { return WorkerContainerType.Work; }

        public void OnArriveEvent(Worker worker)
        {
            (this as IWorkerContainer).OnArriveSetPosition(worker, WorldProNode.transform.position + new Vector3(0, 2f, 0));
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
                Worker.SetTimeStatusAll(TimeStatus.Work_OnDuty);
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