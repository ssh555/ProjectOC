using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Sirenix.OdinInspector;
namespace ML.Engine.Timer
{
    public sealed class CounterDownTimerManager : Manager.GlobalManager.IGlobalManager
    {
        /// <summary>
        /// Update 计时器
        /// </summary>
        [ShowInInspector]
        private List<CounterDownTimer> updateCounterDownTimers = new List<CounterDownTimer>();
        /// <summary>
        /// FixedUpdate 计时器
        /// </summary>
        [ShowInInspector]
        private List<CounterDownTimer> fixedCounterDownTimers = new List<CounterDownTimer>();
        /// <summary>
        /// 不受影响的真实时间计时器
        /// </summary>
        [ShowInInspector]
        private List<CounterDownTimer> realCounterDownTimers = new List<CounterDownTimer>();
        /// <summary>
        /// 销毁列表
        /// </summary>
        [ShowInInspector]
        private List<CounterDownTimer> destroyCounterDownTimers = new List<CounterDownTimer>();

        public float TimeScale = 1;

        public CounterDownTimerManager()
        {
            this._lastRealTime = Time.realtimeSinceStartup;
        }

        public void Update(float deltaTime)
        {
            deltaTime *= TimeScale;
            foreach (var timer in this.updateCounterDownTimers)
            {
                
                timer.UpdateCurrentTime(deltaTime);
            }
            
        }

        public void FixedUpdate(float deltaTime)
        {
            deltaTime *= TimeScale;
            foreach (var timer in fixedCounterDownTimers)
            {
                timer.UpdateCurrentTime(deltaTime);
            }
        }

        private float _lastRealTime = 0;
        public void LateUpdate(float deltaTime)
        {
            foreach (var timer in realCounterDownTimers)
            {
                timer.UpdateCurrentTime(Time.realtimeSinceStartup - this._lastRealTime);
                this._lastRealTime = Time.realtimeSinceStartup;
            }

            deltaTime *= TimeScale;
            foreach (var item in destroyCounterDownTimers)
            {
                if (fixedCounterDownTimers.Contains(item))
                {
                    fixedCounterDownTimers.Remove(item);
                }
                else if (this.updateCounterDownTimers.Contains(item))
                {
                    this.updateCounterDownTimers.Remove(item);
                }
                else if (this.realCounterDownTimers.Contains(item))
                {
                    //Debug.Log("destroyrealCounterDownTimers " + item + " " + Time.frameCount);
                    this.realCounterDownTimers.Remove(item);
                }
            }
            /*if(destroyCounterDownTimers.Count>0)
                Debug.Log("realCounterDownTimers " + realCounterDownTimers.Count +" "+ Time.frameCount);*/
            destroyCounterDownTimers.Clear();
        }

        /// <summary>
        /// tType -> 0 Fixed, 1 Update, 2 Real
        /// </summary>
        /// <param name="countDownTimer"></param>
        /// <param name="tType"></param>
        public void AddTimer(CounterDownTimer countDownTimer, int tType)
        {
            /*if(tType == 2)
                Debug.Log("AddTimer " + countDownTimer + " " + Time.frameCount);*/
            if (this.destroyCounterDownTimers.Contains(countDownTimer))
            {
                destroyCounterDownTimers.Remove(countDownTimer);
            }
            if (tType == 0)
            {
                if(!this.fixedCounterDownTimers.Contains(countDownTimer))
                {
                    this.fixedCounterDownTimers.Add(countDownTimer);
                }
            }
            else if(tType == 1)
            {
                if (!this.updateCounterDownTimers.Contains(countDownTimer))
                {
                    this.updateCounterDownTimers.Add(countDownTimer);
                }
            }
            else if(tType == 2)
            {
                
                if (!this.realCounterDownTimers.Contains(countDownTimer))
                {
                    //Debug.Log("AddrealCounterDownTimers " + countDownTimer + " " + Time.frameCount);
                    this.realCounterDownTimers.Add(countDownTimer);
                }
            }
        }

        public bool RemoveTimer(CounterDownTimer countDownTimer)
        {
            
            if (fixedCounterDownTimers.Contains(countDownTimer) || this.updateCounterDownTimers.Contains(countDownTimer) || this.realCounterDownTimers.Contains(countDownTimer))
            {
                this.destroyCounterDownTimers.Add(countDownTimer);
                //Debug.Log("RemoveTimer true " + countDownTimer + " " + Time.frameCount);
                return true;
            }
            //Debug.Log("RemoveTimer false " + countDownTimer + " " + Time.frameCount);
            return false;
        }
    }
}