using ML.Engine.FSM;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateRelaxing : State
    {
        public WorkerStateRelaxing(string name) : base(name) { }
        private ML.Engine.Timer.CounterDownTimer timer;
        private ML.Engine.Timer.CounterDownTimer TimerForRandomWalk
        {
            get
            {
                if (timer == null)
                {
                    timer = new ML.Engine.Timer.CounterDownTimer(2f, false, false);
                }
                return timer;
            }
        }

        public override void ConfigState()
        {
            BindEnterAction
            (
                ((machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        workerMachine.Worker.Status = Status.Relaxing;
                    }
                })    
            );

            BindUpdateAction
            (
                ((machine, state) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        var worker = workerMachine.Worker;

                        if (worker.APCurrent < worker.APRelaxThreshold && !worker.HaveRestaurantSeat)
                        {
                            ManagerNS.LocalGameManager.Instance.RestaurantManager.AddWorker(worker);
                        }
                        else
                        {
                            ManagerNS.LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
                        }

                        if (!worker.HaveDestination && !worker.HaveRestaurantSeat && !worker.HaveTransport)
                        {
                            if (worker.HaveHome && !worker.GetContainer(WorkerContainerType.Home).IsArrive)
                            {
                                worker.SetDestination(worker.GetContainer(WorkerContainerType.Home).GetTransform().position, worker.GetContainer(WorkerContainerType.Home).OnArriveEvent, WorkerContainerType.Home);
                            }
                            else if (!worker.HaveHome && (timer == null || timer.IsStoped))
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
                                if (positions.Count > 0)
                                {
                                    var pos = positions[UnityEngine.Random.Range(0, positions.Count)];
                                    pos = new UnityEngine.Vector3(pos.x, pos.y-0.25f, pos.z);
                                    worker.SetDestination(pos, threshold:4f);
                                }
                                TimerForRandomWalk.Reset(2f);
                            }
                        }
                    }
                })
            );

            BindExitAction
            (
                ((machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        var worker = workerMachine.Worker;
                        ManagerNS.LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
                        worker.ClearDestination();
                        if (worker.HaveProNode)
                        {
                            worker.SetDestination(worker.GetContainer(WorkerContainerType.Work).GetTransform().position, 
                                worker.GetContainer(WorkerContainerType.Work).OnArriveEvent, WorkerContainerType.Work);
                        }
                    }
                })
            ); 
        }
    }
}