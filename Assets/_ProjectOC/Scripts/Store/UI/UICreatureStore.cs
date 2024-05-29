using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.StoreNS.UI.UICreatureStore;

namespace ProjectOC.StoreNS.UI
{
    public class UICreatureStore : ML.Engine.UI.UIBasePanel<StorePanel>
    {
        #region Data
        #region Mode
        public enum Mode
        {
            Store,
            Creature,
            ChangeItem
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
                if (curMode == Mode.Creature)
                {
                    DataBtnList.EnableBtnList();
                }
                else if (curMode == Mode.ChangeItem)
                {
                    ItemBtnList.EnableBtnList();
                    string curItemID = Store.CreatureItemID;
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
        #endregion

        public CreatureStore Store;
        public List<string> ItemDatas = new List<string>();
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();

        #region BtnList
        private ML.Engine.UI.UIBtnList DataBtnList;
        private int DataIndex => DataBtnList?.GetCurSelectedPos1() ?? 0;
        private ML.Engine.UI.UIBtnList ItemBtnList;
        private int ItemIndex => ItemBtnList?.GetCurSelectedPos1() ?? 0;
        private bool IsInitBtnList;
        protected override void InitBtnInfo()
        {
            ML.Engine.Utility.Synchronizer synchronizer = new ML.Engine.Utility.Synchronizer(2, () => { IsInitBtnList = true; Refresh(); });
            DataBtnList = new ML.Engine.UI.UIBtnList(transform.Find("Store").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            DataBtnList.OnSelectButtonChanged += () => { Refresh(); };
            DataBtnList.ChangBtnNum(Store.DataContainer.GetCapacity(), "Prefab_Store_UI/Prefab_Store_UI_CreatureTemplate.prefab", () => { synchronizer.Check(); });

            ItemDatas = new List<string>() { "" };
            ItemDatas.AddRange(ManagerNS.LocalGameManager.Instance.StoreManager.GetCreatureStoreIconItems());
            ItemBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeCreature").Find("Select").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            ItemBtnList.OnSelectButtonChanged += () => { Refresh(); };
            ItemBtnList.ChangBtnNum(ItemDatas.Count, "Prefab_Store_UI/Prefab_Store_UI_ItemTemplate.prefab", () => { synchronizer.Check(); });
        }
        protected void UpdateBtnInfo()
        {
            IsInitBtnList = false;
            ML.Engine.Utility.Synchronizer synchronizer = new ML.Engine.Utility.Synchronizer(1, () => { IsInitBtnList = true; Refresh(); });
            if (DataBtnList.BtnCnt != Store.DataContainer.GetCapacity())
            {
                DataBtnList.ChangBtnNum(Store.DataContainer.GetCapacity(), "Prefab_Store_UI/Prefab_Store_UI_CreatureTemplate.prefab", () => { synchronizer.Check(); });
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
        private Transform Store_UI;
        private Transform ChangeCreature_UI;
        private Transform Priority;
        private Transform KeyTips;
        private Transform KeyTips1;
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            InitTextContentPathData();
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Text_Priority = transform.Find("TopTitle").Find("Priority").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Store_UI = transform.Find("Store");
            ChangeCreature_UI = transform.Find("ChangeCreature");
            KeyTips = transform.Find("BotKeyTips").Find("KeyTips");
            KeyTips1 = transform.Find("BotKeyTips").Find("KeyTips1");
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
            public ML.Engine.TextContent.TextContent text_WaitPutIn;
            public ML.Engine.TextContent.TextContent text_GetBreed;
            public ML.Engine.TextContent.TextContent text_GetDiscard;

            public ML.Engine.TextContent.KeyTip NextPriority;
            public ML.Engine.TextContent.KeyTip ChangeItem;
            public ML.Engine.TextContent.KeyTip FastAdd;
            public ML.Engine.TextContent.KeyTip Remove;
            public ML.Engine.TextContent.KeyTip Return;
            public ML.Engine.TextContent.KeyTip Confirm;
            public ML.Engine.TextContent.KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            abpath = "OCTextContent/Store";
            abname = "CreatureStorePanel";
            description = "CreatureStorePanel数据加载完成";
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
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeItem.started -= Alter_started;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed -= FastAdd_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
        }
        protected override void RegisterInput()
        {
            DataBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
            ItemBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Enable();
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed += NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeItem.started += Alter_started;
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed += FastAdd_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
        }
        private void NextPriority_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurPriority = (MissionNS.TransportPriority)(((int)Store.TransportPriority + 1) % System.Enum.GetValues(typeof(MissionNS.TransportPriority)).Length);
        }
        private void Alter_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                int dataIndex = DataIndex;
                if (CurMode == Mode.Store && offset.y < 0)
                {
                    CurMode = Mode.Creature;
                }
                else if (offset.y > 0 && 0 <= dataIndex && dataIndex < 5)
                {
                    CurMode = Mode.Store;
                }
                Refresh();
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                Store.FastAdd();
                Refresh();
            }
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                ML.Engine.InventorySystem.ItemManager.Instance.AddItemIconObject(Store.CreatureItemID, Store.WorldStore.transform,
                        new Vector3(0, Store.WorldStore.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0),
                        Quaternion.Euler(Vector3.zero), Vector3.one,
                        (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
                UIMgr.PopPanel();
            }
            else if (CurMode == Mode.ChangeItem)
            {
                CurMode = Mode.Store;
                Refresh();
            }
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Store)
            {
                CurMode = Mode.ChangeItem;
            }
            else if (CurMode == Mode.Creature)
            {
                Store.ChangeData(DataIndex, null);
            }
            else if (CurMode == Mode.ChangeItem)
            {
                Store.ChangeCreature(ItemDatas[ItemIndex]);
                CurMode = Mode.Store;
            }
            Refresh();
        }
        #endregion

        #region UI
        protected void SetUIActive()
        {
            Store_UI.gameObject.SetActive(CurMode == Mode.Store || CurMode == Mode.Creature);
            ChangeCreature_UI.gameObject.SetActive(CurMode == Mode.ChangeItem);
            KeyTips.gameObject.SetActive(CurMode == Mode.Store || CurMode == Mode.Creature);
            KeyTips1.gameObject.SetActive(CurMode == Mode.ChangeItem);

            Store_UI.Find("Icon").Find("Selected").gameObject.SetActive(CurMode == Mode.Store);

            if (CurMode == Mode.Store || CurMode == Mode.Creature)
            {
                KeyTips.Find("KT_ChangeItem").gameObject.SetActive(CurMode == Mode.Store);
                KeyTips.Find("KT_FastAdd").gameObject.SetActive(CurMode == Mode.Store);
                KeyTips.Find("KT_Remove").gameObject.SetActive(CurMode == Mode.Creature && Store.DataContainer.HaveSetData(DataIndex));
                LayoutRebuilder.ForceRebuildLayoutImmediate(KeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
            }
        }

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            CurPriority = Store.TransportPriority;
            Text_Title.text = PanelTextContent.text_Title;
            SetUIActive();
            if (CurMode == Mode.Store || CurMode == Mode.Creature)
            {
                string key = Store.CreatureItemID;
                if (!string.IsNullOrEmpty(key) && !tempSprite.ContainsKey(key))
                {
                    tempSprite[key] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(key);
                }
                Store_UI.Find("Icon").GetComponent<Image>().sprite = tempSprite[key];
                for (int i = 0; i < DataBtnList.BtnCnt; ++i)
                {
                    bool notNull = Store.DataContainer.HaveSetData(i);
                    var creature = notNull ? Store.DataContainer.GetData(i) as ML.Engine.InventorySystem.CreatureItem : null;
                    Transform item = DataBtnList.GetBtn(i).transform;
                    item.Find("OutputIcon").gameObject.SetActive(notNull);
                    item.Find("Output").gameObject.SetActive(notNull);
                    item.Find("ActivityIcon").gameObject.SetActive(notNull);
                    item.Find("Activity").gameObject.SetActive(notNull);
                    item.Find("Gender").gameObject.SetActive(notNull && creature.Gender != Gender.None);
                    item.Find("State").gameObject.SetActive(notNull && Store.DataContainer.GetAmount(i, DataNS.DataOpType.StorageAll) == 0);
                    item.Find("State").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_WaitPutIn;
                    if (notNull)
                    {
                        string itemID = creature.ID;
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            tempSprite[itemID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(itemID);
                        }
                        item.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];
                        item.Find("Output").GetComponent<TMPro.TextMeshProUGUI>().text = creature.Output.ToString();
                        item.Find("Activity").GetComponent<TMPro.TextMeshProUGUI>().text = creature.Activity.ToString();
                        item.Find("Gender").GetComponent<Image>().sprite = creature.Gender == Gender.Male ? tempSprite["Male"] : tempSprite["Female"];
                    }
                    else
                    {
                        item.Find("Icon").GetComponent<Image>().sprite = tempSprite[""];
                    }
                }
            }
            else if (CurMode == Mode.ChangeItem)
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

                #region Desc
                string recipeID = ManagerNS.LocalGameManager.Instance.CreatureManager.GetBreRecipeID(ItemDatas[ItemIndex]);
                string proRecipeID = ManagerNS.LocalGameManager.Instance.CreatureManager.GetProRecipeID(ItemDatas[ItemIndex]);
                var discard = ManagerNS.LocalGameManager.Instance.CreatureManager.GetDiscard(ItemDatas[ItemIndex]);
                string discardID = discard.id;
                int discardNum = discard.num;
                string productID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(recipeID).id;
                if (!tempSprite.ContainsKey(productID))
                {
                    tempSprite[productID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(productID);
                }
                ChangeCreature_UI.Find("Desc").Find("Icon").GetComponent<Image>().sprite = tempSprite[productID];
                bool isValidItemID = ML.Engine.InventorySystem.ItemManager.Instance.IsValidItemID(productID);
                ChangeCreature_UI.Find("Desc").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ?
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(productID) : PanelTextContent.text_Empty;
                ChangeCreature_UI.Find("Desc").Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ?
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemDescription(productID) : PanelTextContent.text_Empty;
                #region Output
                ChangeCreature_UI.Find("Desc").Find("Output").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_GetBreed;
                string proProductID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(proRecipeID).id;
                if (!tempSprite.ContainsKey(proProductID))
                {
                    tempSprite[proProductID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(proProductID);
                }
                ChangeCreature_UI.Find("Desc").Find("Output").Find("Icon").GetComponent<Image>().sprite = tempSprite[proProductID];
                ChangeCreature_UI.Find("Desc").Find("Output").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text =
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(proProductID);
                #endregion

                #region Discard
                ChangeCreature_UI.Find("Desc").Find("Discard").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_GetDiscard;
                if (!tempSprite.ContainsKey(discardID))
                {
                    tempSprite[discardID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(discardID);
                }
                ChangeCreature_UI.Find("Desc").Find("Discard").Find("Icon").GetComponent<Image>().sprite = tempSprite[discardID];
                ChangeCreature_UI.Find("Desc").Find("Discard").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text =
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(discardID);
                ChangeCreature_UI.Find("Desc").Find("Discard").Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>().text = discardNum.ToString();
                #endregion
                #endregion
            }
        }
        #endregion
    }
}