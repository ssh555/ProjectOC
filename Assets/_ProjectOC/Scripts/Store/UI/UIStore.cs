using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.StoreNS.UI.UIStore;
using ML.Engine.UI;
using System.Linq;

namespace ProjectOC.StoreNS.UI
{
    public class UIStore : ML.Engine.UI.UIBasePanel<StorePanel>
    {
        #region Data
        public enum Mode
        {
            Store = 0,
            ChangeItem = 1,
            ChangeIcon = 2,
            Upgrade = 3
        }
        public Mode CurMode = Mode.Store;
        public enum StoreMode
        {
            ChangeItem = 0,
            ChangeIn = 1,
            ChangeOut = 2
        }
        public StoreMode CurStoreMode = StoreMode.ChangeItem;
        public Store Store;
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

        public bool HasUpgrade;

        private bool ItemIsDestroyed = false;

        #region Unity
        public bool IsInit = false;
        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;
        private Image StoreIcon;
        private Sprite EmptySprite;
        private Transform Priority;
        private Transform StoreTransform;
        private Transform ChangeItem;
        private Transform Upgrade;
        private Transform Upgrade_Build;
        private Transform Upgrade_LvOld;
        private Transform Upgrade_LvNew;
        private Transform BotKeyTips_KeyTips;
        private Transform BotKeyTips_ChangeItem;
        private Transform BotKeyTips_Upgrade;
        protected override void Start()
        {
            base.Start();

            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            StoreIcon = transform.Find("TopTitle").Find("Icon").GetComponent<Image>();
            EmptySprite = StoreIcon.sprite;
            Text_Priority = transform.Find("TopTitle").Find("Priority").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            StoreTransform = transform.Find("Store");
            ChangeItem = transform.Find("ChangeItem");

            Upgrade = transform.Find("Upgrade");
            Upgrade_Build = Upgrade.Find("Build");
            Upgrade_LvOld = Upgrade.Find("Level").Find("LvOld");
            Upgrade_LvNew = Upgrade.Find("Level").Find("LvNew");

            BotKeyTips_KeyTips = transform.Find("BotKeyTips").Find("KeyTips");
            BotKeyTips_ChangeItem = transform.Find("BotKeyTips").Find("ChangeItem");
            BotKeyTips_Upgrade = transform.Find("BotKeyTips").Find("Upgrade");
            BotKeyTips_ChangeItem.gameObject.SetActive(false);

            IsInit = true;
            Refresh();
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct StorePanel
        {
            public TextContent text_Title;
            public TextContent text_Empty;
            public TextContent[] TransportPriority;
            public TextContent text_Add;
            public TextContent text_Remove;
            public TextContent text_LvDesc1;
            public TextContent text_LvDesc2;

            public KeyTip Upgrade;
            public KeyTip NextPriority;
            public KeyTip ChangeIcon;
            public KeyTip UpgradeConfirm;
            public KeyTip ChangeItem;
            public KeyTip Remove1;
            public KeyTip Remove10;
            public KeyTip Switch;
            public KeyTip FastAdd;
            public KeyTip Confirm;
            public KeyTip Back;
            public KeyTip UpgradeBack;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/Store";
            this.abname = "StorePanel";
            this.description = "StorePanel数据加载完成";
        }
        #endregion
        #endregion

        #region Override
        protected override void Enter()
        {
            Store.StoreDatas.OnDataChangeEvent += Refresh;
            Store.IsInteracting = true;
            base.Enter();
        }

        protected override void Exit()
        {
            Store.StoreDatas.OnDataChangeEvent -= Refresh;
            Store.IsInteracting = false;
            ClearTemp();
            base.Exit();
        }

        private UIBtnList DataBtnList;
        private UIBtnList ItemBtnList;
        private UIBtnList UPRawBtnList;
        private UIBtnList.Synchronizer synchronizer;
        private bool IsInitBtnList = false;

        protected override void InitBtnInfo()
        {
            synchronizer = new UIBtnList.Synchronizer(3, () => { IsInitBtnList = true; Refresh(); });
            DataBtnList = new UIBtnList(transform.Find("Store").Find("Viewport").GetComponentInChildren<UIBtnListInitor>());
            DataBtnList.OnSelectButtonChanged += () => { Refresh(); };
            DataBtnList.ChangBtnNum(1, "Prefab_Store_UI/Prefab_Store_UI_DataTemplate.prefab", ()=> { synchronizer.Check(); });

            ItemBtnList = new UIBtnList(transform.Find("ChangeItem").Find("Select").Find("Viewport").GetComponentInChildren<UIBtnListInitor>());
            ItemBtnList.OnSelectButtonChanged += () => { Refresh(); };
            ItemBtnList.ChangBtnNum(1, "Prefab_Store_UI/Prefab_Store_UI_ItemTemplate.prefab", () => { synchronizer.Check(); });

            UPRawBtnList = new UIBtnList(transform.Find("Upgrade").Find("Raw").Find("Viewport").GetComponentInChildren<UIBtnListInitor>());
            UPRawBtnList.OnSelectButtonChanged += () => { Refresh(); };
            UPRawBtnList.ChangBtnNum(1, "Prefab_Store_UI/Prefab_Store_UI_UpgradeRawTemplate.prefab", () => { synchronizer.Check(); });
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
            DataBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            DataBtnList.EnableBtnList();
            ItemBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            UPRawBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);

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
            if (CurMode != Mode.ChangeIcon)
            {
                CurMode = Mode.ChangeIcon;
                //this.IsInitCurItemIndex = false;
                //this.ItemDatas.Clear();
            }
            else
            {
                CurMode = Mode.Store;
            }
            Refresh();
        }
        private void Upgrade_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode != Mode.Upgrade && HasUpgrade)
            {
                CurMode = Mode.Upgrade;
            }
            else
            {
                CurMode = Mode.Store;
            }
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
                        this.CurStoreMode++;
                        Refresh();
                    }
                }
                else if (offset.x < 0)
                {
                    if ((int)CurStoreMode > 0)
                    {
                        this.CurStoreMode--;
                        Refresh();
                    }
                }
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store && CurStoreMode == StoreMode.ChangeItem)
            {
                Store.FastAdd(DataBtnList.GetCurSelectedPos1());
                Refresh();
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
                    Store.Remove(DataBtnList.GetCurSelectedPos1(), 1);
                    Refresh();
                }
            }
        }
        private void Remove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store && CurStoreMode == StoreMode.ChangeItem)
            {
                ItemIsDestroyed = true;
                int num = Store.GetAmount(DataBtnList.GetCurSelectedPos1(), DataNS.DataOpType.Storage);
                num = num < 10 ? num : 10;
                Store.Remove(DataBtnList.GetCurSelectedPos1(), num);
                Refresh();
            }
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                ItemManager.Instance.AddItemIconObject(Store.WorldIconItemID, Store.WorldStore.transform,
                                                        new Vector3(0, this.Store.WorldStore.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0),
                                                        Quaternion.Euler(Vector3.zero), Vector3.one,
                                                        (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
                UIMgr.PopPanel();
            }
            else if (CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon || CurMode == Mode.Upgrade)
            {
                this.CurMode = Mode.Store;
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
                    Store.StoreDatas[CurrentDataIndex].CanIn = !Store.StoreDatas[CurrentDataIndex].CanIn;
                }
                else if (CurStoreMode == StoreMode.ChangeOut)
                {
                    Store.StoreDatas[CurrentDataIndex].CanOut = !Store.StoreDatas[CurrentDataIndex].CanOut;
                }
            }
            else if (CurMode == Mode.ChangeItem)
            {
                Store.ChangeStoreData(CurrentDataIndex, CurrentItemData);
                this.CurMode = Mode.Store;
            }
            else if (CurMode == Mode.ChangeIcon)
            {
                string itemID = CurrentItemData;
                this.Store.WorldIconItemID = itemID;
            }
            else if (CurMode == Mode.Upgrade)
            {
                ML.Engine.BuildingSystem.BuildingManager.Instance.Upgrade(Store.WorldStore);
            }
            Refresh();
        }
        #endregion

        #region UI
        #region Temp
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        private List<GameObject> uiStoreDatas = new List<GameObject>();
        private List<GameObject> tempUIItemDatas = new List<GameObject>();
        private List<GameObject> tempUIItemDatasUpgrade = new List<GameObject>();

        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
            }
            foreach (var s in uiStoreDatas)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItemDatas)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItemDatasUpgrade)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
        }
        #endregion
        public override void Refresh()
        {
            // 加载完成JSON数据 & 查找完所有引用
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }

            // StoreIcon
            if (!string.IsNullOrEmpty(Store.WorldIconItemID))
            {
                if (!tempSprite.ContainsKey(Store.WorldIconItemID))
                {
                    tempSprite[Store.WorldIconItemID] = ItemManager.Instance.GetItemSprite(Store.WorldIconItemID);
                }
                StoreIcon.GetComponent<Image>().sprite = tempSprite[Store.WorldIconItemID];
            }
            else
            {
                StoreIcon.GetComponent<Image>().sprite = EmptySprite;
            }

            this.StoreTransform.gameObject.SetActive(CurMode == Mode.Store);
            this.ChangeItem.gameObject.SetActive(CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon);
            this.Upgrade.gameObject.SetActive(CurMode == Mode.Upgrade);
            this.BotKeyTips_KeyTips.gameObject.SetActive(CurMode == Mode.Store);
            this.BotKeyTips_ChangeItem.gameObject.SetActive(CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon);
            this.BotKeyTips_Upgrade.gameObject.SetActive(CurMode == Mode.Upgrade);
            transform.Find("TopTitle").Find("KT_Upgrade").gameObject.SetActive(HasUpgrade);

            if (this.CurMode == Mode.Store)
            {

                #region TopTitle
                Text_Title.text = PanelTextContent.text_Title.GetText();
                #endregion

                #region Store
                //for (int i = 0; i < Store.StoreDatas.Length; i++)
                //{
                //    var uiitem = Instantiate(UIItemTemplate, GridLayout.transform, false);
                //    uiStoreDatas.Add(uiitem.gameObject);
                //}
                // 遍历筛选的StoreDataList
                for (int i = 0; i < Store.StoreDatas.Length; ++i)
                {
                    var uiStoreData = uiStoreDatas[i];
                    var storeData = Store.StoreDatas[i];
                    // Active
                    uiStoreData.SetActive(true);
                    // 更新Icon
                    var img = uiStoreData.transform.Find("Icon").GetComponent<Image>();
                    if (ItemManager.Instance.IsValidItemID(storeData.ItemID))
                    {
                        if (!tempSprite.ContainsKey(storeData.ItemID))
                        {
                            var sprite = ItemManager.Instance.GetItemSprite(storeData.ItemID);
                            tempSprite[storeData.ItemID] = sprite;
                            img.sprite = sprite;
                        }
                        else
                        {
                            img.sprite = tempSprite[storeData.ItemID];
                        }
                    }
                    else
                    {
                        img.sprite = EmptySprite;
                    }
                    // Select Icon
                    uiStoreData.transform.Find("Select").gameObject.SetActive(CurStoreMode == StoreMode.ChangeItem);
                    // Name
                    var nametext = uiStoreData.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    if (storeData.ItemID != "")
                    {
                        nametext.text = ItemManager.Instance.GetItemName(storeData.ItemID);
                    }
                    else
                    {
                        nametext.text = PanelTextContent.text_Empty;
                    }
                    // Amount
                    int amount = storeData.Storage;
                    int amountMax = storeData.MaxCapacity;
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
                    float storage = level * (float)storeData.Storage / amountMax;
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
                    float storageReserve = level * (float)storeData.StorageReserve / amountMax;
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
                    float emptyReserve = level * (float)storeData.EmptyReserve / amountMax;
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
                    uiStoreData.transform.Find("Add").Find("Tick").gameObject.SetActive(storeData.CanIn);
                    uiStoreData.transform.Find("Remove").Find("Tick").gameObject.SetActive(storeData.CanOut);
                    // Selected
                    var selected = uiStoreData.transform.Find("Selected");
                    var selectIcon = uiStoreData.transform.Find("Select");
                    var selectAdd = uiStoreData.transform.Find("Add").Find("Select");
                    var selectARemove = uiStoreData.transform.Find("Remove").Find("Select");

                    if (CurrentDataIndex == i)
                    {
                        selected.gameObject.SetActive(true);
                        selectIcon.gameObject.SetActive(CurStoreMode == StoreMode.ChangeItem);
                        selectAdd.gameObject.SetActive(CurStoreMode == StoreMode.ChangeIn);
                        selectARemove.gameObject.SetActive(CurStoreMode == StoreMode.ChangeOut);
                    }
                    else
                    {
                        selected.gameObject.SetActive(false);
                        selectIcon.gameObject.SetActive(false);
                        selectAdd.gameObject.SetActive(false);
                        selectARemove.gameObject.SetActive(false);
                    }
                }
                #endregion

                #region BotKeyTips
                if (CurStoreMode == StoreMode.ChangeItem)
                {
                    this.BotKeyTips_KeyTips.Find("KT_ChangeItem").gameObject.SetActive(true);
                    this.BotKeyTips_KeyTips.Find("KT_Remove1").gameObject.SetActive(true);
                    this.BotKeyTips_KeyTips.Find("KT_Remove10").gameObject.SetActive(true);
                    this.BotKeyTips_KeyTips.Find("KT_Switch").gameObject.SetActive(false);
                    this.BotKeyTips_KeyTips.Find("KT_FastAdd").gameObject.SetActive(true);
                }
                else
                {
                    this.BotKeyTips_KeyTips.Find("KT_ChangeItem").gameObject.SetActive(false);
                    this.BotKeyTips_KeyTips.Find("KT_Remove1").gameObject.SetActive(false);
                    this.BotKeyTips_KeyTips.Find("KT_Remove10").gameObject.SetActive(false);
                    this.BotKeyTips_KeyTips.Find("KT_Switch").gameObject.SetActive(true);
                    this.BotKeyTips_KeyTips.Find("KT_FastAdd").gameObject.SetActive(false);
                }
                #endregion
            }
            else if (this.CurMode == Mode.ChangeItem || this.CurMode == Mode.ChangeIcon)
            {
                ItemDatas = new List<string>() { "" };
                ItemDatas.AddRange(ManagerNS.LocalGameManager.Instance.StoreManager.GetStoreIconItems());
                #region Item
                // 临时内存生成的UIItemData数量(只增不减，多的隐藏掉即可) - 当前筛选出来的UIItemData数量
                if (!IsInitCurItemIndex)
                {
                    for (int i = 0; i < ItemDatas.Count; ++i)
                    {
                        string itemID = ItemDatas[i];
                        string curItemID = CurMode == Mode.ChangeIcon ? Store.WorldIconItemID : Store.StoreDatas[CurrentDataIndex].ItemID;
                        curItemID = curItemID != null ? curItemID : "";
                        if (curItemID == itemID)
                        {
                            lastItemIndex = currentItemIndex;
                            currentItemIndex = i;
                            IsInitCurItemIndex = true;
                            break;
                        }
                    }
                }

                // 遍历筛选的ItemDataList
                for (int i = 0; i < ItemDatas.Count; ++i)
                {
                    var uiItemData = tempUIItemDatas[i];
                    string itemID = ItemDatas[i];
                    uiItemData.SetActive(true);
                    var img = uiItemData.transform.Find("Icon").GetComponent<Image>();
                    if (ItemManager.Instance.IsValidItemID(itemID))
                    {
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            var sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite[itemID] = sprite;
                        }
                        img.sprite = tempSprite[itemID];
                    }
                    else
                    {
                        img.sprite = EmptySprite;
                    }
                    // Selected
                    var isSelected = CurrentItemData == itemID;
                    uiItemData.transform.Find("Selected").gameObject.SetActive(isSelected);
                }
                #endregion
            }
            else if (this.CurMode == Mode.Upgrade)
            {
                #region Build
                // Icon
                string buildCID = Store.WorldStore.Classification.ToString();
                string buildID = ML.Engine.BuildingSystem.BuildingManager.Instance.GetID(buildCID);
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = ML.Engine.BuildingSystem.BuildingManager.Instance.GetBuildIcon(buildID);
                }
                Upgrade_Build.Find("Icon").GetComponent<Image>().sprite = tempSprite[buildID];
                // Name
                Upgrade_Build.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.BuildingSystem.BuildingManager.Instance.GetName(buildCID) ?? "";
                #endregion

                #region Raw
                bool flagUpgradeBtn = true;
                var raw = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(buildCID);
                //int delta = tempUIItemDatasUpgrade.Count - raw.Count;
                for (int i = 0; i < raw.Count; ++i)
                {
                    var uiItemData = tempUIItemDatasUpgrade[i];
                    string itemID = raw[i].id;
                    int need = raw[i].num;
                    int current = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).InventoryItemAmount(itemID);
                    // Active
                    uiItemData.SetActive(true);
                    // 更新Icon
                    var img = uiItemData.transform.Find("Icon").GetComponent<Image>();
                    if (ItemManager.Instance.IsValidItemID(itemID))
                    {
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            var sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite[itemID] = sprite;
                            img.sprite = sprite;
                        }
                        else
                        {
                            img.sprite = tempSprite[itemID];
                        }
                    }
                    else
                    {
                        img.sprite = EmptySprite;
                    }

                    uiItemData.transform.Find("Background3").gameObject.SetActive(current < need);

                    var nametext = uiItemData.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    var amounttext = uiItemData.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    var needtext = uiItemData.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                    uiItemData.transform.Find("Background3").gameObject.SetActive(current < need);
                    if (current < need)
                    {
                        flagUpgradeBtn = false;
                    }
                    if (itemID != "")
                    {
                        nametext.text = ItemManager.Instance.GetItemName(itemID);
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
                LayoutRebuilder.ForceRebuildLayoutImmediate(ChangeItem_GridLayout.GetComponent<RectTransform>());

                Upgrade.Find("BtnBackground1").gameObject.SetActive(flagUpgradeBtn);
                #endregion

                #region Level
                Upgrade_LvOld.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: " + Store.Level.ToString();
                Upgrade_LvOld.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc1 + Store.LevelCapacity[Store.Level];
                Upgrade_LvOld.Find("Desc1").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc2 + Store.LevelDataCapacity[Store.Level];

                if (this.Store.Level + 1 <= this.Store.LevelMax)
                {
                    Upgrade.Find("BtnBackground").gameObject.SetActive(true);
                    Upgrade.Find("KT_UpgradeConfirm").gameObject.SetActive(true);

                    Upgrade_Build.Find("Image").gameObject.SetActive(true);
                    Upgrade_LvNew.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: " + (Store.Level + 1).ToString();
                    Upgrade_LvNew.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc1 + Store.LevelCapacity[Store.Level + 1];
                    Upgrade_LvNew.Find("Desc1").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc2 + Store.LevelDataCapacity[Store.Level + 1];
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
