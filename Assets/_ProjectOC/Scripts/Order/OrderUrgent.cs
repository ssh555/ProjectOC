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
        private CounterDownTimer deliverDDLTimer;
        public CounterDownTimer DeliverDDLTimer { get { return deliverDDLTimer; } }

        public int ReceiveDDL, DeliverDDL;

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
                LocalGameManager.Instance.OrderManager.RefuseOrder(this.OrderInstanceID);
            };

            //this.deliverDDLTimer = new CounterDownTimer(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, autocycle: false, autoStart: false);
            this.deliverDDLTimer = new CounterDownTimer(10 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, autocycle: false, autoStart: false);
            this.deliverDDLTimer.OnEndEvent += () =>
            {
                //���ü�ʱ��
                //this.deliverDDLTimer.Reset(1440 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, isStoped: true);
                this.deliverDDLTimer.Reset(10 * LocalGameManager.Instance.DispatchTimeManager.TimeScale * DeliverDDL, isStoped: true);
                //����ȡ����������
                LocalGameManager.Instance.OrderManager.CancleOrder(this.OrderInstanceID);
            };

        }

        public void StartReceiveDDLTimer()
        {
            this.receiveDDLTimer.Start();
        }

        public void StartDeliverDDLTimer()
        {
            this.deliverDDLTimer.Start();
        }

        public void Reset()
        {
            this.receiveDDLTimer.Reset(this.receiveDDLTimer.Duration, isStoped: true);
            this.deliverDDLTimer.Reset(this.deliverDDLTimer.Duration, isStoped: true);
        }

        //��ȡ��ǰ����ʱ�޻��������
        public int GetDeliverDDLTimerRemainGameDays()
        {
            return Mathf.CeilToInt((float)this.deliverDDLTimer.CurrentTime / LocalGameManager.Instance.DispatchTimeManager.TimeScale / 1440);
        }

        //��ȡ��ǰ����ʱ�޻������Сʱ���ٷ�
        public string GetDeliverDDLTimerRemainGameHourAndMin()
        {
            int hour = (int)((this.deliverDDLTimer.CurrentTime / LocalGameManager.Instance.DispatchTimeManager.TimeScale) / 60);
            int min = (int)((this.deliverDDLTimer.CurrentTime - hour * 60 * LocalGameManager.Instance.DispatchTimeManager.TimeScale) / LocalGameManager.Instance.DispatchTimeManager.TimeScale);
            return hour.ToString()+":"+ min.ToString();
        }
    }


}


