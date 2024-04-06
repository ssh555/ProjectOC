using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using ProjectOC.StoreNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ML.Engine.Extension;
using ML.Engine.InventorySystem.CompositeSystem;
using static ProjectOC.InventorySystem.UI.UIStore;
using System;
using ML.Engine.BuildingSystem;

//aaa
namespace ProjectOC.InventorySystem.UI
{
    public class UIStore : ML.Engine.UI.UIBasePanel<StorePanel>
    {
        #region Unity
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();

            #region TopTitle
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            StoreIcon = transform.Find("TopTitle").Find("Icon").GetComponent<Image>();
            EmptySprite = StoreIcon.sprite;

            Transform priority = transform.Find("TopTitle").Find("Priority");
            Text_Priority = priority.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            PriorityUrgency = priority.Find("Urgency");
            PriorityNormal = priority.Find("Normal");
            PriorityAlternative = priority.Find("Alternative");
            #endregion

            #region Store
            StoreTransform = transform.Find("Store");
            Transform content = StoreTransform.Find("Viewport").Find("Content");
            GridLayout = content.GetComponent<GridLayoutGroup>();
            UIItemTemplate = content.Find("UIItemTemplate");
            UIItemTemplate.gameObject.SetActive(false);
            #endregion

            #region ChangeItem
            ChangeItem = transform.Find("ChangeItem");
            Transform contentChangeItem = transform.Find("ChangeItem").Find("Select").Find("Viewport").Find("Content");
            ChangeItem_GridLayout = contentChangeItem.GetComponent<GridLayoutGroup>();
            ChangeItem_UIItemTemplate = contentChangeItem.Find("UIItemTemplate");
            ChangeItem_UIItemTemplate.gameObject.SetActive(false);
            ChangeItem.gameObject.SetActive(false);
            #endregion

            #region Upgrade
            Upgrade = transform.Find("Upgrade");
            Upgrade_Build = Upgrade.Find("Build");
            Transform contentUpgrade = transform.Find("Upgrade").Find("Raw").Find("Viewport").Find("Content");
            Upgrade_GridLayout = contentUpgrade.GetComponent<GridLayoutGroup>();
            Upgrade_UIItemTemplate = contentUpgrade.Find("UIItemTemplate");
            Upgrade_UIItemTemplate.gameObject.SetActive(false);
            Upgrade_LvOld = Upgrade.Find("Level").Find("LvOld");
            Upgrade_LvNew = Upgrade.Find("Level").Find("LvNew");
            Upgrade.gameObject.SetActive(false);
            #endregion

            #region BotKeyTips
            BotKeyTips_KeyTips = this.transform.Find("BotKeyTips").Find("KeyTips");
            BotKeyTips_ChangeItem = this.transform.Find("BotKeyTips").Find("ChangeItem");
            BotKeyTips_Upgrade = this.transform.Find("BotKeyTips").Find("Upgrade");
            BotKeyTips_ChangeItem.gameObject.SetActive(false);
            #endregion

            IsInit = true;
            Refresh();
        }
        #endregion

        #region Override
        protected override void Enter()
        {
            Store.OnStoreDataChange += Refresh;
            Store.IsInteracting = true;
            base.Enter();
        }

        protected override void Exit()
        {
            Store.OnStoreDataChange -= Refresh;
            Store.IsInteracting = false;
            ClearTemp();
            base.Exit();
        }
        #endregion

        #region Internal
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
        /// <summary>
        /// 对应的逻辑仓库
        /// </summary>
        public Store Store;
        /// <summary>
        /// 当前的Priority
        /// </summary>
        private MissionNS.TransportPriority curPriority;
        /// <summary>
        /// 封装的Priority，便于在更新值时一并更新其他数据并Refresh
        /// </summary>
        private MissionNS.TransportPriority CurPriority
        {
            get => curPriority;
            set
            {
                if (Priority != null)
                {
                    Priority.Find("Selected").gameObject.SetActive(false);
                }
                curPriority = value;
                switch (curPriority)
                {
                    case MissionNS.TransportPriority.Urgency:
                        Priority = PriorityUrgency;
                        Text_Priority.text = PanelTextContent.text_PriorityUrgency.GetText();
                        break;
                    case MissionNS.TransportPriority.Normal:
                        Priority = PriorityNormal;
                        Text_Priority.text = PanelTextContent.text_PriorityNormal.GetText();
                        break;
                    case MissionNS.TransportPriority.Alternative:
                        Priority = PriorityAlternative;
                        Text_Priority.text = PanelTextContent.text_PriorityAlternative.GetText();
                        break;
                }
                Priority.Find("Selected").gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// UIPanel.Store区域控制处理的StoreData列表
        /// </summary>
        [ShowInInspector]
        private List<StoreData> StoreDatas = new List<StoreData>();
        /// <summary>
        /// 上一次选中的DataIndex，用于移动滑动窗口
        /// </summary>
        private int lastDataIndex = 0;
        /// <summary>
        /// 当前选中的DataIndex
        /// </summary>
        private int currentDataIndex = 0;
        /// <summary>
        /// 封装，方便更新数据和Refresh
        /// </summary>
        private int CurrentDataIndex
        {
            get => currentDataIndex;
            set
            {
                int last = currentDataIndex;
                if(StoreDatas.Count > 0)
                {
                    currentDataIndex = value;
                    if(currentDataIndex == -1)
                    {
                        currentDataIndex = StoreDatas.Count - 1;
                    }
                    else if(currentDataIndex == StoreDatas.Count)
                    {
                        currentDataIndex = 0;
                    }
                    else
                    {
                        var grid = GridLayout.GetGridSize();
                        if (currentDataIndex < 0)
                        {
                            currentDataIndex += (grid.x * grid.y);
                        }
                        else if (currentDataIndex >= StoreDatas.Count)
                        {
                            currentDataIndex -= (grid.x * grid.y);
                            if (currentDataIndex < 0)
                            {
                                currentDataIndex += grid.y;
                            }
                        }
                        // 不计算隐藏的模板
                        while (this.currentDataIndex >= StoreDatas.Count)
                        {
                            this.currentDataIndex -= grid.y;
                        }
                    }
                }
                else
                {
                    currentDataIndex = 0;
                }
                if(last != currentDataIndex)
                {
                    lastDataIndex = last;
                }
                this.Refresh();
            }
        }
        /// <summary>
        /// 当前选中的StoreData
        /// </summary>
        private StoreData CurrentStoreData
        {
            get
            {
                if(CurrentDataIndex < StoreDatas.Count)
                {
                    return StoreDatas[CurrentDataIndex];
                }
                return null;
            }
        }

        [ShowInInspector]
        private List<string> ItemDatas = new List<string>();
        private int lastItemIndex = 0;
        private int currentItemIndex = 0;
        private int CurrentItemIndex
        {
            get => currentItemIndex;
            set
            {
                int last = currentItemIndex;
                if (ItemDatas.Count > 0)
                {
                    currentItemIndex = value;
                    if (currentItemIndex == -1)
                    {
                        currentItemIndex = ItemDatas.Count - 1;
                    }
                    else if (currentItemIndex == ItemDatas.Count)
                    {
                        currentItemIndex = 0;
                    }
                    else
                    {
                        var grid = ChangeItem_GridLayout.GetGridSize();
                        if (currentItemIndex < 0)
                        {
                            currentItemIndex += (grid.x * grid.y);
                        }
                        else if (currentItemIndex >= ItemDatas.Count)
                        {
                            currentItemIndex -= (grid.x * grid.y);
                            if (currentItemIndex < 0)
                            {
                                currentItemIndex += grid.y;
                            }
                        }
                        // 不计算隐藏的模板
                        while (this.currentItemIndex >= ItemDatas.Count)
                        {
                            this.currentItemIndex -= grid.y;
                        }
                    }
                }
                else
                {
                    currentItemIndex = 0;
                }
                if (last != currentItemIndex)
                {
                    lastItemIndex = last;
                }
                this.Refresh();
            }
        }
        private string CurrentItemData
        {
            get
            {
                if (CurrentItemIndex < ItemDatas.Count)
                {
                    return ItemDatas[CurrentItemIndex];
                }
                return null;
            }
        }
        public Player.PlayerCharacter Player;

        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Disable();
            // 切换Priority
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed -= NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeIcon.performed -= ChangeIcon_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Upgrade.performed -= Upgrade_performed;
            // 上下切换StoreData
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeItem.performed -= ChangeItem_performed;
            // 快捷放入
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed -= FastAdd_performed;
            // 取出1个
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove1.performed -= Remove1_performed;
            // 取出10个
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove10.performed -= Remove10_performed;
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Enable();
            // 切换Priority
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed += NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeIcon.performed += ChangeIcon_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Upgrade.performed += Upgrade_performed;
            // 上下切换StoreData
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeItem.performed += ChangeItem_performed;
            // 快捷放入
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed += FastAdd_performed;
            // 取出1个
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove1.performed += Remove1_performed;
            // 取出10个
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove10.performed += Remove10_performed;
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
        }
        private void ChangeIcon_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                CurMode = Mode.ChangeIcon;
                Refresh();
            }
        }
        private void Upgrade_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                CurMode = Mode.Upgrade;
                Refresh();
            }
        }
        private void NextPriority_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            MissionNS.TransportPriority priority = Store.TransportPriority;
            switch (priority)
            {
                case MissionNS.TransportPriority.Urgency:
                    Store.TransportPriority = MissionNS.TransportPriority.Normal;
                    break;
                case MissionNS.TransportPriority.Normal:
                    Store.TransportPriority = MissionNS.TransportPriority.Alternative;
                    break;
                case MissionNS.TransportPriority.Alternative:
                    Store.TransportPriority = MissionNS.TransportPriority.Urgency;
                    break;
            }
            CurPriority = Store.TransportPriority;
        }
        // Store
        private void ChangeItem_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                if (offset.x > 0)
                {
                    if ((int)CurStoreMode < Enum.GetValues(typeof(StoreMode)).Length - 1)
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

                if (offset.y != 0)
                {
                    var grid = GridLayout.GetGridSize();
                    this.CurrentDataIndex += -offset.y * grid.y + offset.x;
                }
            }
            else if (CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                var grid = ChangeItem_GridLayout.GetGridSize();
                this.CurrentItemIndex += -offset.y * grid.y + offset.x;
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store && CurStoreMode == StoreMode.ChangeItem)
            {
                Store.UIFastAdd(Player, CurrentStoreData);
                Refresh();
            }
        }
        private void Remove1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store && CurStoreMode == StoreMode.ChangeItem)
            {
                Store.UIRemove(Player, CurrentStoreData, 1);
                Refresh();
            }
        }
        private void Remove10_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store && CurStoreMode == StoreMode.ChangeItem)
            {
                Store.UIRemove(Player, CurrentStoreData, 10);
                Refresh();
            }
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                UIMgr.PopPanel();
            }
            else if (CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon || CurMode == Mode.Upgrade)
            {
                this.CurMode = Mode.Store;
                this.ItemDatas.Clear();
                this.lastItemIndex = 0;
                this.currentItemIndex = 0;
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
                    if (this.CurrentStoreData != null)
                    {
                        this.CurrentStoreData.CanIn = !this.CurrentStoreData.CanIn;
                    }
                }
                else if (CurStoreMode == StoreMode.ChangeOut)
                {
                    if (this.CurrentStoreData != null)
                    {
                        this.CurrentStoreData.CanOut = !this.CurrentStoreData.CanOut;
                    }
                }
            }
            else if (CurMode == Mode.ChangeItem)
            {
                Store.UIChangeStoreData(Player, CurrentDataIndex, CurrentItemData);
                this.CurMode = Mode.Store;
                this.ItemDatas.Clear();
                this.lastItemIndex = 0;
                this.currentItemIndex = 0;
            }
            else if (CurMode == Mode.ChangeIcon)
            {
                string itemID = CurrentItemData;
                // 更新Icon
                var img = StoreIcon.GetComponent<Image>();
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
                if (!string.IsNullOrEmpty(itemID))
                {
                    ItemManager.Instance.AddItemIconObject(itemID,
                                                        this.Store.WorldStore.transform,
                                                        new Vector3(0, this.Store.WorldStore.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0),
                                                        Quaternion.Euler(0, 0, 0),
                                                        Vector3.one);
                }
            }
            else if (CurMode == Mode.Upgrade)
            {
                this.Store.Upgrade(Player);
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
            foreach(var s in tempSprite)
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

        #region UI对象引用
        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;
        private Image StoreIcon;
        private Sprite EmptySprite;
        private Transform Priority;
        private Transform PriorityUrgency;
        private Transform PriorityNormal;
        private Transform PriorityAlternative;

        private Transform StoreTransform;
        private Transform UIItemTemplate;
        private GridLayoutGroup GridLayout;

        private Transform ChangeItem;
        private Transform ChangeItem_UIItemTemplate;
        private GridLayoutGroup ChangeItem_GridLayout;

        private Transform Upgrade;
        private Transform Upgrade_Build;
        private Transform Upgrade_UIItemTemplate;
        private GridLayoutGroup Upgrade_GridLayout;
        private Transform Upgrade_LvOld;
        private Transform Upgrade_LvNew;

        private Transform BotKeyTips_KeyTips;
        private Transform BotKeyTips_ChangeItem;
        private Transform BotKeyTips_Upgrade;
        #endregion

        public override void Refresh()
        {
            // 加载完成JSON数据 & 查找完所有引用
            if(ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }
            CurPriority = Store.TransportPriority;
            // StoreIcon
            Sprite itemicon = Store.WorldStore.transform.GetComponentInChildren<SpriteRenderer>()?.sprite;
            StoreIcon.sprite = itemicon == null ? EmptySprite : itemicon;

            if (this.CurMode == Mode.Store)
            {
                this.StoreTransform.gameObject.SetActive(true);
                this.ChangeItem.gameObject.SetActive(false);
                this.Upgrade.gameObject.SetActive(false);
                this.BotKeyTips_KeyTips.gameObject.SetActive(true);
                this.BotKeyTips_ChangeItem.gameObject.SetActive(false);
                this.BotKeyTips_Upgrade.gameObject.SetActive(false);

                StoreDatas = Store.StoreDatas;
                #region TopTitle
                Text_Title.text = PanelTextContent.text_Title.GetText();
                #endregion

                #region Store
                for (int i = 0; i < StoreDatas.Count; i++)
                {
                    var uiitem = Instantiate(UIItemTemplate, GridLayout.transform, false);
                    uiStoreDatas.Add(uiitem.gameObject);
                }

                // 用于更新滑动窗口
                // 当前选中的UIStoreData
                GameObject cur = null;
                // 上一个UIStoreData
                GameObject last = null;

                // 遍历筛选的StoreDataList
                for (int i = 0; i < StoreDatas.Count; ++i)
                {
                    var uiStoreData = uiStoreDatas[i];
                    var storeData = StoreDatas[i];
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
                    if (!string.IsNullOrEmpty(storeData.ItemID))
                    {
                        uiStoreData.transform.Find("Add").Find("Tick").gameObject.SetActive(storeData.CanIn);
                        uiStoreData.transform.Find("Remove").Find("Tick").gameObject.SetActive(storeData.CanOut);
                    }
                    else
                    {
                        uiStoreData.transform.Find("Add").Find("Tick").gameObject.SetActive(false);
                        uiStoreData.transform.Find("Remove").Find("Tick").gameObject.SetActive(false);
                    }
                    // Selected
                    var selected = uiStoreData.transform.Find("Selected");
                    var selectIcon = uiStoreData.transform.Find("Select");
                    var selectAdd = uiStoreData.transform.Find("Add").Find("Select");
                    var selectARemove = uiStoreData.transform.Find("Remove").Find("Select");

                    if (CurrentStoreData == StoreDatas[i])
                    {
                        selected.gameObject.SetActive(true);
                        selectIcon.gameObject.SetActive(CurStoreMode == StoreMode.ChangeItem);
                        selectAdd.gameObject.SetActive(CurStoreMode == StoreMode.ChangeIn);
                        selectARemove.gameObject.SetActive(CurStoreMode == StoreMode.ChangeOut);
                        cur = uiStoreData;
                    }
                    else
                    {
                        selected.gameObject.SetActive(false);
                        selectIcon.gameObject.SetActive(false);
                        selectAdd.gameObject.SetActive(false);
                        selectARemove.gameObject.SetActive(false);
                    }
                    if (i == lastDataIndex)
                    {
                        last = uiStoreData;
                    }
                }

                #region 更新滑动窗口
                if (cur != null && last != null)
                {
                    // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                    RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                    RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                    // 获取 ScrollRect 组件
                    ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                    // 获取 Content 的 RectTransform 组件
                    RectTransform contentRect = scrollRect.content;

                    // 获取 UI 元素的四个角点
                    Vector3[] corners = new Vector3[4];
                    uiRectTransform.GetWorldCorners(corners);
                    bool allCornersVisible = true;
                    for (int i = 0; i < 4; ++i)
                    {
                        // 将世界空间的点转换为屏幕空间的点
                        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                        // 判断 ScrollRect 是否包含这个点
                        if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                        {
                            allCornersVisible = false;
                            break;
                        }
                    }

                    // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                    if (!allCornersVisible)
                    {
                        // 将当前选中的这个放置于上一个激活TP的位置

                        // 设置滑动位置

                        // 获取点 A 和点 B 在 Content 中的位置
                        Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                        Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;

                        // 计算点 B 相对于点 A 的偏移量
                        Vector2 offset = positionB - positionA;

                        // 根据偏移量更新 ScrollRect 的滑动位置
                        Vector2 normalizedPosition = scrollRect.normalizedPosition;
                        normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                        scrollRect.normalizedPosition = normalizedPosition;
                    }
                }
                else
                {
                    GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                }
                #endregion

                // 强制立即更新 VerticalLayoutGroup 的布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(GridLayout.GetComponent<RectTransform>());
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
            else if(this.CurMode == Mode.ChangeItem || this.CurMode == Mode.ChangeIcon)
            {
                this.StoreTransform.gameObject.SetActive(false);
                this.ChangeItem.gameObject.SetActive(true);
                this.Upgrade.gameObject.SetActive(false);
                this.BotKeyTips_KeyTips.gameObject.SetActive(false);
                this.BotKeyTips_ChangeItem.gameObject.SetActive(true);
                this.BotKeyTips_Upgrade.gameObject.SetActive(false);

                ItemDatas = new List<string>() { "" };
                ItemDatas.AddRange(ItemManager.Instance.GetAllItemID());
                #region Item
                // 临时内存生成的UIItemData数量(只增不减，多的隐藏掉即可) - 当前筛选出来的UIItemData数量
                int delta = tempUIItemDatas.Count - ItemDatas.Count;
                // > 0 => 有多余，隐藏
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemDatas[tempUIItemDatas.Count - 1 - i].SetActive(false);
                    }
                }
                // < 0 => 不够， 增加
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(ChangeItem_UIItemTemplate, ChangeItem_GridLayout.transform, false);
                        tempUIItemDatas.Add(uiitem.gameObject);
                    }
                }

                // 用于更新滑动窗口
                // 当前选中的UIItemData
                GameObject cur = null;
                // 上一个UIItemData
                GameObject last = null;

                // 遍历筛选的ItemDataList
                for (int i = 0; i < ItemDatas.Count; ++i)
                {
                    var uiItemData = tempUIItemDatas[i];
                    string itemID = ItemDatas[i];
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
                    // Selected
                    var selected = uiItemData.transform.Find("Selected");
                    if (CurrentItemData == ItemDatas[i])
                    {
                        selected.gameObject.SetActive(true);
                        cur = uiItemData;
                    }
                    else
                    {
                        selected.gameObject.SetActive(false);
                    }
                    if (i == lastItemIndex)
                    {
                        last = uiItemData;
                    }
                }

                #region 更新滑动窗口
                if (cur != null && last != null)
                {
                    // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                    RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                    RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                    // 获取 ScrollRect 组件
                    ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                    // 获取 Content 的 RectTransform 组件
                    RectTransform contentRect = scrollRect.content;

                    // 获取 UI 元素的四个角点
                    Vector3[] corners = new Vector3[4];
                    uiRectTransform.GetWorldCorners(corners);
                    bool allCornersVisible = true;
                    for (int i = 0; i < 4; ++i)
                    {
                        // 将世界空间的点转换为屏幕空间的点
                        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                        // 判断 ScrollRect 是否包含这个点
                        if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                        {
                            allCornersVisible = false;
                            break;
                        }
                    }

                    // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                    if (!allCornersVisible)
                    {
                        // 将当前选中的这个放置于上一个激活TP的位置

                        // 设置滑动位置

                        // 获取点 A 和点 B 在 Content 中的位置
                        Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                        Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;

                        // 计算点 B 相对于点 A 的偏移量
                        Vector2 offset = positionB - positionA;

                        // 根据偏移量更新 ScrollRect 的滑动位置
                        Vector2 normalizedPosition = scrollRect.normalizedPosition;
                        normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                        scrollRect.normalizedPosition = normalizedPosition;
                    }
                }
                else
                {
                    ChangeItem_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                }
                #endregion

                // 强制立即更新 VerticalLayoutGroup 的布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(ChangeItem_GridLayout.GetComponent<RectTransform>());
                #endregion
            }
            else if (this.CurMode == Mode.Upgrade)
            {
                this.StoreTransform.gameObject.SetActive(false);
                this.ChangeItem.gameObject.SetActive(false);
                this.Upgrade.gameObject.SetActive(true);
                this.BotKeyTips_KeyTips.gameObject.SetActive(false);
                this.BotKeyTips_ChangeItem.gameObject.SetActive(false);
                this.BotKeyTips_Upgrade.gameObject.SetActive(true);

                #region Build
                // Icon
                string buildCID = Store.WorldStore.Classification.ToString();
                string buildID = BuildingManager.Instance.GetID(buildCID);
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = CompositeManager.Instance.GetCompositonSprite(buildID);
                }
                Upgrade_Build.Find("Icon").GetComponent<Image>().sprite = tempSprite[buildID];
                // Name
                Upgrade_Build.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = BuildingManager.Instance.GetName(buildCID) ?? "";
                #endregion

                #region Raw
                bool flagUpgradeBtn = true;
                List<Formula> raw = this.Store.GetUpgradeRaw();
                List<Formula> rawCur = this.Store.GetUpgradeRawCurrent(Player);
                int delta = tempUIItemDatasUpgrade.Count - raw.Count;
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemDatasUpgrade[tempUIItemDatasUpgrade.Count - 1 - i].SetActive(false);
                    }
                }
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(Upgrade_UIItemTemplate, Upgrade_GridLayout.transform, false);
                        tempUIItemDatasUpgrade.Add(uiitem.gameObject);
                    }
                }
                for (int i = 0; i < raw.Count; ++i)
                {
                    var uiItemData = tempUIItemDatasUpgrade[i];
                    string itemID = raw[i].id;
                    int need = raw[i].num;
                    int current = rawCur[i].num;
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
                Upgrade_LvOld.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc1 + Store.StoreCapacity;
                Upgrade_LvOld.Find("Desc1").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc2 + Store.StoreDataCapacity;

                if (this.Store.Level + 1 <= this.Store.LevelMax)
                {
                    Upgrade.Find("BtnBackground").gameObject.SetActive(true);
                    Upgrade.Find("KT_UpgradeConfirm").gameObject.SetActive(true);

                    Upgrade_Build.Find("Image").gameObject.SetActive(true);
                    Upgrade_LvNew.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: " + (Store.Level + 1).ToString();
                    Upgrade_LvNew.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc1 + Store.LevelStoreCapacity[Store.Level + 1];
                    Upgrade_LvNew.Find("Desc1").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_LvDesc2 + Store.LevelStoreDataCapacity[Store.Level + 1];
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

        #region TextContent
        [System.Serializable]
        public struct StorePanel
        {
            public TextContent text_Title;
            public TextContent text_Empty;
            public TextContent text_PriorityUrgency;
            public TextContent text_PriorityNormal;
            public TextContent text_PriorityAlternative;
            public TextContent text_Add;
            public TextContent text_Remove;
            public TextContent text_LvDesc1;
            public TextContent text_LvDesc2;

            public KeyTip NextPriority;
            public KeyTip ChangeIcon;
            public KeyTip ChangeItem;
            public KeyTip Remove1;
            public KeyTip Remove10;
            public KeyTip FastAdd;
            public KeyTip Confirm;
            public KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/Store";
            this.abname = "StorePanel";
            this.description = "StorePanel数据加载完成";
        }
        #endregion
    }
}
