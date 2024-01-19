using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Serializable]
	public enum BuildingCategory1
	{
		[LabelText("None")]
		None = -1,
		[LabelText("生产节点")]
		ProNode = 0,
		[LabelText("交互建筑")]
		Interact = 1,
		[LabelText("房屋")]
		House = 2,
		[LabelText("家具")]
		Furniture = 3,
	}
}
