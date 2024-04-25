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
                        //var worker = workerMachine.Worker;
                        //worker.Status = Status.Relaxing;

                        //worker.ClearDestination();
                        //if (worker.HaveProNode && worker.ProNode.IsArrive)
                        //{
                        //    worker.RecoverLastPosition();
                        //    worker.ProNode.IsArrive = false;
                        //}

                        //TimerForRandomWalk = new ML.Engine.Timer.CounterDownTimer(2f, false, false);
                    }
                })    
            );

            BindUpdateAction
            (
                (System.Action<StateMachine, State>)((machine, state) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        //var worker = workerMachine.Worker;

                        //if (worker.APCurrent < worker.APRelaxThreshold && ! worker.HaveRestaurant)
                        //{
                        //    ManagerNS.LocalGameManager.Instance.RestaurantManager.AddWorker(worker);
                        //}
                        //else
                        //{
                        //    ManagerNS.LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
                        //}

                        //if (!worker.HaveDestination && !worker.HaveRestaurant)
                        //{
                        //    if (worker.HaveHome && !worker.Home.IsArrive)
                        //    {
                        //        worker.SetDestination(worker.Home.transform.position, OnArriveHomeEvent);
                        //    }
                        //    else if (!worker.HaveHome && (TimerForRandomWalk == null || TimerForRandomWalk.IsStoped))
                        //    {
                        //        // Ëæ»úÓÎ×ß
                        //        List<UnityEngine.Vector3> positions = new List<UnityEngine.Vector3>();
                        //        foreach (var core in ManagerNS.LocalGameManager.Instance.BuildPowerIslandManager.powerCores)
                        //        {
                        //            if (core.GetType() == typeof(LandMassExpand.BuildPowerCore))
                        //            {
                        //                positions.Add(core.transform.position);
                        //            }
                        //        }
                        //        System.Random random = new System.Random();
                        //        worker.SetDestination(positions[random.Next(0, positions.Count)]);
                        //        TimerForRandomWalk.Reset(2f);
                        //    }
                        //}
                    }
                })
            );

            BindExitAction
            (
                (System.Action<StateMachine, State, State>)((machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        //var worker = workerMachine.Worker;
                        //ManagerNS.LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
                        //worker.ClearDestination();
                        //if (worker.HaveRestaurant)
                        //{
                        //    worker.RelaxPlace.RemoveWorker();
                        //}
                        //if (worker.HaveHome && worker.Home.IsArrive)
                        //{
                        //    worker.RecoverLastPosition();
                        //    worker.Home.IsArrive = false;
                        //}
                        //if (worker.HaveProNode)
                        //{
                        //    worker.SetDestination(worker.ProNode.GetTransform().position, worker.ProNode.ArriveProNodeAction);
                        //}
                    }
                })
            ); 
        }

        //private void OnArriveHomeEvent(Worker worker)
        //{
        //    worker.Agent.enabled = false;
        //    worker.LastPosition = worker.transform.position;
        //    worker.transform.position = worker.Home.transform.position + new UnityEngine.Vector3(0, 2f, 0);
        //    worker.Home.IsArrive = true;
        //}
    }
}