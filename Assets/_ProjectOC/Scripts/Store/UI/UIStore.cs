using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.StoreNS.UI.UIStore;

namespace ProjectOC.StoreNS.UI
{
    public class UIStore : ML.Engine.UI.UIBasePanel<StorePanel>
    {
        #region Data
        #region Mode
        public enum Mode
        {
            Store,
            ChangeItem,
            ChangeIcon,
            Upgrade
        }
        private Mode curMode;
        public Mode CurMode
        {
            get => curMode;
            set
            {
                DataBtnList.DisableBtnList();
                ItemBtnList.DisableBtnList();
                curMode = value;
                if (curMode == Mode.Store)
                {
                    DataBtnList.EnableBtnList();
                }
                else if (curMode == Mode.ChangeItem || curMode == Mode.ChangeIcon)
                {
                    ItemBtnList.EnableBtnList();
                    string curItemID = curMode == Mode.ChangeItem ? Store.DataContainer.GetID(DataIndex) : Store.WorldIconItemID;
                    for (int i = 0; i < ItemBtnList.BtnCnt; ++i)
                    {
                        if (curItemID == ItemDatas[i])
                        {
                            ItemBtnList.MoveIndexIUISelected(i);
                            break;
                        }
                    }
                }
            }
        }
        public enum StoreMode
        {
            ChangeItem,
            ChangeIn,
            ChangeOut
        }
        public StoreMode CurStoreMode;
        #endregion

        public Store Store;
        public bool HasUpgrade;
        public List<string> ItemDatas = new List<string>();
        private bool ItemIsDestroyed = false;
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();

        #region BtnList
        private ML.Engine.UI.UIBtnList DataBtnList;
        private int DataIndex => DataBtnList?.GetCurSelectedPos1() ?? 0;
        private ML.Engine.UI.UIBtnList ItemBtnList;
        private int ItemIndex => ItemBtnList?.GetCurSelectedPos1() ?? 0;
        private ML.Engine.UI.UIBtnList RawBtnList;
        private bool IsInitBtnList;
        protected override void InitBtnInfo()
        {
            ML.Engine.Utility.Synchronizer synchronizer = new ML.Engine.Utility.Synchronizer(3, () => { IsInitBtnList = true; Refresh(); });
            DataBtnList = new ML.Engine.UI.UIBtnList(transform.Find("Store").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            DataBtnList.OnSelectButtonChanged += () => { Refresh(); };
            DataBtnList.ChangBtnNum(Store.DataContainer.GetCapacity(), "Prefab_Store_UI/Prefab_Store_UI_DataTemplate.prefab", () => { synchronizer.Check(); });

            ItemDatas = new List<string>() { "" };
            ItemDatas.AddRange(ManagerNS.LocalGameManager.Instance.StoreManager.GetStoreIconItems());
            ItemBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeItem").Find("Select").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            ItemBtnList.OnSelectButtonChanged += () => { Refresh(); };
            ItemBtnList.ChangBtnNum(ItemDatas.Count, "Prefab_Store_UI/Prefab_Store_UI_ItemTemplate.prefab", () => { synchronizer.Check(); });

            if (HasUpgrade)
            {
                var raw = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(Store.WorldStore.Classification.ToString());
                RawBtnList = new ML.Engine.UI.UIBtnList(transform.Find("Upgrade").Find("Raw").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
                RawBtnList.ChangBtnNum(raw.Count, "Prefab_Store_UI/Prefab_Store_UI_UpgradeRawTemplate.prefab", () => { synchronizer.Check(); });
            }
            else { synchronizer.Check(); }
        }
        protected void UpdateBtnInfo()
        {
            IsInitBtnList = false;
            ML.Engine.Utility.Synchronizer synchronizer = new ML.Engine.Utility.Synchronizer(2, () => { IsInitBtnList = true; Refresh(); });
            if (DataBtnList.BtnCnt != Store.DataContainer.GetCapacity())
            {
                DataBtnList.ChangBtnNum(Store.DataContainer.GetCapacity(), "Prefab_Store_UI/Prefab_Store_UI_DataTemplate.prefab", () => { synchronizer.Check(); });
            }
            else { synchronizer.Check(); }
            var raw = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(Store.WorldStore.Classification.ToString());
            if (HasUpgrade && RawBtnList.BtnCnt != raw.Count)
            {
                RawBtnList.ChangBtnNum(raw.Count, "Prefab_Store_UI/Prefab_Store_UI_UpgradeRawTemplate.prefab", () => { synchronizer.Check(); });
            }
            else { synchronizer.Check(); }
        }
        #endregion

        #region UI
        private MissionNS.TransportPriority CurPriority
        {
            get => Store.TransportPriority;
            set
            {
                if (Priority != null)
                {
                    Priority.Find("Selected").gameObject.SetActive(false);
                }
                Store.TransportPriority = value;
                Text_Priority.text = PanelTextContent.TransportPriority[(int)Store.TransportPriority];
                Priority = transform.Find("TopTitle").Find("Priority").GetChild((int)Store.TransportPriority);
                Priority.Find("Selected").gameObject.SetActive(true);
            }
        }
        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;
        private Image StoreIcon;
        private Transform Priority;
        private Transform Upgrade;
        private Transform Upgrade_Build;
        private Transform Upgrade_LvOld;
        private Transform Upgrade_LvNew;
        private Transform KeyTips;
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            InitTextContentPathData();
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            StoreIcon = transform.Find("TopTitle").Find("Icon").GetComponent<Image>();
            Text_Priority = transform.Find("TopTitle").Find("Priority").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            Upgrade = transform.Find("Upgrade");
            Upgrade_Build = Upgrade.Find("Build");
            Upgrade_LvOld = Upgrade.Find("Level").Find("LvOld");
            Upgrade_LvNew = Upgrade.Find("Level").Find("LvNew");

            KeyTips = transform.Find("BotKeyTips").Find("KeyTips");
            IsInit = true;
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct StorePanel
        {
            public ML.Engine.TextContent.TextContent text_Title;
            public ML.Engine.TextContent.TextContent text_Empty;
            public ML.Engine.TextContent.TextContent[] TransportPriority;
            public ML.Engine.TextContent.TextContent text_Add;
            public ML.Engine.TextContent.TextContent text_Remove;
            public ML.Engine.TextContent.TextContent text_LvDesc1;
            public ML.Engine.TextContent.TextContent text_LvDesc2;

            public ML.Engine.TextContent.KeyTip Upgrade;
            public ML.Engine.TextContent.KeyTip NextPriority;
            public ML.Engine.TextContent.KeyTip ChangeIcon;
            public ML.Engine.TextContent.KeyTip UpgradeConfirm;
            public ML.Engine.TextContent.KeyTip ChangeItem;
            public ML.Engine.TextContent.KeyTip Remove1;
            public ML.Engine.TextContent.KeyTip Remove10;
            public ML.Engine.TextContent.KeyTip Switch;
            public ML.Engine.TextContent.KeyTip FastAdd;
            public ML.Engine.TextContent.KeyTip Confirm;
            public ML.Engine.TextContent.KeyTip Back;
            public ML.Engine.TextContent.KeyTip UpgradeBack;
        }
        protected override void InitTextContentPathData()
        {
            abpath = "OCTextContent/Store";
            abname = "StorePanel";
            description = "StorePanel数据加载完成";
        }
        #endregion
        #endregion

        #region Override
        protected override void Enter()
        {
            Store.DataContainer.OnDataChangeEvent += Refresh;
            Store.IsInteracting = true;
            tempSprite.Add("", transform.Find("TopTitle").Find("Icon").GetComponent<Image>().sprite);
            base.Enter();
        }
        protected override void Exit()
        {
            Store.DataContainer.OnDataChangeEvent -= Refresh;
            Store.IsInteracting = false;
            tempSprite.Remove("");
            foreach (var s in tempSprite.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            tempSprite.Clear();
            base.Exit();
        }
        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Disable();
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed -= NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeIcon.performed -= ChangeIcon_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Upgrade.performed -= Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeItem.started -= ChangeItem_started;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed -= FastAdd_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove.canceled -= Remove_cancled;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove.performed -= Remove_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
        }
        protected override void RegisterInput()
        {
            DataBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
            DataBtnList.EnableBtnList();
            ItemBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Enable();
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed += NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeIcon.performed += ChangeIcon_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Upgrade.performed += Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeItem.started += ChangeItem_started;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed += FastAdd_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove.canceled += Remove_cancled;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove.performed += Remove_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
        }
        private void ChangeIcon_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode != Mode.ChangeIcon) { CurMode = Mode.ChangeIcon; }
            else { CurMode = Mode.Store; }
            Refresh();
        }
        private void Upgrade_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode != Mode.Upgrade && HasUpgrade) { CurMode = Mode.Upgrade; }
            else { CurMode = Mode.Store; }
            Refresh();
        }
        private void NextPriority_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurPriority = (MissionNS.TransportPriority)(((int)Store.TransportPriority + 1) % System.Enum.GetValues(typeof(MissionNS.TransportPriority)).Length);
        }
        private void ChangeItem_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                if (offset.x > 0)
                {
                    if ((int)CurStoreMode < System.Enum.GetValues(typeof(StoreMode)).Length - 1)
                    {
                        CurStoreMode++;
                        Refresh();
                    }
                }
                else if (offset.x < 0)
                {
                    if ((int)CurStoreMode > 0)
                    {
                        CurStoreMode--;
                        Refresh();
                    }
                }
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store && CurStoreMode == StoreMode.ChangeItem)
            {
                Store.FastAdd(DataIndex);
            }
        }
        private void Remove_cancled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store && CurStoreMode == StoreMode.ChangeItem)
            {
                if (ItemIsDestroyed)
                {
                    ItemIsDestroyed = false;
                }
                else
                {
                    Store.Remove(DataIndex, 1);
                }
            }
        }
        private void Remove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store && CurStoreMode == StoreMode.ChangeItem)
            {
                ItemIsDestroyed = true;
                int num = Store.DataContainer.GetAmount(DataIndex, DataNS.DataOpType.Storage);
                num = num < 10 ? num : 10;
                Store.Remove(DataIndex, num);
            }
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                ML.Engine.InventorySystem.ItemManager.Instance.AddItemIconObject(Store.WorldIconItemID, Store.WorldStore.transform,
                        new Vector3(0, Store.WorldStore.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0),
                        Quaternion.Euler(Vector3.zero), Vector3.one,
                        (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
                UIMgr.PopPanel();
            }
            else if (CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon || CurMode == Mode.Upgrade)
            {
                CurMode = Mode.Store;
                Refresh();
            }
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                if (CurStoreMode == StoreMode.ChangeItem)
                {
                    CurMode = Mode.ChangeItem;
                }
                else if (CurStoreMode == StoreMode.ChangeIn)
                {
                    Store.DataContainer.ChangeCanIn(DataIndex, !Store.DataContainer.GetCanIn(DataIndex));
                }
                else if (CurStoreMode == StoreMode.ChangeOut)
                {
                    Store.DataContainer.ChangeCanOut(DataIndex, !Store.DataContainer.GetCanOut(DataIndex));
                }
            }
            else if (CurMode == Mode.ChangeItem)
            {
                Store.ChangeData(DataIndex, new DataNS.ItemIDDataObj(ItemDatas[ItemIndex]));
                CurMode = Mode.Store;
            }
            else if (CurMode == Mode.ChangeIcon)
            {
                Store.WorldIconItemID = ItemDatas[ItemIndex];
            }
            else if (CurMode == Mode.Upgrade)
            {
                ML.Engine.BuildingSystem.BuildingManager.Instance.Upgrade(Store.WorldStore);
                UpdateBtnInfo();
            }
            Refresh();
        }
        #endregion

        #region UI
        protected void SetUIActive()
        {
            transform.Find("Store").gameObject.SetActive(CurMode == Mode.Store);
            transform.Find("ChangeItem").gameObject.SetActive(CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon);
            Upgrade.gameObject.SetActive(CurMode == Mode.Upgrade);
            KeyTips.gameObject.SetActive(CurMode == Mode.Store);
            transform.Find("BotKeyTips").Find("KeyTips1").gameObject.SetActive(CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon);
            transform.Find("BotKeyTips").Find("KeyTips2").gameObject.SetActive(CurMode == Mode.Upgrade);

            bool IsStoreModeChangeItem = CurStoreMode == StoreMode.ChangeItem;
            KeyTips.Find("KT_ChangeItem").gameObject.SetActive(IsStoreModeChangeItem);
            KeyTips.Find("KT_Remove1").gameObject.SetActive(IsStoreModeChangeItem);
            KeyTips.Find("KT_Remove10").gameObject.SetActive(IsStoreModeChangeItem);
            KeyTips.Find("KT_FastAdd").gameObject.SetActive(IsStoreModeChangeItem);
            KeyTips.Find("KT_Switch").gameObject.SetActive(!IsStoreModeChangeItem);
            transform.Find("TopTitle").Find("KT_Upgrade").gameObject.SetActive(HasUpgrade);
        }

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            CurPriority = Store.TransportPriority;
            Store.WorldIconItemID = Store.WorldIconItemID ?? "";
            if (!tempSprite.ContainsKey(Store.WorldIconItemID))
            {
                tempSprite[Store.WorldIconItemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(Store.WorldIconItemID);
            }
            StoreIcon.sprite = tempSprite[Store.WorldIconItemID];
            SetUIActive();
            if (CurMode == Mode.Store)
            {
                Text_Title.text = PanelTextContent.text_Title.GetText();
                for (int i = 0; i < DataBtnList.BtnCnt; ++i)
                {
                    var uiStoreData = DataBtnList.GetBtn(i);
                    string itemID = Store.DataContainer.GetID(i);
                    int maxCapacity = Store.DataContainer.GetAmount(i, DataNS.DataOpType.MaxCapacity);
                    var img = uiStoreData.transform.Find("Icon").GetComponent<Image>();
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                    }
                    img.sprite = tempSprite[itemID];
                    uiStoreData.transform.Find("Select").gameObject.SetActive(CurStoreMode == StoreMode.ChangeItem);
                    var nametext = uiStoreData.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    nametext.text = itemID != "" ? ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(itemID) : PanelTextContent.text_Empty;
                    // Amount
                    int amount = Store.DataContainer.GetAmount(i, DataNS.DataOpType.Storage);
                    int amountMax = maxCapacity;
                    var AmountCur = uiStoreData.transform.Find("Amount").Find("Cur").GetComponent<TMPro.TextMeshProUGUI>();
                    var AmountMax = uiStoreData.transform.Find("Amount").Find("Max").GetComponent<TMPro.TextMeshProUGUI>();
                    AmountCur.text = amount.ToString();
                    AmountMax.text = amountMax.ToString();

                    #region ProgressBar
                    Transform progressBar1 = uiStoreData.transform.Find("ProgressBar1");
                    RectTransform bar1Cur1 = progressBar1.Find("Cur").GetComponent<RectTransform>();
                    RectTransform bar1Cur2 = progressBar1.Find("Cur1").GetComponent<RectTransform>();
                    RectTransform bar1Cur3 = progressBar1.Find("Cur2").GetComponent<RectTransform>();
                    Transform progressBar2 = uiStoreData.transform.Find("ProgressBar2");
                    RectTransform bar2Cur1 = progressBar2.Find("Cur").GetComponent<RectTransform>();
                    RectTransform bar2Cur2 = progressBar2.Find("Cur1").GetComponent<RectTransform>();
                    RectTransform bar2Cur3 = progressBar2.Find("Cur2").GetComponent<RectTransform>();
                    Transform progressBar3 = uiStoreData.transform.Find("ProgressBar3");
                    RectTransform bar3Cur1 = progressBar3.Find("Cur").GetComponent<RectTransform>();
                    RectTransform bar3Cur2 = progressBar3.Find("Cur1").GetComponent<RectTransform>();
                    RectTransform bar3Cur3 = progressBar3.Find("Cur2").GetComponent<RectTransform>();
                    List<RectTransform> cur1 = new List<RectTransform>() { bar1Cur1, bar2Cur1, bar3Cur1 };
                    List<RectTransform> cur2 = new List<RectTransform>() { bar1Cur2, bar2Cur2, bar3Cur2 };
                    List<RectTransform> cur3 = new List<RectTransform>() { bar1Cur3, bar2Cur3, bar3Cur3 };
                    foreach (RectTransform rect in cur1)
                    {
                        rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
                    }
                    foreach (RectTransform rect in cur2)
                    {
                        rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
                    }
                    foreach (RectTransform rect in cur3)
                    {
                        rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
                    }

                    int level = Store.Level + 1;
                    progressBar1.Find("None").gameObject.SetActive(level < 1);
                    progressBar2.Find("None").gameObject.SetActive(level < 2);
                    progressBar3.Find("None").gameObject.SetActive(level < 3);
                    float storage = level * (float)amount / amountMax;
                    storage = storage <= 3 ? storage : 3;
                    int storageStage = (int)storage;
                    for (int stage = 0; stage < storageStage; stage++)
                    {
                        cur1[stage].sizeDelta = new Vector2(400, cur1[stage].sizeDelta.y);
                    }
                    float curPos = storage - storageStage;
                    int curStage = storageStage;
                    if (curPos > 0 && curStage <= 2)
                    {
                        cur1[curStage].sizeDelta = new Vector2(400 * curPos, cur1[curStage].sizeDelta.y);
                    }
                    float storageReserve = level * (float)Store.DataContainer.GetAmount(i, DataNS.DataOpType.StorageReserve) / amountMax;
                    storageReserve = storageReserve <= 3 ? storageReserve : 3;
                    int storageReserveStage = (int)(storage + storageReserve);
                    for (int stage = curStage; stage < storageReserveStage; stage++)
                    {
                        cur2[stage].sizeDelta = new Vector2(400, cur2[stage].sizeDelta.y);
                    }
                    curPos = (storage + storageReserve) - storageReserveStage;
                    curStage = storageReserveStage;
                    if (curPos > 0 && curStage <= 2)
                    {
                        cur2[curStage].sizeDelta = new Vector2(400 * curPos, cur2[curStage].sizeDelta.y);
                    }
                    float emptyReserve = level * (float)Store.DataContainer.GetAmount(i, DataNS.DataOpType.EmptyReserve) / amountMax;
                    emptyReserve = emptyReserve <= 3 ? emptyReserve : 3;
                    int emptyReserveStage = (int)(storage + storageReserve + emptyReserve);
                    for (int stage = curStage; stage < emptyReserveStage; stage++)
                    {
                        cur3[stage].sizeDelta = new Vector2(400, cur3[stage].sizeDelta.y);
                    }
                    curPos = (storage + storageReserve + emptyReserve) - emptyReserveStage;
                    curStage = emptyReserveStage;
                    if (curPos > 0 && curStage <= 2)
                    {
                        cur3[curStage].sizeDelta = new Vector2(400 * curPos, cur3[curStage].sizeDelta.y);
                    }
                    #endregion
                    // Add and Remove
                    uiStoreData.transform.Find("Add").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_Add.GetText();
                    uiStoreData.transform.Find("Remove").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_Remove.GetText();
                    uiStoreData.transform.Find("Add").Find("Tick").gameObject.SetActive(Store.DataContainer.GetCanIn(i));
                    uiStoreData.transform.Find("Remove").Find("Tick").gameObject.SetActive(Store.DataContainer.GetCanOut(i));
                    // Selected
                    var selectIcon = uiStoreData.transform.Find("Select");
                    var selectAdd = uiStoreData.transform.Find("Add").Find("Select");
                    var selectARemove = uiStoreData.transform.Find("Remove").Find("Select");
                    bool isCurIndex = (DataIndex == i);
                    selectIcon.gameObject.SetActive(isCurIndex && CurStoreMode == StoreMode.ChangeItem);
                    selectAdd.gameObject.SetActive(isCurIndex && CurStoreMode == StoreMode.ChangeIn);
                    selectARemove.gameObject.SetActive(isCurIndex && CurStoreMode == StoreMode.ChangeOut);
                }
            }
            else if (CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon)
            {
                for (int i = 0; i < ItemBtnList.BtnCnt; ++i)
                {
                    var uiItemData = ItemBtnList.GetBtn(i);
                    string itemID = ItemDatas[i];
                    var img = uiItemData.transform.Find("Icon").GetComponent<Image>();
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                    }
                    img.sprite = tempSprite[itemID];
                }
            }
            else if (CurMode == Mode.Upgrade)
            {
                #region Build
                string buildCID = Store.WorldStore.Classification.ToString();
                string buildID = ML.Engine.BuildingSystem.BuildingManager.Instance.GetID(buildCID);
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = ML.Engine.BuildingSystem.BuildingManager.Instance.GetBuildIcon(buildID);
                }
                Upgrade_Build.Find("Icon").GetComponent<Image>().sprite = tempSprite[buildID];
                Upgrade_Build.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.BuildingSystem.BuildingManager.Instance.GetName(buildCID) ?? "";
                #endregion

                #region Raw
                bool flagUpgradeBtn = true;
                var raw = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(buildCID);
                for (int i = 0; i < RawBtnList.BtnCnt; ++i)
                {
                    if (i >= raw.Count) { break; }
                    var uiItemData = RawBtnList.GetBtn(i);
                    string itemID = raw[i].id;
                    int need = raw[i].num;
                    int current = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).InventoryItemAmount(itemID);
                    var img = uiItemData.transform.Find("Icon").GetComponent<Image>();
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                    }
                    img.sprite = tempSprite[itemID];

                    uiItemData.transform.Find("Background3").gameObject.SetActive(current < need);

                    var nametext = uiItemData.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    var amounttext = uiItemData.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    var needtext = uiItemData.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                    uiItemData.transform.Find("Background3").gameObject.SetActive(current < need);
                    flagUpgradeBtn = current >= need;
                    if (itemID != "")
                    {
                        nametext.text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(itemID);
                        amounttext.text = current.ToString();
                        needtext.text = need.ToString();
                    }
                    else
                    {
                        nametext.text = PanelTextContent.text_Empty;
                        amounttext.text = "0";
                        needtext.text = "0";
                    }
                }
                Upgrade.Find("BtnBackground1").gameObject.SetActive(flagUpgradeBtn);
                #endregion

                #region Level
                Upgrade_LvOld.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: " + Store.Level.ToString();
                int capacity = ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelCapacity[Store.Level];
                int dataCapacity = ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelDataCapacity[Store.Level];
                Upgrade_LvOld.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc1 + capacity;
                Upgrade_LvOld.Find("Desc1").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc2 + dataCapacity;

                if (Store.Level + 1 <= ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelMax)
                {
                    Upgrade.Find("BtnBackground").gameObject.SetActive(true);
                    Upgrade.Find("KT_UpgradeConfirm").gameObject.SetActive(true);

                    Upgrade_Build.Find("Image").gameObject.SetActive(true);
                    Upgrade_LvNew.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: " + (Store.Level + 1).ToString();
                    capacity = ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelCapacity[Store.Level + 1];
                    dataCapacity = ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelDataCapacity[Store.Level + 1];
                    Upgrade_LvNew.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc1 + capacity;
                    Upgrade_LvNew.Find("Desc1").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc2 + dataCapacity;
                }
                else
                {
                    Upgrade.Find("BtnBackground").gameObject.SetActive(false);
                    Upgrade.Find("BtnBackground1").gameObject.SetActive(false);
                    Upgrade.Find("KT_UpgradeConfirm").gameObject.SetActive(false);

                    Upgrade_Build.Find("Image").gameObject.SetActive(false);
                    Upgrade_LvOld.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: MAX";
                    Upgrade_LvNew.gameObject.SetActive(false);
                }
                #endregion
            }
        }
        #endregion
    }
}