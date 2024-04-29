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
        //����ʵ����
        private string orderID;
        [ShowInInspector]
        public OrderType orderType;
        [ShowInInspector]
        //�˱���Ϊ��Ʒ�Ƿ�۳����
        public bool canBeCommit;
        [ShowInInspector]
        public int acceptOrder;


        protected string orderInstanceID;
        [ShowInInspector]
        public string OrderInstanceID { get { return orderInstanceID; } }

        // ʵ�� IComparable �ӿ��е� CompareTo ����
        public int CompareTo(Order other)
        {
            // ���Ȱ��� orderType ����
            int result = orderType.CompareTo(other.orderType);
            if (result != 0)
            {
                return result;
            }

            // ��� orderType ��ͬ������ canBeCommit ����
            result = -canBeCommit.CompareTo(other.canBeCommit);
            if (result != 0)
            {
                return result;
            }

            // ��� canBeCommit Ҳ��ͬ������ acceptOrder ����
            return acceptOrder.CompareTo(other.acceptOrder);
        }

        [ShowInInspector]
        public string OrderID
        {
            get { return orderID; }
            private set { orderID = value; }
        }

        //ʣ��û�۳�����Ʒ
        private Dictionary<string,int> remainRequireItemDic = new Dictionary<string,int>();
        private Dictionary<string, int> requireItem = new Dictionary<string, int>();
        public Dictionary<string, int> RequireItem { get { return requireItem; } }
        public Dictionary<string, int> RemainRequireItemDic
        {
            get { return remainRequireItemDic; }
        }

        //�Ѿ��۳�����Ʒ
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
            //���ⶩ������ȡ��
            this.canBeCancled = orderTableData.OrderType != OrderType.Special;
            this.acceptOrder = 0;
            this.orderInstanceID = OrderManager.Instance.GenerateOrderInstanceID(this.OrderID);
            //���
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


