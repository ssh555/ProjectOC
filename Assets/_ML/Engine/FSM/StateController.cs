using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.FSM
{
    [System.Serializable]
    public class StateController : MonoBehaviour
    {
        [ShowInInspector, ReadOnly]
        public StateMachine Machine { get; protected set; }

        protected bool isRunning = true;

        public virtual void SetStateMachine(StateMachine stateMachine)
        {
            this.Machine = stateMachine;
        }

        protected virtual void Update()
        {
            if (this.isRunning && this.Machine != null)
            {
                this.Machine.Update(Time.deltaTime);
                this.Machine.UpdateState(this);
            }
        }

        protected virtual void LateUpdate()
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
            this.Machine.ResetState();
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


