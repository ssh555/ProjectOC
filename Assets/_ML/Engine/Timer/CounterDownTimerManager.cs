using System.Collections;
using System.Collections.Generic;
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
            for (int i = 0; i < this.updateCounterDownTimers.Count; i++) 
            {
                this.updateCounterDownTimers[i].UpdateCurrentTime(deltaTime);
            }
        }

        public void FixedUpdate(float deltaTime)
        {
            deltaTime *= TimeScale;
            for (int i = 0; i < this.fixedCounterDownTimers.Count; i++) 
            {
                this.fixedCounterDownTimers[i].UpdateCurrentTime(deltaTime);
            }
        }

        private float _lastRealTime = 0;
        public void LateUpdate(float deltaTime)
        {
            for (int i = 0; i < this.realCounterDownTimers.Count; i++) 
            {
                this.realCounterDownTimers[i].UpdateCurrentTime(Time.realtimeSinceStartup - this._lastRealTime);
                this._lastRealTime = Time.realtimeSinceStartup;
            }

            deltaTime *= TimeScale;


            lock (destroyCounterDownTimers as System.Object)
            {
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
                        this.realCounterDownTimers.Remove(item);
                    }
                }
                destroyCounterDownTimers.Clear();
            }
            
            

            
        }

        /// <summary>
        /// tType -> 0 Fixed, 1 Update, 2 Real
        /// </summary>
        /// <param name="countDownTimer"></param>
        /// <param name="tType"></param>
        public void AddTimer(CounterDownTimer countDownTimer, int tType)
        {
            lock(destroyCounterDownTimers)
            {
                if (this.destroyCounterDownTimers.Contains(countDownTimer))
                {
                    destroyCounterDownTimers.Remove(countDownTimer);
                }
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
                    this.realCounterDownTimers.Add(countDownTimer);
                }
            }
        }

        public bool RemoveTimer(CounterDownTimer countDownTimer)
        {
            if (fixedCounterDownTimers.Contains(countDownTimer) || this.updateCounterDownTimers.Contains(countDownTimer) || this.realCounterDownTimers.Contains(countDownTimer))
            {
                lock(destroyCounterDownTimers as System.Object)
                {
                    this.destroyCounterDownTimers.Add(countDownTimer);
                    return true;
                }
            }
            return false;
        }


        public void RemoveAllTimer()
        {
            foreach (var timer in updateCounterDownTimers)
            {
                this.destroyCounterDownTimers.Add(timer);
            }
            foreach (var timer in fixedCounterDownTimers)
            {
                this.destroyCounterDownTimers.Add(timer);
            }
            foreach (var timer in realCounterDownTimers)
            {
                this.destroyCounterDownTimers.Add(timer);
            }
        }

    }
}