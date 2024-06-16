using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace ProjectOC.ManagerNS
{
    [LabelText("ʱ�������"), System.Serializable]
    public sealed class DispatchTimeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        private float timeScale = 1f;
        [LabelText("��ʵ�����������Ϸ��1����"), ReadOnly, ShowInInspector]
        public float TimeScale
        {
            get { return timeScale; }
            set
            {
                if (value > 0)
                {
                    timeScale = value;
                    Timer?.Reset(timeScale);
                }
            }
        }
        [LabelText("��ǰ���� [0, 59]"), ShowInInspector]
        public int CurrentMinute = 0;
        [LabelText("��ǰʱ�� [0, 23]"),  ShowInInspector]
        public int CurrentHour = 0;
        [LabelText("��ǰ���� [0,]"), ShowInInspector]
        public int CurrentDay = 0;
        /// <summary>
        /// ��ʱ����ʱ��ΪTimeScale�룬ѭ����ʱ��ÿ�μ�ʱ���������ʱ�Σ�������ʱ�θ����¼�
        /// </summary>
        private ML.Engine.Timer.CounterDownTimer Timer;
        /// <summary>
        /// ʱ�θ����¼���ʱ���л�ʱ���ã�����Ϊ��ǰ�µ�ʱ��
        /// </summary>
        public event System.Action<int> OnHourChangedAction;
        /// <summary>
        /// ���ڸ����¼��������л�ʱ���ã�����Ϊ��ǰ�µ�����
        /// </summary>
        public event System.Action<int> OnDayChangedAction;

        public void OnRegister()
        {
            Timer = new ML.Engine.Timer.CounterDownTimer(TimeScale, true, false);
            Timer.OnEndEvent += EndActionForTimer;
            Timer.Start();
        }

        public void OnUnregister()
        {
            Timer?.End();
        }

        private void EndActionForTimer()
        {
            CurrentMinute = (CurrentMinute + 1) % 60;
            if (CurrentMinute == 0)
            {
                CurrentHour = (CurrentHour + 1) % 24;
                OnHourChangedAction?.Invoke(CurrentHour);
                if (CurrentHour == 0)
                {
                    CurrentDay += 1;
                    OnDayChangedAction?.Invoke(CurrentDay);
                }
            }

            UpdateDelegationAction();

        }
        #region ί���¼�����
        private List<DelegationAction> DelegationActions = new ();
        struct DelegationAction
        {
            public int day;
            public int hour;
            public Action Action;

            public DelegationAction(int day,int hour, Action Action)
            {
                this.day = day;
                this.hour = hour;
                this.Action = Action;
            }

        }

        /// <summary>
        /// ĳ��ĳСʱ�������¼�
        /// </summary>
        public void AddDelegationAction(int day,int hour,Action action)
        {
            DelegationActions.Add(new DelegationAction(day, hour, action));
        }
        /// <summary>
        /// ���촥�����¼�
        /// </summary>
        public void AddDelegationAction(int hour, Action action)
        {
            DelegationActions.Add(new DelegationAction(CurrentDay, hour, action));
        }

        /// <summary>
        /// �����ĳСʱ�������¼�
        /// </summary>
        public void AddDelegationActionFromNow(int day, int hour, Action action)
        {
            DelegationActions.Add(new DelegationAction(day + CurrentDay, hour, action));
        }
        /// <summary>
        /// ���촥�����¼�,��Сʱ�󴥷�
        /// </summary>
        public void AddDelegationActionFromNow(int hour, Action action)
        {
            DelegationActions.Add(new DelegationAction(CurrentDay,CurrentHour + hour, action));
        }

        private void UpdateDelegationAction()
        {
            for (int i = 0; i < DelegationActions.Count; i++) 
            {
                if (DelegationActions[i].day == CurrentDay && DelegationActions[i].hour == CurrentHour)
                {
                    DelegationActions[i].Action?.Invoke();
                    DelegationActions.Remove(DelegationActions[i]);
                }
            }
        }
        #endregion
    }
}

