using ML.Engine.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.WorkerNS.UI.UIFeature;

namespace ProjectOC.WorkerNS.UI
{
    public class UIFeature : ML.Engine.UI.UIBasePanel<FeaturePanel>
    {
        #region Str
        private const string str = "";
        private const string strExchange = "Exchange";
        private const string strFeature = "Feature";
        private const string strViewport = "Viewport";
        private const string strCommon = "Common";
        private const string strChangeWorker = "ChangeWorker";
        private const string strPrefab_Worker_UI_FeatureTemplate = "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab";
        private const string strSelect = "Select";
        private const string strPrefab_Worker_UI_FeatureWorkerTemplate = "Prefab_Worker_UI/Prefab_Worker_UI_FeatureWorkerTemplate.prefab";
        private const string strTopTitle = "TopTitle";
        private const string strText = "Text";
        private const string strCorrect = "Correct";
        private const string strChangeCorrect = "ChangeCorrect";
        private const string strBotKeyTips = "BotKeyTips";
        private const string strKeyTips = "KeyTips";
        private const string strTex2D_Worker_UI_Empty = "Tex2D_Worker_UI_Empty";
        private const string strNewLine = "\n";
        private const string strExchangeBig = "ExchangeBig";
        private const string strCorrectBig = "CorrectBig";
        private const string strKT_See = "KT_See";
        private const string strKT_NoSee = "KT_NoSee";
        private const string strSub = "Sub";
        private const string strKT_Confirm = "KT_Confirm";
        private const string strKT_Back = "KT_Back";
        private const string strNormal = "Normal";
        private const string strSpecific = "Specific";
        private const string strIcon = "Icon";
        private const string strIconNew = "IconNew";
        private const string strIconAllow = "IconAllow";
        private const string strIconBan = "IconBan";
        private const string strDescIcon = "DescIcon";
        private const string strDesc = "Desc";
        private const string strEffectDesc = "EffectDesc";
        private const string strSelected = "Selected";
        private const string strAmount = "Amount";
        private const string strCur = "Cur";
        private const string strNeed = "Need";
        private const string strBack2 = "Back2";
        private const string strName = "Name";
        private const string strBtnConfirm = "BtnConfirm";
        private const string strTextExchange = "TextExchange";
        private const string strWorker = "Worker";
        private const string strTextCorrect = "TextCorrect";
        private const string strBar = "Bar";
        private const string strKT_ConfirmRun = "KT_ConfirmRun";
        private const string strBtnCancle = "BtnCancle";
        private const string strWarn = "Warn";
        private const string strWarn1 = "Warn1";
        private const string strWarn2 = "Warn2";
        private const string strKT_ConfirmRunCorrect = "KT_ConfirmRunCorrect";
        private const string strKT_CancleRun = "KT_CancleRun";
        private const string strKT_CancleRunCorrect = "KT_CancleRunCorrect";
        private const string strBtnConfirmResult = "BtnConfirmResult";
        private const string strBtnCancleResult = "BtnCancleResult";
        private const string strTime = "Time";
        private const string strItem = "Item";
        private const string strItemDesc = "ItemDesc";
        private const string strmin = "min";
        private const string strs = "s";
        #endregion

        #region Data
        #region Mode
        public enum Mode
        {
            Exchange,
            ChangeWorkerSeat1,
            ChangeWorkerSeat2,
            ChangeWorkerCorrect,
            Correct,
            ChangeCorrect
        }
        private Mode curMode;
        public Mode CurMode
        {
            get => curMode;
            set
            {
                if ((curMode == Mode.Exchange && value == Mode.Correct) || (curMode == Mode.Correct && value == Mode.Exchange))
                {
                    IsSeeInfo = false;
                    CurChangeMode = ChangeMode.Index1;
                }
                curMode = value;
                if (IsChangeWorker)
                {
                    Workers.Clear();
                    Workers.Add(str);
                    Workers.AddRange(ManagerNS.LocalGameManager.Instance.WorkerManager.GetSortWorkerIDForFeatureUI());
                    IsInitBtnList = false;
                    WorkerBtnList.ChangBtnNum(Workers.Count, null, () => { IsInitBtnList = true; Refresh(); });
                    WorkerBtnList.EnableBtnList();
                }
                else
                {
                    WorkerBtnList.DisableBtnList();
                }
                if (curMode == Mode.ChangeCorrect)
                {
                    ChangeCorrectSelectIndex = (int)FeatBuild.CorrectType;
                }
                Refresh();
            }
        }
        public bool IsExchange => curMode <= Mode.ChangeWorkerSeat2;
        public bool IsCorrect => Mode.ChangeWorkerCorrect <= curMode;
        public bool IsChangeWorker => Mode.ChangeWorkerSeat1 <= curMode && curMode <= Mode.ChangeWorkerCorrect;
        public bool IsOnMain => curMode == Mode.Exchange || curMode == Mode.Correct;
        public enum ChangeMode
        {
            Index1,
            Index2
        }
        public ChangeMode CurChangeMode;
        public int ChangeCorrectSelectIndex;
        #endregion

        public FeatureBuilding FeatBuild;
        public bool IsSeeInfo;
        private TMPro.TextMeshProUGUI Text_Title;
        private Transform Select;
        private Transform Common;
        private Transform Exchange;
        private Transform Correct;
        private Transform ChangeWorker;
        private Transform ChangeCorrect;
        private Transform KeyTips;
        private bool CanSee()
        {
            if (CurMode == Mode.Exchange)
            {
                return FeatBuild.Seats[0].HaveWorker || FeatBuild.Seats[1].HaveWorker;
            }
            else if (CurMode == Mode.Correct)
            {
                return FeatBuild.Seat.HaveWorker;
            }
            return IsChangeWorker;
        }
        private bool CanConfirm()
        {
            if (CurMode == Mode.Exchange) { return !FeatBuild.IsExchange; }
            else if (CurMode == Mode.Correct) { return !FeatBuild.IsCorrect; }
            return true;
        }
        private bool CanBtnConfirm()
        {
            if (CurMode == Mode.Exchange) { return FeatBuild.IsExchangeStart; }
            else if (CurMode == Mode.Correct) { return FeatBuild.IsCorrectStart; }
            return true;
        }
        private bool CanBtnBack()
        {
            if (CurMode == Mode.Exchange) { return !FeatBuild.IsExchangeEnd; }
            else if (CurMode == Mode.Correct) { return !FeatBuild.IsCorrectEnd; }
            return true;
        }
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();

        #region BtnList
        private ML.Engine.UI.UIBtnList ExchangeFeatureBtnList;
        private ML.Engine.UI.UIBtnList CommonFeatureBtnList;
        private ML.Engine.UI.UIBtnList WorkerBtnList;
        private ML.Engine.UI.UIBtnList ChangeWorkerFeatureBtnList;
        private int WorkerIndex => WorkerBtnList?.GetCurSelectedPos1() ?? 0;
        private List<string> Workers = new List<string>();
        private bool IsInitBtnList = false;
        protected override void InitBtnInfo()
        {
            Synchronizer synchronizer = new Synchronizer(4, () => { IsInitBtnList = true; Refresh(); });
            ExchangeFeatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strExchange).Find(strFeature).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            CommonFeatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strCommon).Find(strFeature).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            ChangeWorkerFeatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strChangeWorker).Find(strFeature).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            ExchangeFeatureBtnList.ChangBtnNum(3, strPrefab_Worker_UI_FeatureTemplate, () => { synchronizer.Check(); });
            CommonFeatureBtnList.ChangBtnNum(3, strPrefab_Worker_UI_FeatureTemplate, () => { synchronizer.Check(); });
            ChangeWorkerFeatureBtnList.ChangBtnNum(3, strPrefab_Worker_UI_FeatureTemplate, () => { synchronizer.Check(); });
            WorkerBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strChangeWorker).Find(strSelect).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>(),
                strPrefab_Worker_UI_FeatureWorkerTemplate);
            WorkerBtnList.OnSelectButtonChanged += () => { Refresh(); };
            WorkerBtnList.ChangBtnNum(0, null, () => { synchronizer.Check(); });
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct FeaturePanel
        {
            public ML.Engine.TextContent.TextContent textTitle;
            public ML.Engine.TextContent.TextContent textExchange;
            public ML.Engine.TextContent.TextContent textCorrect;
            public ML.Engine.TextContent.TextContent textExchangeTimePrefix;
            public ML.Engine.TextContent.TextContent textCorrectTimePrefix;
            public ML.Engine.TextContent.TextContent textWarnSelectWorker;
            public ML.Engine.TextContent.TextContent textWarnLackExchangeItem;
            public ML.Engine.TextContent.TextContent textWarnLackCorrectItem;
            public ML.Engine.TextContent.TextContent textCancleResult;
            public ML.Engine.TextContent.TextContent textCancleResultDesc;

            public ML.Engine.TextContent.KeyTip KT_Confirm;
            public ML.Engine.TextContent.KeyTip KT_Back;
            public ML.Engine.TextContent.KeyTip KT_SelectPre;
            public ML.Engine.TextContent.KeyTip KT_SelectPost;
            public ML.Engine.TextContent.KeyTip KT_ConfirmRun;
            public ML.Engine.TextContent.KeyTip KT_CancleRun;
            public ML.Engine.TextContent.KeyTip KT_ConfirmRunCorrect;
            public ML.Engine.TextContent.KeyTip KT_CancleRunCorrect;
            public ML.Engine.TextContent.KeyTip KT_ConfirmResult;
            public ML.Engine.TextContent.KeyTip KT_CancleResult;
            public ML.Engine.TextContent.KeyTip KT_See;
            public ML.Engine.TextContent.KeyTip KT_NoSee;
        }
        protected override void InitTextContentPathData()
        {
            abpath = "OCTextContent/Feature";
            abname = "FeaturePanel";
            description = "FeaturePanel数据加载完成";
        }
        #endregion
        #endregion

        #region Unity
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            Text_Title = transform.Find(strTopTitle).Find(strText).GetComponent<TMPro.TextMeshProUGUI>();
            Select = transform.Find(strSelect);
            Common = transform.Find(strCommon);
            Exchange = transform.Find(strExchange);
            Correct = transform.Find(strCorrect);
            ChangeWorker = transform.Find(strChangeWorker);
            ChangeCorrect = transform.Find(strChangeCorrect);
            KeyTips = transform.Find(strBotKeyTips).Find(strKeyTips);
            IsInit = true;
        }
        #endregion

        #region Override
        protected override void Enter()
        {
            FeatBuild.Seats[0].OnArriveInvokeEvent += Refresh;
            FeatBuild.Seats[1].OnArriveInvokeEvent += Refresh;
            FeatBuild.Seat.OnArriveInvokeEvent += Refresh;
            FeatBuild.OnExchangeUpdateEvent += RefreshBarForExchange;
            FeatBuild.OnCorrectUpdateEvent += RefreshBarForCorrect;
            FeatBuild.OnExchangeEndEvent += Refresh;
            FeatBuild.OnCorrectEndEvent += Refresh;
            ManagerNS.LocalGameManager.Instance.WorkerManager.OnDeleteWorkerEvent += OnDeleteWorkerEvent;
            tempSprite.Add(str, ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(strTex2D_Worker_UI_Empty));
            var config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
            tempSprite.Add(config.FeatTransCostItemID, ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatTransCostItemID));
            tempSprite.Add(config.FeatUpCostItemID, ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatUpCostItemID));
            tempSprite.Add(config.FeatDownCostItemID, ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatDownCostItemID));
            tempSprite.Add(config.FeatDelCostItemID, ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatDelCostItemID));
            tempSprite.Add(config.FeatReverseCostItemID, ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatReverseCostItemID));
            base.Enter();
        }
        protected override void Exit()
        {
            FeatBuild.Seats[0].OnArriveInvokeEvent -= Refresh;
            FeatBuild.Seats[1].OnArriveInvokeEvent -= Refresh;
            FeatBuild.Seat.OnArriveInvokeEvent -= Refresh;
            FeatBuild.OnExchangeUpdateEvent -= RefreshBarForExchange;
            FeatBuild.OnCorrectUpdateEvent -= RefreshBarForCorrect;
            FeatBuild.OnExchangeEndEvent -= Refresh;
            FeatBuild.OnCorrectEndEvent -= Refresh;
            ManagerNS.LocalGameManager.Instance.WorkerManager.OnDeleteWorkerEvent -= OnDeleteWorkerEvent;
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
            }
            tempSprite.Clear();
            base.Exit();
        }
        private void OnDeleteWorkerEvent(Worker worker)
        {
            if (IsChangeWorker)
            {
                Workers.Remove(worker.ID);
                IsInitBtnList = false;
                WorkerBtnList.ChangBtnNum(Workers.Count, null, () => { IsInitBtnList = true; Refresh(); });
            }
        }
        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.SeeInfo.performed -= SeeInfo_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.SelectPre.performed -= SelectPre_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.SelectPost.performed -= SelectPost_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.Alter.started -= Alter_started;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.Confirm.started -= FeatConfirm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.Cancle.started -= FeatCancle_performed;
        }
        protected override void RegisterInput()
        {
            WorkerBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.SeeInfo.performed += SeeInfo_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.SelectPre.performed += SelectPre_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.SelectPost.performed += SelectPost_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.Alter.started += Alter_started;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.Confirm.started += FeatConfirm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIFeature.Cancle.started += FeatCancle_performed;
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!CanConfirm()) { return; }
            if (CurMode == Mode.Exchange)
            {
                if (FeatBuild.IsExchangeEnd)
                {
                    FeatBuild.ConfirmExchange();
                }
                else
                {
                    CurMode = CurChangeMode == ChangeMode.Index1 ? Mode.ChangeWorkerSeat1 : Mode.ChangeWorkerSeat2;
                }
            }
            else if (CurMode == Mode.Correct)
            {
                if (FeatBuild.IsCorrectEnd)
                {
                    FeatBuild.ConfirmCorrect();
                }
                else
                {
                    CurMode = CurChangeMode == ChangeMode.Index1 ? Mode.ChangeCorrect : Mode.ChangeWorkerCorrect;
                }
            }
            else if (CurMode == Mode.ChangeCorrect)
            {
                FeatBuild.ChangeCorrectType((FeatureCorrectType)ChangeCorrectSelectIndex);
                CurMode = Mode.Correct;
            }
            else
            {
                if (CurMode != Mode.ChangeWorkerCorrect)
                {
                    int index = CurMode == Mode.ChangeWorkerSeat1 ? 0 : 1;
                    FeatBuild.ChangeWorker(index, ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorker(Workers[WorkerIndex]));
                    CurMode = Mode.Exchange;
                }
                else
                {
                    FeatBuild.ChangeCorrectWorker(ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorker(Workers[WorkerIndex]));
                    CurMode = Mode.Correct;
                }
            }
            Refresh();
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (IsOnMain)
            {
                if (CurMode == Mode.Exchange && FeatBuild.IsExchangeEnd)
                {
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushNoticeUIInstance
                        (ML.Engine.UI.UIManager.NoticeUIType.PopUpUI,
                        new ML.Engine.UI.UIManager.PopUpUIData(PanelTextContent.textCancleResult + strNewLine + PanelTextContent.textCancleResultDesc, null, null,
                            () => { FeatBuild.CancelExchange(); }));
                }
                else if (CurMode == Mode.Correct && FeatBuild.IsCorrectEnd)
                {
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushNoticeUIInstance
                        (ML.Engine.UI.UIManager.NoticeUIType.PopUpUI,
                        new ML.Engine.UI.UIManager.PopUpUIData(PanelTextContent.textCancleResult + strNewLine + PanelTextContent.textCancleResultDesc, null, null,
                            () => { FeatBuild.CancelCorrect(); }));
                }
                else
                {
                    UIMgr.PopPanel();
                }
            }
            else
            {
                CurMode = IsExchange ? Mode.Exchange : Mode.Correct;
            }
        }
        private void SeeInfo_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!CanSee()) { return; }
            IsSeeInfo = !IsSeeInfo;
            Refresh();
        }
        private void SelectPre_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(CurMode == Mode.Correct)
            {
                CurMode = Mode.Exchange;
            }
        }
        private void SelectPost_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Exchange)
            {
                CurMode = Mode.Correct;
            }
        }
        private void Alter_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var f_offset = obj.ReadValue<Vector2>();
            var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
            if (CurMode == Mode.Exchange && offset.x != 0 && FeatBuild.IsExchangeStart)
            {
                CurChangeMode = offset.x < 0 ? ChangeMode.Index1 : ChangeMode.Index2;
            }
            else if (CurMode == Mode.Correct && offset.x != 0 && FeatBuild.IsCorrectStart)
            {
                CurChangeMode = offset.x < 0 ? ChangeMode.Index1 : ChangeMode.Index2;
            }
            else if (CurMode == Mode.ChangeCorrect)
            {
                if (offset.y > 0)
                {
                    ChangeCorrectSelectIndex = (ChangeCorrectSelectIndex - 1) % 4;
                }
                else
                {
                    ChangeCorrectSelectIndex = (ChangeCorrectSelectIndex + 1) % 4;
                }
            }
            Refresh();
        }
        private void FeatConfirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Exchange && FeatBuild.CanExchangeReal)
            {
                FeatBuild.ExchangeFeature();
            }
            else if (CurMode == Mode.Correct && FeatBuild.CanCorrectReal)
            {
                FeatBuild.CorrectFeature();
            }
            Refresh();
        }
        private void FeatCancle_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Exchange && FeatBuild.IsExchange)
            {
                FeatBuild.CancelExchange();
            }
            else if (CurMode == Mode.Correct && FeatBuild.IsCorrect)
            {
                FeatBuild.CancelCorrect();
            }
            Refresh();
        }
        #endregion

        #region UI
        protected void SetUIActive()
        {
            bool isExchange = IsExchange;
            Exchange.gameObject.SetActive(isExchange);
            Correct.gameObject.SetActive(IsCorrect);
            ChangeWorker.gameObject.SetActive(IsChangeWorker);
            ChangeCorrect.gameObject.SetActive(CurMode == Mode.ChangeCorrect);
            Select.Find(strExchange).gameObject.SetActive(!isExchange);
            Select.Find(strExchangeBig).gameObject.SetActive(isExchange);
            Select.Find(strCorrect).gameObject.SetActive(isExchange);
            Select.Find(strCorrectBig).gameObject.SetActive(!isExchange);
            bool canSee = CanSee();
            bool canConfirm = CanBtnConfirm();
            bool canBack = CanBtnBack();
            KeyTips.Find(strKT_See).gameObject.SetActive(canSee && !IsSeeInfo);
            KeyTips.Find(strKT_NoSee).gameObject.SetActive(canSee && IsSeeInfo);
            KeyTips.Find(strSub).gameObject.SetActive(canConfirm || canBack);
            KeyTips.Find(strSub).Find(strKT_Confirm).gameObject.SetActive(canConfirm);
            KeyTips.Find(strSub).Find(strKT_Back).gameObject.SetActive(canBack);
            LayoutRebuilder.ForceRebuildLayoutImmediate(KeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(KeyTips.Find(strSub).GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
        }
        public void ClearFeatureBtnList(ML.Engine.UI.UIBtnList btnList)
        {
            for (int i = 0; i < 3; i++)
            {
                Transform btn = btnList.GetBtn(i).transform;
                btn.Find(strNormal).gameObject.SetActive(!IsSeeInfo);
                btn.Find(strSpecific).gameObject.SetActive(IsSeeInfo);
                Transform feat = IsSeeInfo ? btn.Find(strSpecific) : btn.Find(strNormal);
                feat.Find(strIcon).gameObject.SetActive(false);
                feat.Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = str;
                feat.Find(strIconNew).gameObject.SetActive(false);
                feat.Find(strIconAllow).gameObject.SetActive(false);
                feat.Find(strIconBan).gameObject.SetActive(false);
                if (IsSeeInfo)
                {
                    feat.Find(strDescIcon).gameObject.SetActive(false);
                    feat.Find(strDesc).GetComponent<TMPro.TextMeshProUGUI>().text = str;
                    feat.Find(strEffectDesc).GetComponent<TMPro.TextMeshProUGUI>().text = str;
                }
            }
        }
        public void RefreshFeatureBtnList(ML.Engine.UI.UIBtnList btnList, FeatureSeat seat, bool checkNew = false, bool checkCanCorrect = false)
        {
            ClearFeatureBtnList(btnList);
            if (seat.HaveWorker)
            {
                RefreshFeatureBtnList(btnList, seat.FeatureIDs, true);
                FeatureManager featManager = ManagerNS.LocalGameManager.Instance.FeatureManager;
                for (int i = 0; i < seat.FeatureIDs.Count; i++)
                {
                    string featID = seat.FeatureIDs[i];
                    Transform feat = IsSeeInfo ? btnList.GetBtn(i).transform.Find(strSpecific) : btnList.GetBtn(i).transform.Find(strNormal);
                    feat.Find(strIconNew).gameObject.SetActive(checkNew && seat.IsChanged[i]);
                    feat.Find(strIconAllow).gameObject.SetActive(!checkNew && checkCanCorrect && featManager.GetCanCorrect(featID, FeatBuild.CorrectType));
                    feat.Find(strIconBan).gameObject.SetActive(!checkNew && checkCanCorrect && !featManager.GetCanCorrect(featID, FeatBuild.CorrectType));
                }
            }
        }
        public void RefreshFeatureBtnList(ML.Engine.UI.UIBtnList btnList, List<string> featIDs, bool HaveWorker)
        {
            ClearFeatureBtnList(btnList);
            if (HaveWorker)
            {
                FeatureManager featManager = ManagerNS.LocalGameManager.Instance.FeatureManager;
                for (int i = 0; i < featIDs.Count; i++)
                {
                    string featID = featIDs[i];
                    Transform feat = IsSeeInfo ? btnList.GetBtn(i).transform.Find(strSpecific) : btnList.GetBtn(i).transform.Find(strNormal);
                    if (!string.IsNullOrEmpty(featID))
                    {
                        string iconName = featManager.GetIcon(featID);
                        feat.Find(strIcon).gameObject.SetActive(!string.IsNullOrEmpty(iconName));
                        if (!string.IsNullOrEmpty(iconName))
                        {
                            if (!tempSprite.ContainsKey(iconName))
                            {
                                tempSprite.Add(iconName, ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(iconName));
                            }
                            feat.Find(strIcon).GetComponent<Image>().sprite = tempSprite[iconName];
                        }
                        feat.Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = featManager.GetName(featID);
                        if (IsSeeInfo)
                        {
                            feat.Find(strDesc).GetComponent<TMPro.TextMeshProUGUI>().text = featManager.GetItemDescription(featID);
                            feat.Find(strDescIcon).gameObject.SetActive(true);
                            feat.Find(strDescIcon).GetComponent<Image>().color = featManager.GetColorForUI(featID);
                            feat.Find(strEffectDesc).GetComponent<TMPro.TextMeshProUGUI>().text = 
                                $"<color={featManager.GetColorStrForUI(featID)}>" + featManager.GetEffectsDescription(featID) + "</color>";
                        }
                    }
                }
            }
        }
        public void RefreshWorker(Transform transform, Worker worker, bool isSelect=false)
        {
            if (worker != null && !tempSprite.ContainsKey(worker.Category.ToString()))
            {
                tempSprite[worker.Category.ToString()] = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkerProfile(worker.Category);
            }
            transform.Find(strIcon).GetComponent<Image>().sprite = worker != null ? tempSprite[worker.Category.ToString()] : tempSprite[str];
            transform.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = worker?.Name ?? str;
            transform.Find(strSelected).gameObject.SetActive(isSelect);
        }
        public void RefreshItem(Transform transform, string id, int cur, int need, bool isSelect = false)
        {
            transform.Find(strIcon).GetComponent<Image>().sprite = tempSprite[id];
            transform.Find(strAmount).Find(strCur).GetComponent<TMPro.TextMeshProUGUI>().text = cur.ToString();
            transform.Find(strAmount).Find(strNeed).GetComponent<TMPro.TextMeshProUGUI>().text = need.ToString();
            transform.Find(strAmount).Find(strBack2).gameObject.SetActive(cur < need);
            transform.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemName(id);
            if (transform.Find(strSelected) != null)
            {
                transform.Find(strSelected).gameObject.SetActive(isSelect);
            }
        }
        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            FeatureManager featManager = ManagerNS.LocalGameManager.Instance.FeatureManager;
            var config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
            Text_Title.text = PanelTextContent.textTitle;
            Select.Find(strTextExchange).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textExchange;
            Select.Find(strTextCorrect).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textCorrect;
            SetUIActive();
            if (IsOnMain)
            {
                bool isExchange = CurMode == Mode.Exchange;
                bool isEnd = isExchange ? FeatBuild.IsExchangeEnd : FeatBuild.IsCorrectEnd;
                bool isRun = isExchange ? FeatBuild.IsExchange : FeatBuild.IsCorrect;
                bool isStart = isExchange ? FeatBuild.IsExchangeStart : FeatBuild.IsCorrectStart;
                FeatureSeat seat = isExchange ? FeatBuild.Seats[1] : FeatBuild.Seat;
                bool checkCanCorrect = !isExchange && !isEnd;
                RefreshFeatureBtnList(CommonFeatureBtnList, seat, isEnd, checkCanCorrect);
                RefreshWorker(Common.Find(strWorker), seat.Worker, (CurChangeMode == ChangeMode.Index2 && isStart));
                Common.Find(strBar).gameObject.SetActive(isRun);
                bool canRun = isExchange ? FeatBuild.CanExchangeReal : FeatBuild.CanCorrectReal;
                Common.Find(strBtnConfirm).gameObject.SetActive(isStart);
                Common.Find(strBtnConfirm).Find(strBack2).gameObject.SetActive(!canRun);
                Common.Find(strBtnConfirm).Find(strKT_ConfirmRun).gameObject.SetActive(isExchange);
                Common.Find(strBtnConfirm).Find(strKT_ConfirmRunCorrect).gameObject.SetActive(!isExchange);
                Common.Find(strBtnCancle).gameObject.SetActive(isRun);
                Common.Find(strBtnCancle).Find(strKT_CancleRun).gameObject.SetActive(isExchange);
                Common.Find(strBtnCancle).Find(strKT_CancleRunCorrect).gameObject.SetActive(!isExchange);
                Common.Find(strBtnConfirmResult).gameObject.SetActive(isEnd);
                Common.Find(strBtnCancleResult).gameObject.SetActive(isEnd);

                Common.Find(strWarn).gameObject.SetActive(!canRun);
                bool haveSelectWorker = seat.HaveWorker && (!isExchange || FeatBuild.Seats[0].HaveWorker);
                string itemID = isExchange ? config.FeatTransCostItemID : featManager.GetFeatureCorrectItemID(FeatBuild.CorrectType);
                int cur = ManagerNS.LocalGameManager.Instance.Player.InventoryItemAmount(itemID);
                int need = isExchange ? config.FeatTransCostItemNum : featManager.GetFeatureCorrectItemNum(FeatBuild.CorrectType);
                bool haveEnoughItem = cur >= need;
                Common.Find(strWarn).Find(strWarn1).gameObject.SetActive(!haveSelectWorker);
                Common.Find(strWarn).Find(strWarn2).gameObject.SetActive(!haveEnoughItem);
                Common.Find(strWarn).Find(strWarn1).Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textWarnSelectWorker;
                Common.Find(strWarn).Find(strWarn2).Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = 
                    isExchange ? PanelTextContent.textWarnLackExchangeItem : PanelTextContent.textWarnLackCorrectItem;
                LayoutRebuilder.ForceRebuildLayoutImmediate(Common.Find(strWarn).GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
                Common.Find(strTime).gameObject.SetActive(!isEnd);
                if (isStart)
                {
                    int time = isExchange ? config.FeatTransTime : featManager.GetFeatureCorrectTime(FeatBuild.CorrectType);
                    int minute = time / 60;
                    int second = time % 60;
                    Common.Find(strTime).Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = 
                        (isExchange ? PanelTextContent.textExchangeTimePrefix: PanelTextContent.textCorrectTimePrefix) + minute + strmin + second + strs;
                }
                if (CurMode == Mode.Exchange)
                {
                    RefreshFeatureBtnList(ExchangeFeatureBtnList, FeatBuild.Seats[0], FeatBuild.IsExchangeEnd);
                    RefreshWorker(Exchange.Find(strWorker), FeatBuild.Seats[0].Worker, (CurChangeMode == ChangeMode.Index1 && FeatBuild.IsExchangeStart));
                    RefreshItem(Exchange.Find(strItem), itemID, cur, need);
                }
                else if (CurMode == Mode.Correct)
                {
                    RefreshItem(Correct.Find(strItem), itemID, cur, need, (CurChangeMode == ChangeMode.Index1 && FeatBuild.IsCorrectStart));
                    Correct.Find(strItemDesc).Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemName(itemID);
                    Correct.Find(strItemDesc).Find(strDesc).GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemDescription(itemID);
                    Correct.Find(strItemDesc).Find(strEffectDesc).GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetEffectDescription(itemID);
                }
            }
            else if (IsChangeWorker)
            {
                for (int i = 0; i < WorkerBtnList.BtnCnt; i++)
                {
                    Transform uiworker = WorkerBtnList.GetBtn(i).transform;
                    if (!string.IsNullOrEmpty(Workers[i]))
                    {
                        WorkerCategory key = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorker(Workers[i]).Category;
                        if (!tempSprite.ContainsKey(key.ToString()))
                        {
                            tempSprite[key.ToString()] = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkerProfile(key);
                        }
                        uiworker.Find(strIcon).GetComponent<Image>().sprite = tempSprite[key.ToString()];
                    }
                    else
                    {
                        uiworker.Find(strIcon).GetComponent<Image>().sprite = tempSprite[str];
                    }
                    uiworker.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkerName(Workers[i]);
                }
                Worker selectWorker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorker(Workers[WorkerIndex]);
                if (selectWorker != null)
                {
                    RefreshFeatureBtnList(ChangeWorkerFeatureBtnList, selectWorker.GetFeatureIDs(true), true);
                }
                else
                {
                    ClearFeatureBtnList(ChangeWorkerFeatureBtnList);
                }
            }
            else
            {
                for (int i = 1; i <= 4; i++)
                {
                    ChangeCorrect.Find(strItem + i).GetComponent<Image>().sprite = tempSprite[featManager.GetFeatureCorrectItemID(FeatBuild.CorrectType)];
                    ChangeCorrect.Find(strItem + i).Find(strSelected).gameObject.SetActive(i == ChangeCorrectSelectIndex + 1);
                }
                FeatureCorrectType correctType = (FeatureCorrectType)ChangeCorrectSelectIndex;
                string itemID = ManagerNS.LocalGameManager.Instance.FeatureManager.GetFeatureCorrectItemID(correctType);
                ChangeCorrect.Find(strItemDesc).Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text =
                    ManagerNS.LocalGameManager.Instance.ItemManager.GetItemName(itemID);
                ChangeCorrect.Find(strItemDesc).Find(strDesc).GetComponent<TMPro.TextMeshProUGUI>().text =
                    ManagerNS.LocalGameManager.Instance.ItemManager.GetItemDescription(itemID);
                ChangeCorrect.Find(strItemDesc).Find(strEffectDesc).GetComponent<TMPro.TextMeshProUGUI>().text =
                    ManagerNS.LocalGameManager.Instance.ItemManager.GetEffectDescription(itemID);
            }
        }
        public void RefreshBarForExchange(double time) { if (CurMode == Mode.Exchange) { RefreshBar(time); } }
        public void RefreshBarForCorrect(double time) { if (CurMode == Mode.Correct) { RefreshBar(time); } }
        private void RefreshBar(double time)
        {
            int minute = (int)(time / 60);
            int second = (int)time % 60;
            Common.Find(strTime).Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = minute + strmin + second + strs;
            var bar = Common.Find(strBar).Find(strCur).GetComponent<RectTransform>();
            int total = CurMode == Mode.Exchange ?
                ManagerNS.LocalGameManager.Instance.FeatureManager.Config.FeatTransTime :
                ManagerNS.LocalGameManager.Instance.FeatureManager.GetFeatureCorrectTime(FeatBuild.CorrectType);
            float sizeDeltaX = (float)(470 * (1 - time / total));
            sizeDeltaX = sizeDeltaX <= 470 ? sizeDeltaX : 470;
            bar.sizeDelta = new Vector2(sizeDeltaX, bar.sizeDelta.y);
        }
        #endregion
    }
}