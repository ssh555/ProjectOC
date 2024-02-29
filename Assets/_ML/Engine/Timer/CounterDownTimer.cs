using ML.Engine.Manager.GlobalManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Timer
{
    /// <summary>
    /// ����ʱ��
    /// </summary>
    public sealed class CounterDownTimer
    {
        private static CounterDownTimerManager CDMInstance => Manager.GameManager.Instance.CounterDownTimerManager;

        /// <summary>
        /// ���쵹��ʱ��
        /// tType -> 0 Fixed, 1 Update, 2 Real
        /// </summary>
        /// <param name="duration">��ʼʱ��</param>
        /// <param name="autocycle">�Ƿ��Զ�ѭ��</param>
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
        /// �Ƿ��Զ�ѭ����С�ڵ���0�����ã�
        /// </summary>
        public bool IsAutoCycle { get; private set; }

        private bool _isStoped = false;
        /// <summary>
        /// �Ƿ���ͣ
        /// </summary>
        public bool IsStoped { get => this._isStoped; 
            private set
            {
                this._isStoped = value;
                // ����
                if (!this._isStoped)
                {
                    CDMInstance.AddTimer(this, this.TimerType);
                }
                // ��ͣ
                else
                {
                    CDMInstance.RemoveTimer(this);
                }
            }
        }

        /// <summary>
        /// ����ʱʱ��
        /// </summary>
        public double Time { get { return this.Duration - this.currentTime; } }
        /// <summary>
        /// ��ǰ����ʱʱ��
        /// </summary>
        public double CurrentTime { get { return currentTime; } }

        /// <summary>
        /// �Ƿ�ʱ�䵽
        /// </summary>
        public bool IsTimeUp { get { return CurrentTime <= 0; } }

        /// <summary>
        /// ��ʱʱ�䳤��
        /// </summary>
        public double Duration { get; private set; }

        /// <summary>
        /// ��ǰ��ʱ��ʣ��ʱ��
        /// </summary>
        private double currentTime;

        /// <summary>
        /// ����ʱ�����¼�������Ϊʣ��ʱ��
        /// </summary>
        public Action<double> OnUpdateEvent;

        /// <summary>
        /// ����ʱ����ʱ�����¼�
        /// </summary>
        public event Action OnEndEvent;

        private bool IsEnd = false;

        /// <summary>
        /// ��ʱ������
        /// </summary>
        public double Speed { get; private set; }
        /// <summary>
        /// ��ʱ��������fixedupdate����update
        /// </summary>
        public int TimerType { get; private set; }

        /// <summary>
        /// ���¼�ʱ��ʱ��
        /// </summary>
        /// <returns>����ʣ��ʱ��</returns>
        public double UpdateCurrentTime(float deltaTime)
        {
            if (IsStoped)
                return currentTime;

            //����
            if (Math.Abs(currentTime - 0) < 1e-07)                                       
            // С�ڵ���0ֱ�ӷ��أ����ѭ���Ǿ�����ʱ��
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
            //����ʱ
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
        /// ��ʼ��ʱ��ȡ����ͣ״̬
        /// </summary>
        public void Start()
        {
            Reset(Duration, false);
        }

        /// <summary>
        /// ���ü�ʱ��
        /// </summary>
        /// <param name="duration">����ʱ��</param>
        /// <param name="isStoped">�Ƿ���ͣ</param>
        public void Reset(double duration, bool isStoped = false)
        {
            IsEnd = false;
            Duration = Math.Max(0f, duration);
            currentTime = Duration;
            IsStoped = isStoped;

        }

        /// <summary>
        /// ��ͣ
        /// </summary>
        public void Pause()
        {
            UpdateCurrentTime(0);    // ��ͣǰ�ȸ���һ��
            IsStoped = true;
        }

        /// <summary>
        /// ������ȡ����ͣ��
        /// </summary>
        public void Continue()
        {
            IsStoped = false;
        }

        /// <summary>
        /// ��ֹ����ͣ�����õ�ǰֵΪ0
        /// </summary>
        public void End()
        {
            //Dispose();
            IsStoped = true;
            currentTime = 0f;
        }

        /// <summary>
        /// ��ȡ����ʱ����ʣ�0Ϊû��ʼ��ʱ��1Ϊ��ʱ������
        /// </summary>
        /// <returns></returns>
        public double GetPercent()
        {
            if (currentTime <= 0 || Duration <= 0)
                return 1f;
            return 1f - currentTime / Duration;
        }
        
        /// <summary>
        /// ���õ�ǰ��ʱ��ʣ��ʱ��
        /// </summary>
        /// <param name="time"></param>
        public void SetCurrentTime(double time)
        {
            double setTime = Math.Max(time, this.Duration);
            this.currentTime = setTime;
        }
        
        /// <summary>
        /// ���ü�ʱ���������ʣ��˷�
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
