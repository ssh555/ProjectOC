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
        private List<List<SelectedButton>> TwoDimSelectedButtons = new List<List<SelectedButton>>();//��ά�б�

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
        /// parent: ��ť������ limitNum��һ�ж��ٸ���ť hasInitSelect:�Ƿ��г�ʼѡ�� isLoop:�Ƿ�Ϊѭ����ť isWheel���Ƿ�Ϊ��ת��ť OnSelectedEnter��ѡ�лص� OnSelectedExit��ѡ���ص�
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

            //���ö�ά��ť���λ��
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
                //��ʼ��ѡ�����
                this.TwoDimI = 0;
                this.TwoDimJ = 0;
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
                this.CurSelected.OnSelect(null);
                this.NeedToResetCurSelected = false;
            }


            for (int i = 0; i < TwoDimSelectedButtons.GetLength(0); i++) // ������
            {
                List<SelectedButton> row = new List<SelectedButton>();
                for (int j = 0; j < TwoDimSelectedButtons.GetLength(1); j++) // ������
                {
                    row.Add(TwoDimSelectedButtons[i, j]);
                }
                this.TwoDimSelectedButtons.Add(row);
            }

        }

        /// <summary>
        /// ���밴ť
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


                // ʵ����
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
            //��������
            GameManager.DestroyObj(this.parent.Find("Container").GetChild(SelectedButtonIndex).gameObject);

            // �ȴ�һ֡ ԭ���ǵ�ǰ֡���� transform.childCount�ڵ�ǰ֡���������ϸ��£���Ҫ��һ֡
            yield return null;

            // ����һ֡����BtnList
            InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel, null, null);
            this.UIBtnListContainer?.RefreshEdge();
        }

        public void Check()
        {
            InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel, null, null);
            this.UIBtnListContainer?.RefreshEdge();
        }
        

        /// <summary>
        /// �����±����ö�ά��ť�б�action
        /// </summary>
        public void SetBtnAction(int i, int j, UnityAction action)
        {
            this.TwoDimSelectedButtons[i][j].onClick.AddListener(action);
        }
        /// <summary>
        /// �����ַ������ð�ť�б�action
        /// </summary>
        public void SetBtnAction(string BtnName, UnityAction action)
        {
            if (this.SBDic.ContainsKey(BtnName))
            {
                SBDic[BtnName].onClick.AddListener(action);
            }
        }
        /// <summary>
        /// ��ȡָ����ť
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
        /// ��ȡ��ǰѡ�а�ť
        /// </summary>
        public SelectedButton GetCurSelected()
        {
            return this.CurSelected;
        }
        /// <summary>
        /// ��ȡ��ǰѡ�а�ť
        /// </summary>
        public void SetCurSelected(SelectedButton selectedButton)
        {
            this.CurSelected = selectedButton;
        }
        /// <summary>
        /// �ÿյ�ǰѡ�а�ť
        /// </summary>
        public void SetCurSelectedNull()
        {
            this.CurSelected?.OnDeselect(null);
            this.CurSelected = null;
        }
        /// <summary>
        /// ����
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
        /// ����
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
        /// ����
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
        /// ����
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
        /// ��ָ�������ƶ�
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
        /// ���ð�ť������ʾ�ı�
        /// </summary>
        public void SetBtnText(string btnName, string showText)
        {
            SBDic[btnName].transform.Find("BtnText").GetComponent<TextMeshProUGUI>().text = showText;
        }

        /// <summary>
        /// ����ť�����ص�
        /// </summary>
        private void GridNavigation(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.NavigationPreAction?.Invoke();
            string actionName = obj.action.name;

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
        /// ���̰�ť�����ص� ��ת��ť�涨���Ϸ�Ϊ�±�0
        /// </summary>
        private void RingNavigation(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.NavigationPreAction?.Invoke();

            string actionName = obj.action.name;

            // ʹ�� ReadValue<T>() ������ȡ��������
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
        /// ��ťȷ�ϴ����ص�
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
        /// �ú�������Ϊ�󶨰�ť����InputAction�Ļص�����
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
        /// �ú�������Ϊ�󶨰�ťȷ��InputAction�Ļص�����
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
        /// �ú�������Ϊ�󶨰�ť ��Ӧ�ĸ�InputAction����
        /// </summary>
        public void BindInputAction(string btnName, InputAction InputAction, BindType bindType, Action preAction = null, Action postAction = null)
        {
            //ͳһ����밴�� ���Ҽ���preAction �� postAction
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
        /// �ú�������Ϊ���ť�����Ͱ�ťȷ��InputAction�Ļص�����
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
        /// �ú�������Ϊ�Ƴ����е�Listener
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
        /// �ú�������Ϊ�������ѡ��
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
                //ת��btnlist����ѡ��
                sb.GetUIBtnList().CurSelected = sb;
                this.UIBtnListContainer.CurnavagationMode = NavagationMode.SelectedButton;
                this.UIBtnListContainer?.MoveToBtnList(sb.GetUIBtnList());
            }
            
            return null;
        }

        /// <summary>
        /// �ú�������Ϊ����BtnList
        /// </summary>
        public void EnableBtnList()
        {
            this.isEnable = true;
        }

        /// <summary>
        /// �ú�������Ϊ����BtnList
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

        //��ѡ��Btnlist�˳�Ȼ�����Inner
        public void OnExitToInner()
        {
            this.DisableBtnList();
            this.Selected.gameObject.SetActive(false);
        }

        //��ѡ��Inner�˳�Ȼ�����Btnlist
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
                //��ʼ��ѡ�����
                this.TwoDimI = 0;
                this.TwoDimJ = 0;
                this.CurSelected = TwoDimSelectedButtons[TwoDimI][TwoDimJ];
            }

            this.CurSelected?.OnSelect(null);


            if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.B)
            {
                //�Ӱ�
                this.BindNavigationInputAction(uiBtnListContainer.curGridNavagationInputAction, uiBtnListContainer.curBindType);
            }
            else if(this.uiBtnListContainer.Grid_NavagationType == ContainerType.A)
            {
                //�Ӱ�
                this.BindNavigationInputAction(uiBtnListContainer.curGridNavagationInputAction, uiBtnListContainer.curBindType);
                //������
                this.uiBtnListContainer.DeBindNavigationInputAction();
            }

        }

        public void OnExitInner()
        {
            this.OnEnterToBtnlist();

            if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.B)
            {
                //���
                DeBindInputAction();
                this.OnSelectExit();
            }
            else if(this.uiBtnListContainer.Grid_NavagationType == ContainerType.A)
            {

                //���
                DeBindInputAction();
                //�Ӱ����
                this.uiBtnListContainer.BindNavigationInputAction();
                if(this.uiBtnListContainer.CurnavagationMode == NavagationMode.SelectedButton)
                {
                    this.OnSelectExit();
                }
            }
        }
    }

}


