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
        /// ��ȡʱ�޼�ʱ�� ��λΪ��ʵʱ��min
        /// </summary>
        private CounterDownTimer ReceiveDDLTimer;

        /// <summary>
        /// ����ʱ�޼�ʱ�� ��λΪ��Ϸ�ڵ���
        /// </summary>
        private CounterDownTimer DeliverDDLTimer;
        public OrderUrgent(string orderId, List<OrderMap> RequireItemList, int ReceiveDDL, int DeliverDDL) : base(orderId, RequireItemList)
        {
            this.ReceiveDDLTimer = new CounterDownTimer(ReceiveDDL * 60, autocycle: false, autoStart: false);
            this.ReceiveDDLTimer.OnEndEvent += () =>
            {
                //���ü�ʱ��
                this.ReceiveDDLTimer.Reset(ReceiveDDL * 60, isStoped: true);
                //���þܾ���������
                LocalGameManager.Instance.OrderManager.RefuseOrder(orderId);
            };

            this.DeliverDDLTimer = new CounterDownTimer(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, autocycle: false, autoStart: false);
            this.DeliverDDLTimer.OnEndEvent += () =>
            {
                //���ü�ʱ��
                this.DeliverDDLTimer.Reset(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, isStoped: true);
                //����ȡ����������
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


