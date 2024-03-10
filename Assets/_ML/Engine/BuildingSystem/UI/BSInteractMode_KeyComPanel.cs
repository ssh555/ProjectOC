using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSInteractMode_KeyComPanel : Engine.UI.UIBasePanel
    {
        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;

        #region UIGO引用
        private UIKeyTip keycom;
        private UIKeyTip copymat;
        private UIKeyTip pastemat;
        private UIKeyTip copybuild;
        #endregion

        #endregion

        #region Unity
        private void Awake()
        {
            this.enabled = false;
            Transform keycoms = this.transform.Find("KeyCom");

            keycom = new UIKeyTip();
            keycom.root = keycoms.Find("KT_KeyCom") as RectTransform;
            keycom.img = keycom.root.Find("Image").GetComponent<Image>();
            keycom.keytip = keycom.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            keycom.ReWrite(MonoBuildingManager.Instance.KeyTipDict["keycom"]);

            copymat = new UIKeyTip();
            copymat.root = keycoms.Find("KT_CopyMat") as RectTransform;
            copymat.img = copymat.root.Find("Image").GetComponent<Image>();
            copymat.keytip = copymat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            copymat.description = copymat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            copymat.ReWrite(MonoBuildingManager.Instance.KeyTipDict["copymat"]);

            pastemat = new UIKeyTip();
            pastemat.root = keycoms.Find("KT_PasteMat") as RectTransform;
            pastemat.img = pastemat.root.Find("Image").GetComponent<Image>();
            pastemat.keytip = pastemat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            pastemat.description = pastemat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            pastemat.ReWrite(MonoBuildingManager.Instance.KeyTipDict["pastemat"]);

            copybuild = new UIKeyTip();
            copybuild.root = keycoms.Find("KT_CopyBuild") as RectTransform;
            copybuild.img = copybuild.root.Find("Image").GetComponent<Image>();
            copybuild.keytip = copybuild.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            copybuild.description = copybuild.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            copybuild.ReWrite(MonoBuildingManager.Instance.KeyTipDict["copybuild"]);
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

        public override void Refresh()
        {
            keycom.ReWrite(MonoBuildingManager.Instance.KeyTipDict["keycom"]);
            copymat.ReWrite(MonoBuildingManager.Instance.KeyTipDict["copymat"]);
            pastemat.ReWrite(MonoBuildingManager.Instance.KeyTipDict["pastemat"]);
            copybuild.ReWrite(MonoBuildingManager.Instance.KeyTipDict["copybuild"]);
        }

        #endregion

        #region KeyFunction
        private void UnregisterInput()
        {
            this.Placer.BInput.BuildKeyCom.Disable();

            this.Placer.BInput.BuildKeyCom.CopyBuild.performed -= CopyBuild_perfomed;
            this.Placer.BInput.BuildKeyCom.CopyOutLook.performed -= CopyOutLook_performed;
            this.Placer.BInput.BuildKeyCom.PasteOutLook.performed -= PasteOutLook_performed;
            this.Placer.BInput.BuildKeyCom.KeyCom.canceled -= KeyCom_canceled;
        }

        private void RegisterInput()
        {
            this.Placer.BInput.BuildKeyCom.Enable();

            this.Placer.BInput.BuildKeyCom.CopyBuild.performed += CopyBuild_perfomed;
            this.Placer.BInput.BuildKeyCom.CopyOutLook.performed += CopyOutLook_performed;
            this.Placer.BInput.BuildKeyCom.PasteOutLook.performed += PasteOutLook_performed;
            this.Placer.BInput.BuildKeyCom.KeyCom.canceled += KeyCom_canceled;
        }

        private void KeyCom_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            MonoBuildingManager.Instance.PopPanel();
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

        private void CopyBuild_perfomed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var bpart = BuildingManager.Instance.GetOneBPartCopyInstance(this.Placer.SelectedPartInstance);
            if (bpart == this.Placer.SelectedPartInstance)
            {
                MonoBuildingManager.Instance.PopAndPushPanel<BSPlaceModePanel>();
            }
        }
        #endregion
    }

}
