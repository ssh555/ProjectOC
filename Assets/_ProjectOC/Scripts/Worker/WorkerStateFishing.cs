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
                        Worker worker = workerMachine.Worker;
                        worker.Status = Status.Fishing;
                        if (worker.ProNode != null)
                        {
                            worker.SetDestination(worker.ProNode.GetTransform(), ProNode_Action);
                        }
                    }
                }
            );
        }

        public void ProNode_Action(Worker worker)
        {
            worker.ArriveProNode = true;
            worker.gameObject.SetActive(false);
        }
    }
}