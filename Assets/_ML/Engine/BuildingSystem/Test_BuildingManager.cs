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


    public class Test_BuildingManager : MonoBehaviour
    {
        public static Test_BuildingManager Instance;
        public BuildingManager BM;
        private int IsInit = 3;
        public bool IsLoadOvered => IsInit == 0;

        #region Config
        [LabelText("语言"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.Language language = Config.Language.Chinese;
        [LabelText("平台"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.Platform platform = Config.Platform.Windows;
        [LabelText("输入设备"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.InputDevice inputDevice = Config.InputDevice.Keyboard;
        #endregion


        #region TextContent

        public const string TextContentABPath = "JSON/TextContent/BuildingSystem/UI";

        
        #region KeyTip
        public Dictionary<string, KeyTip> KeyTipDict = new Dictionary<string, KeyTip>();

        #endregion

        #region Category|Type
        public Dictionary<string, TextContent.TextTip> CategoryDict = new Dictionary<string, TextContent.TextTip>();
        public Dictionary<string, TextContent.TextTip> TypeDict = new Dictionary<string, TextContent.TextTip>();

        #endregion

        private IEnumerator InitUITextContents()
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
            var crequest = abmgr.LoadLocalABAsync(TextContentABPath, null, out ab);
            yield return crequest;
            if(crequest != null)
            {
                ab = crequest.assetBundle;
            }

            // KeyTip
            var request = ab.LoadAssetAsync<TextAsset>("KeyTip");
            yield return request;
            KeyTip[] keyTips = JsonConvert.DeserializeObject<KeyTip[]>((request.asset as TextAsset).text);
            foreach(var keytip in keyTips)
            {
                this.KeyTipDict.Add(keytip.keyname, keytip);
            }

            // Category
            request = ab.LoadAssetAsync<TextAsset>("Category");
            yield return request;
            TextContent.TextTip[] categorys = JsonConvert.DeserializeObject<TextContent.TextTip[]>((request.asset as TextAsset).text);
            foreach (var category in categorys)
            {
                this.CategoryDict.Add(category.name, category);
            }

            // Type
            request = ab.LoadAssetAsync<TextAsset>("Type");
            yield return request;
            TextContent.TextTip[] btypes = JsonConvert.DeserializeObject<TextContent.TextTip[]>((request.asset as TextAsset).text);
            foreach (var type in btypes)
            {
                this.TypeDict.Add(type.name, type);
            }
            IsInit--;
#if UNITY_EDITOR
            Debug.Log("InitUITextContents cost time: " + (Time.realtimeSinceStartup - startT));
#endif
        }


        #endregion

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

        private void PushPanel<T>() where T : UIBasePanel
        {
            var panel = this.GetPanel<T>();
            Manager.GameManager.Instance.UIManager.PushPanel(panel);
            panel.transform.SetParent(this.Canvas, false);
        }

        private void PopPanel()
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

            StartCoroutine(InitUITextContents());
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
                area.gameObject.SetActive(IsEnableArea);

                this.PushPanel<UI.BSInteractModePanel>();
            };
            BM.Placer.OnBuildingModeExit += () =>
            {

                this.PopPanel();
            };

            BM.Placer.OnKeyComComplete += () =>
            {
                if(BM.Mode == BuildingMode.Interact)
                {
                    this.PushPanel<UI.BSInteractMode_KeyComPanel>();
                }
                else if(BM.Mode == BuildingMode.Place)
                {
                    this.PushPanel<UI.BSPlaceMode_KeyComPanel>();
                }
                else if(BM.Mode == BuildingMode.Edit)
                {
                    this.PushPanel<UI.BSEditMode_KeyComPanel>();
                }
            };

            BM.Placer.OnKeyComExit += () =>
            {
                this.PopPanel();
            };

            BM.Placer.OnEnterAppearance += (bpart, texs, mats, index) =>
            {
                this.PushPanel<UI.BSAppearancePanel>();
                StartCoroutine((this.GetPeekPanel() as UI.BSAppearancePanel).Init(texs, mats, index));
                // to-do
                ProjectOC.Input.InputManager.PlayerInput.Disable();
            };
            BM.Placer.OnExitAppearance += (bpart) =>
            {
                this.PopPanel();
                // to-do
                ProjectOC.Input.InputManager.PlayerInput.Enable();
                ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Disable();
                ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Disable();
            };

            BM.Placer.OnEditModeEnter += (bpart) =>
            {
                this.PushPanel<UI.BSEditModePanel>();
            };
            BM.Placer.OnEditModeExit += (bpart) =>
            {
                this.PopPanel();
            };

            BM.Placer.OnBuildSelectionEnter += (BuildingCategory1[] c, int ic, BuildingCategory2[] t, int it) =>
            {
                this.PushPanel<UI.BSSelectBPartPanel>();
                StartCoroutine((this.GetPeekPanel() as UI.BSSelectBPartPanel).Init(c, t, ic, it));
            };

            BM.Placer.OnBuildSelectionComfirm += (bpart) =>
            {
                this.PopPanel();
            };
            BM.Placer.OnBuildSelectionCancel += () =>
            {
                this.PopPanel();
            };

            BM.Placer.OnPlaceModeEnter += (bpart) =>
            {
                this.PushPanel<UI.BSPlaceModePanel>();
                var styles = BM.GetAllStyleByBPartHeight(bpart);
                var heights = BM.GetAllHeightByBPartStyle(bpart);
                StartCoroutine((this.GetPeekPanel() as UI.BSPlaceModePanel).Init(styles, heights, Array.IndexOf(styles, bpart.Classification.Category3), Array.IndexOf(heights, bpart.Classification.Category4))) ;
            };
            BM.Placer.OnPlaceModeExit += () =>
            {
                this.PopPanel();
            };
        }



        private void OnDestroy()
        {
            if(Instance == this)
            {
                Instance = null;
            }
        }
    }

}
