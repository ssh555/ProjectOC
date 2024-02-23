using Sirenix.OdinInspector;

namespace ProjectOC.TechTree
{
	[System.Serializable]
	public enum TechPointCategory
	{
		[LabelText("None")]
		None,
		[LabelText("能源")]
		Energy,
		[LabelText("栽培")]
		Plant,
		[LabelText("科技")]
		Technology,
		[LabelText("演算")]
		Calculation,
		[LabelText("共鸣")]
		Resonance,
		[LabelText("熔炼")]
		Smelt,
		[LabelText("储藏")]
		Storage,
		[LabelText("工艺")]
		Craft,
		[LabelText("隐兽")]
		Echo,
		[LabelText("厨艺")]
		Cook,
	}
}
