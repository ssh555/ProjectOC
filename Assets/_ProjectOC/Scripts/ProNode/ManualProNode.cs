using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [LabelText("�˹������ڵ�"), Serializable]
    public class ManualProNode : IProNode, WorkerNS.IWorkerContainer
    {
        #region ProNode
        [LabelText("��������"), ShowInInspector, ReadOnly]
        public WorkerNS.SkillType ExpType => ManagerNS.LocalGameManager.Instance != null ?
            ManagerNS.LocalGameManager.Instance.ProNodeManager.GetExpType(ID) : WorkerNS.SkillType.None;
        public ManualProNode(ProNodeTableData config) : base(config) { }
        #endregion

        #region Override
        public override int GetEff() { return EffBase + Worker?.GetEff(ExpType) ?? 0; }
        public override int GetTimeCost() { return HasRecipe && GetEff() > 0 ? (int)Math.Ceiling((double)100 * Recipe.TimeCost / GetEff()) : 0; }
        public override void FastAdd() { for (int i = 1; i < DataContainer.GetCapacity(); i++) { FastAdd(i); } }
        public override void Destroy()
        {
            RemoveRecipe();
            (this as WorkerNS.IWorkerContainer).RemoveWorker();
        }
        public override bool CanWorking()
        {
            if (HasRecipe)
            {
                foreach (var kv in Recipe.Raw)
                {
                    if (DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) < kv.num) { return false; }
                }
                if (!(HaveWorker && Worker.IsOnProNodeDuty && !Worker.HaveFeatSeat)) { return false; }
                if (StackAll >= StackMax * ProductNum) { return false; }
                if (RequirePower && WorldProNode != null && WorldProNode.PowerCount <= 0) { return false; }
                return true;
            }
            return false;
        }
        protected override void EndActionForMission()
        {
            int missionNum;
            foreach (var kv in Recipe.Raw)
            {
                DataNS.ItemIDDataObj data = new DataNS.ItemIDDataObj(kv.id);
                missionNum = kv.num * RawThreshold - DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) - (this as MissionNS.IMissionObj).GetMissionNum(data, true);
                if (missionNum > 0)
                {
                    missionNum += kv.num * (StackMax - RawThreshold);
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionNS.MissionTransportType.Store_ProNode, data, missionNum, this, MissionNS.MissionInitiatorType.PutIn_Initiator);
                }
            }
            if (StackReserve > 0)
            {
                var missionType = ML.Engine.InventorySystem.ItemManager.Instance.GetItemType(ProductID) == ML.Engine.InventorySystem.ItemType.Feed ?
                    MissionNS.MissionTransportType.ProNode_Restaurant : MissionNS.MissionTransportType.ProNode_Store;
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(missionType, DataContainer.GetData(0), StackReserve, this, MissionNS.MissionInitiatorType.PutOut_Initiator);
                StackReserve = 0;
            }
        }
        protected override void EndActionForProduce()
        {
            ML.Engine.InventorySystem.Item item = Recipe.Composite(this);
            AddItem(item);
            int needAssignNum = (this as MissionNS.IMissionObj).GetNeedAssignNum(DataContainer.GetData(0), false);
            if (Stack >= StackReserve + needAssignNum + StackThreshold * ProductNum)
            {
                StackReserve = Stack - needAssignNum;
            }
            Worker.SettleDuty(ExpType, Recipe.ExpRecipe, RealAPCost_Duty);
            // ��һ������
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
            if (HaveWorker)
            {
                if (IsArrive) { Worker.transform.position += differ; }
                else { Worker.SetDestination(GetTransform().position, OnArriveEvent, GetContainerType()); }
            }
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