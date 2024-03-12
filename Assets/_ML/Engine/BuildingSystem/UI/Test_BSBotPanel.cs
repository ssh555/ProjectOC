using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ML.Engine.BuildingSystem.MonoBuildingManager;

namespace ML.Engine.BuildingSystem.UI
{
    public class Test_BSBotPanel : Engine.UI.UIBasePanel
    {
        private UIKeyTip enterbuild;
        private void Awake()
        {
            StartCoroutine(Init());
        }

        public IEnumerator Init()
        {
            yield return new WaitForSeconds(1);
            enterbuild = new UIKeyTip();
            enterbuild.root = this.transform.Find("KeyTip") as RectTransform;
            enterbuild.img = enterbuild.root.Find("Image").GetComponent<Image>();
            enterbuild.keytip = enterbuild.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            enterbuild.description = enterbuild.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            enterbuild.ReWrite(Manager.GameManager.Instance.GetLocalManager<MonoBuildingManager>().KeyTipDict["enterbuild"]);
        }

        public override void OnExit()
        {
            Manager.GameManager.DestroyObj(this.gameObject);
        }
    }
}

