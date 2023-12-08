using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateWorkingTransport : State
    {
        // 搬运任务，为null表示没有任务
        //CarryMission Mission => 引用machine
        // 搬运的物品的ID
        // 为-1表示为null
        // int ItemID => 根据machine的PDID引用
        // 这次搬运的数量
        // 数量由Worker的属性控制，可在这也加上，或者就完全放在Worker的属性里面
        // 根据这个值，更新worker基础属性的当前负重
        //int CurrentCount;

        public WorkerStateWorkingTransport(string name) : base(name)
        {

        }

        public override void ConfigState()
        {
            this.BindUpdateAction((machine, state) => {
                // 控制搬运流程(开始 - 搬运 - 任务结束)
            });
        }
    }
}