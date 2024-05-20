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
        private Dictionary<string, MineralMapData> mineralMapDatas;

        [LabelText("���ͼԤ����"), ReadOnly, ShowInInspector]
        private GameObject BigMapPrefab;

        /// <summary>
        /// ��ǰѡ�еĴ��ͼ��
        /// </summary>
        private int curMapLayerIndex;
        /// <summary>
        /// ����̨ui refresh
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
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;

        /// <summary>
        /// ��������
        /// </summary>
        public static MineSystemManager Instance { get { return instance; } }

        private static MineSystemManager instance;
        public void Init()
        {
            //��ʼ�����ͼ���ű���
            this.GridScale = 1;

            //��ʼ�������б�
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

            //��ʼ�����ͼ��ͼ���������
            isUnlockIslandMap = new bool[MineSystemData.MAPDEPTH];



            //��ʼ����������
            mainIslandData = new MainIslandData();


            //��ʼ��С��ͼ����

            //Ĭ��ѡ��
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
        /// ���õ����� ����true����ɹ� ����false����ǰ�������ƶ� isCancel��ʾ�Ƿ���Ŀǰ·��
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


