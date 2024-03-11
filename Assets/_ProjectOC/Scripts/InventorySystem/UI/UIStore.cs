using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using ProjectOC.StoreNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ML.Engine.Extension;
using ML.Engine.InventorySystem.CompositeSystem;
using UnityEngine.Purchasing;
using ProjectOC.ProNodeNS;
using ML.Engine.UI;
using ML.Engine.Manager;
using ML.Engine.Input;
using UnityEngine.InputSystem;

namespace ProjectOC.InventorySystem.UI
{
    public class UIStore : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;
        private void Awake()
        {
            InitUITextContents();

            //KeyTips
            UIKeyTipComponents = this.transform.GetComponentsInChildren<UIKeyTipComponent>(true);
            foreach (var item in UIKeyTipComponents)
            {
                item.InitData();
                uiKeyTipDic.Add(item.InputActionName, item);
            }

            #region TopTitle
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            StoreIcon = transform.Find("TopTitle").Find("Icon");

            #region Priority
            Transform priority = transform.Find("TopTitle").Find("Priority");
            Text_Priority = priority.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            PriorityUrgency = priority.Find("Urgency");
            PriorityNormal = priority.Find("Normal");
            PriorityAlternative = priority.Find("Alternative");
            #endregion
            #endregion

            #region Store
            Transform content = transform.Find("Store").Find("Viewport").Find("Content");
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
            Transform contentUpgrade = transform.Find("Upgrade").Find("Raw").Find("Viewport").Find("Content");
            Upgrade_GridLayout = contentUpgrade.GetComponent<GridLayoutGroup>();
            Upgrade_UIItemTemplate = contentUpgrade.Find("UIItemTemplate");
            Upgrade_UIItemTemplate.gameObject.SetActive(false);
            Transform level = transform.Find("Upgrade").Find("Level");
            LvOld = level.Find("LvOld").GetComponent<TMPro.TextMeshProUGUI>();
            LvNew = level.Find("LvNew").GetComponent<TMPro.TextMeshProUGUI>();
            DescOld = level.Find("DescOld").GetComponent<TMPro.TextMeshProUGUI>();
            DescNew = level.Find("DescNew").GetComponent<TMPro.TextMeshProUGUI>();
            Upgrade.gameObject.SetActive(false);
            #endregion

            #region BotKeyTips
            BotKeyTips_KeyTips = this.transform.Find("BotKeyTips").Find("KeyTips");
            
            BotKeyTips_ChangeItem = this.transform.Find("BotKeyTips").Find("ChangeItem");
            
            BotKeyTips_ChangeItem.gameObject.SetActive(false);
            #endregion

            CurPriority = MissionNS.TransportPriority.Normal;
            IsInit = true;
            Refresh();
        }
        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Enter();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.Exit();
            ClearTemp();
        }

        public override void OnPause()
        {
            base.OnPause();
            this.Exit();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.Enter();
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

        private void Enter()
        {
            Store.OnStoreDataChange += Refresh;
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Enable();
            Store.IsInteracting = true;
            UikeyTipIsInit = false;
            this.Refresh();
        }

        private void Exit()
        {
            Store.OnStoreDataChange -= Refresh;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Disable();
            Store.IsInteracting = false;
            this.UnregisterInput();
        }

        private void UnregisterInput()
        {
            // 切换Priority
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed -= NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeIcon.performed -= ChangeIcon_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Upgrade.performed -= Upgrade_performed;
            // 上下切换StoreData
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeItem.performed -= ChangeItem_performed;
            // 改变StoreData存储的Item
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeStoreData.performed -= ChangeStoreData_performed;
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

        private void RegisterInput()
        {
            // 切换Priority
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed += NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeIcon.performed += ChangeIcon_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Upgrade.performed += Upgrade_performed;
            // 上下切换StoreData
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeItem.performed += ChangeItem_performed;
            // 改变StoreData存储的Item
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeStoreData.performed += ChangeStoreData_performed;
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
            MissionNS.TransportPriority temp = CurPriority;
            switch (temp)
            {
                case MissionNS.TransportPriority.Urgency:
                    CurPriority = MissionNS.TransportPriority.Normal;
                    break;
                case MissionNS.TransportPriority.Normal:
                    CurPriority = MissionNS.TransportPriority.Alternative;
                    break;
                case MissionNS.TransportPriority.Alternative:
                    CurPriority = MissionNS.TransportPriority.Urgency;
                    break;
            }
        }

        // Store
        private void ChangeItem_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                var grid = GridLayout.GetGridSize();
                this.CurrentDataIndex += -offset.y * grid.y + offset.x;
            }
            else if (CurMode == Mode.ChangeItem || CurMode == Mode.ChangeIcon)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                var grid = ChangeItem_GridLayout.GetGridSize();
                this.CurrentItemIndex += -offset.y * grid.y + offset.x;
            }
        }
        private void ChangeStoreData_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                CurMode = Mode.ChangeItem;
                Refresh();
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                Store.UIFastAdd(Player, CurrentStoreData);
                Refresh();
            }
        }
        private void Remove1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                Store.UIRemove(Player, CurrentStoreData, 1);
                Refresh();
            }
        }
        private void Remove10_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
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
            if (CurMode == Mode.ChangeItem)
            {
                Store.ChangeStoreData(CurrentDataIndex, CurrentItemData);
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
                    var texture = ItemManager.Instance.GetItemTexture2D(itemID);
                    if (texture != null)
                    {
                        // 查找临时存储的Sprite
                        var sprite = tempSprite.Find(s => s.texture == texture);
                        // 不存在则生成
                        if (sprite == null)
                        {
                            sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite.Add(sprite);
                        }
                        img.sprite = sprite;

                        ItemManager.Instance.AddItemIconObject(itemID,
                                                               this.Store.WorldStore.transform,
                                                               new Vector3(0, this.Store.WorldStore.transform.GetComponent<Collider>().bounds.size.y / 2, 0),
                                                               Quaternion.Euler(90, 0, 0),
                                                               Vector3.one);
                    }
                }
                else
                {
                    img.sprite = null;
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
        private List<Sprite> tempSprite = new List<Sprite>();
        private List<GameObject> uiStoreDatas = new List<GameObject>();
        private List<GameObject> tempUIItemDatas = new List<GameObject>();
        private List<GameObject> tempUIItemDatasUpgrade = new List<GameObject>();

        private Dictionary<string, UIKeyTipComponent> uiKeyTipDic = new Dictionary<string, UIKeyTipComponent>();
        private bool UikeyTipIsInit;
        private InputManager inputManager => GameManager.Instance.InputManager;

        private void ClearTemp()
        {
            foreach(var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
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
            uiKeyTipDic = null;
        }
        #endregion

        #region UI对象引用
        private UIKeyTipComponent[] UIKeyTipComponents;


        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;

        private Transform UIItemTemplate;
        private GridLayoutGroup GridLayout;
        private Transform Priority;
        private Transform PriorityUrgency;
        private Transform PriorityNormal;
        private Transform PriorityAlternative;

        private Transform StoreIcon;

        private Transform ChangeItem;
        private Transform ChangeItem_UIItemTemplate;
        private GridLayoutGroup ChangeItem_GridLayout;

        private Transform Upgrade;
        private Transform Upgrade_UIItemTemplate;
        private GridLayoutGroup Upgrade_GridLayout;
        private TMPro.TextMeshProUGUI LvOld;
        private TMPro.TextMeshProUGUI LvNew;
        private TMPro.TextMeshProUGUI DescOld;
        private TMPro.TextMeshProUGUI DescNew;

        private Transform BotKeyTips_KeyTips;
        private Transform BotKeyTips_ChangeItem;
        #endregion

        public override void Refresh()
        {
            // 加载完成JSON数据 & 查找完所有引用
            if(ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
            }

            if (UikeyTipIsInit == false)
            {
                KeyTip[] keyTips = inputManager.ExportKeyTipValues(PanelTextContent);
                foreach (var keyTip in keyTips)
                {
                    InputAction inputAction = inputManager.GetInputAction((keyTip.keymap.ActionMapName, keyTip.keymap.ActionName));
                    inputManager.GetInputActionBindText(inputAction);
                    if (uiKeyTipDic.ContainsKey(keyTip.keyname))
                    {
                        UIKeyTipComponent uIKeyTipComponent = uiKeyTipDic[keyTip.keyname];
                        if (uIKeyTipComponent.uiKeyTip.keytip != null)
                        {
                            uIKeyTipComponent.uiKeyTip.keytip.text = inputManager.GetInputActionBindText(inputAction);
                        }
                        if (uIKeyTipComponent.uiKeyTip.description != null)
                        {
                            uIKeyTipComponent.uiKeyTip.description.text = keyTip.description.GetText();
                        }
                    }
                    else
                    {
                        //Debug.Log("keyTip.keyname " + keyTip.keyname);
                    }
                }
                UikeyTipIsInit = true;
            }


            if (this.CurMode == Mode.Store)
            {
                this.ChangeItem.gameObject.SetActive(false);
                this.BotKeyTips_ChangeItem.gameObject.SetActive(false);
                this.BotKeyTips_KeyTips.gameObject.SetActive(true);
                this.UIItemTemplate.gameObject.SetActive(false);
                this.ChangeItem_UIItemTemplate.gameObject.SetActive(false);
                this.Upgrade.gameObject.SetActive(false);
                this.Upgrade_UIItemTemplate.gameObject.SetActive(false);

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
                        // 查找临时存储的Sprite
                        var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(storeData.ItemID));
                        // 不存在则生成
                        if (sprite == null)
                        {
                            sprite = ItemManager.Instance.GetItemSprite(storeData.ItemID);
                            tempSprite.Add(sprite);
                        }
                        img.sprite = sprite;
                    }
                    else
                    {
                        img.sprite = null;
                    }
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
                    int amount = storeData.StorageAll;
                    int amountMax = storeData.MaxCapacity;
                    var AmountCur = uiStoreData.transform.Find("Amount").Find("Cur").GetComponent<TMPro.TextMeshProUGUI>();
                    var AmountMax = uiStoreData.transform.Find("Amount").Find("Max").GetComponent<TMPro.TextMeshProUGUI>();
                    AmountCur.text = amount.ToString();
                    AmountMax.text = amountMax.ToString();
                    // ProgressBar
                    int level = Store.Level;
                    int amountEachLevel = amountMax / (level + 1);
                    Transform progressBar1 = uiStoreData.transform.Find("ProgressBar1");
                    RectTransform progressBarRectCur1 = progressBar1.Find("Cur").GetComponent<RectTransform>();
                    RectTransform progressBarRectMax1 = progressBar1.Find("Max").GetComponent<RectTransform>();
                    Transform progressBar2 = uiStoreData.transform.Find("ProgressBar2");
                    RectTransform progressBarRectCur2 = progressBar2.Find("Cur").GetComponent<RectTransform>();
                    RectTransform progressBarRectMax2 = progressBar2.Find("Max").GetComponent<RectTransform>();
                    Transform progressBar3 = uiStoreData.transform.Find("ProgressBar3");
                    RectTransform progressBarRectCur3 = progressBar3.Find("Cur").GetComponent<RectTransform>();
                    RectTransform progressBarRectMax3 = progressBar3.Find("Max").GetComponent<RectTransform>();
                    if (level == 0)
                    {
                        progressBar1.Find("None").gameObject.SetActive(false);
                        progressBar2.Find("None").gameObject.SetActive(true);
                        progressBar3.Find("None").gameObject.SetActive(true);
                    }
                    else if (level == 1)
                    {
                        progressBar1.Find("None").gameObject.SetActive(false);
                        progressBar2.Find("None").gameObject.SetActive(false);
                        progressBar3.Find("None").gameObject.SetActive(true);
                    }
                    else if (level == 2)
                    {
                        progressBar1.Find("None").gameObject.SetActive(false);
                        progressBar2.Find("None").gameObject.SetActive(false);
                        progressBar3.Find("None").gameObject.SetActive(false);
                    }
                    float width1 = amount >= amountEachLevel ? 1 : (float)amount / amountEachLevel;
                    float width2 = amount >= 2 * amountEachLevel ? 1 : (float)amount / amountEachLevel - 1;
                    width2 = width2 >= 0 ? width2 : 0;
                    float width3 = amount >= 3 * amountEachLevel ? 1 : (float)amount / amountMax - 2;
                    width3 = width3 >= 0 ? width3 : 0;
                    progressBarRectCur1.sizeDelta = new Vector2(400 * width1, progressBarRectCur1.sizeDelta.y);
                    progressBarRectCur2.sizeDelta = new Vector2(400 * width2, progressBarRectCur2.sizeDelta.y);
                    progressBarRectCur3.sizeDelta = new Vector2(400 * width3, progressBarRectCur3.sizeDelta.y);

                    // Add and Remove
                    uiStoreData.transform.Find("Add").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_Add.GetText();
                    uiStoreData.transform.Find("Remove").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_Remove.GetText();
                    // Selected
                    var selected = uiStoreData.transform.Find("Selected");
                    if (CurrentStoreData == StoreDatas[i])
                    {
                        selected.gameObject.SetActive(true);
                        cur = uiStoreData;
                    }
                    else
                    {
                        selected.gameObject.SetActive(false);
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

                #endregion
            }
            else if(this.CurMode == Mode.ChangeItem || this.CurMode == Mode.ChangeIcon)
            {
                this.ChangeItem.gameObject.SetActive(true);
                this.BotKeyTips_ChangeItem.gameObject.SetActive(true);
                this.BotKeyTips_KeyTips.gameObject.SetActive(false);
                this.UIItemTemplate.gameObject.SetActive(false);
                this.ChangeItem_UIItemTemplate.gameObject.SetActive(false);
                this.Upgrade.gameObject.SetActive(false);
                this.Upgrade_UIItemTemplate.gameObject.SetActive(false);

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
                        // 查找临时存储的Sprite
                        var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(itemID));
                        // 不存在则生成
                        if (sprite == null)
                        {
                            sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite.Add(sprite);
                        }
                        img.sprite = sprite;
                    }
                    else
                    {
                        img.sprite = null;
                    }
                    // Name
                    var nametext = uiItemData.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    if (itemID != "")
                    {
                        nametext.text = ItemManager.Instance.GetItemName(itemID);
                    }
                    else
                    {
                        nametext.text = PanelTextContent.text_Empty;
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

                #region BotKeyTips

                #endregion
            }
            else if (this.CurMode == Mode.Upgrade)
            {
                this.ChangeItem.gameObject.SetActive(false);
                this.BotKeyTips_ChangeItem.gameObject.SetActive(true);
                this.BotKeyTips_KeyTips.gameObject.SetActive(false);
                this.UIItemTemplate.gameObject.SetActive(false);
                this.ChangeItem_UIItemTemplate.gameObject.SetActive(false);
                this.Upgrade.gameObject.SetActive(true);
                this.Upgrade_UIItemTemplate.gameObject.SetActive(false);

                List<Formula> raw = this.Store.GetUpgradeRaw();
                List<Formula> rawCur = this.Store.GetUpgradeRawCurrent(Player);
                #region Item
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
                        var texture = ItemManager.Instance.GetItemTexture2D(itemID);
                        if (texture != null)
                        {
                            // 查找临时存储的Sprite
                            var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(itemID));
                            // 不存在则生成
                            if (sprite == null)
                            {
                                sprite = ItemManager.Instance.GetItemSprite(itemID);
                                tempSprite.Add(sprite);
                            }
                            img.sprite = sprite;
                        }
                    }
                    else
                    {
                        img.sprite = null;
                    }
                    var nametext = uiItemData.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    var amounttext = uiItemData.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    var needtext = uiItemData.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
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
                #endregion

                #region Level
                LvOld.text = "Lv: " + this.Store.Level.ToString();
                DescOld.text = PanelTextContent.text_LvDesc1 + this.Store.StoreCapacity + "    " + PanelTextContent.text_LvDesc2 + this.Store.StoreDataCapacity;
                if (this.Store.Level + 1 <= this.Store.LevelMax)
                {
                    LvNew.text = "Lv: " + (Store.Level + 1).ToString();
                    DescNew.text = PanelTextContent.text_LvDesc1 + Store.LevelStoreCapacity[Store.Level + 1] + "    " + PanelTextContent.text_LvDesc2 + Store.LevelStoreDataCapacity[Store.Level + 1];
                }
                else
                {
                    LvNew.text = "";
                    DescNew.text = "";
                }
                #endregion

                #region BotKeyTips

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

        public StorePanel PanelTextContent => ABJAProcessor.Datas;
        public ML.Engine.ABResources.ABJsonAssetProcessor<StorePanel> ABJAProcessor;

        private void InitUITextContents()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<StorePanel>("OC/Json/TextContent/Store", "StorePanel", (datas) =>
            {
                Refresh();
                this.enabled = false;
            }, "UI仓库Panel数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}
