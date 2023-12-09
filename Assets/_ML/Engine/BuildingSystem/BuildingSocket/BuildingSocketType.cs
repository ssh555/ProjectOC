using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingSocket
{
	[System.Flags, System.Serializable]
	public enum BuildingSocketType
	{
		[LabelText("All")]
		All = int.MaxValue,
		[LabelText("None")]
		None = 0,
		[LabelText("地基层")]
		Foundation = 1 << 0,
		[LabelText("地板层")]
		Floor = 1 << 1,
		[LabelText("墙层")]
		Wall = 1 << 2,
		[LabelText("地面层")]
		Ground = 1 << 3,
		[LabelText("柱")]
		Piliar = 1 << 4,
		[LabelText("梁")]
		Beam = 1 << 5,
		[LabelText("屋顶")]
		Roof = 1 << 6,
		[LabelText("门")]
		Door = 1 << 7,
		[LabelText("窗")]
		Window = 1 << 8,
		[LabelText("地基外围")]
		FoundationEdge = 1 << 9,
		[LabelText("地板下")]
		UnderFloor = 1 << 10,
	}
}
