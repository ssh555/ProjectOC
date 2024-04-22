using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class UIPinchRacePanel : ML.Engine.UI.UIBasePanel<UIPinchRacePanel.PinchRacePanelStruct>
    {
        #region ML

        protected override void Awake()
        {
            raceNameText = transform.Find("RaceInfo/RaceText").GetComponent<TextMeshProUGUI>();
            raceDescription = transform.Find("RaceInfo/RaceDesciption").GetComponent<TextMeshProUGUI>();
            pinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
            base.Awake();
        }

        protected override void Enter()
        {
            base.Enter();
        }

        protected override void Exit()
        {
            base.Exit();
        }
        #endregion

        #region Internal
        protected override void RegisterInput()
        {
            
            for(int i = 0;i<RacePinchDatas.Count;i++)
            {
                int tmpI = i;
                this.UIBtnListContainer.AddBtn(0, "OC/UI/PinchFace/PinchRaceButtonTemplate.prefab"
                    , () =>
                    {
                        //创建种族
                        // Debug.Log(RacePinchDatas[tmpI].raceName);
                        
                    });
            }
            
            UIBtnListContainer.UIBtnLists[1].SetBtnAction(0,0,() =>
            {
                //创建种族
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                LocalGameManager.Instance.PinchFaceManager.GenerateCustomRaceUI();
            });
            
 
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }
        
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }
        
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
        }
        #endregion
        
        #region TextContent
        [System.Serializable]
        public struct PinchRacePanelStruct
        {
            public TextTip[] Btns;
        }
        protected override void OnLoadJsonAssetComplete(PinchRacePanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PinchRace";
        }
        
        protected override void InitBtnInfo()
        {
            this.UIBtnListContainer = new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            //
            this.UIBtnListContainer.AddOnSelectButtonChangedAction(() =>
            {
                //右侧种族描述更新，中英文切换直接换RacePinchData
                Vector2Int _curPos = UIBtnListContainer.UIBtnLists[0].GetCurSelectedPos();
                if (_curPos == -Vector2Int.one)
                {
                    raceNameText.text = "";
                    raceDescription.text = "";
                }
                else
                {
                    RacePinchData raceData = RacePinchDatas[_curPos.x];
                    raceNameText.SetText(raceData.raceName);
                    raceDescription.SetText(raceData.raceDescription);
                }
            });
        }
        private void InitBtnData(PinchRacePanelStruct datas)
        {
            // foreach (var tt in datas.Btns)
            // {
            //     this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            // }
        }
        
        #endregion


        #region PinchRacePanel

        private PinchFaceManager pinchFaceManager;
        [ShowInInspector]
        private UIBtnListContainer UIBtnListContainer;
        
        public List<RacePinchData> RacePinchDatas=>pinchFaceManager.RacePinchDatas;

        private TextMeshProUGUI raceNameText, raceDescription;

        #endregion


    }   
}
