using Sirenix.OdinInspector;
namespace ProjectOC.WorkerNS
{
    [LabelText("Ч������")]
    public enum EffectType
    {
        [LabelText("None")]
        None,
        [LabelText("����")]
        AlterWorkerVariable,
        [LabelText("�����ڵ�")]
        AlterProNodeVariable,
        [LabelText("������")]
        AlterEchoVariable,
        [LabelText("���")]
        AlterPlayerVariable
    }
}