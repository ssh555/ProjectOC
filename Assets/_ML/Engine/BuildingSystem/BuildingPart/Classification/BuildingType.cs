using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Serializable]
	public enum BuildingType
	{
		[LabelText("None")]
		None,
		[LabelText("地基")]
		Foundation,
		[LabelText("地板")]
		Floor,
	}
}
