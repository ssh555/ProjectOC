using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.TextContent;
using Sirenix.OdinInspector;
using ProjectOC.ProNodeNS;
using ProjectOC.StoreNS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace ML.Engine.BuildingSystem.UI
{
    /// <summary>
    /// 放置的三级
    /// </summary>
    public class BSPlaceModePanel : Engine.UI.UIBasePanel, Timer.ITickComponent
    {
        #region Property|Field

        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;
        private ProjectOC.Player.PlayerCharacter Player => GameObject.Find("PlayerCharacter")?.GetComponent<ProjectOC.Player.PlayerCharacter>();


        #region UIGO引用
        private Dictionary<BuildingCategory3, RectTransform> styleInstance = new Dictionary<BuildingCategory3, RectTransform>();
        private RectTransform styleParent;
        private RectTransform templateStyle;

        #region KeyTip
        private UIKeyTip place;
        private UIKeyTip altersocket;
        private UIKeyTip altermat;
        private UIKeyTip rotateleft;
        private UIKeyTip rotateright;
        private UIKeyTip back;
        private UIKeyTip keycom;
        private UIKeyTip stylelast;
        private UIKeyTip stylenext;
        private UIKeyTip heightlast;
        private UIKeyTip heightnext;

        #endregion

        #endregion

        #endregion

        #region Unity

        private void Awake()
        {
            this.StartCoroutine(InitStyleTexture2D());

            this.styleParent = this.transform.Find("KT_AlterHeight").Find("KT_AlterStyle").Find("Content") as RectTransform;
            this.templateStyle = this.styleParent.Find("StyleTemplate") as RectTransform;
            this.templateStyle.gameObject.SetActive(false);

            Transform keytips = this.transform.Find("KeyTip");

            place = new UIKeyTip();
            place.root = keytips.Find("KT_Place") as RectTransform;
            place.img = place.root.Find("Image").GetComponent<Image>();
            place.keytip = place.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            place.description = place.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            place.ReWrite(MonoBuildingManager.Instance.KeyTipDict["place"]);

            altersocket = new UIKeyTip();
            altersocket.root = keytips.Find("KT_AlterSocket") as RectTransform;
            altersocket.img = altersocket.root.Find("Image").GetComponent<Image>();
            altersocket.keytip = altersocket.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            altersocket.description = altersocket.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            altersocket.ReWrite(MonoBuildingManager.Instance.KeyTipDict["altersocket"]);

            altermat = new UIKeyTip();
            altermat.root = keytips.Find("KT_AlterMat") as RectTransform;
            altermat.img = altermat.root.Find("Image").GetComponent<Image>();
            altermat.keytip = altermat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            altermat.description = altermat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            altermat.ReWrite(MonoBuildingManager.Instance.KeyTipDict["altermat"]);

            rotateright = new UIKeyTip();
            rotateright.root = keytips.Find("KT_Rotate") as RectTransform;
            rotateright.img = rotateright.root.Find("Right").GetComponent<Image>();
            rotateright.keytip = rotateright.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            rotateright.description = rotateright.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            rotateright.ReWrite(MonoBuildingManager.Instance.KeyTipDict["rotateright"]);

            rotateleft = new UIKeyTip();
            rotateleft.root = keytips.Find("KT_Rotate") as RectTransform;
            rotateleft.img = rotateleft.root.Find("Left").GetComponent<Image>();
            rotateleft.keytip = rotateleft.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            rotateleft.description = rotateright.description;
            rotateleft.ReWrite(MonoBuildingManager.Instance.KeyTipDict["rotateleft"]);

            back = new UIKeyTip();
            back.root = keytips.Find("KT_Back") as RectTransform;
            back.img = back.root.Find("Image").GetComponent<Image>();
            back.keytip = back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            back.description = back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            back.ReWrite(MonoBuildingManager.Instance.KeyTipDict["back"]);

            keycom = new UIKeyTip();
            keycom.root = this.transform.Find("KT_KeyCom") as RectTransform;
            keycom.img = keycom.root.Find("Image").GetComponent<Image>();
            keycom.keytip = keycom.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            keycom.description = keycom.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            keycom.ReWrite(MonoBuildingManager.Instance.KeyTipDict["keycom"]);



            keytips = this.transform.Find("KT_AlterHeight");
            heightlast = new UIKeyTip();
            heightlast.root = keytips.Find("KT_Down") as RectTransform;
            heightlast.img = heightlast.root.Find("Image").GetComponent<Image>();
            heightlast.keytip = heightlast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            heightlast.ReWrite(MonoBuildingManager.Instance.KeyTipDict["heightlast"]);

            heightnext = new UIKeyTip();
            heightnext.root = keytips.Find("KT_Up") as RectTransform;
            heightnext.img = heightnext.root.Find("Image").GetComponent<Image>();
            heightnext.keytip = heightnext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            heightnext.ReWrite(MonoBuildingManager.Instance.KeyTipDict["heightnext"]);

            keytips = keytips.Find("KT_AlterStyle");
            stylelast = new UIKeyTip();
            stylelast.root = keytips.Find("KT_Left") as RectTransform;
            stylelast.img = stylelast.root.Find("Image").GetComponent<Image>();
            stylelast.keytip = stylelast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            stylelast.ReWrite(MonoBuildingManager.Instance.KeyTipDict["stylelast"]);

            stylenext = new UIKeyTip();
            stylenext.root = keytips.Find("KT_Right") as RectTransform;
            stylenext.img = stylenext.root.Find("Image").GetComponent<Image>();
            stylenext.keytip = stylenext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            stylenext.ReWrite(MonoBuildingManager.Instance.KeyTipDict["stylenext"]);
        }

        #endregion

        #region 载入资产
        public const string TStyleABPath = "UI/BuildingSystem/Texture2D/Style";

        public Dictionary<BuildingCategory3, Texture2D> TStyleDict = null;
        /// <summary>
        /// 资产是否完成载入
        /// </summary>
        public bool IsInit = false;
        private IEnumerator InitStyleTexture2D()
        {
#if UNITY_EDITOR
            float startT = Time.time;
#endif
            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }

            TStyleDict = new Dictionary<BuildingCategory3, Texture2D>();

            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TStyleABPath, null, out ab);
            yield return crequest;
            if(crequest != null)
            {
                ab = crequest.assetBundle;
            }

            var request = ab.LoadAllAssetsAsync<Texture2D>();
            yield return request;

            foreach (var obj in request.allAssets)
            {
                var tex = (obj as Texture2D);
                if (tex != null && Enum.IsDefined(typeof(BuildingCategory3), tex.name))
                {
                    TStyleDict.Add((BuildingCategory3)Enum.Parse(typeof(BuildingCategory3), tex.name), tex);
                }
            }

            IsInit = true;
            this.enabled = false;

            this.Refresh();
#if UNITY_EDITOR
            Debug.Log("InitStyleTexture2D cost time: " + (Time.time - startT));
#endif
        }

        private IEnumerator UnloadAsset()
        {
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TStyleABPath, null, out ab);
            yield return crequest;
            if (crequest != null)
            {
                ab = crequest.assetBundle;
            }
            ab.UnloadAsync(true);
        }

        public Sprite GetStyleSprite(BuildingCategory3 style)
        {
            if (!TStyleDict.ContainsKey(style))
            {
                style = BuildingCategory3.None;
            }
            Texture2D texture = TStyleDict[style];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
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
                Destroy(instance.GetComponentInChildren<Image>().sprite);
                Destroy(instance.gameObject);
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
            this.RegisterInput();
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
            this.UnregisterInput();
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
        }

        #endregion

        #region KeyFunction
        private void UnregisterInput()
        {
            this.Placer.DisablePlayerInput();

            this.Placer.BInput.BuildPlaceMode.Disable();

            Manager.GameManager.Instance.TickManager.UnregisterFixedTick(this);
            this.Placer.BInput.BuildPlaceMode.KeyCom.performed -= Placer_EnterKeyCom;
            this.Placer.backInputAction.performed -= Placer_CancelPlace;
            this.Placer.BInput.BuildPlaceMode.Rotate.performed -= Placer_RotateBPart;
            this.Placer.BInput.BuildPlaceMode.ChangeActiveSocket.performed -= Placer_ChangeActiveSocket;
            this.Placer.BInput.BuildPlaceMode.ChangeStyle.performed -= Placer_ChangeBPartStyle;
            this.Placer.BInput.BuildPlaceMode.ChangeHeight.performed -= Placer_ChangeBPartHeight;
            this.Placer.BInput.BuildPlaceMode.ChangeOutLook.performed -= Placer_EnterAppearance;
            this.Placer.comfirmInputAction.performed -= Placer_ComfirmPlaceBPart;
        }

        private void RegisterInput()
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
            this.Placer.BInput.BuildPlaceMode.Rotate.performed += Placer_RotateBPart;
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
            MonoBuildingManager.Instance.PushPanel<BSAppearancePanel>();
        }

        private void Placer_ChangeBPartStyle(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.AlternateBPartOnHeight(obj.ReadValue<float>() > 0);
           this.Refresh();
        }

        private void Placer_ChangeBPartHeight(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.AlternateBPartOnStyle(obj.ReadValue<float>() > 0);
            this.Refresh();
        }

        private void Placer_ChangeActiveSocket(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.SelectedPartInstance.AlternativeActiveSocket();
        }

        private void Placer_RotateBPart(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            int offset = obj.ReadValue<float>() > 0 ? 1 : -1;

            if(this.Placer.IsEnableGridSupport)
            {
                this.Placer.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.Placer.EnableGridRotRate * offset, this.Placer.SelectedPartInstance.transform.up);
            }
            else
            {
                this.Placer.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.Placer.DisableGridRotRate * Time.deltaTime * offset, this.Placer.SelectedPartInstance.transform.up);
            }
        }

        private void Placer_CancelPlace(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.ResetBPart();

            this.Placer.Mode = BuildingMode.Interact;

            MonoBuildingManager.Instance.PopPanel();
        }

        private void Placer_EnterKeyCom(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            MonoBuildingManager.Instance.PushPanel<BSPlaceMode_KeyComPanel>();
        }
        #endregion

        #region Event
        private bool CheckCostResources(IBuildingPart bpart)
        {
            if (CompositeManager.Instance.CanComposite(Player.Inventory, BuildingManager.Instance.GetID(bpart.Classification.ToString().Replace('-', '_'))))
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
            CompositeManager.Instance.OnlyCostResource(Player.Inventory, BuildingManager.Instance.GetID(bpart.Classification.ToString().Replace('-', '_')));
            BM.Placer.SelectedPartInstance.CheckCanInPlaceMode -= CheckCostResources;
        }

        private void Placer_OnPlaceModeChangeBPart(IBuildingPart obj)
        {
            obj.CheckCanInPlaceMode += CheckCostResources;
        }
        #endregion
    }
}

