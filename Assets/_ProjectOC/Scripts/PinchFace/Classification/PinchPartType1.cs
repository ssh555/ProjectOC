using Sirenix.OdinInspector;

namespace ProjectOC.PinchFace
{
	[System.Serializable]
	public enum PinchPartType1
	{
		[LabelText("头部")]
		HeadPart = 0,
		[LabelText("面部")]
		FacePart = 1,
		[LabelText("躯干")]
		TrunkPart = 2,
		[LabelText("手部")]
		ArmPart = 3,
		[LabelText("腿部")]
		LegPart = 4,
		[LabelText("身体")]
		BodyPart = 5,
		[LabelText("头发")]
		HairPart = 6,
		[LabelText("眼睛")]
		EyePart = 7,
	}
}
