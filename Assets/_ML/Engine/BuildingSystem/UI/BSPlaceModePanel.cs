using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using static ML.Engine.BuildingSystem.UI.BSPlaceModePanel;
using Sirenix.OdinInspector;
using ML.Engine.Utility;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ProjectOC.Player;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static Unity.Burst.Intrinsics.X86.Avx;
using System.Linq;

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

        private Transform Slots;
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
            this.Slots = this.transform.Find("Slots");
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
        private bool isChangeStyle = true;
        private bool isChangeHeight = false;
        public override void Refresh()
        {
            if (!IsInit)
            {
                return;
            }
            ClearInstance();
            for (int i = 0;i<stylecnt;i++)
            {
                if (swichArray[i][index_j] != null)
                {
                    var style = swichArray[i][index_j].Classification.Category3;
                    var go = Instantiate<GameObject>(this.templateStyle.gameObject, this.styleParent, false);
                    go.GetComponentInChildren<Image>().sprite = GetStyleSprite(style);
                    go.SetActive(true);
                    this.styleInstance.Add(style, go.transform as RectTransform);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.styleParent.parent.GetComponent<RectTransform>());
            // 更换 Style
            foreach (var instance in this.styleInstance)
            {
                var img = instance.Value.GetComponentInChildren<Image>();
                if (instance.Key != swichArray[index_i][index_j].Classification.Category3)
                {
                    Disactive(img);
                }
                else
                {
                    Active(img);
                }
            }

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
            InitSwitchData();
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

            Manager.GameManager.Instance.TickManager.RegisterFixedTick(-1, this);
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
            var offset = obj.ReadValue<float>() < 0 ? -1 : 1;
            this.Placer.AlternateBPart(GetNextOnSwichingStyle(offset));
            this.Refresh();
        }
        private void Placer_ChangeBPartHeight(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var offset = obj.ReadValue<float>() < 0 ? -1 : 1;
            this.Placer.AlternateBPart(GetNextOnSwichingHeight(offset));
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
                    if (this.Placer.SelectedPartInstance.AttachedSocket == null)
                    {
                        this.Placer.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.Placer.EnableGridRotRate * offset, Vector3.up);
                    }
                    else
                    {
                        this.Placer.SelectedPartInstance.ActiveSocket.AsMatchRotOffset *= offset > 0 ? this.Placer.SelectedPartInstance.AttachedSocket.AsTargetRotDelta : Quaternion.Inverse(this.Placer.SelectedPartInstance.AttachedSocket.AsTargetRotDelta);
                    }
                }
            }
            else
            {
                if (this.Placer.SelectedPartInstance.AttachedSocket != null)
                {
                    if (obj.started)
                    {
                        this.Placer.SelectedPartInstance.ActiveSocket.AsMatchRotOffset *= offset > 0 ? this.Placer.SelectedPartInstance.AttachedSocket.AsTargetRotDelta : Quaternion.Inverse(this.Placer.SelectedPartInstance.AttachedSocket.AsTargetRotDelta);
                    }
                }
                else
                {
                    IsRotate = !IsRotate;
                    rotOffset = offset;
                }
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
            return ProjectOC.ManagerNS.LocalGameManager.Instance.Player.InventoryHaveItems(BuildingManager.Instance.GetRawAll(bpart.Classification.ToString()));
        }

        private void OnPlaceModeSuccess(IBuildingPart bpart)
        {
            ProjectOC.ManagerNS.LocalGameManager.Instance.Player.InventoryCostItems(BuildingManager.Instance.GetRawAll(bpart.Classification.ToString()), needJudgeNum:true, priority:-1);
            BM.Placer.SelectedPartInstance.CheckCanInPlaceMode -= CheckCostResources;
        }

        private void Placer_OnPlaceModeChangeBPart(IBuildingPart obj)
        {
            obj.CheckCanInPlaceMode += CheckCostResources;
        }
        #endregion

        #region Resource

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
        protected override void InitObjectPool()
        {
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "SlotPool", 10, "Prefab_BuildingSystem/Prefab_BS_UI_Slot.prefab", (handle) => { RefreshSlots(); });
            base.InitObjectPool();
        }
        private void RefreshSlots()
        {
            var PlayerInventory = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory;
            this.objectPool.ResetPool("SlotPool");
            foreach (var raw in BuildingManager.Instance.GetRaw(this.Placer.SelectedPartInstance.Classification.ToString()))
            {
                var tPrefab = this.objectPool.GetNextObject("SlotPool", Slots);
                int needNum = raw.num;
                int haveNum = PlayerInventory.GetItemAllNum(raw.id);
                tPrefab.transform.Find("ItemNumber").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = needNum.ToString() + "/" + haveNum.ToString();
                if (needNum > haveNum)
                {
                    tPrefab.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.red;
                }
                tPrefab.transform.Find("ItemName").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(raw.id);
                tPrefab.transform.Find("ItemIcon").GetComponent<Image>().sprite = ItemManager.Instance.GetItemSprite(raw.id);
            }
        }
        #endregion

        #region SwitchHightAndWeight
        [ShowInInspector]
        private int heightcnt = 0;
        [ShowInInspector]
        private int stylecnt = 0;
        [ShowInInspector]
        private List<List<IBuildingPart>> swichArray = new List<List<IBuildingPart>>();
        [ShowInInspector]
        private int index_i = 0;
        [ShowInInspector]
        private int index_j = 0;
        private void InitSwitchData()
        {
            var stylesBuildingPart = BM.GetAllStyleIBuildingPartOnHeight1(this.Placer.SelectedPartInstance);
            stylecnt = stylesBuildingPart.Count;
            foreach (var style in stylesBuildingPart)
            {
                var tlist = BM.GetAllHeightIBuildingPartOnStyle(style);
                heightcnt = Mathf.Max(tlist.Count, heightcnt);
                swichArray.Add(tlist);
            }

            for (int i = 0; i < swichArray.Count; i++)
            {
                for (int j = 0; j < heightcnt; j++) 
                {
                    if (j >= swichArray[i].Count) swichArray[i].Add(null);
                }
            }
        }

        private IBuildingPart GetNextOnSwichingStyle(int offset)
        {
            int cnt = 1;
            while (swichArray[(index_i + stylecnt + cnt * offset)%stylecnt][index_j]==null) ++cnt;
            index_i = (index_i + stylecnt + cnt * offset) % stylecnt;
            return swichArray[index_i][index_j];
        }

        private IBuildingPart GetNextOnSwichingHeight(int offset)
        {
            int cnt = 1;
            while (swichArray[index_i][(index_j + heightcnt + cnt * offset) % heightcnt] == null) ++cnt;
            index_j = (index_j + heightcnt + cnt * offset) % heightcnt;
            return swichArray[index_i][index_j];
        }

        #endregion
    }
}

