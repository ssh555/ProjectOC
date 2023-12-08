using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateRelaxing : State
    {
        /// <summary>
        /// 是否完成休息
        /// </summary>
        public bool IsCompleteRelax { get; private set; } = false;

        public WorkerStateRelaxing(string name) : base(name)
        {
        }
        public override void  ConfigState()
        {
            this.BindEnterAction
            (
                (machine, state1, state2) =>
                {
                    this.IsCompleteRelax = false;
                    //DispatchManager加入事件监听: 结束休息时段并且体力高于阈值，则置为true;
                    //timer (loop) => 休息时暂定每秒回复1 %
                }    
            );
            this.BindEnterAction
            (
                (machine, state1, state2) =>
                {
                    this.IsCompleteRelax = false;
                    //移除Enter加入的Event
                    //结束timer
                }
            );
            this.BindUpdateAction
            (
                (machine, state) =>
                {
                    // TODO:
                    //① 不处于休息时段，则监听体力是否回复至第一阈值，是则置isCompleteRelax为true
                    //② 体力回复至第二阈值,也置为true
                }
            );
        }
    }
}