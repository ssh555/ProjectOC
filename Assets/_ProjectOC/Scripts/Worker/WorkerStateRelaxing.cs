using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateRelaxing : State
    {
        public WorkerStateRelaxing(string name) : base(name)
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
                        workerMachine.Worker.Status = Status.Relaxing;
                        if (!workerMachine.Worker.HasRestaurant)
                        {
                            if (workerMachine.Worker.HasHome)
                            {
                                workerMachine.Worker.SetDestination(workerMachine.Worker.Home.transform.position);
                            }
                            else if (!workerMachine.Worker.HasDestination)
                            {
                                // Ëæ»úÓÎ×ß workerMachine.Worker.SetDestination();
                            }
                        }
                    }
                }    
            );
            this.BindExitAction
            (
                (machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        
                    }
                }
            ); 
        }
    }
}