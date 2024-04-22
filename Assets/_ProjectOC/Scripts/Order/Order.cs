using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.Order.OrderManager;

namespace ProjectOC.Order
{
    public abstract class Order
    {
        private string orderID;

        public string OrderID
        {
            get { return orderID; }
            private set { orderID = value; }
        }

        //剩余没扣除的物品
        private Dictionary<string,int> remainRequireItemDic = new Dictionary<string,int>();
        private Dictionary<string, int> RequireItem = new Dictionary<string, int>();
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

        private readonly bool canBeCancled;
        //private bool canBeCommit { get; set; }

        public Order(string orderId,List<OrderMap> RequireItemList)
        { 
            this.OrderID = orderId;
            
            //深拷贝
            foreach (var item in RequireItemList)
            {
                this.remainRequireItemDic.Add(item.id, item.num);
                RequireItem.Add(item.id, item.num);
                addedItemDic.Add(item.id, 0);
            }

        }

        public bool ChangeRequireItemDic(string ItemId,int ItemNum)
        {
            if(this.remainRequireItemDic.ContainsKey(ItemId))
            {
                remainRequireItemDic[ItemId] -= ItemNum;
                addedItemDic[ItemId] += ItemNum;
            }

            foreach (var item in RequireItem)
            {
                Debug.Log(item.Key+" "+addedItemDic[item.Key] + " "+item.Value);
                if (addedItemDic[item.Key] != item.Value)
                {
                    return false;
                }
            }
            return true;
        }

    }


}


