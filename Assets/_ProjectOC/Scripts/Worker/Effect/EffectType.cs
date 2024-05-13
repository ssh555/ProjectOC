using Sirenix.OdinInspector;
namespace ProjectOC.WorkerNS
{
    [LabelText("效果类型")]
    public enum EffectType
    {
        [LabelText("None")]
        None,
        [LabelText("刁民")]
        AlterWorkerVariable,
        [LabelText("生产节点")]
        AlterProNodeVariable,
        [LabelText("共鸣轮")]
        AlterEchoVariable,
        [LabelText("玩家")]
        AlterPlayerVariable
    }
}