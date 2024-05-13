using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �ɿ�ϵͳ����
/// </summary>
public class MineSystemData
{
    const int MAPDEPTH = 3;

    /// <summary>
    /// ���ͼ�浵����
    /// </summary>
    public class IslandMapData
    {
        [LabelText("���ͼ����"), ReadOnly, ShowInInspector]
        private int MAPDEPTH = MineSystemData.MAPDEPTH;
        [LabelText("���ͼ���ű���"), ReadOnly, ShowInInspector]
        private float GridScale;
        [LabelText("�����б�"), ReadOnly, ShowInInspector]
        private List<MapRegionData> MapRegionDatas;
        [LabelText("���ͼ��ͼ���������"), ReadOnly, ShowInInspector]
        private bool[] isUnlockIslandMap = new bool[MineSystemData.MAPDEPTH];
        [LabelText("��������"), ReadOnly, ShowInInspector]
        private MainIslandData mainIslandData;
        [LabelText("С��ͼ����"), ReadOnly, ShowInInspector]
        private Dictionary<string,MineralMapData> mineralMapDatas;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public class MapRegionData
    {
        [LabelText("����ID"), ReadOnly, ShowInInspector]
        private string MapRegionID;
        [LabelText("�Ƿ�Ϊ�ϰ���"), ReadOnly, ShowInInspector]
        private bool IsBlock;
        [LabelText("����λ��"), ReadOnly, ShowInInspector]
        private Vector2 position;
        [LabelText("����Ԥ�����ʲ�·��"), ReadOnly, ShowInInspector]
        private string PrefabPath;
        [LabelText("�����ͼ���������"), ReadOnly, ShowInInspector]
        private bool[] isUnlockLayer = new bool[MineSystemData.MAPDEPTH];
        [LabelText("С��ͼ����"), ReadOnly, ShowInInspector]
        private string[] mineralDataID = new string[MineSystemData.MAPDEPTH];
    }

    /// <summary>
    /// С��ͼ���ݣ��ɿ��ͼ���ݣ�
    /// </summary>
    public class MineralMapData
    {
        [LabelText("�ɿ��ͼID"), ReadOnly, ShowInInspector]
        private string MineralMapID;
        [LabelText("�ɿ��ͼԤ�����ʲ�·��"), ReadOnly, ShowInInspector]
        private string PrefabPath;
        [LabelText("�ɿ��ͼ�еĿ�������"), ReadOnly, ShowInInspector]
        private List<MineData> MineDatas;
    }

    /// <summary>
    /// ������������
    /// </summary>
    public class MineData
    {
        [LabelText("����ID"), ReadOnly, ShowInInspector]
        private string MineralMapID;
        [LabelText("����λ��"), ReadOnly, ShowInInspector]
        private Vector2 position;
        [LabelText("ʣ�࿪�ɴ���"), ReadOnly, ShowInInspector]
        private int RemainMineNum;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public class MainIslandData
    {
        [LabelText("�ƶ��ٶ�"), ReadOnly, ShowInInspector]
        private float moveSpeed;
        [LabelText("����λ��"), ReadOnly, ShowInInspector]
        private Vector2 position;
        [LabelText("Ŀ��λ��"), ReadOnly, ShowInInspector]
        private Transform TargetTransform;
        [LabelText("��ǰ���ڵĵ�ͼ��"), ReadOnly, ShowInInspector]
        private int curMineLayer;
        [LabelText("��ǰ���ڵĴ��ͼ����ID"), ReadOnly, ShowInInspector]
        private string curMapRegionID;
    }

}
