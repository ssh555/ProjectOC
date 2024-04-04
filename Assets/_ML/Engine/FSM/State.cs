using System;
using ML.PlayerCharacterNS;

namespace ML.Engine.FSM
{
    public class State : IState
    {
        #region Property
        public string Name = "";

        /// <summary>
        /// 状态更替时的优先级 => 优先检测高优先级(数字大的)
        /// </summary>
        public short priority = 0;

        /// <summary>
        /// 依附的StateMachine
        /// </summary>
        public StateMachine stateMachine;

        /// <summary>
        /// 对应的状态机, 上一个状态， 进入的状态
        /// </summary>
        protected Action<StateMachine, State, State> EnterStateAction;

        /// <summary>
        /// 对应的状态机， 退出时的状态， 下一个状态
        /// </summary>
        protected Action<StateMachine, State, State> ExitStateAction;

        /// <summary>
        /// 对应的状态机， 当前状态
        /// </summary>
        protected Action<StateMachine, State> UpdateStateAction;

        #endregion

        #region Construction
        public State(string name)
        {
            this.Name = name;
            this.ConfigState();
        }

        /// <summary>
        /// 可用于继承实现，方便配置状态
        /// </summary>
        public virtual void ConfigState()
        {

        }
        #endregion

        #region 状态进入事件相关方法
        /// <summary>
        /// 绑定状态进入事件
        /// </summary>
        /// <param name="action"></param>
        public void BindEnterAction(Action<StateMachine, State, State> action)
        {
            this.EnterStateAction += action;
        }

        /// <summary>
        /// 解绑状态进入事件
        /// </summary>
        /// <param name="action"></param>
        public void UnBindEnterAction(Action<StateMachine, State, State> action)
        {
            this.EnterStateAction -= action;
        }

        /// <summary>
        /// 清空状态进入事件
        /// </summary>
        public void ClearEnterAction()
        {
            this.EnterStateAction = null;
        }

        /// <summary>
        /// 获得状态进入事件的副本
        /// </summary>
        /// <returns></returns>
        public Action<StateMachine, State, State> GetEnterActionEctype()
        {
            return (Action<StateMachine, State, State>)this.EnterStateAction.Clone();
        }

        /// <summary>
        /// 异步执行状态进入事件
        /// </summary>
        /// <param name="machine">所属状态机</param>
        /// <param name="preState">上一个状态</param>
        /// <param name="callback">异步回调函数</param>
        /// <param name="object">回调参数</param>
        public IAsyncResult AsyncInvokeEnterAction(StateMachine machine, State preState, AsyncCallback callback, object @object)
        {
            return this.EnterStateAction?.BeginInvoke(machine, preState, this, callback, @object);
        }

        /// <summary>
        /// 同步执行状态进入事件
        /// </summary>
        /// <param name="machine">所属状态机</param>
        /// <param name="preState">上一个状态</param>
        public void InvokeEnterAction(StateMachine machine, State preState)
        {
            this.EnterStateAction?.Invoke(machine, preState, this);
        }
        #endregion

        #region 状态退出事件相关方法
        /// <summary>
        /// 绑定状态退出事件
        /// </summary>
        /// <param name="action"></param>
        public void BindExitAction(Action<StateMachine, State, State> action)
        {
            this.ExitStateAction += action;
        }

        /// <summary>
        /// 解绑状态退出事件
        /// </summary>
        /// <param name="action"></param>
        public void UnBindExitAction(Action<StateMachine, State, State> action)
        {
            this.ExitStateAction -= action;
        }

        /// <summary>
        /// 清空状态退出事件
        /// </summary>
        public void ClearExitAction()
        {
            this.ExitStateAction = null;
        }

        /// <summary>
        /// 获得状态退出事件的副本
        /// </summary>
        /// <returns></returns>
        public Action<StateMachine, State, State> GetExitActionEctype()
        {
            return (Action<StateMachine, State, State>)this.ExitStateAction.Clone();
        }

        /// <summary>
        /// 异步执行状态退出事件
        /// </summary>
        /// <param name="machine">所属状态机</param>
        /// <param name="preState">下一个状态</param>
        /// <param name="callback">异步回调函数</param>
        /// <param name="object">回调参数</param>
        public IAsyncResult AsyncInvokeExitAction(StateMachine machine, State postState, AsyncCallback callback, object @object)
        {
            return this.ExitStateAction?.BeginInvoke(machine, this, postState, callback, @object);
        }

        /// <summary>
        /// 同步执行状态退出事件
        /// </summary>
        /// <param name="machine">所属状态机</param>
        /// <param name="preState">下一个状态</param>
        public void InvokeExitAction(StateMachine machine, State postState)
        {
            this.ExitStateAction?.Invoke(machine, this, postState);
        }
        #endregion

        #region 状态更新事件相关方法
        /// <summary>
        /// 绑定每帧调用的状态事件
        /// </summary>
        /// <param name="action"></param>
        public void BindUpdateAction(Action<StateMachine, State> action)
        {
            this.UpdateStateAction += action;
        }

        /// <summary>
        /// 解绑每帧调用的状态事件
        /// </summary>
        /// <param name="action"></param>
        public void UnBindUpdateAction(Action<StateMachine, State> action)
        {
            this.UpdateStateAction -= action;
        }

        /// <summary>
        /// 清空每帧调用的状态事件
        /// </summary>
        public void ClearUpdateAction()
        {
            this.UpdateStateAction = null;
        }

        /// <summary>
        /// 获得每帧调用的状态事件的副本
        /// </summary>
        /// <returns></returns>
        public Action<StateMachine, State> GetUpdateActionEctype()
        {
            return (Action<StateMachine, State>)this.UpdateStateAction.Clone();
        }

        /// <summary>
        /// 异步执行每帧调用的状态事件
        /// </summary>
        /// <param name="machine">所属状态机</param>
        /// <param name="callback">异步回调函数</param>
        /// <param name="object">回调参数</param>
        public IAsyncResult AsyncInvokeUpdateAction(StateMachine machine, AsyncCallback callback, object @object)
        {
            return this.UpdateStateAction?.BeginInvoke(machine, this, callback, @object);
        }

        /// <summary>
        /// 同步执行每帧调用的状态事件
        /// </summary>
        /// <param name="machine">所属状态机</param>
        public void InvokeUpdateAction(StateMachine machine)
        {
            this.UpdateStateAction?.Invoke(machine, this);
        }
        #endregion

    }
}

