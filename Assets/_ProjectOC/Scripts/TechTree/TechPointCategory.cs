using Sirenix.OdinInspector;

namespace ProjectOC.TechTree
{
	[System.Serializable]
	public enum TechPointCategory
	{
		[LabelText("None")]
		None,
		[LabelText("能源")]
		Energy = 1,
		[LabelText("栽培")]
		Plant = 2,
		[LabelText("演算")]
		Calculus = 3,
		[LabelText("冶炼")]
		Smelt = 4,
		[LabelText("贮藏")]
		Storage = 5,
		[LabelText("工艺")]
		Craft = 6,
		[LabelText("隐兽")]
		Echo = 7,
		[LabelText("厨艺")]
		Cook = 8,
	}
}
