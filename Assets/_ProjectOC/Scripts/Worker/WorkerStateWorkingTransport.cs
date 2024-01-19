using ML.Engine.FSM;
using System;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateWorkingTransport : State
    {
        public WorkerStateWorkingTransport(string name) : base(name)
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
                        worker.Status = Status.Working;

                        worker.SetDestination(worker.Transport.Source.GetTransform(), Transport_Source_Action);
                    }
                }
            );
        }

        private void Transport_Source_Action(Worker worker)
        {
            worker.Transport.PutOutSource();
            worker.SetDestination(worker.Transport.Target.GetTransform(), Transport_Target_Action);
        }
        private void Transport_Target_Action(Worker worker)
        {
            worker.Transport.PutInTarget();
            worker.ClearDestination();
        }
    }
}