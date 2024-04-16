using ML.Engine.Timer;
using Sirenix.OdinInspector;
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
        /// <summary>
        /// ʱ�����ٱ���
        /// </summary>
        private float timeScale = 0.1f;
        [LabelText("��ʵ�����������Ϸ��1����"), FoldoutGroup("����"), ShowInInspector]
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
        [LabelText("��ǰ���� [0, 59]"), ReadOnly, ShowInInspector]
        public int CurrentMinute = 0;
        [LabelText("��ǰʱ�� [0, 23]"), ReadOnly, ShowInInspector]
        public int CurrentHour = 0;
        [LabelText("��ǰ���� [0,]"), ReadOnly, ShowInInspector]
        public int CurrentDay = 0;
        /// <summary>
        /// ��ʱ����ʱ��ΪTimeScale�룬ѭ����ʱ��ÿ�μ�ʱ���������ʱ�Σ�������ʱ�θ����¼�
        /// </summary>
        private CounterDownTimer Timer;
        /// <summary>
        /// ʱ�θ����¼���ʱ���л�ʱ���ã�����Ϊ��ǰ�µ�ʱ��
        /// </summary>
        public event Action<int> OnHourChangedAction;
        /// <summary>
        /// ���ڸ����¼��������л�ʱ���ã�����Ϊ��ǰ�µ�����
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

