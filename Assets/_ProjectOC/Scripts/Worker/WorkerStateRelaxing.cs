using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateRelaxing : State
    {
        public WorkerStateRelaxing(string name) : base(name) { }
        public override void ConfigState()
        {
            this.BindEnterAction
            (
                (machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        var worker = workerMachine.Worker;
                        worker.Status = Status.Relaxing;

                        worker.ClearDestination();
                        if (worker.HasProNode && worker.ProNode.IsWorkerArrive)
                        {
                            worker.RecoverLastPosition();
                            worker.ProNode.IsWorkerArrive = false;
                        }
                    }
                }    
            );

            this.BindUpdateAction
            (
                (machine, state) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        var worker = workerMachine.Worker;
                        if (worker.APCurrent < worker.APRelaxThreshold && ! worker.HasRestaurant)
                        {
                            ManagerNS.LocalGameManager.Instance.RestaurantManager.AddWorker(worker);
                        }
                        else
                        {
                            ManagerNS.LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
                        }

                        if (!worker.HasDestination && !worker.HasRestaurant)
                        {
                            if (worker.HasHome && !worker.Home.HasArrive)
                            {
                                worker.SetDestination(worker.Home.transform.position, OnArriveHomeEvent);
                            }
                            else if (!worker.HasHome)
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
                        var worker = workerMachine.Worker;
                        ManagerNS.LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
                        worker.ClearDestination();
                        worker.Home?.UnBindWorker();
                        if (worker.HasProNode)
                        {
                            worker.SetDestination(worker.ProNode.GetTransform().position, OnArriveProNodeEvent);
                        }
                    }
                }
            ); 
        }

        private void OnArriveHomeEvent(Worker worker)
        {
            worker.Agent.enabled = false;
            worker.LastPosition = worker.transform.position;
            worker.transform.position = worker.Home.transform.position + new UnityEngine.Vector3(0, 2f, 0);
            worker.Home.HasArrive = true;
        }

        private void OnArriveProNodeEvent(Worker worker)
        {
            worker.Agent.enabled = false;
            worker.LastPosition = worker.transform.position;
            worker.transform.position = worker.ProNode.WorldProNode.transform.position + new UnityEngine.Vector3(0, 2f, 0);
            worker.ProNode.IsWorkerArrive = true;
            worker.ProNode.StartProduce();
        }
    }
}