using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ML.Engine.Extension;
using ML.Engine.Timer;
using ML.Engine.Manager;
using ML.Engine.UI;
using UnityEngine.U2D;
using UnityEngine.ResourceManagement.AsyncOperations;
using static ProjectOC.InventorySystem.UI.UIInfiniteInventory;

namespace ProjectOC.InventorySystem.UI
{
    public class UIInfiniteInventory : ML.Engine.UI.UIBasePanel<InventoryPanel>
    {
        #region Input
        /// 用于Drop和Destroy按键响应Cancel
        /// 长按响应了Destroy就置为true
        /// Cancel就不响应Drop 并 重置
        /// </summary>
        private bool ItemIsDestroyed = false;
        #endregion

        #region Unity
        public bool IsInit = false;
        public SpriteAtlas inventoryAtlas;
        protected override void Awake()
        {
            base.Awake();
            this.InitTextContentPathData();
            this.functionExecutor.AddFunction(new List<Func<AsyncOperationHandle>> {
                this.LoadInventoryAtlas});
            this.functionExecutor.SetOnAllFunctionsCompleted(() =>
            {
                this.Refresh();
            });

            StartCoroutine(functionExecutor.Execute());



            // TopTitle
            TopTitleText = this.transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            // ItemType
            var content = this.transform.Find("ItemType").Find("Content");

            ItemTypeTemplate = content.Find("ItemTypeContainer").Find("ItemTypeTemplate");
            ItemTypeTemplate.gameObject.SetActive(false);

            // Inventory
            Inventory_GridLayout = this.transform.Find("Inventory").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            UIItemTemplate = Inventory_GridLayout.transform.Find("UIItemTemplate");
            UIItemTemplate.gameObject.SetActive(false);

            // ItemInfo
            var info = this.transform.Find("ItemInfo").Find("Info");
            Info_ItemName = info.Find("Name").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Info_ItemIcon = info.Find("Icon").Find("IconImage").GetComponent<Image>();
            Info_ItemWeight = info.Find("Weight").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Info_ItemDescription = info.Find("ItemDescription").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Info_ItemEffectDescription = info.Find("EffectDescription").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();


            ItemTypes = Enum.GetValues(typeof(ML.Engine.InventorySystem.ItemType)).Cast<ML.Engine.InventorySystem.ItemType>().Where(e => (int)e > 0).ToArray();


            var kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_Use = kt.Find("KT_Use");
            KT_Drop = kt.Find("KT_Drop");
            KT_Destroy = kt.Find("KT_Destroy");

            IsInit = true;
            //Refresh();
        }

        protected override void Start()
        {
            CurrentItemTypeIndex = 0;

            base.Start();
        }

        private void OnDestroy()
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.Release(this.gameObject);
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.Release(this.inventoryAtlas);
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

        protected override void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Enable();
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(0);
            base.Enter();
        }

        protected override void Exit()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Disable();
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(1);
            this.UnregisterInput();
            base.Exit();
        }

        #endregion

        #region Internal
        /// <summary>
        /// 对应的逻辑背包
        /// </summary>
        public ML.Engine.InventorySystem.InfiniteInventory inventory;

        /// <summary>
        /// UI显示的ItemType枚举
        /// </summary>
        private ItemType[] ItemTypes;
        /// <summary>
        /// 当前选中的ItemTypes的Index
        /// </summary>
        private int _currentItemTypeIndex = 0;
        /// <summary>
        /// 当前选中的ItemType
        /// </summary>
        private ItemType CurrentItemType => ItemTypes[CurrentItemTypeIndex];
        /// <summary>
        /// 封装的ItemTypeIndex，便于在更新值时一并更新其他数据并Refresh
        /// </summary>
        private int CurrentItemTypeIndex
        {
            get => _currentItemTypeIndex;
            set
            {
                _currentItemTypeIndex = value;
                SelectedItems.Clear();
                SelectedItems.AddRange(inventory.GetItemList().Where(item => ItemManager.Instance.GetItemType(item.ID) == CurrentItemType));
                CurrentItemIndex = 0;
            }
        }
        /// <summary>
        /// UIPanel.Inventory区域控制处理的Item列表(根据选中的ItemType筛选出来的)
        /// </summary>
        [ShowInInspector]
        private List<Item> SelectedItems = new List<Item>();
        /// <summary>
        /// 上一次选中的ItemIndex，用于移动滑动窗口
        /// </summary>
        private int _lastItemIndex = 0;
        /// <summary>
        /// 当前选中的ItemIndex
        /// </summary>
        private int _currentItemIndex = 0;
        /// <summary>
        /// 封装，方便更新数据和Refresh
        /// </summary>
        private int CurrentItemIndex
        {
            get => _currentItemIndex;
            set
            {
                int last = _currentItemIndex;
                if (SelectedItems.Count > 0)
                {
                    _currentItemIndex = value;
                    if (_currentItemIndex == -1)
                    {
                        _currentItemIndex = SelectedItems.Count - 1;
                    }
                    else if (_currentItemIndex == SelectedItems.Count)
                    {
                        _currentItemIndex = 0;
                    }
                    else
                    {
                        var grid = Inventory_GridLayout.GetGridSize();
                        if (_currentItemIndex < 0)
                        {
                            _currentItemIndex += (grid.x * grid.y);
                        }
                        else if (_currentItemIndex >= SelectedItems.Count)
                        {
                            _currentItemIndex -= (grid.x * grid.y);
                            if (_currentItemIndex < 0)
                            {
                                _currentItemIndex += grid.y;
                            }
                        }
                        // 不计算隐藏的模板
                        while (this._currentItemIndex >= SelectedItems.Count)
                        {
                            this._currentItemIndex -= grid.y;
                        }
                    }
                }
                else
                {
                    _currentItemIndex = 0;
                }
                if (last != _currentItemIndex)
                {
                    _lastItemIndex = last;
                }
                this.Refresh();
            }
        }
        /// <summary>
        /// 当前选中的Item
        /// </summary>
        private Item CurrentItem
        {
            get
            {
                if (CurrentItemIndex < SelectedItems.Count)
                {
                    return SelectedItems[CurrentItemIndex];
                }
                return null;
            }
        }

        private void UnregisterInput()
        {
            // 切换类目
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed -= LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed -= NextTerm_performed;
            // 切换Item
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.started -= AlterItem_started;
            //ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.performed -= AlterItem_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.canceled -= AlterItem_canceled;
            // 使用
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Comfirm_performed;
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            // 丢弃
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Drop.performed -= Drop_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Drop.canceled -= Drop_canceled;
            /*            // 销毁
                        ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.started -= Destroy_started;
                        ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.performed -= Destroy_performed;
                        ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.canceled -= Destroy_canceled;*/
        }

        private void RegisterInput()
        {
            // 切换类目
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed += LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed += NextTerm_performed;
            // 切换Item
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.started += AlterItem_started;
            //ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.performed += AlterItem_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.canceled += AlterItem_canceled;
            // 使用
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Comfirm_performed;
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

            // 丢弃
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Drop.performed += Drop_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Drop.canceled += Drop_canceled;
            /*            // 销毁
                        ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.started += Destroy_started;
                        ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.performed += Destroy_performed;
                        ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.canceled += Destroy_canceled;*/

        }

        public void DropItem()
        {
            var item = this.CurrentItem;
            this.inventory.RemoveItem(item);
            SelectedItems.Remove(item);
            this.CurrentItemIndex = this.CurrentItemIndex;
            // 将Item生成为世界物体
#pragma warning disable CS4014
            ItemManager.Instance.SpawnWorldItem(item, this.inventory.Owner.position, this.inventory.Owner.rotation);
#pragma warning restore CS4014
        }

        public void DestroyItem()
        {
            var item = this.CurrentItem;
            this.inventory.RemoveItem(item);
            SelectedItems.Remove(item);
            this.CurrentItemIndex = this.CurrentItemIndex;
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }

        private void Comfirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(this.CurrentItem.CanUse())
            {
                this.CurrentItem.Execute(1);
                this.CurrentItemIndex = this.CurrentItemIndex;
            }
        }

        #region AlterItem_performed
        private float TimeInterval = 0.1f;
        CounterDownTimer timer;
        #endregion


        private void AlterItem_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.CounterDownTimerManager.RemoveTimer(timer);
            timer = new CounterDownTimer(TimeInterval, true, true, 1, 2);
            timer.OnEndEvent += () =>
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));

                var grid = Inventory_GridLayout.GetGridSize();
                this.CurrentItemIndex += -offset.y * grid.y + offset.x;

            };
        }

        private void AlterItem_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.CounterDownTimerManager.RemoveTimer(timer);
        }



        private void LastTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurrentItemTypeIndex = (CurrentItemTypeIndex - 1 + ItemTypes.Length) % ItemTypes.Length;
        }

        private void NextTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurrentItemTypeIndex = (CurrentItemTypeIndex + 1 + ItemTypes.Length) % ItemTypes.Length;
        }


        /// <summary>
        /// Drop
        /// </summary>
        /// <param name="obj"></param>
        private void Drop_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.ItemIsDestroyed)
            {
                this.ItemIsDestroyed = false;
            }
            else
            {
                DropItem();
            }
        }

        /// <summary>
        /// Destroy
        /// </summary>
        /// <param name="obj"></param>
        private void Drop_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.ItemIsDestroyed = true;
            Debug.Log("DestroyItem");
            DestroyItem();
        }


        /// <summary>
        /// Destroy
        /// </summary>
        /// <param name="obj"></param>

        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private Dictionary<ML.Engine.InventorySystem.ItemType, GameObject> tempItemType = new Dictionary<ML.Engine.InventorySystem.ItemType, GameObject>();
        private List<GameObject> tempUIItems = new List<GameObject>();

        private void ClearTemp()
        {
            foreach(var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempItemType.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItems)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }

        }

        #endregion

        #region UI对象引用

        private UIKeyTipComponent[] UIKeyTipComponents;

        private TMPro.TextMeshProUGUI TopTitleText;

        private Transform ItemTypeTemplate;

        private GridLayoutGroup Inventory_GridLayout;
        private Transform UIItemTemplate;

        private TMPro.TextMeshProUGUI Info_ItemName;
        private Image Info_ItemIcon;
        private TMPro.TextMeshProUGUI Info_ItemWeight;
        private TMPro.TextMeshProUGUI Info_ItemDescription;
        private TMPro.TextMeshProUGUI Info_ItemEffectDescription;

        private Transform KT_Use;
        private Transform KT_Drop;
        private Transform KT_Destroy;


        #endregion

        public override void Refresh()
        {
            // 加载完成JSON数据 & 查找完所有引用
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }

            #region TopTitle
            // 更新标题文本
            this.TopTitleText.text = PanelTextContent.toptitle.GetText();
            #endregion

            #region ItemType
 
            foreach (var itemtype in ItemTypes)
            {
                // 对应的ItemType显示对象不存在，则实例化生成
                if (!tempItemType.TryGetValue(itemtype, out var obj))
                {
                    // 实例化
                    obj = Instantiate(ItemTypeTemplate.gameObject, ItemTypeTemplate.parent, false);
                    // 模板是false,需要设置为true
                    obj.SetActive(true);
                    // 加入临时内存管理
                    tempItemType.Add(itemtype, obj);
                    // 载入ItemType对应的Texture2D
                    var sprite = inventoryAtlas.GetSprite(itemtype.ToString());
                    //var tex = ab.LoadAsset<Texture2D>(itemtype.ToString());
                    // 创建Sprite并加入临时内存管理

                    if (sprite != null)
                    {
                        obj.transform.Find("Image").GetComponent<Image>().sprite = sprite;
                        tempSprite.Add(sprite);
                    }
                }

                // 刷新显示文本
                obj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.itemtype.FirstOrDefault(it => it.name == itemtype.ToString()).GetDescription();
                // 更新 Selected
                var selected = obj.transform.Find("Selected").gameObject;
                selected.SetActive(CurrentItemType == itemtype);
            }


            // 强制刷新Layout布局
            // Unity 底层不会自动更新，需要手动调用

            LayoutRebuilder.ForceRebuildLayoutImmediate(this.ItemTypeTemplate.parent.GetComponent<RectTransform>());
            #endregion

            #region Inventory
            // 临时内存生成的UIItem数量(只增不减，多的隐藏掉即可) - 当前筛选出来的Item数量
            int delta = tempUIItems.Count - SelectedItems.Count;
            // > 0 => 有多余，隐藏
            if (delta > 0)
            {
                for(int i = 0; i < delta; ++i)
                {
                    tempUIItems[tempUIItems.Count - 1 - i].SetActive(false);
                }
            }
            // < 0 => 不够， 增加
            else if (delta < 0)
            {
                delta = -delta;
                for (int i = 0; i < delta; ++i)
                {
                    var uiitem = Instantiate(UIItemTemplate, Inventory_GridLayout.transform, false);
                    tempUIItems.Add(uiitem.gameObject);
                }
            }

            // 用于更新滑动窗口
            // 当前选中的UIItem

            GameObject cur = null;
            // 上一个UIItem
            GameObject last = null;

            // 遍历筛选的ItemList
            for (int i = 0; i < SelectedItems.Count; ++i)
            {
                var item = tempUIItems[i];
                // Active
                item.SetActive(true);
                // 更新Icon
                var img = item.transform.Find("Icon").GetComponent<Image>();
                // 查找临时存储的Sprite
                var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(SelectedItems[i].ID));
                // 不存在则生成
                if (sprite == null)
                {
                    sprite = ItemManager.Instance.GetItemSprite(SelectedItems[i].ID);
                    tempSprite.Add(sprite);
                }
                img.sprite = sprite;
                // Amount
                var amounttext = item.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                amounttext.gameObject.SetActive(ItemManager.Instance.GetCanStack(SelectedItems[i].ID));
                amounttext.text = SelectedItems[i].Amount.ToString();
                // Selected
                var selected = item.transform.Find("Selected");
                if(CurrentItem == SelectedItems[i])
                {
                    selected.gameObject.SetActive(true);
                    cur = item;
                }
                else
                {
                    selected.gameObject.SetActive(false);
                }
                if(i == _lastItemIndex)
                {
                    last = item;
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
                Inventory_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
            }
            #endregion


            // 强制立即更新 GridLayoutGroup 的布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(Inventory_GridLayout.GetComponent<RectTransform>());
            #endregion

            #region ItemInfo
            if (CurrentItem != null)
            {
                // 显示ItemInfo
                this.transform.Find("ItemInfo").gameObject.SetActive(true);
                // 更新Item名称
                Info_ItemName.text = ItemManager.Instance.GetItemName(CurrentItem.ID);
                // 更新图标 => 必定是载入了的
                Info_ItemIcon.sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(CurrentItem.ID));
                // 更新单个重量: JsonText + Amount
                Info_ItemWeight.text = PanelTextContent.weightprefix + ItemManager.Instance.GetWeight(CurrentItem.ID);

                // 更新描述文本: JsonText + ItemDescription
                Info_ItemDescription.text = PanelTextContent.descriptionprefix + "\n" + ItemManager.Instance.GetItemDescription(CurrentItem.ID);
                // 强制更新布局 => 更新文本高度 -> 用于更新父物体的高度，适配UI显示
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_ItemDescription.GetComponent<RectTransform>());
                // 更新父物体的高度
                Info_ItemDescription.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(Info_ItemDescription.transform.parent.GetComponent<RectTransform>().sizeDelta.x, Info_ItemDescription.GetComponent<RectTransform>().sizeDelta.y);


                Info_ItemEffectDescription.text = PanelTextContent.effectdescriptionprefix + "\n" + ItemManager.Instance.GetEffectDescription(CurrentItem.ID);
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_ItemEffectDescription.GetComponent<RectTransform>());
                Info_ItemEffectDescription.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(Info_ItemEffectDescription.transform.parent.GetComponent<RectTransform>().sizeDelta.x, Info_ItemEffectDescription.GetComponent<RectTransform>().sizeDelta.y);

                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_ItemEffectDescription.transform.parent.parent.GetComponent<RectTransform>());
            }
            else
            {
                this.transform.Find("ItemInfo").gameObject.SetActive(false);
            }
            #endregion


            #region BotKeyTips
            if (CurrentItem != null)
            {
                KT_Use.gameObject.SetActive(CurrentItem.CanUse());
                KT_Drop.gameObject.SetActive(CurrentItem.CanDrop());
                KT_Destroy.gameObject.SetActive(CurrentItem.CanDestroy());
            }
            #endregion


        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct InventoryPanel
        {
            public TextContent toptitle;
            public TextTip[] itemtype;
            public KeyTip LastTerm;
            public KeyTip NextTerm;
            public TextContent weightprefix;
            public TextContent descriptionprefix;
            public TextContent effectdescriptionprefix;
            public KeyTip Use;
            public KeyTip Back;
            public KeyTip Drop;
            public KeyTip Destroy;
        }

        private void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/Inventory";
            this.abname = "InventoryPanel";
            this.description = "InventoryPanel数据加载完成";
        }
        #endregion

        private AsyncOperationHandle LoadInventoryAtlas()
        {
            var handle = GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>("OC/UI/Inventory/Texture/SA_Inventory_UI.spriteatlasv2");

            handle.Completed += (handle) =>
            {
                inventoryAtlas = handle.Result as SpriteAtlas;
            };
            return handle;
        }

    }

}
