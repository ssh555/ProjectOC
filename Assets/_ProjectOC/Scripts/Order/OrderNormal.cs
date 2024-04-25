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
    public class OrderNormal : Order
    {
        /// <summary>
        /// 刷新计时器 单位为游戏时间日
        /// </summary>
        private CounterDownTimer RefreshTimer;

        private int DemandCycle;

        public OrderNormal(OrderTableData orderTableData) :base(orderTableData)
        {
            this.DemandCycle = orderTableData.CD;
            //this.RefreshTimer = new CounterDownTimer(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DemandCycle, autocycle: false, autoStart: false);
            this.RefreshTimer = new CounterDownTimer(1 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DemandCycle, autocycle: false, autoStart: false);
            this.RefreshTimer.OnEndEvent += () =>
            {
                //重置计时器
                //this.RefreshTimer.Reset(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DemandCycle, isStoped: true);
                this.RefreshTimer.Reset(1 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DemandCycle, isStoped: true);

                //TODO 记录当前游戏日，于当日00：00加入常规列表中 AddOrderToOrderDelegationMap

                LocalGameManager.Instance.DispatchTimeManager.OnDayChangedAction += OnDayChangedAction;

            };
        }
        private void OnDayChangedAction(int day)
        {
            LocalGameManager.Instance.OrderManager.AddOrderToOrderDelegationMap(this.OrderID);
            LocalGameManager.Instance.DispatchTimeManager.OnDayChangedAction -= OnDayChangedAction;
        }
        public void StartRefreshTimer()
        {
            this.RefreshTimer.Start();
        }
    }


}


