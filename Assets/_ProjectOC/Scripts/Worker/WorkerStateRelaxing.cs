using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateRelaxing : State
    {
        /// <summary>
        /// �Ƿ������Ϣ
        /// </summary>
        public bool IsCompleteRelax { get; private set; } = false;

        public WorkerStateRelaxing(string name) : base(name)
        {
        }
        public override void  ConfigState()
        {
            this.BindEnterAction
            (
                (machine, state1, state2) =>
                {
                    this.IsCompleteRelax = false;
                    //DispatchManager�����¼�����: ������Ϣʱ�β�������������ֵ������Ϊtrue;
                    //timer (loop) => ��Ϣʱ�ݶ�ÿ��ظ�1 %
                }    
            );
            this.BindEnterAction
            (
                (machine, state1, state2) =>
                {
                    this.IsCompleteRelax = false;
                    //�Ƴ�Enter�����Event
                    //����timer
                }
            );
            this.BindUpdateAction
            (
                (machine, state) =>
                {
                    // TODO:
                    //�� ��������Ϣʱ�Σ�����������Ƿ�ظ�����һ��ֵ��������isCompleteRelaxΪtrue
                    //�� �����ظ����ڶ���ֵ,Ҳ��Ϊtrue
                }
            );
        }
    }
}