using ML.Engine.FSM;
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
        public Worker Worker;
        /// <summary>
        /// 体力是否高于工作阈值
        /// </summary>
        public bool IsAPAboveWorkThreshold { get => Worker.APCurrent >= Worker.APWorkThreshold; }
        /// <summary>
        /// 体力是否高于休息阈值
        /// </summary>
        public bool IsAPAboveRelaxThreshold { get => Worker.APCurrent >= Worker.APRelaxThreshold; }
        /// <summary>
        /// 体力达到最大值
        /// </summary>
        public bool IsAPMax { get => Worker.APCurrent >= Worker.APMax; }

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
            if (curState == null || curState.Name != StateFishing.Name)
            {
                return false;
            }
            //Debug.Log($"{Time.frameCount} {IsAPAboveWorkThreshold} {Worker.IsOnDuty} {Worker.ProNode.State == ProNodeState.Production}");
            // 体力高于工作阈值 && 在生产节点 && 生产节点在生产
            return IsAPAboveWorkThreshold && Worker.IsOnDuty && Worker.HasProNode && Worker.ProNode.State == ProNodeState.Production;
        }
        /// <summary>
        /// 生产节点工作到摸鱼
        /// </summary>
        private bool EdgeWorkingDutyToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateWorkingDuty.Name)
            {
                return false;
            }
            // 体力低于工作阈值 || 没在生产节点 || 生产节点未在生产
            return !IsAPAboveWorkThreshold || !Worker.IsOnDuty || !Worker.HasProNode || Worker.ProNode.State != ProNodeState.Production;
        }
        /// <summary>
        /// 摸鱼到搬运
        /// </summary>
        private bool EdgeFishingToWorkingTransport(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateFishing.Name)
            {
                return false;
            }
            // 体力高于工作阈值 && 有任务
            return IsAPAboveWorkThreshold && Worker.HasTransport;
        }
        /// <summary>
        /// 搬运到摸鱼
        /// </summary>
        private bool EdgeWorkingTransportToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateWorkingTransport.Name)
            {
                return false;
            }
            // 体力低于工作阈值 || 没有任务
            return !IsAPAboveWorkThreshold || !Worker.HasTransport;
        }
        /// <summary>
        /// 摸鱼到休息
        /// </summary>
        private bool EdgeFishingToRelaxing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateFishing.Name)
            {
                return false;
            }
            return Worker.CurTimeFrameStatus == TimeStatus.Relax || !IsAPAboveWorkThreshold;
        }
        /// <summary>
        /// 休息到摸鱼
        /// </summary>
        private bool EdgeRelaxingToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateRelaxing.Name)
            {
                return false;
            }
            return Worker.CurTimeFrameStatus != TimeStatus.Relax && IsAPAboveRelaxThreshold;
        }
    }
}

