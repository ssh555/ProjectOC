using System;

namespace ML.Engine.Event
{
    public class MonitorEvent
    {
        /// <summary>
        /// 该函数功能为监听的事件
        /// </summary>
        public delegate bool MonitorCondition();
        private MonitorCondition MonitorConditionDelegateBuffOn;
        private MonitorCondition MonitorConditionDelegateBuffOff;
        /// <summary>
        /// 该函数功能为满足监听条件时执行的函数
        /// </summary>
        private Action BuffOnAction;

        /// <summary>
        /// 该函数功能为不满足监听条件时执行的函数
        /// </summary>
        private Action BuffOffAction;

        /// <summary>
        /// 该函数功能为不满足监听条件时执行的函数
        /// </summary>
        private bool hasBuffOn;

        public MonitorEvent(MonitorCondition monitorConditionBuffOn, Action buffOnAction, MonitorCondition monitorConditionBuffOff = null, Action buffOffAction = null)
        {
            this.MonitorConditionDelegateBuffOn = monitorConditionBuffOn;
            this.MonitorConditionDelegateBuffOff = monitorConditionBuffOff;
            this.BuffOnAction = buffOnAction;
            this.BuffOffAction = buffOffAction;
            this.hasBuffOn = false;
        }

        /// <summary>
        /// 返回1 2 表示需要移除
        /// </summary>
        public int Check()
        {
            if(hasBuffOn)
            {
                if(MonitorConditionDelegateBuffOff != null)
                {
                    if (MonitorConditionDelegateBuffOff.Invoke())
                    {
                        BuffOffAction?.Invoke();
                        return 2;
                    }
                }
                else
                {
                    if (!MonitorConditionDelegateBuffOn.Invoke())
                    {
                        BuffOffAction?.Invoke();
                        return 2;
                    }
                }
            }
            else
            {
                if(MonitorConditionDelegateBuffOff == null)
                {
                    if (MonitorConditionDelegateBuffOn.Invoke())
                    {
                        BuffOnAction?.Invoke();
                        return 1;
                    }
                }
                else
                {
                    if (MonitorConditionDelegateBuffOn.Invoke())
                    {
                        hasBuffOn = true;
                        BuffOnAction?.Invoke();
                    }
                }
            }
            return 0;
        }
    }
}



