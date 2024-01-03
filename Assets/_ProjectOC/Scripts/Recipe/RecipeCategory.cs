using Sirenix.OdinInspector;

namespace ML.Engine.InventorySystem
{
    /// <summary>
    /// 配方类目
    /// </summary>
    public enum RecipeCategory 
    { 
        [LabelText("None")]
        None,
        [LabelText("露水收集器")]
        WaterCollector,
        [LabelText("水源纯化器")]
        WaterPurifier,
        [LabelText("种籽箱")]
        SeedBox,
        [LabelText("光流整束器")]
        Integrator,
        [LabelText("苗圃")]
        SeedPlot,
        [LabelText("捕捞站")]
        Detector,
        [LabelText("精炼炉")]
        Refine,
        [LabelText("锯木床")]
        Sawn,
        [LabelText("纺织机")]
        Textile,
        [LabelText("聚合器")]
        Aggregator,
        [LabelText("精密加工机")]
        Processor,
        [LabelText("风味工坊")]
        Kitchen,
        [LabelText("演算仪")]
        Calculus,
        [LabelText("储物仓")]
        Store,
        [LabelText("储液罐")]
        Reservoir,
        [LabelText("恒温仓")]
        Incubator,
        [LabelText("共鸣轮")]
        EchoWheel,
        [LabelText("导流节点")]
        DiversionNode,
        [LabelText("生命导流桩")]
        LifeDiversion,
        [LabelText("生态投影装置")]
        Projector,
    }
}

