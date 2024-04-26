using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// 特性类型
    /// 从上到下的顺序为种族、增益、减益、整活特性
    /// </summary>
    public enum FeatureType
    {
        [LabelText("种族特性")]
        Race,
        [LabelText("增益特性")]
        Buff,
        [LabelText("减益特性")]
        DeBuff,
        [LabelText("整活特性")]
        None,
    }
}

