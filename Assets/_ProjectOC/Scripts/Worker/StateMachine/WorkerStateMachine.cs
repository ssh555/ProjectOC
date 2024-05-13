using ML.Engine.FSM;
using ProjectOC.ProNodeNS;
using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [LabelText("����״̬��"), System.Serializable]
    public sealed class WorkerStateMachine : StateMachine
    {
        /// <summary>
        /// ״̬����������
        /// </summary>
        public Worker Worker;
        /// <summary>
        /// �����Ƿ���ڹ�����ֵ
        /// </summary>
        public bool IsAPAboveWorkThreshold { get => Worker.APCurrent >= Worker.APWorkThreshold; }
        /// <summary>
        /// �����Ƿ������Ϣ��ֵ
        /// </summary>
        public bool IsAPAboveRelaxThreshold { get => Worker.APCurrent >= Worker.APRelaxThreshold; }
        /// <summary>
        /// �����ﵽ���ֵ
        /// </summary>
        public bool IsAPMax { get => Worker.APCurrent >= Worker.APMax; }

        private WorkerStateFishing StateFishing;
        private WorkerStateWorkingDuty StateWorkingDuty;
        private WorkerStateWorkingTransport StateWorkingTransport;
        private WorkerStateRelaxing StateRelaxing;
        public WorkerStateMachine(Worker worker)
        {
            Worker = worker;
            StateFishing = new WorkerStateFishing("StateFishing");
            StateWorkingDuty = new WorkerStateWorkingDuty("StateWorkingDuty");
            StateWorkingTransport = new WorkerStateWorkingTransport("StateWorkingTransport");
            StateRelaxing = new WorkerStateRelaxing("StateRelaxing");
            // ���ó�ʼ״̬
            AddState(StateFishing);
            AddState(StateWorkingDuty);
            AddState(StateWorkingTransport);
            AddState(StateRelaxing);
            // ���ӱ�
            ConnectState(StateFishing.Name, StateWorkingDuty.Name, EdgeFishingToWorkingDuty);
            ConnectState(StateFishing.Name, StateWorkingTransport.Name, EdgeFishingToWorkingTransport);
            ConnectState(StateFishing.Name, StateRelaxing.Name, EdgeFishingToRelaxing);

            ConnectState(StateWorkingDuty.Name, StateFishing.Name, EdgeWorkingDutyToFishing);
            ConnectState(StateWorkingTransport.Name, StateFishing.Name, EdgeWorkingTransportToFishing);
            ConnectState(StateRelaxing.Name, StateFishing.Name, EdgeRelaxingToFishing);

            SetInitState("StateFishing");
        }
        /// <summary>
        /// ���㵽�����ڵ㹤��
        /// </summary>
        private bool EdgeFishingToWorkingDuty(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateFishing.Name)
            {
                return false;
            }
            // �������ڹ�����ֵ && �������ڵ� && �����ڵ�������
            return IsAPAboveWorkThreshold && Worker.IsOnProNodeDuty && Worker.ProNode.State == ProNodeState.Production;
        }
        /// <summary>
        /// �����ڵ㹤��������
        /// </summary>
        private bool EdgeWorkingDutyToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateWorkingDuty.Name)
            {
                return false;
            }
            // �������ڹ�����ֵ || û�������ڵ� || �����ڵ�δ������
            return !IsAPAboveWorkThreshold || !Worker.IsOnProNodeDuty || Worker.ProNode.State != ProNodeState.Production;
        }
        /// <summary>
        /// ���㵽����
        /// </summary>
        private bool EdgeFishingToWorkingTransport(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateFishing.Name)
            {
                return false;
            }
            // �������ڹ�����ֵ && ������
            return IsAPAboveWorkThreshold && Worker.HaveTransport;
        }
        /// <summary>
        /// ���˵�����
        /// </summary>
        private bool EdgeWorkingTransportToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateWorkingTransport.Name)
            {
                return false;
            }
            // �������ڹ�����ֵ || û������
            return !IsAPAboveWorkThreshold || !Worker.HaveTransport;
        }
        /// <summary>
        /// ���㵽��Ϣ
        /// </summary>
        private bool EdgeFishingToRelaxing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateFishing.Name)
            {
                return false;
            }
            return Worker.CurTimeStatus == TimeStatus.Relax || !IsAPAboveWorkThreshold;
        }
        /// <summary>
        /// ��Ϣ������
        /// </summary>
        private bool EdgeRelaxingToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != StateRelaxing.Name)
            {
                return false;
            }
            return Worker.CurTimeStatus != TimeStatus.Relax && IsAPAboveRelaxThreshold;
        }
    }
}

