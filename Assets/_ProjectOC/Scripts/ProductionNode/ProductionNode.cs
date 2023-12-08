using ML.Engine.Manager;
using ML.Engine.Timer;
using ProjectOC.ItemNS;
using ProjectOC.MissionNS;
using ProjectOC.WorkerNS;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ProductionNodeNS
{
    /// <summary>
    /// 生产节点
    /// </summary>
    [System.Serializable]
    public abstract class ProductionNode : MonoBehaviour, ITickComponent
    {
        /// <summary>
        /// 场景实例化ID，全局唯一
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
        public ProductionNodeState State;
        /// <summary>
        /// 搬运优先级
        /// </summary>
        public PriorityTransport PriorityTransport;

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
        public int StackNumCur { get { return StackNumAssign+ StackNumNoAssign; } }
        /// <summary>
        /// 堆放暂存上限
        /// </summary>
        public int StackNumMax;
        /// <summary>
        /// 当前没有分配给任务的堆积值
        /// 玩家只能拿取此项，第一个进度条
        /// </summary>
        public int StackNumNoAssign { get; protected set; }
        /// <summary>
        /// 当前已分配给任务但尚未被搬运的堆积值
        /// Worker只能拿取此项，第二个进度条
        /// </summary>
        public int StackNumAssign { get; protected set; }
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

        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public RecipeManager RecipeManager { get => GameManager.Instance.GetLocalManager<RecipeManager>(); }

        public void InitProductionNode(string id)
        {
            GameManager.Instance.TickManager.RegisterTick(tickPriority, this);
            // TODO: 读表拿数据
            this.ID = id;
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
            Recipe recipe = RecipeManager.CreateRecipe(recipeID);
            if (recipe != null)
            {
                this.RemoveRecipe();
                this.Recipe = recipe;
                this.State = ProductionNodeState.Stagnation;
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
            this.StackNumNoAssign = 0;
            this.StackNumAssign = 0;
            this.RawItems.Clear();
            this.State = ProductionNodeState.Vacancy;
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
        /// 更改在岗刁民
        /// </summary>
        /// <param name="worker">新刁民</param>
        /// <returns>更改是否成功</returns>
        public bool ChangeWorker(Worker worker)
        {
            if (this.Type == ProductionNodeType.Mannul && worker != null)
            {
                this.RemoveWorker();
                worker.Status = Status.Fishing;
                worker.IsOnDuty = true;
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
                    this.Worker.Status = Status.Fishing;
                    this.Worker.IsOnDuty = false;
                    this.Worker.SetTimeStatusAll(TimeStatus.Relax);
                    this.Worker = null;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 开始制作物品
        /// </summary>
        public void StartWorking()
        {
            if (this.CanWorking())
            {
                foreach (var kv in this.Recipe.RawItems)
                {
                    this.RawItems[kv.Key] -= kv.Value;
                }
                // 启动生产计时器
                this.TimerForProduce.Reset(this.TimeCost);
                if (this.Type == ProductionNodeType.Mannul)
                {
                    this.Worker.Status = Status.Working;
                }
                this.State = ProductionNodeState.Production;
            }
        }
        /// <summary>
        /// 停止制作物品
        /// </summary>
        public void EndWorking()
        {
            // 停止生产计时器
            if (this.Recipe != null)
            {
                this.State = ProductionNodeState.Stagnation;
            }
            else
            {
                this.State = ProductionNodeState.Vacancy;
            }
            if (this.Type == ProductionNodeType.Mannul && this.Worker != null)
            {
                this.Worker.Status = Status.Fishing;
            }
            if (this.timerForProduce != null)
            {
                this.TimerForProduce.End();
                this.timerForProduce = null;
            }
        }

        /// <summary>
        /// 是否可以开始生产
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
                if (this.Type == ProductionNodeType.Mannul && this.Worker == null)
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
        /// 是否正在运行
        /// </summary>
        /// <returns></returns>
        public bool IsWorking()
        {
            if (this.timerForProduce != null && !this.timerForProduce.IsStoped)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 原料是否充足
        /// 没配方默认原料充足
        /// </summary>
        /// <returns></returns>
        public bool IsRawItemsEnough()
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
            }
            return true;
        }
        /// <summary>
        /// 产出是否堆积满
        /// </summary>
        /// <returns></returns>
        public bool IsStackMax()
        {
            return this.StackNumCur >= this.StackNumMax;
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

        protected void EndActionForMission()
        {
            // 只有有Recipe时才会让TimerForMission运行
            foreach (var kv in this.Recipe.RawItems)
            {
                if (this.RawItems[kv.Key] < kv.Value * this.NeedQuantityThreshold)
                {
                    // TODO:分发任务，从仓库搬运kv.Value * this.NeedQuantityThreshold - this.RawItems[kv.Key]份材料来
                }
            }
            if (this.StackNumAssign > 0)
            {
                // TODO:分发任务，搬运StackNumNoAssignToMission份产出物品到仓库
            }
        }
        protected void EndActionForProduce()
        {
            // 结算
            if (this.CanWorking())
            {
                StackNumNoAssign += 1;
                if (StackNumNoAssign >= StackCarryThreshold)
                {
                    StackNumAssign += StackNumNoAssign;
                    StackNumNoAssign = 0;
                }
                this.Worker.AlterExp(this.ExpType, this.Recipe.ExpRecipe * this.Worker.ExpRate[this.ExpType]);
                this.Worker.AlterAP(this.Worker.APCost);
            }
            // 下一次生产
            if (this.CanWorking())
            {
                this.StartWorking();
            }
            else
            {
                this.EndWorking();
            }
        }
    }
}