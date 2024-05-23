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
                // ����Ƿ���˳�����
                if (order == curCheckNum)
                {
                    // ִ�ж�Ӧ˳��Ĳ���
                    CheckActions[order]?.Invoke();

                    // ���µ�ǰ�������
                    curCheckNum++;

                    // ������в�����ɣ��򴥷� OnAllFinish
                    if (curCheckNum == CheckNum && !isTrigger)
                    {
                        OnAllFinish?.Invoke();
                        isTrigger = true;
                    }
                }
                else
                {
                    // ������ǰ���˳����У����׳��쳣���߽�����������
                    throw new Exception("Operation not in order!");
                }
            }
        }

        private Action OnAllFinish;
    }
}


