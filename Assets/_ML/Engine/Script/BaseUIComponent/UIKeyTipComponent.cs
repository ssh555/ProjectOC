using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.TextContent;
using UnityEngine.EventSystems;
namespace ML.Engine.UI
{
    public class UIKeyTipComponent : UIBehaviour
    {
        public TMPro.TextMeshProUGUI keytip;
        public TMPro.TextMeshProUGUI description;
        public string InputActionName;

        public void Init()
        {
            var Image = this.transform.Find("Image");
            if (Image != null)
            {
                var KeyText = Image.Find("KeyText");
                var KeyTipText = Image.Find("KeyTipText");
                keytip = null;
                description = null;
                if (KeyText != null) { keytip = KeyText.GetComponent<TMPro.TextMeshProUGUI>(); }
                if (KeyTipText != null) { description = KeyTipText.GetComponent<TMPro.TextMeshProUGUI>(); }
            }
            InputActionName = this.gameObject.name;
            this.enabled = false;
        }


    }

}


