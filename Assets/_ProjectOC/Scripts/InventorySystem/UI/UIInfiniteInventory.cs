using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ProjectOC.InventorySystem.UI
{
    public class UIInfiniteInventory : ML.Engine.UI.UIBasePanel
    {

        #region Input
        /// <summary>
        /// 用于Drop和Destroy按键响应Cancel
        /// 长按响应了Destroy就置为true
        /// Cancel就不响应Drop 并 重置
        /// </summary>
        private bool ItemIsDestroyed = false;
        #endregion

        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();



            // TopTitle
            TopTitleText = this.transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            // ItemType
            var content = this.transform.Find("ItemType").Find("Content");
            KT_LastTerm = new UIKeyTip();
            KT_LastTerm.img = content.Find("KT_LastTerm").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_LastTerm.keytip = KT_LastTerm.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_NextTerm = new UIKeyTip();
            KT_NextTerm.img = content.Find("KT_NextTerm").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_NextTerm.keytip = KT_NextTerm.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();

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

            // BotKeyTips
            var kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_Use = new UIKeyTip();
            KT_Use.img = kt.Find("KT_Use").Find("Image").GetComponent<Image>();
            KT_Use.keytip = KT_Use.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Use.description = KT_Use.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Back = new UIKeyTip();
            KT_Back.img = kt.Find("KT_Back").Find("Image").GetComponent<Image>();
            KT_Back.keytip = KT_Back.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Back.description = KT_Back.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Drop = new UIKeyTip();
            KT_Drop.img = kt.Find("KT_Drop").Find("Image").GetComponent<Image>();
            KT_Drop.keytip = KT_Drop.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Drop.description = KT_Drop.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Destroy = new UIKeyTip();
            KT_Destroy.img = kt.Find("KT_Destroy").Find("Image").GetComponent<Image>();
            KT_Destroy.keytip = KT_Destroy.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Destroy.description = KT_Destroy.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            ItemTypes = Enum.GetValues(typeof(ML.Engine.InventorySystem.ItemType)).Cast<ML.Engine.InventorySystem.ItemType>().Where(e => (int)e > 0).ToArray();
            CurrentItemTypeIndex = 0;


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
        public ML.Engine.InventorySystem.InfiniteInventory inventory;

        private ItemType[] ItemTypes;
        private int _currentItemTypeIndex = 0;
        private ItemType CurrentItemType => ItemTypes[CurrentItemTypeIndex];
        private int CurrentItemTypeIndex
        {
            get => _currentItemTypeIndex;
            set
            {
                _currentItemTypeIndex = value;
                SelectedItems.Clear();
                SelectedItems.AddRange(inventory.GetItemList().Where(item => ItemSpawner.Instance.GetItemType(item.ID) == CurrentItemType));
                CurrentItemIndex = 0;
                this.Refresh();
            }
        }
        [ShowInInspector]
        private List<Item> SelectedItems = new List<Item>();
        private int _lastItemIndex = 0;
        private int _currentItemIndex = 0;
        private int CurrentItemIndex
        {
            get => _currentItemIndex;
            set
            {
                int last = _currentItemIndex;
                if(SelectedItems.Count > 0)
                {
                    _currentItemIndex = value;
                    if(_currentItemIndex == -1)
                    {
                        _currentItemIndex = SelectedItems.Count - 1;
                    }
                    else if(_currentItemIndex == SelectedItems.Count)
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
                if(last != _currentItemIndex)
                {
                    _lastItemIndex = last;
                }
                this.Refresh();
            }
        }
        private Item CurrentItem
        {
            get
            {
                return SelectedItems[CurrentItemIndex];
            }
        }

        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Enable();
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(0);
            this.Refresh();
        }

        private void Exit()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Disable();
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(1);
            this.UnregisterInput();
        }

        private void UnregisterInput()
        {
            // 切换类目
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed -= LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed -= NextTerm_performed;
            // 切换Item
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.performed -= AlterItem_performed;
            // 使用
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed -= Comfirm_performed;
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            // 丢弃
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.DropAndDestroy.canceled -= DropAndDestroy_canceled;
            // 销毁
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.DropAndDestroy.performed -= DropAndDestroy_performed;
        }

        private void RegisterInput()
        {
            // 切换类目
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed += LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed += NextTerm_performed;
            // 切换Item
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.performed += AlterItem_performed;
            // 使用
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed += Comfirm_performed;
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            // 丢弃
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.DropAndDestroy.canceled += DropAndDestroy_canceled;
            // 销毁
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.DropAndDestroy.performed += DropAndDestroy_performed;
        }

        public void DropItem()
        {
            var item = this.CurrentItem;
            this.inventory.RemoveItem(item);
            SelectedItems.Remove(item);
            this.CurrentItemIndex = this.CurrentItemIndex;
            // 将Item生成为世界物体
            ItemSpawner.Instance.SpawnWorldItem(item, this.inventory.Owner.position, this.inventory.Owner.rotation);
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

        private void AlterItem_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var f_offset = obj.ReadValue<Vector2>();
            var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
            var grid = Inventory_GridLayout.GetGridSize();
            this.CurrentItemIndex += -offset.y * grid.y + offset.x;

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
        private void DropAndDestroy_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
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
        private void DropAndDestroy_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.ItemIsDestroyed = true;
            DestroyItem();
        }

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
                Destroy(s);
            }
            foreach (var s in tempItemType.Values)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItems)
            {
                Destroy(s);
            }
        }

        #endregion

        #region UI对象引用
        private TMPro.TextMeshProUGUI TopTitleText;

        private UIKeyTip KT_LastTerm;
        private Transform ItemTypeTemplate;
        private UIKeyTip KT_NextTerm;

        private GridLayoutGroup Inventory_GridLayout;
        private Transform UIItemTemplate;

        private TMPro.TextMeshProUGUI Info_ItemName;
        private Image Info_ItemIcon;
        private TMPro.TextMeshProUGUI Info_ItemWeight;
        private TMPro.TextMeshProUGUI Info_ItemDescription;
        private TMPro.TextMeshProUGUI Info_ItemEffectDescription;

        private UIKeyTip KT_Use;
        private UIKeyTip KT_Back;
        private UIKeyTip KT_Drop;
        private UIKeyTip KT_Destroy;
        #endregion
        
        public void Refresh()
        {
            if(ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
            }

            #region TopTitle
            this.TopTitleText.text = PanelTextContent.toptitle.GetText();
            #endregion

            #region ItemType
            this.KT_LastTerm.ReWrite(PanelTextContent.lastterm);
            this.KT_NextTerm.ReWrite(PanelTextContent.nextterm);
            foreach(var itemtype in ItemTypes)
            {
                if(!tempItemType.TryGetValue(itemtype, out var obj))
                {
                    obj = Instantiate(ItemTypeTemplate.gameObject, ItemTypeTemplate.parent, false);
                    obj.SetActive(true);
                    tempItemType.Add(itemtype, obj);
                    var ab = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadLocalAB("UI/Inventory/Texture2D/ItemType");
                    var tex = ab.LoadAsset<Texture2D>(itemtype.ToString());
                    if(tex != null)
                    {
                        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        obj.transform.Find("Image").GetComponent<Image>().sprite = sprite;
                        tempSprite.Add(sprite);
                    }
                }
                obj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.itemtype.FirstOrDefault(it => it.name == itemtype.ToString()).GetDescription();
                var selected = obj.transform.Find("Selected").gameObject;
                // 更新 Selected
                selected.SetActive(CurrentItemType == itemtype);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(this.ItemTypeTemplate.parent.GetComponent<RectTransform>());
            #endregion

            #region Inventory
            int delta = tempUIItems.Count - SelectedItems.Count;
            // >0 => 有多余，隐藏
            if(delta > 0)
            {
                for(int i = 0; i < delta; ++i)
                {
                    tempUIItems[tempUIItems.Count - 1 - i].SetActive(false);
                }
            }
            // <0 => 不够， 增加
            else if (delta < 0)
            {
                delta = -delta;
                for (int i = 0; i < delta; ++i)
                {
                    var uiitem = Instantiate(UIItemTemplate, Inventory_GridLayout.transform, false);
                    tempUIItems.Add(uiitem.gameObject);
                }
            }

            GameObject cur = null;
            GameObject last = null;

            for (int i = 0; i < SelectedItems.Count; ++i)
            {
                var item = tempUIItems[i];
                // Active
                item.SetActive(true);
                // Icon
                var img = item.transform.Find("Icon").GetComponent<Image>();
                var sprite = tempSprite.Find(s => s.texture == ItemSpawner.Instance.GetItemTexture2D(SelectedItems[i].ID));
                if(sprite == null)
                {
                    sprite = ItemSpawner.Instance.GetItemSprite(SelectedItems[i].ID);
                    tempSprite.Add(sprite);
                }
                img.sprite = sprite;
                // Amount
                var amounttext = item.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                amounttext.gameObject.SetActive(ItemSpawner.Instance.GetCanStack(SelectedItems[i].ID));
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
            if(cur != null && last != null)
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
                #endregion


            }
            else
            {
                Inventory_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
            }
            // 强制立即更新 GridLayoutGroup 的布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(Inventory_GridLayout.GetComponent<RectTransform>());
            #endregion

            #region ItemInfo
            Info_ItemName.text = ItemSpawner.Instance.GetItemName(CurrentItem.ID);
            Info_ItemIcon.sprite = tempSprite.Find(s => s.texture == ItemSpawner.Instance.GetItemTexture2D(CurrentItem.ID));
            Info_ItemWeight.text = PanelTextContent.weightprefix + ItemSpawner.Instance.GetWeight(CurrentItem.ID);
            Info_ItemDescription.text = PanelTextContent.descriptionprefix + "\n" + ItemSpawner.Instance.GetItemDescription(CurrentItem.ID);
            Info_ItemEffectDescription.text = PanelTextContent.effectdescriptionprefix + "\n" + ItemSpawner.Instance.GetEffectDescription(CurrentItem.ID);
            #endregion

            #region BotKeyTips
            KT_Use.ReWrite(PanelTextContent.use);
            KT_Use.img.transform.parent.gameObject.SetActive(CurrentItem.CanUse());
            KT_Back.ReWrite(PanelTextContent.back);
            KT_Drop.ReWrite(PanelTextContent.drop);
            KT_Drop.img.transform.parent.gameObject.SetActive(CurrentItem.CanDrop());
            KT_Destroy.ReWrite(PanelTextContent.destroy);
            KT_Destroy.img.transform.parent.gameObject.SetActive(CurrentItem.CanDestroy());
            #endregion


        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct InventoryPanel
        {
            public TextContent toptitle;
            public TextTip[] itemtype;
            public KeyTip lastterm;
            public KeyTip nextterm;
            public TextContent weightprefix;
            public TextContent descriptionprefix;
            public TextContent effectdescriptionprefix;
            public KeyTip use;
            public KeyTip back;
            public KeyTip drop;
            public KeyTip destroy;
        }

        public static InventoryPanel PanelTextContent => ABJAProcessor.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<InventoryPanel> ABJAProcessor;

        private void InitUITextContents()
        {
            if(ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<InventoryPanel>("JSON/TextContent/Inventory", "InventoryPanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI背包Panel数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region to-delete
        [Button("生成测试文件")]
        void GenTESTFILE()
        {
            List<ItemSpawner.ItemTableJsonData> datas = new List<ItemSpawner.ItemTableJsonData>();

            var itypes = Enum.GetValues(typeof(ML.Engine.InventorySystem.ItemType)).Cast<ML.Engine.InventorySystem.ItemType>().Where(e => (int)e > 0).ToArray();
            foreach(var itype in itypes)
            {
                int cnt = UnityEngine.Random.Range(50, 100);
                for(int i = 0; i < cnt; ++i)
                {
                    var data = new ItemSpawner.ItemTableJsonData();
                    // id
                    data.id = itype.ToString() + "_" + i;
                    // name
                    data.name = new TextContent();
                    data.name.Chinese = data.id;
                    data.name.English = data.id;
                    // type
                    data.type = "ResourceItem";
                    // sort
                    data.sort = i;
                    // itemtype
                    data.itemtype = itype;
                    // weight
                    data.weight = UnityEngine.Random.Range(1, 10);
                    // bcanstack
                    data.bcanstack = UnityEngine.Random.Range(1, 10) < 9;
                    // maxamount
                    data.maxamount = 999;
                    // texture2d
                    data.texture2d = "100001";
                    // worldobject
                    data.worldobject = "TESTWorldItem";
                    // description
                    data.description = "TTTTTTTTTTTTTTTTTTTTTTTT\nXXXXXXXXXXXXXXXXXXXXXXXX\nTTTTTTTTTTTTTTTTTTTTTTTT";
                    // effectsDescription
                    data.effectsDescription = "<color=#6FB502><b><sprite name=\"Triangle\" index=0 tint=1>+10%金币掉落\n<color=#6FB502><b><sprite name=\"Triangle\" index=0 tint=1>+10%攻击力持续300s</b></color>";
                    datas.Add(data);
                }
            }

            string json = JsonConvert.SerializeObject(datas.ToArray(), Formatting.Indented);

            System.IO.File.WriteAllText(Application.streamingAssetsPath + "/../../../t.json", json);
            Debug.Log("输出路径: " + System.IO.Path.GetFullPath(Application.streamingAssetsPath + "/../../../t.json"));
        }
        
        #endregion
    }

}
