using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    public class FreezeFrameAbility : IFreezeFrame
    {
        /// <summary>
        /// �ڲ���ʱ�� => ���ڿ���ĳ���ʱ��
        /// </summary>
        protected Timer.CounterDownTimer timer;

        /// <summary>
        /// Ӱ��Ķ�����
        /// </summary>
        public Animator animator { get; set; }

        /// <summary>
        /// ��ǰ�Ƿ��ڿ���|��֡״̬
        /// </summary>
        public bool IsApplying { get; protected set; } = false;

        /// <summary>
        /// ��ʼ�����֡ʱ�Ĵ����¼�
        /// </summary>
        public event System.Action<FreezeFrameParams> OnStartFreezeFrame;

        /// <summary>
        /// ��ʼ����ʱ��
        /// </summary>
        public FreezeFrameAbility()
        {
            timer = new Timer.CounterDownTimer(1, false, false);
            timer.OnEndEvent += Timer_OnEndEvent;
        }

        /// <summary>
        /// ��ʱ����ʱ����ʱ�¼� => ��������
        /// </summary>
        private void Timer_OnEndEvent()
        {
            this.__InternalEndFreezeFrame__();
        }

        /// <summary>
        /// Ӧ�ÿ���
        /// </summary>
        /// <param name="freezeFrameParams"></param>
        public void ApplyFreezeFrame(FreezeFrameParams freezeFrameParams)
        {
            this.IsApplying = true;
            this.timer.Reset(freezeFrameParams.duration);
            this.__InternalApplyFreezeFrame__(freezeFrameParams);
            this.OnStartFreezeFrame?.Invoke(freezeFrameParams);
        }

        /// <summary>
        /// ��������
        /// </summary>
        public void EndFreezeFrame()
        {
            if(this.IsApplying == false)
            {
                return;
            }
            this.IsApplying = false;
            this.timer.End();
            this.__InternalEndFreezeFrame__();
        }

        /// <summary>
        /// �̳к�����ĵ�Ӧ�ÿ��⺯��
        /// </summary>
        /// <param name="freezeFrameParams"></param>
        protected virtual void __InternalApplyFreezeFrame__(FreezeFrameParams freezeFrameParams)
        {
            this.animator.speed = freezeFrameParams.timeScale;
        }

        /// <summary>
        /// �̳к�����ĵĽ������⺯��
        /// </summary>
        protected virtual void __InternalEndFreezeFrame__()
        {
            this.IsApplying = false;
            this.animator.speed = 1;
        }
    }
}

