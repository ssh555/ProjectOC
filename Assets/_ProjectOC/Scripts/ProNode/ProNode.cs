using ML.Engine.Timer;
using ML.Engine.InventorySystem;
using ProjectOC.MissionNS;
using ProjectOC.WorkerNS;
using System.Collections.Generic;
using System;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;
using Sirenix.OdinInspector;
using ML.Engine.Manager;
using ProjectOC.Player;

namespace ProjectOC.ProNodeNS
{
    /// <summary>
    /// �����ڵ�
    /// </summary>
    [System.Serializable]
    public class ProNode: IMissionObj, IInventory
    {
        #region ��ǰ����
        [LabelText("��Ӧ��ȫ�������ڵ�"), ReadOnly]
        public WorldProNode WorldProNode;
        [LabelText("����ʵ��ID"), ShowInInspector, ReadOnly]
        public string UID { get { return WorldProNode?.InstanceID ?? ""; } }
        [LabelText("ID"), ReadOnly]
        public string ID = "";
        [LabelText("�ȼ�"), ReadOnly]
        public int Level = 0;
        [LabelText("�������ȼ�"), ReadOnly]
        public TransportPriority TransportPriority = TransportPriority.Normal;
        [LabelText("�����������䷽"), ReadOnly]
        public Recipe Recipe;
        [LabelText("�˹������ڵ㳣פ����"), ReadOnly]
        public Worker Worker;
        [LabelText("�����Ƿ񵽴������ڵ�"), ReadOnly]
        public bool IsWorkerArrive;
        [LabelText("�Ѿ�����İ�������"), ReadOnly]
        public List<MissionTransport> MissionTransports = new List<MissionTransport>();
        [LabelText("û�з�������Ķѻ�ֵ"), ShowInInspector, ReadOnly]
        public int Stack { get; protected set; }
        [LabelText("�ѷ����������δ�����˵Ķѻ�ֵ"), ShowInInspector, ReadOnly]
        public int StackReserve { get; protected set; }
        [LabelText("ԭ����"), ShowInInspector, ReadOnly]
        protected Dictionary<string, int> RawItems = new Dictionary<string, int>();
        #endregion

        #region ��������
        [LabelText("��������Ч��"), PropertyTooltip("��λ %"), FoldoutGroup("����"), ShowInInspector]
        public int EffBase { get; private set; } = 100;
        [LabelText("���ȼ�"), FoldoutGroup("����"), ShowInInspector]
        public int LevelMax { get; private set; } = 2;
        [LabelText("������ߵĻ�������Ч��"), FoldoutGroup("����"), ShowInInspector]
        public List<int> LevelUpgradeEff = new List<int>() { 50, 50, 50 };
        [LabelText("�Ƿ���Ҫ����"), FoldoutGroup("����")]
        public bool RequirePower = false;
        #endregion

        #region ��������
        [LabelText("����"), ShowInInspector, ReadOnly]
        public string Name { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetName(ID); }
        [LabelText("�����ڵ�����"), ShowInInspector, ReadOnly]
        public ProNodeType ProNodeType { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetProNodeType(ID); }
        [LabelText("�����ڵ���Ŀ"), ShowInInspector, ReadOnly]
        public RecipeCategory Category { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetCategory(ID); }
        [LabelText("�����ڵ��ִ���䷽��Ŀ"), ShowInInspector, ReadOnly]
        public List<RecipeCategory> RecipeCategoryFilter { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRecipeCategoryFilterd(ID); }
        [LabelText("��������"), ShowInInspector, ReadOnly]
        public WorkType ExpType { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetExpType(ID); }
        [LabelText("�ѷ����޷���"), ShowInInspector, ReadOnly]
        public int StackMaxNum { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetStack(ID); }
        [LabelText("�ѷ���ֵ����"), ShowInInspector, ReadOnly]
        public int StackThresholdNum { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetStackThreshold(ID); }
        [LabelText("������ֵ����"), ShowInInspector, ReadOnly]
        public int RawThresholdNum { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRawThreshold(ID); }
        #endregion

        #region Property
        [LabelText("�Ƿ��������䷽"), ShowInInspector, ReadOnly]
        public bool HasRecipe { get => Recipe != null && !string.IsNullOrEmpty(Recipe.ID); }
        [LabelText("�Ƿ��е���"), ShowInInspector, ReadOnly]
        public bool HasWorker { get => ProNodeType == ProNodeType.Mannul && Worker != null && !string.IsNullOrEmpty(Worker.Name); }
        [LabelText("�ܶѻ�����"), ShowInInspector, ReadOnly]
        public int StackAll { get { return StackReserve + Stack; } }
        [LabelText("�ܶѻ�����"), ShowInInspector, ReadOnly]
        public int StackAllNum { get => (ProductNum != 0) ? (StackAll / ProductNum) : StackAll; }
        [LabelText("û�з�������Ķѻ�����"), ShowInInspector, ReadOnly]
        public int StackNum { get => (ProductNum != 0) ? (Stack / ProductNum): Stack; }
        [LabelText("������ID"), ShowInInspector, ReadOnly]
        public string ProductItem { get => Recipe?.ProductID ?? ""; }
        [LabelText("һ������������������"), ShowInInspector, ReadOnly]
        public int ProductNum { get => Recipe?.ProductNum ?? 0; }
        [LabelText("�Ƿ���������"), ShowInInspector, ReadOnly]
        public bool IsOnRunning { get => timerForMission != null && !timerForMission.IsStoped; }
        [LabelText("�Ƿ�����������Ʒ"), ShowInInspector, ReadOnly]
        public bool IsOnProduce { get => timerForProduce != null && !timerForProduce.IsStoped; }
        [LabelText("�����ڵ�״̬"), ShowInInspector, ReadOnly]
        public ProNodeState State { get => HasRecipe ? (IsOnProduce ? ProNodeState.Production : ProNodeState.Stagnation) : ProNodeState.Vacancy; }
        [LabelText("����Ч��"), PropertyTooltip("��λ %"), ShowInInspector, ReadOnly]
        public int Eff { get => HasWorker ? EffBase + Worker.Eff[ExpType] : EffBase; }
        [LabelText("����һ������Ҫ��ʱ��"), ShowInInspector, ReadOnly]
        public int TimeCost { get => HasRecipe && Eff > 0 ? (int)Math.Ceiling((double)100 * Recipe.TimeCost / Eff) : 0; }
        #endregion

        #region Action
        public event Action OnActionChange;
        public event Action<double> OnProduceUpdate;
        public event Action OnProduceEnd;
        #endregion

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
                    timerForProduce = new CounterDownTimer(TimeCost, false, false);
                    timerForProduce.OnEndEvent += EndActionForProduce;
                    timerForProduce.OnUpdateEvent += UpdateActionForProduce;
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

        #region ����
        public ProNode(ProNodeTableData config)
        {
            ID = config.ID ?? "";
        }

        public void Destroy()
        {
            StopRun();
            RemoveRecipe();
            RemoveWorker();
        }

        public void OnPositionChange()
        {
            if (HasWorker)
            {
                if (IsWorkerArrive)
                {
                    Worker.transform.position = WorldProNode.transform.position + new Vector3(0, 2f, 0);
                }
                else
                {
                    Worker.SetDestination(WorldProNode.transform.position, ArriveProNodeAction);
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
                Level = level;
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
                if (!string.IsNullOrEmpty(recipeID))
                {
                    Recipe recipe = ManagerNS.LocalGameManager.Instance.RecipeManager.SpawnRecipe(recipeID);
                    if (recipe != null)
                    {
                        RemoveRecipe();
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
                    RemoveRecipe();
                    Recipe = null;
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
            bool flag = false;
            List<Item> resItems = new List<Item>();
            var inventory = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory;
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
                        if (!inventory.AddItem(item))
                        {
                            flag = true;
                        }
                    }
                }
            }
            // �ز�
            foreach (var raw in RawItems)
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
                        if (!inventory.AddItem(item))
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
            List<MissionTransport> missions = new List<MissionTransport>();
            missions.AddRange(MissionTransports);
            foreach (MissionTransport mission in missions)
            {
                mission.End();
            }
            MissionTransports.Clear();
            Stack = 0;
            StackReserve = 0;
            RawItems.Clear();
            Recipe = null;
        }

        /// <summary>
        /// �����ڸڵ���
        /// </summary>
        /// <param name="worker">�µ���</param>
        /// <returns>�����Ƿ�ɹ�</returns>
        public bool ChangeWorker(Worker worker)
        {
            if (ProNodeType == ProNodeType.Mannul && worker != null)
            {
                RemoveWorker();
                worker.ChangeProNode(this);
                worker.SetTimeStatusAll(TimeStatus.Work_OnDuty);
                worker.SetDestination(WorldProNode.transform.position, ArriveProNodeAction);
                Worker = worker;
                worker.StatusChangeAction += OnWorkerStatusChangeAction;
                worker.APChangeAction += OnWorkerAPChangeAction;
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
            if (ProNodeType == ProNodeType.Mannul)
            {
                StopProduce();
                if (HasWorker)
                {
                    Worker.StatusChangeAction -= OnWorkerStatusChangeAction;
                    Worker.APChangeAction -= OnWorkerAPChangeAction;
                    Worker.ClearDestination();
                    Worker.ProNode = null;
                    Worker.RecoverLastPosition();
                    Worker = null;
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
                // ����������ʱ��
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
                TimerForProduce.End();
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
                if (ProNodeType == ProNodeType.Mannul && !(HasWorker && Worker.IsOnDuty))
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

        #region Action����
        /// <summary>
        /// ���񵽴������ڵ�
        /// </summary>
        public void ArriveProNodeAction(Worker worker)
        {
            worker.Agent.enabled = false;
            worker.LastPosition = worker.transform.position;
            worker.transform.position = WorldProNode.transform.position + new Vector3(0, 2f, 0);
            IsWorkerArrive = true;
            worker.ProNode.StartProduce();
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
                    missionNum += kv.num * (StackMaxNum - RawThresholdNum);
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionTransportType.Store_ProNode, kv.id, missionNum, this);
                }
            }
            missionNum = StackReserve - GetAssignNum(ProductItem, false);
            if (missionNum > 0)
            {
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionTransportType.ProNode_Store, ProductItem, missionNum, this);
            }
        }

        protected void UpdateActionForProduce(double time)
        {
            OnProduceUpdate?.Invoke(time);
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
            }
            // ��һ������
            if (!StartProduce())
            {
                StopProduce();
            }
            OnActionChange?.Invoke();
            OnProduceEnd?.Invoke();
        }

        protected void OnWorkerStatusChangeAction(Status status)
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

        protected void OnWorkerAPChangeAction(int ap)
        {
            OnActionChange?.Invoke();
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
                foreach (MissionTransport mission in MissionTransports)
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
                        OnActionChange?.Invoke();
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
                        OnActionChange?.Invoke();
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
                            OnActionChange?.Invoke();
                        }
                        else
                        {
                            if (complete && StackReserve < amount)
                            {
                                return 0;
                            }
                            amount = !complete && StackReserve < amount ? Stack : amount;
                            StackReserve -= amount;
                        }
                        OnActionChange?.Invoke();
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
                    var inventory = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory;
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
                var inventory = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory;
                foreach (var kv in tempRawItems)
                {
                    string itemID = kv.Key;
                    int amount = inventory.GetItemAllNum(itemID);
                    int maxAmount = StackMaxNum * Recipe.GetRawNum(itemID) - kv.Value;
                    amount = amount <= maxAmount ? amount : maxAmount;
                    if (inventory.RemoveItem(itemID, amount))
                    {
                        RawItems[itemID] += amount;
                        StartProduce();
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
            return Add(itemID, amount, true) >= amount;
        }
        public int PutOut(string itemID, int amount)
        {
            return Remove(itemID, amount, false, true);
        }
        #endregion

        #region IInventory�ӿ�
        public bool AddItem(Item item)
        {
            if (item != null)
            {
                return Add(item.ID, item.Amount) >= item.Amount;
            }
            return false;
        }
        public bool RemoveItem(Item item)
        {
            if (item != null)
            {
                return Remove(item.ID, item.Amount) >= item.Amount;
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
            return Remove(itemID, amount) >= amount;
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