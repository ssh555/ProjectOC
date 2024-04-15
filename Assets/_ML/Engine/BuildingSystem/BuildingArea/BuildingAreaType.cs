using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingArea
{
	[System.Serializable]
	public enum BuildingAreaType
	{
		[LabelText("None")]
		None,
		[LabelText("Ground")]
		Ground,
		[LabelText("Floor")]
		Floor,
		[LabelText("Table")]
		Table,
		[LabelText("Wall")]
		Wall,
	}
}
