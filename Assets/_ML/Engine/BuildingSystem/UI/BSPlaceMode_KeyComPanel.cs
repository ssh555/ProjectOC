using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.BuildingSystem.UI.BSPlaceMode_KeyComPanel;

namespace ML.Engine.BuildingSystem.UI
{
    /// <summary>
    /// 放置的三级的次级交互轮
    /// </summary>
    public class BSPlaceMode_KeyComPanel : Engine.UI.UIBasePanel<BSPlaceMode_KeyComPanelStruct>
    {
        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;
        private MonoBuildingManager monoBM;
        #endregion

        #region Unity
        protected override void Awake()
        {
            this.enabled = false;
            monoBM = ML.Engine.Manager.GameManager.Instance.GetLocalManager<MonoBuildingManager>();
        }
        #endregion

        #region Override
        #endregion

        #region KeyFunction
        protected override void UnregisterInput()
        {
            this.Placer.BInput.BuildKeyCom.Disable();

            this.Placer.BInput.BuildKeyCom.CopyBuild.performed -= CopyBuild_GridSupport;
            this.Placer.BInput.BuildKeyCom.CopyOutLook.performed -= CopyOutLook_performed;
            this.Placer.BInput.BuildKeyCom.PasteOutLook.performed -= PasteOutLook_performed;
            this.Placer.BInput.BuildKeyCom.KeyCom.canceled -= KeyCom_canceled;
        }

        protected override void RegisterInput()
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
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct BSPlaceMode_KeyComPanelStruct
        {
            public KeyTip[] KeyTips;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/BuildingSystem/UI";
            this.abname = "BSPlaceMode_KeyComPanel";
            this.description = "BSPlaceMode_KeyComPanel数据加载完成";
        }
        #endregion
    }
}

