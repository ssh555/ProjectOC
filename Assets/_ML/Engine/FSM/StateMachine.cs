using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.FSM
{
    [System.Serializable]
    public class StateMachine
    {
        /// <summary>
        /// 两个状态之间的单向连线 => 是否应该从箭尾进入箭头所指的状态
        /// </summary>
        /// <returns></returns>
        public delegate bool IsChangeState(StateMachine stateMachine, State curState);

        /// <summary>
        /// 状态节点
        /// </summary>
        [LabelText("状态节点"), ShowInInspector, ReadOnly]
        protected Dictionary<string, State> StateDict;

        /// <summary>
        /// Key => 此状态机中的一个状态的名称
        /// Value => Key状态对应的 <改变条件， 指向的状态的名称>
        /// </summary>
        protected Dictionary<string, Dictionary<string, IsChangeState>> StateGraph;
        
        [LabelText("当前状态"), ShowInInspector, ReadOnly]
        public string CurrentStateName { get
            {
                if (_currentState != null && this.StateDict.ContainsKey(_currentState.Name))
                {
                    return _currentState.Name;
                }
                return "";
            } }
        public State CurrentState => _currentState;
        private State _currentState = null;

        public string DefaultStateName { get; protected set; } = "";

        public StateMachine()
        {
            this.StateDict = new Dictionary<string, State>();
            this.StateGraph = new Dictionary<string, Dictionary<string, IsChangeState>>();
        }

        #region 节点及边的加入删除
        /// <summary>
        /// 加入状态节点
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool AddState(State state)
        {
            if (state.Name == "" || this.StateDict.ContainsKey(state.Name))
            {
                return false;
            }
            this.StateDict.Add(state.Name, state);
            state.stateMachine = this;
            return true;
        }
    
        /// <summary>
        /// 移除孤立的状态节点
        /// 若状态节点有指向，则移除失败，返回null
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public State RemoveState(string stateName)
        {
            // 是否有出边
            if (this.StateGraph.ContainsKey(stateName))
            {
                return null;
            }
            // 是否有入边
            foreach(var edge in this.StateGraph.Values)
            {
                if (edge.ContainsKey(stateName))
                {
                    return null;
                }
            }

            State state;
            
            if(this.StateDict.TryGetValue(stateName, out state))
            {
                state.stateMachine = null;
                return state;
            }
            return null;
        }

        /// <summary>
        /// 强制删除状态节点及其连接的边
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public State ForceRemoveState(string stateName)
        {
            State state;

            if (this.StateDict.TryGetValue(stateName, out state))
            {
                // 删除出边
                this.StateDict.Remove(stateName);
                // 删除入边
                foreach(var edge in this.StateGraph.Values)
                {
                    edge.Remove(stateName);
                }
                state.stateMachine = null;
                return state;
            }
            return null;
        }

        /// <summary>
        /// 连接两个节点
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        /// <param name="isChangeState"></param>
        /// <param name="IsCover">若边存在，是否覆盖</param>
        /// <returns></returns>
        public bool ConnectState(string fromState, string toState, IsChangeState isChangeState, bool IsCover = false)
        {
            // 节点均存在
            if(this.StateDict.ContainsKey(fromState) && this.StateDict.ContainsKey(toState))
            {
                if (this.StateGraph.ContainsKey(fromState))
                {
                    // from -> to 的边已存在 && 覆盖
                    if (this.StateGraph[fromState].ContainsKey(toState))
                    {
                        if (IsCover)
                        {
                            this.StateGraph[fromState][toState] = isChangeState;
                            return true;
                        }
                        return false;
                    }
                    // 边不存在
                    this.StateGraph[fromState].Add(toState, isChangeState);
                }
                else
                {
                    this.StateGraph.Add(fromState, new Dictionary<string, IsChangeState>());
                    this.StateGraph[fromState].Add(toState, isChangeState);
                }

            }
            return false;
        }

        /// <summary>
        /// 断开两个节点的边
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        /// <returns></returns>
        public bool DisconnectState(string fromState, string toState)
        {
            return this.StateGraph[fromState].Remove(toState);
        }
        #endregion

        /// <summary>
        /// 设置初始状态
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public bool SetInitState(string stateName)
        {
            State state;
            if (this.StateDict.TryGetValue(stateName, out state))
            {
                DefaultStateName = stateName;
                _currentState = state;
                return true;
            }
            return false;
        }

        public virtual void ResetState()
        {
            if (this.StateDict.TryGetValue(this.DefaultStateName, out State state))
            {
                _currentState.InvokeExitAction(this, state);
                state.InvokeEnterAction(this, _currentState);
                _currentState = state;
            }
        }

        public void ResetCurrentState()
        {
            _currentState.InvokeExitAction(this, _currentState);
            _currentState.InvokeEnterAction(this, _currentState);
        }

        /// <summary>
        /// 更新当前状态事件
        /// </summary>
        /// <param name="controller"></param>
        public void UpdateState(StateController controller)
        {
            if(this._currentState != null)
            {
                this._currentState.InvokeUpdateAction(this);
            }
        }

        /// <summary>
        /// 状态切换更新
        /// </summary>
        /// <param name="controller"></param>
        public bool LateChangeState(StateController controller)
        {
            if(_currentState == null)
            {
                return false;
            }
            if (this.StateGraph.ContainsKey(this._currentState.Name))
            {
                int m = int.MinValue;
                string key = "";
                foreach (var edge in this.StateGraph[_currentState.Name])
                {
                    if (this.StateDict[edge.Key].priority >= m && edge.Value.Invoke(this, this._currentState))
                    {
                        m = this.StateDict[edge.Key].priority;
                        key = edge.Key;
                    }
                }
                if(key != "")
                {
                    string preState = this._currentState.Name;
                    this._currentState.InvokeExitAction(this, this.StateDict[key]);
                    this._currentState = this.StateDict[key];
                    this._currentState.InvokeEnterAction(this, this.StateDict[preState]);

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取对应的状态
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public State GetState(string name)
        {
            if (this.StateDict.ContainsKey(name))
            {
                return this.StateDict[name];
            }
            return null;
        }

        public virtual void Update(float deltaTime)
        {

        }
    }
}

