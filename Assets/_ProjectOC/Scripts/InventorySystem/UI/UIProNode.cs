using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.TextContent;
using ProjectOC.ManagerNS;
using ProjectOC.ProNodeNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ML.Engine.Extension;
using ML.Engine.BuildingSystem;
using ML.Engine.UI;
using ML.Engine.Manager;
using ML.Engine.Input;
using static ProjectOC.InventorySystem.UI.UIProNode;
using System;
using ProjectOC.MissionNS;
using UnityEditor;


namespace ProjectOC.InventorySystem.UI
{
    public class UIProNode : ML.Engine.UI.UIBasePanel<ProNodePanel>
    {
        #region Unity
        public bool IsInit = false;
        protected override void Awake()
        {
            base.Awake();

            // TopTitle
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            Transform priority = transform.Find("TopTitle").Find("Priority");
            Text_Priority = priority.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            PriorityUrgency = priority.Find("Urgency");
            PriorityNormal = priority.Find("Normal");
            PriorityAlternative = priority.Find("Alternative");
            // ProNode
            ProNodeUI = transform.Find("ProNode");
            // ProNode Recipe
            Transform recipe = transform.Find("ProNode").Find("Recipe");
            Product = recipe.Find("Product");
            Raw_GridLayout = recipe.Find("Raw").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Raw_UIItemTemplate = Raw_GridLayout.transform.Find("UIItemTemplate");
            Raw_UIItemTemplate.gameObject.SetActive(false);
            // ProNode Worker
            UIWorker = transform.Find("ProNode").Find("Worker");
            // ProNode Eff
            UIWorkerEff = transform.Find("ProNode").Find("Eff");
            Text_Eff = UIWorkerEff.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>();
            Text_EffProNode = UIWorkerEff.Find("EffProNode").GetComponent<TMPro.TextMeshProUGUI>();
            Text_EffWorker = UIWorkerEff.Find("EffWorker").GetComponent<TMPro.TextMeshProUGUI>();
            // ChangeRecipe
            ChangeRecipe = transform.Find("ChangeRecipe");
            Recipe_GridLayout = ChangeRecipe.Find("Select").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Recipe_UIItemTemplate = Recipe_GridLayout.transform.Find("UIItemTemplate");
            Recipe_Raw_GridLayout = ChangeRecipe.Find("Recipe").Find("Raw").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Recipe_Raw_UIItemTemplate = Recipe_Raw_GridLayout.transform.Find("UIItemTemplate");
            Recipe_Product = ChangeRecipe.Find("Recipe").Find("Product");

            Recipe_UIItemTemplate.gameObject.SetActive(false);
            ChangeRecipe.gameObject.SetActive(false);
            // ChangeWorker
            ChangeWorker = transform.Find("ChangeWorker");
            Worker_GridLayout = ChangeWorker.Find("Select").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Worker_UIItemTemplate = Worker_GridLayout.transform.Find("UIItemTemplate");
            Worker_UIItemTemplate.gameObject.SetActive(false);
            ChangeWorker.gameObject.SetActive(false);
            // LevelUp
            ChangeLevel = transform.Find("Upgrade");
            Transform contentUpgrade = ChangeLevel.Find("Raw").Find("Viewport").Find("Content");
            Level_GridLayout = contentUpgrade.GetComponent<GridLayoutGroup>();
            Level_UIItemTemplate = contentUpgrade.Find("UIItemTemplate");
            Level_UIItemTemplate.gameObject.SetActive(false);
            Transform level = transform.Find("Upgrade").Find("Level");
            LvOld = level.Find("LvOld").GetComponent<TMPro.TextMeshProUGUI>();
            LvNew = level.Find("LvNew").GetComponent<TMPro.TextMeshProUGUI>();
            DescOld = level.Find("DescOld").GetComponent<TMPro.TextMeshProUGUI>();
            DescNew = level.Find("DescNew").GetComponent<TMPro.TextMeshProUGUI>();
            Level_Build = ChangeLevel.Find("Raw").Find("Build");
            ChangeLevel.gameObject.SetActive(false);

            // BotKeyTips
            BotKeyTips_ProNode = this.transform.Find("BotKeyTips").Find("KeyTips");


            // BotKeyTips ChangeRecipe
            BotKeyTips_Recipe = this.transform.Find("BotKeyTips").Find("ChangeRecipe");

            BotKeyTips_Recipe.gameObject.SetActive(false);
            // BotKeyTips ChangeWorker
            BotKeyTips_Worker = this.transform.Find("BotKeyTips").Find("ChangeWorker");

            BotKeyTips_Worker.gameObject.SetActive(false);
            // BotKeyTips LevelUp
            BotKeyTips_Level = this.transform.Find("BotKeyTips").Find("Upgrade");

            BotKeyTips_Level.gameObject.SetActive(false);


            IsInit = true;

        }

        protected override void Start()
        {
            base.Start();
        }

        #endregion

        #region Override
        protected override void Enter()
        {
            ProNode.OnActionChange += RefreshDynamic;
            ProNode.OnProduceTimerUpdate += OnProduceTimerUpdateAction;
            ProNode.OnProduceEnd += Refresh;
            base.Enter();
        }

        protected override void Exit()
        {
            ProNode.OnActionChange -= RefreshDynamic;
            ProNode.OnProduceTimerUpdate -= OnProduceTimerUpdateAction;
            ProNode.OnProduceEnd -= Refresh;
            ClearTemp();
            base.Exit();
        }
        #endregion

        #region Internal
        public enum Mode
        {
            ProNode = 0,
            ChangeRecipe = 1,
            ChangeWorker = 2,
            ChangeLevel = 3,
        }
        public Mode CurMode = Mode.ProNode;
        public Worker Worker => ProNode.Worker;
        /// <summary>
        /// 对应的生产节点
        /// </summary>
        public ProNodeNS.ProNode ProNode;
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

                Text_Priority.text = PanelTextContent.TransportPriority[(int)curPriority];
                Priority = transform.Find("TopTitle").Find("Priority").GetChild((int)curPriority);
                Priority.Find("Selected").gameObject.SetActive(true);
            }
        }
        #region ProNode Raw
        private List<Formula> Raws => ProNode.Recipe?.Raw;
        /// <summary>
        /// 上一次选中的ItemIndex，用于移动滑动窗口
        /// </summary>
        private int lastRawIndex = 0;
        /// <summary>
        /// 当前选中的ItemIndex
        /// </summary>
        private int currentRawIndex = 0;
        /// <summary>
        /// 封装，方便更新数据和Refresh
        /// </summary>
        private int CurrentRawIndex
        {
            get => currentRawIndex;
            set
            {
                int last = currentRawIndex;
                if (Raws != null && Raws.Count > 0)
                {
                    currentRawIndex = value;
                    if (currentRawIndex == -1)
                    {
                        currentRawIndex = Raws.Count - 1;
                    }
                    else if (currentRawIndex == Raws.Count)
                    {
                        currentRawIndex = 0;
                    }
                    else
                    {
                        var grid = Raw_GridLayout.GetGridSize();
                        if (currentRawIndex < 0)
                        {
                            currentRawIndex += (grid.x * grid.y);
                        }
                        else if (currentRawIndex >= Raws.Count)
                        {
                            currentRawIndex -= (grid.x * grid.y);
                            if (currentRawIndex < 0)
                            {
                                currentRawIndex += grid.y;
                            }
                        }
                        // 不计算隐藏的模板
                        while (this.currentRawIndex >= Raws.Count)
                        {
                            this.currentRawIndex -= grid.y;
                        }
                    }
                }
                else
                {
                    currentRawIndex = 0;
                }
                if (last != currentRawIndex)
                {
                    lastRawIndex = last;
                }
                this.Refresh();
            }
        }
        /// <summary>
        /// 当前选中的Item
        /// </summary>
        private string CurrentRaw
        {
            get
            {
                if (Raws != null && CurrentRawIndex < Raws.Count)
                {
                    return Raws[CurrentRawIndex].id;
                }
                return null;
            }
        }
        #endregion

        #region ChangeRecipe
        [ShowInInspector]
        private List<string> Recipes = new List<string>();
        private int lastRecipeIndex = 0;
        private int currentRecipeIndex = 0;
        private int CurrentRecipeIndex
        {
            get => currentRecipeIndex;
            set
            {
                int last = currentRecipeIndex;
                if (Recipes.Count > 0)
                {
                    currentRecipeIndex = value;
                    if (currentRecipeIndex == -1)
                    {
                        currentRecipeIndex = Recipes.Count - 1;
                    }
                    else if (currentRecipeIndex == Recipes.Count)
                    {
                        currentRecipeIndex = 0;
                    }
                    else
                    {
                        var grid = Recipe_GridLayout.GetGridSize();
                        if (currentRecipeIndex < 0)
                        {
                            currentRecipeIndex += (grid.x * grid.y);
                        }
                        else if (currentRecipeIndex >= Recipes.Count)
                        {
                            currentRecipeIndex -= (grid.x * grid.y);
                            if (currentRecipeIndex < 0)
                            {
                                currentRecipeIndex += grid.y;
                            }
                        }
                        // 不计算隐藏的模板
                        while (this.currentRecipeIndex >= Recipes.Count)
                        {
                            this.currentRecipeIndex -= grid.y;
                        }
                    }
                }
                else
                {
                    currentRecipeIndex = 0;
                }
                if (last != currentRecipeIndex)
                {
                    lastRecipeIndex = last;
                }
                this.Refresh();
            }
        }
        private string CurrentRecipe
        {
            get
            {
                if (CurrentRecipeIndex < Recipes.Count)
                {
                    return Recipes[CurrentRecipeIndex];
                }
                return null;
            }
        }
        #endregion

        #region ChangeWorker
        [ShowInInspector]
        private List<Worker> Workers = new List<Worker>();
        private int lastWorkerIndex = 0;
        private int currentWorkerIndex = 0;
        private int CurrentWorkerIndex
        {
            get => currentWorkerIndex;
            set
            {
                int last = currentWorkerIndex;
                if (Workers.Count > 0)
                {
                    currentWorkerIndex = value;
                    if (currentWorkerIndex == -1)
                    {
                        currentWorkerIndex = Workers.Count - 1;
                    }
                    else if (currentWorkerIndex == Workers.Count)
                    {
                        currentWorkerIndex = 0;
                    }
                    else
                    {
                        var grid = Worker_GridLayout.GetGridSize();
                        if (currentWorkerIndex < 0)
                        {
                            currentWorkerIndex += (grid.x * grid.y);
                        }
                        else if (currentWorkerIndex >= Workers.Count)
                        {
                            currentWorkerIndex -= (grid.x * grid.y);
                            if (currentWorkerIndex < 0)
                            {
                                currentWorkerIndex += grid.y;
                            }
                        }
                        // 不计算隐藏的模板
                        while (this.currentWorkerIndex >= Workers.Count)
                        {
                            this.currentWorkerIndex -= grid.y;
                        }
                    }
                }
                else
                {
                    currentWorkerIndex = 0;
                }
                if (last != currentWorkerIndex)
                {
                    lastWorkerIndex = last;
                }
                this.Refresh();
            }
        }
        private Worker CurrentWorker
        {
            get
            {
                if (CurrentWorkerIndex < Workers.Count)
                {
                    return Workers[CurrentWorkerIndex];
                }
                return null;
            }
        }
        #endregion

        public Player.PlayerCharacter Player;

        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Upgrade.performed -= Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed -= NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove1.performed -= Remove1_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove10.performed -= Remove10_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed -= FastAdd_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.ChangeWorker.performed -= ChangeWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.RemoveWorker.performed -= RemoveWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.AlterRawItem.performed -= Alter_performed;
        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Upgrade.performed += Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed += NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove1.performed += Remove1_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove10.performed += Remove10_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed += FastAdd_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.ChangeWorker.performed += ChangeWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.RemoveWorker.performed += RemoveWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.AlterRawItem.performed += Alter_performed;
        }
        private void Upgrade_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                CurMode = Mode.ChangeLevel;
                Refresh();
            }
        }
        private void NextPriority_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            MissionNS.TransportPriority temp = CurPriority;

            CurPriority = (TransportPriority)(((int)CurPriority + 1) % System.Enum.GetValues(typeof(TransportPriority)).Length);
        }

        private void Alter_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                var grid = Raw_GridLayout.GetGridSize();
                this.CurrentRawIndex += -offset.y * grid.y + offset.x;
            }
            else if (CurMode == Mode.ChangeRecipe)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                var grid = Recipe_GridLayout.GetGridSize();
                this.CurrentRecipeIndex += -offset.y * grid.y + offset.x;
            }
            else if (CurMode == Mode.ChangeWorker)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                var grid = Worker_GridLayout.GetGridSize();
                this.CurrentWorkerIndex += -offset.y * grid.y + offset.x;
            }
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                CurMode = Mode.ChangeRecipe;
            }
            else if (CurMode == Mode.ChangeRecipe)
            {
                ProNode.ChangeRecipe(Player, CurrentRecipe);
                if (ItemManager.Instance.IsValidItemID(ProNode.Recipe?.Product.id))
                {
                    var texture = ItemManager.Instance.GetItemTexture2D(ProNode.Recipe.Product.id);
                    if (texture != null)
                    {
                        ItemManager.Instance.AddItemIconObject(ProNode.Recipe.Product.id,
                                                               this.ProNode.WorldProNode.transform,
                                                               new Vector3(0, this.ProNode.WorldProNode.transform.GetComponent<Collider>().bounds.size.y / 2, 0),
                                                               Quaternion.Euler(new Vector3(90, 0, 0)),
                                                               Vector3.one);
                    }
                }
                CurMode = Mode.ProNode;
            }
            else if (CurMode == Mode.ChangeWorker)
            {
                ProNode.ChangeWorker(CurrentWorker);
                CurMode = Mode.ProNode;
            }
            else if (CurMode == Mode.ChangeLevel)
            {
                this.ProNode.Upgrade(Player);
            }
            Refresh();
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                UIMgr.PopPanel();
            }
            else
            {
                CurMode = Mode.ProNode;
                Refresh();
            }
        }

        private void Remove1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                ProNode.UIRemove(Player, 1);
                Refresh();
            }
        }
        private void Remove10_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                ProNode.UIRemove(Player, 10);
                Refresh();
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                ProNode.UIFastAdd(Player, CurrentRaw);
                Refresh();
            }
        }
        private void ChangeWorker_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode && this.ProNode.ProNodeType == ProNodeType.Mannul)
            {
                CurMode = Mode.ChangeWorker;
                Refresh();
            }
        }
        private void RemoveWorker_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode && this.ProNode.ProNodeType == ProNodeType.Mannul)
            {
                ProNode.RemoveWorker();
                Refresh();
            }
        }
        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private List<GameObject> tempUIItems = new List<GameObject>();
        private List<GameObject> tempUIItemsRecipe = new List<GameObject>();
        private List<GameObject> tempUIItemsRecipeRaw = new List<GameObject>();
        private List<GameObject> tempUIItemsWorker = new List<GameObject>();
        private List<GameObject> tempUIItemsLevel = new List<GameObject>();
        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItems)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItemsRecipe)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItemsRecipeRaw)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItemsWorker)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItemsLevel)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
        }
        #endregion

        #region UI对象引用

        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;
        private TMPro.TextMeshProUGUI Text_Eff;
        private TMPro.TextMeshProUGUI Text_EffProNode;
        private TMPro.TextMeshProUGUI Text_EffWorker;
        private TMPro.TextMeshProUGUI LvOld;
        private TMPro.TextMeshProUGUI LvNew;
        private TMPro.TextMeshProUGUI DescOld;
        private TMPro.TextMeshProUGUI DescNew;

        private Transform Priority;
        private Transform PriorityUrgency;
        private Transform PriorityNormal;
        private Transform PriorityAlternative;
        private Transform Product;
        private Transform UIWorker;
        private Transform UIWorkerEff;

        private GridLayoutGroup Raw_GridLayout;
        private Transform Raw_UIItemTemplate;
        private GridLayoutGroup Recipe_GridLayout;
        private Transform Recipe_UIItemTemplate;
        private GridLayoutGroup Recipe_Raw_GridLayout;
        private Transform Recipe_Raw_UIItemTemplate;
        private Transform Recipe_Product;

        private GridLayoutGroup Worker_GridLayout;
        private Transform Worker_UIItemTemplate;
        private GridLayoutGroup Level_GridLayout;
        private Transform Level_UIItemTemplate;
        private Transform Level_Build;

        private Transform ProNodeUI;
        private Transform ChangeRecipe;
        private Transform ChangeWorker;
        private Transform ChangeLevel;
        private Transform BotKeyTips_ProNode;
        private Transform BotKeyTips_Recipe;
        private Transform BotKeyTips_Worker;
        private Transform BotKeyTips_Level;
        #endregion
        public override void Refresh()
        {
            // 加载完成JSON数据 & 查找完所有引用
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }

            if (CurMode == Mode.ProNode)
            {
                this.ProNodeUI.gameObject.SetActive(true);
                this.ChangeRecipe.gameObject.SetActive(false);
                this.ChangeWorker.gameObject.SetActive(false);
                this.ChangeLevel.gameObject.SetActive(false);
                this.BotKeyTips_ProNode.gameObject.SetActive(true);
                this.BotKeyTips_Recipe.gameObject.SetActive(false);
                this.BotKeyTips_Worker.gameObject.SetActive(false);
                this.BotKeyTips_Level.gameObject.SetActive(false);
                this.Raw_UIItemTemplate.gameObject.SetActive(false);

                #region ProNode
                #region TopTitle
                foreach (TextTip tp in PanelTextContent.proNodeType)
                {
                    if (tp.name == ProNode.Category.ToString())
                    {
                        Text_Title.text = tp.GetDescription();
                        break;
                    }
                }
                #endregion

                if (ProNode.Recipe != null)
                {
                    #region Product
                    string productID = ProNode.Recipe.ProductID;
                    var nameProduct = Product.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    nameProduct.text = ItemManager.Instance.GetItemName(productID);
                    var imgProduct = Product.transform.Find("Icon").GetComponent<Image>();
                    if (ItemManager.Instance.IsValidItemID(productID))
                    {
                        var texture = ItemManager.Instance.GetItemTexture2D(productID);
                        if (texture != null)
                        {
                            var spriteProduct = tempSprite.Find(s => s.texture == texture);
                            if (spriteProduct == null)
                            {
                                spriteProduct = ItemManager.Instance.GetItemSprite(productID);
                                tempSprite.Add(spriteProduct);
                            }
                            imgProduct.sprite = spriteProduct;
                        }
                        else
                        {
                            imgProduct.sprite = null;
                        }
                    }
                    else
                    {
                        imgProduct.sprite = null;
                    }
                    var amountProduct = Product.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    amountProduct.text = ProNode.GetItemAllNum(productID).ToString();

                    var tiemProduct = Product.transform.Find("Time").GetComponent<TMPro.TextMeshProUGUI>();
                    tiemProduct.text = "Time: " + ProNode.Recipe.TimeCost.ToString();
                    RectTransform rect = Product.transform.Find("Mask").GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, 0);
                    #endregion

                    #region Raw
                    // 临时内存生成的UIItem数量(只增不减，多的隐藏掉即可) - 当前筛选出来的Item数量
                    int delta = tempUIItems.Count - Raws.Count;
                    // > 0 => 有多余，隐藏
                    if (delta > 0)
                    {
                        for (int i = 0; i < delta; ++i)
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
                            var uiitem = Instantiate(Raw_UIItemTemplate, Raw_GridLayout.transform, false);
                            tempUIItems.Add(uiitem.gameObject);
                        }
                    }
                    // 用于更新滑动窗口
                    // 当前选中的UIItem
                    GameObject cur = null;
                    // 上一个UIItem
                    GameObject last = null;
                    // 遍历筛选的ItemList
                    for (int i = 0; i < Raws.Count; ++i)
                    {
                        var itemID = Raws[i].id;
                        var item = tempUIItems[i];
                        // Active
                        item.SetActive(true);
                        // Name
                        var name = item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                        name.text = ItemManager.Instance.GetItemName(itemID);
                        var img = item.transform.Find("Icon").GetComponent<Image>();
                        if (ItemManager.Instance.IsValidItemID(itemID))
                        {
                            // 更新Icon
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
                            }
                        }
                        else
                        {
                            img.sprite = null;
                        }
                        // Amount
                        var amount = item.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                        amount.text = ProNode.GetItemAllNum(itemID).ToString();
                        // NeedAmount
                        var needAmount = item.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                        needAmount.text = ProNode.Recipe.GetRawNum(itemID).ToString();
                        // Selected
                        var selected = item.transform.Find("Selected");
                        if (CurrentRaw == itemID)
                        {
                            selected.gameObject.SetActive(true);
                            cur = item;
                        }
                        else
                        {
                            selected.gameObject.SetActive(false);
                        }
                        if (i == lastRawIndex)
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
                        Raw_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                    }
                    #endregion
                    // 强制立即更新 GridLayoutGroup 的布局
                    LayoutRebuilder.ForceRebuildLayoutImmediate(Raw_GridLayout.GetComponent<RectTransform>());
                    #endregion
                }
                else
                {
                    Product.transform.Find("Icon").GetComponent<Image>().sprite = null;
                    Product.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
                    Product.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "0";
                    Product.transform.Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = "Time: 0";
                    RectTransform rect = Product.transform.Find("Mask").GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, 0);
                    for (int i = 0; i < tempUIItems.Count; ++i)
                    {
                        tempUIItems[tempUIItems.Count - 1 - i].SetActive(false);
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(Raw_GridLayout.GetComponent<RectTransform>());
                }
                #region Worker
                if (this.ProNode.ProNodeType == ProNodeType.Mannul)
                {
                    UIWorker.gameObject.SetActive(true);
                    if (Worker != null)
                    {
                        var name = UIWorker.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                        name.text = Worker.Name;
                        //var img = UIWorker.transform.Find("Icon").GetComponent<Image>();
                        //WorkerManager workerManager = ManagerNS.LocalGameManager.Instance.WorkerManager;
                        //var sprite = tempSprite.Find(s => s.texture == workerManager.GetTexture2D());
                        //if (sprite == null)
                        //{
                        //    sprite = workerManager.GetSprite();
                        //    tempSprite.Add(sprite);
                        //}
                        //img.sprite = sprite;
                        var onDuty = UIWorker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();

                        onDuty.text = PanelTextContent.workerStatus[(int)Worker.Status];
                        if (Worker.Status == Status.Fishing && Worker.IsOnDuty)
                        {
                            onDuty.text = PanelTextContent.textWorkerOnDuty;
                        }

                        var rect = UIWorker.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                        rect.offsetMax = new Vector2(rect.offsetMax.x, -1 * (int)(100 - 100 * Worker.APCurrent / Worker.APMax));
                        UIWorker.transform.Find("AP").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Worker.APCurrent}/{Worker.APMax}";
                    }
                    else
                    {
                        UIWorker.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
                        UIWorker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
                        RectTransform rect = UIWorker.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                        rect.offsetMax = new Vector2(rect.offsetMax.x, -100);
                        UIWorker.transform.Find("AP").GetComponent<TMPro.TextMeshProUGUI>().text = "0/0";
                    }
                }
                else
                {
                    UIWorker.gameObject.SetActive(false);
                }
                #endregion
                #region Eff
                if (this.ProNode.ProNodeType == ProNodeType.Mannul)
                {
                    UIWorkerEff.gameObject.SetActive(true);
                    Text_Eff.text = PanelTextContent.textPrefixEff + ": +" + ProNode.Eff.ToString() + "%";
                    Text_EffProNode.text = ProNode.Name + ": +" + ProNode.EffBase.ToString() + "%";
                    if (Worker != null)
                    {
                        Text_EffWorker.text = Worker.Name + ": +" + (ProNode.Eff - ProNode.EffBase).ToString() + "%";
                    }
                    else
                    {
                        Text_EffWorker.text = "";
                    }
                }
                else
                {
                    UIWorkerEff.gameObject.SetActive(false);
                }
                #endregion
                #endregion

                #region BotKeyTips
                if (this.ProNode.ProNodeType == ProNodeType.Auto)
                {
                    BotKeyTips_ProNode.Find("KT_ChangeWorker").gameObject.SetActive(false);
                    BotKeyTips_ProNode.Find("KT_RemoveWorker").gameObject.SetActive(false);

                }
                #endregion
            }
            else if (CurMode == Mode.ChangeRecipe)
            {
                this.ChangeRecipe.gameObject.SetActive(true);
                this.BotKeyTips_ProNode.gameObject.SetActive(false);
                this.BotKeyTips_Recipe.gameObject.SetActive(true);
                this.Recipe_UIItemTemplate.gameObject.SetActive(false);
                this.Recipe_Raw_UIItemTemplate.gameObject.SetActive(false);

                Recipes = new List<string>() { "" };
                Recipes.AddRange(ProNode.GetCanProduceRecipe());
                ML.Engine.InventorySystem.CompositeSystem.Formula product;
                int delta;
                #region Select
                // 临时内存生成的UIItem数量(只增不减，多的隐藏掉即可) - 当前筛选出来的Item数量
                delta = tempUIItemsRecipe.Count - Recipes.Count;
                // > 0 => 有多余，隐藏
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemsRecipe[tempUIItemsRecipe.Count - 1 - i].SetActive(false);
                    }
                }
                // < 0 => 不够， 增加
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(Recipe_UIItemTemplate, Recipe_GridLayout.transform, false);
                        tempUIItemsRecipe.Add(uiitem.gameObject);
                    }
                }
                // 用于更新滑动窗口
                // 当前选中的UIItem
                GameObject cur = null;
                // 上一个UIItem
                GameObject last = null;
                // 遍历筛选的ItemList
                for (int i = 0; i < Recipes.Count; ++i)
                {
                    var recipeID = Recipes[i];
                    product = LocalGameManager.Instance.RecipeManager.GetProduct(recipeID);
                    var item = tempUIItemsRecipe[i];
                    // Active
                    item.SetActive(true);
                    // Name
                    var name = item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    name.text = ItemManager.Instance.GetItemName(product.id);
                    if (name.text == "")
                    {
                        name.text = PanelTextContent.textEmpty;
                    }
                    var img = item.transform.Find("Icon").GetComponent<Image>();
                    if (ItemManager.Instance.IsValidItemID(product.id))
                    {
                        var texture = ItemManager.Instance.GetItemTexture2D(product.id);
                        if (texture != null)
                        {
                            var sprite = tempSprite.Find(s => s.texture == texture);
                            if (sprite == null)
                            {
                                sprite = ItemManager.Instance.GetItemSprite(product.id);
                                tempSprite.Add(sprite);
                            }
                            img.sprite = sprite;
                        }
                    }
                    else
                    {
                        img.sprite = null;
                    }
                    // Selected
                    var selected = item.transform.Find("Selected");
                    if (CurrentRecipe == recipeID)
                    {
                        selected.gameObject.SetActive(true);
                        cur = item;
                    }
                    else
                    {
                        selected.gameObject.SetActive(false);
                    }
                    if (i == lastRecipeIndex)
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
                    Recipe_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                }
                // 强制立即更新 GridLayoutGroup 的布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(Recipe_GridLayout.GetComponent<RectTransform>());
                #endregion
                #endregion

                #region Product
                product = LocalGameManager.Instance.RecipeManager.GetProduct(CurrentRecipe);
                var nameProduct = Recipe_Product.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                nameProduct.text = ItemManager.Instance.GetItemName(product.id);
                if (nameProduct.text == "")
                {
                    nameProduct.text = PanelTextContent.textEmpty;
                }
                var imgProduct = Recipe_Product.transform.Find("Icon").GetComponent<Image>();
                if (ItemManager.Instance.IsValidItemID(product.id))
                {
                    var texture = ItemManager.Instance.GetItemTexture2D(product.id);
                    if (texture != null)
                    {
                        var spriteProduct = tempSprite.Find(s => s.texture == texture);
                        if (spriteProduct == null)
                        {
                            spriteProduct = ItemManager.Instance.GetItemSprite(product.id);
                            tempSprite.Add(spriteProduct);
                        }
                        imgProduct.sprite = spriteProduct;
                    }
                }
                else
                {
                    imgProduct.sprite = null;
                }

                var amountProduct = Recipe_Product.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                amountProduct.text = product.num.ToString();
                var tiemProduct = Recipe_Product.transform.Find("Time").GetComponent<TMPro.TextMeshProUGUI>();
                tiemProduct.text = LocalGameManager.Instance.RecipeManager.GetTimeCost(CurrentRecipe).ToString();
                #endregion

                #region Raw
                List<ML.Engine.InventorySystem.CompositeSystem.Formula> recipeRaws = LocalGameManager.Instance.RecipeManager.GetRaw(CurrentRecipe);
                delta = tempUIItemsRecipeRaw.Count - recipeRaws.Count;
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemsRecipeRaw[tempUIItemsRecipeRaw.Count - 1 - i].SetActive(false);
                    }
                }
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(Recipe_Raw_UIItemTemplate, Recipe_Raw_GridLayout.transform, false);
                        tempUIItemsRecipeRaw.Add(uiitem.gameObject);
                    }
                }
                for (int i = 0; i < recipeRaws.Count; ++i)
                {
                    var itemID = recipeRaws[i].id;
                    var item = tempUIItemsRecipeRaw[i];
                    item.SetActive(true);
                    // Name
                    var name = item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    name.text = ItemManager.Instance.GetItemName(itemID);
                    if (name.text == "")
                    {
                        name.text = PanelTextContent.textEmpty;
                    }
                    // NeedAmount
                    var amount = item.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                    amount.text = recipeRaws[i].num.ToString();
                    // 更新Icon
                    if (ItemManager.Instance.IsValidItemID(itemID))
                    {
                        var img = item.transform.Find("Icon").GetComponent<Image>();
                        var texture = ItemManager.Instance.GetItemTexture2D(itemID);
                        if (texture != null)
                        {
                            var sprite = tempSprite.Find(s => s.texture == texture);
                            if (sprite == null)
                            {
                                sprite = ItemManager.Instance.GetItemSprite(itemID);
                                tempSprite.Add(sprite);
                            }
                            img.sprite = sprite;
                        }
                    }
                }
                Recipe_Raw_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                // 强制立即更新 GridLayoutGroup 的布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(Recipe_Raw_GridLayout.GetComponent<RectTransform>());
                #endregion

                #region BotKeyTips
                #endregion
            }
            else if (CurMode == Mode.ChangeWorker)
            {
                this.ChangeWorker.gameObject.SetActive(true);
                this.BotKeyTips_ProNode.gameObject.SetActive(false);
                this.BotKeyTips_Worker.gameObject.SetActive(true);
                this.Worker_UIItemTemplate.gameObject.SetActive(false);
                Workers = new List<Worker>() { null };
                Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();
                #region Select
                // 临时内存生成的UIItem数量(只增不减，多的隐藏掉即可) - 当前筛选出来的Item数量
                int delta = tempUIItemsWorker.Count - Workers.Count;
                // > 0 => 有多余，隐藏
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemsWorker[tempUIItemsWorker.Count - 1 - i].SetActive(false);
                    }
                }
                // < 0 => 不够， 增加
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(Worker_UIItemTemplate, Worker_GridLayout.transform, false);
                        tempUIItemsWorker.Add(uiitem.gameObject);
                    }
                }
                // 用于更新滑动窗口
                // 当前选中的UIItem
                GameObject cur = null;
                // 上一个UIItem
                GameObject last = null;
                // 遍历筛选的ItemList
                for (int i = 0; i < Workers.Count; ++i)
                {
                    var worker = Workers[i];
                    var item = tempUIItemsWorker[i];
                    // Active
                    item.SetActive(true);
                    if (worker != null)
                    {
                        // Name
                        var name = item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                        name.text = worker.Name;
                        if (name.text == "")
                        {
                            name.text = PanelTextContent.textEmpty;
                        }
                        //var img = item.transform.Find("Icon").GetComponent<Image>();
                        //var sprite = tempSprite.Find(s => s.texture == LocalGameManager.Instance.WorkerManager.GetTexture2D());
                        //if (sprite == null)
                        //{
                        //    sprite = LocalGameManager.Instance.WorkerManager.GetSprite();
                        //    tempSprite.Add(sprite);
                        //}
                        //img.sprite = sprite;
                        // State
                        var state = item.transform.Find("State").GetComponent<TMPro.TextMeshProUGUI>();

                        state.text = PanelTextContent.workerStatus[(int)Worker.Status];
                        if (Worker.Status == Status.Fishing && Worker.IsOnDuty)
                        {
                            state.text = PanelTextContent.textWorkerOnDuty;
                        }
                        // PrograssBar
                        var rect = item.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                        rect.offsetMax = new Vector2(rect.offsetMax.x, -1 * (int)(100 - 100 * worker.APCurrent / worker.APMax));
                        // Eff
                        var eff = item.transform.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>();
                        eff.text = worker.Eff[ProNode.ExpType].ToString();
                        // Selected
                        var selected = item.transform.Find("Selected");
                        if (CurrentWorker == worker)
                        {
                            selected.gameObject.SetActive(true);
                            cur = item;
                        }
                        else
                        {
                            selected.gameObject.SetActive(false);
                        }
                        if (i == lastWorkerIndex)
                        {
                            last = item;
                        }
                    }
                    else
                    {
                        item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = "";
                        item.transform.Find("State").GetComponent<TMPro.TextMeshProUGUI>().text = "";
                        var rect = item.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                        rect.offsetMax = new Vector2(rect.offsetMax.x, -100);
                        item.transform.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>().text = "";
                        var selected = item.transform.Find("Selected");
                        if (CurrentWorker == null)
                        {
                            selected.gameObject.SetActive(true);
                            cur = item;
                        }
                        else
                        {
                            selected.gameObject.SetActive(false);
                        }
                        if (i == lastWorkerIndex)
                        {
                            last = item;
                        }
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
                    Worker_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                }
                // 强制立即更新 GridLayoutGroup 的布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(Worker_GridLayout.GetComponent<RectTransform>());
                #endregion
                #endregion

                #region BotKeyTips

                #endregion
            }
            else if (CurMode == Mode.ChangeLevel)
            {
                this.ChangeLevel.gameObject.SetActive(true);
                this.ProNodeUI.gameObject.SetActive(false);
                this.BotKeyTips_ProNode.gameObject.SetActive(false);
                this.BotKeyTips_Level.gameObject.SetActive(true);
                this.Level_UIItemTemplate.gameObject.SetActive(false);

                string cid = this.ProNode.WorldProNode.Classification.ToString();
                string cidUpgrade = BuildingManager.Instance.GetUpgradeCID(cid);
                if (string.IsNullOrEmpty(cidUpgrade))
                {
                    cidUpgrade = cid;
                }
                this.Level_Build.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = BuildingManager.Instance.GetName(cidUpgrade);

                List<Formula> raw = this.ProNode.GetUpgradeRaw();
                List<Formula> rawCur = this.ProNode.GetUpgradeRawCurrent(Player);
                #region Item
                int delta = tempUIItemsLevel.Count - raw.Count;
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemsLevel[tempUIItemsLevel.Count - 1 - i].SetActive(false);
                    }
                }
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(Level_UIItemTemplate, Level_GridLayout.transform, false);
                        tempUIItemsLevel.Add(uiitem.gameObject);
                    }
                }
                for (int i = 0; i < raw.Count; ++i)
                {
                    var uiItemData = tempUIItemsLevel[i];
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
                        nametext.text = PanelTextContent.textEmpty;
                        amounttext.text = "0";
                        needtext.text = "0";
                    }
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(Level_GridLayout.GetComponent<RectTransform>());
                #endregion

                #region Level
                LvOld.text = "Lv: " + this.ProNode.Level.ToString();
                DescOld.text = PanelTextContent.text_LvDesc + this.ProNode.EffBase;
                if (this.ProNode.Level + 1 <= this.ProNode.LevelMax)
                {
                    LvNew.text = "Lv: " + (ProNode.Level + 1).ToString();
                    DescNew.text = PanelTextContent.text_LvDesc + (ProNode.LevelUpgradeEff[ProNode.Level + 1] + this.ProNode.EffBase);
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
        public struct ProNodePanel
        {
            public TextTip[] proNodeType;
            public TextContent textEmpty;

            public TextContent[] TransportPriority;

            /*            public TextContent textUrgency;
                        public TextContent textNormal;
                        public TextContent textAlternative;*/

            public TextContent textStateVacancy;
            public TextContent textStateStagnation;
            public TextContent textStateProduction;

            public TextContent[] workerStatus;
            /*            public TextContent textWorkerStateWork;
                        public TextContent textWorkerStateFish;
                        public TextContent textWorkerStateRelax;*/
            public TextContent textWorkerOnDuty;

            public TextContent textWorkerStateTransport;
            public TextContent textPrefixTime;
            public TextContent textPrefixEff;
            public TextContent text_LvDesc;

            public KeyTip Upgrade;
            public KeyTip NextPriority;
            public KeyTip ChangeRecipe;
            public KeyTip Remove1;
            public KeyTip Remove10;
            public KeyTip FastAdd;
            public KeyTip Back;
            public KeyTip ChangeWorker;
            public KeyTip RemoveWorker;
            public KeyTip ConfirmRecipe;
            public KeyTip BackRecipe;
            public KeyTip ConfirmWorker;
            public KeyTip BackWorker;
            public KeyTip ConfirmLevel;
            public KeyTip BackLevel;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/ProNode";
            this.abname = "ProNodePanel";
            this.description = "ProNodePanel数据加载完成";
        }
        #endregion

        #region Action
        private void OnProduceTimerUpdateAction(double time)
        {
            if (CurMode == Mode.ProNode)
            {
                RectTransform rect = Product.transform.Find("Mask").GetComponent<RectTransform>();
                float percent = 1 - (float)(time / ProNode.TimeCost);
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, percent * Product.transform.Find("Icon").GetComponent<RectTransform>().sizeDelta.y);
                Product.transform.Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textPrefixTime + time.ToString("F2");
            }
        }

        public void RefreshDynamic()
        {
            if (CurMode == Mode.ProNode)
            {
                if (ProNode.Recipe != null)
                {
                    var amountProduct = Product.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    amountProduct.text = ProNode.GetItemAllNum(ProNode.Recipe.ProductID).ToString();
                    for (int i = 0; i < Raws.Count; ++i)
                    {
                        var amount = tempUIItems[i].transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                        amount.text = ProNode.GetItemAllNum(Raws[i].id).ToString();
                    }
                }
                if (Worker != null)
                {
                    var onDuty = UIWorker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();
                    onDuty.text = PanelTextContent.workerStatus[(int)Worker.Status];
                    if (Worker.Status == Status.Fishing && Worker.IsOnDuty)
                    {
                        onDuty.text = PanelTextContent.textWorkerOnDuty;
                    }

                    Text_Eff.text = PanelTextContent.textPrefixEff + ": +" + ProNode.Eff.ToString() + "%";
                    Text_EffProNode.text = ProNode.Name + ": +" + ProNode.EffBase.ToString() + "%";
                    Text_EffWorker.text = Worker.Name + ": +" + (ProNode.Eff - ProNode.EffBase).ToString() + "%";
                    var rect = UIWorker.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                    rect.offsetMax = new Vector2(rect.offsetMax.x, -1 * (int)(100 - 100 * Worker.APCurrent / Worker.APMax));
                    UIWorker.transform.Find("AP").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Worker.APCurrent}/{Worker.APMax}";
                }
            }
        }
        #endregion

    }
}
