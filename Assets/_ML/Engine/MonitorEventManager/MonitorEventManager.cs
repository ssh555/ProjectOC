using ML.Engine.Timer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ML.Engine.MonitorEvent.MonitorEvent;
namespace ML.Engine.MonitorEvent
{
    /// <summary>
    /// �������ڴ�������¼� ��������ĳ�������򴥷���Ӧ�ĺ���
    /// </summary>
    public sealed class MonitorEventManager : ML.Engine.Manager.GlobalManager.IGlobalManager, ITickComponent
    {
        /// <summary>
        /// ��ǰ�ļ����¼��б�
        /// </summary>
        private List<MonitorEvent> monitorEvents;

        /// <summary>
        /// ������;�ļ����¼��б�
        /// </summary>
        private List<MonitorEvent> tmpMonitorEvents;
        private int monitorEventCnt { get { return monitorEvents.Count; } }
        /// <summary>
        /// Timerһ�μ�ʱ֮����Ҫ�����MonitorEvent��
        /// </summary>
        private int TaskCnt;
        /// <summary>
        /// ��ǰʣ��δ�����MonitorEvent��
        /// </summary>
        private int curRemainTaskCnt;
        /// <summary>
        /// ��֮֡�����ʵʱ��
        /// </summary>
        private float RealTimeIntervalBetweenTwoFrame;
        /// <summary>
        /// Ԥ�Ƶ��´θ��»��м�֡
        /// </summary>
        private int FrameCntByNextUpdate;
        /// <summary>
        /// ���¼��
        /// </summary>
        private int RefreshInterval = 1;
        /// <summary>
        /// ��һ�θ��½���ʱ����ʵʱ��
        /// </summary>
        private float LastEndRealTime;
        /// <summary>
        /// ��ǰ����ʣ��ʱ��
        /// </summary>
        private float RemainRealTime;
        public void OnRegister()
        {
            monitorEvents = new List<MonitorEvent>();
            LastEndRealTime = 0;
            RemainRealTime = RefreshInterval;
        }
        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {
            if (monitorEventCnt == 0) return;

            if (RemainRealTime > 0)
            {
                RealTimeIntervalBetweenTwoFrame = Time.realtimeSinceStartup - LastEndRealTime;

                //�жϵ�ǰ�Ƿ�Ϊ���һ֡

                if(RemainRealTime - RealTimeIntervalBetweenTwoFrame<=0)
                {
                    //��ʣ���ȫ������
                    for (int i = 0; i < monitorEventCnt; ++i)
                    {
                        if (monitorEvents[i].Check() == 0)
                        {
                            tmpMonitorEvents.Add(monitorEvents[i]);
                        }
                    }
                }
                else
                {
                    var divideNum = RemainRealTime / RealTimeIntervalBetweenTwoFrame;
                    var RefreshNum = Mathf.CeilToInt(monitorEventCnt / divideNum);

                    List<MonitorEvent> topRefreshNumList = monitorEvents.GetRange(0, Mathf.Min(RefreshNum, monitorEventCnt));
                    monitorEvents.RemoveRange(0, Mathf.Min(RefreshNum, monitorEventCnt));

                    for (int i = 0; i < RefreshNum; ++i)
                    {
                        if (topRefreshNumList[i].Check() == 0)
                        {
                            tmpMonitorEvents.Add(topRefreshNumList[i]);
                        }
                    }
                }
                RemainRealTime -= Time.deltaTime;
            }
            else
            {
                //������� ������һ�θ���
                //����һ���µ�
                monitorEvents = tmpMonitorEvents.ToList();
                RemainRealTime = RefreshInterval;
                LastEndRealTime = Time.realtimeSinceStartup;
                tmpMonitorEvents.Clear();
            }

        }
        #endregion

        #region External
        public void RegisterMonitorEvent(MonitorCondition monitorConditionBuffOn, Action buffOnAction, MonitorCondition monitorConditionBuffOff = null, Action buffOffAction = null)
        {
            this.monitorEvents.Add(new MonitorEvent(monitorConditionBuffOn, buffOnAction, monitorConditionBuffOff, buffOffAction));
        }
        #endregion

    }
}



