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

            Transform keytips = this.transform.Find("KeyTip");

            place = new UIKeyTip();
            place.root = keytips.Find("KT_Place") as RectTransform;
            place.img = place.root.Find("Image").GetComponent<Image>();
            place.keytip = place.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            place.description = place.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            place.ReWrite(Test_BuildingManager.Instance.KeyTipDict["place"]);

            altersocket = new UIKeyTip();
            altersocket.root = keytips.Find("KT_AlterSocket") as RectTransform;
            altersocket.img = altersocket.root.Find("Image").GetComponent<Image>();
            altersocket.keytip = altersocket.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            altersocket.description = altersocket.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            altersocket.ReWrite(Test_BuildingManager.Instance.KeyTipDict["altersocket"]);

            altermat = new UIKeyTip();
            altermat.root = keytips.Find("KT_AlterMat") as RectTransform;
            altermat.img = altermat.root.Find("Image").GetComponent<Image>();
            altermat.keytip = altermat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            altermat.description = altermat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            altermat.ReWrite(Test_BuildingManager.Instance.KeyTipDict["altermat"]);

            rotateright = new UIKeyTip();
            rotateright.root = keytips.Find("KT_Rotate") as RectTransform;
            rotateright.img = rotateright.root.Find("Right").GetComponent<Image>();
            rotateright.keytip = rotateright.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            rotateright.description = rotateright.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            rotateright.ReWrite(Test_BuildingManager.Instance.KeyTipDict["rotateright"]);

            rotateleft = new UIKeyTip();
            rotateleft.root = keytips.Find("KT_Rotate") as RectTransform;
            rotateleft.img = rotateleft.root.Find("Left").GetComponent<Image>();
            rotateleft.keytip = rotateleft.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            rotateleft.description = rotateright.description;
            rotateleft.ReWrite(Test_BuildingManager.Instance.KeyTipDict["rotateleft"]);

            back = new UIKeyTip();
            back.root = keytips.Find("KT_Back") as RectTransform;
            back.img = back.root.Find("Image").GetComponent<Image>();
            back.keytip = back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            back.description = back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            back.ReWrite(Test_BuildingManager.Instance.KeyTipDict["back"]);

            keycom = new UIKeyTip();
            keycom.root = this.transform.Find("KT_KeyCom") as RectTransform;
            keycom.img = keycom.root.Find("Image").GetComponent<Image>();
            keycom.keytip = keycom.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            keycom.description = keycom.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            keycom.ReWrite(Test_BuildingManager.Instance.KeyTipDict["keycom"]);



            keytips = this.transform.Find("KT_AlterHeight");
            heightlast = new UIKeyTip();
            heightlast.root = keytips.Find("KT_Down") as RectTransform;
            heightlast.img = heightlast.root.Find("Image").GetComponent<Image>();
            heightlast.keytip = heightlast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            heightlast.ReWrite(Test_BuildingManager.Instance.KeyTipDict["heightlast"]);

            heightnext = new UIKeyTip();
            heightnext.root = keytips.Find("KT_Up") as RectTransform;
            heightnext.img = heightnext.root.Find("Image").GetComponent<Image>();
            heightnext.keytip = heightnext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            heightnext.ReWrite(Test_BuildingManager.Instance.KeyTipDict["heightnext"]);

            keytips = keytips.Find("KT_AlterStyle");
            stylelast = new UIKeyTip();
            stylelast.root = keytips.Find("KT_Left") as RectTransform;
            stylelast.img = stylelast.root.Find("Image").GetComponent<Image>();
            stylelast.keytip = stylelast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            stylelast.ReWrite(Test_BuildingManager.Instance.KeyTipDict["stylelast"]);

            stylenext = new UIKeyTip();
            stylenext.root = keytips.Find("KT_Right") as RectTransform;
            stylenext.img = stylenext.root.Find("Image").GetComponent<Image>();
            stylenext.keytip = stylenext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            stylenext.ReWrite(Test_BuildingManager.Instance.KeyTipDict["stylenext"]);
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

