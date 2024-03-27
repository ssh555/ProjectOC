using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProjectOC.ManagerNS;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace ML.Engine.BuildingSystem.UI
{
    /// <summary>
    /// 放置的一级
    /// </summary>
    public class BSSelectBPartPanel : Engine.UI.UIBasePanel
    {
        #region 载入资源
        public const string TCategorySpriteAtlasPath = "ML/BuildingSystem/Category/SA_Build_Category.spriteatlasv2";
        public const string TTypeABPath = "ML/BuildingSystem/Type/SA_Build_Type.spriteatlasv2";

        public int IsInit = -1;

        private SpriteAtlas typeAtlas = null,categoryAtlas = null;
        private MonoBuildingManager monoBM;
        public Sprite GetCategorySprite(BuildingCategory1 category)
        {
            Sprite sprite = categoryAtlas.GetSprite(category.ToString());;
            if (sprite == null)
            {
                sprite = categoryAtlas.GetSprite("None");
            }
            return sprite;
        }
        public Sprite GetTypeSprite(BuildingCategory2 type)
        {
            Sprite sprite = typeAtlas.GetSprite(type.ToString());;
            if (sprite == null)
            {
                sprite = typeAtlas.GetSprite("None");
            }
            return sprite;
        }


        private void InitCategoryTexture2D()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(TCategorySpriteAtlasPath).Completed += (handle) =>
            {
                categoryAtlas = handle.Result as SpriteAtlas;
                CanSelectCategory1 = BuildingManager.Instance.GetRegisteredCategory();

                ++IsInit;
                if (IsInit >= 1)
                {
                    InitAsset();
                }
            };
        }

        private void InitTypeTexture2D()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(TTypeABPath).Completed += (handle) =>
            {
                typeAtlas = handle.Result as SpriteAtlas;
                CanSelectCategory2 = BuildingManager.Instance.GetRegisteredType();

                ++IsInit;
                if (IsInit >= 1)
                {
                    InitAsset();
                }
            };
        }


        private void UnloadAsset()
        {
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            abmgr.Release(this.categoryAtlas);
            abmgr.Release(this.typeAtlas);
        }

        private void InitAsset()
        {
            if (S_LastSelectedCategory1Index != -1)
            {
                this.SelectedCategory1Index = S_LastSelectedCategory1Index;
            }
            this.UpdatePlaceBuildingType(this.CanSelectCategory1[this.SelectedCategory1Index]);
            if (S_LastSelectedCategory2Index != -1)
            {
                this.SelectedCategory2Index = S_LastSelectedCategory2Index;
            }

            this.ClearInstance();

            foreach (var category in this.CanSelectCategory1)
            {
                var go = Instantiate<GameObject>(this.templateCategory.gameObject, this.categoryParent, false);
                go.GetComponentInChildren<Image>().sprite = GetCategorySprite(category);
                go.GetComponentInChildren<TextMeshProUGUI>().text = monoBM.Category1Dict[category.ToString()].GetDescription();
                go.SetActive(true);
                this.categoryInstance.Add(category, go.transform as RectTransform);
            }

            this.Refresh();
        }
        #endregion

        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;

        #region UIGO引用
        private Dictionary<BuildingCategory1, RectTransform> categoryInstance = new Dictionary<BuildingCategory1, RectTransform>();
        private Dictionary<BuildingCategory2, RectTransform> typeInstance = new Dictionary<BuildingCategory2, RectTransform>();

        private RectTransform categoryParent;
        private RectTransform templateCategory;
        private RectTransform typeParent;
        private RectTransform templateType;

        private UIKeyTip categorylast;
        private UIKeyTip categorynext;
        private UIKeyTip typelast;
        private UIKeyTip typenext;
        private UIKeyTip comfirm;
        private UIKeyTip back;

        #endregion

        #endregion

        #region Unity
        protected override void Awake()
        {
            InitCategoryTexture2D();
            InitTypeTexture2D();
            monoBM = GameManager.Instance.GetLocalManager<MonoBuildingManager>();
            this.categoryParent = this.transform.Find("SelectCategory").Find("Content") as RectTransform;
            this.templateCategory = this.categoryParent.Find("CategoryTemplate") as RectTransform;
            this.templateCategory.gameObject.SetActive(false);

            this.typeParent = this.transform.Find("SelectType").Find("Content") as RectTransform;
            this.templateType = this.typeParent.Find("TypeTemplate") as RectTransform;
            this.templateType.gameObject.SetActive(false);

            Transform keytip = this.transform.Find("KeyTip");

            comfirm = new UIKeyTip();
            comfirm.root = keytip.Find("KT_Comfirm") as RectTransform;
            comfirm.img = comfirm.root.Find("Image").GetComponent<Image>();
            comfirm.keytip = comfirm.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            comfirm.description = comfirm.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            comfirm.ReWrite(monoBM.KeyTipDict["comfirm"]);

            back = new UIKeyTip();
            back.root = keytip.Find("KT_Back") as RectTransform;
            back.img = back.root.Find("Image").GetComponent<Image>();
            back.keytip = back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            back.description = back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            back.ReWrite(monoBM.KeyTipDict["back"]);

            keytip = this.transform.Find("SelectCategory");

            categorylast = new UIKeyTip();
            categorylast.root = keytip.Find("KT_Last") as RectTransform;
            categorylast.img = categorylast.root.Find("Image").GetComponent<Image>();
            categorylast.keytip = categorylast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            categorylast.ReWrite(monoBM.KeyTipDict["categorylast"]);

            categorynext = new UIKeyTip();
            categorynext.root = keytip.Find("KT_Next") as RectTransform;
            categorynext.img = categorynext.root.Find("Image").GetComponent<Image>();
            categorynext.keytip = categorynext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            categorynext.ReWrite(monoBM.KeyTipDict["categorynext"]);

            keytip = this.transform.Find("SelectType");

            typelast = new UIKeyTip();
            typelast.root = keytip.Find("KT_Last") as RectTransform;
            typelast.img = typelast.root.Find("Image").GetComponent<Image>();
            typelast.keytip = typelast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            typelast.ReWrite(monoBM.KeyTipDict["typelast"]);

            typenext = new UIKeyTip();
            typenext.root = keytip.Find("KT_Next") as RectTransform;
            typenext.img = typenext.root.Find("Image").GetComponent<Image>();
            typenext.keytip = typenext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            typenext.ReWrite(monoBM.KeyTipDict["typenext"]);
        }

        #endregion

        #region Refresh
        public override void Refresh()
        {
            this.ClearCategory2Instance();

            // 更换 Category1
            foreach (var instance in this.categoryInstance)
            {
                var img = instance.Value.GetComponentInChildren<Image>();
                if(instance.Key != this.SelectedCategory1)
                {
                    Disactive(img);
                }
                else
                {
                    Active(img);
                }
            }

            // 更换 Category2
            foreach (var category2 in this.CanSelectCategory2)
            {
                var go = Instantiate<GameObject>(this.templateType.gameObject, this.typeParent, false);
                go.GetComponentInChildren<Image>().sprite = GetTypeSprite(category2);
                go.GetComponentInChildren<TextMeshProUGUI>().text = monoBM.Category2Dict[category2.ToString()].GetDescription();
                go.SetActive(true);

                this.typeInstance.Add(category2, go.transform as RectTransform);

                if(category2 == this.SelectedCategory2)
                {
                    Active(go.GetComponentInChildren<Image>());
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.templateCategory.parent.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.templateType.parent.GetComponent<RectTransform>());
        }

        /// <summary>
        /// to-yl
        /// </summary>
        /// <param name="img"></param>
        private void Active(Image img)
        {
            img.transform.localScale = Vector3.one * 1.2f;
        }

        /// <summary>
        /// to-yl
        /// </summary>
        /// <param name="img"></param>
        private void Disactive(Image img)
        {
            img.transform.localScale = Vector3.one;
        }

        private void ClearInstance()
        {
            foreach (var instance in this.categoryInstance.Values)
            {
                Manager.GameManager.DestroyObj(instance.GetComponentInChildren<Image>().sprite);
                Manager.GameManager.DestroyObj(instance.gameObject);
            }
            this.categoryInstance.Clear();
            foreach (var instance in this.typeInstance.Values)
            {
                Manager.GameManager.DestroyObj(instance.GetComponentInChildren<Image>().sprite);
                Manager.GameManager.DestroyObj(instance.gameObject);
            }
            this.typeInstance.Clear();
        }

        private void ClearCategory2Instance()
        {
            foreach (var instance in this.typeInstance.Values)
            {
                Manager.GameManager.DestroyObj(instance.GetComponentInChildren<Image>().sprite);
                Manager.GameManager.DestroyObj(instance.gameObject);
            }
            this.typeInstance.Clear();
        }
        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Placer.Mode = BuildingMode.Place;
            this.Placer.InteractBPartList.Clear();
            this.Placer.SelectedPartInstance = null;

        }
        public override void OnExit()
        {
            S_LastSelectedCategory1Index = this.SelectedCategory1Index;
            S_LastSelectedCategory2Index = this.SelectedCategory2Index;
            base.OnExit();
            this.ClearInstance();
            this.UnloadAsset();
        }
        #endregion

        #region KeyFunction
        protected override void UnregisterInput()
        {
            this.Placer.BInput.BuildSelection.Disable();

            this.Placer.comfirmInputAction.performed -= Placer_ComfirmSelection;
            this.Placer.backInputAction.performed -= Placer_CancelSelection;
            this.Placer.BInput.BuildSelection.AlterCategory.performed -= Placer_AlterCategory1;

            this.Placer.BInput.BuildSelection.AlternativeType.started -= Placer_AlterCategory2;
        }

        protected override void RegisterInput()
        {
            this.Placer.BInput.BuildSelection.Enable();

            this.Placer.comfirmInputAction.performed += Placer_ComfirmSelection;
            this.Placer.backInputAction.performed += Placer_CancelSelection;
            this.Placer.BInput.BuildSelection.AlterCategory.performed += Placer_AlterCategory1;

            this.Placer.BInput.BuildSelection.AlternativeType.started += Placer_AlterCategory2;
        }

        private void Placer_AlterCategory2(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            int offset = obj.ReadValue<float>() > 0 ? 1 : -1;
            this.SelectedCategory2Index = (this.SelectedCategory2Index + offset + this.CanSelectCategory2.Length) % this.CanSelectCategory2.Length;

            this.Refresh();
        }

        private void Placer_AlterCategory1(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            int offset = obj.ReadValue<float>() > 0 ? 1 : -1;
            this.SelectedCategory1Index = (this.SelectedCategory1Index + this.CanSelectCategory1.Length + offset) % this.CanSelectCategory1.Length;
            this.UpdatePlaceBuildingType(this.CanSelectCategory1[this.SelectedCategory1Index]);
        }

        private void Placer_CancelSelection(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.Mode = BuildingMode.Interact;
            monoBM.PopPanel();
        }

        private void Placer_ComfirmSelection(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(this.CanSelectCategory1[this.SelectedCategory1Index], this.CanSelectCategory2[this.SelectedCategory2Index]);
            monoBM.PopAndPushPanel<BSPlaceModePanel>();
        }
        #endregion

        #region Select
        public static int S_LastSelectedCategory1Index = -1;
        public static int S_LastSelectedCategory2Index = -1;

        /// <summary>
        /// 当前选中的Category1Index
        /// </summary>
        [ShowInInspector, LabelText("Category1Index"), FoldoutGroup("PlaceMode")]
        protected int SelectedCategory1Index = 0;
        /// <summary>
        /// 当前选中的Category2Index
        /// </summary>
        [ShowInInspector, LabelText("Category2Index"), FoldoutGroup("PlaceMode")]
        protected int SelectedCategory2Index = 0;
        /// <summary>
        /// 当前可选的Category1
        /// </summary>
        [ShowInInspector, LabelText("Category1Array"), FoldoutGroup("PlaceMode")]
        protected BuildingCategory1[] CanSelectCategory1;
        /// <summary>
        /// 当前可选的Category2
        /// </summary>
        [ShowInInspector, LabelText("Category2Array"), FoldoutGroup("PlaceMode")]
        protected BuildingCategory2[] CanSelectCategory2;
        /// <summary>
        /// 选择的Category1
        /// </summary>
        public BuildingCategory1 SelectedCategory1 => this.CanSelectCategory1[this.SelectedCategory1Index];
        /// <summary>
        /// 选择的Category2
        /// </summary>
        public BuildingCategory2 SelectedCategory2 => this.CanSelectCategory2[this.SelectedCategory2Index];

        protected void UpdatePlaceBuildingType(BuildingCategory1 category)
        {
            this.CanSelectCategory2 = BuildingManager.Instance.GetRegisteredType().Where(type => (int)type >= ((int)category * 100) && (int)type < ((int)category * 100 + 100)).ToArray();

            this.SelectedCategory2Index = 0;

            this.Refresh();
        }
        #endregion
    }

}
