using ML.Engine.Manager;
using ML.Engine.Timer;
using ML.Engine.UI;
using ML.Engine.Utility;
using Newtonsoft.Json;
using ProjectOC.ManagerNS;
using ProjectOC.Order;
using ProjectOC.Player;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static ProjectOC.MineSystem.MineSystemData;
using static ProjectOC.Order.OrderManager;

namespace ProjectOC.MineSystem
{
    [System.Serializable]
    public class MineSystemManager : ML.Engine.Manager.LocalManager.ILocalManager, ITickComponent
    {
        [LabelText("���ͼ���ű���"), ReadOnly, ShowInInspector]
        private float gridScale;
        public float GridScale {  get { return gridScale; } set {  gridScale = value; } }
        [LabelText("�����б�"), ReadOnly, ShowInInspector]
        private List<MapRegionData> mapRegionDatas;
        public List<MapRegionData> MapRegionDatas { get { return mapRegionDatas; } }
        [LabelText("���ͼ��ͼ���������"), ReadOnly, ShowInInspector]
        private bool[] isUnlockIslandMap;
        [LabelText("��������"), ReadOnly, ShowInInspector]
        private MainIslandData mainIslandData;
        public MainIslandData MainIslandData { get {  return mainIslandData; } }
        [LabelText("С��ͼ����"), ReadOnly, ShowInInspector]
        private Dictionary<string, MineralMapData> mineralMapDatas = new Dictionary<string, MineralMapData>();

        [LabelText("���ͼԤ����"), ReadOnly, ShowInInspector]
        private GameObject BigMapPrefab;

        /// <summary>
        /// ��ǰѡ�еĴ��ͼ��
        /// </summary>
        private int curMapLayerIndex;
        [ShowInInspector]
        public int CurMapLayerIndex { get { return curMapLayerIndex; } set { curMapLayerIndex = value; } }
        /// <summary>
        /// ����̨ui refresh
        /// </summary>
        public event Action RefreshUI;
        /// <summary>
        /// id , mapregiondata
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, MapRegionData> IDToMapRegionDic = new Dictionary<string, MapRegionData>();
        /// <summary>
        ///  RegionNum, mapregiondata
        /// </summary>
        [ShowInInspector]
        private Dictionary<int, MapRegionData> RegionNumToRegionDic = new Dictionary<int, MapRegionData>();
        private int curRegionNum = -1;
        [ShowInInspector]
        public int CurRegionNum { get { return curRegionNum; } }
        [ShowInInspector]
        private int lastRegionNum = -1;


        #region SmallMap
        [ShowInInspector]
        private MineralMapData mineralMapData;
        #endregion


        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {
            //����get
            var isReachTarget = mainIslandData.isReachTarget;
            //�����ƶ�����ui refresh
            if (mainIslandData.IsMoving && !isReachTarget) 
            {
                RefreshUI?.Invoke();
            }
        }
        #endregion
        #region Base
        SynchronizerInOrder synchronizerInOrder;
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;

        /// <summary>
        /// ��������
        /// </summary>
        public static MineSystemManager Instance { get { return instance; } }

        private static MineSystemManager instance;
        public void Init()
        {
            #region �첽��ʼ��

            synchronizerInOrder = new SynchronizerInOrder(4, () => {
                for (int i = 0; i < this.mapRegionDatas.Count; i++)
                {
                    for (int j = 0; j < mapRegionDatas[i].mineralDataID.Length; j++)
                    {
                        mapRegionDatas[i].mineralDataID[j] = SmallMapDatas[i * MineSystemData.MAPDEPTH + j].name;
                    }
                }
            });

            //0 ��ʼ��MineralTableData
            synchronizerInOrder.AddCheckAction(0, () => {
                LoadMineralTableData();
            });

            //1 ��ʼ��BigMapTableData
            synchronizerInOrder.AddCheckAction(1, () => {
                LoadBigMapTableData();
            });

            //2 ��ʼ����ʼ�������б�
            synchronizerInOrder.AddCheckAction(2, () => {
                LoadMapRegionData();
            });

            //3 ��ȡС��ͼ���������
            synchronizerInOrder.AddCheckAction(3, () => {
                LoadSmallMapMineData();
            });
            synchronizerInOrder.StartExecution();
            #endregion

            #region ͬ����ʼ��
            //��ʼ�����ͼ���ű���
            this.GridScale = 1;

            //��ʼ�����ͼ��ͼ���������
            isUnlockIslandMap = new bool[MineSystemData.MAPDEPTH];

            //��ʼ����������
            mainIslandData = new MainIslandData();

            //Ĭ��ѡ��
            curMapLayerIndex = 0;
            #endregion
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

        #region LoadData
        [ShowInInspector]
        private List<MineSmallMapEditData> SmallMapDatas;
        private Dictionary<string, MineralTableData> MineralTableDataDic = new Dictionary<string, MineralTableData>();

        //�߻����ͼ����
        private string bigMapDataJson = "Assets/_ProjectOC/OCResources/Json/TableData/WorldMap.json";
        private string _jsonData;
        private int[,] bigMapTableData;
        public int[,] BigMapTableData { get { return bigMapTableData; } }
        private void LoadSmallMapMineData()
        {
            GameManager.Instance.ABResourceManager.LoadAssetsAsync<MineSmallMapEditData>("Config_Mine_MineEditorData", 
                (smd) => {
                    List<MineData> MineDatas = new List<MineData>();
                    foreach (var mine in smd.mineData)
                    {
                        foreach (var pos in mine.MinePoses)
                        {
                            if(this.MineralTableDataDic.ContainsKey(mine.MineID))
                            {
                                int RemainNum = this.MineralTableDataDic[mine.MineID].MineNum;
                                MineDatas.Add(new MineData(mine.MineID, pos, RemainNum));
                            }
                        }
                    }
                    MineralMapData mineralMapData = new MineralMapData(smd.name, MineDatas);
                    mineralMapDatas.Add(smd.name, mineralMapData); 
                }
                ).Completed += 
                (handle) =>
                {
                    SmallMapDatas = handle.Result.ToList<MineSmallMapEditData>();
                    SmallMapDatas.Sort((MineSmallMapEditData a, MineSmallMapEditData b) => 
                    {
                        int t1 = Convert.ToInt32(a.name.Split('_')[1]);
                        int t2 = Convert.ToInt32(b.name.Split('_')[1]);
                        return t1.CompareTo(t2);
                    });
                    synchronizerInOrder.Check(3);
                };
        }

        private void LoadMineralTableData()
        {
            ML.Engine.ABResources.ABJsonAssetProcessor<MineralTableData[]> ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<MineralTableData[]>("OCTableData", "Mineral", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.MineralTableDataDic.Add(data.ID, data);
                }
            }, "��������");
            ABJAProcessor.StartLoadJsonAssetData();
            synchronizerInOrder.Check(0);
        }

        private void LoadBigMapTableData()
        {
            //�߻����ͼ����
            _jsonData = File.ReadAllText(bigMapDataJson);
            bigMapTableData = JsonConvert.DeserializeObject<int[,]>(_jsonData);
            synchronizerInOrder.Check(1);
        }
        private void LoadMapRegionData()
        {
            //��ʼ�������б�
            GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Mine_UIPrefab/Prefab_MineSystem_UI_BigMap.prefab").Completed += (handle) =>
            {
                this.BigMapPrefab = handle.Result;
                this.mapRegionDatas = new List<MapRegionData>();
                var NormalRegion = this.BigMapPrefab.transform.Find("NormalRegion");
                for (int i = 0; i < NormalRegion.childCount; i++)
                {
                    var child = NormalRegion.GetChild(i);
                    MapRegionData mapRegionData = new MapRegionData(child.name, false);
                    this.mapRegionDatas.Add(mapRegionData);
                    IDToMapRegionDic.Add(child.name, mapRegionData);
                    RegionNumToRegionDic.Add(i + 1, mapRegionData);
                }
                var BlockRegion = this.BigMapPrefab.transform.Find("BlockRegion");
                for (int i = 0; i < BlockRegion.childCount; i++)
                {
                    var child = BlockRegion.GetChild(i);
                    MapRegionData mapRegionData = new MapRegionData(child.name, true);
                    IDToMapRegionDic.Add(child.name, mapRegionData);
                    RegionNumToRegionDic.Add(0, mapRegionData);
                }
                synchronizerInOrder.Check(2);
            };
        }
        #endregion

        #region External
        /// <summary>
        /// ���õ����� ����true����ɹ� ����false����ǰ�������ƶ� isCancel��ʾ�Ƿ���Ŀǰ·��
        /// </summary>
        public bool SetNewNavagatePoint(Vector3 pos,bool isCancel = false)
        {
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

        public void UnlockMapRegion(int RegionNum)
        {
            if (!this.RegionNumToRegionDic.ContainsKey(RegionNum)) return;

            //�����ϰ�
            if (RegionNum == 0)
            {
                mainIslandData.Reset();
                curRegionNum = lastRegionNum;
                return;
            }

            lastRegionNum = curRegionNum;
            curRegionNum = RegionNum;
            RegionNumToRegionDic[RegionNum].isUnlockLayer[curMapLayerIndex] = true;
        }

        public bool CheckRegionIsUnlocked(int RegionNum)
        {
            if (!this.RegionNumToRegionDic.ContainsKey(RegionNum)) return false;
            return RegionNumToRegionDic[RegionNum].isUnlockLayer[curMapLayerIndex];
        }

        public void ChangeCurMineralMapData(int curSelectRegion)
        {
            Debug.Log("GetCurMineralMapData " + RegionNumToRegionDic.ContainsKey(curSelectRegion) + " " + curSelectRegion);
            if(!RegionNumToRegionDic.ContainsKey(curSelectRegion))
            {
                this.mineralMapData = null;
                return;
            }
            if(!CheckRegionIsUnlocked(curSelectRegion))
            {
                this.mineralMapData = null;
                return;
            }

            string MineralMapDataID = RegionNumToRegionDic[curSelectRegion].mineralDataID[CurMapLayerIndex];
            Debug.Log(MineralMapDataID);
            if(!mineralMapDatas.ContainsKey(MineralMapDataID) )
            {
                this.mineralMapData = null;
                return;
            }
            this.mineralMapData =  mineralMapDatas[MineralMapDataID];
        }
        #endregion
    }
}


