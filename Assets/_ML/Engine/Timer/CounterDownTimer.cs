using ML.Engine.Manager.GlobalManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Timer
{
    /// <summary>
    /// 倒计时器
    /// </summary>
    public sealed class CounterDownTimer
    {
        private static CounterDownTimerManager CDMInstance => Manager.GameManager.Instance.CounterDownTimerManager;

        /// <summary>
        /// 构造倒计时器
        /// tType -> 0 Fixed, 1 Update, 2 Real
        /// </summary>
        /// <param name="duration">起始时间</param>
        /// <param name="autocycle">是否自动循环</param>
        public CounterDownTimer(float duration, bool autocycle = false, bool autoStart = true, double speed = 1f, int tType = 0)
        {
            Duration = Mathf.Max(0f, duration);
            TimerType = tType;
            IsAutoCycle = autocycle;
            Speed = speed;
            Reset(duration, !autoStart);
        }


        ~CounterDownTimer()
        {
            CDMInstance.RemoveTimer(this);
        }


        /// <summary>
        /// 是否自动循环（小于等于0后重置）
        /// </summary>
        public bool IsAutoCycle { get; private set; }

        private bool _isStoped = false;
        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsStoped { get => this._isStoped; 
            private set
            {
                this._isStoped = value;
                // 启动
                if (!this._isStoped)
                {
                    CDMInstance.AddTimer(this, this.TimerType);
                }
                // 暂停
                else
                {
                    CDMInstance.RemoveTimer(this);
                }
            }
        }

        /// <summary>
        /// 正计时时间
        /// </summary>
        public double Time { get { return this.Duration - this.currentTime; } }
        /// <summary>
        /// 当前倒计时时间
        /// </summary>
        public double CurrentTime { get { return currentTime; } }

        /// <summary>
        /// 是否时间到
        /// </summary>
        public bool IsTimeUp { get { return CurrentTime <= 0; } }

        /// <summary>
        /// 计时时间长度
        /// </summary>
        public double Duration { get; private set; }

        /// <summary>
        /// 当前计时器剩余时间
        /// </summary>
        private double currentTime;

        /// <summary>
        /// 倒计时运行事件，参数为剩余时间
        /// </summary>
        public Action<double> OnUpdateEvent;

        /// <summary>
        /// 倒计时结束时运行事件
        /// </summary>
        public event Action OnEndEvent;

        private bool IsEnd = false;

        /// <summary>
        /// 计时器速率
        /// </summary>
        public double Speed { get; private set; }
        /// <summary>
        /// 计时器更新是fixedupdate还是update
        /// </summary>
        public int TimerType { get; private set; }

        /// <summary>
        /// 更新计时器时间
        /// </summary>
        /// <returns>返回剩余时间</returns>
        public double UpdateCurrentTime(float deltaTime)
        {
            if (IsStoped)
                return currentTime;

            //结束
            if (Math.Abs(currentTime - 0) < 1e-07)                                       
            // 小于等于0直接返回，如果循环那就重置时间
            {
                if (IsEnd)
                {
                    return currentTime;
                }
                IsEnd = true;
                if (IsAutoCycle)
                {
                    Reset(Duration, false);
                }
                else
                {
                    this.IsStoped = true;
                }
                currentTime = 0;
                this.OnUpdateEvent?.Invoke(currentTime);
                this.OnEndEvent?.Invoke();
                return currentTime;
            }
            //倒计时
            double countTime = deltaTime * Speed;
            currentTime -= countTime;
            currentTime = Math.Max(currentTime, 0);
            if (Math.Abs(currentTime - 0) < 1e-07)
            {
                currentTime = 0;
            }

            this.OnUpdateEvent?.Invoke(currentTime);
            return currentTime;
        }
        
        /// <summary>
        /// 开始计时，取消暂停状态
        /// </summary>
        public void Start()
        {
            Reset(Duration, false);
        }

        /// <summary>
        /// 重置计时器
        /// </summary>
        /// <param name="duration">持续时间</param>
        /// <param name="isStoped">是否暂停</param>
        public void Reset(double duration, bool isStoped = false)
        {
            IsEnd = false;
            Duration = Math.Max(0f, duration);
            currentTime = Duration;
            IsStoped = isStoped;

        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            UpdateCurrentTime(0);    // 暂停前先更新一遍
            IsStoped = true;
        }

        /// <summary>
        /// 继续（取消暂停）
        /// </summary>
        public void Continue()
        {
            IsStoped = false;
        }

        /// <summary>
        /// 终止，暂停且设置当前值为0
        /// </summary>
        public void End()
        {
            //Dispose();
            IsStoped = true;
            currentTime = 0f;
        }

        /// <summary>
        /// 获取倒计时完成率（0为没开始计时，1为计时结束）
        /// </summary>
        /// <returns></returns>
        public double GetPercent()
        {
            if (currentTime <= 0 || Duration <= 0)
                return 1f;
            return 1f - currentTime / Duration;
        }
        
        /// <summary>
        /// 设置当前计时器剩余时间
        /// </summary>
        /// <param name="time"></param>
        public void SetCurrentTime(double time)
        {
            double setTime = Math.Max(time, this.Duration);
            this.currentTime = setTime;
        }
        
        /// <summary>
        /// 设置计时器更新速率，乘法
        /// </summary>
        /// <param name="speed"></param>
        public void SetUpdateSpeed(float speed)
        {
            this.Speed *= speed;
        }
        
        public void ResetUpdateSpeed(float speed)
        {
            this.Speed = speed;
        }


        public string ConvertToMinAndSec()
        {
            int min = (int)(this.currentTime) / 60;
            int sec = (int)(this.currentTime) - min * 60;
            return min.ToString() + "min" + sec.ToString() + "s";
        }
        
    }
}
