using ML.Engine.Timer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ManagerNS
{
    /// <summary>
    /// 调度的时间管理器
    /// </summary>
    [System.Serializable]
    public sealed class DispatchTimeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        /// <summary>
        /// 时间流速比例，现实 timeScale秒 等于游戏内 1h
        /// </summary>
        private float timeScale = 60;
        /// <summary>
        /// 时间流速比例，现实timeScale s等于游戏1h
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
        private int currentTimeFrame = 0;
        /// <summary>
        /// 当前所处时段 [0, 23]
        /// </summary>
        public int CurrentTimeFrame 
        { 
            get { return currentTimeFrame; } 
            private set {currentTimeFrame = value;} 
        }
        /// <summary>
        /// 计时器 => 时段更新(TimeScale确定) 一天循环24次，一个时段计时一次，计时结束时调用时段更新事件，Loop
        /// </summary>
        private CounterDownTimer Timer;
        /// <summary>
        /// 时段更新事件
        /// </summary>
        public event Action<int> OnTimeFrameChanged;
        public void Init()
        {
            this.Timer = new CounterDownTimer(this.TimeScale, true, false);
            this.Timer.OnEndEvent += EndActionForTimer;
            this.Timer.Start();
        }
        private void EndActionForTimer()
        {
            this.CurrentTimeFrame = (this.CurrentTimeFrame + 1) % 24;
            this.OnTimeFrameChanged?.Invoke(this.CurrentTimeFrame);
        }
    }
}

