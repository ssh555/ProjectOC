using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Example.InventorySystem.MainHUD.UI
{
    public class UIClassificationContainer : MonoBehaviour
    {
        public UIClassSwitchBtn inventoryBtn;
        public UIClassSwitchBtn compositeBtn;

        private void Awake()
        {
            this.inventoryBtn = this.transform.GetChild(0).GetChild(0).GetComponent<UIClassSwitchBtn>();
            this.compositeBtn = this.transform.GetChild(0).GetChild(1).GetComponent<UIClassSwitchBtn>();
        }
    }

}
