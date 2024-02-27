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
		[LabelText("演算")]
		Calculus,
		[LabelText("冶炼")]
		Smelt,
		[LabelText("贮藏")]
		Storage,
		[LabelText("工艺")]
		Craft,
		[LabelText("隐兽")]
		Echo,
		[LabelText("厨艺")]
		Cook,
	}
}
