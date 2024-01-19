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
		[LabelText("Smelt")]
		Smelt,
        [LabelText("Storage")]
        Storage,
        [LabelText("Craft")]
        Craft,
        [LabelText("Echo")]
        Echo,
        [LabelText("Cook")]
        Cook,
        [LabelText("工艺")]
		Technology,
		[LabelText("演算")]
		Calculus,
		[LabelText("共鸣")]
		Resonance,
	}
}
