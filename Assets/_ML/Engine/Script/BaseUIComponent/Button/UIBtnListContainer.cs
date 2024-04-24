using ML.Engine.Manager;
using OpenCover.Framework.Model;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnList;
using static ML.Engine.UI.UIBtnListContainer;
using static ML.Engine.UI.UIBtnListContainerInitor;
using static ML.Engine.UI.UIBtnListInitor;

namespace ML.Engine.UI
{
    [Serializable]
    public class UIBtnListContainer
    {
        [ShowInInspector]
        private List<UIBtnList> uIBtnLists = new List<UIBtnList>();
        public List<UIBtnList> UIBtnLists { get { return uIBtnLists; } }
        [ShowInInspector]
        public int UIBtnListNum { get { return uIBtnLists.Count; } }
        [ShowInInspector]
        private UIBtnList curSelectUIBtnList = null;
        public UIBtnList CurSelectUIBtnList { 
            set 
            {
                curSelectUIBtnList = value;
               
            }
            get { return curSelectUIBtnList; }
        }
        [ShowInInspector]
        public int CurSelectUIBtnListIndex { get { if (curSelectUIBtnList != null && this.UIBtnListIndexDic.ContainsKey(curSelectUIBtnList))
                    {
                    return this.UIBtnListIndexDic[curSelectUIBtnList];
                }
                return -1;


            } }
        
        [ShowInInspector]
        private bool isEnable = false;

        public bool IsEnable { get { return isEnable; } }
        
        public enum NavagationMode
        {
            BtnList = 0,
            SelectedButton
        }
        [ShowInInspector]
        private NavagationMode navagationMode = NavagationMode.BtnList;
        public NavagationMode CurnavagationMode { set { navagationMode = value; } get { return navagationMode; } }
        public enum BindType
        {
            started = 0,
            performed,
            canceled
        }
        [ShowInInspector]
        private ContainerType gridNavagationType;

        public ContainerType Grid_NavagationType { set {  }  get { return gridNavagationType; } }

        private InputAction gridNavagationInputAction;
        public InputAction curGridNavagationInputAction { get { return gridNavagationInputAction; } }
        private BindType bindType;
        public BindType curBindType { get { return bindType; } }

        [ShowInInspector]
        private BtnListContainerInitData btnListContainerInitData;

        [ShowInInspector]
        private bool isEmpty;

        public bool IsEmpty { get { return isEmpty; } }

        private Transform parent;
        public void RefreshIsEmpty()
        {
            foreach (var btnlist in this.uIBtnLists)
            {
                if(btnlist.IsEmpty == false)
                {
                    this.isEmpty = false;
                    return;
                }
            }
            this.isEmpty = true;
        }

        private event Action OnSelectButtonChanged = null;
        private event Action OnSelectButtonListChanged = null;

        public void AddOnSelectButtonChangedAction(Action action)
        {
            if (action != null)
            {
                this.OnSelectButtonChanged += action;
            }
        }

        public void AddOnSelectButtonListChangedAction(Action action)
        {
            if (action != null)
            {
                this.OnSelectButtonListChanged += action;
            }
        }

        public void InvokeOnSelectButtonChanged()
        {
            this.OnSelectButtonChanged?.Invoke();
        }

        public UIBtnListContainer(UIBtnListContainerInitor uIBtnListContainerInitor)
        {
            this.parent = uIBtnListContainerInitor.transform;
            BtnListContainerInitData btnListContainerInitData = uIBtnListContainerInitor.btnListContainerInitData;
            this.gridNavagationType = btnListContainerInitData.containerType;
            this.btnListContainerInitData = btnListContainerInitData;
            this.InitBtnlistInfo();
        }

        [ShowInInspector]
        private Dictionary<UIBtnListInitor, UIBtnList> UIBtnListDic = new Dictionary<UIBtnListInitor, UIBtnList>();
        [ShowInInspector]
        private Dictionary<UIBtnList, int> UIBtnListIndexDic = new Dictionary<UIBtnList, int>();
        
        //������ǰ��btnlist
        public UIBtnList InitBtnlistInfo()
        {
            this.UIBtnListIndexDic.Clear();
            UIBtnListInitor[] uIBtnListInitors = this.parent.GetComponentsInChildren<UIBtnListInitor>();
            UIBtnList uIBtnList = null;
            for (int i = 0; i < uIBtnListInitors.Length; i++)
            {
                if(!UIBtnListDic.ContainsKey(uIBtnListInitors[i])) 
                {
                    uIBtnList = new UIBtnList(uIBtnListInitors[i])
                    {
                        UIBtnListContainer = this
                    };
                    uIBtnLists.Add(uIBtnList);
                    UIBtnListDic.Add(uIBtnListInitors[i], uIBtnList);
                }
                
            }

            for (int i = 0; i < uIBtnListInitors.Length; i++)
            {
                UIBtnListIndexDic.Add(this.uIBtnLists[i], i);
            }


            //if (this.gridNavagationType == ContainerType.A)
            //{
                RefreshBtnListNavagation();
                RefreshEdge();
            //}

            return uIBtnList;
            
        }
        
        //����ˢ������BtnList
        public void RefreshAll()
        {
            this.UIBtnListIndexDic.Clear();
            this.uIBtnLists.Clear();
            this.UIBtnListDic.Clear();
            UIBtnListInitor[] uIBtnListInitors = this.parent.GetComponentsInChildren<UIBtnListInitor>();
            UIBtnList uIBtnList = null;
            for (int i = 0; i < uIBtnListInitors.Length; i++)
            {
                if(!UIBtnListDic.ContainsKey(uIBtnListInitors[i])) 
                {
                    uIBtnList = new UIBtnList(uIBtnListInitors[i])
                    {
                        UIBtnListContainer = this
                    };
                    uIBtnLists.Add(uIBtnList);
                    UIBtnListDic.Add(uIBtnListInitors[i], uIBtnList);
                }
            }
            for (int i = 0; i < uIBtnListInitors.Length; i++)
            {
                UIBtnListIndexDic.Add(this.uIBtnLists[i], i);
            }
            RefreshBtnListNavagation();
            RefreshEdge();
        }

        public void BindNavigationInputAction(InputAction NavigationInputAction, BindType bindType)
        {
            this.isEnable = true;
            this.gridNavagationInputAction = NavigationInputAction;
            this.bindType = bindType;
            //B���� 
            if (gridNavagationType == ContainerType.B)
            {
                navagationMode = NavagationMode.SelectedButton;

                FindEnterableUIBtnList();
                return;
            }

            //A���� �Ȱ�BtnListNavagation
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

            // ʹ�� ReadValue<T>() ������ȡ��������
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
            // ��ȡ����������
            int rowCount = btnlist.Count;
            int colCount = (rowCount > 0) ? btnlist[0].Count : 0;
            switch (edgeType)
            {
                case EdgeType.���˳ʱ��:
                    for (int i = 0; i < rowCount; i++)
                    {
                        if (btnlist[rowCount - i - 1][0] != null) 
                        {
                            selectedButtons.Add(btnlist[rowCount - i - 1][0]);
                        }
                        else
                        {
                            for (int j = 0; j < colCount; j++) 
                            {
                                if (btnlist[rowCount - i - 1][j] != null)
                                {
                                    if (btnlist[rowCount - i - 1][j] != null)
                                    {
                                        selectedButtons.Add(btnlist[rowCount - i - 1][j]);
                                        break;
                                    }
                                }
                            }
                        }
                        
                    }

                    break;
                case EdgeType.�����ʱ��:
                    for (int i = 0; i < rowCount; i++)
                    {
                        if (btnlist[i][0] != null)
                        {
                            selectedButtons.Add(btnlist[i][0]);
                        }
                        else
                        {
                            for (int j = 0; j < colCount; j++)
                            {
                                if (btnlist[i][j] != null)
                                {
                                    if (btnlist[i][j] != null)
                                    {
                                        selectedButtons.Add(btnlist[i][j]);
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    break;
                case EdgeType.�Ҳ�˳ʱ��:
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
                case EdgeType.�Ҳ���ʱ��:
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
                case EdgeType.�ϲ�˳ʱ��:
                    for (int i = 0; i < colCount; i++)
                    {
                        if (btnlist[0][i] != null)
                        {
                            selectedButtons.Add(btnlist[0][i]);
                        }
                        else
                        {
                            for (int j = 0; j < rowCount; j++)
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
                case EdgeType.�ϲ���ʱ��:
                    for (int i = 0; i < colCount; i++)
                    {
                        if (btnlist[0][colCount - i - 1] != null)
                        {
                            selectedButtons.Add(btnlist[0][colCount - i - 1]);
                        }
                        else
                        {
                            for (int j = 0; j < rowCount; j++)
                            {
                                if (btnlist[j][colCount - i - 1] != null)
                                {
                                    selectedButtons.Add(btnlist[j][colCount - i - 1]);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case EdgeType.�²�˳ʱ��:
                    for (int i = 0; i < colCount; i++)
                    {
                        if (btnlist[rowCount - 1][colCount - i - 1] != null) 
                        {
                            selectedButtons.Add(btnlist[rowCount - 1][colCount - i - 1]);
                        }
                        else
                        {
                            for(int j = rowCount - 1;j >= 0; j--)
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
                case EdgeType.�²���ʱ��:
                    for (int i = 0; i < colCount; i++)
                    {
                        if (btnlist[rowCount - 1][i] != null)
                        {
                            selectedButtons.Add(btnlist[rowCount - 1][i]);
                        }
                        else
                        {
                            for (int j = rowCount - 1; j >= 0; j--)
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

        //edge1��/�� edge2��/��
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
                    if(linkType == LinkType.��������)
                    {
                        navigation.selectOnRight = edge2[i];
                    }
                    else if(linkType == LinkType.��������)
                    {
                        navigation.selectOnDown = edge2[i];
                    }
                    
                    edge1[i].navigation = navigation;
                    

                    navigation = edge2[i].navigation;
                    if (linkType == LinkType.��������)
                    {
                        navigation.selectOnLeft = edge1[i];
                    }
                    else if (linkType == LinkType.��������)
                    {
                        navigation.selectOnUp = edge1[i];
                    }
                    
                    edge2[i].navigation = navigation;
                }
                for(int i = l2; i < l1; i++) 
                {
                    Navigation navigation = edge1[i].navigation;
                    if (linkType == LinkType.��������)
                    {
                        navigation.selectOnRight = edge2[l2 - 1];
                    }
                    else if (linkType == LinkType.��������)
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
                    if (linkType == LinkType.��������)
                    {
                        navigation.selectOnLeft = edge1[i];
                    }
                    else if (linkType == LinkType.��������)
                    {
                        navigation.selectOnUp = edge1[i];
                    }

                    edge2[i].navigation = navigation;

                    navigation = edge1[i].navigation;
                    if (linkType == LinkType.��������)
                    {
                        navigation.selectOnRight = edge2[i];
                    }
                    else if (linkType == LinkType.��������)
                    {
                        navigation.selectOnDown = edge2[i];
                    }

                    edge1[i].navigation = navigation;
                }
                for (int i = l1; i < l2; i++)
                {
                    Navigation navigation = edge2[i].navigation;
                    if (linkType == LinkType.��������)
                    {
                        navigation.selectOnLeft = edge1[l1 - 1];
                    }
                    else if (linkType == LinkType.��������)
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
            //Debug.Log("this.isEnable == false " + (this.isEnable == false) + " this.CurSelectUIBtnList == uIBtnList " + (this.CurSelectUIBtnList == uIBtnList) + " uIBtnList == null " + (uIBtnList == null));
            if (this.isEnable == false || this.CurSelectUIBtnList == uIBtnList || uIBtnList == null || uIBtnList.IsEmpty) return;
            
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
                //��ǰ�˳�
                this.CurSelectUIBtnList?.OnExitInner();
            }

            if (this.CurSelectUIBtnList != null) 
            {
                this.OnSelectButtonListChanged?.Invoke();
            }

            //����CurSelectUIBtnList
            this.CurSelectUIBtnList = uIBtnList;
            
            //��ǰ����
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

            if(this.gridNavagationType == ContainerType.B)
            {
                for (int i = 0; i < this.UIBtnLists.Count; i++)
                {
                    if (this.UIBtnLists[i].BtnCnt > 0)
                    {
                        MoveToBtnList(this.UIBtnLists[i]);
                        return;
                    }
                }
            }
            else if(this.gridNavagationType == ContainerType.A)
            {
                for (int i = 0; i < this.UIBtnLists.Count; i++)
                {
                    if (this.UIBtnLists[i] != null) 
                    {
                        MoveToBtnList(this.UIBtnLists[i]);
                        return;
                    }
                }
            }

            
        }

        public void RefreshEdge()
        {
            //����
            if (this.uIBtnLists.Count == 0) return;
            
            if(gridNavagationType == ContainerType.B)
            {
                //B���������ݹ̶�
                foreach (var linkData in btnListContainerInitData.LinkDatas)
                {
                    this.LinkTwoEdge(GetEdge(uIBtnLists[linkData.btnlist1], linkData.btnlist1type), GetEdge(uIBtnLists[linkData.btnlist2], linkData.btnlist2type), linkData.linktype);
                }
            }
            else if(gridNavagationType == ContainerType.A)
            {
                //A���������ݸ��� isHorizontal������
                if(this.btnListContainerInitData.isHorizontal)
                {
                    for (int i = 0; i < this.uIBtnLists.Count - 1; i++)
                    {
                        this.LinkTwoEdge(GetEdge(uIBtnLists[i], EdgeType.�Ҳ�˳ʱ��), GetEdge(uIBtnLists[i + 1], EdgeType.�����ʱ��), LinkType.��������);
                    }
                    if(this.btnListContainerInitData.isLoop)
                    {
                        this.LinkTwoEdge(GetEdge(uIBtnLists[this.uIBtnLists.Count - 1], EdgeType.�Ҳ�˳ʱ��), GetEdge(uIBtnLists[0], EdgeType.�����ʱ��), LinkType.��������);
                    }
                }
                else
                {
                    for (int i = 0; i < this.uIBtnLists.Count - 1; i++)
                    {
                        this.LinkTwoEdge(GetEdge(uIBtnLists[i], EdgeType.�²���ʱ��), GetEdge(uIBtnLists[i + 1], EdgeType.�ϲ�˳ʱ��), LinkType.��������);
                    }
                    if (this.btnListContainerInitData.isLoop)
                    {
                        this.LinkTwoEdge(GetEdge(uIBtnLists[this.uIBtnLists.Count - 1], EdgeType.�²���ʱ��), GetEdge(uIBtnLists[0], EdgeType.�ϲ�˳ʱ��), LinkType.��������);
                    }
                }
            }
        }
        
        public void AddBtn(int BtnListIndex, string prefabpath, UnityAction BtnAction = null, Action OnSelectEnter = null, Action OnSelectExit = null, UnityAction<SelectedButton> BtnSettingAction = null, string BtnText = null)
        {
            if (BtnListIndex >= 0 || BtnListIndex < this.uIBtnLists.Count)
            {
                this.uIBtnLists[BtnListIndex].AddBtn(prefabpath, BtnAction, OnSelectEnter, OnSelectExit, BtnSettingAction, BtnText:BtnText);
            }
            else
            {
                Debug.LogError("�����ڸ�BtnList!");
            }
        }

        public void DeleteBtn(int BtnListIndex,int SelectedButtonIndex)
        {
            if ((BtnListIndex >= 0 || BtnListIndex < this.uIBtnLists.Count) && (SelectedButtonIndex >= 0 && SelectedButtonIndex < this.uIBtnLists[BtnListIndex].BtnCnt))
            {
                this.uIBtnLists[BtnListIndex].DeleteButton(SelectedButtonIndex);
               
            }
            else
            {
                Debug.LogError("�����ڸ�BtnList��SelectedButton!");
            }
        }

        public AsyncOperationHandle<GameObject> AddBtnListAType(string prefabpath,InputAction inputAction = null,BindType bindType = BindType.started,List<UnityAction> actions = null)
        {
            var ans = Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(prefabpath);
                ans.Completed += (handle) =>
            {
                var btnlist = handle.Result.GetComponent<UIBtnListInitor>();
                btnlist.gameObject.name = btnlist.GetHashCode().ToString();
                btnlist.transform.SetParent(this.parent, false);

                //��btnlist�е�btn����ص�
                var btns = btnlist.GetComponentsInChildren<SelectedButton>();

                if(actions.Count == btns.Length)
                {
                    for (int i = 0; i < btns.Length; i++)
                    {
                        btns[i].onClick.AddListener(actions[i]);
                    }
                }
                else
                {
                    Debug.LogError("��ť������ص�������ƥ�䣡");
                }

                bool needMoveToBtnList = this.IsEmpty;
                if (inputAction != null)
                {
                    UIBtnList uIBtnList = InitBtnlistInfo();
                    uIBtnList.BindButtonInteractInputAction(inputAction, bindType);
                }
                
                if (needMoveToBtnList)FindEnterableUIBtnList();
                RefreshIsEmpty();
            };
                return ans;
        }


        public void AddBtnListBType(string prefabpath, LinkData linkData,InputAction inputAction = null,BindType bindType = BindType.started)
        {
            Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(prefabpath).Completed += (handle) =>
            {
                var btnlist = handle.Result.GetComponent<UIBtnListInitor>();
                btnlist.gameObject.name = btnlist.GetHashCode().ToString();
                btnlist.transform.SetParent(this.parent, false);
                if (linkData.btnlist1 >= 0 && linkData.btnlist2 >= 0) 
                {
                    this.btnListContainerInitData.AddLinkData(linkData);
                }
                UIBtnList uIBtnList = InitBtnlistInfo();
                if (inputAction != null)
                {
                    uIBtnList.BindButtonInteractInputAction(inputAction, bindType);
                }
            }; 
        }

        public void RefreshBtnListContainer(BtnListContainerInitData btnListContainerInitData)
        {
            this.btnListContainerInitData = btnListContainerInitData;
            RefreshAll();
        }
        

        public void DeleteBtnList(int BtnListIndex)
        {
            if (BtnListIndex >= 0 || BtnListIndex < this.uIBtnLists.Count)
            {
                
                
                GameManager.Instance.StartCoroutine(DestroyAndRefreshBtnList(BtnListIndex));
            }
            else
            {
                Debug.LogError("�����ڸ�BtnList!");
            }
        }

        public void SetEmptyAllBtnList(Action OnResetFinish)
        {
            GameManager.Instance.StartCoroutine(DestroyAndRefreshBtnList(OnResetFinish));
        }
        private IEnumerator DestroyAndRefreshBtnList(int BtnListIndex)
        {
            bool NeedMoveToBtnList = false;
            if (this.uIBtnLists[BtnListIndex] == this.curSelectUIBtnList)
            {
                NeedMoveToBtnList = true;
            }
            for (int i = 0; i < this.UIBtnListDic.Count; i++)
            {
                if (this.UIBtnListDic.ElementAt(i).Value == this.uIBtnLists[BtnListIndex])
                {
                    UIBtnListDic.Remove(this.UIBtnListDic.ElementAt(i).Key);
                    UIBtnListIndexDic.Remove(this.uIBtnLists[BtnListIndex]);
                    uIBtnLists.RemoveAt(BtnListIndex);
                }
            }

            //��������
            GameManager.DestroyObj(this.parent.GetChild(BtnListIndex).gameObject);

            // �ȴ�һ֡ ԭ���ǵ�ǰ֡���� transform.childCount�ڵ�ǰ֡���������ϸ��£���Ҫ��һ֡
            yield return null;

            // ����һ֡����BtnListContainer
            if (NeedMoveToBtnList)
            {
                this.curSelectUIBtnList = null;
                FindEnterableUIBtnList();
            }
                
            InitBtnlistInfo();
            RefreshIsEmpty();
        }
        private IEnumerator DestroyAndRefreshBtnList(Action OnResetFinish)
        {
            for (int i = 0; i < this.UIBtnListDic.Count; i++)
            {
                UIBtnListDic.Clear();
                UIBtnListIndexDic.Clear();
                uIBtnLists.Clear();
            }

            SelectedButton[] selectedButtons = this.parent.GetComponentsInChildren<SelectedButton>();
            for(int i = 0;i < selectedButtons.Length;i++)
            {
                GameManager.DestroyObj(selectedButtons[i].gameObject);
            }
            // �ȴ�һ֡ ԭ���ǵ�ǰ֡���� transform.childCount�ڵ�ǰ֡���������ϸ��£���Ҫ��һ֡
            yield return null;

            InitBtnlistInfo();
            RefreshIsEmpty();
            OnResetFinish?.Invoke();
        }
        public void RefreshBtnListNavagation()
        {
            if (this.uIBtnLists.Count == 0) return;
            if (this.btnListContainerInitData.isHorizontal)
            {
                for (int i = 0; i < this.uIBtnLists.Count - 1; i++)
                {
                    uIBtnLists[i].RightUI = uIBtnLists[i + 1];
                    uIBtnLists[i + 1].LeftUI = uIBtnLists[i];
                }
                if (this.btnListContainerInitData.isLoop)
                {
                    uIBtnLists[0].LeftUI = uIBtnLists[this.uIBtnLists.Count - 1];
                    uIBtnLists[this.uIBtnLists.Count - 1].RightUI = uIBtnLists[0];
                }
            }
            else
            {
                for (int i = 0; i < this.uIBtnLists.Count - 1; i++)
                {
                    uIBtnLists[i].DownUI = uIBtnLists[i + 1];
                    uIBtnLists[i + 1].UpUI = uIBtnLists[i];
                }
                if (this.btnListContainerInitData.isLoop)
                {
                    uIBtnLists[0].UpUI = uIBtnLists[this.uIBtnLists.Count - 1];
                    uIBtnLists[this.uIBtnLists.Count - 1].DownUI = uIBtnLists[0];
                }
            }
        }
        
        /// <summary>
        /// �ú�������Ϊ����BtnListContainer
        /// </summary>
        public void SetIsEnableTrue()
        {
            this.isEnable = true;
        }

        /// <summary>
        /// �ú�������Ϊ����BtnListContainer
        /// </summary>
        public void SetIsEnableFalse()
        {
            this.isEnable = false;
        }
    }
}


