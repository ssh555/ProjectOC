using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.NPC;
using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
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
            btnList = optionTmpBtn.transform.GetComponentInParent<UIBtnListInitor>();
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
            if (currentOptionIndex != -1)
            {
                ClearOptionBtn();
            }
            
            _dialogManager.LoadDialogueOption(currentOptionIndex);   
        }

        public void PopPanel()
        {
            BtnListDisable();
            ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
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
        private UICameraImage uICameraImage;
        private UIBtnListInitor btnList;
        private UIBtnList UIBtnList;

        private int currentOptionIndex
        {
            get
            {
                if (UIBtnList == null)
                {
                    return -1;   
                }
                else
                {
                    return UIBtnList.GetCurSelectedPos1();   
                }
            }
        }

        public void ShowDialogText(string _text,string _npcName)
        {
            dialogText.text = _text;
            npcNameText.text = _npcName;
        }
        
        public void ShowOption(OptionTableData _optionDatas)
        {
            List<OptionTableData.OnePieceOption> _options = _optionDatas.Options;

            //生成设置Option
            for (int i = 0; i < _options.Count; i++)
            {
                SelectedButton _btn = GameObject.Instantiate(optionTmpBtn,optionTmpBtn.transform.parent);
                _btn.gameObject.name = $"OptionBtn{i}";
                _btn.gameObject.SetActive(true);
                _btn.GetComponentInChildren<TextMeshProUGUI>().text = _options[i].OptionText;
            }
            BtnListInit();
        }

        public void ClearOptionBtn()
        {
            BtnListDisable();
            foreach (Transform _btn in optionTmpBtn.transform.parent)
            {
                if (_btn.gameObject != optionTmpBtn.gameObject)
                {
                    Destroy(_btn.gameObject);
                }
            }
        }

        private void BtnListDisable()
        {
            if (UIBtnList == null)
                return;
            this.UIBtnList.DisableBtnList();
            this.UIBtnList.RemoveAllListener();
            this.UIBtnList.DeBindInputAction();
        }
        private void BtnListInit()
        {
            UIBtnList = new UIBtnList(btnList);
            this.UIBtnList.EnableBtnList();
            this.UIBtnList.BindNavigationInputAction(ProjectOC.Input.InputManager.PlayerInput.PlayerUI.AlterSelected, UIBtnListContainer.BindType.started);
            this.UIBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.started);

        }


        #endregion
    }
}

