using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.Order.OrderManager;

namespace ProjectOC.Order
{
    public class OrderUrgent : Order
    {
        /// <summary>
        /// 接取时限计时器 单位为现实时间min
        /// </summary>
        private CounterDownTimer ReceiveDDLTimer;

        /// <summary>
        /// 交付时限计时器 单位为游戏内的日
        /// </summary>
        private CounterDownTimer DeliverDDLTimer;
        public OrderUrgent(string orderId, List<OrderMap> RequireItemList, int ReceiveDDL, int DeliverDDL) : base(orderId, RequireItemList)
        {
            this.ReceiveDDLTimer = new CounterDownTimer(ReceiveDDL * 60, autocycle: false, autoStart: false);
            this.ReceiveDDLTimer.OnEndEvent += () =>
            {
                //重置计时器
                this.ReceiveDDLTimer.Reset(ReceiveDDL * 60, isStoped: true);
                //调用拒绝订单函数
                LocalGameManager.Instance.OrderManager.RefuseOrder(orderId);
            };

            this.DeliverDDLTimer = new CounterDownTimer(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, autocycle: false, autoStart: false);
            this.DeliverDDLTimer.OnEndEvent += () =>
            {
                //重置计时器
                this.DeliverDDLTimer.Reset(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, isStoped: true);
                //调用取消订单函数
                LocalGameManager.Instance.OrderManager.CancleOrder(orderId);
            };

        }

        public void StartReceiveDDLTimer()
        {
            this.ReceiveDDLTimer.Start();
        }

        public void StartDeliverDDLTimer()
        {
            this.DeliverDDLTimer.Start();
        }
    }


}


