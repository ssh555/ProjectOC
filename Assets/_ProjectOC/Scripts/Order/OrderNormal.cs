using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.Order.OrderManager;

namespace ProjectOC.Order
{
    public class OrderNormal : Order
    {
        /// <summary>
        /// ˢ�¼�ʱ�� ��λΪ��Ϸʱ����
        /// </summary>
        private CounterDownTimer RefreshTimer;

        public OrderNormal(string orderId, List<OrderMap> RequireItemList, int DemandCycle) : base(orderId, RequireItemList)
        {
            
            this.RefreshTimer = new CounterDownTimer(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DemandCycle, autocycle: false, autoStart: false);
            this.RefreshTimer.OnEndEvent += () =>
            {
                //���ü�ʱ��
                this.RefreshTimer.Reset(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DemandCycle, isStoped: true);

                //TODO ��¼��ǰ��Ϸ�գ��ڵ���00��00���볣���б��� AddOrderToOrderDelegationMap

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


