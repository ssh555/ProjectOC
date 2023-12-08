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
    /// �����ڵ�
    /// </summary>
    [System.Serializable]
    public abstract class ProductionNode : MonoBehaviour, ITickComponent
    {
        /// <summary>
        /// ����ʵ����ID��ȫ��Ψһ
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
        public ProductionNodeState State;
        /// <summary>
        /// �������ȼ�
        /// </summary>
        public PriorityTransport PriorityTransport;

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
        public int StackNumCur { get { return StackNumAssign+ StackNumNoAssign; } }
        /// <summary>
        /// �ѷ��ݴ�����
        /// </summary>
        public int StackNumMax;
        /// <summary>
        /// ��ǰû�з��������Ķѻ�ֵ
        /// ���ֻ����ȡ�����һ��������
        /// </summary>
        public int StackNumNoAssign { get; protected set; }
        /// <summary>
        /// ��ǰ�ѷ����������δ�����˵Ķѻ�ֵ
        /// Workerֻ����ȡ����ڶ���������
        /// </summary>
        public int StackNumAssign { get; protected set; }
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

        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public RecipeManager RecipeManager { get => GameManager.Instance.GetLocalManager<RecipeManager>(); }

        public void InitProductionNode(string id)
        {
            GameManager.Instance.TickManager.RegisterTick(tickPriority, this);
            // TODO: ����������
            this.ID = id;
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
            this.StackNumNoAssign = 0;
            this.StackNumAssign = 0;
            this.RawItems.Clear();
            this.State = ProductionNodeState.Vacancy;
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
        /// �����ڸڵ���
        /// </summary>
        /// <param name="worker">�µ���</param>
        /// <returns>�����Ƿ�ɹ�</returns>
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
        /// ��ʼ������Ʒ
        /// </summary>
        public void StartWorking()
        {
            if (this.CanWorking())
            {
                foreach (var kv in this.Recipe.RawItems)
                {
                    this.RawItems[kv.Key] -= kv.Value;
                }
                // ����������ʱ��
                this.TimerForProduce.Reset(this.TimeCost);
                if (this.Type == ProductionNodeType.Mannul)
                {
                    this.Worker.Status = Status.Working;
                }
                this.State = ProductionNodeState.Production;
            }
        }
        /// <summary>
        /// ֹͣ������Ʒ
        /// </summary>
        public void EndWorking()
        {
            // ֹͣ������ʱ��
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
        /// �Ƿ���Կ�ʼ����
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
        /// �Ƿ���������
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
        /// ԭ���Ƿ����
        /// û�䷽Ĭ��ԭ�ϳ���
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
        /// �����Ƿ�ѻ���
        /// </summary>
        /// <returns></returns>
        public bool IsStackMax()
        {
            return this.StackNumCur >= this.StackNumMax;
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

        protected void EndActionForMission()
        {
            // ֻ����Recipeʱ�Ż���TimerForMission����
            foreach (var kv in this.Recipe.RawItems)
            {
                if (this.RawItems[kv.Key] < kv.Value * this.NeedQuantityThreshold)
                {
                    // TODO:�ַ����񣬴Ӳֿ����kv.Value * this.NeedQuantityThreshold - this.RawItems[kv.Key]�ݲ�����
                }
            }
            if (this.StackNumAssign > 0)
            {
                // TODO:�ַ����񣬰���StackNumNoAssignToMission�ݲ�����Ʒ���ֿ�
            }
        }
        protected void EndActionForProduce()
        {
            // ����
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
            // ��һ������
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