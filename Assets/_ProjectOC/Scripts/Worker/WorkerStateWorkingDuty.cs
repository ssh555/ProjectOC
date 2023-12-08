using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateWorkingDuty : State
    {
        // 值班处的ID编号，-1表示未值班
        // string DutyDepartmentID => 引用machine
        public WorkerStateWorkingDuty(string name) : base(name)
        {

        }

        public override void ConfigState()
        {
            this.BindUpdateAction((machine, state) => {
                // 开始前往对应的生产节点
                // 到达后开始运作
            });
        }
    }
}