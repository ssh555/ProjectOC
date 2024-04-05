using ML.Engine.Timer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ManagerNS
{
    /// <summary>
    /// ʱ�������
    /// </summary>
    [System.Serializable]
    public sealed class DispatchTimeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        private float timeScale = 1;
        /// <summary>
        /// ʱ�����ٱ�������ʵTimeScale�������Ϸ��1���ӡ�
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
        /// ��ǰ����ʱ�� [0, 23]
        /// </summary>
        public int CurrentTimeFrame = 0;
        /// <summary>
        /// ��ǰ�������� [0, 59]
        /// </summary>
        public int CurrentMinute = 0;
        /// <summary>
        /// ��ǰ���ڣ���0��ʼ
        /// </summary>
        public int CurrentDay = 0;
        /// <summary>
        /// ��ʱ����ʱ��ΪTimeScale�룬ѭ����ʱ��ÿ�μ�ʱ���������ʱ�Σ�������ʱ�θ����¼�
        /// </summary>
        private CounterDownTimer Timer;
        /// <summary>
        /// ʱ�θ����¼���ʱ���л�ʱ���ã�����Ϊ��ǰ�µ�ʱ��
        /// </summary>
        public event Action<int> OnTimeFrameChanged;
        /// <summary>
        /// ���ڸ����¼��������л�ʱ���ã�����Ϊ��ǰ�µ�����
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

