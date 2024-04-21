using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingArea
{
	[System.Serializable]
	public enum BuildingAreaType
	{
		[LabelText("None")]
		None,
		[LabelText("地面")]
		Ground,
		[LabelText("地板表面")]
		Floor,
		[LabelText("Table")]
		Table,
		[LabelText("墙面")]
		Wall,
		[LabelText("天花板")]
		Ceiling,
	}
}
