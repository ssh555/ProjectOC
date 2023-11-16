using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Flags, System.Serializable]
	public enum BuildingCategory
	{
		[LabelText("All")]
		All = int.MaxValue,
		[LabelText("None")]
		None = 0,
		[LabelText("房屋")]
		Room = 1 << 0,
	}
}
