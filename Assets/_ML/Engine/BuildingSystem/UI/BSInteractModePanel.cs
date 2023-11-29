using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSInteractModePanel : Engine.UI.UIBasePanel
    {
        private BuildingManager BM => BuildingManager.Instance;

        private Image keyComFillImage;

        private void Awake()
        {
            this.keyComFillImage = this.transform.Find("BPartInteractTip").Find("KT_KeyCom").Find("Image").Find("T_KeyComTipFill").GetComponent<Image>();
        }


        private void Placer_OnKeyComCancel(float cur, float total)
        {
            this.keyComFillImage.fillAmount = 0;
        }

        private void Placer_OnKeyComInProgress(float cur, float total)
        {
            this.keyComFillImage.fillAmount = cur / total;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            BM.Placer.OnKeyComInProgress += Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel += Placer_OnKeyComCancel;
        }


        public override void OnPause()
        {
            base.OnPause();
            BM.Placer.OnKeyComInProgress -= Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel -= Placer_OnKeyComCancel;
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            BM.Placer.OnKeyComInProgress += Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel += Placer_OnKeyComCancel;
            this.keyComFillImage.fillAmount = 0;
        }

        public override void OnExit()
        {
            Destroy(this.gameObject);
            BM.Placer.OnKeyComInProgress -= Placer_OnKeyComInProgress;
            BM.Placer.OnKeyComCancel -= Placer_OnKeyComCancel;
        }
    }
}

