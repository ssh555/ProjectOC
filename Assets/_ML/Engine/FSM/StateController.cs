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
                // ��ǰ֡һֱ����״̬��ֱ����ǰ֡״̬���ٸı�
                while (this.Machine.LateChangeState(this)) continue;
#endif
            }
        }

        /// <summary>
        /// ֹͣ���Ƶ�״̬�� �� ��״̬����Ϊ default
        /// </summary>
        public void Stop()
        {
            this.isRunning = false;
            this.Machine.ResetState();
        }

        /// <summary>
        /// ����״̬��
        /// </summary>
        public void ReStart()
        {
            this.isRunning = true;
            Machine.CurrentState.InvokeEnterAction(Machine, null);
        }

    }
}


