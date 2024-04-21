using ML.Engine.Manager;
using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
namespace ML.Engine.UI
{
    public class BtnUI : ML.Engine.UI.UIBasePanel,INoticeUI
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {

            base.Awake();

            this.Msg = this.transform.Find("BtnText").GetComponent<TextMeshProUGUI>();
        }

        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        #endregion

        #region Override

        #endregion

        #region Internal

        protected override void UnregisterInput()
        {

            //ȷ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

        }

        protected override void RegisterInput()
        {
            //ȷ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //�ֶ�ģ���ջ
            GameManager.Instance.UIManager.GetTopUIPanel().OnRecovery();
            this.OnExit();
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //�ֶ�ģ���ջ
            GameManager.Instance.UIManager.GetTopUIPanel().OnRecovery();
            this.OnExit();
        }


        #endregion

        #region INoticeUI
        public void SaveAsInstance()
        {
            this.gameObject.SetActive(false);
        }

        public void CopyInstance<D>(D data)
        {
            this.gameObject.SetActive(true);
            if(data is UIManager.BtnUIData btnData)
            {
                this.Msg.text = btnData.msg;
                //TODO
            }
        }
        #endregion

        #region UI
        #region Temp


        #endregion

        #region UI��������

        public TMPro.TextMeshProUGUI Msg;

        #endregion


        #endregion

        #region TextContent
        [System.Serializable]
        public struct BtnUIStruct
        {
        }

        #endregion
    }

}
