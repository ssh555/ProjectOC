using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateWorkingTransport : State
    {
        // ��������Ϊnull��ʾû������
        //CarryMission Mission => ����machine
        // ���˵���Ʒ��ID
        // Ϊ-1��ʾΪnull
        // int ItemID => ����machine��PDID����
        // ��ΰ��˵�����
        // ������Worker�����Կ��ƣ�������Ҳ���ϣ����߾���ȫ����Worker����������
        // �������ֵ������worker�������Եĵ�ǰ����
        //int CurrentCount;

        public WorkerStateWorkingTransport(string name) : base(name)
        {

        }

        public override void ConfigState()
        {
            this.BindUpdateAction((machine, state) => {
                // ���ư�������(��ʼ - ���� - �������)
            });
        }
    }
}