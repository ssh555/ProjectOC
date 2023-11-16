using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Flags, System.Serializable]
	public enum BuildingType
	{
		[LabelText("All")]
		All = int.MaxValue,
		[LabelText("None")]
		None = 0,
		[LabelText("地基")]
		Foundation = 1 << 0,
		[LabelText("地板")]
		Floor = 1 << 1,
	}
}
