using ML.Engine.Timer;
using Sirenix.OdinInspector;
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
        /// <summary>
        /// 时间流速比例
        /// </summary>
        private float timeScale = 0.1f;
        [LabelText("现实多少秒等于游戏内1分钟"), FoldoutGroup("配置"), ShowInInspector]
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
        [LabelText("当前分钟 [0, 59]"), ReadOnly, ShowInInspector]
        public int CurrentMinute = 0;
        [LabelText("当前时段 [0, 23]"), ReadOnly, ShowInInspector]
        public int CurrentHour = 0;
        [LabelText("当前日期 [0,]"), ReadOnly, ShowInInspector]
        public int CurrentDay = 0;
        /// <summary>
        /// 计时器，时间为TimeScale秒，循环计时。每次计时结束后更新时段，并调用时段更新事件
        /// </summary>
        private CounterDownTimer Timer;
        /// <summary>
        /// 时段更新事件，时段切换时调用，参数为当前新的时段
        /// </summary>
        public event Action<int> OnHourChangedAction;
        /// <summary>
        /// 日期更新事件，日期切换时调用，参数为当前新的日期
        /// </summary>
        public event Action<int> OnDayChangedAction;

        public void OnRegister()
        {
            this.Timer = new CounterDownTimer(TimeScale, true, false);
            this.Timer.OnEndEvent += EndActionForTimer;
            this.Timer.Start();
        }
        public void OnUnregister()
        {
            this.Timer?.End();
        }

        private void EndActionForTimer()
        {
            this.CurrentMinute = (this.CurrentMinute + 1) % 60;
            if (this.CurrentMinute == 0)
            {
                this.CurrentHour = (this.CurrentHour + 1) % 24;
                this.OnHourChangedAction?.Invoke(this.CurrentHour);
                if (this.CurrentHour == 0)
                {
                    this.CurrentDay += 1;
                    this.OnDayChangedAction?.Invoke(this.CurrentDay);
                }
            }
        }
    }
}

