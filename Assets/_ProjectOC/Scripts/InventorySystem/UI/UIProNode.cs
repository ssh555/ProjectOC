using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectOC.InventorySystem.UI
{
    public class UIProNode : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();
            // TopTitle
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Upgrade = new UIKeyTip();
            KT_Upgrade.img = transform.Find("TopTitle").Find("KT_Upgrade").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_Upgrade.keytip = KT_Upgrade.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Upgrade.description = KT_Upgrade.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();
            Transform priority = transform.Find("TopTitle").Find("Priority");
            Text_Priority = priority.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            KT_NextPriority = new UIKeyTip();
            KT_NextPriority.img = priority.Find("KT_NextPriority").GetComponent<UnityEngine.UI.Image>();
            KT_NextPriority.keytip = KT_NextPriority.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            PriorityUrgency = priority.Find("Urgency");
            PriorityNormal = priority.Find("Normal");
            PriorityAlternative = priority.Find("Alternative");
            // ProNode
            Product = transform.Find("ProNode").Find("Recipe").Find("Product");
            RawGridLayout = transform.Find("ProNode").Find("Recipe").Find("Raw").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            UIItemTemplate = RawGridLayout.transform.Find("UIItemTemplate");
            UIItemTemplate.gameObject.SetActive(false);
            UIWorker = transform.Find("ProNode").Find("Worker");
            // BotKeyTips
            Transform kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_ChangeRecipe = new UIKeyTip();
            KT_ChangeRecipe.img = kt.Find("KT_ChangeRecipe").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_ChangeRecipe.keytip = KT_ChangeRecipe.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ChangeRecipe.description = KT_ChangeRecipe.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Remove1 = new UIKeyTip();
            KT_Remove1.img = kt.Find("KT_Remove1").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_Remove1.keytip = KT_Remove1.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Remove1.description = KT_Remove1.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Remove10 = new UIKeyTip();
            KT_Remove10.img = kt.Find("KT_Remove10").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_Remove10.keytip = KT_Remove10.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Remove10.description = KT_Remove10.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_FastAdd = new UIKeyTip();
            KT_FastAdd.img = kt.Find("KT_FastAdd").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_FastAdd.keytip = KT_FastAdd.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_FastAdd.description = KT_FastAdd.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Back = new UIKeyTip();
            KT_Back.img = kt.Find("KT_Back").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_Back.keytip = KT_Back.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Back.description = KT_Back.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_ChangeWorker = new UIKeyTip();
            KT_ChangeWorker.img = kt.Find("KT_ChangeWorker").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_ChangeWorker.keytip = KT_ChangeWorker.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ChangeWorker.description = KT_ChangeWorker.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_RemoveWorker = new UIKeyTip();
            KT_RemoveWorker.img = kt.Find("KT_RemoveWorker").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_RemoveWorker.keytip = KT_RemoveWorker.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_RemoveWorker.description = KT_RemoveWorker.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            CurPriority = MissionNS.TransportPriority.Normal;
            ProNode.OnActionChange += RefreshDynamic;
            ProNode.OnProduceTimerUpdate += (double time) =>
            {
                RectTransform rect = Product.transform.Find("Mask").GetComponent<RectTransform>();
                float percent = 1 - (float) (100 * time / ProNode.TimeCost);
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, percent * Product.transform.Find("Icon").GetComponent<RectTransform>().sizeDelta.y);
            };
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
        public Worker Worker => ProNode.Worker;
        /// <summary>
        /// ��Ӧ�������ڵ�
        /// </summary>
        public ProNodeNS.ProNode ProNode;
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
                        Text_Priority.text = PanelTextContent.textUrgency.GetText();
                        break;
                    case MissionNS.TransportPriority.Normal:
                        Priority = PriorityNormal;
                        Text_Priority.text = PanelTextContent.textNormal.GetText();
                        break;
                    case MissionNS.TransportPriority.Alternative:
                        Priority = PriorityAlternative;
                        Text_Priority.text = PanelTextContent.textAlternative.GetText();
                        break;
                }
                Priority.Find("Selected").gameObject.SetActive(true);
            }
        }
        [ShowInInspector]
        private List<string> RawItems = new List<string>();
        /// <summary>
        /// ��һ��ѡ�е�ItemIndex�������ƶ���������
        /// </summary>
        private int lastItemIndex = 0;
        /// <summary>
        /// ��ǰѡ�е�ItemIndex
        /// </summary>
        private int currentItemIndex = 0;
        /// <summary>
        /// ��װ������������ݺ�Refresh
        /// </summary>
        private int CurrentItemIndex
        {
            get => currentItemIndex;
            set
            {
                int last = currentItemIndex;
                if(RawItems.Count > 0)
                {
                    currentItemIndex = value;
                    if(currentItemIndex == -1)
                    {
                        currentItemIndex = RawItems.Count - 1;
                    }
                    else if(currentItemIndex == RawItems.Count)
                    {
                        currentItemIndex = 0;
                    }
                    else
                    {
                        var grid = RawGridLayout.GetGridSize();
                        if (currentItemIndex < 0)
                        {
                            currentItemIndex += (grid.x * grid.y);
                        }
                        else if (currentItemIndex >= RawItems.Count)
                        {
                            currentItemIndex -= (grid.x * grid.y);
                            if (currentItemIndex < 0)
                            {
                                currentItemIndex += grid.y;
                            }
                        }
                        // ���������ص�ģ��
                        while (this.currentItemIndex >= RawItems.Count)
                        {
                            this.currentItemIndex -= grid.y;
                        }
                    }
                }
                else
                {
                    currentItemIndex = 0;
                }
                if(last != currentItemIndex)
                {
                    lastItemIndex = last;
                }
                this.Refresh();
            }
        }
        /// <summary>
        /// ��ǰѡ�е�Item
        /// </summary>
        private string CurrentRawItem
        {
            get
            {
                if(CurrentItemIndex < RawItems.Count)
                {
                    return RawItems[CurrentItemIndex];
                }
                return null;
            }
        }
        private Player.PlayerCharacter Player => GameObject.Find("PlayerCharacter")?.GetComponent<Player.PlayerCharacter>();

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
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Upgrade.performed -= Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed -= NextPriority_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed -= ChangeRecipe_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove1.performed -= Remove1_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove10.performed -= Remove10_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed -= FastAdd_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.ChangeWorker.performed -= ChangeWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.RemoveWorker.performed -= RemoveWorker_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.AlterRawItem.performed -= AlterItem_performed;
        }

        private void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Upgrade.performed += Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed += NextPriority_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed += ChangeRecipe_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove1.performed += Remove1_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove10.performed += Remove10_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed += FastAdd_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.ChangeWorker.performed += ChangeWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.RemoveWorker.performed += RemoveWorker_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.AlterRawItem.performed += AlterItem_performed;
        }
        private void Upgrade_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
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

        private void ChangeRecipe_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            // �л�UI Panel
        }
        private void Remove1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ProNode.UIRemove(Player, 1);
            Refresh();
        }
        private void Remove10_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ProNode.UIRemove(Player, 10);
            Refresh();
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ProNode.UIFastAdd(Player, CurrentRawItem);
            Refresh();
        }
        private void ChangeWorker_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            // �л�UI Panel
        }
        private void RemoveWorker_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ProNode.RemoveWorker();
            Refresh();
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }
        private void AlterItem_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var f_offset = obj.ReadValue<Vector2>();
            var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
            var grid = RawGridLayout.GetGridSize();
            this.CurrentItemIndex += -offset.y * grid.y + offset.x;
        }
        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private List<GameObject> tempUIItems = new List<GameObject>();

        private void ClearTemp()
        {
            foreach(var s in tempSprite)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItems)
            {
                Destroy(s);
            }
        }
        #endregion

        #region UI��������
        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;

        private UIKeyTip KT_Upgrade;
        private UIKeyTip KT_NextPriority;
        private UIKeyTip KT_ChangeRecipe;
        private UIKeyTip KT_Remove1;
        private UIKeyTip KT_Remove10;
        private UIKeyTip KT_FastAdd;
        private UIKeyTip KT_Back;
        private UIKeyTip KT_ChangeWorker;
        private UIKeyTip KT_RemoveWorker;

        private Transform Product;
        private GridLayoutGroup RawGridLayout;
        private Transform UIItemTemplate;
        private Transform UIWorker;
        private Transform Priority;
        private Transform PriorityUrgency;
        private Transform PriorityNormal;
        private Transform PriorityAlternative;
        #endregion
        
        public void Refresh()
        {
            // �������JSON���� & ��������������
            if(ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
            }

            #region TopTitle
            //Text_Title.text = PanelTextContent.textTitle.GetText();
            KT_Upgrade.ReWrite(PanelTextContent.ktUpgrade);
            KT_NextPriority.ReWrite(PanelTextContent.ktNextPriority);
            #endregion

            #region ProNode
            if (ProNode.Recipe != null)
            {
                #region Product
                string productID = ProNode.Recipe.GetProductID();
                var nameProduct = Product.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                nameProduct.text = ItemManager.Instance.GetItemName(productID); 
                var imgProduct = Product.transform.Find("Icon").GetComponent<Image>();
                var spriteProduct = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(productID));
                if (spriteProduct == null)
                {
                    spriteProduct = ItemManager.Instance.GetItemSprite(productID);
                    tempSprite.Add(spriteProduct);
                }
                imgProduct.sprite = spriteProduct;
                var amountProduct = Product.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                amountProduct.text = ProNode.GetItemAllNum(productID).ToString();
                #endregion
                #region Raw
                // ��ʱ�ڴ����ɵ�UIItem����(ֻ��������������ص�����) - ��ǰɸѡ������Item����
                int delta = tempUIItems.Count - RawItems.Count;
                // > 0 => �ж��࣬����
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
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
                        var uiitem = Instantiate(UIItemTemplate, RawGridLayout.transform, false);
                        tempUIItems.Add(uiitem.gameObject);
                    }
                }
                // ���ڸ��»�������
                // ��ǰѡ�е�UIItem
                GameObject cur = null;
                // ��һ��UIItem
                GameObject last = null;
                // ����ɸѡ��ItemList
                for (int i = 0; i < RawItems.Count; ++i)
                {
                    var itemID = RawItems[i];
                    var item = tempUIItems[i];
                    // Active
                    item.SetActive(true);
                    // Name
                    var name = item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    name.text = ItemManager.Instance.GetItemName(itemID);
                    // ����Icon
                    var img = item.transform.Find("Icon").GetComponent<Image>();
                    // ������ʱ�洢��Sprite
                    var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(itemID));
                    // ������������
                    if (sprite == null)
                    {
                        sprite = ItemManager.Instance.GetItemSprite(itemID);
                        tempSprite.Add(sprite);
                    }
                    img.sprite = sprite;
                    // Amount
                    var amount = item.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    amount.text = ProNode.GetItemAllNum(itemID).ToString();
                    // NeedAmount
                    var needAmount = item.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                    needAmount.text = ProNode.Recipe.GetRawNum(itemID).ToString();
                    // Selected
                    var selected = item.transform.Find("Selected");
                    if (CurrentRawItem == itemID)
                    {
                        selected.gameObject.SetActive(true);
                        cur = item;
                    }
                    else
                    {
                        selected.gameObject.SetActive(false);
                    }
                    if (i == lastItemIndex)
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
                    RawGridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                }
                #endregion
                // ǿ���������� GridLayoutGroup �Ĳ���
                LayoutRebuilder.ForceRebuildLayoutImmediate(RawGridLayout.GetComponent<RectTransform>());
                #endregion
            }
            #region Worker
            if (Worker != null)
            {
                var name = UIWorker.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                name.text = Worker.Name;
                var img = UIWorker.transform.Find("Icon").GetComponent<Image>();
                WorkerManager workerManager = ManagerNS.LocalGameManager.Instance.WorkerManager;
                var sprite = tempSprite.Find(s => s.texture == workerManager.GetTexture2D());
                if (sprite == null)
                {
                    sprite = workerManager.GetSprite();
                    tempSprite.Add(sprite);
                }
                img.sprite = sprite;
                var onDuty = UIWorker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();
                // TODO:֮���޸�
                onDuty.text = Worker.IsOnDuty ? "ֵ����":"������";
                var efficiency = UIWorker.transform.Find("Efficiency").GetComponent<TMPro.TextMeshProUGUI>();
                efficiency.text = ProNode.Eff.ToString() + "%";
                var effects = UIWorker.transform.Find("Effect").GetComponent<TMPro.TextMeshProUGUI>();
                effects.text = "";
                foreach (Feature feature in Worker.Features)
                {
                    effects.text += feature.EffectsDescription.ToString() + "\n";
                }
                var rect = UIWorker.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                rect.offsetMax = new Vector2(rect.offsetMax.x, (int)(100 * Worker.APCurrent / Worker.APMax));
            }
            #endregion
            #endregion

            #region BotKeyTips
            KT_ChangeRecipe.ReWrite(PanelTextContent.ktChangeRecipe);
            KT_Remove1.ReWrite(PanelTextContent.ktRemove1);
            KT_Remove10.ReWrite(PanelTextContent.ktRemove10);
            KT_FastAdd.ReWrite(PanelTextContent.ktFastAdd);
            KT_Back.ReWrite(PanelTextContent.ktBack);
            KT_ChangeWorker.ReWrite(PanelTextContent.ktChangeWorker);
            KT_RemoveWorker.ReWrite(PanelTextContent.ktRemoveWorker);
            #endregion
        }

        public void RefreshDynamic()
        {
            if (ProNode.Recipe != null)
            {
                var amountProduct = Product.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                amountProduct.text = ProNode.GetItemAllNum(ProNode.Recipe.GetProductID()).ToString();
                for (int i = 0; i < RawItems.Count; ++i)
                {
                    var amount = tempUIItems[i].transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    amount.text = ProNode.GetItemAllNum(RawItems[i]).ToString();
                }
            }
            if (Worker != null)
            {
                var onDuty = UIWorker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();
                // TODO:֮���޸�
                onDuty.text = Worker.IsOnDuty ? "ֵ����" : "������";
                var efficiency = UIWorker.transform.Find("Efficiency").GetComponent<TMPro.TextMeshProUGUI>();
                efficiency.text = ProNode.Eff.ToString() + "%";
                var effects = UIWorker.transform.Find("Effect").GetComponent<TMPro.TextMeshProUGUI>();
                effects.text = "";
                foreach (Feature feature in Worker.Features)
                {
                    effects.text += feature.EffectsDescription.ToString() + "\n";
                }
                var rect = UIWorker.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                rect.offsetMax = new Vector2(rect.offsetMax.x, (int)(100 * Worker.APCurrent / Worker.APMax));
            }
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct ProNodePanel
        {
            public TextTip[] proNodeType;
            public TextContent textEmpty;
            public TextContent textUrgency;
            public TextContent textNormal;
            public TextContent textAlternative;
            public TextContent textStateVacancy;
            public TextContent textStateStagnation;
            public TextContent textStateProduction;
            public TextContent textWorkerStateWork;
            public TextContent textWorkerStateTransport;
            public TextContent textWorkerStateFish;
            public TextContent textWorkerStateRelax;
            public TextContent textPrefixEff;

            public KeyTip ktUpgrade;
            public KeyTip ktNextPriority;
            public KeyTip ktChangeRecipe;
            public KeyTip ktRemove1;
            public KeyTip ktRemove10;
            public KeyTip ktFastAdd;
            public KeyTip ktBack;
            public KeyTip ktChangeWorker;
            public KeyTip ktRemoveWorker;
            public KeyTip ktConfirmRecipe;
            public KeyTip ktBackRecipe;
            public KeyTip ktConfirmWorker;
            public KeyTip ktBackWorker;
            public KeyTip ktConfirmLevel;
            public KeyTip ktBackLevel;
        }

        public static ProNodePanel PanelTextContent => ABJAProcessor.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<ProNodePanel> ABJAProcessor;

        private void InitUITextContents()
        {
            if(ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ProNodePanel>("Binary/TextContent/ProNode", "ProNodePanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI�����ڵ�Panel����");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion
    }
}
