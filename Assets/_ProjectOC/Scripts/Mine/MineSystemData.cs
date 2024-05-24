using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.MineSystem.MineSystemData;
using static ProjectOC.Order.OrderManager;
/// <summary>
/// 采矿系统数据
/// </summary>
namespace ProjectOC.MineSystem
{
    public class MineSystemData
    {
        public const int MAPDEPTH = 3;

        /// <summary>
        /// 大地图存档数据
        /// </summary>
        public class IslandMapData
        {
            [LabelText("大地图层数"), ReadOnly, ShowInInspector]
            public int MAPDEPTH = MineSystemData.MAPDEPTH;
            [LabelText("大地图缩放比例"), ReadOnly, ShowInInspector]
            public float GridScale;
            [LabelText("区块列表"), ReadOnly, ShowInInspector]
            public List<MapRegionData> MapRegionDatas;
            [LabelText("大地图地图层解锁数组"), ReadOnly, ShowInInspector]
            public bool[] isUnlockIslandMap = new bool[MineSystemData.MAPDEPTH];
            [LabelText("主岛数据"), ReadOnly, ShowInInspector]
            public MainIslandData mainIslandData;
            [LabelText("小地图数据"), ReadOnly, ShowInInspector]
            public Dictionary<string, MineralMapData> mineralMapDatas;
        }

        /// <summary>
        /// 区块数据
        /// </summary>
        public class MapRegionData
        {
            [LabelText("区块ID"), ReadOnly, ShowInInspector]
            public string MapRegionID;
            [LabelText("是否为障碍物"), ReadOnly, ShowInInspector]
            public bool IsBlock;
            [LabelText("区块地图层解锁数组"), ReadOnly, ShowInInspector]
            public bool[] isUnlockLayer;
            [LabelText("小地图数组"), ReadOnly, ShowInInspector]
            public string[] mineralDataID;

            public MapRegionData(string mapRegionID, bool isBlock)
            {
                MapRegionID = mapRegionID;
                IsBlock = isBlock;
                this.isUnlockLayer = new bool[MineSystemData.MAPDEPTH];
                this.mineralDataID = new string[MineSystemData.MAPDEPTH];
            }
        }

        /// <summary>
        /// 小地图数据（采矿地图数据）
        /// </summary>
        public class MineralMapData
        {
            [LabelText("采矿地图ID"), ReadOnly, ShowInInspector]
            public string MineralMapID;
/*            [LabelText("采矿地图预制体资产路径"), ReadOnly, ShowInInspector]
            public string PrefabPath;*/
            [LabelText("采矿地图中的矿物数据"), ReadOnly, ShowInInspector]
            public List<MineData> MineDatas;

            public MineralMapData(string mineralMapID, List<MineData> mineDatas)
            {
                MineralMapID = mineralMapID;
                MineDatas = mineDatas;
            }
        }

        /// <summary>
        /// 单个矿物数据
        /// </summary>
        public class MineData
        {
            [LabelText("矿物ID"), ReadOnly, ShowInInspector]
            public string MineID;
            [LabelText("矿物位置"), ReadOnly, ShowInInspector]
            public Vector2 position;
            [LabelText("剩余开采次数"), ReadOnly, ShowInInspector]
            public int RemainMineNum;

            public MineData(string mineralMapID, Vector2 position, int remainMineNum)
            {
                MineID = mineralMapID;
                this.position = position;
                RemainMineNum = remainMineNum;
            }
        }

        /// <summary>
        /// 主岛数据
        /// </summary>
        public class MainIslandData
        {
            [LabelText("移动速度"), ReadOnly, ShowInInspector]
            private float moveSpeed = 1;
            [LabelText("主岛位置"), ReadOnly, ShowInInspector]
            private Vector2 curPos;
            public Vector2 CurPos { get { return curPos; }  }
            [LabelText("目标位置"), ReadOnly, ShowInInspector]
            private Vector2 targetPos;
            public Vector2 TargetPos { get { return targetPos; } set {  targetPos = value; } }
            [LabelText("是否在移动"), ReadOnly, ShowInInspector]
            private bool isMoving;
            public bool IsMoving { get { return isMoving; } set { isMoving = value; } }
            [LabelText("当前所在的地图层"), ReadOnly, ShowInInspector]
            private int curMineLayer;
            [LabelText("当前所在的大地图区块ID"), ReadOnly, ShowInInspector]
            private string curMapRegionID;

            public bool isReachTarget { 
            get {
                    if (Vector2.Distance(curPos, targetPos)<1f)
                    {
                        
                        isMoving = false;
                        return true;
                    }
                    if(isMoving)
                        curPos += moveSpeed * (targetPos - curPos).normalized;
                    return false;
                }
            }
        }

        /// <summary>
        /// 矿物表数据
        /// </summary>
        [System.Serializable]
        public struct MineralTableData
        {
            public string ID;
            public string Icon;
            public List<ML.Engine.InventorySystem.Formula> MineEff;
            public int MineNum;
        }
    }
}


