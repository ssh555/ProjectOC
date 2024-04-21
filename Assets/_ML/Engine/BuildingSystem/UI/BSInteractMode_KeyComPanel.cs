using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.BuildingSystem.UI.BSInteractMode_KeyComPanel;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSInteractMode_KeyComPanel : Engine.UI.UIBasePanel<BSInteractMode_KeyComPanelStruct>
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
        public override void Refresh()
        {
        }

        #endregion

        #region KeyFunction
        protected override void UnregisterInput()
        {
            this.Placer.BInput.BuildKeyCom.Disable();

            this.Placer.BInput.BuildKeyCom.CopyBuild.performed -= CopyBuild_perfomed;
            this.Placer.BInput.BuildKeyCom.CopyOutLook.performed -= CopyOutLook_performed;
            this.Placer.BInput.BuildKeyCom.PasteOutLook.performed -= PasteOutLook_performed;
            this.Placer.BInput.BuildKeyCom.KeyCom.canceled -= KeyCom_canceled;
        }

        protected override void RegisterInput()
        {
            this.Placer.BInput.BuildKeyCom.Enable();

            this.Placer.BInput.BuildKeyCom.CopyBuild.performed += CopyBuild_perfomed;
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
            // �������ƵĲ��ʵ���ǰѡ�еĿɽ���������
            BuildingManager.Instance.PasteBPartMaterial(this.Placer.SelectedPartInstance);
        }

        private void CopyOutLook_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            // ���Ƶ�ǰѡ�еĿɽ���������Ĳ���
            BuildingManager.Instance.CopyBPartMaterial(this.Placer.SelectedPartInstance);
        }

        private void CopyBuild_perfomed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var bpart = BuildingManager.Instance.GetOneBPartCopyInstance(this.Placer.SelectedPartInstance);
            if (bpart == this.Placer.SelectedPartInstance)
            {
                monoBM.PopAndPushPanel<BSPlaceModePanel>();
            }
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct BSInteractMode_KeyComPanelStruct
        {
            public KeyTip[] KeyTips;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/BuildingSystem/UI";
            this.abname = "BSInteractMode_KeyComPanel";
            this.description = "BSInteractMode_KeyComPanel���ݼ������";
        }
        #endregion
    }

}
