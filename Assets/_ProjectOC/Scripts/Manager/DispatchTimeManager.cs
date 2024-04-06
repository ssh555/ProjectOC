using ML.Engine.Timer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ManagerNS
{
    /// <summary>
    /// 时间管理器
    /// </summary>
    [System.Serializable]
    public sealed class DispatchTimeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        private float timeScale = 1;
        /// <summary>
        /// 时间流速比例，现实TimeScale秒等于游戏内1分钟。
        /// </summary>
        public float TimeScale 
        { 
            get { return timeScale; }
            set
            {
                if (value > 0)
                {
                    this.timeScale = value;
                    this.Timer.Reset(timeScale);
                }
            }
        }
        /// <summary>
        /// 当前所处时段 [0, 23]
        /// </summary>
        public int CurrentTimeFrame = 0;
        /// <summary>
        /// 当前所处分钟 [0, 59]
        /// </summary>
        public int CurrentMinute = 0;
        /// <summary>
        /// 当前日期，从0开始
        /// </summary>
        public int CurrentDay = 0;
        /// <summary>
        /// 计时器，时间为TimeScale秒，循环计时。每次计时结束后更新时段，并调用时段更新事件
        /// </summary>
        private CounterDownTimer Timer;
        /// <summary>
        /// 时段更新事件，时段切换时调用，参数为当前新的时段
        /// </summary>
        public event Action<int> OnTimeFrameChanged;
        /// <summary>
        /// 日期更新事件，日期切换时调用，参数为当前新的日期
        /// </summary>
        public event Action<int> OnDayChanged;


        public void Init()
        {
            this.Timer = new CounterDownTimer(TimeScale, true, false);
            this.Timer.OnEndEvent += EndActionForTimer;
            this.Timer.Start();
        }
        private void EndActionForTimer()
        {
            this.CurrentMinute = (this.CurrentMinute + 1) % 60;
            if (this.CurrentMinute == 0)
            {
                this.CurrentTimeFrame = (this.CurrentTimeFrame + 1) % 24;
                this.OnTimeFrameChanged?.Invoke(this.CurrentTimeFrame);
                if (this.CurrentTimeFrame == 0)
                {
                    this.CurrentDay += 1;
                    this.OnDayChanged?.Invoke(this.CurrentDay);
                }
            }
        }
    }
}

