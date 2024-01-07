using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSSelectBPartPanel : Engine.UI.UIBasePanel
    {
        public const string TCategoryABPath = "UI/BuildingSystem/Texture2D/Category";
        public const string TTypeABPath = "UI/BuildingSystem/Texture2D/Type";

        public static Dictionary<BuildingCategory1, Texture2D> TCategoryDict = null;
        public static Dictionary<BuildingCategory2, Texture2D> TTypeDict = null;
        public static int IsInit = -1;

        public static Sprite GetCategorySprite(BuildingCategory1 category)
        {
            if(!TCategoryDict.ContainsKey(category))
            {
                category = BuildingCategory1.None;
            }
            Texture2D texture = TCategoryDict[category];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        public static Sprite GetTypeSprite(BuildingCategory2 type)
        {
            if (!TTypeDict.ContainsKey(type))
            {
                type = BuildingCategory2.None;
            }
            Texture2D texture = TTypeDict[type];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        private BuildingManager BM => BuildingManager.Instance;

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

        private static bool IsLoading = false;
        private void Awake()
        {
            //    if (ABJAProcessor == null)
            //    {
            //        ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TextContent.KeyTip[]>("JSON/TextContent/InteractSystem", "InteractKeyTip", (datas) =>
            //        {
            //            foreach (var tip in datas)
            //            {
            //                STATIC_KTDict.Add(tip.keyname, tip);
            //            }

            //        }, null);
            //    }
            //    ABJAProcessor.StartLoadJsonAssetData();
            if (!IsLoading)
            {
                IsLoading = true;

                StartCoroutine(InitCategoryTexture2D());
                StartCoroutine(InitTypeTexture2D());
            }

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

            var request = ab.LoadAllAssetsAsync<Texture2D>();
            yield return request;

            foreach (var obj in request.allAssets)
            {
                var tex = (obj as Texture2D);
                if (tex != null && Enum.IsDefined(typeof(BuildingCategory1), tex.name))
                {
                    TCategoryDict.Add((BuildingCategory1)Enum.Parse(typeof(BuildingCategory1), tex.name), tex);
                }
            }

            ++IsInit;
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

            var request = ab.LoadAllAssetsAsync<Texture2D>();
            yield return request;

            foreach (var obj in request.allAssets)
            {
                var tex = (obj as Texture2D);
                if (tex != null && Enum.IsDefined(typeof(BuildingCategory2), tex.name))
                {
                    TTypeDict.Add((BuildingCategory2)Enum.Parse(typeof(BuildingCategory2), tex.name), tex);
                }
            }

            ++IsInit;

#if UNITY_EDITOR
            Debug.Log("InitTypeTexture2D cost time: " + (Time.time - startT));
#endif
        }



        public IEnumerator Init(BuildingCategory1[] categorys, BuildingCategory2[] types, int cIndex, int tIndex)
        {
            while(IsInit < 1)
            {
                yield return null;
            }

            this.ClearInstance();

            foreach (var category in categorys)
            {
                var go = Instantiate<GameObject>(this.templateCategory.gameObject, this.categoryParent, false);
                go.GetComponentInChildren<Image>().sprite = GetCategorySprite(category);
                go.GetComponentInChildren<TextMeshProUGUI>().text = MonoBuildingManager.Instance.Category1Dict[category.ToString()].GetDescription();
                go.SetActive(true);
                this.categoryInstance.Add(category, go.transform as RectTransform);
            }

            StartCoroutine(this.ShowCategoryAndType(categorys[cIndex], types, tIndex));
        }

        private IEnumerator ShowCategoryAndType(BuildingCategory1 category, BuildingCategory2[] types, int tIndex)
        {
            this.ClearTypeInstance();

            // ¸ü»» Category
            foreach (var instance in this.categoryInstance)
            {
                var img = instance.Value.GetComponentInChildren<Image>();
                if(instance.Key != category)
                {
                    Disactive(img);
                }
                else
                {
                    Active(img);
                }
            }

            // ¸ü»» Type
            foreach (var type in types)
            {
                var go = Instantiate<GameObject>(this.templateType.gameObject, this.typeParent, false);
                go.GetComponentInChildren<Image>().sprite = GetTypeSprite(type);
                go.GetComponentInChildren<TextMeshProUGUI>().text = MonoBuildingManager.Instance.Category2Dict[type.ToString()].GetDescription();
                go.SetActive(true);

                this.typeInstance.Add(type, go.transform as RectTransform);

                if(type == types[tIndex])
                {
                    Active(go.GetComponentInChildren<Image>());
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.templateCategory.parent.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.templateType.parent.GetComponent<RectTransform>());
            yield break;
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

        private void ClearTypeInstance()
        {
            foreach (var instance in this.typeInstance.Values)
            {
                Destroy(instance.GetComponentInChildren<Image>().sprite);
                Destroy(instance.gameObject);
            }
            this.typeInstance.Clear();
        }


        public override void OnEnter()
        {
            base.OnEnter();
            BM.Placer.OnBuildSelectionTypeChanged += Placer_OnBuildSelectionTypeChanged;
        }

        private void Placer_OnBuildSelectionTypeChanged(BuildingCategory1 category, BuildingCategory2[] types, int tIndex)
        {
            StartCoroutine(this.ShowCategoryAndType(category, types, tIndex));
        }

        public override void OnPause()
        {
            base.OnPause();
            BM.Placer.OnBuildSelectionTypeChanged -= Placer_OnBuildSelectionTypeChanged;
        }

        public override void OnRecovery()
        {
            BM.Placer.OnBuildSelectionTypeChanged += Placer_OnBuildSelectionTypeChanged;

            base.OnRecovery();
        }

        public override void OnExit()
        {
            BM.Placer.OnBuildSelectionTypeChanged -= Placer_OnBuildSelectionTypeChanged;

            Destroy(this.gameObject);
        }

        private void OnDestroy()
        {

            this.ClearInstance();
        }

    }

}
