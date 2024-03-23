using ML.Engine.InventorySystem.CompositeSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.OrderNS
{
    /// <summary>
    /// 订单
    /// </summary>
    public struct Order
    {
        public string ID;
        public string Name;
        public string Description;
        public OrderType OrderType;
        /// <summary>
        /// 物资清单 (ItemID, Amount)
        /// </summary>
        public List<Formula> RequireList;
        /// <summary>
        /// 物品报酬 (ItemID, Amount)
        /// </summary>
        public List<Formula> ItemRewards;
        /// <summary>
        /// 氏族信赖报酬 (氏族ID, Amount)
        /// </summary>
        public List<Formula> ClanRewards;
        /// <summary>
        /// 角色信赖报酬 (角色ID, Amount)
        /// </summary>
        public List<Formula> CharaRewards;
        /// <summary>
        /// 需求周期
        /// </summary>
        public int CD;

        /// <summary>
        /// 接取日期
        /// </summary>
        public int AcceptDay;
        /// <summary>
        /// 完成日期
        /// </summary>
        public int FinishDay;

        public void End()
        {
            CD -= 1;
        }
    }
}
