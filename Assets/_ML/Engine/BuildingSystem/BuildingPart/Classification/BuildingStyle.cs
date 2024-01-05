using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Serializable]
	public enum BuildingStyle
	{
		[LabelText("None")]
		None,
		[LabelText("正方形")]
		Base,
		[LabelText("三角形")]
		Triangle,
		[LabelText("四方圆形")]
		Quagrant,
		[LabelText("方形转角")]
		BaseCorner,
	}
}
