using Sirenix.OdinInspector;

namespace ML.Engine.InventorySystem
{
	[System.Serializable]
	public enum ItemType
	{
		[LabelText("None")]
		None,
		[LabelText("装备")]
		Equip,
		[LabelText("食物")]
		Food,
		[LabelText("材料")]
		Material,
		[LabelText("任务")]
		Mission,
	}
}
