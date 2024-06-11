using ML.Engine.Timer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ML.Engine.MonitorEvent.MonitorEvent;
namespace ML.Engine.MonitorEvent
{
    /// <summary>
    /// 该类用于处理监听事件 监听满足某个条件则触发相应的函数
    /// </summary>
    public sealed class MonitorEventManager : ML.Engine.Manager.GlobalManager.IGlobalManager, ITickComponent
    {
        /// <summary>
        /// 当前的监听事件列表
        /// </summary>
        private List<MonitorEvent> monitorEvents;

        /// <summary>
        /// 更新中途的监听事件列表
        /// </summary>
        private List<MonitorEvent> tmpMonitorEvents;
        private int monitorEventCnt { get { return monitorEvents.Count; } }
        /// <summary>
        /// Timer一次计时之间需要处理的MonitorEvent数
        /// </summary>
        private int TaskCnt;
        /// <summary>
        /// 当前剩余未处理的MonitorEvent数
        /// </summary>
        private int curRemainTaskCnt;
        /// <summary>
        /// 两帧之间的真实时间
        /// </summary>
        private float RealTimeIntervalBetweenTwoFrame;
        /// <summary>
        /// 预计到下次更新还有几帧
        /// </summary>
        private int FrameCntByNextUpdate;
        /// <summary>
        /// 更新间隔
        /// </summary>
        private int RefreshInterval = 1;
        /// <summary>
        /// 上一次更新结束时的真实时间
        /// </summary>
        private float LastEndRealTime;
        /// <summary>
        /// 当前更新剩余时间
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

                //判断当前是否为最后一帧

                if(RemainRealTime - RealTimeIntervalBetweenTwoFrame<=0)
                {
                    //将剩余的全部处理
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
                //更新完毕 开启下一次更新
                //拷贝一份新的
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



