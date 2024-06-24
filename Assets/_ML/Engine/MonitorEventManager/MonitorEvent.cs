using System;

namespace ML.Engine.Event
{
    public class MonitorEvent
    {
        /// <summary>
        /// �ú�������Ϊ�������¼�
        /// </summary>
        public delegate bool MonitorCondition();
        private MonitorCondition MonitorConditionDelegateBuffOn;
        private MonitorCondition MonitorConditionDelegateBuffOff;
        /// <summary>
        /// �ú�������Ϊ�����������ʱִ�еĺ���
        /// </summary>
        private Action BuffOnAction;

        /// <summary>
        /// �ú�������Ϊ�������������ʱִ�еĺ���
        /// </summary>
        private Action BuffOffAction;

        /// <summary>
        /// �ú�������Ϊ�������������ʱִ�еĺ���
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
        /// ����1 2 ��ʾ��Ҫ�Ƴ�
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



