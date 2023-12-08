using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSEditModePanel : Engine.UI.UIBasePanel
    {
        private BuildingManager BM => BuildingManager.Instance;

        private Image keyComFillImage;

        #region KeyTip
        private UIKeyTip place;
        private UIKeyTip altersocket;
        private UIKeyTip rotateleft;
        private UIKeyTip rotateright;
        private UIKeyTip back;
        private UIKeyTip keycom;

        #endregion
        private void Awake()
        {
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

            this.keyComFillImage = this.transform.Find("KT_KeyCom").Find("Image").Find("T_KeyComTipFill").GetComponent<Image>();
        }


        private void Placer_OnKeyComCancel(float cur, float total)
        {
            this.keyComFillImage.fillAmount = 0;
        }

        private void Placer_OnKeyComInProgress(float cur, float total)
        {
            this.keyComFillImage.fillAmount = cur / total;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            BM.Placer.OnKeyComInProgress += Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel += Placer_OnKeyComCancel;
        }


        public override void OnPause()
        {
            base.OnPause();
            BM.Placer.OnKeyComInProgress -= Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel -= Placer_OnKeyComCancel;
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            BM.Placer.OnKeyComInProgress += Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel += Placer_OnKeyComCancel;
            this.keyComFillImage.fillAmount = 0;
        }



        public override void OnExit()
        {
            Destroy(this.gameObject);
            BM.Placer.OnKeyComInProgress -= Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel -= Placer_OnKeyComCancel;
        }
    }
}

