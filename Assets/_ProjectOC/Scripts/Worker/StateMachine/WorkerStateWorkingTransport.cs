using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateWorkingTransport : State
    {
        public WorkerStateWorkingTransport(string name) : base(name) { }
        public override void ConfigState()
        {
            this.BindEnterAction
            (
                (machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        workerMachine.Worker.Status = Status.Working;
                    }
                }
            );
        }
    }
}