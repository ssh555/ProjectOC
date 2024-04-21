using Sirenix.OdinInspector;

namespace ProjectOC.TechTree
{
	[System.Serializable]
	public enum TechPointCategory
	{
		[LabelText("None")]
		None,
		[LabelText("其他")]
		Other = 1,
		[LabelText("生产")]
		Product = 2,
	}
}
