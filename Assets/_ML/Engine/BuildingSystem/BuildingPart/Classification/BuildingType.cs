using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Serializable]
	public enum BuildingType
	{
		[LabelText("None")]
		None = -1,
		[LabelText("地基")]
		Foundation = 0,
		[LabelText("地板")]
		Floor,
	}
}
