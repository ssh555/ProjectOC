using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using ProjectOC.MineSystem;

namespace ProjectOC.ProNodeNS
{
    [LabelText("采矿生产节点"), Serializable]
    public class MineProNode : IProNode, WorkerNS.IWorkerContainer
    {
        #region ProNode
        [LabelText("开采时间"), ReadOnly]
        public int MineTimeCost;
        [LabelText("开采经验"), ReadOnly]
        public int MineExp;
        [LabelText("开采堆叠上限"), ReadOnly]
        public int MineStackMax;
        [LabelText("开采搬运阈值"), ReadOnly]
        public int MineTransThreshold;
        [LabelText("堆积预留量"), ReadOnly]
        public List<int> StackReserves = new List<int>();
        [LabelText("是否有矿"), ReadOnly]
        public bool HasMine => MineDatas != null && MineDatas.Count > 0;
        [LabelText("矿"), ReadOnly, NonSerialized]
        public List<MineSystemData.MineData> MineDatas = new List<MineSystemData.MineData>();

        [LabelText("经验类型"), ShowInInspector, ReadOnly]
        public WorkerNS.SkillType ExpType = WorkerNS.SkillType.Collect;

        public MineProNode(ProNodeTableData config) : base(config) 
        {
            var mineConfig = ManagerNS.LocalGameManager.Instance.ProNodeManager.Config;
            MineTimeCost = mineConfig.MineTimeCost;
            MineExp = mineConfig.MineExp;
            MineStackMax = mineConfig.MineStackThreshold;
            MineTransThreshold = mineConfig.MineTransThreshold;
            DataContainer.OnDataChangeEvent -= OnContainerDataChangeEvent;
            DataContainer.OnDataChangeEvent += OnContainerDataChangeEventForMineData;
            OnLevelChangeEvent += () => { RemoveMine(); (this as WorkerNS.IWorkerContainer).RemoveWorker(); };
        }
        public void ChangeMine(List<MineSystemData.MineData> mines)
        {
            lock (this)
            {
                RemoveMine();
                if (mines != null && mines.Count > 0)
                {
                    MineDatas.AddRange(mines);
                    List<DataNS.IDataObj> datas = new List<DataNS.IDataObj>();
                    List<int> dataCapacitys = new List<int>();
                    for (int i = 0; i < MineDatas.Count; i++)
                    {
                        dataCapacitys.Add(MineDatas[i].GainItems.num * MineStackMax);
                        datas.Add(new DataNS.ItemIDDataObj(MineDatas[i].GainItems.id));
                        StackReserves.Add(0);
                        MineDatas[i].RegisterProNode();
                    }
                    ResetData(datas, dataCapacitys);
                    StartRun();
                }
            }
        }
        public void RemoveMine()
        {
            StopRun();
            ClearData();
            if (MineDatas != null)
            {
                for (int i = 0; i < MineDatas.Count; i++)
                {
                    MineDatas[i].UnRegisterProNode();
                }
                MineDatas.Clear();
            }
            StackReserves.Clear();
        }
        protected void OnContainerDataChangeEventForMineData()
        {
            if (HasMine)
            {
                for (int i = 0; i < MineDatas.Count; i++)
                {
                    int cur = StackReserves[i] + (this as MissionNS.IMissionObj).GetNeedAssignNum(DataContainer.GetData(i), false) 
                        - DataContainer.GetAmount(i, DataNS.DataOpType.Storage);
                    if (cur > 0)
                    {
                        foreach (var mission in (this as MissionNS.IMissionObj).GetMissions(DataContainer.GetData(i), false))
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
                        StackReserves[i] -= cur;
                    }
                }
            }
            StartProduce();
            OnDataChangeEvent?.Invoke();
        }
        #endregion

        #region Override
        public override int GetEff() { return Worker != null ? EffBase + Worker.GetEff(ExpType) : 0; }
        public override int GetTimeCost() { int eff = GetEff(); return HasMine && eff > 0 ? (int)Math.Ceiling((double)100 * MineTimeCost / eff) : 0; }
        public override void Destroy()
        {
            RemoveMine();
            (this as WorkerNS.IWorkerContainer).RemoveWorker();
        }
        public override bool CanWorking()
        {
            if (HasMine)
            {
                if (!(HaveWorker && Worker.IsOnProNodeDuty && !Worker.HaveFeatSeat)) { return false; }
                bool flag = true;
                for (int i = 0; i < MineDatas.Count; i++)
                {
                    int stackAll = DataContainer.GetAmount(i, DataNS.DataOpType.StorageAll);
                    if (stackAll < MineStackMax * MineDatas[i].GainItems.num && MineDatas[i].RemianMineNum > 0)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag) { return false; }
                if (RequirePower && WorldProNode != null && WorldProNode.PowerCount <= 0) { return false; }
                return true;
            }
            return false;
        }
        protected override void EndActionForMission()
        {
            for(int i = 0; i < StackReserves.Count; i++)
            {
                if (StackReserves[i] > 0)
                {
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionNS.MissionTransportType.ProNode_Store,
                        DataContainer.GetData(i), StackReserves[i], this, MissionNS.MissionInitiatorType.PutOut_Initiator);
                    StackReserves[i] = 0;
                }
            }
        }
        protected override void EndActionForProduce()
        {
            for (int i = 0; i < MineDatas.Count; i++)
            {
                int stackAll = DataContainer.GetAmount(i, DataNS.DataOpType.StorageAll);
                int gainNum = MineDatas[i].GainItems.num;
                if (stackAll < MineStackMax * gainNum && MineDatas[i].Consume())
                {
                    DataContainer.ChangeAmount(i, gainNum, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty, true);
                    int needAssignNum = (this as MissionNS.IMissionObj).GetNeedAssignNum(DataContainer.GetData(i), false);
                    int stack = DataContainer.GetAmount(i, DataNS.DataOpType.Storage);
                    if (stack >= StackReserves[i] + needAssignNum + MineTransThreshold * gainNum)
                    {
                        StackReserves[i] = stack - needAssignNum;
                    }
                }
            }
            Worker.SettleDuty(ExpType, MineExp, RealAPCost_Duty);
            if (!StartProduce()) { StopProduce(); }
            OnProduceEndEvent?.Invoke();
        }
        protected void OnWorkerStatusChangeEvent(WorkerNS.Status status)
        {
            if (status != WorkerNS.Status.Relaxing) { StartProduce(); }
            else { StopProduce(); }
            OnDataChangeEvent?.Invoke();
        }
        protected void OnWorkerAPChangeEvent(int ap) { OnDataChangeEvent?.Invoke(); }
        public override void ProNodeOnPositionChange(Vector3 differ)
        {
            OnPositionChange(differ);
            (this as MissionNS.IMissionObj).OnPositionChangeTransport();
        }
        public override void FastAdd() { }
        public override void PutIn(int index, DataNS.IDataObj data, int amount) { }
        #endregion

        #region IWorkerContainer
        public Action<WorkerNS.Worker> OnSetWorkerEvent { get; set; }
        public Action<bool, WorkerNS.Worker> OnRemoveWorkerEvent { get; set; }
        public WorkerNS.Worker Worker { get; set; }
        public bool IsArrive { get; set; }
        public bool HaveWorker => Worker != null && !string.IsNullOrEmpty(Worker.ID);
        public WorkerNS.WorkerContainerType GetContainerType() { return WorkerNS.WorkerContainerType.Work; }
        public void OnArriveEvent(WorkerNS.Worker worker)
        {
            (this as WorkerNS.IWorkerContainer).OnArriveSetPosition(worker, WorldProNode.transform.position + new Vector3(0, 2f, 0));
            worker.ProNode.StartProduce();
        }
        public void OnPositionChange(Vector3 differ)
        {
            RemoveMine();
            (this as WorkerNS.IWorkerContainer).RemoveWorker();
        }
        public void SetWorkerRelateData()
        {
            if (Worker != null)
            {
                Worker.SetTimeStatusAll(WorkerNS.TimeStatus.Work_OnDuty);
                Worker.SetDestination(WorldProNode.transform.position, OnArriveEvent, GetContainerType());
                Worker.OnStatusChangeEvent += OnWorkerStatusChangeEvent;
                Worker.OnAPChangeEvent += OnWorkerAPChangeEvent;
            }
        }
        public void RemoveWorkerRelateData()
        {
            StopProduce();
            if (HaveWorker)
            {
                Worker.OnStatusChangeEvent -= OnWorkerStatusChangeEvent;
                Worker.OnAPChangeEvent -= OnWorkerAPChangeEvent;
            }
        }
        public bool TempRemoveWorker()
        {
            if (Worker != null && !IsOnProduce && IsArrive)
            {
                Worker.RecoverLastPosition();
                IsArrive = false;
                return true;
            }
            return false;
        }
        #endregion
    }
}