using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Serializable]
	public enum BuildingCategory2
	{
		[LabelText("None")]
		None = -1,
		[LabelText("地基")]
		Foundation = 0,
		[LabelText("地板")]
		Floor = 001,
		[LabelText("地基外沿")]
		FoundationEdge = 002,
		[LabelText("墙")]
		Wall = 003,
		[LabelText("柱")]
		Pillar = 004,
		[LabelText("梁")]
		Beam = 005,
		[LabelText("屋顶")]
		Roof = 006,
		[LabelText("门")]
		Door = 007,
		[LabelText("窗")]
		Window = 008,
		[LabelText("吊灯")]
		CeilingLamp = 100,
		[LabelText("椅子")]
		Chair = 101,
		[LabelText("挂钟")]
		Clock = 102,
		[LabelText("生命导流桩")]
		PowerCore = 200,
		[LabelText("导流节点")]
		PowerSub = 201,
		[LabelText("用电器")]
		ElectAppliance = 202,
	}
}
