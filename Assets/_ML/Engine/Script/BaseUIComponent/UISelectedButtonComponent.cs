using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace ML.Engine.UI
{
    public class UISelectedButtonComponent
    {
        public SelectedButton selectedButton;
        public TextMeshProUGUI textMeshProUGUI;
        public UISelectedButtonComponent(Transform parent,string btnName)
        {
            if (parent != null)
            {
                var btn = parent.Find(btnName);
                this.selectedButton = btn.GetComponent<SelectedButton>();
                this.textMeshProUGUI = btn.Find("BtnText").GetComponent<TextMeshProUGUI>();
            }
        }


    }


}


