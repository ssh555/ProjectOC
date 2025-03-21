﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.TextContent
{
    public struct UIKeyTip
    {
        public RectTransform root;
        public Image img;
        public TextMeshProUGUI keytip;
        public TextMeshProUGUI description;

        public void ReWrite(KeyTip keyTip)
        {
            if (this.keytip)
            {
                this.keytip.text = keyTip.GetKeyMapText();
            }

            if (this.description)
            {
                this.description.text = keyTip.GetDescription();
            }
                
        }
    }
}
