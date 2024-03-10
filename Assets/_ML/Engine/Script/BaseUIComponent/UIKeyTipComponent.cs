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

            var Image = this.transform.Find("Image");
            if (Image != null)
            {
                var KeyText = Image.Find("KeyText");
                var KeyTipText = Image.Find("KeyTipText");
                if (KeyText != null) { uiKeyTip.keytip = KeyText.GetComponent<TMPro.TextMeshProUGUI>(); }
                if (KeyTipText != null) { uiKeyTip.description = KeyTipText.GetComponent<TMPro.TextMeshProUGUI>(); }
            }
            
            InputActionName = this.gameObject.name;
        }


    }

}


