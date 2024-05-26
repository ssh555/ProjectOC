using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [LabelText("繁育中心"), Serializable]
    public class BreedProNode : IProNode
    {
        // Container Index 0:Product, 1:Raw, 2:Parent1, 3:Parent2, 4:Discard
        #region ProNode
        public int OutputThreshold;
        public ML.Engine.InventorySystem.CreatureItem Creature1 => HasRecipe ? DataContainer.GetData(2) as ML.Engine.InventorySystem.CreatureItem : null;
        public ML.Engine.InventorySystem.CreatureItem Creature2 => HasRecipe ? DataContainer.GetData(3) as ML.Engine.InventorySystem.CreatureItem : null;
        public ML.Engine.InventorySystem.CreatureItem Child => HasRecipe ? DataContainer.GetData(0) as ML.Engine.InventorySystem.CreatureItem : null;
        public bool HasCreature => HasRecipe && DataContainer.GetAmount(2, DataNS.DataOpType.Storage) > 0 && DataContainer.GetAmount(3, DataNS.DataOpType.Storage) > 0;
        public int DiscardStackAll => HasCreature ? DataContainer.GetAmount(4, DataNS.DataOpType.StorageAll) : 0;
        public int DiscardStack => HasCreature ? DataContainer.GetAmount(4, DataNS.DataOpType.Storage) : 0;
        public int DiscardReserve;
        public BreedProNode(ProNodeTableData config) : base(config) { OnDataChangeEvent += CheckDiscardReserve; }
        public bool ChangeCreature(int index, ML.Engine.InventorySystem.CreatureItem creature)
        {
            lock (this)
            {
                if (index != 0 || index != 1 || (creature != null && !ManagerNS.LocalGameManager.Instance.Player.GetInventory().RemoveItem(creature))) { return false; }
                if (index == 0)
                {
                    OutputThreshold = 0;
                    DiscardReserve = 0;
                    if (creature != null && ChangeRecipe(creature.ProRecipeID))
                    {
                        DataContainer.AddCapacity(3, new List<int> { 1, 1, creature.Discard.num * StackMax });
                        ChangeData(2, creature);
                        ChangeData(4, new DataNS.ItemIDDataObj(creature.Discard.id), false);
                        DataContainer.ChangeAmount(2, 1, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty);
                        return true;
                    }
                    else { ChangeRecipe(""); }
                }
                else
                {
                    if (HasRecipe && creature != null)
                    {
                        ChangeData(3, creature);
                        DataContainer.ChangeAmount(3, 1, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty);
                        return true;
                    }
                    else { ChangeData(3, null); }
                }
                return false;
            }
        }
        #endregion

        #region Override
        public override int GetEff() 
        {
            if (ManagerNS.LocalGameManager.Instance != null)
            {
                return EffBase + (Creature1?.Output ?? 0) * 
                    ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.CreatureOutputAddEff;
            }
            return EffBase; 
        }
        public override int GetTimeCost() { int eff = GetEff(); return HasRecipe && eff > 0 ? (int)Math.Ceiling((double)100 * Recipe.TimeCost / eff) : 0; }
        public override void FastAdd() { FastAdd(1); }
        public override void Destroy() { RemoveRecipe(); }
        public override bool CanWorking()
        {
            if (HasRecipe)
            {
                if (!HasCreature) { return false; }
                foreach (var kv in Recipe.Raw) { if (DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) < kv.num) { return false; } }
                if (DiscardStackAll >= Creature1.Discard.num * StackMax) { return false; }
                if (StackAll >= StackMax * ProductNum) { return false; }
                if (DataContainer.HaveSetData(0)) { return false; }
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
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionNS.MissionTransportType.ProNode_Store, 
                    DataContainer.GetData(0), StackReserve, this, MissionNS.MissionInitiatorType.PutOut_Initiator, reserveEmpty:true);
                StackReserve = 0;
            }
            if (DiscardReserve > 0)
            {
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission(MissionNS.MissionTransportType.ProNode_Store, 
                    DataContainer.GetData(4), DiscardReserve, this, MissionNS.MissionInitiatorType.PutOut_Initiator);
                DiscardReserve = 0;
            }
        }
        protected override void EndActionForProduce()
        {
            lock (this)
            {
                ML.Engine.InventorySystem.Item item = Recipe.Composite(this);
                if (item is ML.Engine.InventorySystem.CreatureItem creature)
                {
                    int output1 = Creature1.Output;
                    int output2 = Creature2.Output;
                    int low = -3 + output1 <= output2 ? output1 : output2;
                    int high = 3 + output1 <= output2 ? output2 : output1;
                    List<int> bounds = new List<int>();
                    for (int i = 0; i < 5; i++)
                    {
                        bounds.Add((4 * low + (high - low) * i) / 4);
                    }
                    List<int> ranges = ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.OutputRanges;
                    int output = 0;
                    int rand = UnityEngine.Random.Range(1, 101);
                    for (int i = 0; i < 4; i++)
                    {
                        if (ranges[i] <= rand && rand <= ranges[i + 1] - 1)
                        {
                            output = UnityEngine.Random.Range(bounds[i], bounds[i + 1] + 1);
                            break;
                        }
                    }
                    output = output <= 0 ? 0 : output;
                    output = output >= 50 ? 50 : output;
                    if (output >= OutputThreshold)
                    {
                        creature.Output = output;
                        ChangeData(0, creature);
                        DataContainer.ChangeAmount(0, 1, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty);
                        StackReserve = 1;
                    }
                    else
                    {
                        int discardNum = creature.Discard.num;
                        DataContainer.ChangeAmount(4, discardNum, DataNS.DataOpType.Storage, DataNS.DataOpType.Empty, true);
                        int needAssignNum = (this as MissionNS.IMissionObj).GetNeedAssignNum(DataContainer.GetData(4), false);
                        int discardStack = DiscardStack;
                        if (discardStack >= DiscardReserve + needAssignNum + StackThreshold * discardNum)
                        {
                            DiscardReserve = discardStack - needAssignNum;
                        }
                    }
                }
            }
            // 下一次生产
            if (!StartProduce()) { StopProduce(); }
            OnProduceEndEvent?.Invoke();
        }
        public override void ProNodeOnPositionChange(UnityEngine.Vector3 differ)
        {
            (this as MissionNS.IMissionObj).OnPositionChangeTransport();
        }
        public override void PutIn(int index, DataNS.IDataObj data, int amount) { }
        #endregion
        public void CheckDiscardReserve()
        {
            if (HasCreature)
            {
                int cur = DiscardReserve + (this as MissionNS.IMissionObj).GetNeedAssignNum(DataContainer.GetData(4), false) - DiscardStack;
                if (cur > 0)
                {
                    foreach (var mission in (this as MissionNS.IMissionObj).GetMissions(DataContainer.GetData(4), false))
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