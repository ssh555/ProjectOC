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

namespace ProjectOC.InventorySystem.UI
{
    public class UIStore : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();

            #region TopTitle
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ChangeIcon = new UIKeyTip();
            KT_ChangeIcon.img = transform.Find("TopTitle").Find("KT_ChangeIcon").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_ChangeIcon.keytip = KT_ChangeIcon.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            StoreIcon = transform.Find("TopTitle").Find("Icon");

            #region Priority
            Transform priority = transform.Find("TopTitle").Find("Priority");
            Text_Priority = priority.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            KT_NextPriority = new UIKeyTip();
            KT_NextPriority.img = priority.Find("KT_NextPriority").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_NextPriority.keytip = KT_NextPriority.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
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
            
            KT_ChangeItem = new UIKeyTip();
            KT_ChangeItem.img = BotKeyTips_KeyTips.Find("KT_ChangeItem").Find("Image").GetComponent<Image>();
            KT_ChangeItem.keytip = KT_ChangeItem.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ChangeItem.description = KT_ChangeItem.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Remove1 = new UIKeyTip();
            KT_Remove1.img = BotKeyTips_KeyTips.Find("KT_Remove1").Find("Image").GetComponent<Image>();
            KT_Remove1.keytip = KT_Remove1.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Remove1.description = KT_Remove1.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Remove10 = new UIKeyTip();
            KT_Remove10.img = BotKeyTips_KeyTips.Find("KT_Remove10").Find("Image").GetComponent<Image>();
            KT_Remove10.keytip = KT_Remove10.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Remove10.description = KT_Remove10.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_FastAdd = new UIKeyTip();
            KT_FastAdd.img = BotKeyTips_KeyTips.Find("KT_FastAdd").Find("Image").GetComponent<Image>();
            KT_FastAdd.keytip = KT_FastAdd.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_FastAdd.description = KT_FastAdd.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            BotKeyTips_ChangeItem = this.transform.Find("BotKeyTips").Find("ChangeItem");
            KT_ChangeItem_Confirm = new UIKeyTip();
            KT_ChangeItem_Confirm.img = BotKeyTips_ChangeItem.Find("KT_Confirm").Find("Image").GetComponent<Image>();
            KT_ChangeItem_Confirm.keytip = KT_ChangeItem_Confirm.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ChangeItem_Confirm.description = KT_ChangeItem_Confirm.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_ChangeItem_Back = new UIKeyTip();
            KT_ChangeItem_Back.img = BotKeyTips_ChangeItem.Find("KT_Back").Find("Image").GetComponent<Image>();
            KT_ChangeItem_Back.keytip = KT_ChangeItem_Back.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ChangeItem_Back.description = KT_ChangeItem_Back.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();
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
        private Mode CurMode = Mode.Store;
        /// <summary>
        /// ��Ӧ���߼��ֿ�
        /// </summary>
        public Store Store;
        /// <summary>
        /// ��ǰ��Priority
        /// </summary>
        private MissionNS.TransportPriority curPriority;
        /// <summary>
        /// ��װ��Priority�������ڸ���ֵʱһ�������������ݲ�Refresh
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
        /// UIPanel.Store������ƴ����StoreData�б�
        /// </summary>
        [ShowInInspector]
        private List<StoreData> StoreDatas = new List<StoreData>();
        /// <summary>
        /// ��һ��ѡ�е�DataIndex�������ƶ���������
        /// </summary>
        private int lastDataIndex = 0;
        /// <summary>
        /// ��ǰѡ�е�DataIndex
        /// </summary>
        private int currentDataIndex = 0;
        /// <summary>
        /// ��װ������������ݺ�Refresh
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
                        // ���������ص�ģ��
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
        /// ��ǰѡ�е�StoreData
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
                        // ���������ص�ģ��
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
        private Player.PlayerCharacter Player => GameObject.Find("PlayerCharacter")?.GetComponent<Player.PlayerCharacter>();

        private void Enter()
        {
            Store.OnStoreDataChange += Refresh;
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Enable();
            Store.IsInteracting = true;
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
            // �л�Priority
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed -= NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeIcon.performed -= ChangeIcon_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Upgrade.performed -= Upgrade_performed;
            // �����л�StoreData
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Alter.performed -= Alter_performed;
            // �ı�StoreData�洢��Item
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeStoreData.performed -= ChangeStoreData_performed;
            // ��ݷ���
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed -= FastAdd_performed;
            // ȡ��1��
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove1.performed -= Remove1_performed;
            // ȡ��10��
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove10.performed -= Remove10_performed;
            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed -= Confirm_performed;
        }

        private void RegisterInput()
        {
            // �л�Priority
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed += NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeIcon.performed += ChangeIcon_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Upgrade.performed += Upgrade_performed;
            // �����л�StoreData
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Alter.performed += Alter_performed;
            // �ı�StoreData�洢��Item
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeStoreData.performed += ChangeStoreData_performed;
            // ��ݷ���
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed += FastAdd_performed;
            // ȡ��1��
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove1.performed += Remove1_performed;
            // ȡ��10��
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove10.performed += Remove10_performed;
            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed += Confirm_performed;
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
        private void Alter_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
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
                Refresh();
            }
            else if (CurMode == Mode.ChangeIcon)
            {
                string itemID = CurrentItemData;
                // ����Icon
                if (ItemManager.Instance.IsValidItemID(itemID))
                {
                    var img = StoreIcon.GetComponent<Image>();
                    var texture = ItemManager.Instance.GetItemTexture2D(itemID);
                    if (texture != null)
                    {
                        // ������ʱ�洢��Sprite
                        var sprite = tempSprite.Find(s => s.texture == texture);
                        // ������������
                        if (sprite == null)
                        {
                            sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite.Add(sprite);
                        }
                        img.sprite = sprite;
                    }
                }
                
                this.CurMode = Mode.Store;
                Refresh();
            }
            else if (CurMode == Mode.Upgrade)
            {
                this.Store.Upgrade(Player);
                Refresh();
            }
        }
        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private List<GameObject> uiStoreDatas = new List<GameObject>();
        private List<GameObject> tempUIItemDatas = new List<GameObject>();
        private List<GameObject> tempUIItemDatasUpgrade = new List<GameObject>();

        private void ClearTemp()
        {
            foreach(var s in tempSprite)
            {
                Destroy(s);
            }
            foreach (var s in uiStoreDatas)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItemDatas)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItemDatasUpgrade)
            {
                Destroy(s);
            }
        }
        #endregion

        #region UI��������
        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;

        private UIKeyTip KT_NextPriority;
        private UIKeyTip KT_ChangeIcon;
        private UIKeyTip KT_ChangeItem;
        private UIKeyTip KT_Remove1;
        private UIKeyTip KT_Remove10;
        private UIKeyTip KT_FastAdd;
        private UIKeyTip KT_ChangeItem_Confirm;
        private UIKeyTip KT_ChangeItem_Back;

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

        public void Refresh()
        {
            // �������JSON���� & ��������������
            if(ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
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
                KT_ChangeIcon.ReWrite(PanelTextContent.kt_ChangeIcon);
                KT_NextPriority.ReWrite(PanelTextContent.kt_NextPriority);
                #endregion

                #region Store
                for (int i = 0; i < StoreDatas.Count; i++)
                {
                    var uiitem = Instantiate(UIItemTemplate, GridLayout.transform, false);
                    uiStoreDatas.Add(uiitem.gameObject);
                }

                // ���ڸ��»�������
                // ��ǰѡ�е�UIStoreData
                GameObject cur = null;
                // ��һ��UIStoreData
                GameObject last = null;

                // ����ɸѡ��StoreDataList
                for (int i = 0; i < StoreDatas.Count; ++i)
                {
                    var uiStoreData = uiStoreDatas[i];
                    var storeData = StoreDatas[i];
                    // Active
                    uiStoreData.SetActive(true);
                    // ����Icon
                    if (ItemManager.Instance.IsValidItemID(storeData.ItemID))
                    {
                        var img = uiStoreData.transform.Find("Icon").GetComponent<Image>();
                        // ������ʱ�洢��Sprite
                        var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(storeData.ItemID));
                        // ������������
                        if (sprite == null)
                        {
                            sprite = ItemManager.Instance.GetItemSprite(storeData.ItemID);
                            tempSprite.Add(sprite);
                        }
                        img.sprite = sprite;
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

                #region ���»�������
                if (cur != null && last != null)
                {
                    // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
                    RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                    RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                    // ��ȡ ScrollRect ���
                    ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                    // ��ȡ Content �� RectTransform ���
                    RectTransform contentRect = scrollRect.content;

                    // ��ȡ UI Ԫ�ص��ĸ��ǵ�
                    Vector3[] corners = new Vector3[4];
                    uiRectTransform.GetWorldCorners(corners);
                    bool allCornersVisible = true;
                    for (int i = 0; i < 4; ++i)
                    {
                        // ������ռ�ĵ�ת��Ϊ��Ļ�ռ�ĵ�
                        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                        // �ж� ScrollRect �Ƿ���������
                        if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                        {
                            allCornersVisible = false;
                            break;
                        }
                    }

                    // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
                    if (!allCornersVisible)
                    {
                        // ����ǰѡ�е������������һ������TP��λ��

                        // ���û���λ��

                        // ��ȡ�� A �͵� B �� Content �е�λ��
                        Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                        Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;

                        // ����� B ����ڵ� A ��ƫ����
                        Vector2 offset = positionB - positionA;

                        // ����ƫ�������� ScrollRect �Ļ���λ��
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

                // ǿ���������� VerticalLayoutGroup �Ĳ���
                LayoutRebuilder.ForceRebuildLayoutImmediate(GridLayout.GetComponent<RectTransform>());
                #endregion

                #region BotKeyTips
                KT_ChangeItem.ReWrite(PanelTextContent.kt_ChangeItem);
                KT_Remove1.ReWrite(PanelTextContent.kt_Remove1);
                KT_Remove10.ReWrite(PanelTextContent.kt_Remove10);
                KT_FastAdd.ReWrite(PanelTextContent.kt_FastAdd);
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
                // ��ʱ�ڴ����ɵ�UIItemData����(ֻ��������������ص�����) - ��ǰɸѡ������UIItemData����
                int delta = tempUIItemDatas.Count - ItemDatas.Count;
                // > 0 => �ж��࣬����
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemDatas[tempUIItemDatas.Count - 1 - i].SetActive(false);
                    }
                }
                // < 0 => ������ ����
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(ChangeItem_UIItemTemplate, ChangeItem_GridLayout.transform, false);
                        tempUIItemDatas.Add(uiitem.gameObject);
                    }
                }

                // ���ڸ��»�������
                // ��ǰѡ�е�UIItemData
                GameObject cur = null;
                // ��һ��UIItemData
                GameObject last = null;

                // ����ɸѡ��ItemDataList
                for (int i = 0; i < ItemDatas.Count; ++i)
                {
                    var uiItemData = tempUIItemDatas[i];
                    string itemID = ItemDatas[i];
                    // Active
                    uiItemData.SetActive(true);
                    // ����Icon
                    if (ItemManager.Instance.IsValidItemID(itemID))
                    {
                        var img = uiItemData.transform.Find("Icon").GetComponent<Image>();
                        // ������ʱ�洢��Sprite
                        var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(itemID));
                        // ������������
                        if (sprite == null)
                        {
                            sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite.Add(sprite);
                        }
                        img.sprite = sprite;
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

                #region ���»�������
                if (cur != null && last != null)
                {
                    // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
                    RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                    RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                    // ��ȡ ScrollRect ���
                    ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                    // ��ȡ Content �� RectTransform ���
                    RectTransform contentRect = scrollRect.content;

                    // ��ȡ UI Ԫ�ص��ĸ��ǵ�
                    Vector3[] corners = new Vector3[4];
                    uiRectTransform.GetWorldCorners(corners);
                    bool allCornersVisible = true;
                    for (int i = 0; i < 4; ++i)
                    {
                        // ������ռ�ĵ�ת��Ϊ��Ļ�ռ�ĵ�
                        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                        // �ж� ScrollRect �Ƿ���������
                        if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                        {
                            allCornersVisible = false;
                            break;
                        }
                    }

                    // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
                    if (!allCornersVisible)
                    {
                        // ����ǰѡ�е������������һ������TP��λ��

                        // ���û���λ��

                        // ��ȡ�� A �͵� B �� Content �е�λ��
                        Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                        Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;

                        // ����� B ����ڵ� A ��ƫ����
                        Vector2 offset = positionB - positionA;

                        // ����ƫ�������� ScrollRect �Ļ���λ��
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

                // ǿ���������� VerticalLayoutGroup �Ĳ���
                LayoutRebuilder.ForceRebuildLayoutImmediate(ChangeItem_GridLayout.GetComponent<RectTransform>());
                #endregion

                #region BotKeyTips
                KT_ChangeItem_Confirm.ReWrite(PanelTextContent.kt_Confirm);
                KT_ChangeItem_Back.ReWrite(PanelTextContent.kt_Back);
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
                    // ����Icon
                    if (ItemManager.Instance.IsValidItemID(itemID))
                    {
                        var img = uiItemData.transform.Find("Icon").GetComponent<Image>();
                        var texture = ItemManager.Instance.GetItemTexture2D(itemID);
                        if (texture != null)
                        {
                            // ������ʱ�洢��Sprite
                            var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(itemID));
                            // ������������
                            if (sprite == null)
                            {
                                sprite = ItemManager.Instance.GetItemSprite(itemID);
                                tempSprite.Add(sprite);
                            }
                            img.sprite = sprite;
                        }
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
                LvOld.text = "Lv: " + (this.Store.Level + 1).ToString();
                DescOld.text = PanelTextContent.text_LvDesc1 + this.Store.StoreCapacity + "    " + PanelTextContent.text_LvDesc2 + this.Store.StoreDataCapacity;
                if (this.Store.Level + 1 <= this.Store.LevelMax)
                {
                    LvNew.text = "Lv: " + (Store.Level + 2).ToString();
                    DescNew.text = PanelTextContent.text_LvDesc1 + Store.LevelStoreCapacity[Store.Level + 1] + "    " + PanelTextContent.text_LvDesc2 + Store.LevelStoreDataCapacity[Store.Level + 1];
                }
                #endregion

                #region BotKeyTips
                KT_ChangeItem_Confirm.ReWrite(PanelTextContent.kt_Confirm);
                KT_ChangeItem_Back.ReWrite(PanelTextContent.kt_Back);
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

            public KeyTip kt_NextPriority;
            public KeyTip kt_ChangeIcon;
            public KeyTip kt_ChangeItem;
            public KeyTip kt_Remove1;
            public KeyTip kt_Remove10;
            public KeyTip kt_FastAdd;
            public KeyTip kt_Confirm;
            public KeyTip kt_Back;
        }

        public static StorePanel PanelTextContent => ABJAProcessor.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<StorePanel> ABJAProcessor;

        private void InitUITextContents()
        {
            if(ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<StorePanel>("Json/TextContent/Inventory", "StorePanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI�ֿ�Panel����");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion
    }
}
