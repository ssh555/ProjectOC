using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    public class FreezeFrameAbility : IFreezeFrame
    {
        /// <summary>
        /// 内部计时器 => 用于卡肉的持续时间
        /// </summary>
        protected Timer.CounterDownTimer timer;

        /// <summary>
        /// 影响的动画机
        /// </summary>
        public Animator animator { get; set; }

        /// <summary>
        /// 当前是否处于卡肉|顿帧状态
        /// </summary>
        public bool IsApplying { get; protected set; } = false;

        /// <summary>
        /// 开始卡肉顿帧时的触发事件
        /// </summary>
        public event System.Action<FreezeFrameParams> OnStartFreezeFrame;

        /// <summary>
        /// 初始化计时器
        /// </summary>
        public FreezeFrameAbility()
        {
            timer = new Timer.CounterDownTimer(1, false, false);
            timer.OnEndEvent += Timer_OnEndEvent;
        }

        /// <summary>
        /// 计时器计时结束时事件 => 结束卡肉
        /// </summary>
        private void Timer_OnEndEvent()
        {
            this.__InternalEndFreezeFrame__();
        }

        /// <summary>
        /// 应用卡肉
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
        /// 结束卡肉
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
        /// 继承后需更改的应用卡肉函数
        /// </summary>
        /// <param name="freezeFrameParams"></param>
        protected virtual void __InternalApplyFreezeFrame__(FreezeFrameParams freezeFrameParams)
        {
            this.animator.speed = freezeFrameParams.timeScale;
        }

        /// <summary>
        /// 继承后需更改的结束卡肉函数
        /// </summary>
        protected virtual void __InternalEndFreezeFrame__()
        {
            this.IsApplying = false;
            this.animator.speed = 1;
        }
    }
}

