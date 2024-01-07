using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ML.Engine.Timer
{
    /// <summary>
    /// ����� TickManager ע�����ʹ��
    /// </summary>
    public interface ITickComponent
    {
        /// <summary>
        /// ���ȼ���ֻ���� TickManager��ֵ
        /// </summary>
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        /// <summary>
        /// ��ʵ�ֵ�ÿ֡ Tick ����
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
