using ML.Engine.FSM;
using ML.Engine.Manager;
using ProjectOC.MissionNS;
using ProjectOC.ProductionNodeNS;
using UnityEditor.Experimental.GraphView;

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
        /// ֵ�ദ��ID��ţ�-1��ʾδֵ��
        /// </summary>
        public string DutyProductionNodeID = "-1";
        private ProductionNode dutyProductionNode;
        public ProductionNode DutyProductionNode
        {
            get
            {
                if (this.DutyProductionNodeID == "-1")
                {
                    this.dutyProductionNode = null;
                }
                else
                {
                    if (this.dutyProductionNode == null || this.dutyProductionNode.UID != this.DutyProductionNodeID)
                    {
                        ProductionNodeManager manager = GameManager.Instance.GetLocalManager<ProductionNodeManager>();
                        if (manager != null)
                        {
                            dutyProductionNode = manager.GetProductionNodeByUID(this.DutyProductionNodeID);
                        }
                    }
                }
                return this.dutyProductionNode;
            }
        }
        /// <summary>
        /// ֵ�ദ��ԭ���Ƿ����
        /// ֵ�ദΪ�ջ���û�䷽Ĭ��ԭ�ϳ���
        /// </summary>
        public bool IsRawItemsEnough
        {
            get
            {
                if (this.DutyProductionNode != null)
                {
                    return DutyProductionNode.IsRawItemsEnough();
                }
                return true;
            }
        }
        /// <summary>
        /// ֵ�ദ�Ƿ�ѻ���
        /// �ڵ�Ϊ��Ĭ��Ϊû�жѻ���
        /// </summary>
        public bool IsStackMax
        {
            get
            {
                if (this.DutyProductionNode != null)
                {
                    return DutyProductionNode.IsStackMax();
                }
                return false;
            }
        }
        /// <summary>
        /// �����Ƿ���ڹ�����ֵ
        /// </summary>
        public bool IsAPBelowWorkThreshold
        { 
            get
            {
                if (this.Worker != null)
                {
                    return this.Worker.APCurrent <= this.Worker.APWorkThreshold;
                }
                return false;
            }
        }
        /// <summary>
        /// �����Ƿ������Ϣ��ֵ
        /// </summary>
        public bool IsAPAboveWorkThreshold
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

        /// <summary>
        /// ��������
        /// </summary>
        public MissionTransport Mission;
        /// <summary>
        /// ��ǰʱ�εİ���
        /// </summary>
        public TimeStatus CurTimeFrameStatus
        {
            get
            {
                if (this.Worker != null)
                {
                    return this.Worker.CurTimeFrameStatus;
                }
                return TimeStatus.None;
            }
        }

        /// <summary>
        /// ��Ϣ�Ƿ����
        /// </summary>
        public bool IsCompleteRelax { get { return this.StateRelaxing.IsCompleteRelax; } }
        private WorkerStateFishing StateFishing;
        private WorkerStateWorkingDuty StateWorkingDuty;
        private WorkerStateWorkingTransport StateWorkingTransport;
        private WorkerStateRelaxing StateRelaxing;

        public WorkerStateMachine()
        {
            this.StateFishing = new WorkerStateFishing("");
            this.StateWorkingDuty = new WorkerStateWorkingDuty("");
            this.StateWorkingTransport = new WorkerStateWorkingTransport("");
            this.StateRelaxing = new WorkerStateRelaxing("");
            // ���ӱ�
            // ���ó�ʼ״̬
        }
    }
}

