using ML.Engine.Manager;
using ML.Engine.Timer;
using ML.Engine.UI;
using ML.Engine.Utility;
using Newtonsoft.Json;
using ProjectOC.ProNodeNS;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using static ProjectOC.MineSystem.MineSystemData;

namespace ProjectOC.MineSystem
{
    [System.Serializable]
    public class MineSystemManager : ML.Engine.Manager.LocalManager.ILocalManager, ITickComponent
    {
        #region BigMap
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

        #endregion

        #region SmallMap
        [ShowInInspector]
        private MineralMapData mineralMapData;
        public MineralMapData MineralMapData { get { return mineralMapData; } }

        /// <summary>
        ///��ǰ��Ȧ��Ȧס�Ŀ��Ｏ��
        /// </summary>
        private List<MineData> curMiningData = new List<MineData>();
        [ShowInInspector]
        public List<MineData> CurMiningData { get { return curMiningData; } set { curMiningData = value; } }

        /// <summary>
        /// ��¼���������ڵ�Ŀ�Ȧλ������  С��ͼ����-> ��С��ͼ��PlaceCircleData�ֵ�
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, PlaceCircleData> PlacedCircleDataDic = new Dictionary<string, PlaceCircleData>();
        #endregion

        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {
            if(mainIslandData != null)
            {
                //����get
                var isReachTarget = mainIslandData.isReachTarget;
                //�����ƶ�����ui refresh
                if (mainIslandData.IsMoving && !isReachTarget)
                {
                    MainIslandRectTransform.anchoredPosition = mainIslandData.CurPos;
                    if(isRectTransformInit)
                    {
                        DetectMainIslandCurRegion();
                    }
                    RefreshUI?.Invoke();
                }
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

        #region ����̨UI
        private IslandRudderPanel islandRudderPanelInstance;
        public IslandRudderPanel IslandRudderPanelInstance { get { return islandRudderPanelInstance; } }
        private GameObject bigMapInstance;
        public GameObject BigMapInstance { get { return bigMapInstance; } }
        #endregion 

        public void Init()
        {
            #region �첽��ʼ��
            synchronizerInOrder = new SynchronizerInOrder(5, () => {
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

            //2 ��ʼ�������б�
            synchronizerInOrder.AddCheckAction(2, () => {
                LoadMapRegionData();
            });

            //3 ��ȡС��ͼ����ͼ������
            synchronizerInOrder.AddCheckAction(3, () => {
                LoadSmallMapTexture2DData();
            });

            //4 ��ȡС��ͼ���������
            synchronizerInOrder.AddCheckAction(4, () => {
                LoadSmallMapMineData();
            });

            synchronizerInOrder.StartExecution();

            LoadTechAtlas();

            #region ����̨UI
            //��ʼ���������

            GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Mine_UIPanel/Prefab_Mine_UI_IslandRudderPanel.prefab").Completed += (handle) =>
            {
                islandRudderPanelInstance = handle.Result.GetComponent<IslandRudderPanel>();
                islandRudderPanelInstance.gameObject.SetActive(false);
                MainIslandRectTransform = handle.Result.transform.Find("GraphCursorNavigation").Find("Scroll View").Find("Viewport").Find("Content").Find("MainIsland").GetComponent<RectTransform>();
                ColliderRadiu = MainIslandRectTransform.GetComponent<CircleCollider2D>().radius;
                IslandRudderGraphCursorNavigation = handle.Result.transform.Find("GraphCursorNavigation").GetComponent<GraphCursorNavigation>();
                GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Mine_UIPrefab/Prefab_MineSystem_UI_BigMap.prefab").Completed += (handle) =>
                {
                    bigMapInstance = handle.Result;
                    var BigMapInstanceTrans = bigMapInstance.transform;
                    MapRegionRectTransform = BigMapInstanceTrans.Find("NormalRegion") as RectTransform;
                    var cursorNavigation = islandRudderPanelInstance.transform.Find("GraphCursorNavigation").GetComponent<GraphCursorNavigation>();
                    BigMapInstanceTrans.SetParent(cursorNavigation.Content.Find("BigMap"));
                    BigMapInstanceTrans.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

                    cursorNavigation.CurZoomscale = GridScale;
                    cursorNavigation.NavagationSpeed = MineSystemConfig.IslandRudderSensitivity;
                    cursorNavigation.ZoomSpeed = MineSystemConfig.IslandRudderZoomSpeed;
                    cursorNavigation.ZoomInLimit = MineSystemConfig.IslandRudderZoomInLimit;
                    cursorNavigation.ZoomOutLimit = MineSystemConfig.IslandRudderZoomOutLimit;

                    isRectTransformInit = true;
                    CalculateMainIslandWorldPos();
                    DetectMainIslandCurRegion();
                };
            };
            #endregion


            #endregion

            #region ͬ����ʼ��
            //��ʼ�����ͼ���ű���
            this.GridScale = (MineSystemConfig.IslandRudderZoomInLimit - MineSystemConfig.IslandRudderZoomOutLimit) * MineSystemConfig.IslandRudderInitZoomRate + MineSystemConfig.IslandRudderZoomOutLimit; ;

            //��ʼ�����ͼ��ͼ���������
            isUnlockIslandMap = new bool[MineSystemData.MAPDEPTH];

            


            //Ĭ��ѡ��
            curMapLayerIndex = 0;
            #endregion
        }

        private MineSystemConfig mineSystemConfig;
        public MineSystemConfig MineSystemConfig { get { return mineSystemConfig; }  }
        public void OnRegister()
        {
            if (instance == null)
            {
                instance = this;
                ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<MineSystemConfigAsset>("Config_Mine").Completed += (handle) =>
                {
                    mineSystemConfig = new MineSystemConfig(handle.Result.Config);
                    Init();
                };
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

        //С��ͼ�����ز�
        private Dictionary<int,Texture2D> IndexToTextureDic = new Dictionary<int,Texture2D>();

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
                                MineDatas.Add(new MineData(mine.MineID, pos, RemainNum, MineralTableDataDic[mine.MineID].MineEff));
                            }
                        }
                    }
                    Texture2D texture2D = this.IndexToTextureDic[int.Parse(smd.name.Split('_')[1])];
                    MineralMapData mineralMapData = new MineralMapData(smd.name, MineDatas, texture2D);
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
                    synchronizerInOrder.Check(4);
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
                    string regionNumstr = child.name.Split('_')[1];
                    int regionNum;
                    bool isRegionNum = int.TryParse(regionNumstr, out regionNum);
                    if(isRegionNum)
                    {
                        MapRegionData mapRegionData = new MapRegionData(child.name, true);
                        IDToMapRegionDic.Add(child.name, mapRegionData);
                        RegionNumToRegionDic.Add(regionNum, mapRegionData);
                    }
                }
                synchronizerInOrder.Check(2);
            };
        }

        private void LoadSmallMapTexture2DData()
        {
            GameManager.Instance.ABResourceManager.LoadAssetsAsync<Texture2D>("Texture2D_Mine_SmallMapTex",
                (texture2D) => {
                    int index = int.Parse(texture2D.name.Split('_')[2]);
                    IndexToTextureDic.Add(index, texture2D);
                }
                ).Completed +=
                (handle) =>
                {
                    synchronizerInOrder.Check(3);
                };
        }

        private SpriteAtlas mineAtlas;
        private void LoadTechAtlas()
        {
            GM.ABResourceManager.LoadAssetAsync<SpriteAtlas>("SA_Mine_UI").Completed += (handle) =>
            {
                mineAtlas = handle.Result as SpriteAtlas;
            };
        }
        #endregion

        #region Internal
        #region ����λ�ü��
        private int curRegionNum = -1;
        [ShowInInspector]
        public int CurRegionNum { get { return curRegionNum; } }
        [ShowInInspector]
        private int lastRegionNum = -1;
        private int CurColliderPointRegion = -1;
        private int PreColliderPointRegion = -1;
        private float ColliderRadiu;

        private RectTransform MapRegionRectTransform;
        private RectTransform MainIslandRectTransform;
        private GraphCursorNavigation IslandRudderGraphCursorNavigation;
        private bool isRectTransformInit = false;

        private void DetectMainIslandCurRegion()
        {
            PreColliderPointRegion = CurColliderPointRegion;
            CurColliderPointRegion = DetectRegion(MainIslandRectTransform.position + (Vector3)mainIslandData.MovingDir * ColliderRadiu * IslandRudderGraphCursorNavigation.CurZoomRate);
            if (PreColliderPointRegion != CurColliderPointRegion && CurColliderPointRegion <= 0)
            {
                //�ϰ���ײ
                UnlockMapRegion(CurColliderPointRegion);
                return;
            }
            lastRegionNum = curRegionNum;
            curRegionNum = DetectRegion(MainIslandRectTransform.position);
            if(curRegionNum == -1)
            {
                //�߽���ײ
                mainIslandData.Reset();
                curRegionNum = lastRegionNum;
                return;
            }

            if (lastRegionNum != curRegionNum)
            {
                //����������
                UnlockMapRegion(curRegionNum);
                lastRegionNum = curRegionNum;
            }
        }


        [ShowInInspector]
        private int smallMapcurSelectRegion = -1;
        public int SmallMapCurSelectRegion { set { smallMapcurSelectRegion = value; } }
        public int DetectRegion(Vector3 pos)
        {
            Vector3 worldPosition = pos;
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(MapRegionRectTransform, worldPosition, null, out localPosition);
            Vector2 referenceSize = MapRegionRectTransform.rect.size;
            Vector2 anchorPosition = new Vector2(localPosition.x / referenceSize.x + 0.5f, localPosition.y / referenceSize.y + 0.5f);
            anchorPosition = new Vector2(anchorPosition.x, 1 - anchorPosition.y);
            int width = bigMapTableData.GetLength(0);
            Vector2Int gridPos = new Vector2Int(
            Mathf.Clamp((int)(anchorPosition.x * (width)), 0, width - 1),
            Mathf.Clamp((int)(anchorPosition.y * (width)), 0, width - 1));
            return bigMapTableData[gridPos.y, gridPos.x];
        }

        private void CalculateMainIslandWorldPos()
        {
            int width = bigMapTableData.GetLength(0);
            Vector2 anchorPosition = Vector2.zero;
            anchorPosition.x = mineSystemConfig.MainIslandInitPos.y / width;
            anchorPosition.y = mineSystemConfig.MainIslandInitPos.x / width;
            anchorPosition.y = 1 - anchorPosition.y;
            Vector2 referenceSize = MapRegionRectTransform.rect.size;
            Vector2 localPosition = Vector2.zero;
            localPosition.x = (anchorPosition.x - 0.5f) * referenceSize.x;
            localPosition.y = (anchorPosition.y - 0.5f) * referenceSize.y;
            Vector2 worldPosition = RectTransformUtility.WorldToScreenPoint(null, MapRegionRectTransform.TransformPoint(localPosition));
            // ����������ת��Ϊ��Ļ����
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
            // ����Ļ����ת��Ϊ RectTransform �ı�������
            RectTransformUtility.ScreenPointToLocalPointInRectangle(MainIslandRectTransform, screenPoint, null, out Vector2 localPoint);
            //��ʼ����������
            mainIslandData = new MainIslandData(mineSystemConfig.MainIslandSpeed, localPoint);
            MainIslandRectTransform.anchoredPosition = localPoint;
        }


        #endregion
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
            if (RegionNum <= 0)
            {
                mainIslandData.Reset();
                curRegionNum = lastRegionNum;
            }
            else
            {
                lastRegionNum = curRegionNum;
                curRegionNum = RegionNum;
            }
            RegionNumToRegionDic[RegionNum].isUnlockLayer[curMapLayerIndex] = true;
            if(this.islandRudderPanelInstance.gameObject.activeInHierarchy)
            {
                this.islandRudderPanelInstance.Refresh();
            }
        }

        public bool CheckRegionIsUnlocked(int RegionNum)
        {
            if (!this.RegionNumToRegionDic.ContainsKey(RegionNum)) return false;
            return RegionNumToRegionDic[RegionNum].isUnlockLayer[curMapLayerIndex];
        }

        public void ChangeCurMineralMapData(int curSelectRegion)
        {
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
            if(curSelectRegion <= 0)
            {
                return;
            }
            string MineralMapDataID = RegionNumToRegionDic[curSelectRegion].mineralDataID[CurMapLayerIndex];
            if(!mineralMapDatas.ContainsKey(MineralMapDataID) )
            {
                this.mineralMapData = null;
                return;
            }
            this.mineralMapData =  mineralMapDatas[MineralMapDataID];
        }

        /// <summary>
        /// �����Ȧ����
        /// </summary>
        public void AddMineralCircleData(Vector2 CirclePos,string ProNodeId, int curSlectNum)
        {
            PlaceCircleData placeCircleData = new PlaceCircleData(ProNodeId, (curSlectNum, curMapLayerIndex));
            placeCircleData.PlaceCirclePosition = CirclePos;
            if (this.PlacedCircleDataDic.ContainsKey(ProNodeId))
            {
                PlacedCircleDataDic.Remove(ProNodeId);
            }
            this.PlacedCircleDataDic.Add(ProNodeId, placeCircleData);
        }

        /// <summary>
        /// �����ڵ�����ʱ���� �Ƴ���Ȧ����
        /// </summary>
        public void RemoveMineralCircleData(string ProNodeId)
        {
            if (this.PlacedCircleDataDic.ContainsKey(ProNodeId))
            {
                this.PlacedCircleDataDic.Remove(ProNodeId);
            }
        }

        /// <summary>
        /// ��ȡ��Ȧ����
        /// </summary>
        public PlaceCircleData GetMineralCircleData(string ProNodeId,bool isCheckSmallMapcurSelectRegion = true)
        {
            
            if (this.PlacedCircleDataDic.ContainsKey(ProNodeId))
            {
                PlaceCircleData placeCircleData = this.PlacedCircleDataDic[ProNodeId];
                if ((isCheckSmallMapcurSelectRegion && placeCircleData.SmallMapTuple.Item1 == smallMapcurSelectRegion) || (!isCheckSmallMapcurSelectRegion)) 
                {
                    return placeCircleData;
                }
            }
            return null;
        }

        /// <summary>
        /// ��ȡ��Ȧ����
        /// </summary>
        public PlaceCircleData GetMineralCircleData(string ProNodeId)
        {
            if (this.PlacedCircleDataDic.ContainsKey(ProNodeId))
            {
                PlaceCircleData placeCircleData = this.PlacedCircleDataDic[ProNodeId];
                return this.PlacedCircleDataDic[ProNodeId];
            }
            return null;
        }

        /// <summary>
        /// ��ȡ����ͼ��
        /// </summary>
        public Sprite GetMineSprite(string MineId)
        {
            if (this.MineralTableDataDic.ContainsKey(MineId))
            {
                return this.mineAtlas.GetSprite(MineralTableDataDic[MineId].Icon);
            }
            return null;
        }
        #endregion
    }
}


