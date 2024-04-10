using ML.Engine.Timer;
using ML.Engine.InventorySystem;
using ProjectOC.MissionNS;
using ProjectOC.WorkerNS;
using System.Collections.Generic;
using System;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.BuildingSystem;
using ProjectOC.ManagerNS;

namespace ProjectOC.ProNodeNS
{
    /// <summary>
    /// �����ڵ�
    /// </summary>
    [System.Serializable]
    public class ProNode: IMissionObj, IInventory
    {
        /// <summary>
        /// ��Ӧ��ȫ�������ڵ�
        /// </summary>
        public WorldProNode WorldProNode;

        /// <summary>
        /// ����ʵ��ID��ȫ��Ψһ
        /// </summary>
        public string UID { get { return WorldProNode?.InstanceID ?? ""; } }

        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";

        #region ��������
        /// <summary>
        /// ����
        /// </summary>
        public string Name { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetName(ID); }
        /// <summary>
        /// �����ڵ�����
        /// </summary>
        public ProNodeType ProNodeType { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetProNodeType(ID); }
        /// <summary>
        /// �����ڵ���Ŀ
        /// </summary>
        public RecipeCategory Category { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetCategory(ID); }
        /// <summary>
        /// �����ڵ��ִ���䷽��Ŀ
        /// </summary>
        public List<RecipeCategory> RecipeCategoryFilter { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRecipeCategoryFilterd(ID); }
        /// <summary>
        /// ��������
        /// </summary>
        public WorkType ExpType { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetExpType(ID); }
        /// <summary>
        /// �ѷ��ݴ����޷���
        /// </summary>
        public int StackMaxNum { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetStack(ID); }
        /// <summary>
        /// �ѻ�������ֵ��δ���������ķ����ﵽ��ֵ��ȫ�����ָ�����Ȼ����������
        /// </summary>
        public int StackThresholdNum { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetStackThreshold(ID); }
        /// <summary>
        ///  ��ԭ���Ͽ�������Item�������ڴ�ֵʱ��������������
        ///  ���� MaxStackNum - ��ֵ������ԭ���ϵ��������ڵ�
        /// </summary>
        public int RawThresholdNum { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRawThreshold(ID); }
        #endregion

        #region ���������������
        /// <summary>
        /// ��������Ч�ʣ���λ%
        /// </summary>
        public int EffBase { get; private set; } = 100;
        /// <summary>
        /// ���ȼ�
        /// </summary>
        public int LevelMax { get; private set; } = 2;
        /// <summary>
        /// ������ߵĻ�������Ч��
        /// </summary>
        public List<int> LevelUpgradeEff = new List<int>() { 50, 50, 50 };
        #endregion

        #region Property
        /// <summary>
        /// �ܶѻ�����
        /// </summary>
        public int StackAll { get { return StackReserve + Stack; } }
        /// <summary>
        /// �ܶѻ�����
        /// </summary>
        public int StackAllNum { get => (ProductNum != 0) ? (StackAll / ProductNum) : StackAll; }
        /// <summary>
        /// û�з�������Ķѻ�����
        /// </summary>
        public int StackNum { get => (ProductNum != 0) ? (Stack / ProductNum): Stack; }
        /// <summary>
        /// ������ID
        /// </summary>
        public string ProductItem { get => Recipe?.ProductID ?? ""; }
        /// <summary>
        /// һ������������������
        /// </summary>
        public int ProductNum { get => Recipe?.ProductNum ?? 0; }
        /// <summary>
        /// �����ڵ�״̬
        /// </summary>
        public ProNodeState State
        {
            get
            {
                if (HasRecipe)
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
                else
                {
                    return ProNodeState.Vacancy;
                }
            }
        }
        /// <summary>
        /// ����Ч�� ��λ%
        /// </summary>
        public int Eff
        {
            get
            {
                if (HasWorker)
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
                if (this.HasRecipe && this.Eff > 0)
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
        /// �ȼ�
        /// </summary>
        public int Level = 0;
        /// <summary>
        /// �������ȼ�
        /// </summary>
        public TransportPriority TransportPriority = TransportPriority.Normal;
        /// <summary>
        /// �����������䷽
        /// </summary>
        public Recipe Recipe;
        public bool HasRecipe { get => Recipe != null && !string.IsNullOrEmpty(Recipe.ID); }
        /// <summary>
        /// �˹������ڵ㳣פ����
        /// </summary>
        public Worker Worker;
        public bool HasWorker { get => this.ProNodeType == ProNodeType.Mannul && Worker != null && !string.IsNullOrEmpty(Worker.Name); }
        /// <summary>
        /// �Ѿ�����İ�������
        /// </summary>
        public List<MissionTransport> MissionTransports = new List<MissionTransport>();
        /// <summary>
        /// û�з�������Ķѻ�ֵ
        /// ���������ȡ�����һ��������
        /// </summary>
        public int Stack { get; protected set; }
        /// <summary>
        /// �ѷ����������δ�����˵Ķѻ�ֵ
        /// Workerֻ����ȡ����ڶ���������
        /// </summary>
        public int StackReserve { get; protected set; }
        public bool RequirePower = false;
        /// <summary>
        ///  ԭ����ID, ���ж��ٸ�
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
        #region ��ʱ��
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
                    timerForProduce.OnUpdateEvent += onProduceTimerUpdate;
                }
                return timerForProduce;
            }
        }

        /// <summary>
        /// �����ʱ��
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
        #endregion

        public ProNode(ProNodeTableData config)
        {
            this.ID = config.ID ?? "";
        }
        /// <summary>
        /// ����ʱ����
        /// </summary>
        public void Destroy(Player.PlayerCharacter player = null)
        {
            this.StopRun();
            this.RemoveRecipe(player);
            this.RemoveWorker();
        }

        public void OnPositionChange()
        {
            if (HasWorker)
            {
                if (this.Worker.ArriveProNode)
                {
                    Worker.transform.position = WorldProNode.transform.position + new Vector3(0, 2f, 0);
                }
                else
                {
                    Worker.SetDestination(WorldProNode.transform.position, ProNode_Action);
                }
            }
            foreach (var mission in MissionTransports)
            {
                mission?.UpdateTransportDestionation();
            }
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
        /// ��ȡ�����ڵ�����������䷽
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
        /// ����������
        /// </summary>
        /// <param name="recipeID">�䷽ID</param>
        /// <returns>�����Ƿ�ɹ�</returns>
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
        /// �Ƴ���ǰ������
        /// </summary>
        public void RemoveRecipe(Player.PlayerCharacter player)
        {
            this.StopRun();
            // ���ѷŵĳ�Ʒ���زģ�ȫ����������ұ���
            bool flag = false;
            List<Item> resItems = new List<Item>();
            // �ѷŵĳ�Ʒ
            if (HasRecipe && StackAll > 0)
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
                        if (player == null || !player.Inventory.AddItem(item))
                        {
                            flag = true;
                        }
                    }
                }
            }
            // �ز�
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
                        if (player == null || !player.Inventory.AddItem(item))
                        {
                            flag = true;
                        }
                    }
                }
            }
            // û�мӵ���ұ����Ķ����WorldItem
            foreach (Item item in resItems)
            {
#pragma warning disable CS4014
                ItemManager.Instance.SpawnWorldItem(item, WorldProNode.transform.position, WorldProNode.transform.rotation);
#pragma warning restore CS4014
            }
            // �������
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
        /// �����ڸڵ���
        /// </summary>
        /// <param name="worker">�µ���</param>
        /// <returns>�����Ƿ�ɹ�</returns>
        public bool ChangeWorker(Worker worker)
        {
            if (this.ProNodeType == ProNodeType.Mannul && worker != null)
            {
                this.RemoveWorker();
                worker.ChangeProNode(this);
                worker.SetTimeStatusAll(TimeStatus.Work_OnDuty);
                worker.SetDestination(WorldProNode.transform.position, ProNode_Action);
                this.Worker = worker;
                worker.StatusChangeAction += OnWorkerStatusChangeAction;
                worker.APChangeAction += OnWorkerAPChangeAction;
                return true;
            }
            return false;
        }

        public void ProNode_Action(Worker worker)
        {
            worker.ArriveProNode = true;
            worker.Agent.enabled = false;
            worker.LastPosition = worker.transform.position;
            worker.transform.position = WorldProNode.transform.position + new Vector3(0, 2f, 0);
            worker.ProNode.StartProduce();
        }

        /// <summary>
        /// �Ƴ��ڸڵ���
        /// </summary>
        /// <returns>�Ƿ�ɹ�</returns>
        public bool RemoveWorker()
        {
            if (this.ProNodeType == ProNodeType.Mannul)
            {
                this.StopProduce();
                if (HasWorker)
                {
                    this.Worker.StatusChangeAction -= OnWorkerStatusChangeAction;
                    this.Worker.APChangeAction -= OnWorkerAPChangeAction;
                    this.Worker.ClearDestination();
                    this.Worker.ProNode = null;
                    if (!this.Worker.Agent.enabled)
                    {
                        this.Worker.transform.position = this.Worker.LastPosition;
                        this.Worker.Agent.enabled = true;
                    }
                    this.Worker = null;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// ��ʼ���������ڵ㣬���ʱ�򲢲�һ����ʼ������Ʒ
        /// </summary>
        public void StartRun()
        {
            this.TimerForMission.Start();
            this.StartProduce();
        }

        /// <summary>
        /// ȡ�����У����ʱ���ֹͣ������Ʒ
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
        /// ��ʼ������Ʒ
        /// </summary>
        public bool StartProduce()
        {
            if (this.CanWorking() && (timerForProduce == null || TimerForProduce.IsStoped))
            {
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
        public void StopProduce()
        {
            if (this.timerForProduce != null)
            {
                this.TimerForProduce?.End();
            }
        }

        /// <summary>
        /// �Ƿ���Կ�ʼ������Ʒ
        /// </summary>
        public bool CanWorking()
        {
            if (HasRecipe)
            {
                foreach (var kv in this.Recipe.Raw)
                {
                    if (this.RawItems[kv.id] < kv.num)
                    {
                        return false;
                    }
                }
                if (this.ProNodeType == ProNodeType.Mannul && !(HasWorker && Worker.IsOnDuty))
                {
                    return false;
                }
                if (this.StackAllNum >= this.StackMaxNum)
                {
                    return false;
                }
                if (RequirePower && WorldProNode.PowerCount <= 0)
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
        /// ��������Ĺ���
        /// </summary>
        protected void EndActionForMission()
        {
            int missionNum;
            foreach (var kv in Recipe.Raw)
            {
                missionNum = kv.num * RawThresholdNum - RawItems[kv.id] - GetAssignNum(kv.id, true);
                if (missionNum > 0)
                {
                    // �Ӳֿ���˲�����
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionTransportType.Store_ProNode, kv.id, missionNum, this);
                }
            }
            missionNum = StackReserve - GetAssignNum(ProductItem, false);
            if (missionNum > 0)
            {
                // ���˲�����Ʒ���ֿ�
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionTransportType.ProNode_Store, ProductItem, missionNum, this);
            }
        }

        protected void EndActionForProduce()
        {
            // ����
            Item item = Recipe.Composite(this);
            AddItem(item);
            if (this.ProNodeType == ProNodeType.Mannul)
            {
                Worker.AlterExp(ExpType, Recipe.ExpRecipe);
                Worker.AlterAP(-1 * Worker.APCost);
            }
            // ��һ������
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
            }
            else
            {
                StopProduce();
            }
            OnActionChange?.Invoke();
        }
        private void OnWorkerAPChangeAction(int ap)
        {
            OnActionChange?.Invoke();
        }

        /// <summary>
        /// ��ȡ�Ѿ������������Ʒ����
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="isIn">true��ʾ���룬false��ʾȡ��</param>
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

        #region UI�ӿ�
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
                        break;
                    }
                }
                StartProduce();
            }
        }
        public void UIFastAdd(Player.PlayerCharacter player)
        {
            if (player != null)
            {
                Dictionary<string, int> tempRawItems = new Dictionary<string, int>(RawItems);
                foreach (var kv in tempRawItems)
                {
                    string itemID = kv.Key;
                    int amount = player.Inventory.GetItemAllNum(itemID);
                    int maxAmount = StackMaxNum * Recipe.GetRawNum(itemID) - kv.Value;
                    amount = amount >= maxAmount ? maxAmount : amount;
                    if (player.Inventory.RemoveItem(itemID, amount))
                    {
                        RawItems[itemID] += amount;
                        StartProduce();
                    }
                }
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
                        break;
                    }
                }
                StartProduce();
            }
        }
        public List<Formula> GetUpgradeRaw()
        {
            List<Formula> result = new List<Formula>();
            if (this.WorldProNode != null)
            {
                string upgradeCID = BuildingManager.Instance.GetUpgradeCID(this.WorldProNode.Classification.ToString());
                List<Formula> formulas = BuildingManager.Instance.GetRaw(upgradeCID);
                if (formulas != null)
                {
                    result.AddRange(formulas);
                }
            }
            return result;
        }

        public List<Formula> GetUpgradeRawCurrent(Player.PlayerCharacter player)
        {
            List<Formula> result = new List<Formula>();
            if (this.WorldProNode != null)
            {
                List<Formula> formulas = GetUpgradeRaw();
                foreach (Formula formula in formulas)
                {
                    int num = player.Inventory.GetItemAllNum(formula.id);
                    Formula newFormula = new Formula();
                    newFormula.id = formula.id;
                    newFormula.num = num;
                    result.Add(newFormula);
                }
            }
            return result;
        }

        public void Upgrade(Player.PlayerCharacter player)
        {
            if (this.WorldProNode != null)
            {
                string ID = BuildingManager.Instance.GetID(this.WorldProNode.Classification.ToString());
                string upgradeID = BuildingManager.Instance.GetUpgradeID(this.WorldProNode.Classification.ToString());
                string upgradeCID = BuildingManager.Instance.GetClassification(upgradeID);
                Transform parent = this.WorldProNode.transform.parent;

                if (!string.IsNullOrEmpty(upgradeID)
                    && !string.IsNullOrEmpty(upgradeCID)
                    && BuildingManager.Instance.IsValidBPartID(upgradeCID)
                    && CompositeManager.Instance.OnlyCostResource(player.Inventory, upgradeID))
                {
                    // ���ز���
                    CompositeManager.Instance.OnlyReturnResource(player.Inventory, ID);
                    if (BuildingManager.Instance.GetOneBPartInstance(upgradeCID) is WorldProNode upgrade)
                    {
                        upgrade.transform.SetParent(parent);
                        upgrade.InstanceID = this.WorldProNode.InstanceID;
                        upgrade.transform.position = this.WorldProNode.transform.position;
                        upgrade.transform.rotation = this.WorldProNode.transform.rotation;
                        ML.Engine.Manager.GameManager.DestroyObj(this.WorldProNode.gameObject);
                        LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(upgrade, this);
                        this.SetLevel(upgrade.Classification.Category4 - 1);
                    }
                }
            }
        }
        #endregion

        #region IMission�ӿ�
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
        /// ����ȡ��������
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
                StartProduce();
                OnActionChange?.Invoke();
                return amount;
            }
            return 0;
        }
        #endregion

        #region IInventory�ӿ�
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
                    this.StartProduce();
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
                    this.StartProduce();
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
                        this.StartProduce();
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