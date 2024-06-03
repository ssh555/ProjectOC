using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.PinchFace;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace ProjectOC.LandMassExpand
{
    public class UIIslandUpdatePanel : UIBasePanel<UIIslandUpdatePanel.IslandUpdatePanelStruct>
    {
        #region UI&引用
        [SerializeField,FoldoutGroup("UI")] 
        private TextMeshProUGUI EventText,StateText;
        [SerializeField,FoldoutGroup("UI")] 
        private GameObject TargetPrefab;
        [SerializeField,FoldoutGroup("UI")] 
        private UnityEngine.UI.Image IslandImage,IslandStateImage;
        [SerializeField,FoldoutGroup("UI")] 
        private Transform emoijTransform;
        [SerializeField, FoldoutGroup("UI")] 
        private SelectedButton _updateBtn;

        [HideInInspector]
        public IslandUpdateInteract IslandUpdateInteract;

        private SpriteAtlas sa;
        private int currentIslandIndex = 0;
        #endregion

        #region Internal
        protected override void InitBtnInfo()
        {
            DeleteTargetPrefabs();
        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed += Maininteract_performed;

            // 切换类目
            ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed += LastTerm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed += NextTerm_performed;
        }
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed -= Maininteract_performed;

            // 切换类目
            ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed -= LastTerm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed -= NextTerm_performed;
        }

        private void Maininteract_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (IslandUpdateInteract.CouldUpdate)
            {
                DeleteTargetPrefabs();
                LocalGameManager.Instance.BuildPowerIslandManager.IslandUpdate();
                IslandUpdateInteract.IslandInfoUpdate(this);
            }
        }
        
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (currentIslandIndex == 2)
            {
                if(isBackEnable)
                {
                    IslandRudderPanelInstance.transform.SetParent(null, false);
                    ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                    ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                    return;
                }
            }
            else
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
            }
        }

        private void LastTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            currentIslandIndex = (currentIslandIndex + Function.childCount - 1) % Function.childCount;
            this.Refresh();
        }

        private void NextTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            currentIslandIndex = (currentIslandIndex + 1) % Function.childCount;
            this.Refresh();
        }
        #endregion

        #region TextContent
        public struct IslandUpdatePanelStruct
        {
            public TextTip[] Btns;
        }
        protected override void OnLoadJsonAssetComplete(UIIslandUpdatePanel.IslandUpdatePanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PinchRace";
        }
        private void InitBtnData(UIIslandUpdatePanel.IslandUpdatePanelStruct datas)
        {
            // foreach (var tt in datas.Btns)
            // {
            //     this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            // }
        }
        
        #endregion

        #region UIProcess
        public void SetTargetInfo(string _text, bool _finished)
        {
            GameObject newTarget = GameObject.Instantiate(TargetPrefab,TargetPrefab.transform.parent);
            newTarget.GetComponentInChildren<TextMeshProUGUI>().text = _text;
            newTarget.SetActive(true);
            if (LocalGameManager.Instance.BuildPowerIslandManager.CurrentLandLevelData.IsMax)
            {
                newTarget.transform.Find("Image").gameObject.SetActive(false);
            }
            
            
            if (_finished)
            {
                Transform _imageTrans = newTarget.transform.Find("Image");
                _imageTrans.GetComponent<Image>().color = Color.green;
                _imageTrans.GetChild(0).GetComponent<Image>().color = Color.green;
                _imageTrans.GetChild(0).gameObject.SetActive(true);
            }
            
        }

        public void SetCouldUpdate(bool _couldUpdate)
        {
            if (_couldUpdate)
            {
                _updateBtn.GetComponent<Image>().color = new Color(0.274758f,0.759f,0.3485978f);
            }
            else
            {
                _updateBtn.GetComponent<Image>().color = Color.gray;
            }
            
        }
        
        private void DeleteTargetPrefabs()
        {
            foreach (Transform _childTransf in TargetPrefab.transform.parent)
            {
                if (_childTransf != TargetPrefab.transform)
                {
                    Destroy(_childTransf.gameObject);                    
                }
            }
        }
        
        public void SetLevelInfo(int _level,string _StateText,string _EventText)
        {
            if (_level != 0)
            {
                emoijTransform.GetChild(_level-1).GetComponent<Image>().color = Color.white;
            }
            emoijTransform.GetChild(_level).GetComponent<Image>().color = Color.green;
            
            StateText.text = _StateText;
            EventText.text = _EventText;
        }

        public override void Refresh()
        {
            #region FunctionType
            for (int i = 0; i < Function.childCount; i++)
            {
                Function.GetChild(i).Find("Selected").gameObject.SetActive(currentIslandIndex == i);
                FunctionPanel.GetChild(i).gameObject.SetActive(currentIslandIndex == i);
            }

            if(currentIslandIndex == 2)
            {
                PushIslandRudderPanel();
            }
            else
            {
                if(isPushed)
                {
                    isPushed = false;
                    IslandRudderPanelInstance.transform.SetParent(null, false);
                    ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                }
            }
            #endregion
        }

        private void PushIslandRudderPanel()
        {
            IslandRudderPanelInstance.gameObject.SetActive(true);
            IslandRudderPanelInstance.transform.SetParent(this.transform, false);
            this.SetHidePanel(false);
            IslandRudderPanelInstance.UIIslandUpdatePanel = this;
            ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(IslandRudderPanelInstance);
            isPushed = true;
        }

        #endregion

        #region override
        public override void OnExit()
        {
            base.OnExit();
            this.SetHidePanel(true);
        }
        public override void OnPause()
        {
            //不走base.OnPause();
        }

        public override void OnRecovery()
        {
            //不走base.OnRecovery();
        }
        #endregion

        #region IslandRudderPanel
        [SerializeField, FoldoutGroup("UI")]
        private Transform Function;
        [SerializeField, FoldoutGroup("UI")]
        private Transform FunctionPanel;
        private bool isPushed = false;
        private UIIslandRudderPanel IslandRudderPanelInstance => LocalGameManager.Instance.MineSystemManager.IslandRudderPanelInstance;

        public void ChangeToIslandRudderPanel()
        {
            ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed -= Maininteract_performed;
        }

        public void IslandRudderPanelChangeTo()
        {
            ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed += Maininteract_performed;
        }

        private bool isBackEnable = true;
        public void DisableBack_performed()
        {
            isBackEnable = false;
        }
        public void  EnableBack_performed()
        {
            isBackEnable = true;
        }
        #endregion
    }
}

