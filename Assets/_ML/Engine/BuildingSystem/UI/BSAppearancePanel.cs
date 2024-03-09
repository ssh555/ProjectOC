using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSAppearancePanel : Engine.UI.UIBasePanel
    {
        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;

        #region UIGO引用
        private UnityEngine.UI.Image[] matInstance;
        private int activeIndex;
        private RectTransform matParent;
        private RectTransform templateMat;

        private UIKeyTip comfirm;
        private UIKeyTip back;
        private UIKeyTip matlast;
        private UIKeyTip matnext;

        #endregion

        #endregion

        #region Unity
        private void Awake()
        {
            LoadMatPackages();

            matParent = this.transform.Find("KT_AlterMat").Find("KT_AlterStyle").Find("Content") as RectTransform;
            this.templateMat = matParent.Find("MatTemplate") as RectTransform;
            templateMat.gameObject.SetActive(false);

            Transform keytips = this.transform.Find("KeyTip");

            comfirm = new UIKeyTip();
            comfirm.root = keytips.Find("KT_Comfirm") as RectTransform;
            comfirm.img = comfirm.root.Find("Image").GetComponent<Image>();
            comfirm.keytip = comfirm.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            comfirm.description = comfirm.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            comfirm.ReWrite(MonoBuildingManager.Instance.KeyTipDict["comfirm"]);

            back = new UIKeyTip();
            back.root = keytips.Find("KT_Back") as RectTransform;
            back.img = back.root.Find("Image").GetComponent<Image>();
            back.keytip = back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            back.description = back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            back.ReWrite(MonoBuildingManager.Instance.KeyTipDict["back"]);

            keytips = this.transform.Find("KT_AlterMat").Find("KT_AlterStyle");
            matlast = new UIKeyTip();
            matlast.root = keytips.Find("KT_Left") as RectTransform;
            matlast.img = matlast.root.Find("Image").GetComponent<Image>();
            matlast.keytip = matlast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            matlast.ReWrite(MonoBuildingManager.Instance.KeyTipDict["matlast"]);

            matnext = new UIKeyTip();
            matnext.root = keytips.Find("KT_Right") as RectTransform;
            matnext.img = matnext.root.Find("Image").GetComponent<Image>();
            matnext.keytip = matnext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            matnext.ReWrite(MonoBuildingManager.Instance.KeyTipDict["matnext"]);

        }

        private void OnDestroy()
        {
            Manager.GameManager.Instance.ABResourceManager.Release(matHandle);
        }
        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.RegisterInput();
        }

        public override void OnPause()
        {
            throw new Exception("建造系统外观UI面板不允许存在叠加其他UI面板的情况");
        }

        public override void OnRecovery()
        {
            throw new Exception("建造系统外观UI面板不允许存在叠加其他UI面板的情况");
        }

        public override void OnExit()
        {
            this.UnregisterInput();
            ClearInstance();

            Manager.GameManager.DestroyObj(this.gameObject);
        }
        #endregion

        #region Internal
        #region KeyFunction
        public void UnregisterInput()
        {
            this.Placer.BInput.BuildingAppearance.Disable();

            this.Placer.comfirmInputAction.performed += Placer_ComfirmAppearance;
            this.Placer.backInputAction.performed -= Placer_CancelAppearance;
            this.Placer.BInput.BuildingAppearance.AlterMaterial.performed -= Placer_OnChangeAppearance;

            //// to-do
            //ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
            //ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Disable();
            //ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Disable();
        }

        private void RegisterInput()
        {
            this.Placer.BInput.BuildingAppearance.Enable();

            this.Placer.comfirmInputAction.performed += Placer_ComfirmAppearance;
            this.Placer.backInputAction.performed += Placer_CancelAppearance;
            this.Placer.BInput.BuildingAppearance.AlterMaterial.performed += Placer_OnChangeAppearance;

            //// to-do
            //ProjectOC.Input.InputManager.PlayerInput.Player.Disable();
        }

        private void Placer_OnChangeAppearance(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Vector2 offset = obj.ReadValue<Vector2>();
            int of = offset.x > 0 ? 1 : -1;
            this._aCurrentIndex = (this._aCurrentIndex + of + _aCurrentTexs.Length) % _aCurrentTexs.Length;
            this.Placer.SelectedPartInstance.SetCopiedMaterial(this._aCurrentMatPackages[this._aCurrentIndex]);
            Refresh();
        }

        private void Placer_CancelAppearance(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            // 回退到之前的Mat
            this.Placer.SelectedPartInstance.SetCopiedMaterial(this._aMat);
            this.ExitAppearancePanel();
        }

        private void Placer_ComfirmAppearance(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.ExitAppearancePanel();
        }

        #endregion

        #region Refresh
        public override void Refresh()
        {
            Disactive(this.matInstance[activeIndex]);
            activeIndex = _aCurrentIndex;
            Active(this.matInstance[activeIndex]);
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
            if (matInstance != null)
            {
                foreach (var img in matInstance)
                {
                    Manager.GameManager.DestroyObj(img.sprite);
                    Manager.GameManager.DestroyObj(img.gameObject);
                }
            }

        }
        #endregion

        #region Appearance
        /// <summary>
        /// 用于UI显示的Mat-Texture2D
        /// </summary>
        private Texture2D[] _aCurrentTexs;
        /// <summary>
        /// 用于替换的Mat
        /// </summary>
        private BuildingCopiedMaterial[] _aCurrentMatPackages;
        /// <summary>
        /// 当前选择的MatIndex
        /// </summary>
        private int _aCurrentIndex;
        /// <summary>
        /// 所有的材质包
        /// </summary>
        private Dictionary<BuildingPartClassification, Engine.BuildingSystem.BSBPartMatPackage> _allMatPackages;

        /// <summary>
        /// 进入时选中的建筑物的Mode
        /// </summary>
        private BuildingMode _aMode;
        /// <summary>
        /// 进入时选中的建筑物的Mat
        /// </summary>
        private BuildingCopiedMaterial _aMat;

        /// <summary>
        /// 进入外观选择界面
        /// </summary>
        protected void EnterAppearancePanel()
        {
            this._aMode = this.Placer.SelectedPartInstance.Mode;
            this._aMat = this.Placer.SelectedPartInstance.GetCopiedMaterial();
            this.Placer.SelectedPartInstance.Mode = BuildingMode.None;

            var kv = _allMatPackages[this.Placer.SelectedPartInstance.Classification].ToMatPackage();

            this._aCurrentTexs = kv.Keys.ToArray();
            this._aCurrentMatPackages = kv.Values.ToArray();
            var mat = this.Placer.SelectedPartInstance.GetCopiedMaterial();
            this._aCurrentIndex = System.Array.IndexOf(this._aCurrentMatPackages, mat);

            // Init & Refresh
            ClearInstance();

            matInstance = new UnityEngine.UI.Image[this._aCurrentTexs.Length];
            for (int i = 0; i < this._aCurrentTexs.Length; ++i)
            {
                Sprite sprite = Sprite.Create(this._aCurrentTexs[i], new Rect(0, 0, this._aCurrentTexs[i].width, this._aCurrentTexs[i].height), new Vector2(0.5f, 0.5f));
                var img = Instantiate<GameObject>(templateMat.gameObject, matParent, false).GetComponent<UnityEngine.UI.Image>();
                img.sprite = sprite;

                Disactive(img);

                img.gameObject.SetActive(true);
                matInstance[i] = img;
            }

            activeIndex = _aCurrentIndex;

            Refresh();


        }

        protected void ExitAppearancePanel()
        {
            // 弹出外观UI
            MonoBuildingManager.Instance.PopPanel();
            // 禁用 Input.Appearance
            this.Placer.BInput.BuildingAppearance.Disable();
            this.Placer.SelectedPartInstance.Mode = this._aMode;
        }

        private const string MatPABPath = "ML/BuildingSystem/MatPackage";
        private AsyncOperationHandle matHandle;
        protected void LoadMatPackages()
        {
            _allMatPackages = new Dictionary<BuildingPartClassification, BSBPartMatPackage>();
            matHandle = Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<BSBPartMatPackage>(MatPABPath, (matP) =>
            {
                lock(_allMatPackages)
                {
                    this._allMatPackages.Add(matP.Classification, matP);
                }
            });
            matHandle.Completed += (handle) =>
            {
                EnterAppearancePanel();

                this.enabled = false;
            };
        }

        #endregion

        #endregion
    }

}
