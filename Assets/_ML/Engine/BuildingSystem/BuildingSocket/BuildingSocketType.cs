using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem.BuildingSocket
{
	[System.Serializable]
	public enum BuildingSocketType
	{
		[LabelText("None")]
		None,
		[LabelText("生命导流桩吸附点")]
		LifeDiversion = 1,
		[LabelText("功能建筑吸附点")]
		PronodeAndInteract = 2,
		[LabelText("地板和地表")]
		SurfaceAndFloor = 3,
		[LabelText("墙面A")]
		Wallsurface_A = 4,
		[LabelText("天花板A")]
		Ceiling_A = 5,
		[LabelText("地板表面")]
		Floor_A = 6,
		[LabelText("地板中点")]
		center_Bfloor = 7,
		[LabelText("Q地板中点")]
		center_Qfloor = 8,
		[LabelText("T地板中点")]
		center_Tfloor = 9,
		[LabelText("地板角点")]
		point_Bfloor = 10,
		[LabelText("Q地板角点")]
		point_Qfloor = 11,
		[LabelText("T地板角点")]
		point_Tfloor = 12,
		[LabelText("B墙底心")]
		center_Wall = 13,
		[LabelText("Q墙底心")]
		center_QWall = 14,
		[LabelText("L墙底心")]
		center_LWall = 15,
		[LabelText("B墙角点")]
		point_Bwall = 16,
		[LabelText("Q墙角点")]
		point_Qwall = 17,
		[LabelText("L墙角点")]
		point_Lwall = 18,
		[LabelText("柱顶")]
		Pillar_Up = 19,
		[LabelText("柱底")]
		Pillar_Down = 20,
		[LabelText("梁心")]
		Center_Beam = 21,
		[LabelText("梁角")]
		Point_Beam = 22,
		[LabelText("B顶心")]
		center_Broof = 23,
		[LabelText("Q顶心")]
		center_Qroof = 24,
		[LabelText("T顶心")]
		center_Troof = 25,
		[LabelText("L顶心")]
		center_Lroof = 26,
		[LabelText("顶角点")]
		point_Roof = 27,
		[LabelText("T墙底心")]
		center_Twall = 28,
		[LabelText("T墙角点")]
		point_Twall = 29,
		[LabelText("长梁心")]
		Beam_LC = 30,
		[LabelText("长梁顶点")]
		Beam_LP = 31,
		[LabelText("B外延")]
		Edge_B = 32,
		[LabelText("L外延")]
		Edge_L = 33,
		[LabelText("Q外延")]
		Edge_Q = 34,
	}
}
