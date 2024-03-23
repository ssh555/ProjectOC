using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.TextContent;
using ProjectOC.ProNodeNS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.OrderNS
{
    [System.Serializable]
    public struct OrderTableData
    {
        public string ID;
        public TextContent Name;
        public TextContent Description;
        public OrderType OrderType;
        public List<Formula> RequireList;
        public List<Formula> ItemRewards;
        public List<Formula> ClanRewards;
        public List<Formula> CharaRewards;
        public int CD;
    }
    [System.Serializable]
    public struct OrderUnlockTableData
    {
        public string ClanID;
        public List<string> Level_0_Unlock;
        public List<string> Level_1_Unlock;
        public List<string> Level_2_Unlock;
        public List<string> Level_3_Unlock;
        public List<string> Level_4_Unlock;
        public List<string> Level_5_Unlock;
    }

    /// <summary>
    /// 订单管理器
    /// </summary>
    public sealed class OrderManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Load And Data
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        private Dictionary<string, OrderTableData> OrderTableDict = new Dictionary<string, OrderTableData>();
        private Dictionary<string, OrderUnlockTableData> OrderUnlockTableDict = new Dictionary<string, OrderUnlockTableData>();

        public ML.Engine.ABResources.ABJsonAssetProcessor<OrderTableData[]> ABJAProcessor;
        public ML.Engine.ABResources.ABJsonAssetProcessor<OrderUnlockTableData[]> ABJAProcessor1;

        public void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<OrderTableData[]>("OC/Json/TableData", "Order", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.OrderTableDict.Add(data.ID, data);
                }
            }, "订单表数据");
            ABJAProcessor.StartLoadJsonAssetData();

            ABJAProcessor1 = new ML.Engine.ABResources.ABJsonAssetProcessor<OrderUnlockTableData[]>("OC/Json/TableData", "OrderUnlock", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.OrderUnlockTableDict.Add(data.ClanID, data);
                }
            }, "订单解锁表数据");
            ABJAProcessor1.StartLoadJsonAssetData();
        }
        #endregion

        #region Getter
        public bool IsValidID(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return OrderTableDict.ContainsKey(id);
            }
            return false;
        }
        public string GetName(string id)
        {
            if (IsValidID(id))
            {
                return OrderTableDict[id].Name;
            }
            return "";
        }
        public string GetDescription(string id)
        {
            if (IsValidID(id))
            {
                return OrderTableDict[id].Description;
            }
            return "";
        }
        public OrderType GetOrderType(string id)
        {
            if (IsValidID(id))
            {
                return OrderTableDict[id].OrderType;
            }
            return OrderType.None;
        }


        #endregion
    }
}

