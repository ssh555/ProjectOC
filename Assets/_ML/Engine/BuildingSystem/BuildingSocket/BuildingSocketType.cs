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
	}
}
