using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSAppearancePanel : Engine.UI.UIBasePanel
    {
        private UIKeyTip comfirm;
        private UIKeyTip back;


        private void Awake()
        {
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

        }

        public override void OnExit()
        {
            Destroy(this.gameObject);
        }
    }

}
