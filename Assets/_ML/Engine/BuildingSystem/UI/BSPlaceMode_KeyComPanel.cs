using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace ML.Engine.BuildingSystem.UI
{
    /// <summary>
    /// 放置的三级的次级交互轮
    /// </summary>
    public class BSPlaceMode_KeyComPanel : Engine.UI.UIBasePanel
    {
        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;
        private MonoBuildingManager monoBM;
        #region UIGO引用
        private UIKeyTip keycom;
        private UIKeyTip copymat;
        private UIKeyTip pastemat;
        private UIKeyTip switchframe;
        #endregion
        #endregion

        #region Unity
        protected override void Awake()
        {
            this.enabled = false;
            Transform keycoms = this.transform.Find("KeyCom");
            monoBM = ML.Engine.Manager.GameManager.Instance.GetLocalManager<MonoBuildingManager>();
            keycom = new UIKeyTip();
            keycom.root = keycoms.Find("KT_KeyCom") as RectTransform;
            keycom.img = keycom.root.Find("Image").GetComponent<Image>();
            keycom.keytip = keycom.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            keycom.ReWrite(monoBM.KeyTipDict["keycom"]);

            copymat = new UIKeyTip();
            copymat.root = keycoms.Find("KT_CopyMat") as RectTransform;
            copymat.img = copymat.root.Find("Image").GetComponent<Image>();
            copymat.keytip = copymat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            copymat.description = copymat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            copymat.ReWrite(monoBM.KeyTipDict["copymat"]);

            pastemat = new UIKeyTip();
            pastemat.root = keycoms.Find("KT_PasteMat") as RectTransform;
            pastemat.img = pastemat.root.Find("Image").GetComponent<Image>();
            pastemat.keytip = pastemat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            pastemat.description = pastemat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            pastemat.ReWrite(monoBM.KeyTipDict["pastemat"]);

            switchframe = new UIKeyTip();
            switchframe.root = keycoms.Find("KT_SwitchFrame") as RectTransform;
            switchframe.img = switchframe.root.Find("Image").GetComponent<Image>();
            switchframe.keytip = switchframe.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            switchframe.description = switchframe.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            switchframe.ReWrite(monoBM.KeyTipDict["switchframe"]);
        }
        #endregion

        #region Override
        public override void OnExit()
        {
            base.OnExit();
            this.UnregisterInput();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.RegisterInput();
        }

        public override void OnPause()
        {
            base.OnPause();
            this.UnregisterInput();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.RegisterInput();
        }
        #endregion

        #region KeyFunction
        private void UnregisterInput()
        {
            this.Placer.BInput.BuildKeyCom.Disable();

            this.Placer.BInput.BuildKeyCom.CopyBuild.performed -= CopyBuild_GridSupport;
            this.Placer.BInput.BuildKeyCom.CopyOutLook.performed -= CopyOutLook_performed;
            this.Placer.BInput.BuildKeyCom.PasteOutLook.performed -= PasteOutLook_performed;
            this.Placer.BInput.BuildKeyCom.KeyCom.canceled -= KeyCom_canceled;
        }

        private void RegisterInput()
        {
            this.Placer.BInput.BuildKeyCom.Enable();

            this.Placer.BInput.BuildKeyCom.CopyBuild.performed += CopyBuild_GridSupport;
            this.Placer.BInput.BuildKeyCom.CopyOutLook.performed += CopyOutLook_performed;
            this.Placer.BInput.BuildKeyCom.PasteOutLook.performed += PasteOutLook_performed;
            this.Placer.BInput.BuildKeyCom.KeyCom.canceled += KeyCom_canceled;
        }

        private void KeyCom_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            monoBM.PopPanel();
        }

        private void PasteOutLook_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            // 拷贝复制的材质到当前选中的可交互建筑物
            BuildingManager.Instance.PasteBPartMaterial(this.Placer.SelectedPartInstance);
        }

        private void CopyOutLook_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            // 复制当前选中的可交互建筑物的材质
            BuildingManager.Instance.CopyBPartMaterial(this.Placer.SelectedPartInstance);
        }

        private void CopyBuild_GridSupport(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            // 辅助网格
            this.Placer.IsEnableGridSupport = !this.Placer.IsEnableGridSupport;
        }
        #endregion

        #region Refresh
        public override void Refresh()
        {
            keycom.ReWrite(monoBM.KeyTipDict["keycom"]);
            copymat.ReWrite(monoBM.KeyTipDict["copymat"]);
            pastemat.ReWrite(monoBM.KeyTipDict["pastemat"]);
            switchframe.ReWrite(monoBM.KeyTipDict["switchframe"]);
        }
        #endregion
    }
}

