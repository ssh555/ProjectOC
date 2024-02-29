using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSEditModePanel : Engine.UI.UIBasePanel, Timer.ITickComponent
    {
        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;



        #region UIGO引用
        #region KeyTip
        private UIKeyTip place;
        private UIKeyTip altersocket;
        private UIKeyTip rotateleft;
        private UIKeyTip rotateright;
        private UIKeyTip back;
        private UIKeyTip keycom;

        #endregion

        #endregion

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
        private void Awake()
        {
            this.enabled = false;

            Transform keytips = this.transform.Find("KeyTip");

            place = new UIKeyTip();
            place.root = keytips.Find("KT_Place") as RectTransform;
            place.img = place.root.Find("Image").GetComponent<Image>();
            place.keytip = place.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            place.description = place.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            place.ReWrite(MonoBuildingManager.Instance.KeyTipDict["place"]);

            altersocket = new UIKeyTip();
            altersocket.root = keytips.Find("KT_AlterSocket") as RectTransform;
            altersocket.img = altersocket.root.Find("Image").GetComponent<Image>();
            altersocket.keytip = altersocket.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            altersocket.description = altersocket.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            altersocket.ReWrite(MonoBuildingManager.Instance.KeyTipDict["altersocket"]);

            rotateright = new UIKeyTip();
            rotateright.root = keytips.Find("KT_Rotate") as RectTransform;
            rotateright.img = rotateright.root.Find("Right").GetComponent<Image>();
            rotateright.keytip = rotateright.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            rotateright.description = rotateright.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            rotateright.ReWrite(MonoBuildingManager.Instance.KeyTipDict["rotateright"]);

            rotateleft = new UIKeyTip();
            rotateleft.root = keytips.Find("KT_Rotate") as RectTransform;
            rotateleft.img = rotateleft.root.Find("Left").GetComponent<Image>();
            rotateleft.keytip = rotateleft.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            rotateleft.description = rotateright.description;
            rotateleft.ReWrite(MonoBuildingManager.Instance.KeyTipDict["rotateleft"]);

            back = new UIKeyTip();
            back.root = keytips.Find("KT_Back") as RectTransform;
            back.img = back.root.Find("Image").GetComponent<Image>();
            back.keytip = back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            back.description = back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            back.ReWrite(MonoBuildingManager.Instance.KeyTipDict["back"]);

            keycom = new UIKeyTip();
            keycom.root = this.transform.Find("KT_KeyCom") as RectTransform;
            keycom.img = keycom.root.Find("Image").GetComponent<Image>();
            keycom.keytip = keycom.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            keycom.description = keycom.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            keycom.ReWrite(MonoBuildingManager.Instance.KeyTipDict["keycom"]);
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.RegisterInput();

            // 记录原 Transform
            this._editOldPos = this.Placer.SelectedPartInstance.transform.position;
            this._editOldRotation = this.Placer.SelectedPartInstance.transform.rotation;
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



        public override void OnExit()
        {
            base.OnExit();
            this.UnregisterInput();
            this.Placer.SelectedPartInstance = null;
        }
        #endregion

        #region Refresh
        public override void Refresh()
        {
            place.ReWrite(MonoBuildingManager.Instance.KeyTipDict["place"]);
            altersocket.ReWrite(MonoBuildingManager.Instance.KeyTipDict["altersocket"]);
            rotateright.ReWrite(MonoBuildingManager.Instance.KeyTipDict["rotateright"]);
            rotateleft.ReWrite(MonoBuildingManager.Instance.KeyTipDict["rotateleft"]);
            back.ReWrite(MonoBuildingManager.Instance.KeyTipDict["back"]);
            keycom.ReWrite(MonoBuildingManager.Instance.KeyTipDict["keycom"]);
        }
        #endregion

        #region KeyFunction
        private void UnregisterInput()
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

        private void RegisterInput()
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
                MonoBuildingManager.Instance.PopPanel();
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
            MonoBuildingManager.Instance.PopPanel();
        }

        private void Placer_EnterKeyCom(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            MonoBuildingManager.Instance.PushPanel<BSEditMode_KeyComPanel>();
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
    }
}

