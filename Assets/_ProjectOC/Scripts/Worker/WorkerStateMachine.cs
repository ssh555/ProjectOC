using ML.Engine.FSM;
using ML.Engine.Manager;
using ProjectOC.MissionNS;
using ProjectOC.ProductionNodeNS;
using UnityEditor.Experimental.GraphView;

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
        /// 值班处的ID编号，-1表示未值班
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
        /// 值班处的原料是否充足
        /// 值班处为空或者没配方默认原料充足
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
        /// 值班处是否堆积满
        /// 节点为空默认为没有堆积满
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
        /// 体力是否低于工作阈值
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
        /// 体力是否高于休息阈值
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
        /// 搬运任务
        /// </summary>
        public MissionTransport Mission;
        /// <summary>
        /// 当前时段的安排
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
        /// 休息是否完成
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
            // 连接边
            // 设置初始状态
        }
    }
}

