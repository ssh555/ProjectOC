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
using ProjectOC.ManagerNS;
using ML.Engine.Utility;

namespace ProjectOC.InventorySystem.UI
{
    public class UIInfiniteInventory : ML.Engine.UI.UIBasePanel<InventoryPanel>
    {
        #region ���ݳ�ʼ��
        //Ӧ�ó���Ϊ������categoryManages
        [ShowInInspector]
        private List<CategoryManage> categoryManages;
        private void InitData()
        {
            this.categoryManages = ItemManager.Instance.GetCategoryManageByApplicationScenario(ApplicationScenario.Bag);
        }
        #endregion

        #region Unity
        public bool IsInit = false;
        public SpriteAtlas inventoryAtlas;
        protected override void Awake()
        {
            base.Awake();

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
            Info_ItemWeight = info.Find("Weight").Find("Image").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Info_ItemDescription = info.Find("ItemDescription").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Info_ItemEffectDescription = info.Find("EffectDescription").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();



            var kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_Use = kt.Find("KT_Use");

            KT_Destroy = kt.Find("KT_Destroy");

            IsInit = true;
        }

        protected override void Start()
        {
            CurrentItemTypeIndex = 0;
            base.Start();
        }
        protected override void OnDestroy()
        {
            GameManager.Instance.ABResourceManager.Release(inventoryAtlas);
        }
        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            InitData();
        }

        protected override void Enter()
        {
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(0);
            base.Enter();
        }

        protected override void Exit()
        {
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(1);
            ClearTemp();
            base.Exit();
        }

        #endregion

        #region Internal
        /// <summary>
        /// ��Ӧ���߼�����
        /// </summary>
        public ML.Engine.InventorySystem.InfiniteInventory inventory;
        /// <summary>
        /// ��ǰѡ�е�ItemTypes��Index
        /// </summary>
        private int _currentItemTypeIndex = 0;
        /// <summary>
        /// ��ǰѡ�е�ItemType
        /// </summary>
        [ShowInInspector]
        private List<ItemType> CurrentItemCategory => this.categoryManages != null ? (this.categoryManages.Count > 0 ? this.categoryManages[CurrentItemTypeIndex].ItemTypes : new List<ItemType>()) : null;
        /// <summary>
        /// ��װ��ItemTypeIndex�������ڸ���ֵʱһ�������������ݲ�Refresh
        /// </summary>
        private int CurrentItemTypeIndex
        {
            get => _currentItemTypeIndex;
            set
            {
                _currentItemTypeIndex = value;
                SelectedItems.Clear();

                foreach (var Category in CurrentItemCategory)
                {
                    SelectedItems.AddRange(inventory.GetItemList().Where(item => ItemManager.Instance.GetItemType(item.ID) == Category));
                }
                CurrentItemIndex = 0;
            }
        }
        /// <summary>
        /// UIPanel.Inventory������ƴ����Item�б�(����ѡ�е�ItemTypeɸѡ������)
        /// </summary>
        [ShowInInspector]
        private List<Item> SelectedItems = new List<Item>();
        /// <summary>
        /// ��һ��ѡ�е�ItemIndex�������ƶ���������
        /// </summary>
        private int _lastItemIndex = 0;
        /// <summary>
        /// ��ǰѡ�е�ItemIndex
        /// </summary>
        private int _currentItemIndex = 0;
        /// <summary>
        /// ��װ������������ݺ�Refresh
        /// </summary>
        private int CurrentItemIndex
        {
            get => _currentItemIndex;
            set
            {
                int tmp = _currentItemIndex;
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
                        var grid = Inventory_GridLayout.GetGridSize(SelectedItems.Count);
                        if (_currentItemIndex < 0)
                        {
                            //_currentItemIndex += (grid.x * grid.y);
                            _currentItemIndex = tmp;
                        }
                        else if (_currentItemIndex >= SelectedItems.Count)
                        {
                            /*_currentItemIndex -= (grid.x * grid.y);
                            if (_currentItemIndex < 0)
                            {
                                _currentItemIndex += grid.y;
                            }*/
                            _currentItemIndex = tmp;
                        }
                        // ���������ص�ģ��
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
        /// ��ǰѡ�е�Item
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

        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Disable();
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed -= LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed -= NextTerm_performed;
            // �л�Item
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.started -= AlterItem_started;

            // ʹ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Comfirm_performed;
            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            // ����
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.performed -= Destroy_performed;
        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Enable();
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed += LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed += NextTerm_performed;
            // �л�Item
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.started += AlterItem_started;
            // ʹ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Comfirm_performed;
            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

            // ����
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.performed += Destroy_performed;
        }

        private void Destroy_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var item = this.CurrentItem;

            if(ItemManager.Instance.GetCanDestroy(item.ID))
            {
                this.inventory.RemoveItem(item);
                SelectedItems.Remove(item);
                this.CurrentItemIndex = this.CurrentItemIndex;
            }
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }

        private void Comfirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (ItemManager.Instance.GetCanUse(this.CurrentItem.ID))
            {
                this.CurrentItem.Execute(1);
                this.CurrentItemIndex = this.CurrentItemIndex;
            }
        }

        private void AlterItem_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var f_offset = obj.ReadValue<Vector2>();


            var vector2 = obj.ReadValue<UnityEngine.Vector2>();
            float angle = Mathf.Atan2(vector2.x, vector2.y);

            angle = angle * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle = angle + 360;
            }


            if (angle < 45 || angle > 315)
            {
                f_offset = new Vector2(0, 1);
            }
            else if (angle > 45 && angle < 135)
            {
                f_offset = new Vector2(1, 0);
            }
            else if (angle > 135 && angle < 225)
            {
                f_offset = new Vector2(0, -1);
            }
            else if (angle > 225 && angle < 315)
            {
                f_offset = new Vector2(-1, 0);
            }

            var grid = Inventory_GridLayout.GetGridSize(SelectedItems.Count);
            this.CurrentItemIndex += -(int)f_offset.y * grid.y + (int)f_offset.x;
        }

        private void LastTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (categoryManages.Count == 0) return;
            CurrentItemTypeIndex = (CurrentItemTypeIndex - 1 + categoryManages.Count) % categoryManages.Count;
        }

        private void NextTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (categoryManages.Count == 0) return;
            CurrentItemTypeIndex = (CurrentItemTypeIndex + 1 + categoryManages.Count) % categoryManages.Count;
        }
        /// <summary>
        /// Destroy
        /// </summary>
        /// <param name="obj"></param>

        #endregion

        #region UI
        #region Temp
        private Dictionary<string,Sprite> spriteDictionart = new Dictionary<string, Sprite>();
        private Dictionary<ML.Engine.InventorySystem.ItemType, GameObject> tempItemType = new Dictionary<ML.Engine.InventorySystem.ItemType, GameObject>();
        private List<GameObject> tempUIItems = new List<GameObject>();

        private void ClearTemp()
        {
            foreach(var s in spriteDictionart.Values)
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

        #region UI��������
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

        private Transform KT_Destroy;

        //��ʼ��Category ֻ��ʼ��һ��
        private bool isInitCategory = false;

        #endregion

        public override void Refresh()
        {
            // �������JSON���� & ��������������
            if (!this.objectPool.IsLoadFinish())
            {
                return;
            }

            #region TopTitle
            // ���±����ı�
            this.TopTitleText.text = PanelTextContent.toptitle.GetText();
            #endregion

            #region ItemType
            if(isInitCategory == false)
            {
                foreach (var itemtype in this.categoryManages)
                {
                    // ʵ����
                    var obj = Instantiate(ItemTypeTemplate.gameObject, ItemTypeTemplate.parent, false);
                    // ģ����false,��Ҫ����Ϊtrue
                    obj.SetActive(true);
                    // ����ItemType��Ӧ��Texture2D
                    Sprite sprite = null;
                    sprite = inventoryAtlas.GetSprite(itemtype.CategoryIcon);
                    //var tex = ab.LoadAsset<Texture2D>(itemtype.ToString());
                    // ����Sprite��������ʱ�ڴ����

                    if (sprite != null)
                    {
                        obj.transform.Find("Image").GetComponent<Image>().sprite = sprite;
                    }

                    // ˢ����ʾ�ı�
                    obj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = itemtype.CategoryName.ToString();


                    // ���� Selected
                    var selected = obj.transform.Find("Selected").gameObject;
                    selected.SetActive(CurrentItemCategory == itemtype.ItemTypes);
                }


                // ǿ��ˢ��Layout����
                // Unity �ײ㲻���Զ����£���Ҫ�ֶ�����

                LayoutRebuilder.ForceRebuildLayoutImmediate(this.ItemTypeTemplate.parent.GetComponent<RectTransform>());
                isInitCategory = true;
            }

            //����ѡ��
            for (int i = 0; i < this.ItemTypeTemplate.parent.childCount; i++)
            {
                this.ItemTypeTemplate.parent.GetChild(i).Find("Selected").gameObject.SetActive(i - 1 == this.CurrentItemTypeIndex);
            }
            #endregion

            #region Inventory
            // ��ʱ�ڴ����ɵ�UIItem����(ֻ��������������ص�����) - ��ǰɸѡ������Item����
            int delta = tempUIItems.Count - SelectedItems.Count;
            // > 0 => �ж��࣬����
            if (delta > 0)
            {
                for(int i = 0; i < delta; ++i)
                {
                    tempUIItems[tempUIItems.Count - 1 - i].SetActive(false);
                }
            }
            // < 0 => ������ ����
            else if (delta < 0)
            {
                delta = -delta;
                for (int i = 0; i < delta; ++i)
                {
                    var uiitem = Instantiate(UIItemTemplate, Inventory_GridLayout.transform, false);
                    tempUIItems.Add(uiitem.gameObject);
                }
            }

            // ���ڸ��»�������
            // ��ǰѡ�е�UIItem

            GameObject cur = null;
            // ��һ��UIItem
            GameObject last = null;

            // ����ɸѡ��ItemList
            for (int i = 0; i < SelectedItems.Count; ++i)
            {
                var item = tempUIItems[i];
                // Active
                item.SetActive(true);
                // ����Icon
                var img = item.transform.Find("Icon").GetComponent<Image>();
                // ������ʱ�洢��Sprite
                Sprite sprite = null;
                // ������������
                if (!spriteDictionart.TryGetValue(SelectedItems[i].ID,out sprite))
                {
                    sprite = ItemManager.Instance.GetItemSprite(SelectedItems[i].ID);
                    spriteDictionart.Add(SelectedItems[i].ID,sprite);
                }
                img.sprite = sprite;
                // Amount
                var amounttext = item.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                amounttext.gameObject.SetActive(ItemManager.Instance.GetCanStack(SelectedItems[i].ID));
                amounttext.text = SelectedItems[i].Amount > 999 ? "999+" : SelectedItems[i].Amount.ToString();
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
                Inventory_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
            }
            #endregion


            // ǿ���������� GridLayoutGroup �Ĳ���
            LayoutRebuilder.ForceRebuildLayoutImmediate(Inventory_GridLayout.GetComponent<RectTransform>());
            #endregion

            #region ItemInfo
            if (CurrentItem != null)
            {
                // ��ʾItemInfo
                this.transform.Find("ItemInfo").gameObject.SetActive(true);
                // ����Item����
                Info_ItemName.text = ItemManager.Instance.GetItemName(CurrentItem.ID);
                Sprite _sprite = spriteDictionart[CurrentItem.ID];
                // ����ͼ�� => �ض��������˵�
                Info_ItemIcon.sprite = _sprite;
                // ���µ�������: Amount
                Info_ItemWeight.text = ItemManager.Instance.GetWeight(CurrentItem.ID).ToString();

                // ���������ı�: ItemDescription
                Info_ItemDescription.text = ItemManager.Instance.GetItemDescription(CurrentItem.ID);
                // ǿ�Ƹ��²��� => �����ı��߶� -> ���ڸ��¸�����ĸ߶ȣ�����UI��ʾ
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_ItemDescription.GetComponent<RectTransform>());
                // ���¸�����ĸ߶�
                Info_ItemDescription.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(Info_ItemDescription.transform.parent.GetComponent<RectTransform>().sizeDelta.x, Info_ItemDescription.GetComponent<RectTransform>().sizeDelta.y);

                Info_ItemEffectDescription.text = "<color=orange>" + ItemManager.Instance.GetEffectDescription(CurrentItem.ID) + "</color>";
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
                KT_Use.gameObject.SetActive(ItemManager.Instance.GetCanUse(this.CurrentItem.ID));
                KT_Destroy.gameObject.SetActive(ItemManager.Instance.GetCanDestroy(this.CurrentItem.ID));
            }
            #endregion


        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct InventoryPanel
        {
            public TextContent toptitle;
            public KeyTip LastTerm;
            public KeyTip NextTerm;
            public KeyTip Use;
            public KeyTip Back;
            public KeyTip Destroy;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/Inventory";
            this.abname = "InventoryPanel";
            this.description = "InventoryPanel���ݼ������";
        }

        protected override void InitObjectPool()
        {
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Texture2D, "Texture2DPool", 1,
            "SA_Inventory_UI_UIPanel", (handle) =>
            {
                inventoryAtlas = handle.Result as SpriteAtlas;
            }
            );
            base.InitObjectPool();
        }
        #endregion



    }

}
