using ML.Engine.Manager;
using ML.Engine.Timer;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.Order;
using ProjectOC.Player;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static ProjectOC.MineSystem.MineSystemData;
using static ProjectOC.Order.OrderManager;

namespace ProjectOC.MineSystem
{
    [System.Serializable]
    public class MineSystemManager : ML.Engine.Manager.LocalManager.ILocalManager, ITickComponent
    {
        [LabelText("大地图缩放比例"), ReadOnly, ShowInInspector]
        private float gridScale;
        public float GridScale {  get { return gridScale; } set {  gridScale = value; } }
        [LabelText("区块列表"), ReadOnly, ShowInInspector]
        private List<MapRegionData> mapRegionDatas;
        public List<MapRegionData> MapRegionDatas { get { return mapRegionDatas; } }
        [LabelText("大地图地图层解锁数组"), ReadOnly, ShowInInspector]
        private bool[] isUnlockIslandMap;
        [LabelText("主岛数据"), ReadOnly, ShowInInspector]
        private MainIslandData mainIslandData;
        public MainIslandData MainIslandData { get {  return mainIslandData; } }
        [LabelText("小地图数据"), ReadOnly, ShowInInspector]
        private Dictionary<string, MineralMapData> mineralMapDatas;

        [LabelText("大地图预制体"), ReadOnly, ShowInInspector]
        private GameObject BigMapPrefab;

        /// <summary>
        /// 当前选中的大地图层
        /// </summary>
        private int curMapLayerIndex;
        /// <summary>
        /// 岛舵台ui refresh
        /// </summary>
        public event Action RefreshUI;
        /// <summary>
        /// id , mapregiondata
        /// </summary>
        private Dictionary<string, MapRegionData> IDToMapRegionDic = new Dictionary<string, MapRegionData>(); 


        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {
            //触发get
            var isReachTarget = mainIslandData.isReachTarget;
            //主岛移动触发ui refresh
            if (mainIslandData.IsMoving && !isReachTarget) 
            {
                RefreshUI?.Invoke();
            }
        }
        #endregion
        #region Base
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;

        /// <summary>
        /// 单例管理
        /// </summary>
        public static MineSystemManager Instance { get { return instance; } }

        private static MineSystemManager instance;
        public void Init()
        {
            //初始化大地图缩放比例
            this.GridScale = 1;

            //初始化区块列表
            GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Mine_UIPrefab/Prefab_MineSystem_UI_BigMap.prefab").Completed += (handle) =>
            {
                this.BigMapPrefab = handle.Result;

                this.mapRegionDatas = new List<MapRegionData>();

                var Layer = this.BigMapPrefab.transform.Find("Layer");

                for(int i =0;i< Layer.childCount; i++)
                {
                    var child = Layer.GetChild(i);
                    MapRegionData  mapRegionData= new MapRegionData(child.name,false, child.GetComponent<RectTransform>().anchoredPosition);
                    this.mapRegionDatas.Add(mapRegionData);
                    IDToMapRegionDic.Add(child.name, mapRegionData);
                }
            };

            //初始化大地图地图层解锁数组
            isUnlockIslandMap = new bool[MineSystemData.MAPDEPTH];



            //初始化主岛数据
            mainIslandData = new MainIslandData();


            //初始化小地图数据

            //默认选中
            curMapLayerIndex = 0;

        }
        public void OnRegister()
        {
            if (instance == null)
            {
                instance = this;
                ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
                Init();
            }
        }

        public void OnUnregister()
        {
            if (instance == this)
            {
                instance = null;
                ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            }
        }
        #endregion

        #region External
        /// <summary>
        /// 设置导航点 返回true代表成功 返回false代表当前主岛在移动 isCancel表示是否打断目前路径
        /// </summary>
        public bool SetNewNavagatePoint(Vector3 pos,bool isCancel = false)
        {
            Debug.Log("SetNewNavagatePoint "+ pos);
            if (isCancel)
            {
                mainIslandData.TargetPos = pos;
                mainIslandData.IsMoving = true;
                return true;
            }

            if (mainIslandData.IsMoving)
            {
                return false;
            }

            mainIslandData.TargetPos = pos;
            mainIslandData.IsMoving = true;
            return true;
        }

        public void UnlockMapRegion(string ID)
        {
            if (!this.IDToMapRegionDic.ContainsKey(ID)) return;
            IDToMapRegionDic[ID].isUnlockLayer[curMapLayerIndex] = true;
        }


        #endregion

    }
}


