using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ProjectOC.Dialog
{
    public class UIDialogPanel : UIBasePanel<UIDialogPanel.DialogPanelStruct>
    {
        #region Unity

        protected override void Awake()
        {
            base.Awake();
            _dialogManager = LocalGameManager.Instance.DialogManager;
        }
        #endregion

        #region Internal

        protected override void RegisterInput()
        {
            base.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

        }

        protected override void UnregisterInput()
        {
            base.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!optionMode)
            {
                //选项跳转
                _dialogManager.LoadDialogue();
            }
        }
        #endregion
        
        #region TextContent
        
        public struct DialogPanelStruct
        {
            public TextTip[] Btns;
        }
        protected override void OnLoadJsonAssetComplete(UIDialogPanel.DialogPanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PinchRace";
        }
        private void InitBtnData(UIDialogPanel.DialogPanelStruct datas)
        {
            // foreach (var tt in datas.Btns)
            // {
            //     this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            // }
        }
        #endregion
        
        
        #region UI处理
        private DialogManager _dialogManager;
        [SerializeField,FoldoutGroup("UI")]
        private SelectedButton optionTmpBtn;
        [SerializeField, FoldoutGroup("UI")] 
        private TextMeshProUGUI dialogText, npcNameText;

        private bool optionMode = false;
        public void ShowDialogText(string _text)
        {
            dialogText.text = _text;
        }

        public void ShowOption(OptionTableData _optionDatas)
        {
            optionMode = true;
            //处理Option数据为List
            List<OnePieceOption> _options = new List<OnePieceOption>();
            // if (_optionDatas.Optiontext1 != "")
            // {
            //     _options.Add(new OnePieceOption(_optionDatas.Optiontext1,_optionDatas.OptionNextID1));
            // }
            // if (_optionDatas.Optiontext2 != "")
            // {
            //     _options.Add(new OnePieceOption(_optionDatas.Optiontext2,_optionDatas.OptionNextID2));
            // }
            // if (_optionDatas.Optiontext3 != "")
            // {
            //     _options.Add(new OnePieceOption(_optionDatas.Optiontext3,_optionDatas.OptionNextID3));
            // }

            
            //生成设置Option
            for (int i = 0; i < _options.Count; i++)
            {
                int _index = i;
                SelectedButton _btn = GameObject.Instantiate(optionTmpBtn);
                _btn.GetComponentInChildren<TextMeshProUGUI>().text = _options[i].OptionText;
                _btn.onClick.AddListener(() =>
                {
                    _dialogManager.LoadDialogue(_options[_index].NextID);
                });
            }
            
            //btnList
            
        }
        

        public struct OnePieceOption
        {
            public TextContent OptionText;
            public string NextID;

            public OnePieceOption(TextContent _optionText, string _nextID)
            {
                OptionText = _optionText;
                NextID = _nextID;
            }
        }
        #endregion
    }
}

