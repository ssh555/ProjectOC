using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ML.Engine.Extension;
using ML.Engine.Timer;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Manager;
using ProjectOC.WorkerEchoNS;
using ProjectOC.WorkerNS;
using ML.Engine.UI;
using UnityEngine.U2D;
using ML.Engine.Input;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

namespace ProjectOC.InventorySystem.UI
{
    public class UIInfiniteInventory : ML.Engine.UI.UIBasePanel
    {

        #region Input

        #endregion

        #region Unity
        public bool IsInit = false;
        public SpriteAtlas inventoryAtlas;
        private void Awake()
        {
            InitUITextContents();
            LoadInventoryAtlas();
            //KeyTips
            UIKeyTipComponents = this.transform.GetComponentsInChildren<UIKeyTipComponent>(true);
            foreach (var item in UIKeyTipComponents)
            {
                item.InitData();
                uiKeyTipDic.Add(item.InputActionName, item);
            }




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
            CurrentItemTypeIndex = 0;

            var kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_Use = kt.Find("KT_Use");
            KT_Drop = kt.Find("KT_Drop");
            KT_Destroy = kt.Find("KT_Destroy");

            IsInit = true;
            Refresh();
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

        #endregion

        #region Internal
        /// <summary>
        /// ��Ӧ���߼�����
        /// </summary>
        public ML.Engine.InventorySystem.InfiniteInventory inventory;

        /// <summary>
        /// UI��ʾ��ItemTypeö��
        /// </summary>
        private ItemType[] ItemTypes;
        /// <summary>
        /// ��ǰѡ�е�ItemTypes��Index
        /// </summary>
        private int _currentItemTypeIndex = 0;
        /// <summary>
        /// ��ǰѡ�е�ItemType
        /// </summary>
        private ItemType CurrentItemType => ItemTypes[CurrentItemTypeIndex];
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
                SelectedItems.AddRange(inventory.GetItemList().Where(item => ItemManager.Instance.GetItemType(item.ID) == CurrentItemType));
                CurrentItemIndex = 0;
            }
        }
        /// <summary>
        /// UIPanel.Inventory������ƴ�����Item�б�(����ѡ�е�ItemTypeɸѡ������)
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
                if(last != _currentItemIndex)
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
                if(CurrentItemIndex < SelectedItems.Count)
                {
                    return SelectedItems[CurrentItemIndex];
                }
                return null;
            }
        }

        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Enable();
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(0);
            UikeyTipIsInit = false;
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
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed -= LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed -= NextTerm_performed;
            // �л�Item
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.started -= AlterItem_started;
            //ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.performed -= AlterItem_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.canceled -= AlterItem_canceled;
            // ʹ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Comfirm_performed;
            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            // ����
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Drop.started -= Drop_started;
            // ����
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.started -= Destroy_started;
        }

        private void RegisterInput()
        {
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed += LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed += NextTerm_performed;
            // �л�Item
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.started += AlterItem_started;
            //ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.performed += AlterItem_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.canceled += AlterItem_canceled;
            // ʹ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Comfirm_performed;
            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            // ����
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Drop.started += Drop_started;

            // ����
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Destroy.started += Destroy_started;

        }

        public void DropItem()
        {
            var item = this.CurrentItem;
            this.inventory.RemoveItem(item);
            SelectedItems.Remove(item);
            this.CurrentItemIndex = this.CurrentItemIndex;
            // ��Item����Ϊ��������
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
        private void Drop_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            DropItem();
        }

        /// <summary>
        /// Destroy
        /// </summary>
        /// <param name="obj"></param>
        private void Destroy_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            DestroyItem();
        }

        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private Dictionary<ML.Engine.InventorySystem.ItemType, GameObject> tempItemType = new Dictionary<ML.Engine.InventorySystem.ItemType, GameObject>();
        private List<GameObject> tempUIItems = new List<GameObject>();

        private Dictionary<string, UIKeyTipComponent> uiKeyTipDic = new Dictionary<string, UIKeyTipComponent>();
        private bool UikeyTipIsInit;
        private InputManager inputManager => GameManager.Instance.InputManager;
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

            uiKeyTipDic = null;
        }

        #endregion

        #region UI��������

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
            // �������JSON���� & ��������������
            if(ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
            }


            if (UikeyTipIsInit == false)
            {
                KeyTip[] keyTips = inputManager.ExportKeyTipValues(PanelTextContent_Main);
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

            #region TopTitle
            // ���±����ı�
            this.TopTitleText.text = PanelTextContent_Main.toptitle.GetText();
            #endregion

            #region ItemType
            // ˢ��ItemTypeѡ������
            foreach(var itemtype in ItemTypes)
            {
                // ��Ӧ��ItemType��ʾ���󲻴��ڣ���ʵ��������
                if(!tempItemType.TryGetValue(itemtype, out var obj))
                {
                    // ʵ����
                    obj = Instantiate(ItemTypeTemplate.gameObject, ItemTypeTemplate.parent, false);
                    // ģ����false,��Ҫ����Ϊtrue
                    obj.SetActive(true);
                    // ������ʱ�ڴ����?
                    tempItemType.Add(itemtype, obj);
                    // ����ItemType��Ӧ��Texture2D
                    var sprite = inventoryAtlas.GetSprite(itemtype.ToString());
                    //var tex = ab.LoadAsset<Texture2D>(itemtype.ToString());
                    // ����Sprite��������ʱ�ڴ����?
                    if(sprite != null)
                    {
                        obj.transform.Find("Image").GetComponent<Image>().sprite = sprite;
                        tempSprite.Add(sprite);
                    }
                }
                
                // ˢ����ʾ�ı�
                obj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent_Main.itemtype.FirstOrDefault(it => it.name == itemtype.ToString()).GetDescription();
                // ���� Selected
                var selected = obj.transform.Find("Selected").gameObject;
                selected.SetActive(CurrentItemType == itemtype);
            }

            // ǿ��ˢ��Layout����
            // Unity �ײ㲻���Զ����£���Ҫ�ֶ�����
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.ItemTypeTemplate.parent.GetComponent<RectTransform>());
            #endregion

            #region Inventory
            // ��ʱ�ڴ����ɵ�UIItem����(ֻ��������������ص�����?) - ��ǰɸѡ������Item����
            int delta = tempUIItems.Count - SelectedItems.Count;
            // > 0 => �ж��࣬����
            if(delta > 0)
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
                var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(SelectedItems[i].ID));
                // ������������
                if(sprite == null)
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

            #region ���»�������
            if(cur != null && last != null)
            {
                // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
                RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                // ��ȡ ScrollRect ���?
                ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                // ��ȡ Content �� RectTransform ���?
                RectTransform contentRect = scrollRect.content;

                // ��ȡ UI Ԫ�ص��ĸ��ǵ�
                Vector3[] corners = new Vector3[4];
                uiRectTransform.GetWorldCorners(corners);
                bool allCornersVisible = true;
                for (int i = 0; i < 4; ++i)
                {
                    // ������ռ�ĵ�ת��Ϊ��Ļ�ռ�ĵ�?
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

                    // �����? B ����ڵ�? A ��ƫ����
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
            if(CurrentItem != null)
            {
                // ��ʾItemInfo
                this.transform.Find("ItemInfo").gameObject.SetActive(true);
                // ����Item����
                Info_ItemName.text = ItemManager.Instance.GetItemName(CurrentItem.ID);
                // ����ͼ�� => �ض��������˵�
                Info_ItemIcon.sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(CurrentItem.ID));
                // ���µ�������: JsonText + Amount
                Info_ItemWeight.text = PanelTextContent_Main.weightprefix + ItemManager.Instance.GetWeight(CurrentItem.ID);

                // ���������ı�: JsonText + ItemDescription
                Info_ItemDescription.text = PanelTextContent_Main.descriptionprefix + "\n" + ItemManager.Instance.GetItemDescription(CurrentItem.ID);
                // ǿ�Ƹ��²��� => �����ı��߶� -> ���ڸ��¸�����ĸ߶ȣ�����UI��ʾ
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_ItemDescription.GetComponent<RectTransform>());
                // ���¸�����ĸ߶�?
                Info_ItemDescription.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(Info_ItemDescription.transform.parent.GetComponent<RectTransform>().sizeDelta.x, Info_ItemDescription.GetComponent<RectTransform>().sizeDelta.y);


                Info_ItemEffectDescription.text = PanelTextContent_Main.effectdescriptionprefix + "\n" + ItemManager.Instance.GetEffectDescription(CurrentItem.ID);
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

        public InventoryPanel PanelTextContent_Main => ABJAProcessor.Datas;
        public ML.Engine.ABResources.ABJsonAssetProcessor<InventoryPanel> ABJAProcessor;

        private void InitUITextContents()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<InventoryPanel>("OC/Json/TextContent/Inventory", "InventoryPanel", (datas) =>
            {
                Refresh();
                this.enabled = false;
            }, "UI����Panel����");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion

        private void LoadInventoryAtlas()
        {
            GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>("OC/UI/Inventory/Texture/SA_Inventory_UI.spriteatlasv2").Completed += (handle) =>
            {
                inventoryAtlas = handle.Result as SpriteAtlas;
            };
        }

    }

}
