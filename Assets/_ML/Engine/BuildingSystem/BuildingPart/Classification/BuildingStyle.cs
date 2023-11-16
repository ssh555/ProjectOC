using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingPart
{
	[System.Flags, System.Serializable]
	public enum BuildingStyle
	{
		[LabelText("All")]
		All = int.MaxValue,
		[LabelText("None")]
		None = 0,
		[LabelText("正方形")]
		Base = 1 << 0,
		[LabelText("三角形")]
		Triangle = 1 << 1,
		[LabelText("四分圆形")]
		Quagrant = 1 << 2,
	}
}
