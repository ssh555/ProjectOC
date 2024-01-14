using ML.Engine.FSM;
using ProjectOC.MissionNS;
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
        /// ֵ��ڵ�
        /// </summary>
        public ProNode ProNode { get { return this.Worker.ProNode; } }
        /// <summary>
        /// ��������
        /// </summary>
        public Transport Transport { get { return this.Worker.Transport; } }
        /// <summary>
        /// ��ǰʱ�εİ���
        /// </summary>
        public TimeStatus CurTimeFrameStatus { get { return this.Worker.CurTimeFrameStatus; } }
        /// <summary>
        /// �����Ƿ���ڹ�����ֵ
        /// </summary>
        public bool IsAPAboveWorkThreshold
        { 
            get
            {
                return this.Worker.APCurrent >= this.Worker.APWorkThreshold;
            }
        }
        /// <summary>
        /// �����Ƿ������Ϣ��ֵ
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
            if (curState == null || curState.Name != this.StateWorkingDuty.Name)
            {
                return false;
            }
            // �������ڹ�����ֵ && �������ڵ� && �����ڵ�������
            return this.IsAPAboveWorkThreshold && this.ProNode != null && this.ProNode.State == ProNodeState.Production;
        }
        /// <summary>
        /// �����ڵ㹤��������
        /// </summary>
        private bool EdgeWorkingDutyToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateFishing.Name)
            {
                return false;
            }
            // �������ڹ�����ֵ || û�������ڵ� || �����ڵ�δ������
            return !this.IsAPAboveWorkThreshold || this.ProNode == null || this.ProNode.State != ProNodeState.Production;
        }
        /// <summary>
        /// ���㵽����
        /// </summary>
        private bool EdgeFishingToWorkingTransport(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateWorkingTransport.Name)
            {
                return false;
            }
            // �������ڹ�����ֵ && û�������ڵ� && ������
            return this.IsAPAboveWorkThreshold && this.ProNode == null && this.Transport != null;
        }
        /// <summary>
        /// ���˵�����
        /// </summary>
        private bool EdgeWorkingTransportToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateFishing.Name)
            {
                return false;
            }
            // �������ڹ�����ֵ || �������ڵ� || û������
            return !this.IsAPAboveWorkThreshold || this.ProNode != null || this.Transport == null;
        }
        /// <summary>
        /// ���㵽��Ϣ
        /// </summary>
        private bool EdgeFishingToRelaxing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateRelaxing.Name)
            {
                return false;
            }
            // �������ڹ�����ֵ || ������Ϣʱ��
            return !this.IsAPAboveWorkThreshold || this.CurTimeFrameStatus == TimeStatus.Relax;
        }
        /// <summary>
        /// ��Ϣ������
        /// </summary>
        private bool EdgeRelaxingToFishing(StateMachine stateMachine, State curState)
        {
            if (curState == null || curState.Name != this.StateFishing.Name)
            {
                return false;
            }
            // ����������Ϣ��ֵ && ��������Ϣʱ��
            return this.IsAPAboveRelaxThreshold && this.CurTimeFrameStatus != TimeStatus.Relax;
        }
    }
}

