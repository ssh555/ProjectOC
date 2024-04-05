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

namespace ML.Engine.UI
{
    [ShowInInspector]
    public class UIBtnListContainer : Selectable
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

        private SelectedButton curSelectSelectedButton = null;
        private SelectedButton lastSelectSelectedButton = null;
        private enum NavagationMode
        {
            BtnList = 0,
            SelectedButton
        }

        private NavagationMode navagationMode = NavagationMode.BtnList;
        public enum BindType
        {
            started = 0,
            performed,
            canceled
        }
        public enum GridNavagationType
        {
            A=0,
            B
        }

        private GridNavagationType gridNavagationType;

        public GridNavagationType Grid_NavagationType { set { Debug.Log("set "+value); }  get { return gridNavagationType; } }

        private InputAction gridNavagationInputAction;
        public InputAction curGridNavagationInputAction { get { return gridNavagationInputAction; } }
        private BindType bindType;
        public BindType curBindType { get { return bindType; } }
        public UIBtnListContainer(List<BtnListinitData1> InitDatas, GridNavagationType gridNavagationType)
        {
            this.gridNavagationType = gridNavagationType;
        }
        public UIBtnListContainer(List<BtnListinitData2> InitDatas, GridNavagationType gridNavagationType)
        {
            this.gridNavagationType = gridNavagationType;
            foreach (var data in InitDatas)
            {
                UIBtnList btnList = new UIBtnList(data.transform, data.j, data.hasinitselect, data.isloop);
                btnList.UIBtnListContainer = this;
                uIBtnLists.Add(btnList);
            }
        }

        public void BindNavigationInputAction(InputAction NavigationInputAction, BindType bindType)
        {
            this.gridNavagationInputAction = NavigationInputAction;
            this.bindType = bindType;
            //B类型 
            if (gridNavagationType == GridNavagationType.B)
            {
                navagationMode = NavagationMode.SelectedButton;
                if (uIBtnLists.Count > 0)
                {
                    curSelectUIBtnList = uIBtnLists[0];
                    uIBtnLists[0].OnSelectEnter();
                }
                return;
            }

            //A类型 先绑定BtnListNavagation
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
        }

        private void BtnListNavagation(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            /*if (navagationMode == NavagationMode.SelectedButton)
            {
                curSelectUIBtnList.BindNavigationInputAction(NavigationInputAction, bindType);
            }
            else
            {
                //TODO
            }*/


        }

        public enum EdgeType
        {
            LP=0,
            LN,
            RP,
            RN,
            UP,
            UN,
            DP,
            DN
        }


        public enum LinkType
        {
            LTR = 0,
            UTD
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
            foreach (var item in selectedButtons)
            {
                Debug.Log("AddEdge " + item.GetHashCode());
            }
            return selectedButtons;
        }

        //edge1左/上 edge2右/下
        public void LinkTwoEdge(List<SelectedButton> edge1, List<SelectedButton> edge2, LinkType linkType) 
        {
            int l1 = edge1.Count;
            int l2 = edge2.Count;
            foreach (SelectedButton button in edge1) { Debug.Log("1 "+button.gameObject.name); }
            foreach (SelectedButton button in edge2) { Debug.Log("2 "+button.gameObject.name); }
            Debug.Log(l1+ " " + l2);
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
        }


        public void MoveToBtnList(UIBtnList uIBtnList)
        {
            Debug.Log(this.CurSelectUIBtnList.GetHashCode() + " " + uIBtnList);
            if (this.CurSelectUIBtnList == uIBtnList) return;

            //绑定输入
            uIBtnList.BindNavigationInputAction(this.gridNavagationInputAction, this.bindType);
            //当前退出
            this.CurSelectUIBtnList.OnExitInner();
            //更改CurSelectUIBtnList
            this.CurSelectUIBtnList = uIBtnList;
            //当前进入
            this.CurSelectUIBtnList.OnSelectEnter();
        }



    }
}


