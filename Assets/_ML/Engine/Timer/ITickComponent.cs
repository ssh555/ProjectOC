using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ML.Engine.Timer
{
    /// <summary>
    /// 需调用 TickManager 注册才能使用
    /// </summary>
    public interface ITickComponent
    {
        /// <summary>
        /// 优先级，只能由 TickManager赋值
        /// </summary>
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        /// <summary>
        /// 需实现的每帧 Tick 函数
        /// </summary>
        /// <param name="deltatime"></param>
        public virtual void Tick(float deltatime)
        {

        }

        public virtual void FixedTick(float deltatime)
        {

        }

        public virtual void LateTick(float deltatime)
        {

        }

        public void DisposeTick()
        {
            if(Manager.GameManager.Instance != null && Manager.GameManager.Instance.TickManager != null)
            {
                Manager.GameManager.Instance.TickManager.UnregisterTick(this);
                Manager.GameManager.Instance.TickManager.UnregisterLateTick(this);
                Manager.GameManager.Instance.TickManager.UnregisterFixedTick(this);
            }

        }
    }

}
