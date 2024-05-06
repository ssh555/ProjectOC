using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.PinchFace;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UIElements.Image;

namespace ProjectOC.LandMassExpand
{
    public class UIIslandUpdatePanel : UIBasePanel<UIIslandUpdatePanel.IslandUpdatePanelStruct>
    {
        #region UI
        [SerializeField,FoldoutGroup("UI")] 
        private TextMeshProUGUI EventText;
        [SerializeField,FoldoutGroup("UI")] 
        private GameObject TargetPrefab;
        [SerializeField,FoldoutGroup("UI")] 
        private UnityEngine.UI.Image IslandImage,IslandStateImage,emoijImage;
        
        #endregion

        #region Internal
        protected override void InitBtnInfo()
        {
            foreach (GameObject _childGo in TargetPrefab.transform.parent)
            {
                if (_childGo != TargetPrefab)
                {
                    Destroy(_childGo);                    
                }
            }
        }

        protected override void RegisterInput()
        {
            base.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }
        protected override void UnregisterInput()
        {
            base.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
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

        public void SetTargetInfo(string _text, bool _finished)
        {
            GameObject newTarget = GameObject.Instantiate(TargetPrefab);
            newTarget.GetComponentInChildren<TextMeshProUGUI>().text = _text;
            newTarget.SetActive(true);

            if (_finished)
            {
                Transform _imageTrans = newTarget.transform.Find("Image");
                _imageTrans.GetComponent<Image>().tintColor = Color.green;
                _imageTrans.GetChild(0).GetComponent<Image>().tintColor = Color.green;
                _imageTrans.GetChild(0).gameObject.SetActive(true);
            }
            
        }
        
        #endregion
    }
}

