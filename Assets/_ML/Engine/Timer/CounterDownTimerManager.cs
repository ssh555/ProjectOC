using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ML.Engine.Timer
{
    public sealed class CounterDownTimerManager : Manager.GlobalManager.IGlobalManager
    {
        /// <summary>
        /// Update 计时器
        /// </summary>
        private List<CounterDownTimer> updateCounterDownTimers = new List<CounterDownTimer>();
        /// <summary>
        /// FixedUpdate 计时器
        /// </summary>
        private List<CounterDownTimer> fixedCounterDownTimers = new List<CounterDownTimer>();
        /// <summary>
        /// 销毁列表
        /// </summary>
        private List<CounterDownTimer> destroyCounterDownTimers = new List<CounterDownTimer>();

        public float TimeScale = 1;

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

        public void LateUpdate(float deltaTime)
        {
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
            }
            destroyCounterDownTimers.Clear();
        }

        public void AddTimer(CounterDownTimer countDownTimer, bool isFixed)
        {
            if (isFixed)
            {
                if(!this.fixedCounterDownTimers.Contains(countDownTimer))
                    this.fixedCounterDownTimers.Add(countDownTimer);
            }
            else
            {
                if (!this.updateCounterDownTimers.Contains(countDownTimer))
                    this.updateCounterDownTimers.Add(countDownTimer);
            }
        }

        public bool RemoveTimer(CounterDownTimer countDownTimer)
        {
            if (fixedCounterDownTimers.Contains(countDownTimer) || this.updateCounterDownTimers.Contains(countDownTimer))
            {
                this.destroyCounterDownTimers.Add(countDownTimer);
                return true;
            }
            return false;
        }
    }
}