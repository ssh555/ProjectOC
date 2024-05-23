using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [LabelText("�����ڵ�"), Serializable]
    public abstract class IProNode : DataNS.DataContainerOwner, WorkerNS.IEffectObj
    {
        #region Data
        [HideInInspector]
        public IWorldProNode WorldProNode;
        [ReadOnly]
        public string ID = "";
        [LabelText("�������䷽"), ReadOnly]
        public ML.Engine.InventorySystem.Recipe Recipe;
        [LabelText("��������Ч��"), ReadOnly]
        public int EffBase;
        [LabelText("�ѻ�Ԥ����"), ReadOnly]
        public int StackReserve = 0;
        #endregion

        #region Timer
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
                    timerForProduce = new ML.Engine.Timer.CounterDownTimer(GetTimeCost(), false, false);
                    timerForProduce.OnEndEvent += EndActionForProduce;
                    timerForProduce.OnUpdateEvent += UpdateActionForProduce;
                }
                return timerForProduce;
            }
        }
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
        public bool HasRecipe => Recipe.IsValidRecipe;
        [LabelText("�ѻ�����"), ShowInInspector, ReadOnly]
        public int Stack => Recipe.IsValidRecipe ? DataContainer.GetAmount(0, DataNS.DataOpType.Storage) : 0;
        [LabelText("�ܶѻ�����"), ShowInInspector, ReadOnly]
        public int StackAll => Recipe.IsValidRecipe ? DataContainer.GetAmount(0, DataNS.DataOpType.StorageAll) : 0;
        [LabelText("������ID"), ShowInInspector, ReadOnly]
        public string ProductID => Recipe.ProductID;
        [LabelText("һ������������������"), ShowInInspector, ReadOnly]
        public int ProductNum => Recipe.ProductNum;
        [LabelText("�Ƿ���������"), ShowInInspector, ReadOnly]
        public bool IsOnRunning => timerForMission != null && !timerForMission.IsStoped;
        [LabelText("�Ƿ�����������Ʒ"), ShowInInspector, ReadOnly]
        public bool IsOnProduce => timerForProduce != null && !timerForProduce.IsStoped;
        [LabelText("�����ڵ�״̬"), ShowInInspector, ReadOnly]
        public ProNodeState State => HasRecipe ? (IsOnProduce ? ProNodeState.Production : ProNodeState.Stagnation) : ProNodeState.Vacancy;
        #endregion

        #region Table Property
        [LabelText("�����ڵ�����"), ShowInInspector, ReadOnly]
        public ProNodeType ProNodeType => ManagerNS.LocalGameManager.Instance != null ? 
            ManagerNS.LocalGameManager.Instance.ProNodeManager.GetProNodeType(ID) : ProNodeType.None;
        [LabelText("�����ڵ���Ŀ"), ShowInInspector, ReadOnly]
        public ML.Engine.InventorySystem.RecipeCategory Category => ManagerNS.LocalGameManager.Instance != null ? 
            ManagerNS.LocalGameManager.Instance.ProNodeManager.GetCategory(ID) : ML.Engine.InventorySystem.RecipeCategory.None;
        [LabelText("�ѷ�������"), ShowInInspector, ReadOnly]
        public int StackMax => ManagerNS.LocalGameManager.Instance != null ? 
            ManagerNS.LocalGameManager.Instance.ProNodeManager.GetMaxStack(ID) : 0;
        [LabelText("�ѷ���ֵ��"), ShowInInspector, ReadOnly]
        public int StackThreshold => ManagerNS.LocalGameManager.Instance != null ? 
            ManagerNS.LocalGameManager.Instance.ProNodeManager.GetStackThreshold(ID) : 0;
        [LabelText("������ֵ��"), ShowInInspector, ReadOnly]
        public int RawThreshold => ManagerNS.LocalGameManager.Instance != null ? 
            ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRawThreshold(ID) : 0;
        [LabelText("�Ƿ���Ҫ����"), ShowInInspector, ReadOnly]
        public bool RequirePower => ManagerNS.LocalGameManager.Instance != null ? 
            ManagerNS.LocalGameManager.Instance.ProNodeManager.GetCanCharge(ID) : false;
        #endregion

        #region Abstract
        public abstract int GetEff();
        public abstract int GetTimeCost();
        public abstract void FastAdd();
        public abstract void Destroy();
        /// <summary>
        /// �Ƿ���Կ�ʼ������Ʒ
        /// </summary>
        public abstract bool CanWorking();
        protected abstract void EndActionForMission();
        protected abstract void EndActionForProduce();
        public abstract void ProNodeOnPositionChange(Vector3 diff);
        #endregion

        #region ProNode
        public IProNode(ProNodeTableData config)
        {
            ID = config.ID ?? "";
            InitData(0, 0);
            EffBase = ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.EffBase;
            DataContainer.OnDataChangeEvent += OnContainerDataChangeEvent;
        }
        /// <summary>
        /// ��ʼ���������ڵ㣬���ʱ�򲢲�һ����ʼ������Ʒ
        /// </summary>
        public void StartRun()
        {
            if (!IsOnRunning)
            {
                TimerForMission.Start();
            }
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
            if (CanWorking())
            {
                if (!IsOnProduce)
                {
                    TimerForProduce.Reset(GetTimeCost());
                    return true;
                }
            }
            else
            {
                StopProduce();
            }
            return false;
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
        /// ��ȡ�����ڵ�����������䷽
        /// </summary>
        public List<string> GetCanProduceRecipe()
        {
            List<string> result = new List<string>();
            foreach (var recipeCategory in ManagerNS.LocalGameManager.Instance.ProNodeManager.GetRecipeCategoryFilterd(ID))
            {
                result.AddRange(ManagerNS.LocalGameManager.Instance.RecipeManager.GetRecipeIDsByCategory(recipeCategory));
            }
            return ManagerNS.LocalGameManager.Instance.RecipeManager.SortRecipeIDs(result);
        }
        public bool ChangeRecipe(string recipeID)
        {
            lock (this)
            {
                RemoveRecipe();
                if (!string.IsNullOrEmpty(recipeID))
                {
                    var recipe = ManagerNS.LocalGameManager.Instance.RecipeManager.SpawnRecipe(recipeID);
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
                        List<DataNS.IDataObj> datas = new List<DataNS.IDataObj>();
                        foreach (string id in itemIDs)
                        {
                            datas.Add(new DataNS.ItemIDDataObj(id));
                        }
                        ResetData(datas, dataCapacitys);
                        StartRun();
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

        #region Level
        [LabelText("�ȼ�"), ReadOnly]
        public int Level = 0;
        public bool SetLevel(int level)
        {
            var config = ManagerNS.LocalGameManager.Instance.ProNodeManager.Config;
            if (0 <= level && level <= config.LevelMax)
            {
                if (level > Level)
                {
                    for (int i = Level; i < level; i++)
                    {
                        EffBase += config.LevelUpgradeEff[i];
                    }
                }
                else if (level < Level)
                {
                    for (int i = Level; i > level; i--)
                    {
                        EffBase -= config.LevelUpgradeEff[i - 1];
                    }
                }
                Level = level;
                return true;
            }
            return false;
        }
        #endregion

        #region Event
        public Action OnDataChangeEvent;
        public event Action<double> OnProduceUpdateEvent;
        public Action OnProduceEndEvent;
        protected void UpdateActionForProduce(double time) { OnProduceUpdateEvent?.Invoke(time); }
        protected void OnContainerDataChangeEvent()
        {
            if (HasRecipe)
            {
                int cur = StackReserve + (this as MissionNS.IMissionObj).GetNeedAssignNum(DataContainer.GetData(0), false) - Stack;
                if (cur > 0)
                {
                    foreach (var mission in (this as MissionNS.IMissionObj).GetMissions(DataContainer.GetData(0), false))
                    {
                        int needAssignNum = mission.NeedAssignNum;
                        int missionNum = mission.MissionNum;
                        if (cur > needAssignNum)
                        {
                            mission.ChangeMissionNum(missionNum - needAssignNum);
                            cur -= needAssignNum;
                        }
                        else
                        {
                            mission.ChangeMissionNum(missionNum - cur);
                            cur = 0;
                            break;
                        }
                    }
                }
                if (cur > 0)
                {
                    StackReserve -= cur;
                }
            }
            StartProduce();
            OnDataChangeEvent?.Invoke();
        }
        #endregion

        #region DataContainerOwner
        public override Transform GetTransform() { return WorldProNode.transform; }
        public override string GetUID() { return WorldProNode.InstanceID; }
        public override MissionNS.MissionObjType GetMissionObjType() { return MissionNS.MissionObjType.ProNode; }
        #endregion

        #region IEffectObj
        [LabelText("��������_ֵ��"), ReadOnly, ShowInInspector]
        private int InitAPCost_Duty;
        [LabelText("����ֵ����������"), ReadOnly, ShowInInspector]
        private int ModifyAPCost_Duty;
        [LabelText("ֵ���������ı���"), ReadOnly, ShowInInspector]
        private float FactorAPCost_Duty;
        [LabelText("����ֵ����������"), ReadOnly, ShowInInspector]
        public int RealAPCost_Duty => (int)(InitAPCost_Duty * FactorAPCost_Duty + ModifyAPCost_Duty);
        public List<WorkerNS.Effect> Effects { get; set; } = new List<WorkerNS.Effect>();
        public void ApplyEffect(WorkerNS.Effect effect)
        {
            if (effect.EffectType != WorkerNS.EffectType.AlterProNodeVariable) { Debug.Log("type != AlterProNodeVariable"); return; }
            bool flag = true;
            if (effect.ParamStr == "EffBase")
            {
                EffBase += effect.ParamInt;
            }
            else if (effect.ParamStr == "FactorAPCostDuty")
            {
                FactorAPCost_Duty += effect.ParamFloat;
            }
            else
            {
                flag = false;
            }
            if (flag)
            {
                Effects.Add(effect);
            }
            else
            {
                Debug.Log($"ParamStr Error {effect.ParamStr}");
            }
        }
        public void RemoveEffect(WorkerNS.Effect effect)
        {
            if (effect.EffectType != WorkerNS.EffectType.AlterProNodeVariable) { Debug.Log("type != AlterProNodeVariable"); return; }
            Effects.Remove(effect);
            if (effect.ParamStr == "EffBase")
            {
                EffBase -= effect.ParamInt;
            }
            else if (effect.ParamStr == "FactorAPCostDuty")
            {
                FactorAPCost_Duty -= effect.ParamFloat;
            }
        }
        #endregion
    }
}