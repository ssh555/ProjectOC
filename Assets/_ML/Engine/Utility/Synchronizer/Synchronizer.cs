using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Utility
{
    public class Synchronizer
    {
        private int CheckNum;
        private int curCheckNum;
        private Action OnAllFinish;
        private System.Object lockObject = new System.Object();

        private bool isTrigger;
        public Synchronizer(int checkNum, Action OnAllFinish)
        {
            //Debug.Log("checkNum " + checkNum);
            this.curCheckNum = 0;
            this.CheckNum = checkNum;
            this.OnAllFinish = OnAllFinish;
            this.isTrigger = false;
        }

        public void Check()
        {
            lock (lockObject)
            {
                ++curCheckNum;
                //Debug.Log("Check " + curCheckNum);
                if (isTrigger == false && curCheckNum == CheckNum)
                {
                    OnAllFinish?.Invoke();
                    isTrigger = true;
                }
            }
        }
    }
}


