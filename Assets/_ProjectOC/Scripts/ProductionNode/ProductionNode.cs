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
    /// �����ڵ�
    /// </summary>
    [System.Serializable]
    public class ProductionNode
    {
        /// <summary>
        /// ����ʵ��ID��ȫ��Ψһ
        /// </summary>
        public string UID = "";
        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// ����
        /// </summary>
        public string Name = "";
        /// <summary>
        /// �����ڵ�����
        /// </summary>
        public ProductionNodeType Type;
        /// <summary>
        /// �����ڵ���Ŀ
        /// </summary>
        public ProductionNodeCategory Category;
        /// <summary>
        /// �����ڵ��ִ���䷽��Ŀ
        /// </summary>
        public List<ItemCategory> RecipeCategoryFiltered = new List<ItemCategory>();
        /// <summary>
        /// ��������
        /// </summary>
        public WorkType ExpType;
        
        /// <summary>
        /// �����������䷽
        /// </summary>
        public Recipe Recipe;
        /// <summary>
        /// �˹������ڵ㳣פ����
        /// </summary>
        public Worker Worker;
        /// <summary>
        /// �����ڵ�״̬
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
        /// �������ȼ�
        /// </summary>
        public PriorityTransport PriorityTransport;
        /// <summary>
        /// �Ѿ�����İ�������
        /// </summary>
        public List<MissionTransport> MissionTransports = new List<MissionTransport>();

        /// <summary>
        /// ��������Ч�� ��λ%
        /// </summary>
        public int EffBase { get; private set; } = 100;
        /// <summary>
        /// ����Ч�� ��λ%
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
        /// ����һ������Ҫ��ʱ��
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
        /// �ȼ�
        /// </summary>
        public int Level;
        /// <summary>
        /// ���ȼ�
        /// </summary>
        public int LevelMax = 3;
        /// <summary>
        /// ��������Ĳ���
        /// </summary>
        public List<Dictionary<string, int>> LevelUpgradeRequire = new List<Dictionary<string, int>>();
        /// <summary>
        /// ������ߵĻ�������Ч������Ĭ��ֵ�������û�в��죬������
        /// </summary>
        public List<int> LevelUpgradeEff = new List<int>() { 50, 50, 50 };

        /// <summary>
        /// ��ǰ�ѻ���ֵ
        /// </summary>
        public int StackNumCur { get { return StackNumReserve + StackNum; } }
        /// <summary>
        /// �ѷ��ݴ�����
        /// </summary>
        public int StackNumMax;
        /// <summary>
        /// ��ǰû�з��������Ķѻ�ֵ
        /// ���ֻ����ȡ�����һ��������
        /// </summary>
        public int StackNum { get; protected set; }
        /// <summary>
        /// ��ǰ�ѷ����������δ�����˵Ķѻ�ֵ
        /// Workerֻ����ȡ����ڶ���������
        /// </summary>
        public int StackNumReserve { get; protected set; }
        /// <summary>
        /// �ѻ�������ֵ��δ�������������ﵽ��ֵ��ȫ�����ָ�����Ȼ����������
        /// </summary>
        public int StackCarryThreshold;
        /// <summary>
        ///  ��ԭ���Ͽ�������Item�������ڴ�ֵʱ��������������
        ///  ���� MaxStackNum - ��ֵ������ԭ���ϵ��������ڵ�
        /// </summary>
        public int NeedQuantityThreshold;
        /// <summary>
        ///  ԭ����ID, ���ж��ٷ�
        /// </summary>
        protected Dictionary<string, int> RawItems = new Dictionary<string, int>();

        /// <summary>
        /// ������ʱ����ʱ��Ϊ�䷽����һ�������ʱ��
        /// </summary>
        protected CounterDownTimer timerForProduce;
        /// <summary>
        /// ������ʱ����ʱ��Ϊ�䷽����һ�������ʱ��
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
        /// �����ʱ��
        /// �ڼ�ʱ���ڲ���ʼ���кͽ���ʱ�����ж�
        /// ÿ���һ�θ���һ������ UpdateMission
        /// </summary>
        protected CounterDownTimer timerForMission;
        /// <summary>
        /// �����ʱ��
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
        /// ��ȡ�����ڵ�����������䷽
        /// </summary>
        /// <returns>�䷽ID�б�</returns>
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
        /// ����������
        /// </summary>
        /// <param name="recipeID">�䷽ID</param>
        /// <returns>�����Ƿ�ɹ�</returns>
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
        /// �Ƴ���ǰ������
        /// </summary>
        public void RemoveRecipe()
        {
            this.StopRunning();
            // TODO: ���ѷŵĳ�Ʒ���زģ��Լ����������زģ�ȫ����������ұ���
            if (this.Recipe != null)
            {
                // this.Recipe.ProductItems; ��Ʒ��ƷID
                // this.StackNumCur; ��Ʒ��Ʒ����
            }
            foreach (var items in this.RawItems)
            {
                // items.Key; �ز���ƷID  items.Value; �ز���Ʒ����
            }
            // �������
            this.Recipe = null;
            this.StackNum = 0;
            this.StackNumReserve = 0;
            this.RawItems.Clear();
        }
        /// <summary>
        /// �����ڸڵ���
        /// </summary>
        /// <param name="worker">�µ���</param>
        /// <returns>�����Ƿ�ɹ�</returns>
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
        /// �Ƴ��ڸڵ���
        /// </summary>
        /// <returns>�Ƿ�ɹ�</returns>
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
        /// ��ʼ���������ڵ㣬���ʱ�򲢲�һ����ʼ������Ʒ
        /// </summary>
        public void StartRunning()
        {
            this.TimerForMission.Start();
            this.StartWorking();
        }
        /// <summary>
        /// ȡ�����У����ʱ���ֹͣ������Ʒ
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
        /// ��ʼ������Ʒ
        /// TODO:����״̬�ı�ʱ���ø÷���
        /// </summary>
        public bool StartWorking()
        {
            if (this.CanWorking())
            {
                foreach (var kv in this.Recipe.RawItems)
                {
                    this.RawItems[kv.Key] -= kv.Value;
                }
                // ����������ʱ��
                this.TimerForProduce.Reset(this.TimeCost);
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
        public void EndWorking()
        {
            if (this.timerForProduce != null)
            {
                // ����һ��ֹͣ������������
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
        /// �Ƿ���Կ�ʼ������Ʒ
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
        /// ����ԭ��
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="num">���ӵ�����</param>
        /// <param name="isWorker">�Ƿ��ǵ������</param>
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
        /// �����ڵ�����
        /// </summary>
        public bool Upgrade()
        {
            if (this.Level < this.LevelMax && this.Level >= 0)
            {
                // TODO:�ӱ�����ȡ��������
                this.EffBase += LevelUpgradeEff[Level];
                this.Level += 1;
                return true;
            }
            return false;
        }
        /// <summary>
        /// �����ڵ㽵��
        /// </summary>
        /// <returns></returns>
        public bool Downgrade()
        {
            if (this.Level <= this.LevelMax && this.Level > 0)
            {
                // TODO:����������ӵ�����
                this.Level -= 1;
                this.EffBase -= LevelUpgradeEff[Level];
                return true;
            }
            return false;
        }

        /// <summary>
        /// ��������Ĺ���
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
                    // �Ӳֿ���˲�����
                    List<MissionTransport> missions = GameManager.Instance.GetLocalManager<MissionBroadCastManager>()?.
                        CreateRetrievalMission(kv.Key, missionNum, worldNode.gameObject.transform, this.UID);
                    this.MissionTransports.AddRange(missions);
                }
            }
            missionNum = this.StackNumReserve - GetAssignNum(this.Recipe.ProductItem);
            if (missionNum > 0)
            {
                // ���˲�����Ʒ���ֿ�
                MissionTransport mission = GameManager.Instance.GetLocalManager<MissionBroadCastManager>()?.
                    CreateStoreageMission(this.Recipe.ProductItem, missionNum, worldNode.gameObject.transform, this.UID);
                this.MissionTransports.Add(mission);
            }
        }
        protected void EndActionForProduce()
        {
            // ����
            StackNum += 1;
            if (StackNum >= StackCarryThreshold)
            {
                StackNumReserve += StackNum;
                StackNum = 0;
            }
            this.Worker.AlterExp(this.ExpType, this.Recipe.ExpRecipe * this.Worker.ExpRate[this.ExpType]);
            this.Worker.AlterAP(-1 * this.Worker.APCost);
            // ��һ������
            if (!this.StartWorking())
            {
                this.EndWorking();
            }
        }
        /// <summary>
        /// ��ȡ�Ѿ������������Ʒ����
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="isIn">true��ʾ���룬false��ʾ���</param>
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