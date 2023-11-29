using ML.Engine.BuildingSystem.BuildingPart;
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

        public static Dictionary<BuildingCategory, Texture2D> TCategoryDict = null;
        public static Dictionary<BuildingType, Texture2D> TTypeDict = null;
        public static int IsInit = -1;

        public static Sprite GetCategorySprite(BuildingCategory category)
        {
            if(!TCategoryDict.ContainsKey(category))
            {
                category = BuildingCategory.None;
            }
            Texture2D texture = TCategoryDict[category];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        public static Sprite GetTypeSprite(BuildingType type)
        {
            if (!TTypeDict.ContainsKey(type))
            {
                type = BuildingType.None;
            }
            Texture2D texture = TTypeDict[type];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        private BuildingManager BM => BuildingManager.Instance;

        private Dictionary<BuildingCategory, RectTransform> categoryInstance = new Dictionary<BuildingCategory, RectTransform>();
        private Dictionary<BuildingType, RectTransform> typeInstance = new Dictionary<BuildingType, RectTransform>();

        private RectTransform categoryParent;
        private RectTransform templateCategory;
        private RectTransform typeParent;
        private RectTransform templateType;

        private void Awake()
        {
            if (TCategoryDict == null)
            {
                StartCoroutine(InitCategoryTexture2D());
                StartCoroutine(InitTypeTexture2D());
            }

            this.categoryParent = this.transform.Find("SelectCategory").Find("Content") as RectTransform;
            this.templateCategory = this.categoryParent.Find("CategoryTemplate") as RectTransform;
            this.templateCategory.gameObject.SetActive(false);

            this.typeParent = this.transform.Find("SelectType").Find("Content") as RectTransform;
            this.templateType = this.typeParent.Find("TypeTemplate") as RectTransform;
            this.templateType.gameObject.SetActive(false);
        }

        private IEnumerator InitCategoryTexture2D()
        {
#if UNITY_EDITOR
            float startT = Time.time;
#endif
            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }

            TCategoryDict = new Dictionary<BuildingCategory, Texture2D>();

            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TCategoryABPath, null, out ab);
            yield return crequest;

            var request = ab.LoadAllAssetsAsync<Texture2D>();
            yield return request;

            foreach (var obj in request.allAssets)
            {
                var tex = (obj as Texture2D);
                if (tex != null && Enum.IsDefined(typeof(BuildingCategory), tex.name))
                {
                    TCategoryDict.Add((BuildingCategory)Enum.Parse(typeof(BuildingCategory), tex.name), tex);
                }
            }

            abmgr.UnLoadLocalABAsync(TCategoryABPath, false, null);
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
            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }

            TTypeDict = new Dictionary<BuildingType, Texture2D>();

            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TTypeABPath, null, out ab);
            yield return crequest;

            var request = ab.LoadAllAssetsAsync<Texture2D>();
            yield return request;

            foreach (var obj in request.allAssets)
            {
                var tex = (obj as Texture2D);
                if (tex != null && Enum.IsDefined(typeof(BuildingType), tex.name))
                {
                    TTypeDict.Add((BuildingType)Enum.Parse(typeof(BuildingType), tex.name), tex);
                }
            }

            abmgr.UnLoadLocalABAsync(TTypeABPath, false, null);
            ++IsInit;

#if UNITY_EDITOR
            Debug.Log("InitTypeTexture2D cost time: " + (Time.time - startT));
#endif
        }



        public IEnumerator Init(BuildingCategory[] categorys, BuildingType[] types, int cIndex, int tIndex)
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
                go.GetComponentInChildren<TextMeshProUGUI>().text = category.ToString();
                go.SetActive(true);
                this.categoryInstance.Add(category, go.transform as RectTransform);
            }

            StartCoroutine(this.ShowCategoryAndType(categorys[cIndex], types, tIndex));
        }

        private IEnumerator ShowCategoryAndType(BuildingCategory category, BuildingType[] types, int tIndex)
        {
            this.ClearTypeInstance();

            // ¸ü»» Category
            foreach (var instance in this.categoryInstance)
            {
                var img = instance.Value.GetComponentInChildren<Image>();
                if(instance.Key != category)
                {
                    img.transform.localScale = Vector3.one;
                }
                else
                {
                    img.transform.localScale = Vector3.one * 1.2f;
                }
            }

            // ¸ü»» Type
            foreach (var type in types)
            {
                var go = Instantiate<GameObject>(this.templateType.gameObject, this.typeParent, false);
                go.GetComponentInChildren<Image>().sprite = GetTypeSprite(type);
                go.GetComponentInChildren<TextMeshProUGUI>().text = type.ToString();
                go.SetActive(true);

                this.typeInstance.Add(type, go.transform as RectTransform);

                if(type == types[tIndex])
                {
                    go.GetComponentInChildren<Image>().transform.localScale = Vector3.one * 1.2f;
                }
            }

            yield break;
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

        private void Placer_OnBuildSelectionTypeChanged(BuildingCategory category, BuildingType[] types, int tIndex)
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
