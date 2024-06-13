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
    /// �������ڴ�������¼� ��������ĳ�������򴥷���Ӧ�ĺ���
    /// </summary>
    [System.Serializable]
    public sealed partial class FunctionLiabrary : ML.Engine.Manager.GlobalManager.IGlobalManager, ITickComponent
    {
        /// <summary>
        /// ��ǰ�ļ����¼��б�
        /// </summary>
        [ShowInInspector]
        private List<MonitorEvent> monitorEvents;

        /// <summary>
        /// ������;�ļ����¼��б�
        /// </summary>
        [ShowInInspector]
        private List<MonitorEvent> tmpMonitorEvents;
        private int monitorEventCnt { get { return monitorEvents.Count; } }
        /// <summary>
        /// Timerһ�μ�ʱ֮����Ҫ�����MonitorEvent��
        /// </summary>
        private int TaskCnt;
        /// <summary>
        /// ��ǰ�����MonitorEvent��
        /// </summary>
        private int curTaskCnt;
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

        private int curCnt = 0;

/*        private IEnumerator AddMonitorEvent(int num)
        {
            yield return new WaitForSeconds(5);
            
            for (int i = 0; i < num; i++)
            {
                RegisterMonitorEvent(() => { return false; }, null);
            }
            Debug.Log($"���� {num}��");
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

                //�жϵ�ǰ�Ƿ�Ϊ���һ֡

                if(RemainRealTime - RealTimeIntervalBetweenTwoFrame<=0)
                {
                    //��ʣ���ȫ������
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
                    //Debug.Log($"{RemainRealTime} {RealTimeIntervalBetweenTwoFrame} {curCnt}���֡ {Time.realtimeSinceStartup} ������{tc}�� �ܹ�������{curTaskCnt}��");
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
                    //Debug.Log($"{RemainRealTime} {RealTimeIntervalBetweenTwoFrame} {curCnt}֡ {Time.realtimeSinceStartup} ������{RefreshNum}�� �ܹ�������{curTaskCnt}��");
                }
                RemainRealTime -= deltatime;
            }
            else
            {
                //������� ������һ�θ���
                //Debug.Log("������� ������һ�θ���");
                //����һ���µ�
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



