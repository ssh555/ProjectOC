using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 采矿系统数据
/// </summary>
public class MineSystemData
{
    const int MAPDEPTH = 3;

    /// <summary>
    /// 大地图存档数据
    /// </summary>
    public class IslandMapData
    {
        [LabelText("大地图层数"), ReadOnly, ShowInInspector]
        private int MAPDEPTH = MineSystemData.MAPDEPTH;
        [LabelText("大地图缩放比例"), ReadOnly, ShowInInspector]
        private float GridScale;
        [LabelText("区块列表"), ReadOnly, ShowInInspector]
        private List<MapRegionData> MapRegionDatas;
        [LabelText("大地图地图层解锁数组"), ReadOnly, ShowInInspector]
        private bool[] isUnlockIslandMap = new bool[MineSystemData.MAPDEPTH];
        [LabelText("主岛数据"), ReadOnly, ShowInInspector]
        private MainIslandData mainIslandData;
        [LabelText("小地图数据"), ReadOnly, ShowInInspector]
        private Dictionary<string,MineralMapData> mineralMapDatas;
    }

    /// <summary>
    /// 区块数据
    /// </summary>
    public class MapRegionData
    {
        [LabelText("区块ID"), ReadOnly, ShowInInspector]
        private string MapRegionID;
        [LabelText("是否为障碍物"), ReadOnly, ShowInInspector]
        private bool IsBlock;
        [LabelText("区块位置"), ReadOnly, ShowInInspector]
        private Vector2 position;
        [LabelText("区块预制体资产路径"), ReadOnly, ShowInInspector]
        private string PrefabPath;
        [LabelText("区块地图层解锁数组"), ReadOnly, ShowInInspector]
        private bool[] isUnlockLayer = new bool[MineSystemData.MAPDEPTH];
        [LabelText("小地图数组"), ReadOnly, ShowInInspector]
        private string[] mineralDataID = new string[MineSystemData.MAPDEPTH];
    }

    /// <summary>
    /// 小地图数据（采矿地图数据）
    /// </summary>
    public class MineralMapData
    {
        [LabelText("采矿地图ID"), ReadOnly, ShowInInspector]
        private string MineralMapID;
        [LabelText("采矿地图预制体资产路径"), ReadOnly, ShowInInspector]
        private string PrefabPath;
        [LabelText("采矿地图中的矿物数据"), ReadOnly, ShowInInspector]
        private List<MineData> MineDatas;
    }

    /// <summary>
    /// 单个矿物数据
    /// </summary>
    public class MineData
    {
        [LabelText("矿物ID"), ReadOnly, ShowInInspector]
        private string MineralMapID;
        [LabelText("矿物位置"), ReadOnly, ShowInInspector]
        private Vector2 position;
        [LabelText("剩余开采次数"), ReadOnly, ShowInInspector]
        private int RemainMineNum;
    }

    /// <summary>
    /// 主岛数据
    /// </summary>
    public class MainIslandData
    {
        [LabelText("移动速度"), ReadOnly, ShowInInspector]
        private float moveSpeed;
        [LabelText("主岛位置"), ReadOnly, ShowInInspector]
        private Vector2 position;
        [LabelText("目标位置"), ReadOnly, ShowInInspector]
        private Transform TargetTransform;
        [LabelText("当前所在的地图层"), ReadOnly, ShowInInspector]
        private int curMineLayer;
        [LabelText("当前所在的大地图区块ID"), ReadOnly, ShowInInspector]
        private string curMapRegionID;
    }

}
