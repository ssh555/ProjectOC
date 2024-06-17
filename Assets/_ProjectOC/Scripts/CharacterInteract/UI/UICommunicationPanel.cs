using ML.Engine.Manager;
using ML.Engine.Timer;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using ML.Engine.Utility;
using static ProjectOC.ResonanceWheelSystem.UI.UICommunicationPanel;
using TMPro;
namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class UICommunicationPanel : ML.Engine.UI.UIBasePanel<CommunicationPanelStruct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {
            base.Awake();
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
            base.Enter();
        }
        protected override void Exit()
        {
            base.Exit();
            ClearTemp();
        }

        public override void OnRecovery()
        {
            //Recovery 不用刷新 防止删除按钮异步报错
            this.RegisterInput();
        }

        public override void OnPause()
        {
            this.UnregisterInput();
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Disable();

            ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed -= LastTerm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed -= NextTerm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.started -= TurnPage_started;
            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.canceled -= TurnPage_canceled;

            this.CharacterBioList.RemoveAllListener();
            this.CharacterBioList.DeBindInputAction();

        }
        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed += LastTerm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed += NextTerm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.started += TurnPage_started;
            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.canceled += TurnPage_canceled;

            this.CharacterBioList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }

        private void LastTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            FunctionIndex = (FunctionIndex + FunctionType - 1) % FunctionType;
            this.Refresh();
        }

        private void NextTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            FunctionIndex = (FunctionIndex + 1) % FunctionType;
            this.Refresh();
        }

        private float TimeInterval = 0.2f;
        private CounterDownTimer timer = null;
        private void TurnPage_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (timer == null)
            {
                timer = new CounterDownTimer(TimeInterval, true, true, 1, 2);
                timer.OnEndEvent += () =>
                {
                    var vector2 = obj.ReadValue<UnityEngine.Vector2>();
                    //this.DescriptionScrollView.verticalScrollbar.value += vector2.y;
                };
            }
        }
        private void TurnPage_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(timer);
            timer = null;
        }

        #endregion

        #region UI
        #region temp
        private Sprite icon_genderfemaleSprite, icon_gendermaleSprite;
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        private void ClearTemp()
        {
            GameManager.DestroyObj(icon_genderfemaleSprite);
            GameManager.DestroyObj(icon_gendermaleSprite);
            // sprite
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
            }
        }

        #endregion

        #region UI对象引用
        [SerializeField, FoldoutGroup("UI")]
        private Transform MessagePanel;
        [SerializeField, FoldoutGroup("UI")]
        private Transform Chat;
        [SerializeField, FoldoutGroup("UI")]
        private Transform ContactsPanel;
        [SerializeField, FoldoutGroup("UI")]
        private Transform Function;
        [SerializeField, FoldoutGroup("UI")]
        private Transform FunctionPanel;

        private int FunctionType { get { return Function.childCount; } }
        private int FunctionIndex = 0;

        #endregion

        public override void Refresh()
        {
            if (this.ABJAProcessorJson == null || !this.ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }
            #region FunctionType
            for (int i = 0; i < FunctionType; i++)
            {
                Function.GetChild(i).Find("Selected").gameObject.SetActive(FunctionIndex == i);
                FunctionPanel.GetChild(i).gameObject.SetActive(FunctionIndex == i);
            }
            var msgContent = LocalGameManager.Instance.CommunicationManager.GetMessageContent(curSelectedCharacterBioMsgID);

            if(msgContent.Count > 0)
            {
                //chat 显示对话
                
            }
            #endregion

            #region KeyTip

            #endregion

        }
        #endregion

        #region Resource

        #region TextContent
        [System.Serializable]
        public struct CommunicationPanelStruct
        {

        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/ResonanceWheel";
            this.abname = "BeastPanel";
            this.description = "BeastPanel数据加载完成";
        }
        #endregion

        protected override void InitObjectPool()
        {
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "CharacterBioPool", LocalGameManager.Instance.CommunicationManager.MessageInfos.Count, "Prefab_CharacterInteract_UIPrefab/Prefab_CharacterInteract_UI_CharacterBioInMessage.prefab");
            base.InitObjectPool();
        }
        [SerializeField, FoldoutGroup("UI")]
        private UIBtnListInitor CharacterBioListInitor;
        [ShowInInspector]
        private UIBtnList CharacterBioList;
        private Dictionary<SelectedButton,string> BtnToMsgIDDic = new Dictionary<SelectedButton,string>();
        private string curSelectedCharacterBioMsgID
        {
            get
            {
                SelectedButton btn = null;
                btn = this.CharacterBioList.GetCurSelected();
                if(btn != null && BtnToMsgIDDic.ContainsKey(btn))
                {
                    return BtnToMsgIDDic[btn];
                }
                return "";
            }
        }
        protected override void InitBtnInfo()
        {
            CharacterBioList = new UIBtnList(CharacterBioListInitor);
            this.CharacterBioList.OnSelectButtonChanged += () => { this.Refresh(); };
        }
        protected override void InitBtnInfoAfterInitObjectPool()
        {
            this.objectPool.ResetAllObject();
            foreach (var MessageInfo in LocalGameManager.Instance.CommunicationManager.MessageInfos)
            {
                var tPrefab = this.objectPool.GetNextObject("CharacterBioPool");
                var data = LocalGameManager.Instance.OCCharacterManager.GetOCCharacterData(MessageInfo.OCChacracterID);
                tPrefab.transform.Find("Icon").Find("Text1").GetComponent<TextMeshProUGUI>().text = data.CodeName;
                BtnToMsgIDDic.Add(tPrefab.GetComponent<SelectedButton>(), MessageInfo.MsgID);
                CharacterBioList.AddBtn(tPrefab, BtnSettingAction: (btn) => {  });
            }
        }
        #endregion
    }


}
