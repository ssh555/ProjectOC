using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.UI;
using Sirenix.OdinInspector;

namespace ML.Engine.BuildingSystem
{
    public class Test_BuildingManager : MonoBehaviour
    {
        public const string UIPanelABPath = "UI/BuildingSystem/Prefabs";

        public const string BPartABPath = "Prefabs/BuildingSystem/BPart";

        public BuildingManager BM;

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

        void Start()
        {
            ML.Engine.Manager.GameManager.Instance.RegisterLocalManager(BM);

            this.Canvas = GameObject.Find("Canvas").transform as RectTransform;

            StartCoroutine(RegisterBPartPrefab());
            StartCoroutine(RigisterUIPanelPrefab());

            StartCoroutine(AddTestEvent());
        }

        private IEnumerator RegisterBPartPrefab()
        {
#if UNITY_EDITOR
            float startT = Time.time;
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
                    BM.RegisterBPartPrefab(bpart);
                }
            }

            // to-do : 暂时材质有用，不能UnLoad
            //abmgr.UnLoadLocalABAsync(BPartABPath, false, null);

#if UNITY_EDITOR
            Debug.Log("RegisterBPartPrefab cost time: " + (Time.time - startT));
#endif

        }

        private IEnumerator RigisterUIPanelPrefab()
        {
#if UNITY_EDITOR
            float startT = Time.time;
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

            var botPanel = this.GetPanel<UI.Test_BSBotPanel>();
            botPanel.transform.SetParent(this.Canvas, false);
            Manager.GameManager.Instance.UIManager.ChangeBotUIPanel(botPanel);

#if UNITY_EDITOR
            Debug.Log("RigisterUIPanelPrefab cost time: " + (Time.time - startT));
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

            //BM.Placer.OnKeyComStart += () =>
            //{
            //    Debug.Log("Start KeyCom");
            //};

            //BM.Placer.OnKeyComInProgress += (float cur, float total) =>
            //{
            //    Debug.Log("KeyCom Progress: " + (cur / total));
            //};

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

            //BM.Placer.OnKeyComCancel += (float cur, float total) =>
            //{
            //    Debug.LogWarning("KeyCom Cancel: " + (cur / total));
            //};

            BM.Placer.OnKeyComExit += () =>
            {
                this.PopPanel();
            };

            BM.Placer.OnEnterAppearance += (bpart) =>
            {
                this.PushPanel<UI.BSAppearancePanel>();
            };
            BM.Placer.OnExitAppearance += (bpart) =>
            {
                this.PopPanel();
            };


            //BM.Placer.OnDestroySelectedBPart += (bpart) =>
            //{
            //};

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
            };
            //BM.Placer.OnBuildSelectionExit += () =>
            //{
            //    this.PopPanel();
            //};
            BM.Placer.OnBuildSelectionComfirm += (bpart) =>
            {
                this.PopPanel();
            };
            BM.Placer.OnBuildSelectionCancel += () =>
            {
                this.PopPanel();
            };
            //BM.Placer.OnBuildSelectionCategoryChanged += (BuildingCategory[] categorys, int index) =>
            //{
            //    Debug.Log("Current Selected Category: " + categorys[index]);
            //};
            //BM.Placer.OnBuildSelectionTypeChanged += (BuildingCategory category, BuildingType[] types, int index) =>
            //{
            //    Debug.Log("Current Selected Category: " + category + "-Type: " + types[index]);
            //};

            BM.Placer.OnPlaceModeEnter += (bpart) =>
            {
                this.PushPanel<UI.BSPlaceModePanel>();
            };
            BM.Placer.OnPlaceModeExit += () =>
            {
                this.PopPanel();
            };
            //BM.Placer.OnPlaceModeChangeStyle += (bpart, isForward) =>
            //{
            //    Debug.Log("PlaceMode Style Changed Selected BPart: " + bpart.Classification.ToString());
            //};
            //BM.Placer.OnPlaceModeChangeHeight += (bpart, isForward) =>
            //{
            //    Debug.Log("PlaceMode Height Changed Selected BPart: " + bpart.Classification.ToString());
            //};
            //BM.Placer.OnPlaceModeSuccess += (bpart) =>
            //{
            //};
        }


        private void Update()
        {
            if (ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.WasPressedThisFrame() && BM.Mode == BuildingMode.None)
            {
                BM.Mode = BuildingMode.Interact;
            }

        }

    }

}
