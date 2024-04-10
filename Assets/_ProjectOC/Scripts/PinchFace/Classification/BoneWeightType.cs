using Sirenix.OdinInspector;

namespace ProjectOC.PinchFace
{
	[System.Serializable]
	public enum BoneWeightType
	{
		[LabelText("None")]
		None = 0,
		[LabelText("头部")]
		Head = 1,
		[LabelText("胸部")]
		Chest = 2,
		[LabelText("腰部")]
		Waist = 3,
		[LabelText("上肢")]
		Arm = 4,
		[LabelText("下肢")]
		Leg = 5,
		[LabelText("尾巴")]
		Tail = 6,
		[LabelText("整体")]
		Root = 7,
	}
}
