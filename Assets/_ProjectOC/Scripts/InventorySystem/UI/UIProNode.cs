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
using ML.Engine.Utility;
using UnityEngine.U2D;
using ML.Engine.Manager;
using ProjectOC.Player;

namespace ProjectOC.InventorySystem.UI
{
    public class UIProNode : ML.Engine.UI.UIBasePanel<ProNodePanel>
    {
        #region Input
        private bool ItemIsDestroyed = false;
        #endregion

        #region Unity
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();

            this.InitTextContentPathData();

            #region TopTitle
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Text_Priority = transform.Find("TopTitle").Find("Priority").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            #endregion

            #region ProNode
            ProNode_UI = transform.Find("ProNode");
            Transform recipe = transform.Find("ProNode").Find("Recipe");
            ProNode_Product = recipe.Find("Product");
            EmptySprite = ProNode_Product.Find("Icon").GetComponent<Image>().sprite;
            ProNode_Raw_GridLayout = recipe.Find("Raw").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            ProNode_Raw_UIItemTemplate = ProNode_Raw_GridLayout.transform.Find("UIItemTemplate");
            ProNode_Raw_UIItemTemplate.gameObject.SetActive(false);
            ProNode_Worker = transform.Find("ProNode").Find("Worker");
            ProNode_Eff = transform.Find("ProNode").Find("Eff");
            #endregion

            #region ChangeRecipe
            Recipe_UI = transform.Find("ChangeRecipe");
            Recipe_UI.gameObject.SetActive(false);
            Recipe_GridLayout = Recipe_UI.Find("Select").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Recipe_UIItemTemplate = Recipe_GridLayout.transform.Find("UIItemTemplate");
            Recipe_UIItemTemplate.gameObject.SetActive(false);
            Recipe_Raw_GridLayout = Recipe_UI.Find("Recipe").Find("Raw").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Recipe_Raw_UIItemTemplate = Recipe_Raw_GridLayout.transform.Find("UIItemTemplate");
            Recipe_Raw_UIItemTemplate.gameObject.SetActive(false);
            Recipe_Desc = Recipe_UI.Find("Desc");
            #endregion

            #region ChangeWorker
            Worker_UI = transform.Find("ChangeWorker");
            Worker_UI.gameObject.SetActive(false);
            Worker_GridLayout = Worker_UI.Find("Select").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Worker_UIItemTemplate = Worker_GridLayout.transform.Find("UIItemTemplate");
            Worker_UIItemTemplate.gameObject.SetActive(false);
            #endregion

            #region Upgrade
            Upgrade_UI = transform.Find("Upgrade");
            Upgrade_UI.gameObject.SetActive(false);
            Upgrade_Build = Upgrade_UI.Find("Build");
            Transform contentUpgrade = transform.Find("Upgrade").Find("Raw").Find("Viewport").Find("Content");
            Upgrade_GridLayout = contentUpgrade.GetComponent<GridLayoutGroup>();
            Upgrade_UIItemTemplate = contentUpgrade.Find("UIItemTemplate");
            Upgrade_UIItemTemplate.gameObject.SetActive(false);
            Upgrade_LvOld = Upgrade_UI.Find("Level").Find("LvOld");
            Upgrade_LvNew = Upgrade_UI.Find("Level").Find("LvNew");
            #endregion

            #region BotKeyTips
            BotKeyTips = this.transform.Find("BotKeyTips").Find("KeyTips");
            BotKeyTips1 = this.transform.Find("BotKeyTips").Find("KeyTips1");
            BotKeyTips1.gameObject.SetActive(false);
            #endregion
         
            IsInit = true;
        }
        #endregion

        #region Override
        protected override void Enter()
        {
            ProNode.OnActionChange += RefreshDynamic;
            ProNode.OnProduceUpdate += OnProduceTimerUpdateAction;
            ProNode.OnProduceEnd += Refresh;
            base.Enter();
        }

        protected override void Exit()
        {
            ProNode.OnActionChange -= RefreshDynamic;
            ProNode.OnProduceUpdate -= OnProduceTimerUpdateAction;
            ProNode.OnProduceEnd -= Refresh;
            ClearTemp();
            base.Exit();
        }
        #endregion

        #region Config
        [System.Serializable]
        public struct APBarColorConfig
        {
            [LabelText("初始体力值")]
            public int Start;
            [LabelText("结束体力值")]
            public int End;
            [LabelText("对应颜色")]
            public Color Color;
        }
        [System.Serializable]
        public struct IconNumConfig
        {
            [LabelText("初始体力值")]
            public int Start;
            [LabelText("结束体力值")]
            public int End;
            [LabelText("对应数量")]
            public int Num;
        }

        [LabelText("体力对应的体力条颜色"), FoldoutGroup("配置项"), ShowInInspector]
        public List<APBarColorConfig> APBarColorConfigs = new List<APBarColorConfig>();
        [LabelText("学习速度对应的图标数量"), FoldoutGroup("配置项"), ShowInInspector]
        public List<IconNumConfig> ExpRateIconNumConfigs = new List<IconNumConfig>();
        private Color GetAPBarColor(int ap)
        {
            foreach (var config in APBarColorConfigs)
            {
                if (config.Start <= ap && (config.End == 0 || ap <= config.End))
                {
                    return config.Color;
                }
            }
            return default(Color);
        }
        private int GetExpRateIconNum(int eff)
        {
            foreach (var config in ExpRateIconNumConfigs)
            {
                if (config.Start <= eff && (config.End == 0 || eff <= config.End))
                {
                    return config.Num;
                }
            }
            return 0;
        }
        #endregion

        #region Internal
        public enum Mode
        {
            ProNode = 0,
            ChangeRecipe = 1,
            ChangeWorker = 2,
            Upgrade = 3,
        }
        public Mode CurMode = Mode.ProNode;
        public enum ProNodeSelectMode
        {
            Recipe = 0,
            Worker = 1
        }
        public ProNodeSelectMode CurProNodeMode = ProNodeSelectMode.Recipe;

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
                if (ProNode_Priority != null)
                {
                    ProNode_Priority.Find("Selected").gameObject.SetActive(false);
                }
                curPriority = value;

                Text_Priority.text = PanelTextContent.TransportPriority[(int)curPriority];
                ProNode_Priority = transform.Find("TopTitle").Find("Priority").GetChild((int)curPriority);
                ProNode_Priority.Find("Selected").gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 是否有升级功能
        /// </summary>
        public bool HasUpgrade;

       // ProNode Raw
        private List<Formula> Raws => ProNode.Recipe?.Raw;
        public bool IsInitCurRecipe;
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

        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Upgrade.performed -= Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed -= NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.canceled -= Remove_canceled;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.performed -= Remove_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed -= FastAdd_RemoveWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.started -= Alter_started;
        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Upgrade.performed += Upgrade_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed += NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.canceled += Remove_canceled;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.performed += Remove_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed += FastAdd_RemoveWorker_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.started += Alter_started;
        }
        private void Upgrade_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode != Mode.Upgrade && HasUpgrade)
            {
                CurMode = Mode.Upgrade;
            }
            else
            {
                CurMode = Mode.ProNode;
            }
            Refresh();
        }
        private void NextPriority_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ProNode.TransportPriority = (TransportPriority)(((int)ProNode.TransportPriority + 1) % System.Enum.GetValues(typeof(TransportPriority)).Length);
            CurPriority = ProNode.TransportPriority;
        }

        private void Alter_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                if (offset.x > 0 && ProNode.ProNodeType == ProNodeType.Mannul)
                {
                    CurProNodeMode = ProNodeSelectMode.Worker;
                }
                else if (offset.x < 0)
                {
                    CurProNodeMode = ProNodeSelectMode.Recipe;
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
                ProNode.ChangeRecipe(CurrentRecipe);
                IsInitCurRecipe = false;
                CurMode = Mode.ProNode;
            }
            else if (CurMode == Mode.ChangeWorker)
            {
                ProNode.ChangeWorker(CurrentWorker);
                lastWorkerIndex = 0;
                currentWorkerIndex = 0;
                CurMode = Mode.ProNode;
            }
            else if (CurMode == Mode.Upgrade)
            {
                BuildingManager.Instance.Upgrade(ProNode.WorldProNode);
            }
            Refresh();
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                if (ProNode.HasRecipe)
                {
                    ItemManager.Instance.AddItemIconObject(ProNode.Recipe.Product.id,
                                                           ProNode.WorldProNode.transform,
                                                           new Vector3(0, ProNode.WorldProNode.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0),
                                                           Quaternion.Euler(Vector3.zero),
                                                           Vector3.one);
                }
                UIMgr.PopPanel();
            }
            else
            {
                CurMode = Mode.ProNode;
                Refresh();
            }
        }

        private void Remove_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode && CurProNodeMode == ProNodeSelectMode.Recipe)
            {
                if (this.ItemIsDestroyed)
                {
                    this.ItemIsDestroyed = false;
                }
                else
                {
                    ProNode.UIRemove(1);
                    Refresh();
                }
            }
        }
        private void Remove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode && CurProNodeMode == ProNodeSelectMode.Recipe)
            {
                this.ItemIsDestroyed = true;
                if (ProNode.Stack < 10)
                {
                    ProNode.UIRemove(ProNode.Stack);
                }
                else
                {
                    ProNode.UIRemove(10);
                }
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
                    ProNode.UIFastAdd();
                }
            }
            Refresh();
        }
        #endregion

        #region UI
        #region Temp
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        private List<GameObject> tempUIItems = new List<GameObject>();
        private List<GameObject> tempUIItemsRecipe = new List<GameObject>();
        private List<GameObject> tempUIItemsRecipeRaw = new List<GameObject>();
        private List<GameObject> tempUIItemsWorker = new List<GameObject>();
        private List<GameObject> tempUIItemsUpgrade = new List<GameObject>();

        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
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
            foreach (var s in tempUIItemsUpgrade)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
        }
        #endregion

        #region UI对象引用
        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;
        private Sprite EmptySprite;
        private Sprite WorkerIcon;
        private Sprite WorkerMaleIcon;
        private Sprite WorkerFemalIcon;

        private Transform ProNode_UI;
        private Transform ProNode_Priority;
        private Transform ProNode_Product;
        private GridLayoutGroup ProNode_Raw_GridLayout;
        private Transform ProNode_Raw_UIItemTemplate;
        private Transform ProNode_Worker;
        private Transform ProNode_Eff;

        private Transform Recipe_UI;
        private GridLayoutGroup Recipe_GridLayout;
        private Transform Recipe_UIItemTemplate;
        private GridLayoutGroup Recipe_Raw_GridLayout;
        private Transform Recipe_Raw_UIItemTemplate;
        private Transform Recipe_Desc;

        private Transform Worker_UI;
        private GridLayoutGroup Worker_GridLayout;
        private Transform Worker_UIItemTemplate;

        private Transform Upgrade_UI;
        private Transform Upgrade_Build;
        private Transform Upgrade_UIItemTemplate;
        private GridLayoutGroup Upgrade_GridLayout;
        private Transform Upgrade_LvOld;
        private Transform Upgrade_LvNew;

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
            CurPriority = ProNode.TransportPriority;

            this.ProNode_UI.gameObject.SetActive(true);
            this.Recipe_UI.gameObject.SetActive(CurMode == Mode.ChangeRecipe);
            this.Worker_UI.gameObject.SetActive(CurMode == Mode.ChangeWorker);
            this.Upgrade_UI.gameObject.SetActive(CurMode == Mode.Upgrade);
            this.BotKeyTips.gameObject.SetActive(CurMode == Mode.ProNode);
            this.BotKeyTips1.gameObject.SetActive(CurMode != Mode.ProNode);
            this.BotKeyTips1.Find("KT_Confirm").gameObject.SetActive(CurMode != Mode.Upgrade);
            transform.Find("TopTitle").Find("KT_Upgrade").gameObject.SetActive(HasUpgrade);

            if (CurMode == Mode.ProNode)
            {
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

                #region Product
                ProNode_Product.Find("Selected").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
                bool hasRecipe = ProNode.HasRecipe;
                ProNode_Product.Find("Mask").gameObject.SetActive(hasRecipe);
                ProNode_Product.transform.Find("Name").gameObject.SetActive(hasRecipe);
                ProNode_Product.transform.Find("Back").gameObject.SetActive(hasRecipe);
                ProNode_Product.transform.Find("Back1").gameObject.SetActive(hasRecipe);
                ProNode_Product.transform.Find("Amount").gameObject.SetActive(hasRecipe);

                if (!hasRecipe || ProNode.State != ProNodeState.Production)
                {
                    ProNode_Product.Find("Mask").GetComponent<Image>().fillAmount = 0;
                }

                if (hasRecipe)
                {
                    #region Product
                    string productID = ProNode.Recipe.ProductID;
                    var imgProduct = ProNode_Product.transform.Find("Icon").GetComponent<Image>();
                    if (ItemManager.Instance.IsValidItemID(productID))
                    {
                        if (!tempSprite.ContainsKey(productID))
                        {
                            var sprite = ItemManager.Instance.GetItemSprite(productID);
                            tempSprite[productID] = sprite;
                        }
                        imgProduct.sprite = tempSprite[productID];
                    }
                    else
                    {
                        imgProduct.sprite = EmptySprite;
                    }
                    ProNode_Product.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(productID);
                    ProNode_Product.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.Stack.ToString();
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
                            var uiitem = Instantiate(ProNode_Raw_UIItemTemplate, ProNode_Raw_GridLayout.transform, false);
                            tempUIItems.Add(uiitem.gameObject);
                        }
                    }
                    for (int i = 0; i < Raws.Count; ++i)
                    {
                        var itemID = Raws[i].id;
                        var item = tempUIItems[i];
                        // Active
                        item.SetActive(true);
                        // Icon
                        var img = item.transform.Find("Icon").GetComponent<Image>();
                        if (ItemManager.Instance.IsValidItemID(itemID))
                        {
                            if (!tempSprite.ContainsKey(itemID))
                            {
                                var sprite = ItemManager.Instance.GetItemSprite(itemID);
                                tempSprite[itemID] = sprite;
                            }
                            img.sprite = tempSprite[itemID];
                        }
                        else
                        {
                            img.sprite = EmptySprite;
                        }
                        // NeedAmount
                        var needAmount = item.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                        needAmount.text = ProNode.Recipe.GetRawNum(itemID).ToString();
                        // Amount
                        var amount = item.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                        amount.text = ProNode.GetItemAllNum(itemID).ToString();
                        // Name
                        item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(itemID);
                        if (int.Parse(amount.text) < int.Parse(needAmount.text))
                        {
                            item.transform.Find("Back").GetComponent<Image>().color = Color.red;
                        }
                        else
                        {
                            item.transform.Find("Back").GetComponent<Image>().color = Color.black;
                        }
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ProNode_Raw_GridLayout.GetComponent<RectTransform>());
                    #endregion
                }
                else
                {
                    ProNode_Product.transform.Find("Icon").GetComponent<Image>().sprite = EmptySprite;
                    for (int i = 0; i < tempUIItems.Count; ++i)
                    {
                        tempUIItems[tempUIItems.Count - 1 - i].SetActive(false);
                    }
                }
                #endregion

                #region Worker
                ProNode_Worker.Find("Selected").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Worker);
                ProNode_Worker.gameObject.SetActive(ProNode.ProNodeType == ProNodeType.Mannul);
                bool hasWorker = ProNode.HasWorker;
                if (ProNode.ProNodeType == ProNodeType.Mannul)
                {
                    ProNode_Worker.Find("Bar").gameObject.SetActive(hasWorker);
                    ProNode_Worker.Find("Bar1").gameObject.SetActive(hasWorker);
                    ProNode_Worker.Find("Image").gameObject.SetActive(hasWorker);
                    ProNode_Worker.Find("Name").gameObject.SetActive(hasWorker);
                    ProNode_Worker.Find("Empty").gameObject.SetActive(!hasWorker);
                    ProNode_Worker.Find("Gender").gameObject.SetActive(hasWorker);
                    var onDuty = ProNode_Worker.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();
                    if (hasWorker)
                    {
                        ProNode_Worker.Find("Icon").GetComponent<Image>().sprite = WorkerIcon;
                        ProNode_Worker.Find("Bar1").GetComponent<Image>().fillAmount = (float)Worker.APCurrent / Worker.APMax;
                        ProNode_Worker.Find("Bar1").GetComponent<Image>().color = GetAPBarColor(Worker.APCurrent);
                        ProNode_Worker.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = Worker.Name;
                        ProNode_Worker.Find("Gender").GetComponent<Image>().sprite = Worker.Gender == Gender.Male ? WorkerMaleIcon : WorkerFemalIcon ;
                        onDuty.text = PanelTextContent.workerStatus[(int)Worker.Status];
                    }
                    else
                    {
                        ProNode_Worker.Find("Icon").GetComponent<Image>().sprite = EmptySprite;
                        onDuty.text = PanelTextContent.textLack;
                    }
                }
                #endregion

                #region Eff
                ProNode_Eff.Find("EffPrefix").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEff;
                ProNode_Eff.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + ProNode.Eff.ToString() + "%";

                string buildCID = ProNode.WorldProNode.Classification.ToString();
                string buildID = BuildingManager.Instance.GetID(buildCID);
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = CompositeManager.Instance.GetCompositonSprite(buildID);
                }
                ProNode_Eff.Find("IconProNode").GetComponent<Image>().sprite = tempSprite[buildID];
                ProNode_Eff.Find("EffProNode").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + ProNode.EffBase.ToString() + "%";
                ProNode_Eff.Find("IconWorker").gameObject.SetActive(hasWorker);
                ProNode_Eff.Find("EffWorker").gameObject.SetActive(hasWorker);
                if (hasWorker)
                {
                    ProNode_Eff.Find("IconWorker").GetComponent<Image>().sprite = WorkerIcon;
                    ProNode_Eff.Find("EffWorker").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + (ProNode.Eff - ProNode.EffBase).ToString() + "%";
                }
                #endregion
                #endregion

                #region BotKeyTips
                BotKeyTips.Find("KT_ChangeWorker").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Worker);
                BotKeyTips.Find("KT_RemoveWorker").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Worker && ProNode.HasWorker);
                BotKeyTips.Find("KT_ChangeRecipe").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
                BotKeyTips.Find("KT_Remove1").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe);
                BotKeyTips.Find("KT_Remove10").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe);
                BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && Raws != null && Raws.Count > 0);
                BotKeyTips.Find("KT_Return").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
                LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
                #endregion
            }
            else if (CurMode == Mode.ChangeRecipe)
            {
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

                if (!IsInitCurRecipe)
                {
                    for (int i = 0; i < Recipes.Count; ++i)
                    {
                        if (ProNode.HasRecipe && ProNode.Recipe.ID == Recipes[i])
                        {
                            lastRecipeIndex = currentRecipeIndex;
                            currentRecipeIndex = i;
                            IsInitCurRecipe = true;
                            break;
                        }
                    }
                }

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
                        if (!tempSprite.ContainsKey(product.id))
                        {
                            var sprite = ItemManager.Instance.GetItemSprite(product.id);
                            tempSprite[product.id] = sprite;
                        }
                        img.sprite = tempSprite[product.id];
                    }
                    else
                    {
                        img.sprite = EmptySprite;
                    }
                    // Selected
                    bool isSelected = CurrentRecipe == recipeID;
                    item.transform.Find("Selected").gameObject.SetActive(isSelected);
                    if (isSelected)
                    {
                        cur = item;
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
                    if (!tempSprite.ContainsKey(product.id))
                    {
                        var sprite = ItemManager.Instance.GetItemSprite(product.id);
                        tempSprite[product.id] = sprite;
                    }
                    Recipe_UI.Find("Recipe").Find("Product").GetComponent<Image>().sprite = tempSprite[product.id];
                    Recipe_UI.Find("Recipe").Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textTime + ":" + LocalGameManager.Instance.RecipeManager.GetTimeCost(CurrentRecipe).ToString() + "s";
                    Recipe_Desc.Find("Icon").GetComponent<Image>().sprite = tempSprite[product.id];
                    Recipe_Desc.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(product.id);
                    Recipe_Desc.Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemDescription(product.id);
                    Recipe_Desc.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetEffectDescription(product.id);
                    Recipe_Desc.Find("WeightIcon").gameObject.SetActive(true);
                    Recipe_Desc.Find("Weight").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetWeight(product.id).ToString();
                }
                else
                {
                    Recipe_UI.Find("Recipe").Find("Product").GetComponent<Image>().sprite = EmptySprite;
                    Recipe_UI.Find("Recipe").Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textTime + ": 0s";
                    Recipe_Desc.Find("Icon").GetComponent<Image>().sprite = EmptySprite;
                    Recipe_Desc.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
                    Recipe_Desc.Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
                    Recipe_Desc.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
                    Recipe_Desc.Find("WeightIcon").gameObject.SetActive(false);
                    Recipe_Desc.Find("Weight").GetComponent<TMPro.TextMeshProUGUI>().text = "";
                }
                #endregion

                #region Raw
                List<Formula> recipeRaws = LocalGameManager.Instance.RecipeManager.GetRaw(CurrentRecipe);
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
                    // Icon
                    if (ItemManager.Instance.IsValidItemID(itemID))
                    {
                        var img = item.transform.Find("Icon").GetComponent<Image>();
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            var sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite[itemID] = sprite;
                        }
                        img.sprite = tempSprite[itemID];
                    }
                }
                Recipe_Raw_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                LayoutRebuilder.ForceRebuildLayoutImmediate(Recipe_Raw_GridLayout.GetComponent<RectTransform>());
                #endregion
            }
            else if (CurMode == Mode.ChangeWorker)
            {
                Workers = new List<Worker>() {};
                Workers.AddRange(LocalGameManager.Instance.WorkerManager.GetWorkers());
                Workers.Sort(new Worker.Sort() { WorkType = ProNode.ExpType});
                if (Worker != null)
                {
                    Workers.Remove(Worker);
                    Workers.Insert(0, Worker);
                }

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
                    // Icon
                    item.transform.Find("Icon").GetComponent<Image>().sprite = WorkerIcon;
                    // Bar1
                    var bar1 = item.transform.Find("Bar1").GetComponent<Image>();
                    bar1.fillAmount = (float)worker.APCurrent / worker.APMax;
                    bar1.color = GetAPBarColor(worker.APCurrent);
                    // Name
                    item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = worker.Name;
                    // Gender
                    item.transform.Find("Gender").GetComponent<Image>().sprite = worker.Gender == Gender.Male ? WorkerMaleIcon : WorkerFemalIcon;
                    // Eff
                    if (worker.Eff.ContainsKey(ProNode.ExpType))
                    {
                        bool effEnable = worker.Eff[ProNode.ExpType] > 0;
                        item.transform.Find("Eff").gameObject.SetActive(effEnable);
                        item.transform.Find("EffZero").gameObject.SetActive(!effEnable);
                        item.transform.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + worker.Eff[ProNode.ExpType].ToString() + '%';
                        string workType = ProNode.ExpType.ToString();
                        if (!tempSprite.ContainsKey(workType))
                        {
                            var sprite = LocalGameManager.Instance.WorkerManager.GetSprite(workType);
                            tempSprite[workType] = sprite != null ? sprite : EmptySprite;
                        }
                        item.transform.Find("IconWorkType").GetComponent<Image>().sprite = tempSprite[workType];
                    }
                    if (worker.Skill.ContainsKey(ProNode.ExpType))
                    {
                        GridLayoutGroup gridLayout = item.transform.Find("Level").GetComponent<GridLayoutGroup>();
                        Transform template = item.transform.Find("Level").Find("Icon");
                        int num = GetExpRateIconNum(worker.ExpRate[ProNode.ExpType]);
                        Image[] images = item.transform.Find("Level").GetComponentsInChildren<Image>();
                        int cnt = 0;
                        foreach (var image in images)
                        {
                            image.transform.gameObject.SetActive(cnt < num);
                            if (cnt < num)
                            {
                                cnt++;
                            }
                        }
                        while (cnt < num)
                        {
                            var uiitem = Instantiate(template, gridLayout.transform, false);
                            uiitem.gameObject.SetActive(true);
                            cnt++;
                        }
                        LayoutRebuilder.ForceRebuildLayoutImmediate(gridLayout.GetComponent<RectTransform>());
                    }
                    // Selected
                    bool isCurrentWorker = Worker == worker;
                    item.transform.Find("StateCur").gameObject.SetActive(isCurrentWorker);
                    item.transform.Find("StateWork").gameObject.SetActive(!isCurrentWorker && (worker.HasProNode || worker.HasTransport));
                    item.transform.Find("StateRelax").gameObject.SetActive(!isCurrentWorker && !worker.HasProNode && !worker.HasTransport);

                    bool isSelected = CurrentWorker == worker;
                    item.transform.Find("Selected").gameObject.SetActive(isSelected);
                    if (isSelected)
                    {
                        cur = item;
                    }
                    if (i == lastWorkerIndex)
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
                    Worker_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(Worker_GridLayout.GetComponent<RectTransform>());
                #endregion
                #endregion
            }
            else if (CurMode == Mode.Upgrade)
            {
                #region Build
                // Icon
                string buildCID = ProNode.WorldProNode.Classification.ToString();
                string buildID = BuildingManager.Instance.GetID(buildCID);
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = CompositeManager.Instance.GetCompositonSprite(buildID);
                }
                Upgrade_Build.Find("Icon").GetComponent<Image>().sprite = tempSprite[buildID];
                // Name
                Upgrade_Build.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = BuildingManager.Instance.GetName(buildCID) ?? "";
                #endregion

                #region Raw
                bool flagUpgradeBtn = true;
                List<Formula> raw = BuildingManager.Instance.GetUpgradeRaw(buildCID);
                int delta = tempUIItemsUpgrade.Count - raw.Count;
                if (delta > 0)
                {
                    for (int i = 0; i < delta; ++i)
                    {
                        tempUIItemsUpgrade[tempUIItemsUpgrade.Count - 1 - i].SetActive(false);
                    }
                }
                else if (delta < 0)
                {
                    delta = -delta;
                    for (int i = 0; i < delta; ++i)
                    {
                        var uiitem = Instantiate(Upgrade_UIItemTemplate, Upgrade_GridLayout.transform, false);
                        tempUIItemsUpgrade.Add(uiitem.gameObject);
                    }
                }
                for (int i = 0; i < raw.Count; ++i)
                {
                    var uiItemData = tempUIItemsUpgrade[i];
                    string itemID = raw[i].id;
                    int need = raw[i].num;
                    int current = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).InventoryItemAmount(itemID);
                    // Active
                    uiItemData.SetActive(true);
                    // 更新Icon
                    var img = uiItemData.transform.Find("Icon").GetComponent<Image>();
                    if (ItemManager.Instance.IsValidItemID(itemID))
                    {
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            var sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite[itemID] = sprite;
                            img.sprite = sprite;
                        }
                        else
                        {
                            img.sprite = tempSprite[itemID];
                        }
                    }
                    else
                    {
                        img.sprite = EmptySprite;
                    }

                    uiItemData.transform.Find("Background3").gameObject.SetActive(current < need);

                    var nametext = uiItemData.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    var amounttext = uiItemData.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    var needtext = uiItemData.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                    uiItemData.transform.Find("Background3").gameObject.SetActive(current < need);
                    if (current < need)
                    {
                        flagUpgradeBtn = false;
                    }
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
                LayoutRebuilder.ForceRebuildLayoutImmediate(Upgrade_GridLayout.GetComponent<RectTransform>());

                Upgrade_UI.Find("BtnBackground1").gameObject.SetActive(flagUpgradeBtn);
                #endregion

                #region Level
                Upgrade_LvOld.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: " + ProNode.Level.ToString();
                Upgrade_LvOld.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textLvDesc + ProNode.EffBase + "%";

                if (this.ProNode.Level + 1 <= this.ProNode.LevelMax)
                {
                    Upgrade_UI.Find("BtnBackground").gameObject.SetActive(true);
                    Upgrade_UI.Find("KT_UpgradeConfirm").gameObject.SetActive(true);

                    Upgrade_Build.Find("Image").gameObject.SetActive(true);
                    Upgrade_LvNew.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: " + (ProNode.Level + 1).ToString();
                    Upgrade_LvNew.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textLvDesc + (ProNode.EffBase + ProNode.LevelUpgradeEff[ProNode.Level + 1]) + "%";
                }
                else
                {
                    Upgrade_UI.Find("BtnBackground").gameObject.SetActive(false);
                    Upgrade_UI.Find("BtnBackground1").gameObject.SetActive(false);
                    Upgrade_UI.Find("KT_UpgradeConfirm").gameObject.SetActive(false);

                    Upgrade_Build.Find("Image").gameObject.SetActive(false);
                    Upgrade_LvOld.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: MAX";
                    Upgrade_LvNew.gameObject.SetActive(false);
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
            public TextContent textTime;
            public TextContent textEff;
            public TextContent textLvDesc;

            public KeyTip Upgrade;
            public KeyTip NextPriority;
            public KeyTip UpgradeConfirm;
            public KeyTip ChangeRecipe;
            public KeyTip Remove1;
            public KeyTip Remove10;
            public KeyTip FastAdd;
            public KeyTip ChangeWorker;
            public KeyTip RemoveWorker;
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

        protected override void InitObjectPool()
        {
            this.objectPool.RegisterPool(ObjectPool.HandleType.Texture2D, "Texture2DPool", 1,
            "OC/UI/ResonanceWheel/Texture/SA_ResonanceWheel_UI.spriteatlasv2", (handle) =>
            {
                SpriteAtlas resonanceWheelAtlas = handle.Result as SpriteAtlas;
                WorkerIcon = resonanceWheelAtlas.GetSprite("icon_beast");
                WorkerMaleIcon = resonanceWheelAtlas.GetSprite("icon_gendermale");
                WorkerFemalIcon = resonanceWheelAtlas.GetSprite("icon_genderfemale");
            }
            );
            base.InitObjectPool();
        }
        #endregion

        #region Action
        private void OnProduceTimerUpdateAction(double time)
        {
            ProNode_Product.Find("Mask").GetComponent<Image>().fillAmount = 1 - (float)(time / ProNode.TimeCost);
        }

        public void RefreshDynamic()
        {
            if (!ProNode.HasRecipe || ProNode.State != ProNodeState.Production)
            {
                ProNode_Product.Find("Mask").GetComponent<Image>().fillAmount = 0;
            }
            if (ProNode.HasRecipe)
            {
                ProNode_Product.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.GetItemAllNum(ProNode.Recipe.ProductID).ToString();
                for (int i = 0; i < Raws.Count; ++i)
                {
                    if (i < tempUIItems.Count)
                    {
                        GameObject item = tempUIItems[i];
                        var amount = item.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                        amount.text = ProNode.GetItemAllNum(Raws[i].id).ToString();

                        // NeedAmount
                        var needAmount = item.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                        needAmount.text = ProNode.Recipe.GetRawNum(Raws[i].id).ToString();

                        if (int.Parse(amount.text) < int.Parse(needAmount.text))
                        {
                            item.transform.Find("Back").GetComponent<Image>().color = Color.red;
                        }
                        else
                        {
                            item.transform.Find("Back").GetComponent<Image>().color = Color.black;
                        }
                    }
                }
            }
            if (ProNode.HasWorker)
            {
                var onDuty = ProNode_Worker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();
                onDuty.text = PanelTextContent.workerStatus[(int)Worker.Status];
                ProNode_Eff.Find("EffPrefix").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEff;
                ProNode_Eff.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + ProNode.Eff.ToString() + "%";
                string buildCID = ProNode.WorldProNode.Classification.ToString();
                string buildID = BuildingManager.Instance.GetID(buildCID);
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = CompositeManager.Instance.GetCompositonSprite(buildID);
                }
                ProNode_Eff.Find("IconProNode").GetComponent<Image>().sprite = tempSprite[buildID];
                ProNode_Eff.Find("EffProNode").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + ProNode.EffBase.ToString() + "%";
                ProNode_Eff.Find("IconWorker").GetComponent<Image>().sprite = WorkerIcon;
                ProNode_Eff.Find("EffWorker").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + (ProNode.Eff - ProNode.EffBase).ToString() + "%";

                var bar1 = ProNode_Worker.Find("Bar1").GetComponent<Image>();
                bar1.fillAmount = (float)Worker.APCurrent / Worker.APMax;
                bar1.color = GetAPBarColor(Worker.APCurrent);
            }
        }
        #endregion
    }
}
