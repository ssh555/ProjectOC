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
        /// ����״̬֮��ĵ������� => �Ƿ�Ӧ�ôӼ�β�����ͷ��ָ��״̬
        /// </summary>
        /// <returns></returns>
        public delegate bool IsChangeState(StateMachine stateMachine, State curState);

        /// <summary>
        /// ״̬�ڵ�
        /// </summary>
        [LabelText("״̬�ڵ�"), ShowInInspector, ReadOnly]
        protected Dictionary<string, State> StateDict;

        /// <summary>
        /// Key => ��״̬���е�һ��״̬������
        /// Value => Key״̬��Ӧ�� <�ı������� ָ���״̬������>
        /// </summary>
        protected Dictionary<string, Dictionary<string, IsChangeState>> StateGraph;
        
        [LabelText("��ǰ״̬"), ShowInInspector, ReadOnly]
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

        #region �ڵ㼰�ߵļ���ɾ��
        /// <summary>
        /// ����״̬�ڵ�
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
        /// �Ƴ�������״̬�ڵ�
        /// ��״̬�ڵ���ָ�����Ƴ�ʧ�ܣ�����null
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public State RemoveState(string stateName)
        {
            // �Ƿ��г���
            if (this.StateGraph.ContainsKey(stateName))
            {
                return null;
            }
            // �Ƿ������
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
        /// ǿ��ɾ��״̬�ڵ㼰�����ӵı�
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public State ForceRemoveState(string stateName)
        {
            State state;

            if (this.StateDict.TryGetValue(stateName, out state))
            {
                // ɾ������
                this.StateDict.Remove(stateName);
                // ɾ�����
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
        /// ���������ڵ�
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        /// <param name="isChangeState"></param>
        /// <param name="IsCover">���ߴ��ڣ��Ƿ񸲸�</param>
        /// <returns></returns>
        public bool ConnectState(string fromState, string toState, IsChangeState isChangeState, bool IsCover = false)
        {
            // �ڵ������
            if(this.StateDict.ContainsKey(fromState) && this.StateDict.ContainsKey(toState))
            {
                if (this.StateGraph.ContainsKey(fromState))
                {
                    // from -> to �ı��Ѵ��� && ����
                    if (this.StateGraph[fromState].ContainsKey(toState))
                    {
                        if (IsCover)
                        {
                            this.StateGraph[fromState][toState] = isChangeState;
                            return true;
                        }
                        return false;
                    }
                    // �߲�����
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
        /// �Ͽ������ڵ�ı�
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
        /// ���ó�ʼ״̬
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
        /// ���µ�ǰ״̬�¼�
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
        /// ״̬�л�����
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
        /// ��ȡ��Ӧ��״̬
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

