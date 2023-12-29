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
        private int IsInit = 5;
        public bool IsLoadOvered => IsInit == 0;

        #region Config
        [LabelText("����"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.Language language = Config.Language.Chinese;
        [LabelText("ƽ̨"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.Platform platform = Config.Platform.Windows;
        [LabelText("�����豸"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.InputDevice inputDevice = Config.InputDevice.Keyboard;
        #endregion


        #region TextContent
        #region KeyTip
        public Dictionary<string, KeyTip> KeyTipDict = new Dictionary<string, KeyTip>();
        #endregion

        #region Category|Type
        public Dictionary<string, TextContent.TextTip> CategoryDict = new Dictionary<string, TextContent.TextTip>();
        public Dictionary<string, TextContent.TextTip> TypeDict = new Dictionary<string, TextContent.TextTip>();

        #endregion
        public static bool IsLoading = false;

        public void InitUITextContents()
        {
            if (!IsLoading)
            {
                IsLoading = true;
                var KeyTip_ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<KeyTip[]>("JSON/TextContent/BuildingSystem/UI", "KeyTip", (datas) =>
                {
                    foreach (var keytip in datas)
                    {
                        this.KeyTipDict.Add(keytip.keyname, keytip);
                    }
                    IsInit--;
                }, null, "����ϵͳ������ʾ");
                KeyTip_ABJAProcessor.StartLoadJsonAssetData();

                var Category_ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TextTip[]>("JSON/TextContent/BuildingSystem/UI", "Category", (datas) =>
                {
                    foreach (var category in datas)
                    {
                        this.CategoryDict.Add(category.name, category);
                    }
                    IsInit--;
                }, null, "����ϵͳCategory1");
                Category_ABJAProcessor.StartLoadJsonAssetData();

                var Type_ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TextTip[]>("JSON/TextContent/BuildingSystem/UI", "Type", (datas) =>
                {
                    foreach (var type in datas)
                    {
                        this.TypeDict.Add(type.name, type);
                    }
                    IsInit--;
                }, null, "����ϵͳCategory2");
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

            InitUITextContents();
        }

        public Dictionary<BuildingPartClassification, IBuildingPart> LoadedBPart = new Dictionary<BuildingPartClassification, IBuildingPart>();
#if UNITY_EDITOR
        [LabelText("���ؽ�����ʱ�Ƿ���Ϊ����"), SerializeField]
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
                    // to-do : �������ã���������Ƽ�������ע�͵�
#if UNITY_EDITOR
                    if (IsAddBPartOnRegister) 
                    {
                        BM.RegisterBPartPrefab(bpart);

                    }
#endif
                }
            }

            // to-do : ��ʱ�������ã�����UnLoad
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

            // to-do : ��ʱ�������ã�����UnLoad
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

        #endregion
    }

}
