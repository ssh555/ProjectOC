using Sirenix.OdinInspector;
using System;
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
            [LabelText("区块号"), ReadOnly, ShowInInspector]
            public int MapRegionNo;
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
                MapRegionNo = int.Parse(MapRegionID.Split('_')[1]);
            }
        }

        /// <summary>
        /// 小地图数据（采矿地图数据）
        /// </summary>
        public class MineralMapData
        {
            [LabelText("采矿地图ID"), ReadOnly, ShowInInspector]
            public string MineralMapID;
            [LabelText("采矿地图中的矿物数据"), ReadOnly, ShowInInspector]
            public List<MineData> MineDatas;
            [LabelText("采矿地图背景素材"), ReadOnly, ShowInInspector]
            public Texture2D texture2D;

            public MineralMapData(string mineralMapID, List<MineData> mineDatas, Texture2D texture2D)
            {
                MineralMapID = mineralMapID;
                MineDatas = mineDatas;
                this.texture2D = texture2D;
            }
        }

        /// <summary>
        /// 所有生产节点的矿圈数据
        /// </summary>
        public class PlaceCircleData
        {
            [LabelText("所属生产节点ID"), ReadOnly, ShowInInspector]
            public string ProNodeID;
            [LabelText("小地图索引元组(区块号,层数号)"), ReadOnly, ShowInInspector]
            public (int,int) SmallMapTuple;
            [LabelText("采矿圈中心位置"), ReadOnly, ShowInInspector]
            public Vector2 PlaceCirclePosition;
            [LabelText("是否放置了采矿圈"), ReadOnly, ShowInInspector]
            public bool isPlacedCircle;

            public PlaceCircleData(string proNodeID,(int,int) smallMapTuple)
            {
                ProNodeID = proNodeID;
                SmallMapTuple = smallMapTuple;
                isPlacedCircle = true;
            }
        }

        /// <summary>
        /// 单个矿物数据
        /// </summary>
        public class MineData
        {
            [LabelText("矿物ID"), ReadOnly, ShowInInspector]
            private string mineID;
            public string MineID { get { return mineID; } }
            [LabelText("矿物位置"), ReadOnly, ShowInInspector]
            private Vector2 position;
            public Vector2 Position { get { return position; } }
            [LabelText("剩余开采次数"), ReadOnly, ShowInInspector]
            private int remainMineNum;
            public int RemianMineNum { get { return remainMineNum; } }
            [LabelText("一次开采获取数量"), ReadOnly, ShowInInspector]
            private List<ML.Engine.InventorySystem.Formula> gainItems;
            public List<ML.Engine.InventorySystem.Formula> GainItems => gainItems;
            //该矿物所属的区块号
            private int regionNum;
            public int RegionNum { get { return regionNum; }set { regionNum = value; } }
            //该矿物所属的层次号
            private int layerNum;
            public int LayerNum { get { return layerNum; } set { layerNum = value; } }
            public MineData(string mineralMapID, Vector2 position, int remainMineNum, List<ML.Engine.InventorySystem.Formula> gainItems)
            {
                mineID = mineralMapID;
                this.position = position;
                this.remainMineNum = remainMineNum;
                this.gainItems = gainItems;
                proNodeCnt = 0;
            }

            /// <summary>
            /// 当矿物采集完一次调用一次该函数 返回true表示该矿还可以继续采集 返回false表示该矿不能继续采集
            /// </summary>
            public bool Consume()
            {
                lock(this)
                {
                    if (remainMineNum >= 1)
                    {
                        remainMineNum--;
                        return true;
                    }
                    return false;
                }
            }
            private int proNodeCnt;

            public int ProNodeCnt { get { return proNodeCnt; } }

            /// <summary>
            /// 注册
            /// </summary>
            public void RegisterProNode()
            {
                lock (this)
                {
                    proNodeCnt++;
                }
            }
            /// <summary>
            /// 注销
            /// </summary>
            public void UnRegisterProNode()
            {
                lock (this)
                {
                    proNodeCnt--;
                }
            }
        }

        /// <summary>
        /// 主岛数据
        /// </summary>
        public class MainIslandData
        {
            [LabelText("移动速度"), ReadOnly, ShowInInspector]
            private float moveSpeed;
            [LabelText("主岛位置"), ReadOnly, ShowInInspector]
            private Vector2 curPos;
            public Vector2 CurPos { get { return curPos; } set { curPos = value; } }
            [LabelText("目标位置"), ReadOnly, ShowInInspector]
            private Vector2 targetPos;
            public Vector2 TargetPos { get { return targetPos; } set {  targetPos = value; } }
            [LabelText("是否在移动"), ReadOnly, ShowInInspector]
            private bool isMoving;
            public bool IsMoving { get { return isMoving; } set { isMoving = value; OnisMovingChanged?.Invoke(value); } }
            public event Action<bool> OnisMovingChanged;
            [LabelText("当前所在的地图层"), ReadOnly, ShowInInspector]
            private int curMineLayer;
            [LabelText("当前所在的大地图区块ID"), ReadOnly, ShowInInspector]
            private string curMapRegionID;

            private Vector2 lastPos;
            public Vector2 LastPos { get { return lastPos; } }
            public Vector2 MovingDir { get { return (targetPos - curPos).normalized; } }
            private bool isPause;
            public bool IsPause { get { return isPause; } set { isPause = value; } }
            public MainIslandData(float moveSpeed)
            {
                this.moveSpeed = moveSpeed;
                lastPos = curPos;
                isPause = false;
            }
            
            public bool isReachTarget { 
            get {
                    if(isPause) return false;
                    if (Vector2.Distance(curPos, targetPos)<1f)
                    {
                        IsMoving = false;
                        return true;
                    }
                    if(IsMoving)
                    {
                        lastPos = curPos;
                        curPos += moveSpeed * (targetPos - curPos).normalized;
                    }
                        
                    return false;
                }
            }

            public void Reset()
            {
                curPos = lastPos;
                targetPos = curPos;
                IsMoving = false;
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


