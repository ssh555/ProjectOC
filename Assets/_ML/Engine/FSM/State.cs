using System;
using ML.PlayerCharacterNS;

namespace ML.Engine.FSM
{
    public class State : IState
    {
        #region Property
        public string Name = "";

        /// <summary>
        /// ״̬����ʱ�����ȼ� => ���ȼ������ȼ�(���ִ��)
        /// </summary>
        public short priority = 0;

        /// <summary>
        /// ������StateMachine
        /// </summary>
        public StateMachine stateMachine;

        /// <summary>
        /// ��Ӧ��״̬��, ��һ��״̬�� �����״̬
        /// </summary>
        protected Action<StateMachine, State, State> EnterStateAction;

        /// <summary>
        /// ��Ӧ��״̬���� �˳�ʱ��״̬�� ��һ��״̬
        /// </summary>
        protected Action<StateMachine, State, State> ExitStateAction;

        /// <summary>
        /// ��Ӧ��״̬���� ��ǰ״̬
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
        /// �����ڼ̳�ʵ�֣���������״̬
        /// </summary>
        public virtual void ConfigState()
        {

        }
        #endregion

        #region ״̬�����¼���ط���
        /// <summary>
        /// ��״̬�����¼�
        /// </summary>
        /// <param name="action"></param>
        public void BindEnterAction(Action<StateMachine, State, State> action)
        {
            this.EnterStateAction += action;
        }

        /// <summary>
        /// ���״̬�����¼�
        /// </summary>
        /// <param name="action"></param>
        public void UnBindEnterAction(Action<StateMachine, State, State> action)
        {
            this.EnterStateAction -= action;
        }

        /// <summary>
        /// ���״̬�����¼�
        /// </summary>
        public void ClearEnterAction()
        {
            this.EnterStateAction = null;
        }

        /// <summary>
        /// ���״̬�����¼��ĸ���
        /// </summary>
        /// <returns></returns>
        public Action<StateMachine, State, State> GetEnterActionEctype()
        {
            return (Action<StateMachine, State, State>)this.EnterStateAction.Clone();
        }

        /// <summary>
        /// �첽ִ��״̬�����¼�
        /// </summary>
        /// <param name="machine">����״̬��</param>
        /// <param name="preState">��һ��״̬</param>
        /// <param name="callback">�첽�ص�����</param>
        /// <param name="object">�ص�����</param>
        public IAsyncResult AsyncInvokeEnterAction(StateMachine machine, State preState, AsyncCallback callback, object @object)
        {
            return this.EnterStateAction?.BeginInvoke(machine, preState, this, callback, @object);
        }

        /// <summary>
        /// ͬ��ִ��״̬�����¼�
        /// </summary>
        /// <param name="machine">����״̬��</param>
        /// <param name="preState">��һ��״̬</param>
        public void InvokeEnterAction(StateMachine machine, State preState)
        {
            this.EnterStateAction?.Invoke(machine, preState, this);
        }
        #endregion

        #region ״̬�˳��¼���ط���
        /// <summary>
        /// ��״̬�˳��¼�
        /// </summary>
        /// <param name="action"></param>
        public void BindExitAction(Action<StateMachine, State, State> action)
        {
            this.ExitStateAction += action;
        }

        /// <summary>
        /// ���״̬�˳��¼�
        /// </summary>
        /// <param name="action"></param>
        public void UnBindExitAction(Action<StateMachine, State, State> action)
        {
            this.ExitStateAction -= action;
        }

        /// <summary>
        /// ���״̬�˳��¼�
        /// </summary>
        public void ClearExitAction()
        {
            this.ExitStateAction = null;
        }

        /// <summary>
        /// ���״̬�˳��¼��ĸ���
        /// </summary>
        /// <returns></returns>
        public Action<StateMachine, State, State> GetExitActionEctype()
        {
            return (Action<StateMachine, State, State>)this.ExitStateAction.Clone();
        }

        /// <summary>
        /// �첽ִ��״̬�˳��¼�
        /// </summary>
        /// <param name="machine">����״̬��</param>
        /// <param name="preState">��һ��״̬</param>
        /// <param name="callback">�첽�ص�����</param>
        /// <param name="object">�ص�����</param>
        public IAsyncResult AsyncInvokeExitAction(StateMachine machine, State postState, AsyncCallback callback, object @object)
        {
            return this.ExitStateAction?.BeginInvoke(machine, this, postState, callback, @object);
        }

        /// <summary>
        /// ͬ��ִ��״̬�˳��¼�
        /// </summary>
        /// <param name="machine">����״̬��</param>
        /// <param name="preState">��һ��״̬</param>
        public void InvokeExitAction(StateMachine machine, State postState)
        {
            this.ExitStateAction?.Invoke(machine, this, postState);
        }
        #endregion

        #region ״̬�����¼���ط���
        /// <summary>
        /// ��ÿ֡���õ�״̬�¼�
        /// </summary>
        /// <param name="action"></param>
        public void BindUpdateAction(Action<StateMachine, State> action)
        {
            this.UpdateStateAction += action;
        }

        /// <summary>
        /// ���ÿ֡���õ�״̬�¼�
        /// </summary>
        /// <param name="action"></param>
        public void UnBindUpdateAction(Action<StateMachine, State> action)
        {
            this.UpdateStateAction -= action;
        }

        /// <summary>
        /// ���ÿ֡���õ�״̬�¼�
        /// </summary>
        public void ClearUpdateAction()
        {
            this.UpdateStateAction = null;
        }

        /// <summary>
        /// ���ÿ֡���õ�״̬�¼��ĸ���
        /// </summary>
        /// <returns></returns>
        public Action<StateMachine, State> GetUpdateActionEctype()
        {
            return (Action<StateMachine, State>)this.UpdateStateAction.Clone();
        }

        /// <summary>
        /// �첽ִ��ÿ֡���õ�״̬�¼�
        /// </summary>
        /// <param name="machine">����״̬��</param>
        /// <param name="callback">�첽�ص�����</param>
        /// <param name="object">�ص�����</param>
        public IAsyncResult AsyncInvokeUpdateAction(StateMachine machine, AsyncCallback callback, object @object)
        {
            return this.UpdateStateAction?.BeginInvoke(machine, this, callback, @object);
        }

        /// <summary>
        /// ͬ��ִ��ÿ֡���õ�״̬�¼�
        /// </summary>
        /// <param name="machine">����״̬��</param>
        public void InvokeUpdateAction(StateMachine machine)
        {
            this.UpdateStateAction?.Invoke(machine, this);
        }
        #endregion

    }
}

