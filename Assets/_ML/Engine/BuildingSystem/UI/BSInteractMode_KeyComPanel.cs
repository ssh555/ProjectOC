using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSInteractMode_KeyComPanel : Engine.UI.UIBasePanel
    {
        private UIKeyTip keycom;
        private UIKeyTip copymat;
        private UIKeyTip pastemat;
        private UIKeyTip copybuild;

        private void Awake()
        {
            Transform keycoms = this.transform.Find("KeyCom");

            keycom = new UIKeyTip();
            keycom.root = keycoms.Find("KT_KeyCom") as RectTransform;
            keycom.img = keycom.root.Find("Image").GetComponent<Image>();
            keycom.keytip = keycom.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            keycom.ReWrite(MonoBuildingManager.Instance.KeyTipDict["keycom"]);

            copymat = new UIKeyTip();
            copymat.root = keycoms.Find("KT_CopyMat") as RectTransform;
            copymat.img = copymat.root.Find("Image").GetComponent<Image>();
            copymat.keytip = copymat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            copymat.description = copymat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            copymat.ReWrite(MonoBuildingManager.Instance.KeyTipDict["copymat"]);

            pastemat = new UIKeyTip();
            pastemat.root = keycoms.Find("KT_PasteMat") as RectTransform;
            pastemat.img = pastemat.root.Find("Image").GetComponent<Image>();
            pastemat.keytip = pastemat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            pastemat.description = pastemat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            pastemat.ReWrite(MonoBuildingManager.Instance.KeyTipDict["pastemat"]);

            copybuild = new UIKeyTip();
            copybuild.root = keycoms.Find("KT_CopyBuild") as RectTransform;
            copybuild.img = copybuild.root.Find("Image").GetComponent<Image>();
            copybuild.keytip = copybuild.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            copybuild.description = copybuild.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            copybuild.ReWrite(MonoBuildingManager.Instance.KeyTipDict["copybuild"]);
        }
        public override void OnExit()
        {
            Destroy(this.gameObject);
        }
    }

}
