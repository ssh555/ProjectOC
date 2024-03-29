using OpenCover.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnList;

namespace ML.Engine.UI
{
    public class UIBtnList
    {
        private SelectedButton[,] TwoDimSelectedButtons;//二维列表
        private int OneDimCnt = 0;
        private int TwoDimH = 0;
        private int TwoDimW = 0;

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

        /// <summary>
        /// btnName,SelectedButton
        /// </summary>
        private Dictionary<string, SelectedButton> SBDic = new Dictionary<string, SelectedButton>();
        /// <summary>
        /// SelectedButton,BtnPos
        /// </summary>
        private Dictionary<SelectedButton, (int, int)> SBPosDic = new Dictionary<SelectedButton,(int,int)>();

        private Dictionary<(InputAction, BindType), Action<InputAction.CallbackContext>> InputActionBindDic = new Dictionary<(InputAction, BindType), Action<InputAction.CallbackContext>>();

        private bool isWheel = false;
        /// <summary>
        /// parent: 按钮父物体 limitNum：一行多少个按钮 hasInitSelect:是否有初始选中 isWheel：是否为轮转按钮 OnSelectedEnter：选中回调 OnSelectedExit：选出回调
        /// </summary>
        public UIBtnList(Transform parent, int limitNum = 1, bool hasInitSelect = true, bool isWheel = false, Action OnSelectedEnter = null, Action OnSelectedExit = null)
        {
            this.isWheel = isWheel;
            if (parent != null)
            {
                InitBtnInfo(parent, limitNum, hasInitSelect, isWheel, OnSelectedEnter, OnSelectedExit);
            }   
        }

        public void InitBtnInfo(Transform parent, int limitNum = 1, bool hasInitSelect = true, bool isWheel = false, Action OnSelectedEnter = null, Action OnSelectedExit = null)
        {
            SBDic.Clear();
            SBPosDic.Clear();
            SelectedButton[] OneDimSelectedButtons = parent.GetComponentsInChildren<SelectedButton>(true);
            this.OneDimCnt = OneDimSelectedButtons.Length;

            foreach (var btn in OneDimSelectedButtons)
            {
                btn.SetUIBtnList(this);
                btn.Init(OnSelectedEnter, OnSelectedExit);
                Navigation navigation = btn.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                SBDic.Add(btn.gameObject.name, btn);
            }

            this.TwoDimW = limitNum;
            this.TwoDimH = OneDimCnt % TwoDimW == 0 ? OneDimCnt / TwoDimW : OneDimCnt / TwoDimW + 1;
            TwoDimSelectedButtons = new SelectedButton[TwoDimH, TwoDimW];
            int cnt = 0;
            for (int i = 0; i < TwoDimH; i++)
            {
                for (int j = 0; j < TwoDimW; j++)
                {
                    if (cnt > OneDimCnt)
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
            for (int i = 0; i < OneDimSelectedButtons.Length; ++i)
            {
                int last = (i - 1 + OneDimSelectedButtons.Length) % OneDimSelectedButtons.Length;
                int next = (i + 1 + OneDimSelectedButtons.Length) % OneDimSelectedButtons.Length;
                Navigation navigation = OneDimSelectedButtons[i].navigation;
                navigation.selectOnUp = i - limitNum >= 0 ? OneDimSelectedButtons[i - limitNum] : OneDimSelectedButtons[i - limitNum + OneDimSelectedButtons.Length];
                navigation.selectOnDown = i + limitNum < OneDimSelectedButtons.Length ? OneDimSelectedButtons[i + limitNum] : OneDimSelectedButtons[i + limitNum - OneDimSelectedButtons.Length];
                navigation.selectOnRight = i % limitNum + 1 < limitNum ? OneDimSelectedButtons[i + 1] : OneDimSelectedButtons[i / limitNum * limitNum];
                navigation.selectOnLeft = i % limitNum - 1 >= 0 ? OneDimSelectedButtons[i - 1] : OneDimSelectedButtons[i / limitNum * limitNum + limitNum - 1];
            }

            //初始化选择对象
            this.TwoDimI = 0;
            this.TwoDimJ = 0;
            this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];

            if (hasInitSelect)
                this.CurSelected.OnSelect(null);
        }

        /// <summary>
        /// 传入下标设置二维按钮列表action
        /// </summary>
        public void SetBtnAction(int i,int j, UnityAction action)
        {
            this.TwoDimSelectedButtons[i,j].onClick.AddListener(action);
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
        /// 置空当前选中按钮
        /// </summary>
        public void SetCurSelectedNull()
        {
            this.CurSelected.OnDeselect(null);
            this.CurSelected = null;
        }
        /// <summary>
        /// 向上
        /// </summary>
        public SelectedButton MoveUPIUISelected()
        {
            int i = TwoDimI - 1 >= 0 ? TwoDimI - 1 : TwoDimI - 1 + TwoDimH;
            if (TwoDimSelectedButtons[i, TwoDimJ] == null) return null;
            TwoDimI = i;
            return RefreshSelected(TwoDimSelectedButtons[TwoDimI, TwoDimJ]);
        }
        /// <summary>
        /// 向下
        /// </summary>
        public SelectedButton MoveDownIUISelected()
        {
            int i = TwoDimI + 1 < TwoDimH ? TwoDimI + 1 : TwoDimI + 1 - TwoDimH;
            if (TwoDimSelectedButtons[i, TwoDimJ] == null) return null;
            TwoDimI = i;
            return RefreshSelected(TwoDimSelectedButtons[TwoDimI, TwoDimJ]);
        }
        /// <summary>
        /// 向左
        /// </summary>
        public SelectedButton MoveLeftIUISelected()
        {
            int j = TwoDimJ - 1 >= 0 ? TwoDimJ - 1 : TwoDimJ - 1 + TwoDimW;
            if (TwoDimSelectedButtons[TwoDimI, j] == null) return null;
            TwoDimJ = j;
            return RefreshSelected(TwoDimSelectedButtons[TwoDimI, TwoDimJ]);
        }
        /// <summary>
        /// 向右
        /// </summary>
        public SelectedButton MoveRightIUISelected()
        {

            int j = TwoDimJ + 1 < TwoDimW ? TwoDimJ + 1 : TwoDimJ + 1 - TwoDimW;
            if (TwoDimSelectedButtons[TwoDimI, j] == null) return null;
            TwoDimJ = j;
            return RefreshSelected(TwoDimSelectedButtons[TwoDimI, TwoDimJ]);
        }
        /// <summary>
        /// 向指定坐标移动
        /// </summary>
        public SelectedButton MoveIndexIUISelected(int i,int j)
        {
            this.CurSelected?.OnDeselect(null);
            this.CurSelected = TwoDimSelectedButtons[i, j];
            this.TwoDimI = i;
            this.TwoDimJ = j;
            this.CurSelected.OnSelect(null);
            return CurSelected;
        }
        public SelectedButton MoveIndexIUISelected(int i)
        {
            this.CurSelected?.OnDeselect(null);
            this.CurSelected = TwoDimSelectedButtons[i,0];
            this.TwoDimI = i;
            this.TwoDimJ = 0;
            this.CurSelected.OnSelect(null);
            return CurSelected;
        }

        /// <summary>
        /// 设置按钮按键提示文本
        /// </summary>
        public void SetBtnText(string btnName,string showText)
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
        /// 轮盘按钮导航回调 轮转按钮规定正上方为下标0，yi'ci'yi
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

            if(this.isEnable)
            {
                this.ButtonInteractPreAction?.Invoke();
                this.CurSelected.Interact();
                this.ButtonInteractPostAction?.Invoke();
            }

        }

        public enum BindType
        { 
            started = 0,
            performed,
            canceled
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
            
            if(this.ButtonInteractInputAction != null)
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


        public void RemoveAllListener()
        {

            for(int i = 0;i<TwoDimI;i++)
            {
                for(int j = 0;j<TwoDimJ;j++)
                {
                    TwoDimSelectedButtons[i,j].onClick.RemoveAllListeners();
                }
            }
            foreach (var item in SBDic)
            {
                item.Value.onClick.RemoveAllListeners();
            }
        }

        public SelectedButton RefreshSelected(SelectedButton sb)
        {
            if(SBPosDic.ContainsKey(sb))
            {
                var (i, j) = SBPosDic[sb];
                return MoveIndexIUISelected(i,j);
            }
            return null;
        }

        public void EnableBtnList()
        {
            this.isEnable = true;
        }

        public void DisableBtnList()
        {
            this.isEnable = false;
        }
    }

}


