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
		[LabelText("工艺")]
		Technology,
		[LabelText("演算")]
		Calculation,
		[LabelText("共鸣")]
		Resonance,
	}
}
