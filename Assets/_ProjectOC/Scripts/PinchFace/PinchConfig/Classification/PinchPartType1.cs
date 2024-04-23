using Sirenix.OdinInspector;

namespace ProjectOC.PinchFace
{
	[System.Serializable]
	public enum PinchPartType1
	{
		[LabelText("None")]
		None = 0,
		[LabelText("头部")]
		HeadPart = 1,
		[LabelText("面部")]
		FacePart = 2,
		[LabelText("躯干")]
		TrunkPart = 3,
		[LabelText("手部")]
		ArmPart = 4,
		[LabelText("腿部")]
		LegPart = 5,
		[LabelText("通用")]
		Common = 6,
	}
}
