using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSInteractModePanel : Engine.UI.UIBasePanel
    {
        private BuildingManager BM => BuildingManager.Instance;

        private Image keyComFillImage;

        #region KeyTip
        private UIKeyTip keycom;
        private UIKeyTip movebuild;
        private UIKeyTip destroybuild;
        private UIKeyTip altermat;
        private UIKeyTip alterbpart;
        private UIKeyTip selectbpart;
        private UIKeyTip back;

        #endregion

        private void Awake()
        {
            Transform keytip = this.transform.Find("BPartInteractTip");

            keycom = new UIKeyTip();
            keycom.root = keytip.Find("KT_KeyCom") as RectTransform;
            keycom.img = keycom.root.Find("Image").GetComponent<Image>();
            keycom.keytip = keycom.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            keycom.description = keycom.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            keycom.ReWrite(Test_BuildingManager.Instance.KeyTipDict["keycom"]);

            movebuild = new UIKeyTip();
            movebuild.root = keytip.Find("KT_MoveBuild") as RectTransform;
            movebuild.img = movebuild.root.Find("Image").GetComponent<Image>();
            movebuild.keytip = movebuild.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            movebuild.description = movebuild.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            movebuild.ReWrite(Test_BuildingManager.Instance.KeyTipDict["movebuild"]);

            destroybuild = new UIKeyTip();
            destroybuild.root = keytip.Find("KT_DestroyBuild") as RectTransform;
            destroybuild.img = destroybuild.root.Find("Image").GetComponent<Image>();
            destroybuild.keytip = destroybuild.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            destroybuild.description = destroybuild.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            destroybuild.ReWrite(Test_BuildingManager.Instance.KeyTipDict["destroybuild"]);

            altermat = new UIKeyTip();
            altermat.root = keytip.Find("KT_AlterMat") as RectTransform;
            altermat.img = altermat.root.Find("Image").GetComponent<Image>();
            altermat.keytip = altermat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            altermat.description = altermat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            altermat.ReWrite(Test_BuildingManager.Instance.KeyTipDict["altermat"]);

            keytip = this.transform.Find("InteractTip");
            alterbpart = new UIKeyTip();
            alterbpart.root = keytip.Find("KT_AlterBPart") as RectTransform;
            alterbpart.img = alterbpart.root.Find("Image").GetComponent<Image>();
            alterbpart.keytip = alterbpart.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            alterbpart.description = alterbpart.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            alterbpart.ReWrite(Test_BuildingManager.Instance.KeyTipDict["alterbpart"]);

            selectbpart = new UIKeyTip();
            selectbpart.root = keytip.Find("KT_SelectBPart") as RectTransform;
            selectbpart.img = selectbpart.root.Find("Image").GetComponent<Image>();
            selectbpart.keytip = selectbpart.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            selectbpart.description = selectbpart.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            selectbpart.ReWrite(Test_BuildingManager.Instance.KeyTipDict["selectbpart"]);

            back = new UIKeyTip();
            back.root = keytip.Find("KT_Back") as RectTransform;
            back.img = back.root.Find("Image").GetComponent<Image>();
            back.keytip = back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            back.description = back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            back.ReWrite(Test_BuildingManager.Instance.KeyTipDict["back"]);

            this.keyComFillImage = this.transform.Find("BPartInteractTip").Find("KT_KeyCom").Find("Image").Find("T_KeyComTipFill").GetComponent<Image>();
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

