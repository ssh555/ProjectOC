using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Serializable]
	public enum BuildingCategory
	{
		[LabelText("None")]
		None = -1,
		[LabelText("房屋")]
		Room = 0,
	}
}
