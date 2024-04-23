using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.Order.OrderManager;

namespace ProjectOC.Order
{
    [Serializable]
    public class OrderSpecial : Order
    {
        public OrderSpecial(string orderId, List<OrderMap> RequireItemList) : base(orderId, RequireItemList)
        {
        }
    }


}


