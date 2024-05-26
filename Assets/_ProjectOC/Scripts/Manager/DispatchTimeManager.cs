using Sirenix.OdinInspector;

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
        }
    }
}

