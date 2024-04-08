using ML.Engine.Manager;
using OpenCover.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnListContainer;
using Sirenix.OdinInspector;
using static ML.Engine.UI.UIBtnListContainerInitor;
using static ML.Engine.UI.UIBtnListInitor;

namespace ML.Engine.UI
{
    public class UIBtnList : ISelected
    {
        #region ISelected
        public ISelected LeftUI { get ; set ; }
        public ISelected RightUI { get; set; }
        public ISelected UpUI { get; set; }
        public ISelected DownUI { get; set; }
        #endregion

        [ShowInInspector]
        private List<List<SelectedButton>> TwoDimSelectedButtons = new List<List<SelectedButton>>();//二维列表

        public List<List<SelectedButton>> GetTwoDimSelectedButtons { get { return TwoDimSelectedButtons; } }
        private int OneDimCnt = 0;
        public int BtnCnt {  get { return OneDimCnt; } }
        private int TwoDimH = 0;
        private int TwoDimW = 0;
        [ShowInInspector]
        private SelectedButton CurSelected;

        //Index
        private int TwoDimI = 0;
        private int TwoDimJ = 0;

        private InputAction NavigationInputAction = null;
        private Action NavigationPreAction = null;
        private Action NavigationPostAction = null;

        private InputAction ButtonInteractInputAction = null;
        private Action ButtonInteractPreAction = null;
        private Action ButtonInteractPostAction = null;
        private BindType NavigationBindType;
        private BindType ButtonInteractBindType;

        private bool isEnable = false;

        private Transform parent;
        public Transform Parent { get { return parent; } }

        private Transform Selected;
        private UIBtnListContainer uiBtnListContainer;
        public UIBtnListContainer UIBtnListContainer { set { uiBtnListContainer = value; } get { return uiBtnListContainer; } }

        private int limitNum;
        private bool hasInitSelect;
        private bool isLoop;
        private bool isWheel;

        private bool NeedToResetCurSelected = false;
        private bool isEmpty { set { this.uiBtnListContainer?.RefreshIsEmpty(); } get { return isEmpty; } }
        public bool IsEmpty { get { return isEmpty; } }

        /// <summary>
        /// btnName,SelectedButton
        /// </summary>
        private Dictionary<string, SelectedButton> SBDic = new Dictionary<string, SelectedButton>();
        /// <summary>
        /// SelectedButton,BtnPos
        /// </summary>
        private Dictionary<SelectedButton, (int, int)> SBPosDic = new Dictionary<SelectedButton, (int, int)>();

        private Dictionary<(InputAction, BindType), Action<InputAction.CallbackContext>> InputActionBindDic = new Dictionary<(InputAction, BindType), Action<InputAction.CallbackContext>>();

        /// <summary>
        /// parent: 按钮父物体 limitNum：一行多少个按钮 hasInitSelect:是否有初始选中 isLoop:是否为循环按钮 isWheel：是否为轮转按钮 OnSelectedEnter：选中回调 OnSelectedExit：选出回调
        /// </summary>
        public UIBtnList(Transform parent, int limitNum = 1, bool hasInitSelect = true, bool isLoop = false, bool isWheel = false, Action OnSelectedEnter = null, Action OnSelectedExit = null)
        {

            this.parent = parent;
            this.limitNum = limitNum;
            this.hasInitSelect = hasInitSelect;
            this.isLoop = isLoop;
            this.isWheel = isWheel;
            if (parent != null)
            {
                InitBtnInfo(parent, limitNum, hasInitSelect, isLoop, isWheel, OnSelectedEnter, OnSelectedExit);
                try
                {
                    this.Selected = parent.Find("Selected");
                    this.Selected.gameObject.SetActive(false);
                }
                catch { }
            }
        }

        public UIBtnList(UIBtnListInitor uIBtnListInitor)
        {
            BtnListInitData btnListInitData = uIBtnListInitor.btnListInitData;
            this.parent = uIBtnListInitor.transform;
            this.limitNum = btnListInitData.limitNum;
            this.hasInitSelect = btnListInitData.hasInitSelect;
            this.isLoop = btnListInitData.isLoop;
            this.isWheel = btnListInitData.isWheel;
            if (parent != null)
            {
                InitBtnInfo(parent, limitNum, hasInitSelect, isLoop, isWheel, null, null);
                try
                {
                    this.Selected = parent.Find("Selected");
                    this.Selected.gameObject.SetActive(false);
                }
                catch { }
            }
        }

        public void InitBtnInfo(Transform parent, int limitNum = 1, bool hasInitSelect = true, bool isLoop = false, bool isWheel = false, Action OnSelectedEnter = null, Action OnSelectedExit = null)
        {
            this.SBDic.Clear();
            this.SBPosDic.Clear();
            this.TwoDimSelectedButtons.Clear();



            SelectedButton[] OneDimSelectedButtons = parent.GetComponentsInChildren<SelectedButton>(true);
            this.OneDimCnt = OneDimSelectedButtons.Length;

            if(this.OneDimCnt == 0)
            {
                this.CurSelected = null;
                this.isEmpty = true;
                this.UIBtnListContainer?.FindEnterableUIBtnList();
            }
            else
            {
                this.isEmpty = false;
            }

            foreach (var btn in OneDimSelectedButtons)
            {
                btn.SetUIBtnList(this);
                btn.Init(OnSelectedEnter, OnSelectedExit);
                Navigation navigation = btn.navigation;
                navigation.mode = Navigation.Mode.None;
                btn.navigation = navigation;
                SBDic.Add(btn.gameObject.name, btn);
            }

            this.TwoDimW = limitNum;
            this.TwoDimH = OneDimCnt % TwoDimW == 0 ? OneDimCnt / TwoDimW : OneDimCnt / TwoDimW + 1;
            SelectedButton[,] TwoDimSelectedButtons = new SelectedButton[TwoDimH, TwoDimW];
            int cnt = 0;
            for (int i = 0; i < TwoDimH; i++)
            {
                for (int j = 0; j < TwoDimW; j++)
                {
                    
                    if (cnt >= OneDimCnt)
                    {
                        TwoDimSelectedButtons[i, j] = null;
                    }
                    else
                    {
                        SBPosDic.Add(OneDimSelectedButtons[cnt], (i, j));
                        TwoDimSelectedButtons[i, j] = OneDimSelectedButtons[cnt++];
                    }
                }
            }

            //设置二维按钮相对位置
            for (int i = 0; i < TwoDimH; i++)
            {
                for (int j = 0; j < TwoDimW; j++)
                {
                    if (TwoDimSelectedButtons[i, j] == null) continue;
                    Navigation navigation = TwoDimSelectedButtons[i, j].navigation;

                    if (isLoop)
                    {
                        navigation.selectOnUp = i - 1 >= 0 ? TwoDimSelectedButtons[i - 1, j] : TwoDimSelectedButtons[TwoDimH - 1, j];
                        navigation.selectOnDown = i + 1 < TwoDimH ? TwoDimSelectedButtons[i + 1, j] : TwoDimSelectedButtons[0, j];
                        navigation.selectOnRight = j + 1 < TwoDimW ? TwoDimSelectedButtons[i, j + 1] : TwoDimSelectedButtons[i, 0];
                        navigation.selectOnLeft = j - 1 >= 0 ? TwoDimSelectedButtons[i, j - 1] : TwoDimSelectedButtons[i, TwoDimW - 1];
                    }
                    else
                    {
                        navigation.selectOnUp = i - 1 >= 0 ? TwoDimSelectedButtons[i - 1, j] : TwoDimSelectedButtons[i, j];
                        navigation.selectOnDown = i + 1 < TwoDimH ? TwoDimSelectedButtons[i + 1, j] : TwoDimSelectedButtons[i, j];
                        navigation.selectOnRight = j + 1 < TwoDimW ? TwoDimSelectedButtons[i, j + 1] : TwoDimSelectedButtons[i, j];
                        navigation.selectOnLeft = j - 1 >= 0 ? TwoDimSelectedButtons[i, j - 1] : TwoDimSelectedButtons[i, j];
                    }

                    TwoDimSelectedButtons[i, j].navigation = navigation;
                }
            }

            if (OneDimCnt > 0 && (hasInitSelect||NeedToResetCurSelected)) 
            {
                //初始化选择对象
                this.TwoDimI = 0;
                this.TwoDimJ = 0;
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
                this.CurSelected.OnSelect(null);
                this.NeedToResetCurSelected = false;
            }


            for (int i = 0; i < TwoDimSelectedButtons.GetLength(0); i++) // 遍历行
            {
                List<SelectedButton> row = new List<SelectedButton>();
                for (int j = 0; j < TwoDimSelectedButtons.GetLength(1); j++) // 遍历列
                {
                    row.Add(TwoDimSelectedButtons[i, j]);
                }
                this.TwoDimSelectedButtons.Add(row);
            }

        }

        /// <summary>
        /// 加入按钮
        /// </summary>
        public void AddBtn(string prefabpath)
        {
            /*if (selectedButton == null) return;
            int i = OneDimCnt / limitNum;
            int j = OneDimCnt % limitNum;
            
            if (i == TwoDimH)
            {
                List<SelectedButton> selectedButtons = new List<SelectedButton>();
                for (int k = 0; k < TwoDimW; k++) selectedButtons.Add(null);
                TwoDimSelectedButtons.Add(selectedButtons);
                ++TwoDimH;
            }

            TwoDimSelectedButtons[i][j] = selectedButton;
            Navigation navigation = TwoDimSelectedButtons[i][j].navigation;

            if (isLoop)
            {
                navigation.selectOnUp = i - 1 >= 0 ? TwoDimSelectedButtons[i - 1][j] : TwoDimSelectedButtons[TwoDimH - 1][j];
                navigation.selectOnDown = i + 1 < TwoDimH ? TwoDimSelectedButtons[i + 1][j] : TwoDimSelectedButtons[0][j];
                navigation.selectOnRight = j + 1 < TwoDimW ? TwoDimSelectedButtons[i][j + 1] : TwoDimSelectedButtons[i][0];
                navigation.selectOnLeft = j - 1 >= 0 ? TwoDimSelectedButtons[i][j - 1] : TwoDimSelectedButtons[i][TwoDimW - 1];
            }
            else
            {
                navigation.selectOnUp = i - 1 >= 0 ? TwoDimSelectedButtons[i - 1][j] : TwoDimSelectedButtons[i][j];
                navigation.selectOnDown = i + 1 < TwoDimH ? TwoDimSelectedButtons[i + 1][j] : TwoDimSelectedButtons[i][j];
                navigation.selectOnRight = j + 1 < TwoDimW ? TwoDimSelectedButtons[i][j + 1] : TwoDimSelectedButtons[i][j];
                navigation.selectOnLeft = j - 1 >= 0 ? TwoDimSelectedButtons[i][j - 1] : TwoDimSelectedButtons[i][j];
            }
            TwoDimSelectedButtons[i][j].navigation = navigation;
            ++OneDimCnt;
            this.UIBtnListContainer?.RefreshEdge();*/
            Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(prefabpath).Completed += (handle) =>
            {


                // 实例化
                var btn = handle.Result.GetComponent<SelectedButton>();
                btn.gameObject.name = btn.GetHashCode().ToString();
                btn.transform.SetParent(this.parent.Find("Container"), false);
                InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel, null, null);

                this.UIBtnListContainer?.RefreshEdge();
/*                if (this.uiBtnListContainer.IsEmpty)
                {
                    this.uiBtnListContainer.MoveToBtnList(this);
                }*/



            };
            
        }

        public void DeleteButton(int SelectedButtonIndex)
        {
            /*int rowIndex = index / limitNum;
            int colIndex = index % limitNum;


            if (rowIndex < this.TwoDimSelectedButtons.Count && colIndex < this.TwoDimSelectedButtons[rowIndex].Count)
            {
                GameObject gameObject = TwoDimSelectedButtons[rowIndex][colIndex].gameObject;
                GameManager.DestroyObj(gameObject);
                this.TwoDimSelectedButtons[rowIndex].RemoveAt(colIndex);
            }

            this.InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
            this.UIBtnListContainer?.RefreshEdge();*/
            int i = SelectedButtonIndex / TwoDimW;
            int j = SelectedButtonIndex % TwoDimW;
            if (this.TwoDimSelectedButtons[i][j] == this.CurSelected)
            {
                this.NeedToResetCurSelected = true;
            }
            GameManager.Instance.StartCoroutine(DestroyAndRefreshBtnList(SelectedButtonIndex));
        }

        private IEnumerator DestroyAndRefreshBtnList(int SelectedButtonIndex)
        {
            //销毁物体
            GameManager.DestroyObj(this.parent.Find("Container").GetChild(SelectedButtonIndex).gameObject);

            // 等待一帧 原因是当前帧销毁 transform.childCount在当前帧并不会马上更新，需要下一帧
            yield return null;

            // 在下一帧更新BtnList
            InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel, null, null);
            this.UIBtnListContainer?.RefreshEdge();
        }

        public void Check()
        {
            InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel, null, null);
            this.UIBtnListContainer?.RefreshEdge();
        }
        

        /// <summary>
        /// 传入下标设置二维按钮列表action
        /// </summary>
        public void SetBtnAction(int i, int j, UnityAction action)
        {
            this.TwoDimSelectedButtons[i][j].onClick.AddListener(action);
        }
        /// <summary>
        /// 传入字符串设置按钮列表action
        /// </summary>
        public void SetBtnAction(string BtnName, UnityAction action)
        {
            if (this.SBDic.ContainsKey(BtnName))
            {
                SBDic[BtnName].onClick.AddListener(action);
            }
        }
        /// <summary>
        /// 获取指定按钮
        /// </summary>
        public SelectedButton GetBtn(string BtnName)
        {
            if (this.SBDic.ContainsKey(BtnName))
            {
                return SBDic[BtnName];
            }
            return null;
        }
        /// <summary>
        /// 获取当前选中按钮
        /// </summary>
        public SelectedButton GetCurSelected()
        {
            return this.CurSelected;
        }
        /// <summary>
        /// 获取当前选中按钮
        /// </summary>
        public void SetCurSelected(SelectedButton selectedButton)
        {
            this.CurSelected = selectedButton;
        }
        /// <summary>
        /// 置空当前选中按钮
        /// </summary>
        public void SetCurSelectedNull()
        {
            this.CurSelected?.OnDeselect(null);
            this.CurSelected = null;
        }
        /// <summary>
        /// 向上
        /// </summary>
        public SelectedButton MoveUPIUISelected()
        {
            if(CurSelected == null)return null;
            SelectedButton selectedButton = CurSelected.navigation.selectOnUp as SelectedButton;
            if (selectedButton == CurSelected)
            {
                if(this.uiBtnListContainer?.Grid_NavagationType == ContainerType.A)
                {
                    this.uiBtnListContainer.CurnavagationMode = NavagationMode.BtnList;
                    this.OnExitInner();
                }
                return null;
            }
                
            return RefreshSelected(selectedButton);
        }
        /// <summary>
        /// 向下
        /// </summary>
        public SelectedButton MoveDownIUISelected()
        {
            if (CurSelected == null) return null;
            SelectedButton selectedButton = CurSelected.navigation.selectOnDown as SelectedButton;
            if (selectedButton == CurSelected)
            {
                if (this.uiBtnListContainer?.Grid_NavagationType == ContainerType.A)
                {
                    this.uiBtnListContainer.CurnavagationMode = NavagationMode.BtnList;
                    this.OnExitInner();
                }
                return null;
            }

            return RefreshSelected(selectedButton);
        }
        /// <summary>
        /// 向左
        /// </summary>
        public SelectedButton MoveLeftIUISelected()
        {
            if (CurSelected == null) return null;
            SelectedButton selectedButton = CurSelected.navigation.selectOnLeft as SelectedButton;
            if (selectedButton == CurSelected)
            {
                if (this.uiBtnListContainer?.Grid_NavagationType == ContainerType.A)
                {
                    this.uiBtnListContainer.CurnavagationMode = NavagationMode.BtnList;
                    this.OnExitInner();
                }
                return null;
            }

            return RefreshSelected(selectedButton);
        }
        /// <summary>
        /// 向右
        /// </summary>
        public SelectedButton MoveRightIUISelected()
        {
            if (CurSelected == null) return null;
            SelectedButton selectedButton = CurSelected.navigation.selectOnRight as SelectedButton;
            if (selectedButton == CurSelected)
            {
                if (this.uiBtnListContainer?.Grid_NavagationType == ContainerType.A)
                {
                    this.uiBtnListContainer.CurnavagationMode = NavagationMode.BtnList;
                    this.OnExitInner();
                }
                return null;
            }

            return RefreshSelected(selectedButton);
        }
        /// <summary>
        /// 向指定坐标移动
        /// </summary>
        public SelectedButton MoveIndexIUISelected(int i, int j)
        {
            this.CurSelected?.OnDeselect(null);
            this.CurSelected = TwoDimSelectedButtons[i][j];
            this.TwoDimI = i;
            this.TwoDimJ = j;
            this.CurSelected.OnSelect(null);
            return CurSelected;
        }
        public SelectedButton MoveIndexIUISelected(int i)
        {
            this.CurSelected?.OnDeselect(null);
            this.CurSelected = TwoDimSelectedButtons[i][0];
            this.TwoDimI = i;
            this.TwoDimJ = 0;
            this.CurSelected.OnSelect(null);
            return CurSelected;
        }

        /// <summary>
        /// 设置按钮按键提示文本
        /// </summary>
        public void SetBtnText(string btnName, string showText)
        {
            SBDic[btnName].transform.Find("BtnText").GetComponent<TextMeshProUGUI>().text = showText;
        }

        /// <summary>
        /// 网格按钮导航回调
        /// </summary>
        private void GridNavigation(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.NavigationPreAction?.Invoke();
            string actionName = obj.action.name;

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
                this.MoveUPIUISelected();
            }
            else if (angle > 45 && angle < 135)
            {
                this.MoveRightIUISelected();
            }
            else if (angle > 135 && angle < 225)
            {
                this.MoveDownIUISelected();
            }
            else if (angle > 225 && angle < 315)
            {
                this.MoveLeftIUISelected();
            }

            this.NavigationPostAction?.Invoke();
        }

        /// <summary>
        /// 轮盘按钮导航回调 轮转按钮规定正上方为下标0
        /// </summary>
        private void RingNavigation(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.NavigationPreAction?.Invoke();

            string actionName = obj.action.name;

            // 使用 ReadValue<T>() 方法获取附加数据
            string actionMapName = obj.action.actionMap.name;

            var vector2 = obj.ReadValue<UnityEngine.Vector2>();
            double angle = Mathf.Atan2(vector2.x, vector2.y);

            angle = angle * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle = angle + 360;
            }

            double sliceAngle = 360.0 / OneDimCnt;
            for (int i = 0; i < OneDimCnt; i++)
            {
                if (i != OneDimCnt - 1 && angle > sliceAngle / 2 + sliceAngle * i && angle < sliceAngle / 2 + sliceAngle * (i + 1))
                {
                    this.MoveIndexIUISelected(OneDimCnt - 1 - i);
                }
                else if (i == OneDimCnt - 1 && (angle < sliceAngle / 2 || angle > 360 - sliceAngle / 2))
                {
                    this.MoveIndexIUISelected(OneDimCnt - 1 - i);
                }
            }

            this.NavigationPostAction?.Invoke();
        }

        /// <summary>
        /// 按钮确认触发回调
        /// </summary>
        public void ButtonInteract(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            if (this.isEnable)
            {
                this.ButtonInteractPreAction?.Invoke();
                this.CurSelected.Interact();
                this.ButtonInteractPostAction?.Invoke();
            }

        }




        /// <summary>
        /// 该函数功能为绑定按钮导航InputAction的回调函数
        /// </summary>
        public void BindNavigationInputAction(InputAction NavigationInputAction, BindType bindType, Action preAction = null, Action postAction = null)
        {
            this.NavigationPreAction = preAction;
            this.NavigationPostAction = postAction;
            this.NavigationInputAction = NavigationInputAction;
            this.NavigationBindType = bindType;

            switch (bindType)
            {
                case BindType.started:
                    this.NavigationInputAction.started += isWheel ? this.RingNavigation : this.GridNavigation;
                    break;
                case BindType.performed:
                    this.NavigationInputAction.performed += isWheel ? this.RingNavigation : this.GridNavigation;
                    break;
                case BindType.canceled:
                    this.NavigationInputAction.canceled += isWheel ? this.RingNavigation : this.GridNavigation;
                    break;
            }
        }

        /// <summary>
        /// 该函数功能为绑定按钮确认InputAction的回调函数
        /// </summary>
        public void BindButtonInteractInputAction(InputAction ButtonInteractInputAction, BindType bindType, Action preAction = null, Action postAction = null)
        {
            this.ButtonInteractPreAction = preAction;
            this.ButtonInteractPostAction = postAction;
            this.ButtonInteractInputAction = ButtonInteractInputAction;
            this.ButtonInteractBindType = bindType;

            switch (bindType)
            {
                case BindType.started:
                    this.ButtonInteractInputAction.started += this.ButtonInteract;
                    break;
                case BindType.performed:
                    this.ButtonInteractInputAction.performed += this.ButtonInteract;
                    break;
                case BindType.canceled:
                    this.ButtonInteractInputAction.canceled += this.ButtonInteract;
                    break;
            }

        }

        /// <summary>
        /// 该函数功能为绑定按钮 对应哪个InputAction触发
        /// </summary>
        public void BindInputAction(string btnName, InputAction InputAction, BindType bindType, Action preAction = null, Action postAction = null)
        {
            //统一点击与按键 并且加入preAction 与 postAction
            SelectedButton btn = GetBtn(btnName);
            btn.SetPreAndPostInteract(preAction, postAction);
            Action<InputAction.CallbackContext> buttonClickAction = (context) => { preAction?.Invoke(); btn.onClick.Invoke(); postAction?.Invoke(); };

            switch (bindType)
            {
                case BindType.started:
                    InputActionBindDic.Add((InputAction, BindType.started), buttonClickAction);
                    InputAction.started += buttonClickAction;
                    break;
                case BindType.performed:
                    InputActionBindDic.Add((InputAction, BindType.performed), buttonClickAction);
                    InputAction.performed += buttonClickAction;
                    break;
                case BindType.canceled:
                    InputActionBindDic.Add((InputAction, BindType.canceled), buttonClickAction);
                    InputAction.canceled += buttonClickAction;
                    break;
            }

        }

        /// <summary>
        /// 该函数功能为解绑按钮导航和按钮确认InputAction的回调函数
        /// </summary>
        public void DeBindInputAction()
        {
            if (this.NavigationInputAction != null)
            {
                switch (this.NavigationBindType)
                {
                    case BindType.started:
                        this.NavigationInputAction.started -= isWheel ? this.RingNavigation : this.GridNavigation;
                        break;
                    case BindType.performed:
                        this.NavigationInputAction.performed -= isWheel ? this.RingNavigation : this.GridNavigation;
                        break;
                    case BindType.canceled:
                        this.NavigationInputAction.canceled -= isWheel ? this.RingNavigation : this.GridNavigation;
                        break;
                }
            }

            if (this.ButtonInteractInputAction != null)
            {
                switch (this.ButtonInteractBindType)
                {
                    case BindType.started:
                        this.ButtonInteractInputAction.started -= this.ButtonInteract;
                        break;
                    case BindType.performed:
                        this.ButtonInteractInputAction.performed -= this.ButtonInteract;
                        break;
                    case BindType.canceled:
                        this.ButtonInteractInputAction.canceled -= this.ButtonInteract;
                        break;
                }
            }

            foreach (var item in InputActionBindDic)
            {
                switch (item.Key.Item2)
                {
                    case BindType.started:
                        item.Key.Item1.started -= item.Value;
                        break;
                    case BindType.performed:
                        item.Key.Item1.performed -= item.Value;
                        break;
                    case BindType.canceled:
                        item.Key.Item1.canceled -= item.Value;
                        break;
                }
            }

        }

        /// <summary>
        /// 该函数功能为移除所有的Listener
        /// </summary>
        public void RemoveAllListener()
        {

            for (int i = 0; i < TwoDimI; i++)
            {
                for (int j = 0; j < TwoDimJ; j++)
                {
                    TwoDimSelectedButtons[i][j].onClick.RemoveAllListeners();
                }
            }
            foreach (var item in SBDic)
            {
                item.Value.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 该函数功能为更新鼠标选中
        /// </summary>
        public SelectedButton RefreshSelected(SelectedButton sb)
        {
            if (sb == null) return null;
            if (SBPosDic.ContainsKey(sb))
            {
                var (i, j) = SBPosDic[sb];
                this.UIBtnListContainer?.MoveToBtnList(sb.GetUIBtnList());
                return MoveIndexIUISelected(i, j);
            }
            else if(sb.GetUIBtnList() != this)
            {
                //this.CurSelected?.OnDeselect(null);
                //转移btnlist进入选中
                sb.GetUIBtnList().CurSelected = sb;
                this.UIBtnListContainer.CurnavagationMode = NavagationMode.SelectedButton;
                this.UIBtnListContainer?.MoveToBtnList(sb.GetUIBtnList());
            }
            
            return null;
        }

        /// <summary>
        /// 该函数功能为启用BtnList
        /// </summary>
        public void EnableBtnList()
        {
            this.isEnable = true;
        }

        /// <summary>
        /// 该函数功能为禁用BtnList
        /// </summary>
        public void DisableBtnList()
        {
            this.isEnable = false;
        }

        public void OnSelectEnter()
        {
            this.EnableBtnList();
            this.Selected.gameObject.SetActive(true);


            if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.B)
            {
                this.OnEnterInner();
            }
            else if(this.uiBtnListContainer.Grid_NavagationType == ContainerType.A)
            {
                if(this.UIBtnListContainer.CurnavagationMode == NavagationMode.SelectedButton)
                {
                    this.OnEnterInner();
                }
            }
        }

        public void OnSelectExit()
        {
            this.SetCurSelectedNull();
            this.DisableBtnList();
            this.Selected.gameObject.SetActive(false);
        }

        //从选中Btnlist退出然后进入Inner
        public void OnExitToInner()
        {
            this.DisableBtnList();
            this.Selected.gameObject.SetActive(false);
        }

        //从选中Inner退出然后进入Btnlist
        public void OnEnterToBtnlist()
        {
            this.SetCurSelectedNull();
            this.EnableBtnList();
            this.Selected.gameObject.SetActive(true);
        }

        public void OnEnterInner()
        {
            this.OnExitToInner();
            if(this.OneDimCnt > 0 && this.CurSelected == null)
            {
                //初始化选择对象
                this.TwoDimI = 0;
                this.TwoDimJ = 0;
                this.CurSelected = TwoDimSelectedButtons[TwoDimI][TwoDimJ];
            }

            this.CurSelected?.OnSelect(null);


            if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.B)
            {
                //加绑
                this.BindNavigationInputAction(uiBtnListContainer.curGridNavagationInputAction, uiBtnListContainer.curBindType);
            }
            else if(this.uiBtnListContainer.Grid_NavagationType == ContainerType.A)
            {
                //加绑
                this.BindNavigationInputAction(uiBtnListContainer.curGridNavagationInputAction, uiBtnListContainer.curBindType);
                //解绑父项导航
                this.uiBtnListContainer.DeBindNavigationInputAction();
            }

        }

        public void OnExitInner()
        {
            this.OnEnterToBtnlist();

            if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.B)
            {
                //解绑
                DeBindInputAction();
                this.OnSelectExit();
            }
            else if(this.uiBtnListContainer.Grid_NavagationType == ContainerType.A)
            {

                //解绑
                DeBindInputAction();
                //加绑父项导航
                this.uiBtnListContainer.BindNavigationInputAction();
                if(this.uiBtnListContainer.CurnavagationMode == NavagationMode.SelectedButton)
                {
                    this.OnSelectExit();
                }
            }
        }
    }

}


