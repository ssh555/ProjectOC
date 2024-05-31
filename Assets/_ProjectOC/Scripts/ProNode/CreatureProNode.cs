using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [LabelText("培育舱"), Serializable]
    public class CreatureProNode : IProNode
    {
        // Container Index 0:Product, 1->Capacity-3:Raw, Capacity-2:Creature, Capacity-1:Discard
        #region ProNode
        public int OutputThreshold;
        public bool HasCreature => HasRecipe && DataContainer.GetCapacity() == Recipe.Raw.Count + 3;
        public ML.Engine.InventorySystem.CreatureItem Creature => HasCreature &&
            DataContainer.GetData(DataContainer.GetCapacity() - 2) is ML.Engine.InventorySystem.CreatureItem item ? item : null;
        public int DiscardStackAll => HasCreature ? DataContainer.GetAmount(DataContainer.GetCapacity() - 1, DataNS.DataOpType.StorageAll) : 0;
        public int DiscardStack => HasCreature ? DataContainer.GetAmount(DataContainer.GetCapacity() - 1, DataNS.DataOpType.Storage) : 0;
        public int DiscardReserve;
        public CreatureProNode(ProNodeTableData config) : base(config) { OnDataChangeEvent += CheckDiscardReserve; }
        public bool ChangeCreature(ML.Engine.InventorySystem.CreatureItem creature)
        {
            lock (this)
            {
                if (creature != null && !ManagerNS.LocalGameManager.Instance.Player.GetInventory().RemoveItem(creature)) { return false; }
                OutputThreshold = 0;
                DiscardReserve = 0;
                if (creature != null && ChangeRecipe(creature.ProRecipeID))
                {
                    DataContainer.AddCapacity(2, new List<int> { 1, creature.Discard.num * StackMax });
                    int capacity = DataContainer.GetCapacity();
                    ChangeData(capacity-2, creature, false, false);
                    ChangeData(capacity-1, new DataNS.ItemIDDataObj(creature.Discard.id), false, false);
                    DataContainer.ChangeAmount(capacity-2, 1, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty);
                }
                else { ChangeRecipe(""); }
                return true;
            }
        }
        #endregion

        #region Override
        public override int GetEff()
        {
            return ManagerNS.LocalGameManager.Instance != null && HasCreature ? 
                EffBase + Creature.Output * ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.CreatureOutputAddEff : 
                EffBase;
        }
        public override int GetTimeCost() { int eff = GetEff(); return HasRecipe && eff > 0 ? (int)Math.Ceiling((double)100 * Recipe.TimeCost / eff) : 0; }
        public override void FastAdd() { if (HasCreature) { for (int i = 1; i < DataContainer.GetCapacity() - 2; i++) { FastAdd(i); } } }
        public override void Destroy() { RemoveRecipe(); }
        public override bool CanWorking()
        {
            if (HasRecipe)
            {
                if (!HasCreature) { return false; }
                foreach (var kv in Recipe.Raw)
                {
                    if (DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) < kv.num) { return false; }
                }
                if (DiscardStackAll >= Creature.Discard.num * StackMax) { return false; }
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
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionNS.MissionTransportType.Store_ProNode, 
                        data, missionNum, this, MissionNS.MissionInitiatorType.PutIn_Initiator);
                }
            }
            if (StackReserve > 0)
            {
                var missionType = ML.Engine.InventorySystem.ItemManager.Instance.GetItemType(ProductID) == ML.Engine.InventorySystem.ItemType.Feed ?
                    MissionNS.MissionTransportType.ProNode_Restaurant : MissionNS.MissionTransportType.ProNode_Store;
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(missionType, 
                    DataContainer.GetData(0), StackReserve, this, MissionNS.MissionInitiatorType.PutOut_Initiator);
                StackReserve = 0;
            }
            var creature = Creature;
            if (creature != null)
            {
                if (creature.Output <= OutputThreshold && (this as MissionNS.IMissionObj).GetMissionNum(creature, false) == 0)
                {
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission
                        (MissionNS.MissionTransportType.Store_ProNode, creature, 1, this, 
                        MissionNS.MissionInitiatorType.PutIn_Initiator, DataContainer.GetCapacity() - 2, true);
                }
                if (DiscardReserve > 0)
                {
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission
                            (MissionNS.MissionTransportType.ProNode_Store, DataContainer.GetData(DataContainer.GetCapacity() - 1), 
                            DiscardReserve, this, MissionNS.MissionInitiatorType.PutOut_Initiator);
                    DiscardReserve = 0;
                }
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
            var creature = Creature;
            creature.Activity -= 1;
            int descCount = ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.CreatureOutputDescCount;
            if (creature.Activity < 0 && creature.Activity % descCount == 0 && creature.Output > 0)
            {
                int descValue = ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.CreatureOutputDescValue;
                creature.Output -= (descValue <= creature.Output) ? descValue : creature.Output;
            }
            // 下一次生产
            if (!StartProduce()) { StopProduce(); }
            OnProduceEndEvent?.Invoke();
        }
        public override void ProNodeOnPositionChange(UnityEngine.Vector3 differ)
        {
            (this as MissionNS.IMissionObj).OnPositionChangeTransport();
        }
        public override void PutIn(int index, DataNS.IDataObj data, int amount)
        {
            if (HasCreature && data is ML.Engine.InventorySystem.CreatureItem item && index == DataContainer.GetCapacity() - 2 && amount == 1)
            {
                var formula = Creature.Discard;
                ChangeData(index, item, false);
                DataContainer.ChangeAmount(index, 1, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty);
                DataContainer.ChangeAmount(DataContainer.GetCapacity() - 1, formula.num, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty, true);
                int needAssignNum = (this as MissionNS.IMissionObj).GetNeedAssignNum(DataContainer.GetData(DataContainer.GetCapacity() - 1), false);
                int discardStack = DiscardStack;
                if (discardStack >= DiscardReserve + needAssignNum + StackThreshold * formula.num)
                {
                    DiscardReserve = discardStack - needAssignNum;
                }
            }
            else
            {
                UnityEngine.Debug.Log($"Error CreatureProNode PutIn {HasCreature} {data is ML.Engine.InventorySystem.CreatureItem} {index} {DataContainer.GetCapacity() - 2} {amount}");
            }
        }
        #endregion
        public void CheckDiscardReserve()
        {
            if (HasCreature)
            {
                int cur = DiscardReserve + (this as MissionNS.IMissionObj).GetNeedAssignNum(DataContainer.GetData(DataContainer.GetCapacity() - 1), false) - DiscardStack;
                if (cur > 0)
                {
                    foreach (var mission in (this as MissionNS.IMissionObj).GetMissions(DataContainer.GetData(DataContainer.GetCapacity() - 1), false))
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
                    DiscardReserve -= cur;
                }
            }
        }
    }
}