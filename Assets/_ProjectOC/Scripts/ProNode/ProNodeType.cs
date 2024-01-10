using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    /// <summary>
    /// 生产节点类型
    /// </summary>
    public enum ProNodeType
    {
        [LabelText("None")]
        None,
        [LabelText("自动型")]
        Auto,
        [LabelText("人工型")]
        Mannul,
    }
}
