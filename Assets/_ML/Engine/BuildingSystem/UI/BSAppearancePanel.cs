using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static ML.Engine.BuildingSystem.UI.BSAppearancePanel;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSAppearancePanel : Engine.UI.UIBasePanel<BSAppearancePanelStruct>
    {
        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;
        private MonoBuildingManager monoBM;
        #region UIGO引用
        private UnityEngine.UI.Image[] matInstance;
        private int activeIndex;
        private RectTransform matParent;
        private RectTransform templateMat;

        #endregion

        #endregion

        #region Unity
        protected override void Awake()
        {
            LoadMatPackages();
            monoBM = ML.Engine.Manager.GameManager.Instance.GetLocalManager<MonoBuildingManager>();
            matParent = this.transform.Find("KT_AlterMat").Find("KT_AlterStyle").Find("Content") as RectTransform;
            this.templateMat = matParent.Find("MatTemplate") as RectTransform;
            templateMat.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            Manager.GameManager.Instance.ABResourceManager.Release(matHandle);
        }
        #endregion

        #region Override
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
            base.OnExit();
            ClearInstance();
        }
        #endregion

        #region Internal
        #region KeyFunction
        protected override void UnregisterInput()
        {
            this.Placer.BInput.BuildingAppearance.Disable();

            this.Placer.comfirmInputAction.performed -= Placer_ComfirmAppearance;
            this.Placer.backInputAction.performed -= Placer_CancelAppearance;
            this.Placer.BInput.BuildingAppearance.AlterMaterial.performed -= Placer_OnChangeAppearance;

            //// to-do
            //ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
            //ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Disable();
            //ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Disable();
        }

        protected override void RegisterInput()
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
            if(matInstance == null || (activeIndex < 0 || activeIndex >= this.matInstance.Length))
            {
                return;
            }
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

            if(_allMatPackages.ContainsKey(this.Placer.SelectedPartInstance.Classification))
            {
                var kv = _allMatPackages[this.Placer.SelectedPartInstance.Classification].ToMatPackage();

                this._aCurrentTexs = kv.Keys.ToArray();
                this._aCurrentMatPackages = kv.Values.ToArray();
                var mat = this.Placer.SelectedPartInstance.GetCopiedMaterial();
                this._aCurrentIndex = System.Array.IndexOf(this._aCurrentMatPackages, mat);
                if(this._aCurrentIndex == -1)
                {
                    this._aCurrentIndex = 0;
#if UNITY_EDITOR
                    Debug.LogWarning($"{this.Placer.SelectedPartInstance.gameObject.name} - {this.Placer.SelectedPartInstance.Classification.ToString()} 的材质包不存在默认预制体配置的材质包");
#endif
                }
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
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"{this.Placer.SelectedPartInstance.gameObject.name} - {this.Placer.SelectedPartInstance.Classification.ToString()} 不存在对应的材质包");
#endif
            }



            Refresh();


        }

        protected void ExitAppearancePanel()
        {
            this.Placer.SelectedPartInstance.Mode = this._aMode;

            // 弹出外观UI
            monoBM.PopPanel();
            // 禁用 Input.Appearance
            this.Placer.BInput.BuildingAppearance.Disable();
        }

        private const string MatPABPath = "BS_MatAsset_MatPackage";
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

        #region TextContent
        [System.Serializable]
        public struct BSAppearancePanelStruct
        {
            public KeyTip[] KeyTips;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/BuildingSystem/UI";
            this.abname = "BSAppearancePanel";
            this.description = "BSAppearancePanel数据加载完成";
        }
        #endregion
    }

}
