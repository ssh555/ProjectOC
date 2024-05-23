using ProjectOC.Order;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Utility
{
    public class SynchronizerInOrder
    {
        private int CheckNum;
        private int curCheckNum;
        private Dictionary<int, Action> CheckActions = new Dictionary<int, Action>();
        private System.Object lockObject = new System.Object();

        private bool isTrigger;

        public SynchronizerInOrder(int checkNum, Action OnAllFinish = null)
        {
            this.curCheckNum = 0;
            this.CheckNum = checkNum;
            this.OnAllFinish = OnAllFinish;
            this.isTrigger = false;
        }

        public void AddCheckAction(int order, Action action)
        {
            lock (lockObject)
            {
                // ��ӵ��ֵ���
                CheckActions.Add(order, action);
            }
        }

        public void Check(int order)
        {
            lock (lockObject)
            {
                Debug.Log("Check " + order + " ִ�����");
                // ����Ƿ���˳�����
                if(CheckActions.ContainsKey(++order))
                {
                    // ִ�ж�Ӧ˳��Ĳ���
                    CheckActions[order].Invoke();

                    // ���µ�ǰ�������
                    curCheckNum = order;

                    // ������в�����ɣ��򴥷� OnAllFinish
                    if (curCheckNum == CheckNum && !isTrigger)
                    {
                        OnAllFinish?.Invoke();
                        isTrigger = true;
                    }
                }
                else
                {
                    Debug.LogError("�����ڴ�ִ�з���˳��"+ order);
                }
            }
        }

        public void StartExecution()
        {
            if (CheckActions.ContainsKey(0))
            {
                CheckActions[0].Invoke();
            }
        }

        private Action OnAllFinish;
    }
}


