using ML.Engine.FSM;
using ProjectOC.ProNodeNS;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ����״̬��
    /// </summary>
    [System.Serializable]
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
            this.Worker = worker;
            this.StateFishing = new WorkerStateFishing("StateFishing");
            this.StateWorkingDuty = new WorkerStateWorkingDuty("StateWorkingDuty");
            this.StateWorkingTransport = new WorkerStateWorkingTransport("StateWorkingTransport");
            this.StateRelaxing = new WorkerStateRelaxing("StateRelaxing");
            // ���ó�ʼ״̬
            this.AddState(StateFishing);
            this.AddState(StateWorkingDuty);
            this.AddState(StateWorkingTransport);
            this.AddState(StateRelaxing);
            // ���ӱ�
            this.ConnectState(StateFishing.Name, StateWorkingDuty.Name, EdgeFishingToWorkingDuty);
            this.ConnectState(StateFishing.Name, StateWorkingTransport.Name, EdgeFishingToWorkingTransport);
            this.ConnectState(StateFishing.Name, StateRelaxing.Name, EdgeFishingToRelaxing);

            this.ConnectState(StateWorkingDuty.Name, StateFishing.Name, EdgeWorkingDutyToFishing);
            this.ConnectState(StateWorkingTransport.Name, StateFishing.Name, EdgeWorkingTransportToFishing);
            this.ConnectState(StateRelaxing.Name, StateFishing.Name, EdgeRelaxingToFishing);

            this.SetInitState("StateFishing");
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
            //Debug.Log($"{Time.frameCount} {IsAPAboveWorkThreshold} {Worker.IsOnDuty} {Worker.ProNode.State == ProNodeState.Production}");
            // �������ڹ�����ֵ && �������ڵ� && �����ڵ�������
            return IsAPAboveWorkThreshold && Worker.IsOnDuty && Worker.HasProNode && Worker.ProNode.State == ProNodeState.Production;
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
            return !IsAPAboveWorkThreshold || !Worker.IsOnDuty || !Worker.HasProNode || Worker.ProNode.State != ProNodeState.Production;
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
            return IsAPAboveWorkThreshold && Worker.HasTransport;
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
            return !IsAPAboveWorkThreshold || !Worker.HasTransport;
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
            return Worker.CurTimeFrameStatus == TimeStatus.Relax || !IsAPAboveWorkThreshold;
        }
        /// <summary>
        /// ��Ϣ������
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

