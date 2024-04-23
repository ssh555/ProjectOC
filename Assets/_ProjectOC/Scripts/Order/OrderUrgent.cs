using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.Order.OrderManager;

namespace ProjectOC.Order
{
    [Serializable]
    public class OrderUrgent : Order
    {
        /// <summary>
        /// 接取时限计时器 单位为现实时间min
        /// </summary>
        private CounterDownTimer receiveDDLTimer;
        public CounterDownTimer ReceiveDDLTimer {  get { return receiveDDLTimer; } }

        /// <summary>
        /// 交付时限计时器 单位为游戏内的日
        /// </summary>
        private CounterDownTimer DeliverDDLTimer;
        public OrderUrgent(string orderId, List<OrderMap> RequireItemList, int ReceiveDDL, int DeliverDDL) : base(orderId, RequireItemList)
        {
            this.receiveDDLTimer = new CounterDownTimer(ReceiveDDL * 60, autocycle: false, autoStart: false);
            this.receiveDDLTimer.OnEndEvent += () =>
            {
                //重置计时器
                this.receiveDDLTimer.Reset(ReceiveDDL * 60, isStoped: true);
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
            this.receiveDDLTimer.Start();
        }

        public void StartDeliverDDLTimer()
        {
            this.DeliverDDLTimer.Start();
        }

        public void Reset()
        {
            this.receiveDDLTimer.Reset(this.receiveDDLTimer.Duration, isStoped: true);
            this.DeliverDDLTimer.Reset(this.DeliverDDLTimer.Duration, isStoped: true);
        }
    }


}


