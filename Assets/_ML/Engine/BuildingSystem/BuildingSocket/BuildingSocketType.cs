using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingSocket
{
	[System.Serializable]
	public enum BuildingSocketType
	{
		[LabelText("None")]
		None,
		[LabelText("Floor_Base_AdjacementCenter")]
		Floor_Base_AdjacementCenter,
		[LabelText("Floor_Triangle_AdjacementCenter")]
		Floor_Triangle_AdjacementCenter,
		[LabelText("Floor_Quagrant_AdjacementCenter")]
		Floor_Quagrant_AdjacementCenter,
	}
}
