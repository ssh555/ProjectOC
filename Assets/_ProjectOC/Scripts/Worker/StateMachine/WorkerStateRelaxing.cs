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
            BindEnterAction
            (
                (System.Action<StateMachine, State, State>)((machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        workerMachine.Worker.Status = Status.Relaxing;
                        TimerForRandomWalk = new ML.Engine.Timer.CounterDownTimer(2f, false, false);
                    }
                })    
            );

            BindUpdateAction
            (
                (System.Action<StateMachine, State>)((machine, state) =>
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

                        if (!worker.HaveDestination && !worker.HaveRestaurantSeat)
                        {
                            if (worker.HaveHome && !worker.GetContainer(WorkerContainerType.Home).IsArrive)
                            {
                                worker.SetDestination(worker.GetContainer(WorkerContainerType.Home).GetTransform().position, worker.GetContainer(WorkerContainerType.Home).OnArriveEvent);
                            }
                            else if (!worker.HaveHome && (TimerForRandomWalk == null || TimerForRandomWalk.IsStoped))
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
                })
            );

            BindExitAction
            (
                (System.Action<StateMachine, State, State>)((machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        var worker = workerMachine.Worker;
                        ManagerNS.LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
                        worker.ClearDestination();
                        if (worker.HaveProNode)
                        {
                            worker.SetDestination(worker.GetContainer(WorkerContainerType.Work).GetTransform().position, worker.GetContainer(WorkerContainerType.Work).OnArriveEvent);
                        }
                    }
                })
            ); 
        }
    }
}