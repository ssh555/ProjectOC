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
		[LabelText("生命导流桩吸附点")]
		LifeDiversion,
		[LabelText("功能建筑吸附点")]
		PronodeAndInteract,
		[LabelText("地板表层")]
		Floorsurface,
		[LabelText("天花板")]
		Ceiling,
		[LabelText("墙面")]
		Wallsurface,
		[LabelText("地板中心吸附点")]
		Floor_Base_Center,
		[LabelText("地板顶点吸附点")]
		Floor_Base_Vertex,
	}
}
