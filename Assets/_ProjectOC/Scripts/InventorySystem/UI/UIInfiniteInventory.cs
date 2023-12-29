using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

            ItemTypes = Enum.GetValues(typeof(ML.Engine.InventorySystem.ItemType)).Cast<ML.Engine.InventorySystem.ItemType>().Where(e => (int)e > 0).ToArray();
            CurrentItemTypeIndex = 0;

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
                SelectedItems.AddRange(inventory.GetItemList().Where(item => item.ItemType == CurrentItemType));
                this.Refresh();
            }
        }
        private List<Item> SelectedItems = new List<Item>();
        private int _currentItemIndex = 0;
        private int CurrentItemIndex
        {
            get => _currentItemIndex;
            set
            {
                _currentItemIndex = value;
                // 不计算隐藏的模板
                if (this._currentItemIndex >= Inventory_GridLayout.GetElementCount() - 1)
                {
                    var grid = Inventory_GridLayout.GetGridSize();
                    this._currentItemIndex %= (grid.x * grid.y);
                }
                this.Refresh();
            }
        }
        private Item CurrentItem => SelectedItems[CurrentItemIndex];

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
            }
        }

        private void AlterItem_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var f_offset = obj.ReadValue<Vector2>();
            var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
            var grid = Inventory_GridLayout.GetGridSize();
            this.CurrentItemIndex += offset.y * grid.y + grid.x;

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


        private TMPro.TextMeshProUGUI TopTitleText;

        private UIKeyTip KT_LastTerm;
        private Transform ItemTypeTemplate;
        private UIKeyTip KT_NextTerm;

        private GridLayoutGroup Inventory_GridLayout;
        private Transform UIItemTemplate;


        public void Refresh()
        {
            if(ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
            }

            // TopTitle
            this.TopTitleText.text = PanelTextContent.toptitle.GetText();

            // ItemType
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

            // Inventory
            int delta = tempUIItems.Count - SelectedItems.Count;
            // >0 => 有多余，隐藏
            if(delta > 0)
            {
                for(int i = 0; i < delta; ++i)
                {
                    tempUIItems[SelectedItems.Count - 1 - i].SetActive(false);
                }
            }
            // <0 => 不够， 增加
            else if (delta < 0)
            {
                var uiitem = Instantiate(UIItemTemplate, Inventory_GridLayout.transform, false);
                tempUIItems.Add(uiitem.gameObject);
            }
            for(int i = 0; i < SelectedItems.Count; ++i)
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
                }
                img.sprite = sprite;
                // Amount
                var amounttext = item.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                amounttext.gameObject.SetActive(SelectedItems[i].bCanStack);
                amounttext.text = SelectedItems[i].Amount.ToString();
                // Selected
                var selected = item.transform.Find("Selected");
                selected.gameObject.SetActive(CurrentItem == SelectedItems[i]);
            }
            // ItemInfo

            // BotKeyTips


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
            }
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion


    }

}
