using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSAppearancePanel : Engine.UI.UIBasePanel
    {
        private BuildingManager BM => BuildingManager.Instance;

        private UnityEngine.UI.Image[] matInstance;
        private int activeIndex;
        private RectTransform matParent;
        private RectTransform templateMat;

        private UIKeyTip comfirm;
        private UIKeyTip back;
        private UIKeyTip matlast;
        private UIKeyTip matnext;

        private void Awake()
        {
            matParent = this.transform.Find("KT_AlterMat").Find("KT_AlterStyle").Find("Content") as RectTransform;
            this.templateMat = matParent.Find("MatTemplate") as RectTransform;
            templateMat.gameObject.SetActive(false);

            Transform keytips = this.transform.Find("KeyTip");

            comfirm = new UIKeyTip();
            comfirm.root = keytips.Find("KT_Comfirm") as RectTransform;
            comfirm.img = comfirm.root.Find("Image").GetComponent<Image>();
            comfirm.keytip = comfirm.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            comfirm.description = comfirm.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            comfirm.ReWrite(Test_BuildingManager.Instance.KeyTipDict["comfirm"]);

            back = new UIKeyTip();
            back.root = keytips.Find("KT_Back") as RectTransform;
            back.img = back.root.Find("Image").GetComponent<Image>();
            back.keytip = back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            back.description = back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            back.ReWrite(Test_BuildingManager.Instance.KeyTipDict["back"]);

            keytips = this.transform.Find("KT_AlterMat").Find("KT_AlterStyle");
            matlast = new UIKeyTip();
            matlast.root = keytips.Find("KT_Left") as RectTransform;
            matlast.img = matlast.root.Find("Image").GetComponent<Image>();
            matlast.keytip = matlast.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            matlast.ReWrite(Test_BuildingManager.Instance.KeyTipDict["matlast"]);

            matnext = new UIKeyTip();
            matnext.root = keytips.Find("KT_Right") as RectTransform;
            matnext.img = matnext.root.Find("Image").GetComponent<Image>();
            matnext.keytip = matnext.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            matnext.ReWrite(Test_BuildingManager.Instance.KeyTipDict["matnext"]);
        }

        public IEnumerator Init(Texture2D[] texs, BuildingCopiedMaterial[] mats, int index)
        {
            ClearInstance();

            matInstance = new UnityEngine.UI.Image[texs.Length];
            for(int i = 0; i < texs.Length; ++i)
            {
                Sprite sprite = Sprite.Create(texs[i], new Rect(0, 0, texs[i].width, texs[i].height), new Vector2(0.5f, 0.5f));
                var img = Instantiate<GameObject>(templateMat.gameObject, matParent, false).GetComponent<UnityEngine.UI.Image>();
                img.sprite = sprite;

                Disactive(img);

                img.gameObject.SetActive(true);
                matInstance[i] = img;
            }

            activeIndex = index;

            StartCoroutine(Show(texs, mats, index));

            yield break;
        }

        private IEnumerator Show(Texture2D[] texs, BuildingCopiedMaterial[] mats, int index)
        {
            Disactive(this.matInstance[activeIndex]);
            activeIndex = index;
            Active(this.matInstance[activeIndex]);
            yield break;
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
            if(matInstance != null)
            {
                foreach (var img in matInstance)
                {
                    Destroy(img.sprite);
                    Destroy(img.gameObject);
                }
            }

        }

        public override void OnEnter()
        {
            base.OnEnter();
            BM.Placer.OnChangeAppearance += Placer_OnChangeAppearance;
        }



        public override void OnPause()
        {
            base.OnPause();
            BM.Placer.OnChangeAppearance -= Placer_OnChangeAppearance;

        }


        public override void OnRecovery()
        {
            base.OnRecovery();

            BM.Placer.OnChangeAppearance += Placer_OnChangeAppearance;
        }

        public override void OnExit()
        {
            BM.Placer.OnChangeAppearance -= Placer_OnChangeAppearance;

            ClearInstance();

            Destroy(this.gameObject);
        }

        private void Placer_OnChangeAppearance(Texture2D[] texs, BuildingCopiedMaterial[] mats, int index)
        {
            StartCoroutine(Show(texs, mats, index));
        }
    }

}
