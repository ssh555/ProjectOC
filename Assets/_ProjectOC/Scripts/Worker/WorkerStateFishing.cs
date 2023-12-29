using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateFishing : State
    {
        public WorkerStateFishing(string name) : base(name)
        {

        }
        public override void ConfigState()
        {
            this.BindEnterAction
            (
                (machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        workerMachine.Worker.Status = Status.Fishing;
                    }
                }
            );
        }
    }
}