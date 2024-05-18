using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateBan : State
    {
        public WorkerStateBan(string name) : base(name) { }
        public override void ConfigState()
        {
            BindEnterAction
            (
                (machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        var worker = workerMachine.Worker;
                        worker.Status = Status.Ban;
                        worker.ClearDestination();
                        worker.ContainerDict[WorkerContainerType.Work]?.RemoveWorker();
                        worker.ContainerDict[WorkerContainerType.Relax]?.RemoveWorker();
                        worker.ContainerDict[WorkerContainerType.Home]?.TempRemoveWorker();
                        worker.FeatSeat.OnArriveEvent(worker);
                    }
                }
            );
        }
    }
}