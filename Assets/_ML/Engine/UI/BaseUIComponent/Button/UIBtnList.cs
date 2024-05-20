using ML.Engine.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnListContainer;
using Sirenix.OdinInspector;
using static ML.Engine.UI.UIBtnListContainerInitor;
using static ML.Engine.UI.UIBtnListInitor;
using static UnityEngine.InputSystem.InputAction;
using ML.Engine.Utility;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO.Pipes;
using UnityEngine.InputSystem.iOS;
using UnityEditor;
using Unity.VisualScripting;
using ML.Engine.Timer;

namespace ML.Engine.UI
{
    public class UIBtnList : ISelected
    {
        #region ISelected
        [ShowInInspector]
        public ISelected LeftUI { get; set; }
        [ShowInInspector]
        public ISelected RightUI { get; set; }
        [ShowInInspector]
        public ISelected UpUI { get; set; }
        [ShowInInspector]
        public ISelected DownUI { get; set; }
        #endregion

        [ShowInInspector]
        private List<List<SelectedButton>> TwoDimSelectedButtons = new List<List<SelectedButton>>();//��ά�б�

        public List<List<SelectedButton>> GetTwoDimSelectedButtons { get { return TwoDimSelectedButtons; } }
        protected int OneDimCnt = 0;
        public int BtnCnt { get { return OneDimCnt; } }
        protected int TwoDimH = 0;
        protected int TwoDimW = 0;
        [ShowInInspector]
        private SelectedButton _CurSelected;

        private SelectedButton CurSelected
        {
            get
            {
                return _CurSelected;
            }
            set
            {
                _CurSelected = value;
                if (_CurSelected != null && SBPosDic.ContainsKey(_CurSelected)) 
                {
                    TwoDimI = SBPosDic[_CurSelected].Item1;
                    TwoDimJ = SBPosDic[_CurSelected].Item2;
                }
            }
        }
        //Index
        [ShowInInspector]
        protected int TwoDimI = 0;
        [ShowInInspector]
        protected int TwoDimJ = 0;

        private InputAction NavigationInputAction = null;
        private Action NavigationPreAction = null;
        private Action NavigationPostAction = null;

        private InputAction ButtonInteractInputAction = null;
        private Action ButtonInteractPreAction = null;
        private Action ButtonInteractPostAction = null;
        private BindType NavigationBindType;
        private BindType ButtonInteractBindType;

        [ShowInInspector]
        private bool isEnable = false;

        public bool IsEnable { get { return isEnable; } }

        private Transform parent;
        public Transform Parent { get { return parent; } }

        private Transform Selected;
        private UIBtnListContainer uiBtnListContainer;
        public UIBtnListContainer UIBtnListContainer { set { uiBtnListContainer = value; } get { return uiBtnListContainer; } }

        private int limitNum;
        private bool hasInitSelect;
        private bool isLoop;
        private bool isWheel;
        private bool readUnActive;
        private bool NeedToResetCurSelected = false;
        private string prefabPath = null;

        [ShowInInspector]
        private bool isEmpty = true;
        public bool IsEmpty { get { return isEmpty; } }

        /// <summary>
        /// btnName,SelectedButton
        /// </summary>
        private Dictionary<string, SelectedButton> SBDic = new Dictionary<string, SelectedButton>();
        /// <summary>
        /// btnName,Index
        /// </summary>
        private Dictionary<string, int> SBDicIndex = new Dictionary<string, int>();
        /// <summary>
        /// SelectedButton,BtnPos
        /// </summary>
        private Dictionary<SelectedButton, (int, int)> SBPosDic = new Dictionary<SelectedButton, (int, int)>();

        private Dictionary<(InputAction, BindType), Action<InputAction.CallbackContext>> InputActionBindDic = new Dictionary<(InputAction, BindType), Action<InputAction.CallbackContext>>();

        public event Action OnSelectButtonChanged;

        private bool isBtnListContainerDeleteAll = false;
        [ShowInInspector]
        /// <summary>
        /// ��������
        /// </summary>
        private UIBtnListSlideWindow UIBtnListSlideWindow = null;

        public UIBtnList(UIBtnListInitor uIBtnListInitor,string prefabPath = null)
        {
            BtnListInitData btnListInitData = uIBtnListInitor.btnListInitData;
            this.parent = uIBtnListInitor.transform;
            this.limitNum = btnListInitData.limitNum;
            this.hasInitSelect = btnListInitData.hasInitSelect;
            this.isLoop = btnListInitData.isLoop;
            this.isWheel = btnListInitData.isWheel;
            this.readUnActive = btnListInitData.readUnActiveButton;

            if (prefabPath != null) 
            {
                this.prefabPath = prefabPath;
            }

            if (btnListInitData.scrollRect != null)
            {
                UIBtnListSlideWindow = new UIBtnListSlideWindow(btnListInitData.scrollRect,this);
            }

            if (parent != null)
            {
                InitBtnInfo(parent, limitNum, hasInitSelect, isLoop, isWheel, this.readUnActive);
                try
                {
                    this.Selected = parent.Find("Selected");
                    this.Selected.gameObject.SetActive(false);
                }
                catch { }
            }
        }

        #region Internal
        public void InitBtnInfo()
        {
            this.SBDic.Clear();
            this.SBDicIndex.Clear();
            this.SBPosDic.Clear();
            this.TwoDimSelectedButtons.Clear();

            SelectedButton[] OneDimSelectedButtons = parent.GetComponentsInChildren<SelectedButton>(true);
            this.OneDimCnt = OneDimSelectedButtons.Length;

            if (this.OneDimCnt == 0 && !isBtnListContainerDeleteAll)
            {
                this.CurSelected = null;
                this.isEmpty = true;
                this.UIBtnListContainer?.FindEnterableUIBtnList();
            }
            else
            {
                this.isEmpty = false;
            }
            this.uiBtnListContainer?.RefreshIsEmpty();
            for (int i = 0; i < OneDimSelectedButtons.Length; i++)
            {
                var btn = OneDimSelectedButtons[i];
                btn.SetUIBtnList(this);
                btn.Init();
                Navigation navigation = btn.navigation;
                navigation.mode = Navigation.Mode.None;
                btn.navigation = navigation;
                SBDic.Add(btn.gameObject.name, btn);
                SBDicIndex.Add(btn.gameObject.name, i);
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

            for (int i = 0; i < TwoDimSelectedButtons.GetLength(0); i++) // ������
            {
                List<SelectedButton> row = new List<SelectedButton>();
                for (int j = 0; j < TwoDimSelectedButtons.GetLength(1); j++) // ������
                {
                    row.Add(TwoDimSelectedButtons[i, j]);
                }
                this.TwoDimSelectedButtons.Add(row);
            }

            if (OneDimCnt > 0 && (hasInitSelect || (NeedToResetCurSelected && this.uiBtnListContainer?.CurSelectUIBtnList == this)))
            {
                //��ʼ��ѡ�����

                this.TwoDimI = 0;
                this.TwoDimJ = 0;
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
                this.CurSelected?.OnSelect(null);
                this.NeedToResetCurSelected = false;
                this.UIBtnListContainer?.InvokeOnSelectButtonChanged();
                this.OnSelectButtonChanged?.Invoke();
            }
        }

        public void InitBtnInfo(Transform parent, int limitNum = 1, bool hasInitSelect = true, bool isLoop = false, bool isWheel = false, bool _readUnActive = true, Action OnSelectedEnter = null, Action OnSelectedExit = null)
        {
            this.SBDic.Clear();
            this.SBDicIndex.Clear();
            this.SBPosDic.Clear();
            this.TwoDimSelectedButtons.Clear();

            SelectedButton[] OneDimSelectedButtons = parent.GetComponentsInChildren<SelectedButton>(_readUnActive);
            this.OneDimCnt = OneDimSelectedButtons.Length;

            if (this.OneDimCnt == 0 && !isBtnListContainerDeleteAll)
            {
                this.CurSelected = null;
                this.isEmpty = true;
                this.UIBtnListContainer?.FindEnterableUIBtnList();
            }
            else
            {
                this.isEmpty = false;
            }
            this.uiBtnListContainer?.RefreshIsEmpty();

            for (int i = 0; i < OneDimSelectedButtons.Length; i++)
            {
                var btn = OneDimSelectedButtons[i];
                btn.SetUIBtnList(this);
                btn.Init();
                Navigation navigation = btn.navigation;
                navigation.mode = Navigation.Mode.None;
                btn.navigation = navigation;
                SBDic.Add(btn.gameObject.name, btn);
                SBDicIndex.Add(btn.gameObject.name, i);
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

            for (int i = 0; i < TwoDimSelectedButtons.GetLength(0); i++) // ������
            {
                List<SelectedButton> row = new List<SelectedButton>();
                for (int j = 0; j < TwoDimSelectedButtons.GetLength(1); j++)  // ������
                {
                    row.Add(TwoDimSelectedButtons[i, j]);
                }
                this.TwoDimSelectedButtons.Add(row);
            }

            if (OneDimCnt > 0 && (hasInitSelect || (NeedToResetCurSelected && this.uiBtnListContainer?.CurSelectUIBtnList == this)))
            {
                //��ʼ��ѡ�����
                this.TwoDimI = 0;
                this.TwoDimJ = 0;
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
                this.CurSelected?.OnSelect(null);
                this.NeedToResetCurSelected = false;
                this.UIBtnListContainer?.InvokeOnSelectButtonChanged();
                this.OnSelectButtonChanged?.Invoke();
            }
        }

        /// <summary>
        /// ���밴ť �첽
        /// </summary>
        public void AddBtn(string VarPrefabpath = null, UnityAction BtnAction = null,Action OnSelectEnter = null, Action OnSelectExit = null, UnityAction<SelectedButton> BtnSettingAction = null,Action OnFinishAdd = null, string BtnText = null,bool NeedRefreshBtnInfo = true)
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
            var tprefabpath = VarPrefabpath == null ? this.prefabPath : VarPrefabpath;
            Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(tprefabpath).Completed += (handle) =>
            {
                // ʵ����
                var btn = handle.Result.GetComponent<SelectedButton>();
                btn.gameObject.name = btn.GetHashCode().ToString();
                btn.transform.SetParent(this.parent.Find("Container"), false);
                btn.transform.localScale = Vector3.one;

                if (BtnAction != null)
                {
                    btn.onClick.AddListener(BtnAction);
                }

                if (OnSelectEnter != null)
                {
                    btn.SetOnSelectEnter(OnSelectEnter);
                }

                if (OnSelectExit != null)
                {
                    btn.SetOnSelectExit(OnSelectExit);
                }

                if (BtnSettingAction != null)
                {
                    BtnSettingAction(btn);
                }

                if (BtnText != null)
                {
                    this.SetBtnText(btn, BtnText);
                }

                if (this.uiBtnListContainer == null)
                {
                    if(NeedRefreshBtnInfo)
                        InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
                    OnFinishAdd?.Invoke();
                    return;
                }

                bool needMoveToBtnList = this.uiBtnListContainer.IsEmpty;
                if (NeedRefreshBtnInfo)
                    InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
                if(needMoveToBtnList)
                {
                    this.UIBtnListContainer?.FindEnterableUIBtnList();
                }
                else
                {
                    this.UIBtnListContainer?.RefreshEdge();
                }
                OnFinishAdd?.Invoke();
            };
        }
        /// <summary>
        /// ���밴ť ͬ��
        /// </summary>
        public void AddBtn(GameObject prefab, UnityAction BtnAction = null, Action OnSelectEnter = null, Action OnSelectExit = null, UnityAction<SelectedButton> BtnSettingAction = null, Action OnFinishAdd = null, string BtnText = null, bool NeedRefreshBtnInfo = true)
        {
            // ʵ����
            
            if(prefab.GetComponent<SelectedButton>() == null)
            {
                prefab.transform.AddComponent<SelectedButton>();
            }
            var btn = prefab.GetComponent<SelectedButton>();
            btn.gameObject.name = btn.GetHashCode().ToString();
            btn.transform.SetParent(this.parent.Find("Container"), false);
            btn.transform.localScale = Vector3.one;

            if (BtnAction != null)
            {
                btn.onClick.AddListener(BtnAction);
            }

            if (OnSelectEnter != null)
            {
                btn.SetOnSelectEnter(OnSelectEnter);
            }

            if (OnSelectExit != null)
            {
                btn.SetOnSelectExit(OnSelectExit);
            }

            if (BtnSettingAction != null)
            {
                BtnSettingAction(btn);
            }

            if (BtnText != null)
            {
                this.SetBtnText(btn, BtnText);
            }

            if (this.uiBtnListContainer == null)
            {
                if (NeedRefreshBtnInfo)
                    InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
                OnFinishAdd?.Invoke();
                return;
            }

            bool needMoveToBtnList = this.uiBtnListContainer.IsEmpty;
            if (NeedRefreshBtnInfo)
                InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
            if (needMoveToBtnList)
            {
                this.UIBtnListContainer?.FindEnterableUIBtnList();
            }
            else
            {
                this.UIBtnListContainer?.RefreshEdge();
            }
            OnFinishAdd?.Invoke();
        }

        //ͬ������
        public class Synchronizer
        {
            private int CheckNum;
            private int curCheckNum;
            private Action OnAllFinish;
            private System.Object lockObject = new System.Object();

            private bool isTrigger;
            public Synchronizer(int checkNum, Action OnAllFinish)
            {
                //Debug.Log("checkNum " + checkNum);
                this.curCheckNum = 0;
                this.CheckNum = checkNum;
                this.OnAllFinish = OnAllFinish;
                this.isTrigger = false;
            }

            public void Check()
            {
                lock(lockObject)
                {
                    ++curCheckNum;
                    //Debug.Log("Check " + curCheckNum);
                    if (isTrigger == false && curCheckNum == CheckNum)
                    {
                        OnAllFinish?.Invoke();
                        isTrigger = true;
                    }
                }
            }
        }


        public void AddBtns(int num, string prefabpath, Action OnAllBtnAdded = null, List<UnityAction> BtnActions = null, Action OnSelectEnter = null, Action OnSelectExit = null, UnityAction<SelectedButton> BtnSettingAction = null, List<string> BtnTexts = null)
        {
            Synchronizer Checker = new Synchronizer(num, OnAllBtnAdded);
            for (int i = 0; i < num; i++)
            {
                var handle = Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(prefabpath);
                handle.Completed += (handle) =>
                {
                    // ʵ����
                    var btn = handle.Result.GetComponent<SelectedButton>();
                    btn.gameObject.name = btn.GetHashCode().ToString();
                    btn.transform.SetParent(this.parent.Find("Container"), false);
                    btn.transform.localScale = Vector3.one;

                    if (BtnActions != null && BtnActions[i] != null)
                    {
                        btn.onClick.AddListener(BtnActions[i]);
                    }

                    if (OnSelectEnter != null)
                    {
                        btn.SetOnSelectEnter(OnSelectEnter);
                    }

                    if (OnSelectExit != null)
                    {
                        btn.SetOnSelectExit(OnSelectExit);
                    }

                    if (BtnSettingAction != null)
                    {
                        BtnSettingAction(btn);
                    }

                    if (BtnTexts != null && BtnTexts[i] != null)
                    {
                        this.SetBtnText(btn, BtnTexts[i]);
                    }


                    if (this.uiBtnListContainer == null)
                    {
                        InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
                    }
                    else
                    {
                        bool needMoveToBtnList = this.uiBtnListContainer.IsEmpty;
                        InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
                        if (needMoveToBtnList)
                        {
                            this.UIBtnListContainer?.FindEnterableUIBtnList();
                        }
                        else
                        {
                            this.UIBtnListContainer?.RefreshEdge();
                        }
                    }
                    
                    Checker.Check();
                };
            }
        }

        public void DeleteButton(int SelectedButtonIndex,Action OnBtnDelete = null)
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
            GameManager.Instance.StartCoroutine(DestroyAndRefreshBtnList(SelectedButtonIndex, OnBtnDelete));
        }

        public void DeleteButton(string btnName, Action OnBtnDelete = null)
        {
            if (this.SBDicIndex.ContainsKey(btnName))
            {
                this.DeleteButton(this.SBDicIndex[btnName], OnBtnDelete);
            }
        }

        public void DeleteAllButton(Action OnAllBtnDeleted = null)
        {
            GameManager.Instance.StartCoroutine(DestroyAllAndRefreshBtnList(OnAllBtnDeleted));
        }

        private IEnumerator DestroyAndRefreshBtnList(int SelectedButtonIndex, Action OnBtnDelete = null)
        {
            //��������
            GameManager.DestroyObj(this.parent.Find("Container").GetChild(SelectedButtonIndex).gameObject);

            // �ȴ�һ֡ ԭ���ǵ�ǰ֡���� transform.childCount�ڵ�ǰ֡���������ϸ��£���Ҫ��һ֡
            yield return null;

            // ����һ֡����BtnList
            InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
            this.UIBtnListContainer?.RefreshEdge();
            OnBtnDelete?.Invoke();
        }

        private IEnumerator DestroyNumAndRefreshBtnList(int num, Action OnAllBtnDeleted = null)
        {
            //������������
            var con = this.parent.Find("Container");
            for (int i = 0; i < num; i++)
            {
                GameManager.DestroyObj(con.GetChild(con.childCount - 1 - i).gameObject);
            }
            // �ȴ�һ֡ ԭ���ǵ�ǰ֡���� transform.childCount�ڵ�ǰ֡���������ϸ��£���Ҫ��һ֡
            yield return null;

            // ����һ֡����BtnList
            InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
            this.UIBtnListContainer?.RefreshEdge();
            OnAllBtnDeleted?.Invoke();
        }

        public void DeleteButtons(int num, Action OnAllBtnDeleted = null)
        {
            GameManager.Instance.StartCoroutine(DestroyNumAndRefreshBtnList(num, OnAllBtnDeleted));
        }

        private IEnumerator DestroyAllAndRefreshBtnList(Action action = null)
        {
            for (int i = 0; i < this.parent.Find("Container").childCount; i++)
            {
                //��������
                GameManager.DestroyObj(this.parent.Find("Container").GetChild(i).gameObject);
            }
            // �ȴ�һ֡ ԭ���ǵ�ǰ֡���� transform.childCount�ڵ�ǰ֡���������ϸ��£���Ҫ��һ֡
            yield return new WaitForSeconds(0.1f);

            // ����һ֡����BtnList
            InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
            this.UIBtnListContainer?.RefreshEdge();

            action?.Invoke();
        }

        public virtual void ChangBtnNum(int newNum, string VarPrefabpath = null, Action OnAllBtnChanged = null)
        {
            var tprefabpath = VarPrefabpath == null ? this.prefabPath : VarPrefabpath;
            if (newNum > this.OneDimCnt)
            {
                this.AddBtns(newNum - this.OneDimCnt, tprefabpath, OnAllBtnAdded: OnAllBtnChanged);
            }
            else if (newNum <= this.OneDimCnt)
            {
                this.DeleteButtons(this.OneDimCnt - newNum, OnAllBtnDeleted: OnAllBtnChanged);
            }
        }

        public void Check()
        {
            InitBtnInfo(this.parent, this.limitNum, this.hasInitSelect, this.isLoop, this.isWheel);
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
        /// ����SelectedButton���ð�ť�б�action
        /// </summary>
        public void SetBtnAction(SelectedButton selectedButton, UnityAction action)
        {
            if (selectedButton.GetUIBtnList() == this)
            {
                selectedButton.onClick.AddListener(action);
            }
        }


        /// <summary>
        /// ����ȫ��Btn��Action
        /// </summary>
        public void SetAllBtnAction(UnityAction action)
        {
            foreach (var item in this.TwoDimSelectedButtons)
            {
                foreach (var btn in item)
                {
                    this.SetBtnAction(btn, action);
                }
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
        public SelectedButton GetBtn(int index1, int index2)
        {
            if (index1 >= 0 && index1 < TwoDimH && index2 >= 0 && index2 < TwoDimW)
            {
                return this.TwoDimSelectedButtons[index1][index2];
            }
            return null;

        }
        public SelectedButton GetBtn(int index)
        {
            int i = index / TwoDimW;
            int j = index % TwoDimW;
            return GetBtn(i, j);
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
            if (this.CurSelected != null)
            {
                this.CurSelected.OnDeselect(null);
            }

            this.CurSelected = null;
        }
        /// <summary>
        /// ����
        /// </summary>
        public SelectedButton MoveUPIUISelected()
        {
            if (this.isEnable == false || CurSelected == null) return null;
            SelectedButton selectedButton = CurSelected.navigation.selectOnUp as SelectedButton;
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
        public SelectedButton MoveDownIUISelected()
        {
            if (this.isEnable == false || CurSelected == null) return null;
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
            if (this.isEnable == false || CurSelected == null) return null;
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
            if (this.isEnable == false || CurSelected == null) return null;
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
            if (GetBtn(i, j) == null) return null;
            this.CurSelected?.OnDeselect(null);
            this.CurSelected = TwoDimSelectedButtons[i][j];
            this.TwoDimI = i;
            this.TwoDimJ = j;
            this.CurSelected.OnSelect(null);
            return CurSelected;
        }
        public SelectedButton MoveIndexIUISelected(int i)
        {
            if (GetBtn(i) == null) return null;

            this.CurSelected?.OnDeselect(null);
            this.CurSelected = GetBtn(i);

            var (x, y) = SBPosDic[this.CurSelected];
            this.TwoDimI = x;
            this.TwoDimJ = y;
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

        public void SetBtnText(SelectedButton sb, string showText)
        {
            sb.transform.Find("BtnText").GetComponent<TextMeshProUGUI>().text = showText;
        }

        /// <summary>
        /// ����ť�����ص�
        /// </summary>
        private void GridNavigation(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.isEnable == false) return;
            if (this.BindNavigationInputCondition != null)
            {
                bool canPerformed = this.BindNavigationInputCondition(obj);
                if (canPerformed == false) return;
            }

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
                //Debug.Log("MoveUPIUISelected " + this.isEnable);
                this.MoveUPIUISelected();
            }
            else if (angle > 45 && angle < 135)
            {
                //Debug.Log("MoveRightIUISelected " + this.isEnable);
                this.MoveRightIUISelected();
            }
            else if (angle > 135 && angle < 225)
            {
                //Debug.Log("MoveDownIUISelected " + this.isEnable);
                this.MoveDownIUISelected();
            }
            else if (angle > 225 && angle < 315)
            {
                //Debug.Log("MoveLeftIUISelected " + this.isEnable);
                this.MoveLeftIUISelected();
            }

            this.NavigationPostAction?.Invoke();
        }

        /// <summary>
        /// ���̰�ť�����ص� ��ת��ť�涨���Ϸ�Ϊ�±�0
        /// </summary>
        private void RingNavigation(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.isEnable == false) return;
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
            if (this.isEnable == false || this.CurSelected == null) return;
            if ((this.BindButtonInteractInputCondition != null && this.BindButtonInteractInputCondition(obj)) || this.BindButtonInteractInputCondition == null)
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
        public delegate bool InputCondition(CallbackContext context);
        private InputCondition BindButtonInteractInputCondition = null;

        //Ŀǰ��ʱ�����Ƹ��ӵ���
        private InputCondition BindNavigationInputCondition = null;

        public void ChangeBindButtonInteractInputCondition(InputCondition inputCondition)
        {
            this.BindButtonInteractInputCondition = inputCondition;
        }

        public void ChangeBindNavigationInputCondition(InputCondition inputCondition)
        {
            this.BindNavigationInputCondition = inputCondition;
        }

        public void BindButtonInteractInputAction(InputAction ButtonInteractInputAction, BindType bindType, Action preAction = null, Action postAction = null, InputCondition inputCondition = null)
        {
            this.ButtonInteractPreAction = preAction;
            this.ButtonInteractPostAction = postAction;
            this.ButtonInteractInputAction = ButtonInteractInputAction;
            this.ButtonInteractBindType = bindType;
            this.BindButtonInteractInputCondition = inputCondition;

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

            try
            {
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
            catch { }
        }

        /// <summary>
        /// �ú�������Ϊ���ť�����Ͱ�ťȷ��InputAction�Ļص�����
        /// </summary>
        public void DeBindInputAction(bool NotIncludeButtonInteractInputAction = false)
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

            if (NotIncludeButtonInteractInputAction == false && this.ButtonInteractInputAction != null)
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
            if (this.isEmpty) return;
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
            if (this.isEnable == false) return null;
            if (sb == null) return null;
            if (SBPosDic.ContainsKey(sb))
            {
                var (i, j) = SBPosDic[sb];
                this.UIBtnListContainer?.MoveToBtnList(sb.GetUIBtnList());
                SelectedButton btn = MoveIndexIUISelected(i, j);
                this.UIBtnListContainer?.InvokeOnSelectButtonChanged();
                this.OnSelectButtonChanged?.Invoke();
                return btn;
            }
            else if (sb.GetUIBtnList() != this)
            {
                //this.CurSelected?.OnDeselect(null);
                //ת��btnlist����ѡ��
                sb.GetUIBtnList().CurSelected = sb;
                this.UIBtnListContainer.CurnavagationMode = NavagationMode.SelectedButton;
                this.UIBtnListContainer?.MoveToBtnList(sb.GetUIBtnList());
                this.UIBtnListContainer?.InvokeOnSelectButtonChanged();

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


        public void ResetCurselected()
        {
            if (OneDimCnt > 0)
            {
                MoveIndexIUISelected(0, 0);
            }
        }

        /// <summary>
        /// ��ȡ��ǰѡ�а�ť�Ķ�ά���� ��û��ѡ���򷵻أ�-1��-1��
        /// </summary>
        public Vector2Int GetCurSelectedPos2()
        {
            if (this.CurSelected != null)
            {
                return new Vector2Int(TwoDimI, TwoDimJ);
            }
            return -Vector2Int.one;
        }
        /// <summary>
        /// ��ȡ��ǰѡ�а�ť��һά���� ��û��ѡ���򷵻�-1
        /// </summary>
        public int GetCurSelectedPos1()
        {
            if (this.CurSelected != null)
            {
                return TwoDimI * TwoDimW + TwoDimJ;
            }
            return -1;
        }

        public void InitSelectBtn()
        {
            this.TwoDimI = 0;
            this.TwoDimJ = 0;
            this.CurSelected = TwoDimSelectedButtons[TwoDimI][TwoDimJ];
            this.CurSelected.OnSelect(null);
            this.UIBtnListContainer?.InvokeOnSelectButtonChanged();
            this.OnSelectButtonChanged?.Invoke();
        }
        #endregion

        #region UIBtnList Switch
        public void OnSelectEnter()
        {

            /*            if (this.uiBtnListContainer == null)
                        {
                            this.EnableBtnList();
                        }*/

            this.EnableBtnList();

            this.Selected?.gameObject.SetActive(true);

            if (this.uiBtnListContainer != null)
            {
                if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.B)
                {
                    this.OnEnterInner();
                }
                else if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.A)
                {
                    if (this.UIBtnListContainer.CurnavagationMode == NavagationMode.SelectedButton)
                    {
                        this.OnEnterInner();
                    }
                }
            }
            else
            {
                if (this.hasInitSelect && OneDimCnt > 0)
                {
                    //��ʼ��ѡ�����
                    this.TwoDimI = 0;
                    this.TwoDimJ = 0;
                    this.CurSelected = TwoDimSelectedButtons[TwoDimI][TwoDimJ];
                    this.CurSelected.OnSelect(null);
                    this.NeedToResetCurSelected = false;
                    this.UIBtnListContainer?.InvokeOnSelectButtonChanged();
                }
            }

        }

        public void OnSelectExit()
        {

            this.SetCurSelectedNull();
            /*            if (this.uiBtnListContainer == null)
                        {
                            this.DisableBtnList();
                        }*/
            this.DisableBtnList();
            this.Selected?.gameObject.SetActive(false);
        }

        //��ѡ��Btnlist�˳�Ȼ�����Inner
        public void OnExitToInner()
        {
            //this.DisableBtnList();
            this.Selected?.gameObject.SetActive(false);
        }

        //��ѡ��Inner�˳�Ȼ�����Btnlist
        public void OnEnterToBtnlist()
        {
            this.SetCurSelectedNull();
            //this.EnableBtnList();
            this.Selected?.gameObject.SetActive(true);
        }

        public void OnEnterInner()
        {
            this.OnExitToInner();
            if (this.OneDimCnt > 0 && this.CurSelected == null)
            {
                //��ʼ��ѡ�����
                this.TwoDimI = 0;
                this.TwoDimJ = 0;
                this.CurSelected = TwoDimSelectedButtons[TwoDimI][TwoDimJ];
                this.UIBtnListContainer?.InvokeOnSelectButtonChanged();
            }
            this.CurSelected?.OnSelect(null);

            if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.B)
            {
                //�Ӱ�
                this.BindNavigationInputAction(uiBtnListContainer.curGridNavagationInputAction, uiBtnListContainer.curBindType);
            }
            else if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.A)
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
                DeBindInputAction(true);
                this.OnSelectExit();
            }
            else if (this.uiBtnListContainer.Grid_NavagationType == ContainerType.A)
            {
                //���
                DeBindInputAction(true);
                //�Ӱ����
                this.uiBtnListContainer.BindNavigationInputAction();
                if (this.uiBtnListContainer.CurnavagationMode == NavagationMode.SelectedButton)
                {
                    this.OnSelectExit();
                }
            }
        }
        #endregion

    }

}


