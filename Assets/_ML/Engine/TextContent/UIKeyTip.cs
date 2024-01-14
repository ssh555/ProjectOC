using ML.Engine.BuildingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
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
                Debug.Log("1 "+keyTip.GetKeyMapText());
                this.keytip.text = keyTip.GetKeyMapText();
            }
                
            if (this.description)
            {
                Debug.Log("2 " + keyTip.GetDescription());
                this.description.text = keyTip.GetDescription();
            }
                
        }
    }
}
