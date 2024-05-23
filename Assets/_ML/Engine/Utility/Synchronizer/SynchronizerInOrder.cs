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
                // 添加到字典中
                CheckActions.Add(order, action);
            }
        }

        public void Check(int order)
        {
            lock (lockObject)
            {
                // 更新当前检查的序号
                curCheckNum++;

                // 如果所有操作完成，则触发 OnAllFinish
                if (curCheckNum == CheckNum && !isTrigger)
                {
                    OnAllFinish?.Invoke();
                    isTrigger = true;
                    return;
                }
                // 检查是否按照顺序进行
                if(CheckActions.ContainsKey(++order))
                {
                    // 执行对应顺序的操作
                    CheckActions[order].Invoke();
                }
                else
                {
                    Debug.LogError("不存在待执行函数顺序 " + order);
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


