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

namespace ML.Engine.BuildingSystem
{


    public class Test_BuildingManager : MonoBehaviour
    {
        public static Test_BuildingManager Instance;
        public BuildingManager BM;

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

        [System.Serializable]
        private struct KeyTips
        {
            public KeyTip[] keytips;
        }


        public Dictionary<string, KeyTip> KeyTipDict = new Dictionary<string, KeyTip>();

        #endregion

        #region Category|Type
        [System.Serializable]
        public struct BCategory
        {
            public string category;
            public TextContent.TextContent name;

            public string GetNameText()
            {
                return name.GetText();
            }
        }
        [System.Serializable]
        public struct BType
        {
            public string type;
            public TextContent.TextContent name;
            public string GetNameText()
            {
                return name.GetText();
            }
        }

        [System.Serializable]
        private struct BCategorys
        {
            public BCategory[] category;
        }
        [System.Serializable]
        private struct BTypes
        {
            public BType[] type;
        }

        public Dictionary<string, BCategory> CategoryDict = new Dictionary<string, BCategory>();
        public Dictionary<string, BType> TypeDict = new Dictionary<string, BType>();

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
            KeyTips keyTips = JsonUtility.FromJson<KeyTips>((request.asset as TextAsset).text);
            foreach(var keytip in keyTips.keytips)
            {
                this.KeyTipDict.Add(keytip.keyname, keytip);
            }

            // Category
            request = ab.LoadAssetAsync<TextAsset>("Category");
            yield return request;
            BCategorys categorys = JsonUtility.FromJson<BCategorys>((request.asset as TextAsset).text);
            foreach (var category in categorys.category)
            {
                this.CategoryDict.Add(category.category, category);
            }

            // Type
            request = ab.LoadAssetAsync<TextAsset>("Type");
            yield return request;
            BTypes btypes = JsonUtility.FromJson<BTypes>((request.asset as TextAsset).text);
            foreach (var type in btypes.type)
            {
                this.TypeDict.Add(type.type, type);
            }

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
            ab = crequest.assetBundle;
            
            var request = ab.LoadAllAssetsAsync<GameObject>();
            yield return request;

            foreach (var obj in request.allAssets)
            {
                var bpart = (obj as GameObject).GetComponent<IBuildingPart>();
                if (bpart != null)
                {
                    this.LoadedBPart.Add(bpart.Classification, bpart);
                    //BM.RegisterBPartPrefab(bpart);
                }
            }

            // to-do : 暂时材质有用，不能UnLoad
            //abmgr.UnLoadLocalABAsync(BPartABPath, false, null);

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
                ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Disable();
                ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Disable();
                area.gameObject.SetActive(true);
                this.PushPanel<UI.BSInteractModePanel>();
            };
            BM.Placer.OnBuildingModeExit += () =>
            {
                ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Enable();
                ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Enable();
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

            BM.Placer.OnBuildSelectionEnter += (BuildingCategory[] c, int ic, BuildingType[] t, int it) =>
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
                StartCoroutine((this.GetPeekPanel() as UI.BSPlaceModePanel).Init(styles, heights, Array.IndexOf(styles, bpart.Classification.Style), Array.IndexOf(heights, bpart.Classification.Height))) ;
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
