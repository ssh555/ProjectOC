using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [LabelText("生产节点状态")]
    public enum ProNodeState
    {
        /// <summary>
        /// 未选择生产项
        /// </summary>
        [LabelText("空置")]
        Vacancy = 0,
        /// <summary>
        /// 已有生产项，因刁民不在岗or成品堆满or素材不足，未在生产中
        /// </summary>
        [LabelText("停滞中")]
        Stagnation = 1,
        /// <summary>
        /// 已有生产项，正在生产中
        /// </summary>
        [LabelText("生产中")]
        Production = 2
    }
}
