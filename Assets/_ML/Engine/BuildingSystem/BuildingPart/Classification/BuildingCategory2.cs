using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Serializable]
	public enum BuildingCategory2
	{
		[LabelText("None")]
		None = -1,
		[LabelText("露水收集器")]
		WaterCollector = 0,
		[LabelText("水源纯化器")]
		WaterPurifier = 001,
		[LabelText("种籽箱")]
		SeedBox = 002,
		[LabelText("光流整束器")]
		Integrator = 003,
		[LabelText("苗圃")]
		SeedPlot = 004,
		[LabelText("矿石锚定潜航器")]
		DiveStation = 005,
		[LabelText("精炼炉")]
		Refine = 006,
		[LabelText("手工作业台")]
		Handwork = 007,
		[LabelText("制药台")]
		Pharmacy = 008,
		[LabelText("聚合器")]
		Aggregator = 009,
		[LabelText("精密加工机")]
		Processor = 010,
		[LabelText("风味工坊")]
		Kitchen = 011,
		[LabelText("演算仪")]
		Calculus = 012,
		[LabelText("物流转运箱")]
		SolidStore = 102,
		[LabelText("液体转运罐")]
		LiquidStore = 103,
		[LabelText("餐厅")]
		Canteen = 104,
		[LabelText("共鸣轮")]
		EchoWheel = 105,
		[LabelText("导流节点")]
		DiversionNode = 101,
		[LabelText("生命导流桩")]
		LifeDiversion = 100,
		[LabelText("生态投影装置")]
		Projector = 106,
		[LabelText("地板")]
		Floor = 201,
		[LabelText("地基外沿")]
		Edge = 203,
		[LabelText("墙")]
		Wall = 202,
		[LabelText("柱")]
		Pillar = 204,
		[LabelText("梁")]
		Beam = 205,
		[LabelText("屋顶")]
		Roof = 206,
		[LabelText("门")]
		Door = 207,
		[LabelText("窗")]
		Window = 208,
		[LabelText("卧具")]
		Bed = 300,
		[LabelText("坐具")]
		Seat = 301,
		[LabelText("桌台")]
		Table = 302,
		[LabelText("箱柜")]
		Cupboard = 303,
		[LabelText("架")]
		Rack = 304,
		[LabelText("池")]
		Pool = 305,
		[LabelText("灯")]
		Lamp = 306,
		[LabelText("装饰")]
		Decoration = 307,
		[LabelText("发酵桶")]
		Liquor = 013,
		[LabelText("养殖生物培育舱")]
		CreaturePro = 014,
		[LabelText("养殖生物繁育仪")]
		CreatureBreed = 015,
		[LabelText("窝")]
		Nest = 107,
		[LabelText("喵喵窝")]
		FeatureModifier = 108,
		[LabelText("生物仓库")]
		Biostore = 109,
	}
}
