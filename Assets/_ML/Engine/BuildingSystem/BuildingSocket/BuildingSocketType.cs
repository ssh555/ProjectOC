using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingSocket
{
	[System.Flags, System.Serializable]
	public enum BuildingSocketType
	{
		[LabelText("All")]
		All = int.MaxValue,
		[LabelText("None")]
		None = 0,
	}
}
