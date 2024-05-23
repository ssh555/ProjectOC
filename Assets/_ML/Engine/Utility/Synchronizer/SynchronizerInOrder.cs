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
                // 检查是否按照顺序进行
                if (order == curCheckNum)
                {
                    // 执行对应顺序的操作
                    CheckActions[order]?.Invoke();

                    // 更新当前检查的序号
                    curCheckNum++;

                    // 如果所有操作完成，则触发 OnAllFinish
                    if (curCheckNum == CheckNum && !isTrigger)
                    {
                        OnAllFinish?.Invoke();
                        isTrigger = true;
                    }
                }
                else
                {
                    // 如果不是按照顺序进行，则抛出异常或者进行其他处理
                    throw new Exception("Operation not in order!");
                }
            }
        }

        private Action OnAllFinish;
    }
}


