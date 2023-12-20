using ML.Engine.FSM;
using ML.Engine.Manager;
using ML.Engine.Timer;
using ProjectOC.ManagerNS;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateRelaxing : State
    {
        public DispatchTimeManager TimeManager { get { return GameManager.Instance.GetLocalManager<DispatchTimeManager>(); } }
        /// <summary>
        /// 计时器
        /// </summary>
        protected CounterDownTimer timer;
        /// <summary>
        /// 计时器
        /// </summary>
        protected CounterDownTimer Timer
        {
            get
            {
                if (timer == null)
                {
                    timer = new CounterDownTimer(1f, true, false);
                }
                return timer;
            }
        }

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
                        workerMachine.Worker.IsOnDuty = false;

                        this.Timer.OnEndEvent += () =>
                        {
                            workerMachine.Worker.AlterAP((int)(workerMachine.Worker.APMax * 0.01));
                        };
                        this.Timer.Start();
                    }
                }    
            );
            this.BindExitAction
            (
                (machine, state1, state2) =>
                {
                    if (machine is WorkerStateMachine workerMachine && workerMachine.Worker != null)
                    {
                        this.Timer.End();
                        this.timer = null;
                    }
                }
            ); 
        }
    }
}