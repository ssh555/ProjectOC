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
                curMode = value;
                if (IsChangeWorker)
                {
                    Workers.Clear();
                    Workers.Add(null);
                    Workers.AddRange(ManagerNS.LocalGameManager.Instance.WorkerManager.GetNotBanWorkers());
                    IsInitBtnList = false;
                    WorkerBtnList.ChangBtnNum(Workers.Count, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureWorkerTemplate.prefab", 
                        () => { IsInitBtnList = true; WorkerBtnList.MoveIndexIUISelected(0); Refresh(); });
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
            }
        }
        public bool IsExchange => curMode <= Mode.ChangeWorkerSeat2;
        public bool IsCorrect => Mode.ChangeWorkerCorrect <= curMode;
        public bool IsChangeWorker => Mode.ChangeWorkerSeat1 <= curMode && curMode <= Mode.ChangeWorkerCorrect;
        public bool IsOnMain => curMode == Mode.Exchange || curMode == Mode.Correct;
        public enum ExchangeMode
        {
            Seat1,
            Seat2,
            Run
        }
        public ExchangeMode CurExchangeMode;
        public enum CorrectMode
        {
            Item,
            Seat,
            Run
        }
        public CorrectMode CurCorrectMode;
        public int ChangeCorrectSelectIndex;
        #endregion

        public FeatureBuilding FeatBuild;
        public bool IsSeeInfo;
        private TMPro.TextMeshProUGUI Text_Title;
        private Transform Select;
        private Transform Exchange;
        private Transform Correct;
        private Transform ChangeWorker;
        private Transform ChangeCorrect;
        private Transform KeyTips;
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();

        #region BtnList
        private ML.Engine.UI.UIBtnList ExchangeFeature1BtnList;
        private ML.Engine.UI.UIBtnList ExchangeFeature2BtnList;
        private ML.Engine.UI.UIBtnList CorrectFeatureBtnList;
        private ML.Engine.UI.UIBtnList WorkerBtnList;
        private ML.Engine.UI.UIBtnList ChangeWorkerFeatureBtnList;
        private int WorkerIndex => WorkerBtnList?.GetCurSelectedPos1() ?? 0;
        private List<Worker> Workers = new List<Worker>();
        private bool IsInitBtnList = false;
        protected override void InitBtnInfo()
        {
            ML.Engine.UI.UIBtnList.Synchronizer synchronizer = new ML.Engine.UI.UIBtnList.Synchronizer(5, () => { IsInitBtnList = true; Refresh(); });
            ExchangeFeature1BtnList = new ML.Engine.UI.UIBtnList(transform.Find("Exchange").Find("Feature1").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            ExchangeFeature2BtnList = new ML.Engine.UI.UIBtnList(transform.Find("Exchange").Find("Feature2").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            CorrectFeatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find("Correct").Find("Feature").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            ChangeWorkerFeatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeWorker").Find("Feature").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            ExchangeFeature1BtnList.ChangBtnNum(3, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab", () => { synchronizer.Check(); });
            ExchangeFeature2BtnList.ChangBtnNum(3, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab", () => { synchronizer.Check(); });
            CorrectFeatureBtnList.ChangBtnNum(3, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab", () => { synchronizer.Check(); });
            ChangeWorkerFeatureBtnList.ChangBtnNum(3, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab", () => { synchronizer.Check(); });
            WorkerBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeWorker").Find("Select").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            WorkerBtnList.OnSelectButtonChanged += () => { Refresh(); };
            WorkerBtnList.ChangBtnNum(0, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureWorkerTemplate.prefab", () => { synchronizer.Check(); });
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
            public ML.Engine.TextContent.TextContent textWarnSelectWorker;
            public ML.Engine.TextContent.TextContent textWarnLackExchangeItem;
            public ML.Engine.TextContent.TextContent textCorrectTimePrefix;

            public ML.Engine.TextContent.KeyTip SelectPre;
            public ML.Engine.TextContent.KeyTip SelectPost;
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
            ManagerNS.LocalGameManager.Instance.WorkerManager.OnDeleteWorkerEvent += OnDeleteWorkerEvent;
            tempSprite.Add("", transform.Find("Exchange").Find("Worker1").Find("Icon").GetComponent<Image>().sprite);
            tempSprite.Add("WorkerIcon", ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_Beast"));
            var config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
            tempSprite.Add("ExchangeItem", ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatTransCostItemID));
            tempSprite.Add("UpItem", ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatUpCostItemID));
            tempSprite.Add("DownItem", ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatDownCostItemID));
            tempSprite.Add("DelItem", ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatDelCostItemID));
            tempSprite.Add("ReverseItem", ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(config.FeatReverseCostItemID));
            base.Enter();
        }
        protected override void Exit()
        {
            ManagerNS.LocalGameManager.Instance.WorkerManager.OnDeleteWorkerEvent -= OnDeleteWorkerEvent;
            tempSprite.Remove("");
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
            }
            base.Exit();
        }
        private void OnDeleteWorkerEvent(Worker worker)
        {
            if (IsChangeWorker)
            {
                Workers.Remove(worker);
                IsInitBtnList = false;
                WorkerBtnList.ChangBtnNum(Workers.Count, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureWorkerTemplate.prefab",
                    () => { IsInitBtnList = true; Refresh(); });
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
            if (CurMode == Mode.Exchange)
            {
                if (CurExchangeMode == ExchangeMode.Seat1)
                {
                    CurMode = Mode.ChangeWorkerSeat1;
                }
                else if (CurExchangeMode == ExchangeMode.Seat2)
                {
                    CurMode = Mode.ChangeWorkerSeat2;
                }
                else if (FeatBuild.IsExchangeEnd)
                {
                    FeatBuild.ConfirmExchange();
                }
            }
            else if (CurMode == Mode.Correct)
            {
                if (CurCorrectMode == CorrectMode.Item)
                {
                    CurMode = Mode.ChangeCorrect;
                }
                else if (CurCorrectMode == CorrectMode.Seat)
                {
                    CurMode = Mode.ChangeWorkerCorrect;
                }
                else if (FeatBuild.IsCorrectEnd)
                {
                    FeatBuild.ConfirmCorrect();
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
                    FeatBuild.ChangeWorker(index, Workers[WorkerIndex]);
                    CurMode = Mode.Exchange;
                }
                else
                {
                    FeatBuild.ChangeCorrectWorker(Workers[WorkerIndex]);
                    CurMode = Mode.Correct;
                }
            }
            Refresh();
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Exchange || CurMode == Mode.Correct)
            {
                UIMgr.PopPanel();
            }
            else
            {
                CurMode = IsExchange ? Mode.Exchange : Mode.Correct;
                Refresh();
            }
        }
        private void SeeInfo_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
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
            if (CurMode == Mode.Exchange && offset.x != 0)
            {
                CurExchangeMode = offset.x > 0 ? ExchangeMode.Seat1 : ExchangeMode.Seat2;
            }
            else if (CurMode == Mode.Correct && offset.x != 0)
            {
                CurCorrectMode = offset.x > 0 ? CorrectMode.Item : CorrectMode.Seat;
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
                Refresh();
            }
        }
        private void FeatConfirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
        }
        private void FeatCancle_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
        }
        #endregion

        #region UI
        protected void SetUIActive()
        {
            bool isExchange = IsExchange;
            Exchange.gameObject.SetActive(isExchange);
            Select.Find("Exchange").gameObject.SetActive(!isExchange);
            Select.Find("ExchangeBig").gameObject.SetActive(isExchange);
            Select.Find("Correct").gameObject.SetActive(isExchange);
            Select.Find("CorrectBig").gameObject.SetActive(!isExchange);
            Correct.gameObject.SetActive(IsCorrect);
            ChangeWorker.gameObject.SetActive(IsChangeWorker);
            ChangeCorrect.gameObject.SetActive(CurMode == Mode.ChangeCorrect);
            //BotKeyTips.Find("KT_Remove1").gameObject.SetActive(hasSetFood);
            //BotKeyTips.Find("KT_Remove10").gameObject.SetActive(hasSetFood);
            //BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(hasSetFood);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
        }
        public void RefreshFeatureBtnList(ML.Engine.UI.UIBtnList btnList, FeatureSeat seat, bool checkNew = false, bool checkCanCorrect = false, bool isReverseEnd = false)
        {
            for (int i = 0; i < 3; i++)
            {
                var feat = btnList.GetBtn(i).transform;
                feat.Find("Normal").gameObject.SetActive(false);
                feat.Find("Specific").gameObject.SetActive(false);
            }
            if (seat.HaveWorker)
            {
                List<Feature> workerFeatures = seat.Worker.GetFeatures(true);
                for (int i = 0; i < seat.FeatureIDs.Count; i++)
                {
                    string featID = seat.FeatureIDs[i];
                    Transform feat = IsSeeInfo ? btnList.GetBtn(i).transform.Find("Specific") : btnList.GetBtn(i).transform.Find("Normal");
                    feat.gameObject.SetActive(true);
                    FeatureManager featManager = ManagerNS.LocalGameManager.Instance.FeatureManager;
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
                        bool isBuff = featManager.GetFeatureType(featID) == FeatureType.Buff;
                        bool isDeBuff = featManager.GetFeatureType(featID) == FeatureType.DeBuff;
                        if (IsSeeInfo)
                        {
                            feat.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = featManager.GetItemDescription(featID);
                            Color color = isBuff ? Color.green : Color.red;
                            if (isReverseEnd)
                            {
                                if (featManager.GetFeatureType(workerFeatures[i].ID) == FeatureType.Buff && isDeBuff)
                                {
                                    color = new Color(0.5f, 0f, 0.5f);
                                }
                                if (featManager.GetFeatureType(workerFeatures[i].ID) == FeatureType.DeBuff && isBuff)
                                {
                                    color = new Color(1f, 0.843f, 0f);
                                }
                            }

                        }
                        string effectDesc = featManager.GetEffectsDescription(featID);
                        if (isBuff)
                        {
                            feat.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = "<color=green>" + effectDesc + "</color>";
                        }
                        else if (isDeBuff)
                        {
                            feat.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = "<color=red>" + effectDesc + "</color>";
                        }
                        else
                        {
                            feat.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = effectDesc;
                        }
                    }
                    feat.Find("IconNew").gameObject.SetActive(checkNew && !seat.Worker.Feature.ContainsKey(featID));
                    feat.Find("IconNew1").gameObject.SetActive(checkCanCorrect && featManager.GetCanCorrect(featID, FeatBuild.CorrectType));
                }
                for (int i = seat.FeatureIDs.Count; i < seat.Worker.Feature.Count; i++)
                {
                    Transform feat = IsSeeInfo ? btnList.GetBtn(i).transform.Find("Specific") : btnList.GetBtn(i).transform.Find("Normal");
                    feat.Find("IconNew").gameObject.SetActive(checkNew);
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
            transform.Find("Icon").GetComponent<Image>().sprite = tempSprite["ExchangeItem"];
            transform.Find("Amount").Find("Cur").GetComponent<TMPro.TextMeshProUGUI>().text = cur.ToString();
            transform.Find("Amount").Find("Need").GetComponent<TMPro.TextMeshProUGUI>().text = need.ToString();
            transform.Find("Back2").gameObject.SetActive(cur < need);
            transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemName(id);
            if (transform.Find("Selected") != null)
            {
                transform.Find("Selected").gameObject.SetActive(isSelect);
            }
        }

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            var config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
            Text_Title.text = PanelTextContent.textTitle;
            Select.Find("TextExchange").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textExchange;
            Select.Find("TextCorrect").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textCorrect;
            SetUIActive();
            if (CurMode == Mode.Exchange)
            {
                RefreshFeatureBtnList(ExchangeFeature1BtnList, FeatBuild.Seats[0], FeatBuild.IsExchangeEnd);
                RefreshFeatureBtnList(ExchangeFeature2BtnList, FeatBuild.Seats[1], FeatBuild.IsExchangeEnd);
                RefreshWorker(Exchange.Find("Worker1"), FeatBuild.Seats[0].Worker, CurExchangeMode == ExchangeMode.Seat1);
                RefreshWorker(Exchange.Find("Worker2"), FeatBuild.Seats[1].Worker, CurExchangeMode == ExchangeMode.Seat2);
                int cur = ManagerNS.LocalGameManager.Instance.Player.InventoryItemAmount(config.FeatTransCostItemID);
                int need = config.FeatTransCostItemNum;
                RefreshItem(Exchange.Find("Item"), config.FeatTransCostItemID, cur, need);
                Exchange.Find("Bar").gameObject.SetActive(CurExchangeMode == ExchangeMode.Run);
                bool canExchange = FeatBuild.CanExhcange();
                Exchange.Find("BtnConfirm").gameObject.SetActive(CurExchangeMode != ExchangeMode.Run);
                Exchange.Find("BtnConfirm").Find("Back2").gameObject.SetActive(!canExchange);
                Exchange.Find("Warn").gameObject.SetActive(!canExchange);
                bool isSelectWorker = FeatBuild.Seats[0].HaveWorker && FeatBuild.Seats[1].HaveWorker;
                bool isEnoughItem = cur >= need;
                Exchange.Find("Warn").Find("Warn1").gameObject.SetActive(!isSelectWorker);
                Exchange.Find("Warn").Find("Warn2").gameObject.SetActive(!isEnoughItem);
                Exchange.Find("Warn").Find("Warn1").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = isSelectWorker ? "" : PanelTextContent.textWarnSelectWorker;
                Exchange.Find("Warn").Find("Warn2").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = isEnoughItem ? "" : PanelTextContent.textWarnLackExchangeItem;
                LayoutRebuilder.ForceRebuildLayoutImmediate(Exchange.Find("Warn").GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
                Exchange.Find("BtnConfirmResult").gameObject.SetActive(CurExchangeMode == ExchangeMode.Run);
                Exchange.Find("BtnCancleResult").gameObject.SetActive(CurExchangeMode == ExchangeMode.Run);
                Exchange.Find("BtnCancle").gameObject.SetActive(CurExchangeMode == ExchangeMode.Run && FeatBuild.IsExchangeEnd);
                if (CurExchangeMode != ExchangeMode.Run)
                {
                    int minute = config.FeatTransTime / 60;
                    int second = config.FeatTransTime % 60;
                    Exchange.Find("Time").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textExchangeTimePrefix + minute + "min" + second + "s";
                }
            }
            else if (CurMode == Mode.Correct)
            {
                RefreshFeatureBtnList(CorrectFeatureBtnList, FeatBuild.Seat, FeatBuild.IsCorrectEnd, !FeatBuild.IsCorrectEnd);
            }
            // var bar = uidata.Find("Bar").Find("Cur").GetComponent<RectTransform>();
            // float sizeDeltaX = uidata.Find("Bar").GetComponent<RectTransform>().sizeDelta.x * amount / maxCapacity;
            // bar.sizeDelta = new Vector2(sizeDeltaX, bar.sizeDelta.y);
        }
        #endregion
    }
}
