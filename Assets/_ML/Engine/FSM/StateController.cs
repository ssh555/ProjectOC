using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.FSM
{
    /// <summary>
    /// 不使用时记得调用 DisposeTick
    /// </summary>
    [System.Serializable]
    public class StateController : Timer.ITickComponent
    {
        [ShowInInspector, ReadOnly]
        public StateMachine Machine { get; protected set; }
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        protected bool isRunning = true;

        public StateController(int priority)
        {
            Manager.GameManager.Instance.TickManager.RegisterTick(priority, this);
            Manager.GameManager.Instance.TickManager.RegisterLateTick(priority, this);
        }

        public virtual void SetStateMachine(StateMachine stateMachine)
        {
            this.Machine = stateMachine;
            Debug.Log("SetStateMachine " + this.Machine);
        }

        public virtual void Tick(float deltatime)
        {
            if (this.isRunning && this.Machine != null)
            {
                this.Machine.Update(deltatime);
                this.Machine.UpdateState(this);
            }
        }

        public virtual void LateTick(float deltatime)
        {
            if (this.isRunning && this.Machine != null)
            {
#if false
                this.Machine.LateChangeState(this);
#else
                // 当前帧一直更新状态，直至当前帧状态不再改变
                while (this.Machine.LateChangeState(this)) continue;
#endif
            }
        }

        /// <summary>
        /// 停止控制的状态机 并 将状态设置为 default
        /// </summary>
        public void Stop()
        {
            this.isRunning = false;
            //TODO Machine is null
            this.Machine?.ResetState();
            Debug.Log(Manager.GameManager.Instance.TickManager.UnregisterTick(this));
            Debug.Log(Manager.GameManager.Instance.TickManager.UnregisterLateTick(this));
/*            Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            Manager.GameManager.Instance.TickManager.UnregisterLateTick(this);*/
        }

        /// <summary>
        /// 重启状态机
        /// </summary>
        public void ReStart()
        {
            this.isRunning = true;
            Machine.CurrentState.InvokeEnterAction(Machine, null);
        }
    }
}


