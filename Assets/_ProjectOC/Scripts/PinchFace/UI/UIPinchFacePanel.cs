using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class UIPinchFacePanel : UIBasePanel<UIPinchFacePanel.PinchFacePanelStruct>
    {
        #region ML
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
            uIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }
        protected override void UnregisterInput()
        {
            base.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            this.uIBtnListContainer.DisableUIBtnListContainer();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //如果当前在btnList[7]
            ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
        }
        
        //btnList0 随机
        //btnList1 2
        //btnList3 完成创建 
        
        //btnList4--？  样式、颜色
        protected override void InitBtnInfo()
        {
            this.uIBtnListContainer =
                new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
        }
        
        //返回右侧BtnList，重新生成
        public void ReGenerateBtnListContainer(List<UIBtnListInitor> _btnLists)
        {
            rightBtnLists = _btnLists;
            UIBtnListContainerInitor newBtnListContainers = this.transform.GetComponentInChildren<UIBtnListContainerInitor>();
            //对 btnList 排序
            //最后一个不需要加连接线
            for (int i = 0; i < _btnLists.Count-1; i++)
            {
                // _listContainer.LinkTwoEdge();
                UIBtnListContainerInitor.LinkData _linkData = new UIBtnListContainerInitor
                    .LinkData(i+4,i+5,UIBtnListContainerInitor.EdgeType.下侧顺时针,UIBtnListContainerInitor.EdgeType.上侧逆时针,UIBtnListContainerInitor.LinkType.上下相连);
                newBtnListContainers.btnListContainerInitData.AddLinkData(_linkData);
            }
            this.uIBtnListContainer = new UIBtnListContainer(newBtnListContainers);
        }
        #endregion

        
        #region TextContent
        public struct PinchFacePanelStruct
        {
            public TextTip[] Btns;
        }
        protected override void OnLoadJsonAssetComplete(UIPinchFacePanel.PinchFacePanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PinchRace";
        }
        private void InitBtnData(UIPinchFacePanel.PinchFacePanelStruct datas)
        {
            // foreach (var tt in datas.Btns)
            // {
            //     this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            // }
        }
        #endregion


        #region FacePanel

        private PinchFaceManager pinchFaceManager;
        private UIBtnListContainer uIBtnListContainer;
        public Transform containerTransf;
        private RacePinchData raceData;
        private List<PinchPart> pinchParts;
        private Dictionary<PinchPartType3, SelectedButton> type3ButtonDic = new Dictionary<PinchPartType3, SelectedButton>();
        private Dictionary<PinchPartType3, PinchPartType2> type3Type2Dic = new Dictionary<PinchPartType3, PinchPartType2>();
        public List<UIBtnListInitor> rightBtnLists = new List<UIBtnListInitor>();
        public void InitRaceData(RacePinchData _racePinchData)
        {
            raceData = _racePinchData;
            pinchParts = new List<PinchPart>();
            
            //生成左侧List<PinchPartType3>
            foreach (var _DicType in pinchFaceManager.pinchPartType2Dic)
            {
                foreach (var _DicType3 in _DicType.Value.pinchPartType3s)
                {
                    if (raceData.pinchPartType3s.Contains(_DicType3))
                    {
                        type3Type2Dic.Add(_DicType3,_DicType.Key);
                    }
                }
            }
            

            foreach (var _partType3 in raceData.pinchPartType3s)
            {
                PinchPartType2 _type2 = type3Type2Dic[_partType3];
                PinchPartType _type = pinchFaceManager.pinchPartType2Dic[_type2];
                this.uIBtnListContainer.AddBtn(1, "OC/UI/PinchFace/Pinch_BaseUISelectedBtn.prefab"
                    ,BtnText:_partType3.ToString()
                    ,BtnSettingAction:(_btn)=>
                    {
                        type3ButtonDic.Add(_partType3,_btn);
                        pinchFaceManager.pinchFaceHelper.RefreshPanelLayout(this.transform);
                    }
                    ,BtnAction: () =>
                    {
                        LeftButton_BtnAction(_partType3);
                    });
            }
        }

        private void LeftButton_BtnAction(PinchPartType3 _type3)
        {
            //删除所有ChangeSetting
            foreach (Transform _container in containerTransf)
            {
                 _container.gameObject.SetActive(false);
                 Destroy(_container.gameObject);
            }
            GenerateChangeSetting(_type3);
        }

        private void GenerateChangeSetting(PinchPartType3 _type3)
        {
            //根据Type寻找对应文件，根据上面的Component生成Prefab
            PinchPartType2 _type2 = type3Type2Dic[_type3];
            PinchPartType _type = pinchFaceManager.pinchPartType2Dic[_type2];
            PinchPartType1 _type1 = _type.pinchPartType1;
            
            //OC/Character/PinchFace/Prefabs/
            //5_Common_PinchType1/20_FaceDress_PinchType2/45_FD_FaceDress_PinchType3
            string pathFore = "OC/Character/PinchFace/Prefabs";
            string pathTemplate = "TemplatePinchType.Prefab";
            string type2Path = pinchFaceManager.pinchFaceHelper.GetType2Path(_type2);
            string templatePath = $"{pathFore}/{type2Path}/{pathTemplate}";

            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(templatePath)
                .Completed += (handle) =>
            {
                var _comps = handle.Result.GetComponents<IPinchSettingComp>();
                pinchParts.Add(new PinchPart(this,_type3,_type2,_comps,containerTransf));
            };
            //加载Type3，是否有com
        }


        #endregion
    }
}