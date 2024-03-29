using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.TextContent;
using ProjectOC.ManagerNS;
using ProjectOC.ProNodeNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ML.Engine.Extension;
using ML.Engine.BuildingSystem;
using ProjectOC.MissionNS;
using static ProjectOC.InventorySystem.UI.UIProNode;

namespace ProjectOC.InventorySystem.UI
{
    public class UIProNode : ML.Engine.UI.UIBasePanel<ProNodePanel>
    {
        #region Unity
        public bool IsInit = false;
        protected override void Awake()
        {

            base.Awake();
            this.InitTextContentPathData();

            // TopTitle
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            Transform priority = transform.Find("TopTitle").Find("Priority");
            Text_Priority = priority.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

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
            Text_Recipe_Time = ChangeRecipe.Find("Recipe").Find("Time").GetComponent<TMPro.TextMeshProUGUI>();
            Text_Recipe_Name = ChangeRecipe.Find("Desc").Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
            Text_Recipe_ItemDesc = ChangeRecipe.Find("Desc").Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>();
            Text_Recipe_EffectDesc = ChangeRecipe.Find("Desc").Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>();

            Recipe_UIItemTemplate.gameObject.SetActive(false);
            ChangeRecipe.gameObject.SetActive(false);
            // ChangeWorker
            ChangeWorker = transform.Find("ChangeWorker");
            Worker_GridLayout = ChangeWorker.Find("Select").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Worker_UIItemTemplate = Worker_GridLayout.transform.Find("UIItemTemplate");
            Worker_UIItemTemplate.gameObject.SetActive(false);
            ChangeWorker.gameObject.SetActive(false);
            // Upgrade
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
            BotKeyTips = this.transform.Find("BotKeyTips").Find("KeyTips");
            // BotKeyTips1
            BotKeyTips1 = this.transform.Find("BotKeyTips").Find("KeyTips1");
            BotKeyTips1.gameObject.SetActive(false);
            //CurPriority = MissionNS.TransportPriority.Normal;
            IsInit = true;
        }

        protected override void Start()
        {
            base.Start();
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
            ProNode.OnActionChange += RefreshDynamic;
            ProNode.OnProduceTimerUpdate += OnProduceTimerUpdateAction;
            ProNode.OnProduceEnd += Refresh;
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Enable();
            base.Enter();
        }

        protected override void Exit()
        {
            ProNode.OnActionChange -= RefreshDynamic;
            ProNode.OnProduceTimerUpdate -= OnProduceTimerUpdateAction;
            ProNode.OnProduceEnd -= Refresh;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Disable();
            this.UnregisterInput();
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
        public enum ProNodeSelectMode
        {
            Product = 0,
            Raw = 1,
            Worker = 2
        }
        public ProNodeSelectMode CurProNodeMode = ProNodeSelectMode.Product;


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
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Upgrade.performed -= Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed -= NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove1.performed -= Remove1_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove10.performed -= Remove10_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed -= FastAdd_RemoveWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.performed -= Alter_performed;
        }

        protected override void RegisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Upgrade.performed += Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed += NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove1.performed += Remove1_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove10.performed += Remove10_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed += FastAdd_RemoveWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.performed += Alter_performed;
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
                if (CurProNodeMode == ProNodeSelectMode.Product)
                {
                    if (offset.y < 0 && this.ProNode.Recipe != null)
                    {
                        CurProNodeMode = ProNodeSelectMode.Raw;
                    }
                    else if (offset.x > 0 && ProNode.ProNodeType == ProNodeType.Mannul)
                    {
                        CurProNodeMode = ProNodeSelectMode.Worker;
                    }
                }
                else if (CurProNodeMode == ProNodeSelectMode.Raw)
                {
                    var grid = Raw_GridLayout.GetGridSize();
                    int rawIndex = this.CurrentRawIndex - offset.y * grid.y + offset.x;
                    this.CurrentRawIndex = rawIndex;
                    if (rawIndex >= Raws.Count || rawIndex < 0)
                    {
                        if (offset.y > 0)
                        {
                            CurProNodeMode = ProNodeSelectMode.Product;
                        }
                        else if (offset.x > 0 && ProNode.ProNodeType == ProNodeType.Mannul)
                        {
                            CurProNodeMode = ProNodeSelectMode.Worker;
                        }
                    }
                }
                else if (CurProNodeMode == ProNodeSelectMode.Worker)
                {
                    if (offset.y < 0 && this.ProNode.Recipe != null)
                    {
                        CurProNodeMode = ProNodeSelectMode.Raw;
                    }
                    else if (offset.x < 0)
                    {
                        CurProNodeMode = ProNodeSelectMode.Product;
                    }
                }
                Refresh();
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
                if (CurProNodeMode == ProNodeSelectMode.Worker && ProNode.ProNodeType == ProNodeType.Mannul)
                {
                    CurMode = Mode.ChangeWorker;
                }
                else
                {
                    CurMode = Mode.ChangeRecipe;
                }
            }
            else if (CurMode == Mode.ChangeRecipe)
            {
                ProNode.ChangeRecipe(Player, CurrentRecipe);
                BoxCollider collider = ProNode.WorldProNode.transform.GetComponent<BoxCollider>();
                ItemManager.Instance.AddItemIconObject(ProNode.Recipe.Product.id,
                                                               this.ProNode.WorldProNode.transform,
                                                               new Vector3(0, this.ProNode.WorldProNode.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0),
                                                               Quaternion.Euler(new Vector3(0, 0, 0)),
                                                               Vector3.one);
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
        private void FastAdd_RemoveWorker_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                if (CurProNodeMode == ProNodeSelectMode.Worker)
                {
                    ProNode.RemoveWorker();

                }
                else
                {
                    ProNode.UIFastAdd(Player, CurrentRaw);
                }
            }
            Refresh();
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
        private TMPro.TextMeshProUGUI Text_Recipe_Time;
        private TMPro.TextMeshProUGUI Text_Recipe_Name;
        private TMPro.TextMeshProUGUI Text_Recipe_ItemDesc;
        private TMPro.TextMeshProUGUI Text_Recipe_EffectDesc;
        private TMPro.TextMeshProUGUI LvOld;
        private TMPro.TextMeshProUGUI LvNew;
        private TMPro.TextMeshProUGUI DescOld;
        private TMPro.TextMeshProUGUI DescNew;

        private Transform Priority;
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
        private Transform BotKeyTips;
        private Transform BotKeyTips1;
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
                this.BotKeyTips.gameObject.SetActive(true);
                this.BotKeyTips1.gameObject.SetActive(false);
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

                Product.transform.Find("Selected").gameObject.SetActive(false);
                foreach (var temp in tempUIItems)
                {
                    temp.transform.Find("Selected").gameObject.SetActive(false);
                }
                ProNodeUI.Find("Worker").Find("Selected").gameObject.SetActive(false);

                if (CurProNodeMode == ProNodeSelectMode.Product)
                {
                    Product.transform.Find("Selected").gameObject.SetActive(true);
                }
                else if (CurProNodeMode == ProNodeSelectMode.Worker)
                {
                    ProNodeUI.Find("Worker").Find("Selected").gameObject.SetActive(true);
                }

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
                    RectTransform rect = Product.transform.Find("Mask").GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, 0);
                    #endregion

                    #region Raw
                    int delta = tempUIItems.Count - Raws.Count;
                    if (delta > 0)
                    {
                        for (int i = 0; i < delta; ++i)
                        {
                            tempUIItems[tempUIItems.Count - 1 - i].SetActive(false);
                        }
                    }
                    else if (delta < 0)
                    {
                        delta = -delta;
                        for (int i = 0; i < delta; ++i)
                        {
                            var uiitem = Instantiate(Raw_UIItemTemplate, Raw_GridLayout.transform, false);
                            tempUIItems.Add(uiitem.gameObject);
                        }
                    }
                    GameObject cur = null;
                    GameObject last = null;
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

                        if (int.Parse(amount.text) < int.Parse(needAmount.text))
                        {
                            item.transform.Find("Back").GetComponent<Image>().color = Color.red;
                        }
                        else
                        {
                            item.transform.Find("Back").GetComponent<Image>().color = Color.black;
                        }

                        if (CurrentRaw == itemID)
                        {
                            if (CurProNodeMode == ProNodeSelectMode.Raw)
                            {
                                item.transform.Find("Selected").gameObject.SetActive(true);
                            }
                            cur = item;
                        }
                        if (i == lastRawIndex)
                        {
                            last = item;
                        }
                    }
                    #region 更新滑动窗口
                    if (cur != null && last != null)
                    {
                        RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                        RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                        ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                        RectTransform contentRect = scrollRect.content;

                        Vector3[] corners = new Vector3[4];
                        uiRectTransform.GetWorldCorners(corners);
                        bool allCornersVisible = true;
                        for (int i = 0; i < 4; ++i)
                        {
                            Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                            if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                            {
                                allCornersVisible = false;
                                break;
                            }
                        }

                        if (!allCornersVisible)
                        {
                            Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                            Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;
                            Vector2 offset = positionB - positionA;
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
                    LayoutRebuilder.ForceRebuildLayoutImmediate(Raw_GridLayout.GetComponent<RectTransform>());
                    #endregion
                }
                else
                {
                    Product.transform.Find("Icon").GetComponent<Image>().sprite = null;
                    Product.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
                    Product.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "0";
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
                        var onDuty = UIWorker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();

                        onDuty.text = PanelTextContent.workerStatus[(int)Worker.Status];
                        if (Worker.Status == Status.Fishing && Worker.IsOnDuty)
                        {
                            onDuty.text = PanelTextContent.textOnDuty;
                        }

                        var rect = UIWorker.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                        rect.offsetMax = new Vector2(rect.offsetMax.x, -1 * (int)(100 - 100 * Worker.APCurrent / Worker.APMax));
                    }
                    else
                    {
                        UIWorker.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
                        UIWorker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textLack;
                        RectTransform rect = UIWorker.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                        rect.offsetMax = new Vector2(rect.offsetMax.x, -100);
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
                    Text_Eff.text = PanelTextContent.textEff + ": +" + ProNode.Eff.ToString() + "%";
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
                if (this.CurProNodeMode == ProNodeSelectMode.Worker)
                {
                    BotKeyTips.Find("KT_ChangeWorker").gameObject.SetActive(true);
                    BotKeyTips.Find("KT_RemoveWorker").gameObject.SetActive(true);
                    BotKeyTips.Find("KT_ChangeRecipe").gameObject.SetActive(false);
                    BotKeyTips.Find("KT_Remove1").gameObject.SetActive(false);
                    BotKeyTips.Find("KT_Remove10").gameObject.SetActive(false);
                    BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(false);
                    BotKeyTips.Find("KT_Return").gameObject.SetActive(false);
                }
                else
                {
                    BotKeyTips.Find("KT_ChangeWorker").gameObject.SetActive(false);
                    BotKeyTips.Find("KT_RemoveWorker").gameObject.SetActive(false);
                    BotKeyTips.Find("KT_ChangeRecipe").gameObject.SetActive(true);
                    BotKeyTips.Find("KT_Remove1").gameObject.SetActive(true);
                    BotKeyTips.Find("KT_Remove10").gameObject.SetActive(true);
                    BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(true);
                    BotKeyTips.Find("KT_Return").gameObject.SetActive(true);
                }
                #endregion
            }
            else if (CurMode == Mode.ChangeRecipe)
            {
                this.ChangeRecipe.gameObject.SetActive(true);
                this.BotKeyTips.gameObject.SetActive(false);
                this.BotKeyTips1.gameObject.SetActive(true);
                this.Recipe_UIItemTemplate.gameObject.SetActive(false);
                this.Recipe_Raw_UIItemTemplate.gameObject.SetActive(false);

                Recipes = new List<string>() { "" };
                Recipes.AddRange(ProNode.GetCanProduceRecipe());
                Formula product;
                int delta;
                #region Select
                delta = tempUIItemsRecipe.Count - Recipes.Count;
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemsRecipe[tempUIItemsRecipe.Count - 1 - i].SetActive(false);
                    }
                }
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(Recipe_UIItemTemplate, Recipe_GridLayout.transform, false);
                        tempUIItemsRecipe.Add(uiitem.gameObject);
                    }
                }
                GameObject cur = null;
                GameObject last = null;
                for (int i = 0; i < Recipes.Count; ++i)
                {
                    var recipeID = Recipes[i];
                    product = LocalGameManager.Instance.RecipeManager.GetProduct(recipeID);
                    var item = tempUIItemsRecipe[i];
                    // Active
                    item.SetActive(true);
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
                    RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                    RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                    ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                    RectTransform contentRect = scrollRect.content;

                    Vector3[] corners = new Vector3[4];
                    uiRectTransform.GetWorldCorners(corners);
                    bool allCornersVisible = true;
                    for (int i = 0; i < 4; ++i)
                    {
                        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                        if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                        {
                            allCornersVisible = false;
                            break;
                        }
                    }

                    if (!allCornersVisible)
                    {
                        Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                        Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;

                        Vector2 offset = positionB - positionA;
                        Vector2 normalizedPosition = scrollRect.normalizedPosition;
                        normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                        scrollRect.normalizedPosition = normalizedPosition;
                    }
                }
                else
                {
                    Recipe_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(Recipe_GridLayout.GetComponent<RectTransform>());
                #endregion
                #endregion

                #region Product
                product = LocalGameManager.Instance.RecipeManager.GetProduct(CurrentRecipe);
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
                        Recipe_Product.GetComponent<Image>().sprite = spriteProduct;
                    }

                    Text_Recipe_Time.text = PanelTextContent.textTime + ":" + LocalGameManager.Instance.RecipeManager.GetTimeCost(CurrentRecipe).ToString() + "s";
                    Text_Recipe_Name.text = ItemManager.Instance.GetItemName(product.id);
                    Text_Recipe_ItemDesc.text = ItemManager.Instance.GetItemDescription(product.id);
                    Text_Recipe_EffectDesc.text = ItemManager.Instance.GetEffectDescription(product.id);
                }
                else
                {
                    Recipe_Product.GetComponent<Image>().sprite = null;
                    Text_Recipe_Time.text = "0";
                    Text_Recipe_Name.text = PanelTextContent.textEmpty;
                    Text_Recipe_ItemDesc.text = PanelTextContent.textEmpty;
                    Text_Recipe_EffectDesc.text = PanelTextContent.textEmpty;
                }
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
                LayoutRebuilder.ForceRebuildLayoutImmediate(Recipe_Raw_GridLayout.GetComponent<RectTransform>());
                #endregion

            }
            else if (CurMode == Mode.ChangeWorker)
            {
                this.ChangeWorker.gameObject.SetActive(true);
                this.BotKeyTips.gameObject.SetActive(false);
                this.BotKeyTips1.gameObject.SetActive(true);
                this.Worker_UIItemTemplate.gameObject.SetActive(false);
                Workers = new List<Worker>() { null };
                Workers.AddRange(LocalGameManager.Instance.WorkerManager.GetWorkers());
                #region Select
                int delta = tempUIItemsWorker.Count - Workers.Count;
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemsWorker[tempUIItemsWorker.Count - 1 - i].SetActive(false);
                    }
                }
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(Worker_UIItemTemplate, Worker_GridLayout.transform, false);
                        tempUIItemsWorker.Add(uiitem.gameObject);
                    }
                }
                GameObject cur = null;
                GameObject last = null;
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
                        state.text = PanelTextContent.workerStatus[(int)worker.Status];
                        if (worker.Status == Status.Fishing && worker.IsOnDuty)
                        {
                            state.text = PanelTextContent.textOnDuty;
                        }
                        // PrograssBar
                        var rect = item.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                        rect.offsetMax = new Vector2(rect.offsetMax.x, (int)(((float)worker.APCurrent/worker.APMax - 1) * 60));
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
                    RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                    RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                    ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                    RectTransform contentRect = scrollRect.content;

                    Vector3[] corners = new Vector3[4];
                    uiRectTransform.GetWorldCorners(corners);
                    bool allCornersVisible = true;
                    for (int i = 0; i < 4; ++i)
                    {
                        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                        if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                        {
                            allCornersVisible = false;
                            break;
                        }
                    }

                    if (!allCornersVisible)
                    {
                        Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                        Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;
                        Vector2 offset = positionB - positionA;
                        Vector2 normalizedPosition = scrollRect.normalizedPosition;
                        normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                        scrollRect.normalizedPosition = normalizedPosition;
                    }
                }
                else
                {
                    Worker_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(Worker_GridLayout.GetComponent<RectTransform>());
                #endregion
                #endregion
            }
            else if (CurMode == Mode.ChangeLevel)
            {
                this.ChangeLevel.gameObject.SetActive(true);
                this.ProNodeUI.gameObject.SetActive(false);
                this.BotKeyTips.gameObject.SetActive(false);
                this.BotKeyTips1.gameObject.SetActive(true);
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
                    var img = uiItemData.transform.Find("Icon").GetComponent<Image>();
                    if (ItemManager.Instance.IsValidItemID(itemID))
                    {
                        var texture = ItemManager.Instance.GetItemTexture2D(itemID);
                        if (texture != null)
                        {
                            var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(itemID));
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
                    var back = uiItemData.transform.Find("Back").GetComponent<Image>();
                    if (itemID != "")
                    {
                        nametext.text = ItemManager.Instance.GetItemName(itemID);
                        amounttext.text = current.ToString();
                        needtext.text = need.ToString();
                        if (current < need)
                        {
                            back.color = Color.red;
                        }
                        else
                        {
                            back.color = Color.black;
                        }
                    }
                    else
                    {
                        back.color = Color.black;
                        nametext.text = PanelTextContent.textEmpty;
                        amounttext.text = "0";
                        needtext.text = "0";
                    }
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(Level_GridLayout.GetComponent<RectTransform>());
                #endregion

                #region Level
                LvOld.text = "Lv: " + this.ProNode.Level.ToString();
                DescOld.text = PanelTextContent.textLvDesc + this.ProNode.EffBase + "%";
                if (this.ProNode.Level + 1 <= this.ProNode.LevelMax)
                {
                    ChangeLevel.Find("Level").Find("Back1").gameObject.SetActive(true);
                    LvNew.text = "Lv: " + (ProNode.Level + 1).ToString();
                    DescNew.text = PanelTextContent.textLvDesc + (ProNode.LevelUpgradeEff[ProNode.Level + 1] + this.ProNode.EffBase) + "%";
                }
                else
                {
                    ChangeLevel.Find("Level").Find("Back1").gameObject.SetActive(false);
                    LvNew.text = "";
                    DescNew.text = "";
                }
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
            public TextContent textLack;
            public TextContent[] TransportPriority;
            public TextContent textStateVacancy;
            public TextContent textStateStagnation;
            public TextContent textStateProduction;
            public TextContent[] workerStatus;
            public TextContent textOnDuty;
            public TextContent textTime;
            public TextContent textEff;
            public TextContent textLvDesc;

            public KeyTip Upgrade;
            public KeyTip NextPriority;
            public KeyTip ChangeRecipe;
            public KeyTip Remove1;
            public KeyTip Remove10;
            public KeyTip FastAdd;
            public KeyTip Return;
            public KeyTip Confirm;
            public KeyTip Back;
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
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, percent * 100);
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
                        onDuty.text = PanelTextContent.textOnDuty;
                    }
                    Text_Eff.text = PanelTextContent.textEff + ": +" + ProNode.Eff.ToString() + "%";
                    Text_EffProNode.text = ProNode.Name + ": +" + ProNode.EffBase.ToString() + "%";
                    Text_EffWorker.text = Worker.Name + ": +" + (ProNode.Eff - ProNode.EffBase).ToString() + "%";
                    var rect = UIWorker.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                    rect.offsetMax = new Vector2(rect.offsetMax.x, -1 * (int)(100 - 100 * Worker.APCurrent / Worker.APMax));
                }
            }
        }
        #endregion

    }
}
