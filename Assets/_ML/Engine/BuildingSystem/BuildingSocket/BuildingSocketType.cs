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
		[LabelText("地板表层A")]
		Floorsurface_A = 3,
		[LabelText("墙面A")]
		Wallsurface_A = 4,
		[LabelText("天花板A")]
		Ceiling_A = 5,
		[LabelText("B地板A")]
		Floor_Base_Center_A = 6,
		[LabelText("B地板C")]
		Floor_Base = 7,
		[LabelText("B墙A")]
		Wall_Base_A = 8,
		[LabelText("B墙C")]
		Wall_Base = 9,
		[LabelText("矮方墙A")]
		Wall_Base_1A = 10,
		[LabelText("矮方墙连接C")]
		Wall_Base1 = 11,
		[LabelText("L墙A")]
		Wall_LB_A = 12,
		[LabelText("矮长墙A")]
		Wall_LB_1A = 13,
		[LabelText("Q墙A")]
		Wall_Qua_A = 14,
		[LabelText("矮弧墙A")]
		Wall_Qua_1A = 15,
		[LabelText("L地板C")]
		Floor_Lbase = 16,
		[LabelText("Q地板C")]
		Floor_Qua = 17,
		[LabelText("Q墙C")]
		Wall_Qua = 18,
		[LabelText("矮弧墙连接C")]
		Wall_Qua1 = 19,
		[LabelText("长柱A")]
		Pillar_A = 20,
		[LabelText("短柱A")]
		Pillar_A1 = 21,
		[LabelText("长柱C")]
		Pillar = 22,
		[LabelText("柱和屋顶")]
		RoofToPillar = 23,
		[LabelText("标准屋顶A")]
		Roof_Base_A = 24,
		[LabelText("矮标准屋顶A")]
		Roof_Base_A1 = 25,
		[LabelText("标准屋顶C")]
		Roof_Base = 26,
		[LabelText("矮标准屋顶C")]
		Roof_Base1 = 27,
		[LabelText("地板角点C")]
		Floor_p_C = 28,
		[LabelText("L墙C")]
		Wall_LB_C = 29,
		[LabelText("矮长墙C")]
		Wall_LB1_C = 30,
		[LabelText("三角墙LA")]
		Wall_tri_A = 31,
		[LabelText("三角墙LC")]
		Wall_tri_C = 32,
		[LabelText("矮三角墙LA")]
		Wall_tri1_A = 33,
		[LabelText("矮三角墙LC")]
		Wall_tri1_C = 34,
		[LabelText("餐厅座位C")]
		Canteen_Seat_C = 35,
		[LabelText("长梁A")]
		Beam_A = 36,
		[LabelText("短梁A")]
		Beam_A1 = 37,
		[LabelText("长梁C")]
		Beam_C = 38,
		[LabelText("短梁C")]
		Beam_C1 = 39,
		[LabelText("外延A")]
		Edge_A = 40,
		[LabelText("外延C")]
		Edge_C = 41,
		[LabelText("长外延C")]
		Edge_L_C = 42,
		[LabelText("长外延A")]
		Edge_L_A = 43,
		[LabelText("弧外延A")]
		Edge_Qua_A = 44,
		[LabelText("弧外延C")]
		Edge_Qua_C = 45,
		[LabelText("T地板A")]
		Floor_Qua_A = 46,
		[LabelText("Q地板A")]
		Floor_Tri_A = 47,
		[LabelText("T高屋顶A")]
		roof_T_A = 48,
		[LabelText("T高屋顶C")]
		roof_T_C = 49,
		[LabelText("Q高屋顶A")]
		roof_Q_A = 50,
		[LabelText("Q高屋顶C")]
		roof_Q_C = 51,
		[LabelText("L高屋顶A")]
		roof_L_A = 52,
		[LabelText("L高屋顶C")]
		roof_L_C = 53,
		[LabelText("三角RA")]
		wall_Tr_A = 54,
		[LabelText("三角RC")]
		wall_Tr_C = 55,
		[LabelText("矮三角RA")]
		wall_Tr1_A = 56,
		[LabelText("矮三角RC")]
		wall_Tr1_C = 57,
		[LabelText("Q矮屋顶A")]
		roof_Q_1A = 58,
		[LabelText("Q矮屋顶C")]
		roof_Q_1C = 59,
		[LabelText("T矮屋顶A")]
		roof_T_1A = 60,
		[LabelText("T矮屋顶C")]
		roof_T_1C = 61,
		[LabelText("L矮屋顶A")]
		roof_L_1A = 62,
		[LabelText("L矮屋顶C")]
		roof_L_1C = 63,
	}
}
