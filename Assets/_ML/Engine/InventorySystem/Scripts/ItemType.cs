using Sirenix.OdinInspector;

namespace ML.Engine.InventorySystem
{
	[System.Serializable]
	public enum ItemType
	{
		[LabelText("None")]
		None,
		[LabelText("蔬菜")]
        Vegetable,
        [LabelText("肉")]
        Meat,
        [LabelText("饲料")]
        Feed,
        [LabelText("便当")]
        BoxLunch,
        [LabelText("果汁")]
        Juice,
        [LabelText("零食")]
        Snack,
        [LabelText("医用品")]
        MedicalPro,
        [LabelText("药物")]
        Medicine,
        [LabelText("木")]
        Wood,
        [LabelText("布料")]
        Fabric,
        [LabelText("石")]
        Stone,
        [LabelText("金")]
        Metal,
        [LabelText("术")]
        Magic,
        [LabelText("演算")]
        Calculus,
        [LabelText("养殖生物")]
        Creature,
        [LabelText("服饰")]
        Cloth,
        [LabelText("任务")]
        Mission,
    }
}
