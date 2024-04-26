using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using static ML.Engine.BuildingSystem.UI.BSPlaceModePanel;
using ML.Engine.Manager;
using ML.Engine.InventorySystem;

namespace ML.Engine.BuildingSystem.UI
{
    /// <summary>
    /// 放置的三级
    /// </summary>
    public class BSPlaceModePanel : Engine.UI.UIBasePanel<BSPlaceModePanelStruct>, Timer.ITickComponent
    {
        #region Property|Field

        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;
        private MonoBuildingManager monoBM;

        #region UIGO引用
        private Dictionary<BuildingCategory3, RectTransform> styleInstance = new Dictionary<BuildingCategory3, RectTransform>();
        private RectTransform styleParent;
        private RectTransform templateStyle;

        private Transform KT_AlterHeight;
        private Transform KT_AlterSocket;
        #endregion

        #endregion

        #region Unity

        protected override void Awake()
        {
            InitStyleTexture2D();
            monoBM = ML.Engine.Manager.GameManager.Instance.GetLocalManager<MonoBuildingManager>();
            this.styleParent = this.transform.Find("KT_AlterHeight").Find("KT_AlterStyle").Find("Content") as RectTransform;
            this.templateStyle = this.styleParent.Find("StyleTemplate") as RectTransform;
            this.templateStyle.gameObject.SetActive(false);

            this.KT_AlterHeight = this.transform.Find("KT_AlterHeight");
            this.KT_AlterSocket = this.transform.Find("KeyTip").Find("KT_AlterSocket");
        }

        #endregion

        #region 载入资产
        public const string TStyleSpriteAtlasPath = "SA_UI_Category3";
        private SpriteAtlas styleAtlas = null;
        private AsyncOperationHandle SAHandle;
        /// <summary>
        /// 资产是否完成载入
        /// </summary>
        public bool IsInit = false;
        private void InitStyleTexture2D()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(TStyleSpriteAtlasPath).Completed += (handle) =>
            {
                SAHandle = handle;
                styleAtlas = handle.Result;


                IsInit = true;
                this.enabled = false;

                this.Refresh();
            };
        }

        private void UnloadAsset()
        {
            Manager.GameManager.Instance.ABResourceManager.Release(SAHandle);
        }

        public Sprite GetStyleSprite(BuildingCategory3 style)
        {
            Sprite sprite = styleAtlas.GetSprite(style.ToString());;
            if (sprite == null)
            {
                sprite = styleAtlas.GetSprite("None");
            }
            return sprite;
        }

        #endregion

        #region Refresh
        public override void Refresh()
        {
            if (!IsInit)
            {
                return;
            }

            var styles = BM.GetAllStyleByBPartHeight(this.Placer.SelectedPartInstance);
            var heights = BM.GetAllHeightByBPartStyle(this.Placer.SelectedPartInstance);
            int sIndex = Array.IndexOf(styles, this.Placer.SelectedPartInstance.Classification.Category3);
            int hIndex = Array.IndexOf(heights, this.Placer.SelectedPartInstance.Classification.Category4);
            this.ClearInstance();

            var s = styles[sIndex];
            var h = heights[hIndex];

            Array.Sort(styles);
            Array.Sort(heights);

            foreach (var style in styles)
            {
                var go = Instantiate<GameObject>(this.templateStyle.gameObject, this.styleParent, false);
                go.GetComponentInChildren<Image>().sprite = GetStyleSprite(style);
                //go.GetComponentInChildren<TextMeshProUGUI>().text = style.ToString();
                go.SetActive(true);
                this.styleInstance.Add(style, go.transform as RectTransform);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.styleParent.parent.GetComponent<RectTransform>());
            // 更换 Style
            foreach (var instance in this.styleInstance)
            {
                var img = instance.Value.GetComponentInChildren<Image>();
                if (instance.Key != s)
                {
                    Disactive(img);
                }
                else
                {
                    Active(img);
                }
            }

            // 更换 Height

            // 下侧显示
            bool active = (BuildingManager.Instance.GetBPartPrefabCountOnHeight(this.Placer.SelectedPartInstance) > 1 || BuildingManager.Instance.GetBPartPrefabCountOnStyle(this.Placer.SelectedPartInstance) > 1);
            this.transform.Find("KT_AlterHeight").gameObject.SetActive(active);

            //家具特殊处理
            this.KT_AlterHeight.gameObject.SetActive(this.Placer.SelectedPartInstance.Classification.Category1 != BuildingCategory1.Furniture);
            this.KT_AlterSocket.gameObject.SetActive(this.Placer.SelectedPartInstance.Classification.Category1 != BuildingCategory1.Furniture);
        }

        /// <summary>
        /// to-yl
        /// </summary>
        /// <param name="img"></param>
        private void Active(Image img)
        {
            img.color = Color.white;
        }

        /// <summary>
        /// to-yl
        /// </summary>
        /// <param name="img"></param>
        private void Disactive(Image img)
        {
            img.color = new Color(144f / 255, 144f / 255, 144f / 255, 144f / 255);
        }

        private void ClearInstance()
        {
            foreach (var instance in this.styleInstance.Values)
            {
                Manager.GameManager.DestroyObj(instance.GetComponentInChildren<Image>().sprite);
                Manager.GameManager.DestroyObj(instance.gameObject);
            }
            this.styleInstance.Clear();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.RegisterInput();
            this.Placer.InteractBPartList.Clear();

            if (BM.Placer.SelectedPartInstance != null)
            {
                BM.Placer.SelectedPartInstance.CheckCanInPlaceMode += CheckCostResources;
            }
            BM.Placer.OnPlaceModeSuccess += OnPlaceModeSuccess;
            BM.Placer.OnPlaceModeChangeBPart += Placer_OnPlaceModeChangeBPart;
        }

        public override void OnPause()
        {
            base.OnPause();
            this.UnregisterInput();

            if (BM.Placer.SelectedPartInstance != null)
            {
                BM.Placer.SelectedPartInstance.CheckCanInPlaceMode -= CheckCostResources;
            }
            BM.Placer.OnPlaceModeSuccess -= OnPlaceModeSuccess;
            BM.Placer.OnPlaceModeChangeBPart -= Placer_OnPlaceModeChangeBPart;
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            if (BM.Placer.SelectedPartInstance != null)
            {
                BM.Placer.SelectedPartInstance.CheckCanInPlaceMode += CheckCostResources;
            }
            BM.Placer.OnPlaceModeSuccess += OnPlaceModeSuccess;
            BM.Placer.OnPlaceModeChangeBPart += Placer_OnPlaceModeChangeBPart;
        }

        public override void OnExit()
        {
            base.OnExit();
            this.ClearInstance();
            this.UnloadAsset();

            if (BM.Placer.SelectedPartInstance != null)
            {
                BM.Placer.SelectedPartInstance.CheckCanInPlaceMode -= CheckCostResources;
            }
            BM.Placer.OnPlaceModeSuccess -= OnPlaceModeSuccess;
            BM.Placer.OnPlaceModeChangeBPart -= Placer_OnPlaceModeChangeBPart;
        }

        #endregion

        #region TickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }


        public virtual void FixedTick(float deltatime)
        {
            // 实时更新落点的位置和旋转以及是否可放置
            this.Placer.TransformSelectedPartInstance();
            if(this.IsRotate)
            {
                this.Placer.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.Placer.DisableGridRotRate * Time.deltaTime * rotOffset, Vector3.up);
            }
        }

        #endregion

        #region KeyFunction
        protected override void UnregisterInput()
        {
            this.Placer.DisablePlayerInput();

            this.Placer.BInput.BuildPlaceMode.Disable();

            Manager.GameManager.Instance.TickManager.UnregisterFixedTick(this);
            this.Placer.BInput.BuildPlaceMode.KeyCom.performed -= Placer_EnterKeyCom;
            this.Placer.backInputAction.performed -= Placer_CancelPlace;

            this.Placer.BInput.BuildPlaceMode.Rotate.started -= Placer_RotateBPart;
            this.Placer.BInput.BuildPlaceMode.Rotate.canceled -= Placer_RotateBPart;
            this.Placer.BInput.BuildPlaceMode.ChangeActiveSocket.performed -= Placer_ChangeActiveSocket;
            this.Placer.BInput.BuildPlaceMode.ChangeStyle.performed -= Placer_ChangeBPartStyle;
            this.Placer.BInput.BuildPlaceMode.ChangeHeight.performed -= Placer_ChangeBPartHeight;
            this.Placer.BInput.BuildPlaceMode.ChangeOutLook.performed -= Placer_EnterAppearance;
            this.Placer.comfirmInputAction.performed -= Placer_ComfirmPlaceBPart;
        }

        protected override void RegisterInput()
        {
            this.Placer.EnablePlayerInput();

            this.Placer.BInput.BuildPlaceMode.Enable();
            this.Placer.BInput.BuildPlaceMode.ChangeOutLook.Enable();
            this.Placer.BInput.BuildPlaceMode.ChangeStyle.Enable();
            this.Placer.BInput.BuildPlaceMode.ChangeHeight.Enable();
            this.Placer.BInput.BuildPlaceMode.KeyCom.Enable();

            Manager.GameManager.Instance.TickManager.RegisterFixedTick(0, this);
            this.Placer.BInput.BuildPlaceMode.KeyCom.performed += Placer_EnterKeyCom;
            this.Placer.backInputAction.performed += Placer_CancelPlace;

            this.Placer.BInput.BuildPlaceMode.Rotate.started += Placer_RotateBPart;
            this.Placer.BInput.BuildPlaceMode.Rotate.canceled += Placer_RotateBPart;
            this.Placer.BInput.BuildPlaceMode.ChangeActiveSocket.performed += Placer_ChangeActiveSocket;
            this.Placer.BInput.BuildPlaceMode.ChangeStyle.performed += Placer_ChangeBPartStyle;
            this.Placer.BInput.BuildPlaceMode.ChangeHeight.performed += Placer_ChangeBPartHeight;
            this.Placer.BInput.BuildPlaceMode.ChangeOutLook.performed += Placer_EnterAppearance;
            this.Placer.comfirmInputAction.performed += Placer_ComfirmPlaceBPart;
        }

        private void Placer_ComfirmPlaceBPart(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.ComfirmPlaceBPart();
        }

        private void Placer_EnterAppearance(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            monoBM.PushPanel<BSAppearancePanel>();
        }

        private void Placer_ChangeBPartStyle(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.AlternateBPartOnHeight(obj.ReadValue<float>() < 0);
           this.Refresh();
        }

        private void Placer_ChangeBPartHeight(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.AlternateBPartOnStyle(obj.ReadValue<float>() < 0);
            this.Refresh();
        }

        private void Placer_ChangeActiveSocket(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.SelectedPartInstance.AlternativeActiveSocket();
        }

        private bool IsRotate = false;
        private float rotOffset = 0;
        private void Placer_RotateBPart(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            int offset = obj.ReadValue<float>() > 0 ? 1 : -1;
            if (this.Placer.IsEnableGridSupport)
            {
                if (obj.started)
                {
                    this.Placer.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.Placer.EnableGridRotRate * offset, Vector3.up);
                }
            }
            else
            {
                IsRotate = !IsRotate;
                rotOffset = offset;
                //this.Placer.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.Placer.DisableGridRotRate * Time.deltaTime * offset, this.Placer.SelectedPartInstance.transform.up);
            }
        }

        private void Placer_CancelPlace(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.ResetBPart();

            this.Placer.Mode = BuildingMode.Interact;

            monoBM.PopPanel();
        }

        private void Placer_EnterKeyCom(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            monoBM.PushPanel<BSPlaceMode_KeyComPanel>();
        }
        #endregion

        #region Event
        private bool CheckCostResources(IBuildingPart bpart)
        {
            List<IInventory> inventorys = (GameManager.Instance.CharacterManager.GetLocalController() as ProjectOC.Player.OCPlayerController).GetInventorys();
            if (CompositeManager.Instance.CanComposite(inventorys, BuildingManager.Instance.GetID(bpart.Classification.ToString())))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnPlaceModeSuccess(IBuildingPart bpart)
        {
            List<IInventory> inventorys = (GameManager.Instance.CharacterManager.GetLocalController() as ProjectOC.Player.OCPlayerController).GetInventorys(true, -1);
            CompositeManager.Instance.OnlyCostResource(inventorys, BuildingManager.Instance.GetID(bpart.Classification.ToString()));
            BM.Placer.SelectedPartInstance.CheckCanInPlaceMode -= CheckCostResources;
        }

        private void Placer_OnPlaceModeChangeBPart(IBuildingPart obj)
        {
            obj.CheckCanInPlaceMode += CheckCostResources;
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct BSPlaceModePanelStruct
        {
            public KeyTip[] KeyTips;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/BuildingSystem/UI";
            this.abname = "BSPlaceModePanel";
            this.description = "BSPlaceModePanel数据加载完成";
        }
        #endregion
    }
}

