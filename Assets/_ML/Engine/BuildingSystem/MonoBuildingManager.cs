using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using ML.Engine.TextContent;
using Unity.VisualScripting;
using Newtonsoft.Json;

namespace ML.Engine.BuildingSystem
{


    public class MonoBuildingManager : MonoBehaviour
    {
        #region Property|Field
        public static MonoBuildingManager Instance;
        public BuildingManager BM;
        private int IsInit = 5;
        /// <summary>
        /// 是否初始化完成所有数据的载入
        /// </summary>
        public bool IsLoadOvered => IsInit == 0;

        #endregion

        #region TextContent
        #region KeyTip
        public Dictionary<string, KeyTip> KeyTipDict = new Dictionary<string, KeyTip>();
        #endregion

        #region Category|Type
        public Dictionary<string, TextContent.TextTip> Category1Dict = new Dictionary<string, TextContent.TextTip>();
        public Dictionary<string, TextContent.TextTip> Category2Dict = new Dictionary<string, TextContent.TextTip>();

        #endregion
       
        public static bool IsLoading = false;

        public void InitUITextContents()
        {
            if (!IsLoading)
            {
                IsLoading = true;
                var KeyTip_ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<KeyTip[]>("Json/TextContent/BuildingSystem/UI", "KeyTip", (datas) =>
                {
                    foreach (var keytip in datas)
                    {
                        this.KeyTipDict.Add(keytip.keyname, keytip);
                    }
                    IsInit--;
                }, null, "建造系统按键提示");
                KeyTip_ABJAProcessor.StartLoadJsonAssetData();

                var Category_ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TextTip[]>("Json/TextContent/BuildingSystem/UI", "Category", (datas) =>
                {
                    foreach (var category in datas)
                    {
                        this.Category1Dict.Add(category.name, category);
                    }
                    IsInit--;
                }, null, "建造系统Category1");
                Category_ABJAProcessor.StartLoadJsonAssetData();

                var Type_ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TextTip[]>("Json/TextContent/BuildingSystem/UI", "Type", (datas) =>
                {
                    foreach (var type in datas)
                    {
                        this.Category2Dict.Add(type.name, type);
                    }
                    IsInit--;
                }, null, "建造系统Category2");
                Type_ABJAProcessor.StartLoadJsonAssetData();
            }
        }

        #endregion

        #region Internal
        public const string UIPanelABPath = "UI/BuildingSystem/Prefabs";

        public const string BPartABPath = "Prefabs/BuildingSystem/BPart";


        private Dictionary<Type, UIBasePanel> uiBasePanelDict = new Dictionary<Type, UIBasePanel>();

        [ShowInInspector]
        private RectTransform Canvas;

        /// <summary>
        /// to-do : to-delete
        /// </summary>
        public BuildingArea.BuildingArea area;
        public bool IsEnableArea = true;

        public T GetPanel<T>() where T : UIBasePanel
        {
            if(this.uiBasePanelDict.ContainsKey(typeof(T)))
            {
                T panel = Instantiate<GameObject>(this.uiBasePanelDict[typeof(T)].gameObject).GetComponent<T>();
                return panel;
            }
            return null;
        }

        public void PushPanel<T>() where T : UIBasePanel
        {
            var panel = this.GetPanel<T>();
            Manager.GameManager.Instance.UIManager.PushPanel(panel);
            panel.transform.SetParent(this.Canvas, false);
        }

        public void PopPanel()
        {
            Manager.GameManager.Instance.UIManager.PopPanel();
        }

        private UIBasePanel GetPeekPanel()
        {
            return Manager.GameManager.Instance.UIManager.GetTopUIPanel();
        }

        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(this.gameObject);
            }
            Instance = this;
        }

        void Start()
        {
            ML.Engine.Manager.GameManager.Instance.RegisterLocalManager(BM);

            this.Canvas = GameObject.Find("Canvas").transform as RectTransform;

            StartCoroutine(RegisterBPartPrefab());
            StartCoroutine(RigisterUIPanelPrefab());

            StartCoroutine(AddTestEvent());

            InitUITextContents();
        }

        public Dictionary<BuildingPartClassification, IBuildingPart> LoadedBPart = new Dictionary<BuildingPartClassification, IBuildingPart>();
#if UNITY_EDITOR
        [LabelText("加载建筑物时是否标记为可用"), SerializeField]
        private bool IsAddBPartOnRegister = true;
#endif
        private IEnumerator RegisterBPartPrefab()
        {
#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif
            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(BPartABPath, null, out ab);
            yield return crequest;
            if(crequest != null)
                ab = crequest.assetBundle;
            
            var request = ab.LoadAllAssetsAsync<GameObject>();
            yield return request;

            foreach (var obj in request.allAssets)
            {
                var bpart = (obj as GameObject).GetComponent<IBuildingPart>();
                if (bpart != null)
                {
                    this.LoadedBPart.Add(bpart.Classification, bpart);
                    // to-do : 仅测试用，后续接入科技树后需注释掉
#if UNITY_EDITOR
                    if (IsAddBPartOnRegister) 
                    {
                        BM.RegisterBPartPrefab(bpart);

                    }
#endif
                }
            }

            // to-do : 暂时材质有用，不能UnLoad
            //abmgr.UnLoadLocalABAsync(BPartABPath, false, null);
            IsInit--;
#if UNITY_EDITOR
            Debug.Log("RegisterBPartPrefab cost time: " + (Time.realtimeSinceStartup - startT));
#endif

        }

        private IEnumerator RigisterUIPanelPrefab()
        {
#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif

            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(UIPanelABPath, null, out ab);
            yield return crequest;
            ab = crequest.assetBundle;

            var request = ab.LoadAllAssetsAsync<GameObject>();
            yield return request;

            foreach (var obj in request.allAssets)
            {
                var panel = (obj as GameObject).GetComponent<UIBasePanel>();
                if(panel)
                {
                    this.uiBasePanelDict.Add(panel.GetType(), panel);
                }
            }

            // to-do : 暂时材质有用，不能UnLoad
            //abmgr.UnLoadLocalABAsync(BPartABPath, false, null);

            //var botPanel = this.GetPanel<UI.Test_BSBotPanel>();
            //botPanel.transform.SetParent(this.Canvas, false);
            //Manager.GameManager.Instance.UIManager.ChangeBotUIPanel(botPanel);
            IsInit--;
#if UNITY_EDITOR
            Debug.Log("RigisterUIPanelPrefab cost time: " + (Time.realtimeSinceStartup - startT));
#endif
        }

        private IEnumerator AddTestEvent()
        {
            while (BM.Placer == null)
            {
                yield return null;
            }
            
            BM.Placer.OnBuildingModeEnter += () =>
            {
                // to-delete
                if(area != null)
                {
                    area.gameObject.SetActive(IsEnableArea);
                }
            };

        }



        private void OnDestroy()
        {
            if(Instance == this)
            {
                Instance = null;
            }
        }

        #endregion
    }

}
