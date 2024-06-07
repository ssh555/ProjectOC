using ML.Engine.Manager;
using ML.Engine.TextContent;
using System;
using TMPro;
using UnityEngine;
using static ML.Engine.UI.PopUpUI;

namespace ML.Engine.UI
{
    public class InputFieldUI : ML.Engine.UI.UIBasePanel<InputFieldUI.InputFieldStruct> ,INoticeUI
    {
        #region Unity
        public bool IsInit = false;
        
        protected override void Awake()
        {
            base.Awake();
            this.Msg1 = this.transform.Find("TextGroup/TextDesciption1").GetComponent<TextMeshProUGUI>();
            this.Msg2 = this.transform.Find("TextGroup/TextDesciption2").GetComponent<TextMeshProUGUI>();
            this.InputField1 = this.transform.Find("TextGroup/InputField1/TextArea/Text").GetComponent<TextMeshProUGUI>();
            this.InputField2 = this.transform.Find("TextGroup/InputField2/TextArea/Text").GetComponent<TextMeshProUGUI>();
            this.ImageGroup = this.transform.Find("ImageGroup");
        }

        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        #endregion

        #region Override
        protected override void Enter()
        {
            this.UIBtnList.EnableBtnList();
            Invoke("RegisterInput", 0.1f);
        }

        protected override void Exit()
        {
            this.UIBtnList.DisableBtnList();
            base.Exit();
        }


        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            this.UIBtnList.RemoveAllListener();
            this.UIBtnList.DeBindInputAction();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {
            this.UIBtnList.BindInputAction("ConfirmBtn", ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.performed, null, () => { GameManager.Instance.UIManager.PopPanel(); GameManager.Instance.UIManager.GetTopUIPanel().SetHidePanel(false); });
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.UIBtnList.BindInputAction("CancleBtn", ML.Engine.Input.InputManager.Instance.Common.Common.Back, UIBtnListContainer.BindType.performed, null, () => { });
            CancelAction();
        }
        #endregion

        #region INoticeUI
        public void SaveAsInstance()
        {
            this.gameObject.SetActive(false);
        }

        public void CopyInstance<D>(D data)
        {
            //PopUPUI UIBtnList初始化放在这，因为下面需要使用它初始化数据
            UIBtnList = new UIBtnList(this.transform.Find("ButtonList").GetComponent<UIBtnListInitor>());
            this.gameObject.SetActive(true);
            if (data is UIManager.InputFieldUIData inputFieldUIData)
            {
                this.Msg1.text = inputFieldUIData.msg1;
                
                this.confirmAction = inputFieldUIData.ConfirmAction;
                this.UIBtnList.SetBtnAction("ConfirmBtn", this.Action_Confirm);
                CancelAction = inputFieldUIData.CancelAction;
                // this.UIBtnList.SetBtnAction("CancleBtn", inputFieldUIData.CancelAction);
                if (inputFieldUIData.msg2 == "")
                {
                    this.Msg2.gameObject.SetActive(false);
                    this.InputField2.transform.parent.parent.gameObject.SetActive(false);
                }
                else
                {
                    this.Msg2.text = inputFieldUIData.msg2;
                }
            }

        }

        void Action_Confirm()
        {
            confirmAction(InputField1.text,InputField2.text);
        }
        #endregion

        #region UI对象引用
        private TMPro.TextMeshProUGUI Msg1,Msg2,InputField1,InputField2;
        private Transform ImageGroup;
        private  UnityEngine.Events.UnityAction<string, string> confirmAction;
        private UnityEngine.Events.UnityAction CancelAction = null;
        #endregion

        #region TextContent
        private UIBtnList UIBtnList;
        [System.Serializable]
        public struct InputFieldStruct
        {
            //BotKeyTips
            public KeyTip Confirm;
            public KeyTip Back;
        }

        // protected override void OnLoadJsonAssetComplete(InputFieldStruct datas)
        // {
        //     InitBtnData(datas);
        // }
        // private void InitBtnData(InputFieldStruct datas)
        // {
        //     
        // }
        protected override void InitTextContentPathData()
        {
            this.abpath = "TextContent";
            this.abname = "PopUpUI";
            this.description = "InputFieldUI数据加载完成";
        }

        #endregion
    }

}
