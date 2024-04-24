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
            foreach (var _btnList in UIBtnListContainer.UIBtnLists)
            {
                _btnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm,UIBtnListContainer.BindType.started);
            }
            
            //0-4,5是通用不需要加
            for (int i = 0; i < pinchFaceManager.pinchPartType1Inclusion.Count-1; i++)
            {
                foreach (var _type2 in pinchFaceManager.pinchPartType1Inclusion[i])
                {
                    this.UIBtnListContainer.AddBtn(i+1,pinchButtonPath
                        ,BtnText: _type2.ToString()
                        ,BtnAction:LeftButton_BtnAction
                        ,BtnSettingAction:(btn) =>
                        {
                            //To-Do 应该是异步加载完刷新一次的，而不是每次生成完Button都刷新，不过看上去也不卡
                            leftButtonDic.Add(_type2, btn);
                            pinchFaceManager.pinchFaceHelper.RefreshPanelLayout(this.transform);
                            SetLeftBtnText(btn, isNake:true);
                        });
                }
            }
        }
        
        
        
        private void LeftButton_BtnAction()
        {
            UIBtnListContainer.UIBtnLists[7].DeleteAllButton();  
            
            int _curListIndex = UIBtnListContainer.CurSelectUIBtnListIndex;
            Vector2Int _curPos = UIBtnListContainer.UIBtnLists[_curListIndex].GetCurSelectedPos2();
            int _curPosOne = _curPos.x * OneRowCount + _curPos.y;
            
            curType2 = pinchFaceManager.pinchPartType1Inclusion[_curListIndex-1][_curPosOne];
            PinchPartType2 _type2 = curType2;
            PinchPartType _ppt = pinchFaceManager.pinchPartType2Dic[curType2];
                
            if (_ppt.couldNaked)
            {
                this.UIBtnListContainer.AddBtn(7, pinchButtonPath
                    , BtnText: "Naked"
                    ,BtnAction: () =>
                    {
                        RightButton_BtnAction(_type2,0);
                    });
            }


            for (int i = 0;i < _ppt.pinchPartType3s.Count; i++)
            {
                this.UIBtnListContainer.AddBtn(7, pinchButtonPath
                    , BtnText: _ppt.pinchPartType3s[i].ToString()
                    ,BtnAction: () =>
                    {
                        RightButton_BtnAction(_type2,i+1);
                    }
                    ,BtnSettingAction: (_btn) =>
                    {
                        SetLeftBtnText(_btn, isNake:true);
                    });
            }

            UIBtnListContainer.CurSelectUIBtnList = UIBtnListContainer.UIBtnLists[7];
        }
        
                

        private void RightButton_BtnAction(PinchPartType2 _type2,int _type3)
        {
            //修改Panel text、图片  ,加入种族
            PinchPartType ppt = pinchFaceManager.pinchPartType2Dic[curType2];
            SelectedButton _btn = leftButtonDic[_type2];
            
            
            
            if (_type3 == 0)
            {
                
                SetLeftBtnText(_btn, ppt.pinchPartType1.ToString(),true);
            }
            else
            {
                SetLeftBtnText(_btn, ppt.pinchPartType3s[_type3].ToString(),false);
            }
            
            AddType3(_type2, _type3);
            BackActionOfList7();
        }
        //右侧面部Button相关函数
        private void AddType3(PinchPartType2 _type2,int _type3)
        {
            PinchPartType dicType = pinchFaceManager.pinchPartType2Dic[_type2];
            
            foreach (var _dicType3 in dicType.pinchPartType3s)
            {
                if (raceData.pinchPartType3s.Contains(_dicType3))
                {
                    raceData.pinchPartType3s.Remove(_dicType3);
                }
            }
            
            //删除之前的
            _type3--;
            if (_type3 >= 0)
            {
                raceData.pinchPartType3s.Add(dicType.pinchPartType3s[_type3-1]);    
            }
        }

        
        private void BackActionOfList7()
        {            
            // 返回 selectType
            int _listIndex = (int)pinchFaceManager.pinchPartType2Dic[curType2].pinchPartType1;
            UIBtnListContainer.CurSelectUIBtnList = UIBtnListContainer.UIBtnLists[_listIndex];
            UIBtnListContainer.UIBtnLists[7].DeleteAllButton();
            curType2 = PinchPartType2.None;
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
        private const int OneRowCount = 5;
        private int curbtnListIndex = -1;
        private PinchPartType2 curType2 = PinchPartType2.None;
        private Dictionary<PinchPartType2, SelectedButton> leftButtonDic = new Dictionary<PinchPartType2, SelectedButton>();
        private RacePinchData raceData = new RacePinchData();
        private string pinchButtonPath = "Prefabs_PinchPart/UIPanel/Prefab_Pinch_BaseUISelectedBtn.prefab";
        
        
        
        private void SetLeftBtnText(SelectedButton _btn,string _str = "",bool isNake = false)
        {
            if (_str != "")
            {
                _btn.GetComponent<TextMeshPro>().SetText(_str);    
            }
            
            Image _image = _btn.transform.Find("Image").GetComponent<Image>();
            if (isNake)
            {
                _image.color = Color.gray;
            }
            else
            {
                _image.color = Color.white;
            }
        }
        #endregion
    }
}