using ML.Engine.Timer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ManagerNS
{
    /// <summary>
    /// ���ȵ�ʱ�������
    /// </summary>
    [System.Serializable]
    public sealed class DispatchTimeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        /// <summary>
        /// ʱ�����ٱ�������ʵ timeScale�� ������Ϸ�� 1h
        /// </summary>
        private float timeScale = 60;
        /// <summary>
        /// ʱ�����ٱ�������ʵtimeScale s������Ϸ1h
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
        /// ��ǰ����ʱ�� [0, 23]
        /// </summary>
        public int CurrentTimeFrame 
        { 
            get { return currentTimeFrame; } 
            private set {currentTimeFrame = value;} 
        }
        /// <summary>
        /// ��ʱ�� => ʱ�θ���(TimeScaleȷ��) һ��ѭ��24�Σ�һ��ʱ�μ�ʱһ�Σ���ʱ����ʱ����ʱ�θ����¼���Loop
        /// </summary>
        private CounterDownTimer Timer;
        /// <summary>
        /// ʱ�θ����¼�
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

