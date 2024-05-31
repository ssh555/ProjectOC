using Sirenix.OdinInspector;

namespace ML.Engine.InventorySystem
{
    [LabelText("配方类目")]
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
        [LabelText("矿石锚定潜航器")]
        DiveStation,
        [LabelText("精炼炉")]
        Refine,
        [LabelText("手工作业台")]
        Handwork,
        [LabelText("制药台")]
        Pharmacy,
        [LabelText("发酵桶")]
        Liquor,
        [LabelText("聚合器")]
        Aggregator,
        [LabelText("精密加工机")]
        Processor,
        [LabelText("风味工坊")]
        Kitchen,
        [LabelText("培育舱")]
        CreaturePro,
        [LabelText("繁育仪")]
        CreatureBreed,
        [LabelText("演算仪")]
        Calculus
    }
}

