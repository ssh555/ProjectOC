using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using ProjectOC.ResonanceWheelSystem.UI;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static ML.Engine.UI.OptionPanel;
using static ML.Engine.UI.PopUpUI;
using static ML.Engine.UI.UIManager;

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
            this.UIBtnList.BindInputAction("ConfirmBtn", ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.performed, null, () => { GameManager.Instance.UIManager.PopPanel(); });

            this.UIBtnList.BindInputAction("CancleBtn", ML.Engine.Input.InputManager.Instance.Common.Common.Back, UIBtnListContainer.BindType.performed, null, () => { GameManager.Instance.UIManager.PopPanel(); });
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
            UIBtnList = new UIBtnList(parent: this.transform, hasInitSelect: false);
            this.gameObject.SetActive(true);
            if (data is UIManager.PopUpUIData popUpUIData)
            {
                this.Msg1.text = popUpUIData.msg1;
                this.Msg2.text = popUpUIData.msg2;
                this.UIBtnList.SetBtnAction("ConfirmBtn", popUpUIData.action);
            }

        }
        #endregion

        #region UI对象引用
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
            this.abpath = "ML/Json/TextContent";
            this.abname = "PopUpUI";
            this.description = "PopUpUI数据加载完成";
        }

        #endregion
    }

}
