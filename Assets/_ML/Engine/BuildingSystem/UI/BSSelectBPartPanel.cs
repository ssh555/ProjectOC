using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.TextContent;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
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
        public const string TCategoryABPath = "UI/BuildingSystem/Texture2D/Category";
        public const string TTypeABPath = "UI/BuildingSystem/Texture2D/Type";

        public Dictionary<BuildingCategory1, Texture2D> TCategoryDict = null;
        public Dictionary<BuildingCategory2, Texture2D> TTypeDict = null;
        public int IsInit = -1;

        public Sprite GetCategorySprite(BuildingCategory1 category)
        {
            if(!TCategoryDict.ContainsKey(category))
            {
                category = BuildingCategory1.None;
            }
            Texture2D texture = TCategoryDict[category];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        public Sprite GetTypeSprite(BuildingCategory2 type)
        {
            if (!TTypeDict.ContainsKey(type))
            {
                type = BuildingCategory2.None;
            }
            Texture2D texture = TTypeDict[type];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }


        private IEnumerator InitCategoryTexture2D()
        {
#if UNITY_EDITOR
            float startT = Time.time;
#endif
            TCategoryDict = new Dictionary<BuildingCategory1, Texture2D>();

            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }


            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TCategoryABPath, null, out ab);
            yield return crequest;
            if(crequest != null)
            {
                ab = crequest.assetBundle;
            }

            var request = ab.LoadAllAssetsAsync<Texture2D>();
            yield return request;

            CanSelectCategory1 = BuildingManager.Instance.GetRegisteredCategory();
            foreach (var obj in request.allAssets)
            {
                var tex = (obj as Texture2D);
                if (tex != null && Enum.IsDefined(typeof(BuildingCategory1), tex.name))
                {
                    TCategoryDict.Add((BuildingCategory1)Enum.Parse(typeof(BuildingCategory1), tex.name), tex);
                }
            }

            ++IsInit;
            if (IsInit >= 1)
            {
                InitAsset();
            }
#if UNITY_EDITOR
            Debug.Log("InitCategoryTexture2D cost time: " + (Time.time - startT));
#endif
        }

        private IEnumerator InitTypeTexture2D()
        {
#if UNITY_EDITOR
            float startT = Time.time;
#endif
            TTypeDict = new Dictionary<BuildingCategory2, Texture2D>();

            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }


            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TTypeABPath, null, out ab);
            yield return crequest;
            if(crequest != null)
            {
                ab = crequest.assetBundle;
            }

            var request = ab.LoadAllAssetsAsync<Texture2D>();
            yield return request;
            CanSelectCategory2 = BuildingManager.Instance.GetRegisteredType();

            foreach (var obj in request.allAssets)
            {
                var tex = (obj as Texture2D);
                if (tex != null && Enum.IsDefined(typeof(BuildingCategory2), tex.name))
                {
                    TTypeDict.Add((BuildingCategory2)Enum.Parse(typeof(BuildingCategory2), tex.name), tex);
                }
            }

            ++IsInit;

            if(IsInit >= 1)
            {
                InitAsset();
            }

#if UNITY_EDITOR
            Debug.Log("InitTypeTexture2D cost time: " + (Time.time - startT));
#endif
        }


        private IEnumerator UnloadAsset()
        {
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle tab;
            var trequest = abmgr.LoadLocalABAsync(TTypeABPath, null, out tab);
            AssetBundle cab;
            var crequest = abmgr.LoadLocalABAsync(TCategoryABPath, null, out cab);

            yield return trequest;
            if(trequest != null)
            {
                tab = trequest.assetBundle;
            }
            tab.UnloadAsync(true);

            yield return crequest;
            if (crequest != null)
            {
                cab = crequest.assetBundle;
            }
            cab.UnloadAsync(true);
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
                go.GetComponentInChildren<TextMeshProUGUI>().text = MonoBuildingManager.Instance.Category1Dict[category.ToString()].GetDescription();
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
        private void Awake()
        {
            StartCoroutine(InitCategoryTexture2D());
            StartCoroutine(InitTypeTexture2D());

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
            comfirm.ReWrite(MonoBuildingManager.Instance.KeyTipDict["comfirm"]);

            back = new UIKeyTip();
            back.root = keytip.Find("KT_Back") as RectTransform;
            back.img = back.root.Find("Image").GetComponent<Image>();
            back.keytip = back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            back.description = back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            back.ReWrite(MonoBuildingManager.Instance.KeyTipDict["back"]);

            keytip = this.transform.Find("SelectCategory");

            categorylast = new UIKeyTip();
            categorylast.root = keytip.Find("KT_Last") as RectTransform;
            categorylast.img = categorylast.root.Find("Image").GetComponent<Image>();
            categorylast.keytip = categorylast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            categorylast.ReWrite(MonoBuildingManager.Instance.KeyTipDict["categorylast"]);

            categorynext = new UIKeyTip();
            categorynext.root = keytip.Find("KT_Next") as RectTransform;
            categorynext.img = categorynext.root.Find("Image").GetComponent<Image>();
            categorynext.keytip = categorynext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            categorynext.ReWrite(MonoBuildingManager.Instance.KeyTipDict["categorynext"]);

            keytip = this.transform.Find("SelectType");

            typelast = new UIKeyTip();
            typelast.root = keytip.Find("KT_Last") as RectTransform;
            typelast.img = typelast.root.Find("Image").GetComponent<Image>();
            typelast.keytip = typelast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            typelast.ReWrite(MonoBuildingManager.Instance.KeyTipDict["typelast"]);

            typenext = new UIKeyTip();
            typenext.root = keytip.Find("KT_Next") as RectTransform;
            typenext.img = typenext.root.Find("Image").GetComponent<Image>();
            typenext.keytip = typenext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            typenext.ReWrite(MonoBuildingManager.Instance.KeyTipDict["typenext"]);
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
                go.GetComponentInChildren<TextMeshProUGUI>().text = MonoBuildingManager.Instance.Category2Dict[category2.ToString()].GetDescription();
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
                Destroy(instance.GetComponentInChildren<Image>().sprite);
                Destroy(instance.gameObject);
            }
            this.categoryInstance.Clear();
            foreach (var instance in this.typeInstance.Values)
            {
                Destroy(instance.GetComponentInChildren<Image>().sprite);
                Destroy(instance.gameObject);
            }
            this.typeInstance.Clear();
        }

        private void ClearCategory2Instance()
        {
            foreach (var instance in this.typeInstance.Values)
            {
                Destroy(instance.GetComponentInChildren<Image>().sprite);
                Destroy(instance.gameObject);
            }
            this.typeInstance.Clear();
        }
        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.RegisterInput();
            this.Placer.Mode = BuildingMode.Place;
            this.Placer.InteractBPartList.Clear();
            this.Placer.SelectedPartInstance = null;

        }

        public override void OnPause()
        {
            base.OnPause();
            this.UnregisterInput();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.RegisterInput();
        }

        public override void OnExit()
        {
            S_LastSelectedCategory1Index = this.SelectedCategory1Index;
            S_LastSelectedCategory2Index = this.SelectedCategory2Index;
            base.OnExit();
            this.ClearInstance();
            this.UnregisterInput();
            this.UnloadAsset();
        }
        #endregion

        #region KeyFunction
        private void UnregisterInput()
        {
            this.Placer.BInput.BuildSelection.Disable();

            this.Placer.comfirmInputAction.performed -= Placer_ComfirmSelection;
            this.Placer.backInputAction.performed -= Placer_CancelSelection;
            this.Placer.BInput.BuildSelection.AlterCategory.performed -= Placer_AlterCategory1;
            this.Placer.BInput.BuildSelection.AlternativeType.performed -= Placer_AlterCategory2;
        }

        private void RegisterInput()
        {
            this.Placer.BInput.BuildSelection.Enable();

            this.Placer.comfirmInputAction.performed += Placer_ComfirmSelection;
            this.Placer.backInputAction.performed += Placer_CancelSelection;
            this.Placer.BInput.BuildSelection.AlterCategory.performed += Placer_AlterCategory1;
            this.Placer.BInput.BuildSelection.AlternativeType.performed += Placer_AlterCategory2;
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
            MonoBuildingManager.Instance.PopPanel();
        }

        private void Placer_ComfirmSelection(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(this.CanSelectCategory1[this.SelectedCategory1Index], this.CanSelectCategory2[this.SelectedCategory2Index]);
            MonoBuildingManager.Instance.PopPanel();
            MonoBuildingManager.Instance.PushPanel<BSPlaceModePanel>();
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
