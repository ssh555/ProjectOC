using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [LabelText("特性类型")]
    public enum FeatureType
    {
        None,
        [LabelText("反转特性")]
        Reverse,
        [LabelText("减益特性")]
        DeBuff,
        [LabelText("增益特性")]
        Buff,
        [LabelText("种族特性")]
        Race,
    }
}

