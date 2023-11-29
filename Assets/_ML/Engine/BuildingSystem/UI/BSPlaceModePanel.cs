using ML.Engine.BuildingSystem.BuildingPart;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSPlaceModePanel : Engine.UI.UIBasePanel
    {
        public const string TStyleABPath = "UI/BuildingSystem/Texture2D/Style";

        public static Dictionary<BuildingStyle, Texture2D> TStyleDict = null;
        public static bool IsInit = false;

        public static Sprite GetStyleSprite(BuildingStyle style)
        {
            if (!TStyleDict.ContainsKey(style))
            {
                style = BuildingStyle.None;
            }
            Texture2D texture = TStyleDict[style];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }


        private BuildingManager BM => BuildingManager.Instance;

        private Dictionary<BuildingStyle, RectTransform> styleInstance = new Dictionary<BuildingStyle, RectTransform>();
        private RectTransform styleParent;
        private RectTransform templateStyle;

        private Image keyComFillImage;

        private void Awake()
        {
            if (TStyleDict == null)
            {
                StartCoroutine(InitStyleTexture2D());
            }

            this.styleParent = this.transform.Find("KT_AlterHeight").Find("KT_AlterStyle").Find("Content") as RectTransform;
            this.templateStyle = this.styleParent.Find("StyleTemplate") as RectTransform;
            this.templateStyle.gameObject.SetActive(false);


            this.keyComFillImage = this.transform.Find("KT_KeyCom").Find("Image").Find("T_KeyComTipFill").GetComponent<Image>();
        }

        private IEnumerator InitStyleTexture2D()
        {
#if UNITY_EDITOR
            float startT = Time.time;
#endif
            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }

            TStyleDict = new Dictionary<BuildingStyle, Texture2D>();

            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TStyleABPath, null, out ab);
            yield return crequest;

            var request = ab.LoadAllAssetsAsync<Texture2D>();
            yield return request;

            foreach (var obj in request.allAssets)
            {
                var tex = (obj as Texture2D);
                if (tex != null && Enum.IsDefined(typeof(BuildingStyle), tex.name))
                {
                    TStyleDict.Add((BuildingStyle)Enum.Parse(typeof(BuildingStyle), tex.name), tex);
                }
            }

            abmgr.UnLoadLocalABAsync(TStyleABPath, false, null);
            IsInit = true;
#if UNITY_EDITOR
            Debug.Log("InitStyleTexture2D cost time: " + (Time.time - startT));
#endif
        }

        public IEnumerator Init(BuildingStyle[] styles, short[] heights, int sIndex, int hIndex)
        {
            while (!IsInit)
            {
                yield return null;
            }

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

            StartCoroutine(this.ShowStyleAndHeight(s, h));
        }


        private IEnumerator ShowStyleAndHeight(BuildingStyle style, short height)
        {
            Debug.Log(style + " " + height);

            // ¸ü»» Style
            foreach (var instance in this.styleInstance)
            {
                var img = instance.Value.GetComponentInChildren<Image>();
                if (instance.Key != style)
                {
                    img.color = Color.white;
                    img.color = new Color(145f / 255, 145f / 255, 145f / 255, 145f / 255);
                }
                else
                {
                    img.color = Color.white;
                }
            }

            // ¸ü»» Height

            yield break;
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

        public override void OnEnter()
        {
            base.OnEnter();
            BM.Placer.OnKeyComInProgress += Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel += Placer_OnKeyComCancel;
            BM.Placer.OnPlaceModeChangeStyle += Placer_OnPlaceModeChangeStyle;
            BM.Placer.OnPlaceModeChangeHeight += Placer_OnPlaceModeChangeHeight;
        }

        public override void OnPause()
        {
            base.OnPause();
            BM.Placer.OnKeyComInProgress -= Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel -= Placer_OnKeyComCancel;
            BM.Placer.OnPlaceModeChangeStyle -= Placer_OnPlaceModeChangeStyle;
            BM.Placer.OnPlaceModeChangeHeight -= Placer_OnPlaceModeChangeHeight;
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            BM.Placer.OnKeyComInProgress += Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel += Placer_OnKeyComCancel;
            this.keyComFillImage.fillAmount = 0;
            BM.Placer.OnPlaceModeChangeStyle += Placer_OnPlaceModeChangeStyle;
            BM.Placer.OnPlaceModeChangeHeight += Placer_OnPlaceModeChangeHeight;
        }

        public override void OnExit()
        {
            this.ClearInstance();
            BM.Placer.OnKeyComInProgress -= Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel -= Placer_OnKeyComCancel;

            BM.Placer.OnPlaceModeChangeStyle -= Placer_OnPlaceModeChangeStyle;
            BM.Placer.OnPlaceModeChangeHeight -= Placer_OnPlaceModeChangeHeight;

            Destroy(this.gameObject);
        }

        private void Placer_OnKeyComCancel(float cur, float total)
        {
            this.keyComFillImage.fillAmount = 0;
        }

        private void Placer_OnKeyComInProgress(float cur, float total)
        {
            this.keyComFillImage.fillAmount = cur / total;
        }

        private void Placer_OnPlaceModeChangeHeight(IBuildingPart bpart, bool arg2)
        {
            var styles = BM.GetAllStyleByBPartHeight(bpart);
            var heights = BM.GetAllHeightByBPartStyle(bpart);
            StartCoroutine(this.Init(styles, heights, Array.IndexOf(styles, bpart.Classification.Style), Array.IndexOf(heights, bpart.Classification.Height)));
        }

        private void Placer_OnPlaceModeChangeStyle(IBuildingPart bpart, bool arg2)
        {
            var styles = BM.GetAllStyleByBPartHeight(bpart);
            var heights = BM.GetAllHeightByBPartStyle(bpart);
            StartCoroutine(this.Init(styles, heights, Array.IndexOf(styles, bpart.Classification.Style), Array.IndexOf(heights, bpart.Classification.Height)));
        }
    }
}

