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
        /// ��ȡʱ�޼�ʱ�� ��λΪ��ʵʱ��min
        /// </summary>
        private CounterDownTimer receiveDDLTimer;
        public CounterDownTimer ReceiveDDLTimer {  get { return receiveDDLTimer; } }

        /// <summary>
        /// ����ʱ�޼�ʱ�� ��λΪ��Ϸ�ڵ���
        /// </summary>
        private CounterDownTimer DeliverDDLTimer;

        private int ReceiveDDL, DeliverDDL;

        public OrderUrgent(OrderTableData orderTableData) : base(orderTableData)
        {
            this.ReceiveDDL = orderTableData.ReceiveDDL;
            this.DeliverDDL = orderTableData.DeliverDDL;
            this.receiveDDLTimer = new CounterDownTimer(ReceiveDDL * 60, autocycle: false, autoStart: false);
            this.receiveDDLTimer.OnEndEvent += () =>
            {
                //���ü�ʱ��
                this.receiveDDLTimer.Reset(ReceiveDDL * 60, isStoped: true);
                //���þܾ���������
                LocalGameManager.Instance.OrderManager.RefuseOrder(this.OrderID);
            };

            this.DeliverDDLTimer = new CounterDownTimer(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, autocycle: false, autoStart: false);
            this.DeliverDDLTimer.OnEndEvent += () =>
            {
                //���ü�ʱ��
                this.DeliverDDLTimer.Reset(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, isStoped: true);
                //����ȡ����������
                LocalGameManager.Instance.OrderManager.CancleOrder(this.OrderID);
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


