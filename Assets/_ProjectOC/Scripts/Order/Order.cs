using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.Order.OrderManager;

namespace ProjectOC.Order
{
    [Serializable]
    public abstract class Order: IComparable<Order>
    {
        //订单实例化
        private string orderID;
        [ShowInInspector]
        public OrderType orderType;
        [ShowInInspector]
        //此变量为物品是否扣除完毕
        public bool canBeCommit;
        [ShowInInspector]
        public int acceptOrder;


        protected string orderInstanceID;
        [ShowInInspector]
        public string OrderInstanceID { get { return orderInstanceID; } }

        // 实现 IComparable 接口中的 CompareTo 方法
        public int CompareTo(Order other)
        {
            // 首先按照 orderType 升序
            int result = orderType.CompareTo(other.orderType);
            if (result != 0)
            {
                return result;
            }

            // 如果 orderType 相同，按照 canBeCommit 降序
            result = -canBeCommit.CompareTo(other.canBeCommit);
            if (result != 0)
            {
                return result;
            }

            // 如果 canBeCommit 也相同，按照 acceptOrder 升序
            return acceptOrder.CompareTo(other.acceptOrder);
        }

        [ShowInInspector]
        public string OrderID
        {
            get { return orderID; }
            private set { orderID = value; }
        }

        //剩余没扣除的物品
        private Dictionary<string,int> remainRequireItemDic = new Dictionary<string,int>();
        private Dictionary<string, int> requireItem = new Dictionary<string, int>();
        public Dictionary<string, int> RequireItem { get { return requireItem; } }
        public Dictionary<string, int> RemainRequireItemDic
        {
            get { return remainRequireItemDic; }
        }

        //已经扣除的物品
        private Dictionary<string, int> addedItemDic = new Dictionary<string, int>();

        public Dictionary<string, int> AddedItemDic
        {
            get { return addedItemDic; }
        }

        public readonly bool canBeCancled;

        public Order(OrderTableData orderTableData)
        { 
            this.OrderID = orderTableData.ID;
            this.orderType = orderTableData.OrderType;
            this.canBeCommit = false;
            //特殊订单不可取消
            this.canBeCancled = orderTableData.OrderType != OrderType.Special;
            this.acceptOrder = 0;
            this.orderInstanceID = OrderManager.Instance.GenerateOrderInstanceID(this.OrderID);
            //深拷贝
            foreach (var item in orderTableData.RequireList)
            {
                this.remainRequireItemDic.Add(item.id, item.num);
                requireItem.Add(item.id, item.num);
                addedItemDic.Add(item.id, 0);
            }

        }

        public Order()
        {
            this.OrderID = "";
        }

        public bool ChangeRequireItemDic(string ItemId,int ItemNum)
        {
            if(this.remainRequireItemDic.ContainsKey(ItemId))
            {
                remainRequireItemDic[ItemId] -= ItemNum;
                addedItemDic[ItemId] += ItemNum;
            }

            foreach (var item in requireItem)
            {
                if (addedItemDic[item.Key] != item.Value)
                {
                    return false;
                }
            }
            return true;
        }

    }


}


