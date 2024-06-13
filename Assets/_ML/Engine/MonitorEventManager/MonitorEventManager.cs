using ML.Engine.Timer;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ML.Engine.Event.MonitorEvent;
namespace ML.Engine.Event
{
    /// <summary>
    /// 该类用于处理监听事件 监听满足某个条件则触发相应的函数
    /// </summary>
    [System.Serializable]
    public sealed partial class FunctionLiabrary : ML.Engine.Manager.GlobalManager.IGlobalManager, ITickComponent
    {
        /// <summary>
        /// 当前的监听事件列表
        /// </summary>
        [ShowInInspector]
        private List<MonitorEvent> monitorEvents;

        /// <summary>
        /// 更新中途的监听事件列表
        /// </summary>
        [ShowInInspector]
        private List<MonitorEvent> tmpMonitorEvents;
        private int monitorEventCnt { get { return monitorEvents.Count; } }
        /// <summary>
        /// Timer一次计时之间需要处理的MonitorEvent数
        /// </summary>
        private int TaskCnt;
        /// <summary>
        /// 当前处理的MonitorEvent数
        /// </summary>
        private int curTaskCnt;
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

        private int curCnt = 0;

/*        private IEnumerator AddMonitorEvent(int num)
        {
            yield return new WaitForSeconds(5);
            
            for (int i = 0; i < num; i++)
            {
                RegisterMonitorEvent(() => { return false; }, null);
            }
            Debug.Log($"加入 {num}个");
        }*/


        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void FixedTick(float deltatime)
        {
            if (monitorEventCnt == 0 && tmpMonitorEvents.Count == 0) return;

            if(LastEndRealTime < 0)
            {
                LastEndRealTime = Time.realtimeSinceStartup;
                return;
            }

            if (RemainRealTime > 0) 
            {
                RealTimeIntervalBetweenTwoFrame = Time.realtimeSinceStartup - LastEndRealTime;

                //判断当前是否为最后一帧

                if(RemainRealTime - RealTimeIntervalBetweenTwoFrame<=0)
                {
                    //将剩余的全部处理
                    //var tc = 0;
                    for (int i = curTaskCnt; i < monitorEventCnt; ++i)
                    {
                        if (monitorEvents[curTaskCnt].Check() == 0)
                        {
                            tmpMonitorEvents.Add(monitorEvents[i]);
                        }
                        ++curTaskCnt;
                        //++tc;
                    }
                    ++curCnt;
                    //Debug.Log($"{RemainRealTime} {RealTimeIntervalBetweenTwoFrame} {curCnt}最后帧 {Time.realtimeSinceStartup} 处理了{tc}个 总共处理了{curTaskCnt}个");
                }
                else
                {
                    var RefreshNum = Mathf.CeilToInt(RealTimeIntervalBetweenTwoFrame / RefreshInterval * monitorEventCnt);

                    for (int i = curTaskCnt, j = 0; j < RefreshNum && i < monitorEventCnt; ++i, ++j) 
                    {
                        if (monitorEvents[i].Check() == 0)
                        {
                            tmpMonitorEvents.Add(monitorEvents[i]);
                        }
                        ++curTaskCnt;
                    }
                    ++curCnt;
                    //Debug.Log($"{RemainRealTime} {RealTimeIntervalBetweenTwoFrame} {curCnt}帧 {Time.realtimeSinceStartup} 处理了{RefreshNum}个 总共处理了{curTaskCnt}个");
                }
                RemainRealTime -= deltatime;
            }
            else
            {
                //更新完毕 开启下一次更新
                //Debug.Log("更新完毕 开启下一次更新");
                //拷贝一份新的
                monitorEvents = tmpMonitorEvents.ToList();
                RemainRealTime = RefreshInterval;
                curTaskCnt = 0;
                curCnt = 0;
                tmpMonitorEvents.Clear();
            }
            LastEndRealTime = Time.realtimeSinceStartup;
        }
        #endregion


        #region Internal
        private void RegisterMonitorEvent(MonitorCondition monitorConditionBuffOn, Action buffOnAction, MonitorCondition monitorConditionBuffOff = null, Action buffOffAction = null)
        {
            this.monitorEvents.Add(new MonitorEvent(monitorConditionBuffOn, buffOnAction, monitorConditionBuffOff, buffOffAction));
        }
        #endregion


        #region External
        public void RegisterMonitorEvent(string monitorConditionBuffOnConditionStr, string buffOnActionStr, string monitorConditionBuffOffConditionStr, string buffOffActionStr)
        {
            RegisterMonitorEvent(
                monitorConditionBuffOn: () => { return ExecuteCondition(monitorConditionBuffOnConditionStr); },
                buffOnAction: () => { ExecuteEvent(buffOnActionStr); },
                monitorConditionBuffOff: () => { return ExecuteCondition(monitorConditionBuffOffConditionStr); },
                buffOffAction: () => { ExecuteEvent(buffOffActionStr); });
        }
        #endregion

    }
}



