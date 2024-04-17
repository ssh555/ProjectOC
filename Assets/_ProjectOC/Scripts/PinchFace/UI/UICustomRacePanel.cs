using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectOC.PinchFace
{
    public class UICustomRacePanel : ML.Engine.UI.UIBasePanel<UICustomRacePanel.PinchCustomRacePanelStruct>
    {
        #region Unity

        protected override void Awake()
        {
            base.Awake();
            pinchFaceManager = LocalGameManager.Instance.PinchFaceManager;

        }

        #endregion


        #region Internal

        protected override void RegisterInput()
        {
            base.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }
        
        //btnList0 随机种族
        //btnList1--5  种族部件
        //btnList6  完成创建
        //btnList7  选择部件
        protected override void InitBtnInfo()
        {
            
            this.UIBtnListContainer = new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            //0-5
            for (int i = 0; i < pinchFaceManager.pinchPartType1Inclusion.Count; i++)
            {
                foreach (var _type2 in pinchFaceManager.pinchPartType1Inclusion[i])
                {
                    this.UIBtnListContainer.AddBtn(i,"OC/UI/PinchFace/Pinch_BaseUISelectedBtn.prefab"
                        ,BtnText: _type2.ToString()
                        ,BtnAction:LeftButton_BtnAction
                        ,BtnSettingAction:(btn) =>
                        {
                            //To-Do 应该是异步加载完刷新一次的，而不是每次生成完Button都刷新，不过看上去也不卡
                            this.RefreshPanelLayout();
                        });
                }
                
            }
            
            this.UIBtnListContainer.AddOnSelectButtonChangedAction(RefreshCurrentSelect);
            
        }

        private void RefreshCurrentSelect()
        {
            //Temp，最好有函数直接返回当前btnList下标
            for (int i = 1; i < 6; i++)
            {
                var _selectPos = UIBtnListContainer.UIBtnLists[i].GetCurSelectedPos();
                if (_selectPos != -Vector2Int.one)
                {
                    curbtnListIndex = i;
                    curBtnPos = _selectPos;
                    int _type2Index = curBtnPos.x * 5 + curBtnPos.y +1;
                    curType2 = pinchFaceManager.pinchPartType1Inclusion[curbtnListIndex][_type2Index];
                    return;
                }
            }
        }
        
        private void LeftButton_BtnAction()
        {
            //naked ?
            
            foreach (var _pinchFaceType3 in    pinchFaceManager.pinchPartType2Dic[curType2].pinchPartType3s)
            {
                this.UIBtnListContainer.AddBtn(7, "OC/UI/PinchFace/Pinch_BaseUISelectedBtn.prefab"
                    , BtnText: _pinchFaceType3.ToString()
                    ,BtnAction:RightButton_BtnAction);
            }
            UIBtnListContainer.CurSelectUIBtnList = UIBtnListContainer.UIBtnLists[7];
        }
        
        //右侧面部Button相关函数
        private void RightButton_BtnAction()
        {
            // 返回 selectType
            var _selectPos = UIBtnListContainer.UIBtnLists[7].GetCurSelectedPos();
            int _type2Index = _selectPos.x * 5 + _selectPos.y +1;
            UIBtnListContainer.UIBtnLists[(int)curType2].
            
            
            UIBtnListContainer.UIBtnLists[7].DeleteAllButton();
        }
        
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //如果当前在btnList[7]
            ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
        }
        #endregion
  

        #region TextContent
        public struct PinchCustomRacePanelStruct
        {
            public TextTip[] Btns;
        }
        protected override void OnLoadJsonAssetComplete(PinchCustomRacePanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PinchRace";
        }
        private void InitBtnData(PinchCustomRacePanelStruct datas)
        {
            // foreach (var tt in datas.Btns)
            // {
            //     this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            // }
        }
        #endregion

        #region CustomRace

        private PinchFaceManager pinchFaceManager;
        private UIBtnListContainer UIBtnListContainer;
        private int curbtnListIndex = -1;
        private Vector2Int curBtnPos = new Vector2Int(-1,-1);
        private Vector3Int selectType = new Vector3Int(-1,-1,-1);
        private PinchPartType2 curType2 = PinchPartType2.None;
        public void RefreshPanelLayout()
        {
            LayoutGroup[] layoutGroups = this.transform.GetComponentsInChildren<LayoutGroup>();
            foreach (var group in layoutGroups)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(group.transform as RectTransform);
            }
        }


        
        private void SetLeftBtnText(int _listIndex,Vector2Int _pos, string _str,bool isNake = false)
        {
            var btn = UIBtnListContainer.UIBtnLists[_listIndex].GetTwoDimSelectedButtons[_pos.x][_pos.y];
            btn.GetComponent<TextMeshPro>().SetText(_str);
            if (isNake)
            {
                
            }
        }
        #endregion
    }
}