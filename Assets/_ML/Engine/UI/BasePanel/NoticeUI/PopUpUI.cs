using ML.Engine.Manager;
using ML.Engine.TextContent;
using System;
using TMPro;
using UnityEngine;
using static ML.Engine.UI.PopUpUI;

namespace ML.Engine.UI
{
    public class PopUpUI : ML.Engine.UI.UIBasePanel<PopUpUIStruct> ,INoticeUI
    {
        #region Unity
        public bool IsInit = false;
        
        protected override void Awake()
        {
            base.Awake();
            this.Msg1 = this.transform.Find("TextGroup").Find("Text1").GetComponent<TextMeshProUGUI>();
            this.Msg2 = this.transform.Find("TextGroup").Find("Text2").GetComponent<TextMeshProUGUI>();
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
        public override void OnEnter()
        {
            base.OnEnter();
        }

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


        }

        protected override void RegisterInput()
        {
            this.UIBtnList.BindInputAction("ConfirmBtn", ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.performed, null, () => { GameManager.Instance.UIManager.PopPanel(); GameManager.Instance.UIManager.GetTopUIPanel().SetHidePanel(false); });

            this.UIBtnList.BindInputAction("CancleBtn", ML.Engine.Input.InputManager.Instance.Common.Common.Back, UIBtnListContainer.BindType.performed, null, () => { GameManager.Instance.UIManager.PopPanel(); GameManager.Instance.UIManager.GetTopUIPanel().SetHidePanel(true); });
        }

        #endregion

        #region INoticeUI
        public void SaveAsInstance()
        {
            this.gameObject.SetActive(false);
        }

        public void CopyInstance<D>(D data)
        {
            //PopUPUI UIBtnList��ʼ�������⣬��Ϊ������Ҫʹ������ʼ������
            UIBtnList = new UIBtnList(this.transform.Find("ButtonList").GetComponent<UIBtnListInitor>());
            this.gameObject.SetActive(true);
            if (data is UIManager.PopUpUIData popUpUIData)
            {
                this.Msg1.text = popUpUIData.msg1;
                this.Msg2.text = popUpUIData.msg2;
                this.UIBtnList.SetBtnAction("ConfirmBtn", popUpUIData.ConfirmAction);
                this.UIBtnList.SetBtnAction("CancleBtn", popUpUIData.CancelAction);
            }

        }
        #endregion

        #region UI��������
        private TMPro.TextMeshProUGUI Msg1,Msg2;
        private Transform ImageGroup;
        #endregion

        #region TextContent
        private UIBtnList UIBtnList;
        [System.Serializable]
        public struct PopUpUIStruct
        {
            //BotKeyTips
            public KeyTip Confirm;
            public KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "TextContent";
            this.abname = "PopUpUI";
            this.description = "PopUpUI���ݼ������";
        }

        #endregion
    }

}
