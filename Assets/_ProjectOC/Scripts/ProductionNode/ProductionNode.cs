using ML.Engine.Manager;
using ML.Engine.Timer;
using ML.Engine.InventorySystem;
using ProjectOC.MissionNS;
using ProjectOC.WorkerNS;
using System.Collections.Generic;
using System;

namespace ProjectOC.ProductionNodeNS
{
    /// <summary>
    /// 生产节点
    /// </summary>
    [System.Serializable]
    public class ProductionNode
    {
        /// <summary>
        /// 建筑实例ID，全局唯一
        /// </summary>
        public string UID = "";
        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// 名称
        /// </summary>
        public string Name = "";
        /// <summary>
        /// 生产节点类型
        /// </summary>
        public ProductionNodeType Type;
        /// <summary>
        /// 生产节点类目
        /// </summary>
        public ProductionNodeCategory Category;
        /// <summary>
        /// 生产节点可执行配方类目
        /// </summary>
        public List<ItemCategory> RecipeCategoryFiltered = new List<ItemCategory>();
        /// <summary>
        /// 经验类型
        /// </summary>
        public WorkType ExpType;
        
        /// <summary>
        /// 正在生产的配方
        /// </summary>
        public Recipe Recipe;
        /// <summary>
        /// 人工生产节点常驻刁民
        /// </summary>
        public Worker Worker;
        /// <summary>
        /// 生产节点状态
        /// </summary>
        public ProductionNodeState State{ 
            get 
            {
                if (this.Recipe == null)
                {
                    return ProductionNodeState.Vacancy;
                }
                else
                {
                    if (this.timerForProduce != null && !this.timerForProduce.IsStoped)
                    {
                        return ProductionNodeState.Production;
                    }
                    else
                    {
                        return ProductionNodeState.Stagnation;
                    }
                }
            } 
        }
        /// <summary>
        /// 搬运优先级
        /// </summary>
        public PriorityTransport PriorityTransport;
        /// <summary>
        /// 已经分配的搬运任务
        /// </summary>
        public List<MissionTransport> MissionTransports = new List<MissionTransport>();

        /// <summary>
        /// 基础生产效率 单位%
        /// </summary>
        public int EffBase { get; private set; } = 100;
        /// <summary>
        /// 生产效率 单位%
        /// </summary>
        public int Eff
        {
            get
            {
                if ((this.Type == ProductionNodeType.Mannul) && this.Worker != null)
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
                    return (int)(this.Recipe.TimeCost / this.Eff) + 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level;
        /// <summary>
        /// 最大等级
        /// </summary>
        public int LevelMax = 3;
        /// <summary>
        /// 升级所需的材料
        /// </summary>
        public List<Dictionary<string, int>> LevelUpgradeRequire = new List<Dictionary<string, int>>();
        /// <summary>
        /// 升级提高的基础生产效率是类默认值，个体间没有差异，不进表。
        /// </summary>
        public List<int> LevelUpgradeEff = new List<int>() { 50, 50, 50 };

        /// <summary>
        /// 当前堆积总值
        /// </summary>
        public int StackNumCur { get { return StackNumReserve + StackNum; } }
        /// <summary>
        /// 堆放暂存上限
        /// </summary>
        public int StackNumMax;
        /// <summary>
        /// 当前没有分配给任务的堆积值
        /// 玩家只能拿取此项，第一个进度条
        /// </summary>
        public int StackNum { get; protected set; }
        /// <summary>
        /// 当前已分配给任务但尚未被搬运的堆积值
        /// Worker只能拿取此项，第二个进度条
        /// </summary>
        public int StackNumReserve { get; protected set; }
        /// <summary>
        /// 堆积搬运阈值，未分配给任务的量达到此值，全部划分给任务，然后生成任务
        /// </summary>
        public int StackCarryThreshold;
        /// <summary>
        ///  当原材料可生产的Item份数低于此值时，发布搬运任务
        ///  搬运 MaxStackNum - 此值份量的原材料到此生产节点
        /// </summary>
        public int NeedQuantityThreshold;
        /// <summary>
        ///  原材料ID, 还有多少份
        /// </summary>
        protected Dictionary<string, int> RawItems = new Dictionary<string, int>();

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
                }
                return timerForProduce;
            }
        }
        /// <summary>
        /// 任务计时器
        /// 在计时器内部开始运行和结束时进行判断
        /// 每完成一次更新一下任务 UpdateMission
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
        public RecipeManager RecipeManager { get => RecipeManager.Instance; }

        public void Init(ProductionNodeManager.ProductionNodeTableJsonData config)
        {
            this.ID = config.id;
            this.Name = config.name;
            this.Type = config.type;
            this.Category = config.category;
            this.RecipeCategoryFiltered = new List<ItemCategory>(config.recipeCategoryFiltered);
            this.ExpType = config.expType;
            this.LevelUpgradeRequire = new List<Dictionary<string, int>>();
            foreach (var dict in config.levelUpgradeRequire)
            {
                this.LevelUpgradeRequire.Add(new Dictionary<string, int>(dict));
            }
            this.StackNumMax = config.stackNumMax;
            this.StackCarryThreshold = config.stackCarryThreshold;
            this.NeedQuantityThreshold = config.needQuantityThreshold;
        }
        public void Init(ProductionNode node)
        {
            this.UID = node.UID;
            this.ID = node.ID;
            this.Name = node.Name;
            this.Type = node.Type;
            this.Category = node.Category;
            this.RecipeCategoryFiltered = new List<ItemCategory>(node.RecipeCategoryFiltered);
            this.ExpType = node.ExpType;
            this.Recipe = node.Recipe;
            this.Worker = node.Worker;
            this.PriorityTransport = node.PriorityTransport;
            this.MissionTransports = new List<MissionTransport>(node.MissionTransports);
            this.EffBase = node.EffBase;
            this.Level = node.Level;
            this.LevelMax = node.LevelMax;
            this.LevelUpgradeRequire = new List<Dictionary<string, int>>();
            foreach (var dict in node.LevelUpgradeRequire)
            {
                this.LevelUpgradeRequire.Add(new Dictionary<string, int>(dict));
            }
            this.LevelUpgradeEff = new List<int>(node.LevelUpgradeEff);
            this.StackNumMax = node.StackNumMax;
            this.StackNum = node.StackNum;
            this.StackNumReserve = node.StackNumReserve;
            this.StackCarryThreshold = node.StackCarryThreshold;
            this.NeedQuantityThreshold = node.NeedQuantityThreshold;
            this.RawItems = new Dictionary<string, int>(node.RawItems);
        }

        /// <summary>
        /// 获取生产节点可以生产的配方
        /// </summary>
        /// <returns>配方ID列表</returns>
        public List<string> GetCanProduceRecipe()
        {
            List<string> result = new List<string>();
            foreach (ItemCategory recipeCategory in this.RecipeCategoryFiltered)
            {
                result.AddRange(RecipeManager.GetRecipeIDsByCategory(recipeCategory));
            }
            return result;
        }
        /// <summary>
        /// 更改生产项
        /// </summary>
        /// <param name="recipeID">配方ID</param>
        /// <returns>更改是否成功</returns>
        public bool ChangeRecipe(string recipeID)
        {
            Recipe recipe = RecipeManager.SpawnRecipe(recipeID);
            if (recipe != null)
            {
                this.RemoveRecipe();
                this.Recipe = recipe;
                foreach (string itemID in this.Recipe.RawItems.Keys)
                {
                    this.RawItems.Add(itemID, 0);
                }
                this.StartRunning();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 移除当前生产项
        /// </summary>
        public void RemoveRecipe()
        {
            this.StopRunning();
            // TODO: 将堆放的成品，素材，以及被打断项的素材，全部返还至玩家背包
            if (this.Recipe != null)
            {
                // this.Recipe.ProductItems; 成品物品ID
                // this.StackNumCur; 成品物品份数
            }
            foreach (var items in this.RawItems)
            {
                // items.Key; 素材物品ID  items.Value; 素材物品份数
            }
            // 清空数据
            this.Recipe = null;
            this.StackNum = 0;
            this.StackNumReserve = 0;
            this.RawItems.Clear();
        }
        /// <summary>
        /// 更改在岗刁民
        /// </summary>
        /// <param name="worker">新刁民</param>
        /// <returns>更改是否成功</returns>
        public bool ChangeWorker(Worker worker)
        {
            if (this.Type == ProductionNodeType.Mannul && worker != null)
            {
                this.RemoveWorker();
                worker.DutyProductionNode = this;
                worker.SetTimeStatusAll(TimeStatus.Work_OnDuty);
                this.Worker = worker;
                this.StartWorking();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 移除在岗刁民
        /// </summary>
        /// <returns>是否成功</returns>
        public bool RemoveWorker()
        {
            if (this.Type == ProductionNodeType.Mannul)
            {
                this.EndWorking();
                if (this.Worker != null)
                {
                    this.Worker.SetTimeStatusAll(TimeStatus.Relax);
                    this.Worker.IsOnDuty = false;
                    this.Worker.DutyProductionNode = null;
                    this.Worker = null;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 开始运行生产节点，这个时候并不一定开始制作物品
        /// </summary>
        public void StartRunning()
        {
            this.TimerForMission.Start();
            this.StartWorking();
        }
        /// <summary>
        /// 取消运行，这个时候会停止制作物品
        /// </summary>
        public void StopRunning()
        {
            if (this.timerForMission != null)
            {
                this.TimerForMission.End();
            }
            this.EndWorking();
        }
        /// <summary>
        /// 开始制作物品
        /// TODO:刁民状态改变时调用该方法
        /// </summary>
        public bool StartWorking()
        {
            if (this.CanWorking())
            {
                foreach (var kv in this.Recipe.RawItems)
                {
                    this.RawItems[kv.Key] -= kv.Value;
                }
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
        public void EndWorking()
        {
            if (this.timerForProduce != null)
            {
                // 生产一半停止返回生产材料
                if (!this.TimerForProduce.IsStoped)
                {
                    foreach (var kv in this.Recipe.RawItems)
                    {
                        this.RawItems[kv.Key] += kv.Value;
                    }
                }
                this.TimerForProduce.End();
            }
        }
        /// <summary>
        /// 是否可以开始制作物品
        /// </summary>
        /// <returns></returns>
        public bool CanWorking()
        {
            if (this.Recipe != null)
            {
                foreach (var kv in this.Recipe.RawItems)
                {
                    if (kv.Value > this.RawItems[kv.Key])
                    {
                        return false;
                    }
                }
                if (this.Type == ProductionNodeType.Mannul && (this.Worker == null || !this.Worker.IsOnDuty))
                {
                    return false;
                }
                if (this.StackNumCur >= this.StackNumMax)
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
        /// 增加原料
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="num">增加的数量</param>
        /// <param name="isWorker">是否是刁民操作</param>
        /// <returns></returns>
        public bool AddRawItem(string itemID, int num, bool isWorker=false)
        {
            if (this.RawItems.ContainsKey(itemID) && num > 0)
            {
                if (!isWorker && (this.RawItems[itemID] + num > this.StackNumMax))
                {
                    return false;
                }
                this.RawItems[itemID] += num;
                this.StartWorking();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 生产节点升级
        /// </summary>
        public bool Upgrade()
        {
            if (this.Level < this.LevelMax && this.Level >= 0)
            {
                // TODO:从背包获取升级材料
                this.EffBase += LevelUpgradeEff[Level];
                this.Level += 1;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 生产节点降级
        /// </summary>
        /// <returns></returns>
        public bool Downgrade()
        {
            if (this.Level <= this.LevelMax && this.Level > 0)
            {
                // TODO:升级材料添加到背包
                this.Level -= 1;
                this.EffBase -= LevelUpgradeEff[Level];
                return true;
            }
            return false;
        }

        /// <summary>
        /// 搬运任务的管理
        /// </summary>
        protected void EndActionForMission()
        {
            int missionNum = 0;
            WorldProductionNode worldNode = ProductionNodeManager.Instance.GetWorldProductionNode(this.UID);
            foreach (var kv in this.Recipe.RawItems)
            {
                missionNum = kv.Value * this.NeedQuantityThreshold - this.RawItems[kv.Key] - GetAssignNum(kv.Key);
                if (missionNum > 0)
                {
                    // 从仓库搬运材料来
                    List<MissionTransport> missions = GameManager.Instance.GetLocalManager<MissionBroadCastManager>()?.
                        CreateRetrievalMission(kv.Key, missionNum, worldNode.gameObject.transform, this.UID);
                    this.MissionTransports.AddRange(missions);
                }
            }
            missionNum = this.StackNumReserve - GetAssignNum(this.Recipe.ProductItem);
            if (missionNum > 0)
            {
                // 搬运产出物品到仓库
                MissionTransport mission = GameManager.Instance.GetLocalManager<MissionBroadCastManager>()?.
                    CreateStoreageMission(this.Recipe.ProductItem, missionNum, worldNode.gameObject.transform, this.UID);
                this.MissionTransports.Add(mission);
            }
        }
        protected void EndActionForProduce()
        {
            // 结算
            StackNum += 1;
            if (StackNum >= StackCarryThreshold)
            {
                StackNumReserve += StackNum;
                StackNum = 0;
            }
            this.Worker.AlterExp(this.ExpType, this.Recipe.ExpRecipe * this.Worker.ExpRate[this.ExpType]);
            this.Worker.AlterAP(-1 * this.Worker.APCost);
            // 下一次生产
            if (!this.StartWorking())
            {
                this.EndWorking();
            }
        }
        /// <summary>
        /// 获取已经分配任务的物品数量
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="isIn">true表示搬入，false表示搬出</param>
        /// <returns></returns>
        private int GetAssignNum(string itemID, bool isIn=true)
        {
            int result = 0;
            foreach (MissionTransport mission in this.MissionTransports)
            {
                if (mission != null && mission.ItemID == itemID)
                {
                    if ((isIn && mission.TargetUID == this.UID) || (!isIn && mission.SourceUID == this.UID))
                    {
                        result += mission.MissionNum;
                    }
                }
            }
            return result;
        }
    }
}