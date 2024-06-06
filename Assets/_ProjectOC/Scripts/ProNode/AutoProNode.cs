using System;
using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [LabelText("自动生产节点"), Serializable]
    public class AutoProNode : IProNode
    {
        #region ProNode
        public AutoProNode(ProNodeTableData config) : base(config) { }
        #endregion

        #region Override
        public override int GetEff() { return EffBase; }
        public override int GetTimeCost() { return HasRecipe && EffBase > 0 ? (int)Math.Ceiling((double)100 * Recipe.TimeCost / EffBase) : 0; }
        public override void FastAdd() { for (int i = 1; i < DataContainer.GetCapacity(); i++) { FastAdd(i); } }
        public override void Destroy() { RemoveRecipe(); }
        public override bool CanWorking()
        {
            if (HasRecipe)
            {
                foreach (var kv in Recipe.Raw)
                {
                    if (DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) < kv.num) { return false; }
                }
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
                    var list = (this as MissionNS.IMissionObj).GetMissions(data);
                    if (list.Count > 0)
                    {
                        list[0].ChangeMissionNum(list[0].MissionNum + missionNum);
                    }
                    else
                    {
                        ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionNS.MissionTransportType.Store_ProNode, data, missionNum, this, MissionNS.MissionInitiatorType.PutIn_Initiator);
                    }
                }
            }
            if (StackReserve > 0)
            {
                var missionType = ML.Engine.InventorySystem.ItemManager.Instance.GetItemType(ProductID) == ML.Engine.InventorySystem.ItemType.Feed ?
                    MissionNS.MissionTransportType.ProNode_Restaurant : MissionNS.MissionTransportType.ProNode_Store;
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission
                    (missionType, DataContainer.GetData(0), StackReserve, this, MissionNS.MissionInitiatorType.PutOut_Initiator);
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
            // 下一次生产
            if (!StartProduce())
            {
                StopProduce();
            }
            OnProduceEndEvent?.Invoke();
        }
        public override void ProNodeOnPositionChange(UnityEngine.Vector3 differ)
        {
            (this as MissionNS.IMissionObj).OnPositionChangeTransport();
        }
        public override void PutIn(int index, DataNS.IDataObj data, int amount) {}
        #endregion
    }
}