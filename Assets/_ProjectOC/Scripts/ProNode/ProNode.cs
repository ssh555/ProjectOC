using ML.Engine.Manager;
using ML.Engine.Timer;
using ML.Engine.InventorySystem;
using ProjectOC.MissionNS;
using ProjectOC.WorkerNS;
using System.Collections.Generic;
using System;
using UnityEngine;

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
        ///// <summary>
        ///// ����1���Ĳ�
        ///// </summary>
        //public Dictionary<string, int> Lv1Required { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetLv1Required(ID); }
        ///// <summary>
        ///// ����2���Ĳ�
        ///// </summary>
        //public Dictionary<string, int> Lv2Required { get => ManagerNS.LocalGameManager.Instance.ProNodeManager.GetLv2Required(ID); }
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
        public int StackAllNum { get => StackAll / ProductNum; }
        /// <summary>
        /// û�з�������Ķѻ�����
        /// </summary>
        public int StackNum { get => Stack / ProductNum; }
        /// <summary>
        /// ������ID
        /// </summary>
        public string ProductItem { get => Recipe?.GetProductID() ?? ""; }
        /// <summary>
        /// һ������������������
        /// </summary>
        public int ProductNum { get => Recipe?.GetProductNum() ?? 0; }
        /// <summary>
        /// �����ڵ�״̬
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
        /// ����Ч�� ��λ%
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
        /// <summary>
        /// �˹������ڵ㳣פ����
        /// </summary>
        public Worker Worker;
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
        /// <summary>
        ///  ԭ����ID, ���ж��ٸ�
        /// </summary>
        protected Dictionary<string, int> RawItems = new Dictionary<string, int>();
        public event Action OnActionChange;
        public event Action<double> OnProduceTimerUpdate;
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
                    timerForProduce.OnUpdateEvent += OnProduceTimerUpdate;
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
            this.ID = config.ID;
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
            Recipe recipe = ManagerNS.LocalGameManager.Instance.RecipeManager.SpawnRecipe(recipeID);
            if (recipe != null)
            {
                this.RemoveRecipe(player);
                this.Recipe = recipe;
                foreach (string itemID in this.Recipe.Raw.Keys)
                {
                    this.RawItems.Add(itemID, 0);
                }
                this.StartRun();
                return true;
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
            if (this.Recipe != null)
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
                        if (!player.Inventory.AddItem(item))
                        {
                            flag = true;
                        }
                    }
                }
            }
            // û�мӵ���ұ����Ķ����WorldItem
            foreach (Item item in resItems)
            {
                ItemManager.Instance.SpawnWorldItem(item, WorldProNode.transform.position, WorldProNode.transform.rotation);
            }
            // �������
            foreach (MissionTransport mission in this.MissionTransports)
            {
                mission.End();
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
                worker.ProNode = this;
                worker.SetTimeStatusAll(TimeStatus.Work_OnDuty);
                this.Worker = worker;
                this.StartProduce();
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
            if (this.ProNodeType == ProNodeType.Mannul)
            {
                this.StopProduce();
                if (this.Worker != null)
                {
                    this.Worker.SetTimeStatusAll(TimeStatus.Relax);
                    this.Worker.ProNode = null;
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
            if (this.CanWorking())
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
                this.TimerForProduce.End();
            }
        }

        /// <summary>
        /// �Ƿ���Կ�ʼ������Ʒ
        /// </summary>
        public bool CanWorking()
        {
            if (this.Recipe != null)
            {
                foreach (var kv in this.Recipe.Raw)
                {
                    if (this.RawItems[kv.Key] < kv.Value)
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
        /// ��������Ĺ���
        /// </summary>
        protected void EndActionForMission()
        {
            int missionNum;
            foreach (var kv in Recipe.Raw)
            {
                missionNum = kv.Value * RawThresholdNum - RawItems[kv.Key] - GetAssignNum(kv.Key, true);
                if (missionNum > 0)
                {
                    // �Ӳֿ���˲�����
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionTransportType.Store_ProNode, kv.Key, missionNum, this);
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
            Worker.AlterExp(ExpType, Recipe.ExpRecipe);
            Worker.AlterAP(-1 * Worker.APCost);
            // ��һ������
            if (!StartProduce())
            {
                StopProduce();
            }
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
            return result;
        }

        #region UI�ӿ�
        public void UIAdd(Player.PlayerCharacter player, string itemID, int amount)
        {
            if (player != null && RawItems.ContainsKey(itemID) && amount > 0 && 
                this.RawItems[itemID] + amount <= this.StackMaxNum * this.Recipe.GetRawNum(itemID))
            {
                if (player.Inventory.RemoveItem(itemID, amount))
                {
                    RawItems[itemID] += amount;
                    StartProduce();
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
        public void UIFastAdd(Player.PlayerCharacter player, string itemID)
        {
            if (player != null && RawItems.ContainsKey(itemID))
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
            if (RawItems.ContainsKey(itemID) && amount >= 0)
            {
                RawItems[itemID] += amount;
                StartProduce();
                return true;
            }
            OnActionChange?.Invoke();
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
                Debug.LogError($"Item Amount Error ItemAmount: {result.Amount} Amount: {amount}");
            }
            OnActionChange?.Invoke();
            return result;
        }
        public bool RemoveItem(string itemID, int amount)
        {
            if (amount >= 0)
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
            if (RawItems.ContainsKey(id))
            {
                return RawItems[id];
            }
            else if (ProductItem == id)
            {
                return StackAll;
            }
            Debug.LogError($"Item {id} is not in ProNode {ID}");
            return 0;
        }

        public Item[] GetItemList()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region TODO: ����
        ///// <summary>
        ///// �����ڵ�����
        ///// </summary>
        //public bool Upgrade(Player.PlayerCharacter player)
        //{
        //    if (this.Level < this.LevelMax && this.Level >= 0)
        //    {
        //        // �ӱ�����ȡ��������
        //        Dictionary<string, int> lvRequired;
        //        if (this.Level == 0)
        //        {
        //            lvRequired = Lv1Required;
        //        } 
        //        else if (this.Level == 1)
        //        {
        //            lvRequired = Lv2Required;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //        foreach (var kv in lvRequired)
        //        {
        //            if (player.Inventory.GetItemAllNum(kv.Key) < kv.Value)
        //            {
        //                return false;
        //            }
        //        }
        //        foreach (var kv in lvRequired)
        //        {
        //            if (player.Inventory.RemoveItem(kv.Key, kv.Value))
        //            {
        //                return false;
        //            }
        //        }
        //        this.EffBase += LevelUpgradeEff[Level];
        //        this.Level += 1;
        //        return true;
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// �����ڵ㽵��
        ///// </summary>
        //public bool Downgrade(Player.PlayerCharacter player)
        //{
        //    if (this.Level <= this.LevelMax && this.Level > 0)
        //    {
        //        // ����������ӵ�����
        //        Dictionary<string, int> lvRequired;
        //        if (this.Level == 1)
        //        {
        //            lvRequired = Lv1Required;
        //        }
        //        else if (this.Level == 2)
        //        {
        //            lvRequired = Lv2Required;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //        foreach (var kv in lvRequired)
        //        {
        //            bool flag = false;
        //            List<Item> items = ItemSpawner.Instance.SpawnItems(kv.Key, kv.Value);
        //            foreach (Item item in items)
        //            {
        //                if (flag)
        //                {
        //                    ItemSpawner.Instance.SpawnWorldItem(item, player.transform.position, player.transform.rotation);
        //                }
        //                else
        //                {
        //                    if (!player.Inventory.AddItem(item))
        //                    {
        //                        flag = true;
        //                    }
        //                }
        //            }
        //        }
        //        this.Level -= 1;
        //        this.EffBase -= LevelUpgradeEff[Level];
        //        return true;
        //    }
        //    return false;
        //}
        #endregion
    }
}