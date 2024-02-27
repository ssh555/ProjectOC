using ML.Engine.Timer;
using ML.Engine.InventorySystem;
using ProjectOC.MissionNS;
using ProjectOC.WorkerNS;
using System.Collections.Generic;
using System;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.BuildingSystem;
using ProjectOC.StoreNS;

namespace ProjectOC.ProNodeNS
{
    /// <summary>
    /// 生产节点
    /// </summary>
    [System.Serializable]
    public class ProNode: IMissionObj, IInventory
    {
        /// <summary>
        /// 对应的全局生产节点
        /// </summary>
        public WorldProNode WorldProNode;

        /// <summary>
        /// 建筑实例ID，全局唯一
        /// </summary>
        public string UID { get { return WorldProNode?.InstanceID ?? ""; } }

        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";

        #region 读表数据
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetName(ID); }
        /// <summary>
        /// 生产节点类型
        /// </summary>
        public ProNodeType ProNodeType { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetProNodeType(ID); }
        /// <summary>
        /// 生产节点类目
        /// </summary>
        public RecipeCategory Category { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetCategory(ID); }
        /// <summary>
        /// 生产节点可执行配方类目
        /// </summary>
        public List<RecipeCategory> RecipeCategoryFilter { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRecipeCategoryFilterd(ID); }
        /// <summary>
        /// 经验类型
        /// </summary>
        public WorkType ExpType { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetExpType(ID); }
        /// <summary>
        /// 堆放暂存上限份数
        /// </summary>
        public int StackMaxNum { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetStack(ID); }
        /// <summary>
        /// 堆积搬运阈值，未分配给任务的份数达到此值，全部划分给任务，然后生成任务
        /// </summary>
        //public int StackThresholdNum { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetStackThreshold(ID); }
        public int StackThresholdNum = 1;
        /// <summary>
        ///  当原材料可生产的Item份数低于此值时，发布搬运任务
        ///  搬运 MaxStackNum - 此值份量的原材料到此生产节点
        /// </summary>
        public int RawThresholdNum { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRawThreshold(ID); }
        #endregion

        #region 不进表的配置数据
        /// <summary>
        /// 基础生产效率，单位%
        /// </summary>
        public int EffBase { get; private set; } = 100;
        /// <summary>
        /// 最大等级
        /// </summary>
        public int LevelMax { get; private set; } = 2;
        /// <summary>
        /// 升级提高的基础生产效率
        /// </summary>
        public List<int> LevelUpgradeEff = new List<int>() { 50, 50, 50 };
        #endregion

        #region Property
        /// <summary>
        /// 总堆积数量
        /// </summary>
        public int StackAll { get { return StackReserve + Stack; } }
        /// <summary>
        /// 总堆积份数
        /// </summary>
        public int StackAllNum { get => StackAll / ProductNum; }
        /// <summary>
        /// 没有分配任务的堆积份数
        /// </summary>
        public int StackNum { get => Stack / ProductNum; }
        /// <summary>
        /// 生产物ID
        /// </summary>
        public string ProductItem { get => Recipe?.ProductID ?? ""; }
        /// <summary>
        /// 一次生产的生产物数量
        /// </summary>
        public int ProductNum { get => Recipe?.ProductNum ?? 0; }
        /// <summary>
        /// 生产节点状态
        /// </summary>
        public ProNodeState State
        {
            get
            {
                if (this.Recipe == null)
                {
                    return ProNodeState.Vacancy;
                }
                else
                {
                    if (this.timerForProduce != null && !this.timerForProduce.IsStoped)
                    {
                        return ProNodeState.Production;
                    }
                    else
                    {
                        return ProNodeState.Stagnation;
                    }
                }
            }
        }
        /// <summary>
        /// 生产效率 单位%
        /// </summary>
        public int Eff
        {
            get
            {
                if ((this.ProNodeType == ProNodeType.Mannul) && this.Worker != null)
                {
                    return this.EffBase + this.Worker.Eff[this.ExpType];
                }
                else
                {
                    return this.EffBase;
                }
            }
        }
        /// <summary>
        /// 生产一次所需要的时间
        /// </summary>
        public int TimeCost
        {
            get
            {
                if (this.Recipe != null && this.Eff > 0)
                {
                    if ((100 * Recipe.TimeCost) % Eff == 0)
                    {
                        return (int)(100 * this.Recipe.TimeCost / this.Eff);
                    }
                    else
                    {
                        return (int)(100 * this.Recipe.TimeCost / this.Eff) + 1;
                    }
                }
                return 0;
            }
        }
        #endregion

        /// <summary>
        /// 等级
        /// </summary>
        public int Level = 0;
        /// <summary>
        /// 搬运优先级
        /// </summary>
        public TransportPriority TransportPriority = TransportPriority.Normal;
        /// <summary>
        /// 正在生产的配方
        /// </summary>
        public Recipe Recipe;
        /// <summary>
        /// 人工生产节点常驻刁民
        /// </summary>
        public Worker Worker;
        /// <summary>
        /// 已经分配的搬运任务
        /// </summary>
        public List<MissionTransport> MissionTransports = new List<MissionTransport>();
        /// <summary>
        /// 没有分配任务的堆积值
        /// 玩家优先拿取此项，第一个进度条
        /// </summary>
        public int Stack { get; protected set; }
        /// <summary>
        /// 已分配给任务但尚未被搬运的堆积值
        /// Worker只能拿取此项，第二个进度条
        /// </summary>
        public int StackReserve { get; protected set; }
        /// <summary>
        ///  原材料ID, 还有多少个
        /// </summary>
        protected Dictionary<string, int> RawItems = new Dictionary<string, int>();
        public event Action OnActionChange;
        public event Action<double> onProduceTimerUpdate;
        public event Action<double> OnProduceTimerUpdate
        {
            add 
            { 
                onProduceTimerUpdate += value;
                if (timerForProduce != null)
                {
                    timerForProduce.OnUpdateEvent += value;
                }
            }
            remove 
            { 
                onProduceTimerUpdate -= value;
                if (timerForProduce != null)
                {
                    timerForProduce.OnUpdateEvent -= value;
                }
            }
        }
        public event Action OnProduceEnd;
        #region 计时器
        /// <summary>
        /// 生产计时器，时间为配方生产一次所需的时间
        /// </summary>
        protected CounterDownTimer timerForProduce;
        /// <summary>
        /// 生产计时器，时间为配方生产一次所需的时间
        /// </summary>
        protected CounterDownTimer TimerForProduce
        {
            get
            {
                if (timerForProduce == null)
                {
                    timerForProduce = new CounterDownTimer(this.TimeCost, false, false);
                    timerForProduce.OnEndEvent += EndActionForProduce;
                    timerForProduce.OnUpdateEvent += onProduceTimerUpdate;
                }
                return timerForProduce;
            }
        }

        /// <summary>
        /// 任务计时器
        /// </summary>
        protected CounterDownTimer timerForMission;
        /// <summary>
        /// 任务计时器
        /// </summary>
        protected CounterDownTimer TimerForMission
        {
            get
            {
                if (timerForMission == null)
                {
                    timerForMission = new CounterDownTimer(1f, true, false);
                    timerForMission.OnEndEvent += EndActionForMission;
                }
                return timerForMission;
            }
        }
        #endregion

        public ProNode(ProNodeTableData config)
        {
            this.ID = config.ID ?? "";
        }

        public bool SetLevel(int level)
        {
            if (0 <= level && level <= LevelMax)
            {
                if (level > Level)
                {
                    for (int i = Level; i < level; i++)
                    {
                        EffBase += LevelUpgradeEff[i];
                    }
                }
                else if (level < Level)
                {
                    for (int i = Level; i > level; i--)
                    {
                        EffBase -= LevelUpgradeEff[i - 1];
                    }
                }
                this.Level = level;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取生产节点可以生产的配方
        /// </summary>
        public List<string> GetCanProduceRecipe()
        {
            List<string> result = new List<string>();
            foreach (RecipeCategory recipeCategory in this.RecipeCategoryFilter)
            {
                result.AddRange(ManagerNS.LocalGameManager.Instance.RecipeManager.GetRecipeIDsByCategory(recipeCategory));
            }
            return result;
        }

        /// <summary>
        /// 更改生产项
        /// </summary>
        /// <param name="recipeID">配方ID</param>
        /// <returns>更改是否成功</returns>
        public bool ChangeRecipe(Player.PlayerCharacter player, string recipeID)
        {
            if (player != null)
            {
                if (!string.IsNullOrEmpty(recipeID))
                {
                    Recipe recipe = ManagerNS.LocalGameManager.Instance.RecipeManager.SpawnRecipe(recipeID);
                    if (recipe != null)
                    {
                        this.RemoveRecipe(player);
                        this.Recipe = recipe;
                        foreach (Formula raw in this.Recipe.Raw)
                        {
                            this.RawItems.Add(raw.id, 0);
                        }
                        this.StartRun();
                        return true;
                    }
                }
                else
                {
                    this.RemoveRecipe(player);
                    this.Recipe = null;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 移除当前生产项
        /// </summary>
        public void RemoveRecipe(Player.PlayerCharacter player)
        {
            this.StopRun();
            // 将堆放的成品，素材，全部返还至玩家背包
            bool flag = false;
            List<Item> resItems = new List<Item>();
            // 堆放的成品
            if (this.Recipe != null && !string.IsNullOrEmpty(ProductItem) && StackAll > 0)
            {
                List<Item> items = ItemManager.Instance.SpawnItems(ProductItem, StackAll);
                foreach (var item in items)
                {
                    if (flag)
                    {
                        resItems.Add(item);
                    }
                    else
                    {
                        if (!player.Inventory.AddItem(item))
                        {
                            flag = true;
                        }
                    }
                }
            }
            // 素材
            foreach (var raw in this.RawItems)
            {
                flag = false;
                List<Item> items = ItemManager.Instance.SpawnItems(raw.Key, raw.Value);
                foreach (var item in items)
                {
                    if (flag)
                    {
                        resItems.Add(item);
                    }
                    else
                    {
                        if (!player.Inventory.AddItem(item))
                        {
                            flag = true;
                        }
                    }
                }
            }
            // 没有加到玩家背包的都变成WorldItem
            foreach (Item item in resItems)
            {
                ItemManager.Instance.SpawnWorldItem(item, WorldProNode.transform.position, WorldProNode.transform.rotation);
            }
            // 清空数据
            foreach (MissionTransport mission in this.MissionTransports)
            {
                mission.End(false);
            }
            this.MissionTransports.Clear();
            this.Stack = 0;
            this.StackReserve = 0;
            this.RawItems.Clear();
            this.Recipe = null;
        }

        /// <summary>
        /// 更改在岗刁民
        /// </summary>
        /// <param name="worker">新刁民</param>
        /// <returns>更改是否成功</returns>
        public bool ChangeWorker(Worker worker)
        {
            if (this.ProNodeType == ProNodeType.Mannul && worker != null)
            {
                this.RemoveWorker();
                worker.ChangeProNode(this);
                worker.SetTimeStatusAll(TimeStatus.Work_OnDuty);
                worker.ClearDestination();
                this.Worker = worker;
                worker.SetDestination(WorldProNode.transform, ProNode_Action);
                worker.StatusChangeAction += OnWorkerStatusChangeAction;
                return true;
            }
            return false;
        }

        public void ProNode_Action(Worker worker)
        {
            worker.ArriveProNode = true;
            worker.gameObject.SetActive(false);
            worker.ClearDestination();
            this.StartProduce();
        }

        /// <summary>
        /// 移除在岗刁民
        /// </summary>
        /// <returns>是否成功</returns>
        public bool RemoveWorker()
        {
            if (this.ProNodeType == ProNodeType.Mannul)
            {
                this.StopProduce();
                if (this.Worker != null)
                {
                    this.Worker.StatusChangeAction -= OnWorkerStatusChangeAction;
                    this.Worker.SetTimeStatusAll(TimeStatus.Relax);
                    this.Worker.ClearDestination();
                    this.Worker.ProNode = null;
                    this.Worker.gameObject.SetActive(true);
                    this.Worker = null;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 开始运行生产节点，这个时候并不一定开始制作物品
        /// </summary>
        public void StartRun()
        {
            this.TimerForMission.Start();
            this.StartProduce();
        }

        /// <summary>
        /// 取消运行，这个时候会停止制作物品
        /// </summary>
        public void StopRun()
        {
            if (this.timerForMission != null)
            {
                this.TimerForMission.End();
            }
            this.StopProduce();
        }

        /// <summary>
        /// 开始制作物品
        /// </summary>
        public bool StartProduce()
        {
            if (this.CanWorking() && (timerForProduce == null || TimerForProduce.IsStoped))
            {
                // 启动生产计时器
                this.TimerForProduce.Reset(this.TimeCost);
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
            if (this.timerForProduce != null)
            {
                this.TimerForProduce.End();
            }
        }

        /// <summary>
        /// 是否可以开始制作物品
        /// </summary>
        public bool CanWorking()
        {
            if (this.Recipe != null)
            {
                foreach (var kv in this.Recipe.Raw)
                {
                    if (this.RawItems[kv.id] < kv.num)
                    {
                        return false;
                    }
                }
                if (this.ProNodeType == ProNodeType.Mannul && (this.Worker == null || !this.Worker.IsOnDuty))
                {
                    return false;
                }
                if (this.StackAllNum >= this.StackMaxNum)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 搬运任务的管理
        /// </summary>
        protected void EndActionForMission()
        {
            int missionNum;
            foreach (var kv in Recipe.Raw)
            {
                missionNum = kv.num * RawThresholdNum - RawItems[kv.id] - GetAssignNum(kv.id, true);
                if (missionNum > 0)
                {
                    // 从仓库搬运材料来
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionTransportType.Store_ProNode, kv.id, missionNum, this);
                }
            }
            missionNum = StackReserve - GetAssignNum(ProductItem, false);
            if (missionNum > 0)
            {
                // 搬运产出物品到仓库
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionTransportType.ProNode_Store, ProductItem, missionNum, this);
            }
        }

        protected void EndActionForProduce()
        {
            // 结算
            Item item = Recipe.Composite(this);
            AddItem(item);
            Worker.AlterExp(ExpType, Recipe.ExpRecipe);
            Worker.AlterAP(-1 * Worker.APCost);
            // 下一次生产
            if (!StartProduce())
            {
                StopProduce();
            }
            OnActionChange?.Invoke();
            OnProduceEnd?.Invoke();
        }

        private void OnWorkerStatusChangeAction(Status status)
        {
            if (status != Status.Relaxing)
            {
                StartProduce();
                OnActionChange?.Invoke();
            }
            else
            {
                StopProduce();
                OnActionChange?.Invoke();
            }
        }

        /// <summary>
        /// 获取已经分配任务的物品数量
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="isIn">true表示放入，false表示取出</param>
        /// <returns></returns>
        private int GetAssignNum(string itemID, bool isIn=true)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (MissionTransport mission in this.MissionTransports)
                {
                    if (mission != null && mission.ItemID == itemID)
                    {
                        if ((isIn && mission.Type == MissionTransportType.Store_ProNode) ||
                            (!isIn && mission.Type == MissionTransportType.ProNode_Store))
                        {
                            result += mission.MissionNum;
                        }
                    }
                }
            }
            return result;
        }

        #region UI接口
        public void UIAdd(Player.PlayerCharacter player, string itemID, int amount)
        {
            if (player != null && !string.IsNullOrEmpty(itemID) && RawItems.ContainsKey(itemID) && amount > 0 && 
                this.RawItems[itemID] + amount <= this.StackMaxNum * this.Recipe.GetRawNum(itemID))
            {
                if (player.Inventory.GetItemAllNum(itemID) >= amount)
                {
                    if (player.Inventory.RemoveItem(itemID, amount))
                    {
                        RawItems[itemID] += amount;
                        StartProduce();
                    }
                    else
                    {
                        //Debug.LogError("ProNode UIAdd Error");
                    }
                }
            }
        }
        public void UIRemove(Player.PlayerCharacter player, int amount)
        {
            if (player != null && amount > 0 && Stack >= amount)
            {
                List<Item> items = ItemManager.Instance.SpawnItems(ProductItem, amount);
                foreach (Item item in items)
                {
                    int itemAmount = item.Amount;
                    if (player.Inventory.AddItem(item))
                    {
                        Stack -= itemAmount;
                    }
                    else
                    {
                        //Debug.LogError("ProNode UIRemove Error");
                        break;
                    }
                }
                StartProduce();
            }
        }
        public void UIFastAdd(Player.PlayerCharacter player, string itemID)
        {
            if (player != null && !string.IsNullOrEmpty(itemID) && RawItems.ContainsKey(itemID))
            {
                int amount = player.Inventory.GetItemAllNum(itemID);
                int maxAmount = StackMaxNum * Recipe.GetRawNum(itemID) - RawItems[itemID];
                amount = amount >= maxAmount ? maxAmount : amount;
                if (player.Inventory.RemoveItem(itemID, amount))
                {
                    RawItems[itemID] += amount;
                    StartProduce();
                }
                else
                {
                    //Debug.LogError("ProNode UIFastAdd Error");
                }
            }
        }
        public void UIFastRemove(Player.PlayerCharacter player)
        {
            if (player != null)
            {
                List<Item> items = ItemManager.Instance.SpawnItems(ProductItem, Stack);
                foreach (Item item in items)
                {
                    int itemAmount = item.Amount;
                    if (player.Inventory.AddItem(item))
                    {
                        Stack -= itemAmount;
                    }
                    else
                    {
                        //Debug.LogError("ProNode UIFastRemove Error");
                        break;
                    }
                }
                StartProduce();
            }
        }

        //public void Upgrade(Player.PlayerCharacter player)
        //{
        //    if (this.WorldProNode != null)
        //    {
        //        string upgradeRawID = BuildingManager.Instance.GetUpgradeRaw(this.WorldProNode.Classification.ToString().Replace('-', '_'));
        //        CompositeManager.CompositionObjectType compObjType = CompositeManager.Instance.Composite(player.Inventory, upgradeRawID, out var composition);
        //        if (compObjType == CompositeManager.CompositionObjectType.BuildingPart && composition is WorldProNode upgrade)
        //        {
        //            upgrade.InstanceID = this.WorldProNode.InstanceID;
        //            upgrade.transform.position = this.WorldProNode.transform.position;
        //            upgrade.transform.rotation = this.WorldProNode.transform.rotation;
        //            UnityEngine.Object.Destroy(this.WorldProNode.gameObject);
        //            this.WorldProNode = upgrade;
        //            upgrade.ProNode = this;
        //            this.SetLevel(upgrade.Classification.Category4 - 1);
        //        }
        //    }
        //}
        public void Upgrade(Player.PlayerCharacter player)
        {
            if (this.WorldProNode != null)
            {
                string ID = BuildingManager.Instance.GetActorID(this.WorldProNode.Classification.ToString().Replace('-', '_'));
                string upgradeID = BuildingManager.Instance.GetUpgradeID(this.WorldProNode.Classification.ToString().Replace('-', '_'));
                string upgradeCID = BuildingManager.Instance.GetUpgradeCID(this.WorldProNode.Classification.ToString().Replace('-', '_'));

                if (!string.IsNullOrEmpty(upgradeID)
                    && !string.IsNullOrEmpty(upgradeCID)
                    && BuildingManager.Instance.IsValidBPartID(upgradeCID)
                    && CompositeManager.Instance.OnlyCostResource(player.Inventory, $"{ID}_{upgradeID}"))
                {
                    if (BuildingManager.Instance.GetOneBPartInstance(upgradeCID) is WorldProNode upgrade)
                    {
                        upgrade.InstanceID = this.WorldProNode.InstanceID;
                        upgrade.transform.position = this.WorldProNode.transform.position;
                        upgrade.transform.rotation = this.WorldProNode.transform.rotation;
                        UnityEngine.Object.Destroy(this.WorldProNode.gameObject);
                        this.WorldProNode = upgrade;
                        upgrade.ProNode = this;
                        this.SetLevel(upgrade.Classification.Category4 - 1);
                    }
                }
            }
        }
        #endregion

        #region IMission接口
        public Transform GetTransform()
        {
            return WorldProNode?.transform;
        }
        public TransportPriority GetTransportPriority()
        {
            return TransportPriority;
        }
        public string GetUID()
        {
            return UID;
        }
        public void AddTransport(Transport transport) {}
        public void RemoveTranport(Transport transport) {}
        public void AddMissionTranport(MissionTransport mission)
        {
            MissionTransports.Add(mission);
        }
        public void RemoveMissionTranport(MissionTransport mission)
        {
            MissionTransports.Remove(mission);
        }
        public bool PutIn(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && RawItems.ContainsKey(itemID) && amount >= 0)
            {
                RawItems[itemID] += amount;
                StartProduce();
                OnActionChange?.Invoke();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 返回取出的数量
        /// </summary>
        public int PutOut(string itemID, int amount)
        {
            if (ProductItem == itemID && amount > 0)
            { 
                if (StackReserve >= amount)
                {
                    StackReserve -= amount;
                }
                else
                {
                    amount = StackReserve;
                    StackReserve = 0;
                }
                OnActionChange?.Invoke();
                return amount;
            }
            return 0;
        }
        #endregion

        #region IInventory接口
        public bool AddItem(Item item)
        {
            if (item.Amount >= 0)
            {
                if (this.RawItems.ContainsKey(item.ID))
                {
                    if (this.RawItems[item.ID] + item.Amount > this.StackMaxNum * this.Recipe.GetRawNum(item.ID))
                    {
                        return false;
                    }
                    this.RawItems[item.ID] += item.Amount;
                    this.StartProduce();
                    return true;
                }
                if (ProductItem == item.ID)
                {
                    Stack += item.Amount;
                    if (StackNum >= StackThresholdNum)
                    {
                        StackReserve += Stack;
                        Stack = 0;
                    }
                    OnActionChange?.Invoke();
                    return true;
                }
            }
            return false;
        }
        public bool RemoveItem(Item item)
        {
            if (item.Amount >= 0)
            {
                if (RawItems.ContainsKey(item.ID) && RawItems[item.ID] >= item.Amount)
                {
                    RawItems[item.ID] -= item.Amount;
                    return true;
                }
                else if (ProductItem == item.ID && Stack >= item.Amount)
                {
                    Stack -= item.Amount;
                    OnActionChange?.Invoke();
                    return true;
                }
            }
            return false;
        }
        public Item RemoveItem(Item item, int amount)
        {
            if (amount > 0)
            {
                if (RawItems.ContainsKey(item.ID))
                {
                    if (RawItems[item.ID] >= amount)
                    {
                        RawItems[item.ID] -= amount;
                    }
                    else
                    {
                        amount = RawItems[item.ID];
                        RawItems[item.ID] = 0;
                    }
                }
                else if (ProductItem == item.ID)
                {
                    if (Stack >= amount)
                    {
                        Stack -= amount;
                    }
                    else
                    {
                        amount = Stack;
                        Stack = 0;
                    }
                }
            }
            Item result = ItemManager.Instance.SpawnItem(item.ID);
            result.Amount = amount;
            if (result.Amount != amount)
            {
                //Debug.LogError($"Item Amount Error ItemAmount: {result.Amount} Amount: {amount}");
            }
            OnActionChange?.Invoke();
            return result;
        }
        public bool RemoveItem(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && amount >= 0)
            {
                if (RawItems.ContainsKey(itemID))
                {
                    if (RawItems[itemID] >= amount)
                    {
                        RawItems[itemID] -= amount;
                        return true;
                    }
                }
                else if (ProductItem == itemID)
                {
                    if (Stack >= amount)
                    {
                        Stack -= amount;
                        OnActionChange?.Invoke();
                        return true;
                    }
                }
            }
            return false;
        }
        public int GetItemAllNum(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (RawItems.ContainsKey(id))
                {
                    return RawItems[id];
                }
                else if (ProductItem == id)
                {
                    return StackAll;
                }
            }
            //Debug.LogError($"Item {id} is not in ProNode {ID}");
            return 0;
        }

        public Item[] GetItemList()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}