using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem;
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
        #region UI&“˝”√
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
        //private int currentIslandIndex = 0;
        #endregion

        #region Internal
        protected override void InitBtnInfo()
        {
            DeleteTargetPrefabs();
        }

        protected override void RegisterInput()
        {
            base.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed += Maininteract_performed;
        }
        protected override void UnregisterInput()
        {
            base.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed -= Maininteract_performed;
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
            ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
        }

        private void LBRB_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //«–ªªµ∫”Ï
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
        

        #endregion
    }
}

