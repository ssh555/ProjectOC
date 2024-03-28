using Sirenix.OdinInspector;
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

        // 待注册队列
        private List<ITickComponent> addTick;
        private List<ITickComponent> addFixedTick;
        private List<ITickComponent> addLateTick;
        // 待移除队列
        private List<ITickComponent> removeTick;
        private List<ITickComponent> removeFixedTick;

        [ShowInInspector]
        private List<ITickComponent> removeLateTick;

        public float TimeScale = 1;

        public TickManager()
        {
            this.tickComponents = new SortedDictionary<int, List<ITickComponent>>();
            this.fixedTickComponents = new SortedDictionary<int, List<ITickComponent>>();
            this.lateTickComponents = new SortedDictionary<int, List<ITickComponent>>();


            this.addTick = new List<ITickComponent>();
            this.addFixedTick = new List<ITickComponent>();
            this.addLateTick = new List<ITickComponent>();

            this.removeTick = new List<ITickComponent>();
            this.removeFixedTick = new List<ITickComponent>();
            this.removeLateTick = new List<ITickComponent>();
        }   

        #region Tick
        public void UpdateTickComponentList()
        {
            foreach (var tick in this.addTick)
            {
                this.tickComponents[tick.tickPriority].Add(tick);
            }
            this.addTick.Clear();
            foreach (var tick in this.removeTick)
            {
                this.tickComponents[tick.tickPriority].Remove(tick);
            }
            this.removeTick.Clear();
            foreach (var list in this.tickComponents.Values)
            {
                // 移除所有null项，不产生额外开销
                int writeIndex = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        if (writeIndex != i)
                        {
                            list[writeIndex] = list[i];
                        }
                        writeIndex++;
                    }
                }
                list.RemoveRange(writeIndex, list.Count - writeIndex);
            }
        }
        public void UpdateFixedTickComponentList()
        {
            foreach (var tick in this.addFixedTick)
            {
                this.fixedTickComponents[tick.fixedTickPriority].Add(tick);
            }
            this.addFixedTick.Clear();
            foreach (var tick in this.removeFixedTick)
            {
                this.fixedTickComponents[tick.fixedTickPriority].Remove(tick);
            }
            this.removeFixedTick.Clear();
            foreach (var list in this.fixedTickComponents.Values)
            {
                // 移除所有null项，不产生额外开销
                int writeIndex = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        if (writeIndex != i)
                        {
                            list[writeIndex] = list[i];
                        }
                        writeIndex++;
                    }
                }
                list.RemoveRange(writeIndex, list.Count - writeIndex);
            }

        }
        public void UpdateLateTickComponentList()
        {
            foreach (var tick in this.addLateTick)
            {
                this.lateTickComponents[tick.lateTickPriority].Add(tick);
            }
            this.addLateTick.Clear();
            foreach (var tick in this.removeLateTick)
            {
                this.lateTickComponents[tick.lateTickPriority].Remove(tick);
            }
            this.removeLateTick.Clear();
            foreach (var list in this.lateTickComponents.Values)
            {
                // 移除所有null项，不产生额外开销
                int writeIndex = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        if (writeIndex != i)
                        {
                            list[writeIndex] = list[i];
                        }
                        writeIndex++;
                    }
                }
                list.RemoveRange(writeIndex, list.Count - writeIndex);
            }
        }

        public void Tick(float deltatime)
        {
            deltatime *= TimeScale;
            foreach (var tickkv in this.tickComponents)
            {
                foreach (var tick in tickkv.Value)
                {
                    tick.Tick(deltatime);
                }
            }
        }

        public void FixedTick(float deltatime)
        {
            deltatime *= TimeScale;
            foreach (var tickkv in this.fixedTickComponents)
            {
                foreach (var tick in tickkv.Value)
                {
                    tick.FixedTick(deltatime);
                }
            }
        }

        public void LateTick(float deltatime)
        {
            deltatime *= TimeScale;
            foreach (var tickkv in this.lateTickComponents)
            {
                foreach (var tick in tickkv.Value)
                {
                    tick.LateTick(deltatime);
                }
            }
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
            if(this.addTick.Contains(tickComponent))
            {
                return false;
            }
            if (this.tickComponents.ContainsKey(tickComponent.tickPriority) && this.tickComponents[tickComponent.tickPriority].Contains(tickComponent))
            {
                return false;
            }
            tickComponent.tickPriority = priority;
            if (this.tickComponents.ContainsKey(priority))
            {
                this.addTick.Add(tickComponent);
                return true;
            }
            else
            {
                this.tickComponents.Add(priority, new List<ITickComponent>());
                this.addTick.Add(tickComponent);
                return true;
            }
        }
        public bool UnregisterTick(ITickComponent tickComponent)
        {
            if (this.addTick.Contains(tickComponent))
            {
                this.addTick.Remove(tickComponent);
                return true;
            }
            if (this.tickComponents.ContainsKey(tickComponent.tickPriority) && this.tickComponents[tickComponent.tickPriority].Contains(tickComponent) && !this.removeTick.Contains(tickComponent))
            {
                this.removeTick.Add(tickComponent);
                return true;
            }
            return false;
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
            if (this.addFixedTick.Contains(tickComponent))
            {
                return false;
            }
            if (this.fixedTickComponents.ContainsKey(tickComponent.fixedTickPriority) && this.fixedTickComponents[tickComponent.fixedTickPriority].Contains(tickComponent))
            {
                return false;
            }
            tickComponent.fixedTickPriority = priority;
            if (this.fixedTickComponents.ContainsKey(priority))
            {
                this.addFixedTick.Add(tickComponent);
                return true;
            }
            else
            {
                this.fixedTickComponents.Add(priority, new List<ITickComponent>());
                this.addFixedTick.Add(tickComponent);
                return true;
            }
        }
        public bool UnregisterFixedTick(ITickComponent tickComponent)
        {
            if(this.addFixedTick.Contains(tickComponent))
            {
                this.addFixedTick.Remove(tickComponent);
                return true;
            }
            if (this.fixedTickComponents.ContainsKey(tickComponent.fixedTickPriority) && this.fixedTickComponents[tickComponent.fixedTickPriority].Contains(tickComponent) && !this.removeFixedTick.Contains(tickComponent))
            {
                this.removeFixedTick.Add(tickComponent);
                return true;
            }
            return false;
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
            if (this.addLateTick.Contains(tickComponent))
            {
                return false;
            }
            if (this.lateTickComponents.ContainsKey(tickComponent.lateTickPriority) && this.lateTickComponents[tickComponent.lateTickPriority].Contains(tickComponent))
            {
                return false;
            }
            tickComponent.lateTickPriority = priority;
            if (this.lateTickComponents.ContainsKey(priority))
            {
                this.addLateTick.Add(tickComponent);
                return true;
            }
            else
            {
                this.lateTickComponents.Add(priority, new List<ITickComponent>());
                this.addLateTick.Add(tickComponent);
                return true;
            }
        }
        public bool UnregisterLateTick(ITickComponent tickComponent)
        {
            if (this.addLateTick.Contains(tickComponent))
            {
                this.addLateTick.Remove(tickComponent);
                return true;
            }
            if (this.lateTickComponents.ContainsKey(tickComponent.lateTickPriority) && this.lateTickComponents[tickComponent.lateTickPriority].Contains(tickComponent) && !this.removeLateTick.Contains(tickComponent))
            {
                this.removeLateTick.Add(tickComponent);
                return true;
            }
            return false;
        }
        #endregion
        #endregion
    }

}
