using ML.Engine.FSM;
using System.Collections.Generic;


namespace ProjectOC.WorkerNS
{
    public class WorkerStateRelaxing : State
    {
        public WorkerStateRelaxing(string name) : base(name) { }
        private ML.Engine.Timer.CounterDownTimer TimerForRandomWalk;

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

                        TimerForRandomWalk = new ML.Engine.Timer.CounterDownTimer(2f, false, false);
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
                            else if (!worker.HasHome && (TimerForRandomWalk == null || TimerForRandomWalk.IsStoped))
                            {
                                // Ëæ»úÓÎ×ß
                                List<UnityEngine.Vector3> positions = new List<UnityEngine.Vector3>();
                                foreach (var core in ManagerNS.LocalGameManager.Instance.BuildPowerIslandManager.powerCores)
                                {
                                    if (core.GetType() == typeof(LandMassExpand.BuildPowerCore))
                                    {
                                        positions.Add(core.transform.position);
                                    }
                                }
                                System.Random random = new System.Random();
                                worker.SetDestination(positions[random.Next(0, positions.Count)]);
                                TimerForRandomWalk.Reset(2f);
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
                        if (worker.HasRestaurant)
                        {
                            worker.Restaurant.RemoveWorker(worker);
                        }
                        if (worker.HasHome && worker.Home.HasArrive)
                        {
                            worker.RecoverLastPosition();
                            worker.Home.HasArrive = false;
                        }
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