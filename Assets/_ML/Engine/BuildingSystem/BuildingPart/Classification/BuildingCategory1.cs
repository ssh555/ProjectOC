using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Serializable]
	public enum BuildingCategory1
	{
		[LabelText("None")]
		None = -1,
		[LabelText("房屋")]
		Room = 0,
		[LabelText("家具")]
		Furniture = 1,
		[LabelText("室外设施")]
		ExternalFacility = 2,
	}
}
