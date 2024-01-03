using ML.Engine.FSM;
using ProjectOC.MissionNS;
using ProjectOC.ProNodeNS;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// 刁民状态机
    /// </summary>
    [System.Serializable]
    public sealed class WorkerStateMachine : StateMachine
    {
        /// <summary>
        /// 状态机所属刁民
        /// </summary>
        public Worker Worker 
        {
            get 
            {
                if (Worker == null)
                {
                    Debug.LogError("Worker is Null");
                }
                return Worker;
            }
            set 
            { 
                Worker = value; 
            }
        }
        /// <summary>
        /// 值班节点
        /// </summary>
        public ProNode ProNode { get { return this.Worker.ProNode; } }
        /// <summary>
        /// 搬运任务
        /// </summary>
        public Transport Transport { get { return this.Worker.Transport; } }
        /// <summary>
        /// 当前时段的安排
        /// </summary>
        public TimeStatus CurTimeFrameStatus { get { return this.Worker.CurTimeFrameStatus; } }
        /// <summary>
        /// 体力是否高于工作阈值
        /// </summary>
        public bool IsAPAboveWorkThreshold
        { 
            get
            {
                return this.Worker.APCurrent >= this.Worker.APWorkThreshold;
            }
        }
        /// <summary>
        /// 体力是否高于休息阈值
        /// </summary>
        public bool IsAPAboveRelaxThreshold
        {
            get
            {
                if (this.Worker != null)
                {
                    return this.Worker.APCurrent >= this.Worker.APRelaxThreshold;
                }
                return false;
            }
        }

        private WorkerStateFishing StateFishing;
        private WorkerStateWorkingDuty StateWorkingDuty;
        private WorkerStateWorkingTransport StateWorkingTransport;
        private WorkerStateRelaxing StateRelaxing;
        public WorkerStateMachine(Worker worker)
        {
            this.Worker = worker;
            this.StateFishing = new WorkerStateFishing("StateFishing");
            this.StateWorkingDuty = new WorkerStateWorkingDuty("StateWorkingDuty");
            this.StateWorkingTransport = new WorkerStateWorkingTransport("StateWorkingTransport");
            this.StateRelaxing = new WorkerStateRelaxing("StateRelaxing");
            // 设置初始状态
            this.AddState(StateFishing);
            this.AddState(StateWorkingDuty);
            this.AddState(StateWorkingTransport);
            this.AddState(StateRelaxing);
            // 连接边
            this.ConnectState(StateFishing.Name, StateWorkingDuty.Name, EdgeFishingToWorkingDuty);
            this.ConnectState(StateFishing.Name, StateWorkingTransport.Name, EdgeFishingToWorkingTransport);
            this.ConnectState(StateFishing.Name, StateRelaxing.Name, EdgeFishingToRelaxing);

            this.ConnectState(StateWorkingDuty.Name, StateFishing.Name, EdgeWorkingDutyToFishing);
            this.ConnectState(StateWorkingTransport.Name, StateFishing.Name, EdgeWorkingTransportToFishing);
            this.ConnectState(StateRelaxing.Name, StateFishing.Name, EdgeRelaxingToFishing);

            this.SetInitState("StateFishing");
        }
        /// <summary>
        /// 摸鱼到生产节点工作
        /// </summary>
        private bool EdgeFishingToWorkingDuty(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateWorkingDuty.Name)
            {
                return false;
            }
            // 体力高于工作阈值 && 有生产节点 && 生产节点在生产
            return this.IsAPAboveWorkThreshold && this.ProNode != null && this.ProNode.State == ProNodeState.Production;
        }
        /// <summary>
        /// 生产节点工作到摸鱼
        /// </summary>
        private bool EdgeWorkingDutyToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateFishing.Name)
            {
                return false;
            }
            // 体力低于工作阈值 || 没有生产节点 || 生产节点未在生产
            return !this.IsAPAboveWorkThreshold || this.ProNode == null || this.ProNode.State != ProNodeState.Production;
        }
        /// <summary>
        /// 摸鱼到搬运
        /// </summary>
        private bool EdgeFishingToWorkingTransport(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateWorkingTransport.Name)
            {
                return false;
            }
            // 体力高于工作阈值 && 没有生产节点 && 有任务
            return this.IsAPAboveWorkThreshold && this.ProNode == null && this.Transport != null;
        }
        /// <summary>
        /// 搬运到摸鱼
        /// </summary>
        private bool EdgeWorkingTransportToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateFishing.Name)
            {
                return false;
            }
            // 体力低于工作阈值 || 有生产节点 || 没有任务
            return !this.IsAPAboveWorkThreshold || this.ProNode != null || this.Transport == null;
        }
        /// <summary>
        /// 摸鱼到休息
        /// </summary>
        private bool EdgeFishingToRelaxing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateRelaxing.Name)
            {
                return false;
            }
            // 体力低于工作阈值 || 处于休息时段
            return !this.IsAPAboveWorkThreshold || this.CurTimeFrameStatus == TimeStatus.Relax;
        }
        /// <summary>
        /// 休息到摸鱼
        /// </summary>
        private bool EdgeRelaxingToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateFishing.Name)
            {
                return false;
            }
            // 体力低于休息阈值 && 不处于休息时段
            return this.IsAPAboveRelaxThreshold && this.CurTimeFrameStatus != TimeStatus.Relax;
        }
    }
}

