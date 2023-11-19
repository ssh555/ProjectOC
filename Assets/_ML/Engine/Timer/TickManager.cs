using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ML.Engine.Timer
{
    /// <summary>
    /// 控制整个系统的Tick => 不包括计时器
    /// </summary>
    public sealed class TickManager : Manager.GlobalManager.IGlobalManager
    {
        private SortedDictionary<int, List<ITickComponent>> tickComponents;
        private SortedDictionary<int, List<ITickComponent>> fixedTickComponents;
        private SortedDictionary<int, List<ITickComponent>> lateTickComponents;

        // 待移除队列
        private List<ITickComponent> removeTick;
        private List<ITickComponent> removeFixedTick;
        private List<ITickComponent> removeLateTick;

        public TickManager()
        {
            this.tickComponents = new SortedDictionary<int, List<ITickComponent>>();
            this.fixedTickComponents = new SortedDictionary<int, List<ITickComponent>>();
            this.lateTickComponents = new SortedDictionary<int, List<ITickComponent>>();

            this.removeTick = new List<ITickComponent>();
            this.removeFixedTick = new List<ITickComponent>();
            this.removeLateTick = new List<ITickComponent>();
        }   

        #region Tick
        public void Tick(float deltatime)
        {
            foreach(var tickkv in this.tickComponents)
            {
                foreach(var tick in tickkv.Value)
                {
                    tick.Tick(deltatime);
                }
            }

            foreach(var tick in this.removeTick)
            {
                this.tickComponents[tick.tickPriority].Remove(tick);
            }
            this.removeTick.Clear();
        }

        public void FixedTick(float deltatime)
        {
            foreach (var tickkv in this.fixedTickComponents)
            {
                foreach (var tick in tickkv.Value)
                {
                    tick.FixedTick(deltatime);
                }
            }

            foreach (var tick in this.removeFixedTick)
            {
                this.fixedTickComponents[tick.fixedTickPriority].Remove(tick);
            }
            this.removeFixedTick.Clear();
        }

        public void LateTick(float deltatime)
        {
            foreach (var tickkv in this.lateTickComponents)
            {
                foreach (var tick in tickkv.Value)
                {
                    tick.LateTick(deltatime);
                }
            }
            foreach (var tick in this.removeLateTick)
            {
                this.lateTickComponents[tick.lateTickPriority].Remove(tick);
            }
            this.removeLateTick.Clear();
        }
        #endregion

        #region 注册管理
        #region Tick
        /// <summary>
        /// 注册Tick
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="tickComponent"></param>
        public bool RegisterTick(int priority, ITickComponent tickComponent)
        {
            if (this.tickComponents.ContainsKey(tickComponent.tickPriority) && this.tickComponents[tickComponent.tickPriority].Contains(tickComponent))
            {
                return false;
            }
            tickComponent.tickPriority = priority;
            if (this.tickComponents.ContainsKey(priority))
            {
                this.tickComponents[priority].Add(tickComponent);
                return true;
            }
            else
            {
                this.tickComponents.Add(priority, new List<ITickComponent>());
                this.tickComponents[priority].Add(tickComponent);
                return true;
            }
        }
        public ITickComponent UnregisterTick(ITickComponent tickComponent)
        {
            if(this.tickComponents.ContainsKey(tickComponent.tickPriority) && this.tickComponents[tickComponent.tickPriority].Contains(tickComponent))
            {
                this.removeTick.Add(tickComponent);
                return tickComponent;
            }
            return null;
        }
        #endregion

        #region FixedTick
        /// <summary>
        /// 注册 FixedTick
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="tickComponent"></param>
        /// <returns></returns>
        public bool RegisterFixedTick(int priority, ITickComponent tickComponent)
        {
            if (this.fixedTickComponents.ContainsKey(tickComponent.fixedTickPriority) && this.fixedTickComponents[tickComponent.fixedTickPriority].Contains(tickComponent))
            {
                return false;
            }
            tickComponent.fixedTickPriority = priority;
            if (this.fixedTickComponents.ContainsKey(priority))
            {
                this.fixedTickComponents[priority].Add(tickComponent);
                return true;
            }
            else
            {
                this.fixedTickComponents.Add(priority, new List<ITickComponent>());
                this.fixedTickComponents[priority].Add(tickComponent);
                return true;
            }
        }
        public ITickComponent UnregisterFixedTick(ITickComponent tickComponent)
        {
            if (this.fixedTickComponents.ContainsKey(tickComponent.fixedTickPriority) && this.fixedTickComponents[tickComponent.fixedTickPriority].Contains(tickComponent))
            {
                this.removeFixedTick.Add(tickComponent);
                return tickComponent;
            }
            return null;
        }
        #endregion

        #region LateTick
        /// <summary>
        /// 注册 LateTick
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="tickComponent"></param>
        /// <returns></returns>
        public bool RegisterLateTick(int priority, ITickComponent tickComponent)
        {
            if (this.lateTickComponents.ContainsKey(tickComponent.lateTickPriority) && this.lateTickComponents[tickComponent.lateTickPriority].Contains(tickComponent))
            {
                return false;
            }
            tickComponent.lateTickPriority = priority;
            if (this.lateTickComponents.ContainsKey(priority))
            {
                this.lateTickComponents[priority].Add(tickComponent);
                return true;
            }
            else
            {
                this.lateTickComponents.Add(priority, new List<ITickComponent>());
                this.lateTickComponents[priority].Add(tickComponent);
                return true;
            }
        }
        public ITickComponent UnregisterLateTick(ITickComponent tickComponent)
        {
            if (this.lateTickComponents.ContainsKey(tickComponent.lateTickPriority) && this.lateTickComponents[tickComponent.lateTickPriority].Contains(tickComponent))
            {
                this.removeLateTick.Add(tickComponent);
                return tickComponent;
            }
            return null;
        }
        #endregion
        #endregion
    }

}
