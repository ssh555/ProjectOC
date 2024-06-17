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
            //ע���¼�ʱ��ʼ��ʱ��
            private int startDay;
            public int StartDay { get { return startDay; } }
            private int startHour;
            public int StartHour { get { return startHour; } }
            private int startMin;
            public int StartMin { get { return startMin; } }

            private int day;
            public int Day { get { return day; } }
            private int hour;
            public int Hour { get { return hour; } }
            private int min;
            public int Min { get { return min; } }
            private Action action;
            public Action Action { get { return action; } }

            public DelegationAction(int day,int hour,int min, Action Action)
            {
                this.startDay = LocalGameManager.Instance.DispatchTimeManager.CurrentDay;
                this.startHour = LocalGameManager.Instance.DispatchTimeManager.CurrentHour;
                this.startMin = LocalGameManager.Instance.DispatchTimeManager.CurrentMinute;
                this.day = day;
                this.hour = hour;
                this.min = min;
                this.action = Action;
            }
        }

        /// <summary>
        /// �����ĳСʱĳ�ִ������¼�
        /// </summary>
        public void AddDelegationActionFromNow(int day, int hour,int min, Action action)
        {
            DelegationActions.Add(new DelegationAction(day, hour, min, action));
        }
        /// <summary>
        /// ��Сʱ���ֺ󴥷�
        /// </summary>
        public void AddDelegationActionFromNow(int hour, int min, Action action)
        {
            DelegationActions.Add(new DelegationAction(0, hour, min, action));
        }

        /// <summary>
        /// ���ֺ󴥷�
        /// </summary>
        public void AddDelegationActionFromNow(int min, Action action)
        {
            DelegationActions.Add(new DelegationAction(0, 0, min, action));
        }

        /// <summary>
        /// ����ĳСʱ�������¼�
        /// </summary>
        public void AddDelegationAction(int hour, Action action)
        {
            if(hour < CurrentHour)
            {
                throw new Exception("����ʱ���Ѿ���ȥ��");
            }

            if(CurrentMinute == 0)
            {
                AddDelegationActionFromNow(hour - CurrentHour, 0, action);
            }
            else
            {
                AddDelegationActionFromNow(hour - CurrentHour - 1, 60 - CurrentMinute, action);
            }
        }

        private bool CheckIsTimeUp(DelegationAction delegationAction)
        {
            //���㵱ǰ���ʱ��
            int gapTime = (1440 * CurrentDay + 60 * CurrentHour + CurrentMinute) - (1440 * delegationAction.StartDay + 60 * delegationAction.StartHour + delegationAction.StartMin);
            int targetTime = 1440 * delegationAction.Day + 60 * delegationAction.Hour + delegationAction.Min ;
            UnityEngine.Debug.Log(gapTime + " " + targetTime+" "+ delegationAction.Day+" "+ delegationAction.Hour);
            return gapTime >= targetTime;
        }

        private void UpdateDelegationAction()
        {
            for (int i = 0; i < DelegationActions.Count; i++) 
            {
                if (CheckIsTimeUp(DelegationActions[i]))
                {
                    DelegationActions[i].Action?.Invoke();
                    DelegationActions.Remove(DelegationActions[i]);
                }
            }
        }
        #endregion
    }
}

