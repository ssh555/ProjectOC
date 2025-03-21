using ML.Engine.FSM;
using ProjectOC.ProNodeNS;
using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [LabelText("刁民状态机"), System.Serializable]
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
        private WorkerStateBan StateBan;

        public WorkerStateMachine(Worker worker)
        {
            Worker = worker;
            StateFishing = new WorkerStateFishing("StateFishing");
            StateWorkingDuty = new WorkerStateWorkingDuty("StateWorkingDuty");
            StateWorkingTransport = new WorkerStateWorkingTransport("StateWorkingTransport");
            StateRelaxing = new WorkerStateRelaxing("StateRelaxing");
            StateBan = new WorkerStateBan("StateBan");
            // 设置初始状态
            AddState(StateFishing);
            AddState(StateWorkingDuty);
            AddState(StateWorkingTransport);
            AddState(StateRelaxing);
            AddState(StateBan);
            // 连接边
            ConnectState(StateFishing.Name, StateWorkingDuty.Name, EdgeFishingToWorkingDuty);
            ConnectState(StateFishing.Name, StateWorkingTransport.Name, EdgeFishingToWorkingTransport);
            ConnectState(StateFishing.Name, StateRelaxing.Name, EdgeFishingToRelaxing);

            ConnectState(StateWorkingDuty.Name, StateFishing.Name, EdgeWorkingDutyToFishing);
            ConnectState(StateWorkingTransport.Name, StateFishing.Name, EdgeWorkingTransportToFishing);
            ConnectState(StateRelaxing.Name, StateFishing.Name, EdgeRelaxingToFishing);

            ConnectState(StateRelaxing.Name, StateBan.Name, EdgeRelaxingToBan);
            ConnectState(StateFishing.Name, StateBan.Name, EdgeFishingToBan);
            ConnectState(StateBan.Name, StateFishing.Name, EdgeBanToFishing);

            SetInitState("StateFishing");
        }
        /// <summary>
        /// 摸鱼到生产节点工作
        /// </summary>
        private bool EdgeFishingToWorkingDuty(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateFishing.Name) { return false; }
            // 体力高于工作阈值 && 在生产节点 && 生产节点在生产
            return IsAPAboveWorkThreshold && Worker.IsOnProNodeDuty && Worker.ProNode.State == ProNodeState.Production;
        }
        /// <summary>
        /// 生产节点工作到摸鱼
        /// </summary>
        private bool EdgeWorkingDutyToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateWorkingDuty.Name) { return false; }
            // 体力低于工作阈值 || 没在生产节点 || 生产节点未在生产
            return !IsAPAboveWorkThreshold || !Worker.IsOnProNodeDuty || Worker.ProNode.State != ProNodeState.Production;
        }
        /// <summary>
        /// 摸鱼到搬运
        /// </summary>
        private bool EdgeFishingToWorkingTransport(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateFishing.Name) { return false; }
            // 体力高于工作阈值 && 有任务
            return IsAPAboveWorkThreshold && Worker.HaveTransport;
        }
        /// <summary>
        /// 搬运到摸鱼
        /// </summary>
        private bool EdgeWorkingTransportToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateWorkingTransport.Name) { return false; }
            // 体力低于工作阈值 || 没有任务
            return !IsAPAboveWorkThreshold || !Worker.HaveTransport;
        }
        /// <summary>
        /// 摸鱼到休息
        /// </summary>
        private bool EdgeFishingToRelaxing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateFishing.Name) { return false; }
            return Worker.CurTimeStatus == TimeStatus.Relax || !IsAPAboveWorkThreshold;
        }
        /// <summary>
        /// 休息到摸鱼
        /// </summary>
        private bool EdgeRelaxingToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateRelaxing.Name) { return false; }
            return Worker.CurTimeStatus != TimeStatus.Relax && IsAPAboveRelaxThreshold;
        }

        private bool EdgeFishingToBan(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateFishing.Name) { return false; }
            return Worker.HaveFeatSeat;
        }
        private bool EdgeBanToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateBan.Name) { return false; }
            return !Worker.HaveFeatSeat;
        }
        private bool EdgeRelaxingToBan(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateRelaxing.Name) { return false; }
            return Worker.HaveFeatSeat;
        }
    }
}

