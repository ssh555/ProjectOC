using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingArea
{
	[System.Flags, System.Serializable]
	public enum BuildingAreaType
	{
		[LabelText("All")]
		All = int.MaxValue,
		[LabelText("None")]
		None = 0,
	}
}
