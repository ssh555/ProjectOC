using ML.Engine.FSM;
using ML.Engine.Timer;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateRelaxing : State
    {
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
                        this.Timer.OnEndEvent += () =>
                        {
                            if (workerMachine.Worker.APMax * 0.01 >= 1)
                            {
                                workerMachine.Worker.AlterAP((int)(workerMachine.Worker.APMax * 0.01));
                            }
                            else
                            {
                                workerMachine.Worker.AlterAP(1);
                            }
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