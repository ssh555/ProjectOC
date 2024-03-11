using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.TextContent;
namespace ML.Engine.UI
{

    public class UIKeyTipComponent : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI keytip;
        public TMPro.TextMeshProUGUI description;
        public string InputActionName;

        private void Awake()
        {
            var Image = this.transform.Find("Image");
            if (Image != null)
            {
                var KeyText = Image.Find("KeyText");
                var KeyTipText = Image.Find("KeyTipText");
                if (KeyText != null) { keytip = KeyText.GetComponent<TMPro.TextMeshProUGUI>(); }
                if (KeyTipText != null) { description = KeyTipText.GetComponent<TMPro.TextMeshProUGUI>(); }
            }
            InputActionName = this.gameObject.name;
            this.enabled = false;
        }

    }

}


