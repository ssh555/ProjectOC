using OpenCover.Framework.Model;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnList;
using static ML.Engine.UI.UIBtnListContainer;
using static ML.Engine.UI.UIBtnListContainerInitor;
using static ML.Engine.UI.UIBtnListInitor;

namespace ML.Engine.UI
{
    [ShowInInspector]
    public class UIBtnListContainer
    {

        public struct BtnListinitData1
        {
            public int i;
            public int j;
            public bool isloop;
            public bool hasinitselect;
            public string prefabpath;
            public BtnListinitData1(int i,int j, bool isloop, bool hasinitselect, string prefabpath)
            {
                this.i = i;
                this.j = j;
                this.isloop = isloop;
                this.hasinitselect = hasinitselect;
                this.prefabpath = prefabpath;
            }
        }

        public struct BtnListinitData2
        {
            public Transform transform;
            public int j;
            public bool isloop;
            public bool hasinitselect;
            public BtnListinitData2(Transform transform, int j, bool isloop, bool hasinitselect)
            {
                this.transform = transform;
                this.j = j;
                this.isloop = isloop;
                this.hasinitselect = hasinitselect;
            }
        }
        [ShowInInspector]
        private List<UIBtnList> uIBtnLists = new List<UIBtnList>();
        public List<UIBtnList> UIBtnLists { get { return uIBtnLists; } }

        private UIBtnList curSelectUIBtnList;
        public UIBtnList CurSelectUIBtnList { 
            set 
            {
                curSelectUIBtnList = value;
               
            }
            get { return curSelectUIBtnList; }
        }

        //private SelectedButton curSelectSelectedButton = null;
        //private SelectedButton lastSelectSelectedButton = null;
        public enum NavagationMode
        {
            BtnList = 0,
            SelectedButton
        }

        private NavagationMode navagationMode = NavagationMode.BtnList;
        public NavagationMode CurnavagationMode { set { navagationMode = value; } get { return navagationMode; } }
        public enum BindType
        {
            started = 0,
            performed,
            canceled
        }

        private ContainerType gridNavagationType;

        public ContainerType Grid_NavagationType { set {  }  get { return gridNavagationType; } }

        private InputAction gridNavagationInputAction;
        public InputAction curGridNavagationInputAction { get { return gridNavagationInputAction; } }
        private BindType bindType;
        public BindType curBindType { get { return bindType; } }

        private BtnListContainerInitData btnListContainerInitData;

        public UIBtnListContainer(Transform transform, BtnListContainerInitData btnListContainerInitData)
        {
            this.gridNavagationType = btnListContainerInitData.containerType;
            this.btnListContainerInitData = btnListContainerInitData;
            UIBtnListInitor[] uIBtnListInitors = transform.GetComponentsInChildren<UIBtnListInitor>();

            Dictionary<UIBtnListInitor, UIBtnList> tmpDic = new Dictionary<UIBtnListInitor, UIBtnList>();
            

            for (int i = 0; i < uIBtnListInitors.Length; i++) 
            {
                Transform parent = uIBtnListInitors[i].transform;
                BtnListInitData btnListInitData = uIBtnListInitors[i].btnListInitData;
                UIBtnList uIBtnList = new UIBtnList(parent, btnListInitData)
                {
                    UIBtnListContainer = this
                };
                uIBtnLists.Add(uIBtnList);
                tmpDic.Add(uIBtnListInitors[i], uIBtnList);
            }

            //加入UIBtnList之间的导航关系
            for (int i = 0; i < btnListContainerInitData.navagations.Count; i++) 
            {
                if(btnListContainerInitData.navagations[i].Up!=null&& tmpDic.ContainsKey(btnListContainerInitData.navagations[i].Up))
                {
                    uIBtnLists[i].UpUI = tmpDic[btnListContainerInitData.navagations[i].Up];
                }
                if (btnListContainerInitData.navagations[i].Down != null && tmpDic.ContainsKey(btnListContainerInitData.navagations[i].Down))
                {
                    uIBtnLists[i].DownUI = tmpDic[btnListContainerInitData.navagations[i].Down];
                }
                if (btnListContainerInitData.navagations[i].Left != null && tmpDic.ContainsKey(btnListContainerInitData.navagations[i].Left))
                {
                    uIBtnLists[i].LeftUI = tmpDic[btnListContainerInitData.navagations[i].Left];
                }
                if (btnListContainerInitData.navagations[i].Right != null && tmpDic.ContainsKey(btnListContainerInitData.navagations[i].Right))
                {
                    uIBtnLists[i].RightUI = tmpDic[btnListContainerInitData.navagations[i].Right];
                }
            }
        }

        public void BindNavigationInputAction(InputAction NavigationInputAction, BindType bindType)
        {
            this.gridNavagationInputAction = NavigationInputAction;
            this.bindType = bindType;
            //B类型 
            if (gridNavagationType == ContainerType.B)
            {
                navagationMode = NavagationMode.SelectedButton;

                FindEnterableUIBtnList();
                return;
            }

            //A类型 先绑定BtnListNavagation
            navagationMode = NavagationMode.BtnList;
            switch (bindType)
            {
                case BindType.started:
                    NavigationInputAction.started += BtnListNavagation;
                    break;
                case BindType.performed:
                    NavigationInputAction.performed += BtnListNavagation;
                    break;
                case BindType.canceled:
                    NavigationInputAction.canceled += BtnListNavagation;
                    break;
            }
            FindEnterableUIBtnList();
        }


        public void BindNavigationInputAction()
        {
            if (this.gridNavagationInputAction != null)
            {
                switch (bindType)
                {
                    case BindType.started:
                        this.gridNavagationInputAction.started += BtnListNavagation;
                        break;
                    case BindType.performed:
                        this.gridNavagationInputAction.performed += BtnListNavagation;
                        break;
                    case BindType.canceled:
                        this.gridNavagationInputAction.canceled += BtnListNavagation;
                        break;
                }
            }
        }

        public void DeBindNavigationInputAction()
        {
            if (this.gridNavagationInputAction != null)
            {
                switch (bindType)
                {
                    case BindType.started:
                        this.gridNavagationInputAction.started -= BtnListNavagation;
                        break;
                    case BindType.performed:
                        this.gridNavagationInputAction.performed -= BtnListNavagation;
                        break;
                    case BindType.canceled:
                        this.gridNavagationInputAction.canceled -= BtnListNavagation;
                        break;
                }
            }
        }

        private void BtnListNavagation(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            // 使用 ReadValue<T>() 方法获取附加数据
            string actionMapName = obj.action.actionMap.name;

            var vector2 = obj.ReadValue<UnityEngine.Vector2>();
            float angle = Mathf.Atan2(vector2.x, vector2.y);

            angle = angle * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle = angle + 360;
            }

            if (angle < 45 || angle > 315)
            {
                this.MoveToUp();
            }
            else if (angle > 45 && angle < 135)
            {
                this.MoveToRight();
            }
            else if (angle > 135 && angle < 225)
            {
                this.MoveToDown();
            }
            else if (angle > 225 && angle < 315)
            {
                this.MoveToLeft();
            }

        }


        public List<SelectedButton> GetEdge(UIBtnList uIBtnList,EdgeType edgeType)
        {
            List<SelectedButton> selectedButtons = new List<SelectedButton>();
            List<List<SelectedButton>> btnlist = uIBtnList.GetTwoDimSelectedButtons;
            // 获取行数和列数
            int rowCount = btnlist.Count;
            int colCount = (rowCount > 0) ? btnlist[0].Count : 0;
            switch (edgeType)
            {
                case EdgeType.LP:
                    for (int i = 0; i < rowCount; i++)
                    {
                        selectedButtons.Add(btnlist[rowCount - i - 1][0]);
                    }

                    break;
                case EdgeType.LN:
                    for (int i = 0; i < rowCount; i++)
                    {
                        selectedButtons.Add(btnlist[i][0]);
                    }
                    break;
                case EdgeType.RP:
                    for (int i = 0; i < rowCount; i++)
                    {
                        if(btnlist[i][colCount - 1]!=null)
                        {
                            selectedButtons.Add(btnlist[i][colCount - 1]);
                        }
                        else
                        {
                            for (int j = colCount - 1; j >= 0; j--) 
                            {
                                if(btnlist[i][j] != null)
                                {
                                    selectedButtons.Add(btnlist[i][j]);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case EdgeType.RN:
                    for (int i = 0; i < rowCount; i++)
                    {
                        if (btnlist[rowCount - i - 1][colCount - 1] != null) 
                        {
                            selectedButtons.Add(btnlist[rowCount - i - 1][colCount - 1]);
                        }
                        else
                        {
                            for (int j = colCount - 1; j >= 0; j--)
                            {
                                if (btnlist[rowCount - i - 1][j] != null)
                                {
                                    selectedButtons.Add(btnlist[rowCount - i - 1][j]);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case EdgeType.UP:
                    for (int i = 0; i < colCount; i++)
                    {
                        selectedButtons.Add(btnlist[0][i]);
                    }
                    break; 
                case EdgeType.UN:
                    for (int i = 0; i < colCount; i++)
                    {
                        selectedButtons.Add(btnlist[0][colCount - i - 1]);
                    }
                    break;
                case EdgeType.DP:
                    for (int i = 0; i < colCount; i++)
                    {
                        if (btnlist[rowCount - 1][colCount - i - 1] != null) 
                        {
                            selectedButtons.Add(btnlist[rowCount - 1][colCount - i - 1]);
                        }
                        else
                        {
                            for(int j = colCount - 1;j >= 0; j--)
                            {
                                if(btnlist[j][colCount - i - 1] != null)
                                {
                                    selectedButtons.Add(btnlist[j][colCount - i - 1]);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case EdgeType.DN:
                    for (int i = 0; i < colCount; i++)
                    {
                        if (btnlist[rowCount - 1][i] != null)
                        {
                            selectedButtons.Add(btnlist[rowCount - 1][i]);
                        }
                        else
                        {
                            for (int j = colCount - 1; j >= 0; j--)
                            {
                                if (btnlist[j][i] != null)
                                {
                                    selectedButtons.Add(btnlist[j][i]);
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }
            return selectedButtons;
        }

        //edge1左/上 edge2右/下
        public void LinkTwoEdge(List<SelectedButton> edge1, List<SelectedButton> edge2, LinkType linkType) 
        {
            int l1 = edge1.Count;
            int l2 = edge2.Count;
            if (l1 == 0 || l2 == 0) return;

            if (l1 >= l2)
            {
                for(int i = 0; i < l2; i++) 
                {
                    Navigation navigation = edge1[i].navigation;
                    if(linkType == LinkType.LTR)
                    {
                        navigation.selectOnRight = edge2[i];
                    }
                    else if(linkType == LinkType.UTD)
                    {
                        navigation.selectOnDown = edge2[i];
                    }
                    
                    edge1[i].navigation = navigation;
                    

                    navigation = edge2[i].navigation;
                    if (linkType == LinkType.LTR)
                    {
                        navigation.selectOnLeft = edge1[i];
                    }
                    else if (linkType == LinkType.UTD)
                    {
                        navigation.selectOnUp = edge1[i];
                    }
                    
                    edge2[i].navigation = navigation;
                }
                for(int i = l2; i < l1; i++) 
                {
                    Navigation navigation = edge1[i].navigation;
                    if (linkType == LinkType.LTR)
                    {
                        navigation.selectOnRight = edge2[l2 - 1];
                    }
                    else if (linkType == LinkType.UTD)
                    {
                        navigation.selectOnDown = edge2[l2 - 1];
                    }
                    
                    edge1[i].navigation = navigation;
                }
            }
            else
            {
                for (int i = 0; i < l1; i++)
                {
                    Navigation navigation = edge2[i].navigation;
                    if (linkType == LinkType.LTR)
                    {
                        navigation.selectOnLeft = edge1[i];
                    }
                    else if (linkType == LinkType.UTD)
                    {
                        navigation.selectOnUp = edge1[i];
                    }

                    edge2[i].navigation = navigation;

                    navigation = edge1[i].navigation;
                    if (linkType == LinkType.LTR)
                    {
                        navigation.selectOnRight = edge2[i];
                    }
                    else if (linkType == LinkType.UTD)
                    {
                        navigation.selectOnDown = edge2[i];
                    }

                    edge1[i].navigation = navigation;
                }
                for (int i = l1; i < l2; i++)
                {
                    Navigation navigation = edge2[i].navigation;
                    if (linkType == LinkType.LTR)
                    {
                        navigation.selectOnLeft = edge1[l1 - 1];
                    }
                    else if (linkType == LinkType.UTD)
                    {
                        navigation.selectOnUp = edge1[l1 - 1];
                    }

                    edge2[i].navigation = navigation;
                }
            }
        }

        public void DisableUIBtnListContainer()
        {
            foreach (var btnlist in uIBtnLists)
            {
                btnlist.DeBindInputAction();
                btnlist.DisableBtnList();
            }
            this.DeBindNavigationInputAction();
        }


        public void MoveToBtnList(UIBtnList uIBtnList)
        {
            if (this.CurSelectUIBtnList == uIBtnList || uIBtnList == null) return;
/*            //绑定输入
            uIBtnList.BindNavigationInputAction(this.gridNavagationInputAction, this.bindType);*/

            if(gridNavagationType == ContainerType.A)
            {
                if(navagationMode == NavagationMode.BtnList)
                {
                    this.CurSelectUIBtnList?.OnSelectExit();
                }
                else if(navagationMode == NavagationMode.SelectedButton)
                {
                    this.CurSelectUIBtnList?.OnExitInner();
                }
            }
            else if(gridNavagationType == ContainerType.B)
            {
                //当前退出
                this.CurSelectUIBtnList?.OnExitInner();
            }
            
            //更改CurSelectUIBtnList
            this.CurSelectUIBtnList = uIBtnList;
            //当前进入
            this.CurSelectUIBtnList.OnSelectEnter();
        }

        public void MoveToUp()
        {
            UIBtnList uIBtnList = this.CurSelectUIBtnList.UpUI as UIBtnList;
            if(uIBtnList != null) 
            { 
                MoveToBtnList(uIBtnList); 
            }
            else
            {
                navagationMode = NavagationMode.SelectedButton;
                this.curSelectUIBtnList?.OnEnterInner();
            }
            
        }

        public void MoveToDown()
        {
            UIBtnList uIBtnList = this.CurSelectUIBtnList.DownUI as UIBtnList;
            if (uIBtnList != null)
            {
                MoveToBtnList(uIBtnList);
            }
            else
            {
                navagationMode = NavagationMode.SelectedButton;
                this.curSelectUIBtnList?.OnEnterInner();
            }
        }

        public void MoveToLeft()
        {
            UIBtnList uIBtnList = this.CurSelectUIBtnList.LeftUI as UIBtnList;
            if (uIBtnList != null)
            {
                MoveToBtnList(uIBtnList);
            }
            else
            {
                navagationMode = NavagationMode.SelectedButton;
                this.curSelectUIBtnList?.OnEnterInner();
            }
        }

        public void MoveToRight()
        {
            UIBtnList uIBtnList = this.CurSelectUIBtnList.RightUI as UIBtnList;
            if (uIBtnList != null)
            {
                MoveToBtnList(uIBtnList);
            }
            else
            {
                navagationMode = NavagationMode.SelectedButton;
                this.curSelectUIBtnList?.OnEnterInner();
            }
        }

        public void FindEnterableUIBtnList()
        {
            RefreshEdge();

            for (int i = 0; i < this.UIBtnLists.Count; i++)
            {
                if (this.UIBtnLists[i].BtnCnt>0)
                {
                    MoveToBtnList(this.UIBtnLists[i]);
                    return;
                }
            }
        }

        private void RefreshEdge()
        {
            //连边
            foreach (var linkData in btnListContainerInitData.LinkDatas)
            {
                this.LinkTwoEdge(GetEdge(uIBtnLists[linkData.btnlist1], linkData.btnlist1type), GetEdge(uIBtnLists[linkData.btnlist2], linkData.btnlist2type), linkData.linktype);
            }
        }

    }
}


