using ML.Engine.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.WorkerNS.UI.UIFeature;

namespace ProjectOC.WorkerNS.UI
{
    public class UIFeature : ML.Engine.UI.UIBasePanel<FeaturePanel>
    {
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
                    Workers.Add("");
                    Workers.AddRange(ManagerNS.LocalGameManager.Instance.WorkerManager.GetNotBanWorkerIDs());
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
            ExchangeFeatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find("Exchange").Find("Feature").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            CommonFeatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find("Common").Find("Feature").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            ChangeWorkerFeatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeWorker").Find("Feature").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            ExchangeFeatureBtnList.ChangBtnNum(3, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab", () => { synchronizer.Check(); });
            CommonFeatureBtnList.ChangBtnNum(3, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab", () => { synchronizer.Check(); });
            ChangeWorkerFeatureBtnList.ChangBtnNum(3, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab", () => { synchronizer.Check(); });
            WorkerBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeWorker").Find("Select").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>(),
                "Prefab_Worker_UI/Prefab_Worker_UI_FeatureWorkerTemplate.prefab");
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
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Select = transform.Find("Select");
            Common = transform.Find("Common");
            Exchange = transform.Find("Exchange");
            Correct = transform.Find("Correct");
            ChangeWorker = transform.Find("ChangeWorker");
            ChangeCorrect = transform.Find("ChangeCorrect");
            KeyTips = transform.Find("BotKeyTips").Find("KeyTips");
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
            tempSprite.Add("", ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_Empty"));
            tempSprite.Add("WorkerIcon", ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_Beast"));
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
                        new ML.Engine.UI.UIManager.PopUpUIData(PanelTextContent.textCancleResult + "\n" + PanelTextContent.textCancleResultDesc, null, null,
                            () => { FeatBuild.CancleExchange(); }));
                }
                else if (CurMode == Mode.Correct && FeatBuild.IsCorrectEnd)
                {
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushNoticeUIInstance
                        (ML.Engine.UI.UIManager.NoticeUIType.PopUpUI,
                        new ML.Engine.UI.UIManager.PopUpUIData(PanelTextContent.textCancleResult + "\n" + PanelTextContent.textCancleResultDesc, null, null,
                            () => { FeatBuild.CancleCorrect(); }));
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
                FeatBuild.CancleExchange();
            }
            else if (CurMode == Mode.Correct && FeatBuild.IsCorrect)
            {
                FeatBuild.CancleCorrect();
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
            Select.Find("Exchange").gameObject.SetActive(!isExchange);
            Select.Find("ExchangeBig").gameObject.SetActive(isExchange);
            Select.Find("Correct").gameObject.SetActive(isExchange);
            Select.Find("CorrectBig").gameObject.SetActive(!isExchange);
            bool canSee = CanSee();
            bool canConfirm = CanBtnConfirm();
            bool canBack = CanBtnBack();
            KeyTips.Find("KT_See").gameObject.SetActive(canSee && !IsSeeInfo);
            KeyTips.Find("KT_NoSee").gameObject.SetActive(canSee && IsSeeInfo);
            KeyTips.Find("Sub").gameObject.SetActive(canConfirm || canBack);
            KeyTips.Find("Sub").Find("KT_Confirm").gameObject.SetActive(canConfirm);
            KeyTips.Find("Sub").Find("KT_Back").gameObject.SetActive(canBack);
            LayoutRebuilder.ForceRebuildLayoutImmediate(KeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(KeyTips.Find("Sub").GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
        }
        public void ClearFeatureBtnList(ML.Engine.UI.UIBtnList btnList)
        {
            for (int i = 0; i < 3; i++)
            {
                Transform btn = btnList.GetBtn(i).transform;
                btn.Find("Normal").gameObject.SetActive(!IsSeeInfo);
                btn.Find("Specific").gameObject.SetActive(IsSeeInfo);
                Transform feat = IsSeeInfo ? btn.Find("Specific") : btn.Find("Normal");
                feat.Find("Icon").gameObject.SetActive(false);
                feat.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = "";
                feat.Find("IconNew").gameObject.SetActive(false);
                feat.Find("IconCorrect").gameObject.SetActive(false);
                if (IsSeeInfo)
                {
                    feat.Find("DescIcon").gameObject.SetActive(false);
                    feat.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = "";
                    feat.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = "";
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
                    Transform feat = IsSeeInfo ? btnList.GetBtn(i).transform.Find("Specific") : btnList.GetBtn(i).transform.Find("Normal");
                    feat.Find("IconNew").gameObject.SetActive(checkNew && seat.IsChanged[i]);
                    feat.Find("IconCorrect").gameObject.SetActive(!checkNew && checkCanCorrect && featManager.GetCanCorrect(featID, FeatBuild.CorrectType));
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
                    Transform feat = IsSeeInfo ? btnList.GetBtn(i).transform.Find("Specific") : btnList.GetBtn(i).transform.Find("Normal");
                    if (!string.IsNullOrEmpty(featID))
                    {
                        string iconName = featManager.GetIcon(featID);
                        feat.Find("Icon").gameObject.SetActive(!string.IsNullOrEmpty(iconName));
                        if (!string.IsNullOrEmpty(iconName))
                        {
                            if (!tempSprite.ContainsKey(iconName))
                            {
                                tempSprite.Add(iconName, ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(iconName));
                            }
                            feat.Find("Icon").GetComponent<Image>().sprite = tempSprite[iconName];
                        }
                        feat.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = featManager.GetName(featID);
                        if (IsSeeInfo)
                        {
                            feat.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = featManager.GetItemDescription(featID);
                            feat.Find("DescIcon").gameObject.SetActive(true);
                            feat.Find("DescIcon").GetComponent<Image>().color = featManager.GetColorForUI(featID);
                            feat.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = 
                                $"<color={featManager.GetColorStrForUI(featID)}>" + featManager.GetEffectsDescription(featID) + "</color>";
                        }
                    }
                }
            }
        }
        public void RefreshWorker(Transform transform, Worker worker, bool isSelect=false)
        {
            transform.Find("Icon").GetComponent<Image>().sprite = worker != null ? tempSprite["WorkerIcon"] : tempSprite[""];
            transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = worker?.Name ?? "";
            transform.Find("Selected").gameObject.SetActive(isSelect);
        }
        public void RefreshItem(Transform transform, string id, int cur, int need, bool isSelect = false)
        {
            transform.Find("Icon").GetComponent<Image>().sprite = tempSprite[id];
            transform.Find("Amount").Find("Cur").GetComponent<TMPro.TextMeshProUGUI>().text = cur.ToString();
            transform.Find("Amount").Find("Need").GetComponent<TMPro.TextMeshProUGUI>().text = need.ToString();
            transform.Find("Amount").Find("Back2").gameObject.SetActive(cur < need);
            transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemName(id);
            if (transform.Find("Selected") != null)
            {
                transform.Find("Selected").gameObject.SetActive(isSelect);
            }
        }
        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            FeatureManager featManager = ManagerNS.LocalGameManager.Instance.FeatureManager;
            var config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
            Text_Title.text = PanelTextContent.textTitle;
            Select.Find("TextExchange").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textExchange;
            Select.Find("TextCorrect").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textCorrect;
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
                RefreshWorker(Common.Find("Worker"), seat.Worker, (CurChangeMode == ChangeMode.Index2 && isStart));
                Common.Find("Bar").gameObject.SetActive(isRun);
                bool canRun = isExchange ? FeatBuild.CanExchangeReal : FeatBuild.CanCorrectReal;
                Common.Find("BtnConfirm").gameObject.SetActive(isStart);
                Common.Find("BtnConfirm").Find("Back2").gameObject.SetActive(!canRun);
                Common.Find("BtnConfirm").Find("KT_ConfirmRun").gameObject.SetActive(isExchange);
                Common.Find("BtnConfirm").Find("KT_ConfirmRunCorrect").gameObject.SetActive(!isExchange);
                Common.Find("BtnCancle").gameObject.SetActive(isRun);
                Common.Find("BtnCancle").Find("KT_CancleRun").gameObject.SetActive(isExchange);
                Common.Find("BtnCancle").Find("KT_CancleRunCorrect").gameObject.SetActive(!isExchange);
                Common.Find("BtnConfirmResult").gameObject.SetActive(isEnd);
                Common.Find("BtnCancleResult").gameObject.SetActive(isEnd);

                Common.Find("Warn").gameObject.SetActive(!canRun);
                bool haveSelectWorker = seat.HaveWorker && (!isExchange || FeatBuild.Seats[0].HaveWorker);
                string itemID = isExchange ? config.FeatTransCostItemID : featManager.GetFeatureCorrectItemID(FeatBuild.CorrectType);
                int cur = ManagerNS.LocalGameManager.Instance.Player.InventoryItemAmount(itemID);
                int need = isExchange ? config.FeatTransCostItemNum : featManager.GetFeatureCorrectItemNum(FeatBuild.CorrectType);
                bool haveEnoughItem = cur >= need;
                Common.Find("Warn").Find("Warn1").gameObject.SetActive(!haveSelectWorker);
                Common.Find("Warn").Find("Warn2").gameObject.SetActive(!haveEnoughItem);
                Common.Find("Warn").Find("Warn1").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textWarnSelectWorker;
                Common.Find("Warn").Find("Warn2").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = 
                    isExchange ? PanelTextContent.textWarnLackExchangeItem : PanelTextContent.textWarnLackCorrectItem;
                LayoutRebuilder.ForceRebuildLayoutImmediate(Common.Find("Warn").GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
                Common.Find("Time").gameObject.SetActive(!isEnd);
                if (isStart)
                {
                    int time = isExchange ? config.FeatTransTime : featManager.GetFeatureCorrectTime(FeatBuild.CorrectType);
                    int minute = time / 60;
                    int second = time % 60;
                    Common.Find("Time").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = 
                        (isExchange ? PanelTextContent.textExchangeTimePrefix: PanelTextContent.textCorrectTimePrefix) + minute + "min" + second + "s";
                }
                if (CurMode == Mode.Exchange)
                {
                    RefreshFeatureBtnList(ExchangeFeatureBtnList, FeatBuild.Seats[0], FeatBuild.IsExchangeEnd);
                    RefreshWorker(Exchange.Find("Worker"), FeatBuild.Seats[0].Worker, (CurChangeMode == ChangeMode.Index1 && FeatBuild.IsExchangeStart));
                    RefreshItem(Exchange.Find("Item"), itemID, cur, need);
                }
                else if (CurMode == Mode.Correct)
                {
                    RefreshItem(Correct.Find("Item"), itemID, cur, need, (CurChangeMode == ChangeMode.Index1 && FeatBuild.IsCorrectStart));
                    Correct.Find("ItemDesc").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemName(itemID);
                    Correct.Find("ItemDesc").Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemDescription(itemID);
                    Correct.Find("ItemDesc").Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetEffectDescription(itemID);
                }
            }
            else if (IsChangeWorker)
            {
                for (int i = 0; i < WorkerBtnList.BtnCnt; i++)
                {
                    Transform uiworker = WorkerBtnList.GetBtn(i).transform;
                    uiworker.Find("Icon").GetComponent<Image>().sprite = !string.IsNullOrEmpty(Workers[i]) ? tempSprite["WorkerIcon"] : tempSprite[""];
                    uiworker.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkerName(Workers[i]);
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
                    ChangeCorrect.Find("Item" + i).GetComponent<Image>().sprite = tempSprite[featManager.GetFeatureCorrectItemID(FeatBuild.CorrectType)];
                    ChangeCorrect.Find("Item" + i).Find("Selected").gameObject.SetActive(i == ChangeCorrectSelectIndex + 1);
                }
                FeatureCorrectType correctType = (FeatureCorrectType)ChangeCorrectSelectIndex;
                string itemID = ManagerNS.LocalGameManager.Instance.FeatureManager.GetFeatureCorrectItemID(correctType);
                ChangeCorrect.Find("ItemDesc").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text =
                    ManagerNS.LocalGameManager.Instance.ItemManager.GetItemName(itemID);
                ChangeCorrect.Find("ItemDesc").Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text =
                    ManagerNS.LocalGameManager.Instance.ItemManager.GetItemDescription(itemID);
                ChangeCorrect.Find("ItemDesc").Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text =
                    ManagerNS.LocalGameManager.Instance.ItemManager.GetEffectDescription(itemID);
            }
        }
        public void RefreshBarForExchange(double time) { if (CurMode == Mode.Exchange) { RefreshBar(time); } }
        public void RefreshBarForCorrect(double time) { if (CurMode == Mode.Correct) { RefreshBar(time); } }
        private void RefreshBar(double time)
        {
            int minute = (int)(time / 60);
            int second = (int)time % 60;
            Common.Find("Time").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = minute + "min" + second + "s";
            var bar = Common.Find("Bar").Find("Cur").GetComponent<RectTransform>();
            int total = CurMode == Mode.Exchange ?
                ManagerNS.LocalGameManager.Instance.FeatureManager.Config.FeatTransTime :
                ManagerNS.LocalGameManager.Instance.FeatureManager.GetFeatureCorrectTime(FeatBuild.CorrectType);
            float sizeDeltaX = (float)(470 * (1 - time / total));
            bar.sizeDelta = new Vector2(sizeDeltaX, bar.sizeDelta.y);
        }
        #endregion
    }
}