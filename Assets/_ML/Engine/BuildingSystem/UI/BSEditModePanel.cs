using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.BuildingSystem.UI.BSEditModePanel;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSEditModePanel : Engine.UI.UIBasePanel<BSEditModePanelStruct>, Timer.ITickComponent
    {
        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;

        private MonoBuildingManager monoBM;
        #endregion

        #region TickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public virtual void FixedTick(float deltatime)
        {
            // 实时更新落点的位置和旋转以及是否可放置
            this.Placer.TransformSelectedPartInstance();
        }

        #endregion

        #region Unity
        protected override void Awake()
        {
            this.enabled = false;
            monoBM = ML.Engine.Manager.GameManager.Instance.GetLocalManager<MonoBuildingManager>();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();

            // TODO : 放置于UI中调用不是很合理
            this.Placer.SelectedPartInstance.OnEnterEdit();

            // 记录原 Transform
            this._editOldPos = this.Placer.SelectedPartInstance.transform.position;
            this._editOldRotation = this.Placer.SelectedPartInstance.transform.rotation;
        }
        public override void OnExit()
        {
            base.OnExit();
            this.Placer.SelectedPartInstance = null;
        }
        #endregion

        #region Refresh
        public override void Refresh()
        {
        }
        #endregion

        #region KeyFunction
        protected override void UnregisterInput()
        {
            Manager.GameManager.Instance.TickManager.UnregisterFixedTick(this);

            this.Placer.DisablePlayerInput();

            this.Placer.BInput.BuildPlaceMode.Disable();
            // 禁用 Input.Edit
            this.Placer.BInput.BuildPlaceMode.Disable();

            this.Placer.BInput.BuildPlaceMode.KeyCom.performed -= Placer_EnterKeyCom;
            this.Placer.backInputAction.performed -= Placer_CancelEdit;
            this.Placer.BInput.BuildPlaceMode.Rotate.performed -= Placer_RotateBPart;
            this.Placer.BInput.BuildPlaceMode.ChangeActiveSocket.performed -= Placer_ChangeActiveSocket;
            this.Placer.comfirmInputAction.performed -= Placer_ComfirmEdit;
        }

        protected override void RegisterInput()
        {
            Manager.GameManager.Instance.TickManager.RegisterFixedTick(0, this);

            this.Placer.EnablePlayerInput();

            this.Placer.Mode = BuildingMode.Edit;
            // 启用 Input.Edit
            this.Placer.BInput.BuildPlaceMode.Enable();
            this.Placer.BInput.BuildPlaceMode.ChangeOutLook.Disable();
            this.Placer.BInput.BuildPlaceMode.ChangeStyle.Disable();
            this.Placer.BInput.BuildPlaceMode.ChangeHeight.Disable();

            this.Placer.BInput.BuildPlaceMode.KeyCom.performed += Placer_EnterKeyCom;
            this.Placer.backInputAction.performed += Placer_CancelEdit;
            this.Placer.BInput.BuildPlaceMode.Rotate.performed += Placer_RotateBPart;
            this.Placer.BInput.BuildPlaceMode.ChangeActiveSocket.performed += Placer_ChangeActiveSocket;
            this.Placer.comfirmInputAction.performed += Placer_ComfirmEdit;
        }

        private void Placer_ComfirmEdit(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(this.Placer.ComfirmEditBPart(this._editOldPos))
            {
                monoBM.PopPanel();
            }
        }

        private void Placer_ChangeActiveSocket(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.SelectedPartInstance.AlternativeActiveSocket();
        }

        private void Placer_RotateBPart(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            int offset = obj.ReadValue<float>() > 0 ? 1 : -1;

            if (this.Placer.IsEnableGridSupport)
            {
                this.Placer.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.Placer.EnableGridRotRate * offset, this.Placer.SelectedPartInstance.transform.up);
            }
            else
            {
                this.Placer.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.Placer.DisableGridRotRate * Time.deltaTime * offset, this.Placer.SelectedPartInstance.transform.up);
            }
        }


        private void Placer_CancelEdit(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Placer.SelectedPartInstance.transform.position = this._editOldPos;
            this.Placer.SelectedPartInstance.transform.rotation = this._editOldRotation;
            monoBM.PopPanel();
        }

        private void Placer_EnterKeyCom(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            monoBM.PushPanel<BSEditMode_KeyComPanel>();
        }

        #endregion
        
        #region Edit
        /// <summary>
        /// 移动前的BPart位置
        /// </summary>
        protected Vector3 _editOldPos;
        /// <summary>
        /// 移动前的BPart旋转
        /// </summary>
        protected Quaternion _editOldRotation;

        #endregion

        #region TextContent
        [System.Serializable]
        public struct BSEditModePanelStruct
        {
            public KeyTip[] KeyTips;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/BuildingSystem/UI";
            this.abname = "BSEditModePanel";
            this.description = "BSEditModePanel数据加载完成";
        }
        #endregion
    }
}

