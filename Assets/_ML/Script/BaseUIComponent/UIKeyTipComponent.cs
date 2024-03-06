using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.TextContent;
namespace ML.Engine.UI
{

    public class UIKeyTipComponent : MonoBehaviour
    {
        public UIKeyTip uiKeyTip;
        public string InputActionName;

        public void InitData()
        {
            uiKeyTip = new UIKeyTip();
            uiKeyTip.keytip = this.transform.Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            uiKeyTip.description = this.transform.Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();
            InputActionName = this.gameObject.name;
        }


    }

}


