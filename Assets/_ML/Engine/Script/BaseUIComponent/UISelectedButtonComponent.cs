using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace ML.Engine.UI
{
    public class UISelectedButtonComponent
    {
        public SelectedButton selectedButton;
        public TextMeshProUGUI textMeshProUGUI;
        public UISelectedButtonComponent(Transform parent, string BtnName)
        {
            if (parent != null)
            {
                this.selectedButton = parent.Find(BtnName).GetComponent<SelectedButton>();
            }
            if (this.selectedButton != null)
            {
                this.textMeshProUGUI = this.selectedButton.transform.Find("BtnText").GetComponent<TextMeshProUGUI>();
            }
        }
    }
}


