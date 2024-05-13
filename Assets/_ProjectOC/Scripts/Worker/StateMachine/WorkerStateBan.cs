using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateBan : State
    {
        public WorkerStateBan(string name) : base(name) { }
        public override void ConfigState()
        {
            this.BindEnterAction
            (
                (machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        workerMachine.Worker.Status = Status.Ban;
                    }
                }
            );
        }
    }
}