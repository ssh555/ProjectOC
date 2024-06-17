using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace ProjectOC.ManagerNS
{
    [LabelText("时间管理器"), System.Serializable]
    public sealed class DispatchTimeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        private float timeScale = 1f;
        [LabelText("现实多少秒等于游戏内1分钟"), ReadOnly, ShowInInspector]
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
        [LabelText("当前分钟 [0, 59]"), ShowInInspector]
        public int CurrentMinute = 0;
        [LabelText("当前时段 [0, 23]"),  ShowInInspector]
        public int CurrentHour = 0;
        [LabelText("当前日期 [0,]"), ShowInInspector]
        public int CurrentDay = 0;
        /// <summary>
        /// 计时器，时间为TimeScale秒，循环计时。每次计时结束后更新时段，并调用时段更新事件
        /// </summary>
        private ML.Engine.Timer.CounterDownTimer Timer;
        /// <summary>
        /// 时段更新事件，时段切换时调用，参数为当前新的时段
        /// </summary>
        public event System.Action<int> OnHourChangedAction;
        /// <summary>
        /// 日期更新事件，日期切换时调用，参数为当前新的日期
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
        #region 委托事件管理
        private List<DelegationAction> DelegationActions = new ();
        struct DelegationAction
        {
            //注册事件时开始的时间
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
        /// 几天后某小时某分触发的事件
        /// </summary>
        public void AddDelegationActionFromNow(int day, int hour,int min, Action action)
        {
            DelegationActions.Add(new DelegationAction(day, hour, min, action));
        }
        /// <summary>
        /// 几小时几分后触发
        /// </summary>
        public void AddDelegationActionFromNow(int hour, int min, Action action)
        {
            DelegationActions.Add(new DelegationAction(0, hour, min, action));
        }

        /// <summary>
        /// 几分后触发
        /// </summary>
        public void AddDelegationActionFromNow(int min, Action action)
        {
            DelegationActions.Add(new DelegationAction(0, 0, min, action));
        }

        /// <summary>
        /// 当天某小时触发的事件
        /// </summary>
        public void AddDelegationAction(int hour, Action action)
        {
            if(hour < CurrentHour)
            {
                throw new Exception("触发时间已经过去！");
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
            //计算当前差的时间
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

