using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [LabelText("自动生产节点"), Serializable]
    public class AutoProNode : IProNode
    {
        #region ProNode
        public AutoProNode(ProNodeTableData config) : base(config) { InitData(0, 0); }
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
        public override bool ChangeRecipe(string recipeID)
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
        protected override void EndActionForMission()
        {
            int missionNum;
            foreach (var kv in Recipe.Raw)
            {
                missionNum = kv.num * RawThreshold - DataContainer.GetAmount(kv.id, DataNS.DataOpType.Storage) 
                    - (this as MissionNS.IMissionObj).GetMissionNum(new DataNS.ItemIDDataObj(kv.id), true);
                if (missionNum > 0)
                {
                    missionNum += kv.num * (StackMax - RawThreshold);
                    ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission
                        (MissionNS.MissionTransportType.Store_ProNode, new DataNS.ItemIDDataObj(kv.id), missionNum, this, MissionNS.MissionInitiatorType.PutIn_Initiator);
                }
            }
            if (StackReserve > 0)
            {
                var missionType = ML.Engine.InventorySystem.ItemManager.Instance.GetItemType(ProductID) == ML.Engine.InventorySystem.ItemType.Feed ?
                    MissionNS.MissionTransportType.ProNode_Restaurant : MissionNS.MissionTransportType.ProNode_Store;
                ManagerNS.LocalGameManager.Instance.MissionManager.CreateTransportMission
                    (missionType, new DataNS.ItemIDDataObj(ProductID), StackReserve, this, MissionNS.MissionInitiatorType.PutOut_Initiator);
                StackReserve = 0;
            }
        }
        protected override void EndActionForProduce()
        {
            ML.Engine.InventorySystem.Item item = Recipe.Composite(this);
            AddItem(item);
            int needAssignNum = (this as MissionNS.IMissionObj).GetNeedAssignNum(new DataNS.ItemIDDataObj(ProductID), false);
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
        #endregion
    }
}