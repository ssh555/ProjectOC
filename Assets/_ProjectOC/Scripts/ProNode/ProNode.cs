using ProjectOC.WorkerNS;
using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [LabelText("�����ڵ�"), Serializable]
    public class ProNode: MissionNS.IMissionObj, IInventory, IWorkerContainer
    {
        #region WorldProNode
        [LabelText("��Ӧ��ȫ�������ڵ�"), ReadOnly]
        public WorldProNode WorldProNode;
        [LabelText("����ʵ��ID"), ShowInInspector, ReadOnly]
        public string UID { get { return WorldProNode?.InstanceID ?? ""; } }
        #endregion

        #region Data
        [LabelText("ID"), ReadOnly]
        public string ID = "";
        [LabelText("�ȼ�"), ReadOnly]
        public int Level = 0;
        [LabelText("�������ȼ�"), ReadOnly]
        public MissionNS.TransportPriority TransportPriority = MissionNS.TransportPriority.Normal;
        [LabelText("�����������䷽"), ReadOnly]
        public Recipe Recipe;
        [LabelText("�Ѿ�����İ�������"), ReadOnly]
        public List<MissionNS.MissionTransport> MissionTransports = new List<MissionNS.MissionTransport>();
        [LabelText("û�з�������Ķѻ�ֵ"), ShowInInspector, ReadOnly]
        public int Stack { get; protected set; }
        [LabelText("�ѷ����������δ�����˵Ķѻ�ֵ"), ShowInInspector, ReadOnly]
        public int StackReserve { get; protected set; }
        [LabelText("ԭ����"), ShowInInspector, ReadOnly]
        protected Dictionary<string, int> RawItems = new Dictionary<string, int>();
        #endregion

        #region Timer
        /// <summary>
        /// ������ʱ����ʱ��Ϊ�䷽����һ�������ʱ��
        /// </summary>
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

        /// <summary>
        /// �����ʱ��
        /// </summary>
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
        public bool HasRecipe => !string.IsNullOrEmpty(Recipe.ID);
        [LabelText("�ܶѻ�����"), ShowInInspector, ReadOnly]
        public int StackAll => StackReserve + Stack;
        [LabelText("�ܶѻ�����"), ShowInInspector, ReadOnly]
        public int StackAllNum => (ProductNum != 0) ? (StackAll / ProductNum) : StackAll;
        [LabelText("û�з�������Ķѻ�����"), ShowInInspector, ReadOnly]
        public int StackNum => (ProductNum != 0) ? (Stack / ProductNum) : Stack;
        [LabelText("������ID"), ShowInInspector, ReadOnly]
        public string ProductItem => Recipe.ProductID;
        [LabelText("һ������������������"), ShowInInspector, ReadOnly]
        public int ProductNum => Recipe.ProductNum;
        [LabelText("�Ƿ���������"), ShowInInspector, ReadOnly]
        public bool IsOnRunning => timerForMission != null && !timerForMission.IsStoped;
        [LabelText("�Ƿ�����������Ʒ"), ShowInInspector, ReadOnly]
        public bool IsOnProduce => timerForProduce != null && !timerForProduce.IsStoped;
        [LabelText("�����ڵ�״̬"), ShowInInspector, ReadOnly]
        public ProNodeState State => HasRecipe ? (IsOnProduce ? ProNodeState.Production : ProNodeState.Stagnation) : ProNodeState.Vacancy;
        [LabelText("����Ч��"), PropertyTooltip("��λ%"), ShowInInspector, ReadOnly]
        public int Eff => HaveWorker ? EffBase + Worker.Eff[ExpType] : EffBase;
        [LabelText("����һ������Ҫ��ʱ��"), ShowInInspector, ReadOnly]
        public int TimeCost => HasRecipe && Eff > 0 ? (int)Math.Ceiling((double)100 * Recipe.TimeCost / Eff) : 0;
        #endregion

        #region Config
        [LabelText("��������Ч��"), PropertyTooltip("��λ %"), FoldoutGroup("����"), ShowInInspector]
        public int EffBase { get; private set; } = 100;
        [LabelText("���ȼ�"), FoldoutGroup("����"), ShowInInspector]
        public int LevelMax { get; private set; } = 2;
        [LabelText("������ߵĻ�������Ч��"), FoldoutGroup("����"), ShowInInspector]
        public List<int> LevelUpgradeEff = new List<int>() { 50, 50, 50 };
        #endregion

        #region Table Property
        [LabelText("����"), ShowInInspector, ReadOnly]
        public string Name => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ProNodeManager.GetName(ID) : "";
        [LabelText("�����ڵ�����"), ShowInInspector, ReadOnly]
        public ProNodeType ProNodeType => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ProNodeManager.GetProNodeType(ID) : ProNodeType.None;
        [LabelText("�����ڵ���Ŀ"), ShowInInspector, ReadOnly]
        public RecipeCategory Category => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ProNodeManager.GetCategory(ID) : RecipeCategory.None;
        [LabelText("�����ڵ��ִ���䷽��Ŀ"), ShowInInspector, ReadOnly]
        public List<RecipeCategory> RecipeCategoryFilter => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRecipeCategoryFilterd(ID) : new List<RecipeCategory>();
        [LabelText("��������"), ShowInInspector, ReadOnly]
        public WorkType ExpType => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ProNodeManager.GetExpType(ID) : WorkType.None;
        [LabelText("�ѷ����޷���"), ShowInInspector, ReadOnly]
        public int StackMaxNum => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ProNodeManager.GetMaxStack(ID) : 0;
        [LabelText("�ѷ���ֵ����"), ShowInInspector, ReadOnly]
        public int StackThresholdNum => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ProNodeManager.GetStackThreshold(ID) : 0;
        [LabelText("������ֵ����"), ShowInInspector, ReadOnly]
        public int RawThresholdNum => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRawThreshold(ID) : 0;
        [LabelText("�Ƿ���Ҫ����"), ShowInInspector, FoldoutGroup("����")]
        public bool RequirePower => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ProNodeManager.GetCanCharge(ID) : false;
        #endregion

        #region Mono
        public void Destroy()
        {
            StopRun();
            RemoveRecipe();
            (this as IWorkerContainer).RemoveWorker();
        }
        #endregion

        #region BuildPart
        public void OnPositionChange(Vector3 differ)
        {
            (this as IWorkerContainer).OnPositionChange(differ);
            foreach (var mission in MissionTransports)
            {
                mission?.UpdateTransportDestionation();
            }
        }
        #endregion

        #region Upgrade
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
                Level = level;
                return true;
            }
            return false;
        }

        #endregion

        #region Method
        public ProNode(ProNodeTableData config)
        {
            ID = config.ID ?? "";
        }
        /// <summary>
        /// ��ȡ�����ڵ�����������䷽
        /// </summary>
        public List<string> GetCanProduceRecipe()
        {
            List<string> result = new List<string>();
            foreach (RecipeCategory recipeCategory in RecipeCategoryFilter)
            {
                result.AddRange(ManagerNS.LocalGameManager.Instance.RecipeManager.GetRecipeIDsByCategory(recipeCategory));
            }
            return ManagerNS.LocalGameManager.Instance.RecipeManager.SortRecipeIDs(result);
        }
        /// <summary>
        /// ����������
        /// </summary>
        public bool ChangeRecipe(string recipeID)
        {
            lock (this)
            {
                RemoveRecipe();
                if (!string.IsNullOrEmpty(recipeID))
                {
                    Recipe recipe = ManagerNS.LocalGameManager.Instance.RecipeManager.SpawnRecipe(recipeID);
                    if (recipe.IsValidRecipe)
                    {
                        Recipe = recipe;
                        foreach (Formula raw in Recipe.Raw)
                        {
                            RawItems.Add(raw.id, 0);
                        }
                        StartRun();
                        return true;
                    }
                }
                else
                {
                    Recipe.ClearData();
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// �Ƴ���ǰ������
        /// </summary>
        public void RemoveRecipe()
        {
            StopRun();
            // ���ѷŵĳ�Ʒ���زģ�ȫ����������ұ���
            List<Item> items = new List<Item>();
            if (HasRecipe && StackAll > 0)
            {
                items.AddRange(ItemManager.Instance.SpawnItems(ProductItem, StackAll));
            }
            foreach (var raw in RawItems)
            {
                if (raw.Value > 0)
                {
                    items.AddRange(ItemManager.Instance.SpawnItems(raw.Key, raw.Value));
                }
            }
            (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(items);

            foreach (MissionNS.MissionTransport mission in MissionTransports.ToArray())
            {
                mission.End();
            }
            MissionTransports.Clear();
            Stack = 0;
            StackReserve = 0;
            RawItems.Clear();
            Recipe.ClearData();
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
                    if (RawItems[kv.id] < kv.num)
                    {
                        return false;
                    }
                }
                if (ProNodeType == ProNodeType.Mannul && !(HaveWorker && Worker.IsOnProNodeDuty))
                {
                    return false;
                }
                if (StackAllNum >= StackMaxNum)
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
        #endregion

        #region Event
        public event Action OnDataChangeEvent;
        public event Action<double> OnProduceUpdateEvent;
        public event Action OnProduceEndEvent;
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
                    missionNum += kv.num * (StackMaxNum - RawThresholdNum);
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionNS.MissionTransportType.Store_ProNode, kv.id, missionNum, this);
                }
            }
            missionNum = StackReserve - GetAssignNum(ProductItem, false);
            if (missionNum > 0)
            {
                var missionType = ItemManager.Instance.GetItemType(ProductItem) == ItemType.Feed ? MissionNS.MissionTransportType.ProNode_Restaurant : MissionNS.MissionTransportType.ProNode_Store;
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(missionType, ProductItem, missionNum, this);
            }
        }

        protected void UpdateActionForProduce(double time)
        {
            OnProduceUpdateEvent?.Invoke(time);
        }

        protected void EndActionForProduce()
        {
            // ����
            Item item = Recipe.Composite(this);
            AddItem(item);
            if (ProNodeType == ProNodeType.Mannul)
            {
                Worker.AlterExp(ExpType, Recipe.ExpRecipe);
                Worker.AlterAP(-1 * Worker.APCost);
                Worker.AlterMood(-1 * Worker.MoodCost);
            }
            // ��һ������
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

        protected void OnWorkerAPChangeEvent(int ap)
        {
            OnDataChangeEvent?.Invoke();
        }
        #endregion

        #region ���ݷ���
        /// <summary>
        /// ��ȡ�Ѿ������������Ʒ����
        /// </summary>
        /// <param name="isIn">true��ʾ���룬false��ʾȡ��</param>
        protected int GetAssignNum(string itemID, bool isIn = true)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (MissionNS.MissionTransport mission in MissionTransports)
                {
                    if (mission != null && mission.ItemID == itemID)
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
        protected int Add(string itemID, int amount, bool exceed = false, bool complete = true)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(itemID) && amount > 0)
                {
                    if (RawItems.ContainsKey(itemID))
                    {
                        int exceedNum = RawItems[itemID] + amount - StackMaxNum * Recipe.GetRawNum(itemID);
                        if (!exceed && exceedNum > 0 && (complete || exceedNum >= amount))
                        {
                            return 0;
                        }
                        amount = !exceed && !complete && exceedNum > 0 ? amount - exceedNum : amount;
                        RawItems[itemID] += amount;
                        StartProduce();
                        OnDataChangeEvent?.Invoke();
                        return amount;
                    }
                    else if (ProductItem == itemID)
                    {
                        if (!exceed && StackAllNum >= StackMaxNum)
                        {
                            return 0;
                        }
                        Stack += amount;
                        if (StackNum >= StackThresholdNum)
                        {
                            StackReserve += Stack;
                            Stack = 0;
                        }
                        OnDataChangeEvent?.Invoke();
                        return amount;
                    }
                }
                return 0;
            }
        }
        protected int Remove(string itemID, int amount, bool complete = true, bool isReserve = false)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(itemID) && amount > 0)
                {
                    if (RawItems.ContainsKey(itemID))
                    {
                        if (complete && RawItems[itemID] < amount)
                        {
                            return 0;
                        }
                        amount = !complete && RawItems[itemID] < amount ? RawItems[itemID] : amount;
                        RawItems[itemID] -= amount;
                        return amount;
                    }
                    else if (ProductItem == itemID)
                    {
                        if (!isReserve)
                        {
                            if (complete && Stack < amount)
                            {
                                return 0;
                            }
                            amount = !complete && Stack < amount ? Stack : amount;
                            Stack -= amount;
                        }
                        else
                        {
                            if (complete && StackReserve < amount)
                            {
                                return 0;
                            }
                            amount = !complete && StackReserve < amount ? StackReserve : amount;
                            StackReserve -= amount;
                        }
                        OnDataChangeEvent?.Invoke();
                        StartProduce();
                        return amount;
                    }
                }
                return 0;
            }
        }
        #endregion

        #region UI�ӿ�
        public void UIRemove(int amount)
        {
            lock (this)
            {
                if (amount > 0 && Stack >= amount)
                {
                    List<Item> items = ItemManager.Instance.SpawnItems(ProductItem, amount);
                    var inventory = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory;
                    foreach (Item item in items)
                    {
                        int itemAmount = item.Amount;
                        if (inventory.AddItem(item))
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
        }
        public void UIFastAdd()
        {
            lock (this)
            {
                Dictionary<string, int> tempRawItems = new Dictionary<string, int>(RawItems);
                var inventory = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory;
                bool flag = false;
                foreach (var kv in tempRawItems)
                {
                    string itemID = kv.Key;
                    int amount = inventory.GetItemAllNum(itemID);
                    int maxAmount = StackMaxNum * Recipe.GetRawNum(itemID) - kv.Value;
                    amount = amount <= maxAmount ? amount : maxAmount;
                    if (inventory.RemoveItem(itemID, amount))
                    {
                        RawItems[itemID] += amount;
                        flag = true;
                    }
                }
                if (flag)
                {
                    StartProduce();
                }
            }
        }
        #endregion

        #region IWorkerContainer
        public Action<Worker> OnSetWorkerEvent { get; set; }
        public Action OnRemoveWorkerEvent { get; set; }
        public Worker Worker { get; set; }
        public bool IsArrive { get; set; }
        public bool HaveWorker => ProNodeType == ProNodeType.Mannul && Worker != null && !string.IsNullOrEmpty(Worker.InstanceID);

        public WorkerContainerType GetContainerType() { return WorkerContainerType.Work; }

        public void OnArriveEvent(Worker worker)
        {
            (this as IWorkerContainer).OnArriveSetPosition(worker, WorldProNode.transform.position + new Vector3(0, 2f, 0));
            worker.ProNode.StartProduce();
        }

        public void SetWorkerRelateData()
        {
            if (ProNodeType == ProNodeType.Mannul && Worker != null)
            {
                Worker.SetTimeStatusAll(TimeStatus.Work_OnDuty);
                Worker.SetDestination(WorldProNode.transform.position, OnArriveEvent);
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
            if (Worker != null && !IsOnProduce)
            {
                if (IsArrive)
                {
                    Worker.RecoverLastPosition();
                    IsArrive = false;
                }
                else
                {
                    Worker.ClearDestination();
                }
                return true;
            }
            return false;
        }
        #endregion

        #region IMission
        public Transform GetTransform() { return WorldProNode?.transform; }
        public MissionNS.TransportPriority GetTransportPriority() { return TransportPriority; }
        public string GetUID() { return UID; }
        public void AddMissionTranport(MissionNS.MissionTransport mission) { MissionTransports.Add(mission); }
        public void RemoveMissionTranport(MissionNS.MissionTransport mission) { MissionTransports.Remove(mission); }
        public bool PutIn(string itemID, int amount)
        {
            return Add(itemID, amount, true) == amount;
        }
        public int PutOut(string itemID, int amount)
        {
            return Remove(itemID, amount, false, true);
        }
        #endregion

        #region IInventory
        public bool AddItem(Item item)
        {
            if (item != null)
            {
                return Add(item.ID, item.Amount) == item.Amount;
            }
            return false;
        }
        public bool RemoveItem(Item item)
        {
            if (item != null)
            {
                return Remove(item.ID, item.Amount) == item.Amount;
            }
            return false;
        }
        public Item RemoveItem(Item item, int amount)
        {
            if (item != null)
            {
                Item result = ItemManager.Instance.SpawnItem(item.ID);
                result.Amount = Remove(item.ID, amount, false);
                return result;
            }
            return null;
        }
        public bool RemoveItem(string itemID, int amount)
        {
            return Remove(itemID, amount) == amount;
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
                    return Stack;
                }
            }
            return 0;
        }
        public Item[] GetItemList()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}