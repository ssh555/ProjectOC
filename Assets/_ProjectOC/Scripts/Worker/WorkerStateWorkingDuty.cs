using ML.Engine.FSM;

namespace ProjectOC.WorkerNS
{
    public class WorkerStateWorkingDuty : State
    {
        // ֵ�ദ��ID��ţ�-1��ʾδֵ��
        // string DutyDepartmentID => ����machine
        public WorkerStateWorkingDuty(string name) : base(name)
        {

        }

        public override void ConfigState()
        {
            this.BindUpdateAction((machine, state) => {
                // ��ʼǰ����Ӧ�������ڵ�
                // �����ʼ����
            });
        }
    }
}