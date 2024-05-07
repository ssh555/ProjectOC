using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.NPC;
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
            btnList = optionTmpBtn.transform.GetComponentInParent<UIBtnListInitor>();
            
            //生成Character
            string ChatCharacterNpcPath = "Prefab_Dialog/Prefab_ChatNpc.prefab";
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(ChatCharacterNpcPath).Completed+=(handle) =>
            {
                _dialogManager.CurrentChatNpc = handle.Result.GetComponent<NPCCharacter>();
                _dialogManager.LoadDialogue(FirstDialogueID);
                
                uICameraImage = transform.Find("Dialogue/UICameraImage").GetComponentInChildren<UICameraImage>();
                RectTransform _rtTransform = uICameraImage.transform as RectTransform;
                RenderTexture _rt = new RenderTexture((int)_rtTransform.rect.width,(int)_rtTransform.rect.height,0);
                
                //todo 临时用这种方法来看向Character
                uICameraImage.Init(_rt);
                //Transform lookAtTransform = handle.Result.transform.Find("CameraLookPos");
                handle.Result.transform.Find("Model").rotation = Quaternion.Euler(0,180,0);
                handle.Result.transform.Find("Model").position = handle.Result.transform.Find("Model").position + Vector3.down * 1.2f;
                uICameraImage.LookAtGameObject(handle.Result);
            };
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
            Debug.Log($"交互触发  {optionMode}");
            if (!optionMode)
            {
                _dialogManager.LoadDialogue();
            }
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
        public string FirstDialogueID;
        [ShowInInspector,ReadOnly]
        private bool optionMode = false;
        public void ShowDialogText(string _text,string _npcName)
        {
            dialogText.text = _text;
            npcNameText.text = _npcName;
        }
        
        public void ShowOption(OptionTableData _optionDatas)
        {
            optionMode = true;
            //处理Option数据为List
            List<OptionTableData.OnePieceOption> _options = _optionDatas.Options;

            //生成设置Option
            for (int i = 0; i < _options.Count; i++)
            {
                int _index = i;
                SelectedButton _btn = GameObject.Instantiate(optionTmpBtn,optionTmpBtn.transform.parent);
                _btn.gameObject.name = $"OptionBtn{i}";
                _btn.gameObject.SetActive(true);
                _btn.GetComponentInChildren<TextMeshProUGUI>().text = _options[i].OptionText;
                _btn.onClick.AddListener(() =>
                {
                    ClearOptionBtn();
                    _dialogManager.LoadDialogue(_options[_index].NextID);
                    optionMode = false;
                });
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

