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
            KT_NextPriority.img = priority.Find("KT_NextPriority").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_NextPriority.keytip = KT_NextPriority.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            PriorityUrgency = priority.Find("Urgency");
            PriorityNormal = priority.Find("Normal");
            PriorityAlternative = priority.Find("Alternative");
            // ProNode Recipe
            Transform recipe = transform.Find("ProNode").Find("Recipe");
            Product = recipe.Find("Product");
            Raw_GridLayout = recipe.Find("Raw").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Raw_UIItemTemplate = Raw_GridLayout.transform.Find("UIItemTemplate");
            Raw_UIItemTemplate.gameObject.SetActive(false);
            // ProNode Worker
            UIWorker = transform.Find("ProNode").Find("Worker");
            // ProNode Eff
            Transform eff = transform.Find("ProNode").Find("Eff");
            Text_Eff = eff.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>();
            Text_EffProNode = eff.Find("EffProNode").GetComponent<TMPro.TextMeshProUGUI>();
            Text_EffWorker = eff.Find("EffWorker").GetComponent<TMPro.TextMeshProUGUI>();
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
            ChangeLevel = transform.Find("LevelUp");
            Level_GridLayout = ChangeLevel.Find("Build").Find("Raw").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            Level_UIItemTemplate = Level_GridLayout.transform.Find("UIItemTemplate");
            Level_UIItemTemplate.gameObject.SetActive(false);
            ChangeLevel.gameObject.SetActive(false);

            // BotKeyTips
            BotKeyTips_ProNode = this.transform.Find("BotKeyTips").Find("KeyTips");

            KT_ChangeRecipe = new UIKeyTip();
            KT_ChangeRecipe.img = BotKeyTips_ProNode.Find("KT_ChangeRecipe").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_ChangeRecipe.keytip = KT_ChangeRecipe.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ChangeRecipe.description = KT_ChangeRecipe.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Remove1 = new UIKeyTip();
            KT_Remove1.img = BotKeyTips_ProNode.Find("KT_Remove1").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_Remove1.keytip = KT_Remove1.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Remove1.description = KT_Remove1.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Remove10 = new UIKeyTip();
            KT_Remove10.img = BotKeyTips_ProNode.Find("KT_Remove10").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_Remove10.keytip = KT_Remove10.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Remove10.description = KT_Remove10.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_FastAdd = new UIKeyTip();
            KT_FastAdd.img = BotKeyTips_ProNode.Find("KT_FastAdd").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_FastAdd.keytip = KT_FastAdd.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_FastAdd.description = KT_FastAdd.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Back = new UIKeyTip();
            KT_Back.img = BotKeyTips_ProNode.Find("KT_Back").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_Back.keytip = KT_Back.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Back.description = KT_Back.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_ChangeWorker = new UIKeyTip();
            KT_ChangeWorker.img = BotKeyTips_ProNode.Find("KT_ChangeWorker").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_ChangeWorker.keytip = KT_ChangeWorker.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ChangeWorker.description = KT_ChangeWorker.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_RemoveWorker = new UIKeyTip();
            KT_RemoveWorker.img = BotKeyTips_ProNode.Find("KT_RemoveWorker").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_RemoveWorker.keytip = KT_RemoveWorker.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_RemoveWorker.description = KT_RemoveWorker.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();
            // BotKeyTips ChangeRecipe
            BotKeyTips_Recipe = this.transform.Find("BotKeyTips").Find("ChangeRecipe");

            KT_ConfirmRecipe = new UIKeyTip();
            KT_ConfirmRecipe.img = BotKeyTips_Recipe.Find("KT_ConfirmRecipe").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_ConfirmRecipe.keytip = KT_ConfirmRecipe.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ConfirmRecipe.description = KT_ConfirmRecipe.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_BackRecipe = new UIKeyTip();
            KT_BackRecipe.img = BotKeyTips_Recipe.Find("KT_BackRecipe").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_BackRecipe.keytip = KT_BackRecipe.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_BackRecipe.description = KT_BackRecipe.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            BotKeyTips_Recipe.gameObject.SetActive(false);
            // BotKeyTips ChangeWorker
            BotKeyTips_Worker = this.transform.Find("BotKeyTips").Find("ChangeWorker");

            KT_ConfirmWorker = new UIKeyTip();
            KT_ConfirmWorker.img = BotKeyTips_Worker.Find("KT_ConfirmWorker").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_ConfirmWorker.keytip = KT_ConfirmWorker.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ConfirmWorker.description = KT_ConfirmWorker.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_BackWorker = new UIKeyTip();
            KT_BackWorker.img = BotKeyTips_Worker.Find("KT_BackWorker").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_BackWorker.keytip = KT_BackWorker.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_BackWorker.description = KT_BackWorker.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            BotKeyTips_Worker.gameObject.SetActive(false);
            // BotKeyTips LevelUp
            BotKeyTips_Level = this.transform.Find("BotKeyTips").Find("LevelUp");

            KT_ConfirmLevel = new UIKeyTip();
            KT_ConfirmLevel.img = BotKeyTips_Level.Find("KT_ConfirmLevel").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_ConfirmLevel.keytip = KT_ConfirmLevel.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ConfirmLevel.description = KT_ConfirmLevel.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_BackLevel = new UIKeyTip();
            KT_BackLevel.img = BotKeyTips_Level.Find("KT_BackLevel").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_BackLevel.keytip = KT_BackLevel.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_BackLevel.description = KT_BackLevel.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();
            BotKeyTips_Level.gameObject.SetActive(false);

            CurPriority = MissionNS.TransportPriority.Normal;
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
        private enum Mode
        {
            ProNode = 0,
            ChangeRecipe = 1,
            ChangeWorker = 2,
            ChangeLevel = 3,
        }
        private Mode CurMode = Mode.ProNode;
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
                if(Raws != null && Raws.Count > 0)
                {
                    currentRawIndex = value;
                    if(currentRawIndex == -1)
                    {
                        currentRawIndex = Raws.Count - 1;
                    }
                    else if(currentRawIndex == Raws.Count)
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
                if(last != currentRawIndex)
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
                if(Raws != null && CurrentRawIndex < Raws.Count)
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

        private Player.PlayerCharacter Player => GameObject.Find("PlayerCharacter")?.GetComponent<Player.PlayerCharacter>();

        private void Enter()
        {
            ProNode.OnActionChange += RefreshDynamic;
            ProNode.OnProduceTimerUpdate += OnProduceTimerUpdateAction;
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Enable();
            //ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(0);
            this.Refresh();
        }

        private void Exit()
        {
            ProNode.OnActionChange -= RefreshDynamic;
            ProNode.OnProduceTimerUpdate -= OnProduceTimerUpdateAction;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Disable();
            //ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(1);
            this.UnregisterInput();
        }

        private void UnregisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed -= Confirm_performed;
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

        private void RegisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed += Confirm_performed;
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
                CurMode = Mode.ProNode;
            }
            else if (CurMode == Mode.ChangeWorker)
            {
                ProNode.ChangeWorker(CurrentWorker);
                CurMode = Mode.ProNode;
            }
            else if (CurMode == Mode.ChangeLevel)
            {
                CurMode = Mode.ProNode;
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
            if (CurMode == Mode.ProNode)
            {
                CurMode = Mode.ChangeWorker;
                Refresh();
            }
        }
        private void RemoveWorker_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ChangeWorker)
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
            foreach(var s in tempSprite)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItems)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItemsRecipe)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItemsRecipeRaw)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItemsWorker)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItemsLevel)
            {
                Destroy(s);
            }
        }
        #endregion

        #region UI对象引用
        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;
        private TMPro.TextMeshProUGUI Text_Eff;
        private TMPro.TextMeshProUGUI Text_EffProNode;
        private TMPro.TextMeshProUGUI Text_EffWorker;

        private UIKeyTip KT_Upgrade;
        private UIKeyTip KT_NextPriority;
        private UIKeyTip KT_ChangeRecipe;
        private UIKeyTip KT_Remove1;
        private UIKeyTip KT_Remove10;
        private UIKeyTip KT_FastAdd;
        private UIKeyTip KT_Back;
        private UIKeyTip KT_ChangeWorker;
        private UIKeyTip KT_RemoveWorker;
        private UIKeyTip KT_ConfirmRecipe;
        private UIKeyTip KT_BackRecipe;
        private UIKeyTip KT_ConfirmWorker;
        private UIKeyTip KT_BackWorker;
        private UIKeyTip KT_ConfirmLevel;
        private UIKeyTip KT_BackLevel;

        private Transform Priority;
        private Transform PriorityUrgency;
        private Transform PriorityNormal;
        private Transform PriorityAlternative;
        private Transform Product;
        private Transform UIWorker;

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

        private Transform ChangeRecipe;
        private Transform ChangeWorker;
        private Transform ChangeLevel;
        private Transform BotKeyTips_ProNode;
        private Transform BotKeyTips_Recipe;
        private Transform BotKeyTips_Worker;
        private Transform BotKeyTips_Level;
        #endregion
        public void Refresh()
        {
            // 加载完成JSON数据 & 查找完所有引用
            if(ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
            }
            if (CurMode == Mode.ProNode)
            {
                this.ChangeRecipe.gameObject.SetActive(false);
                this.ChangeWorker.gameObject.SetActive(false);
                this.ChangeLevel.gameObject.SetActive(false);
                this.BotKeyTips_ProNode.gameObject.SetActive(true);
                this.BotKeyTips_Recipe.gameObject.SetActive(false);
                this.BotKeyTips_Worker.gameObject.SetActive(false);
                this.BotKeyTips_Level.gameObject.SetActive(false);
                this.Raw_UIItemTemplate.gameObject.SetActive(false);

                #region ProNode
                if (ProNode.Recipe != null)
                {
                    #region TopTitle
                    foreach (TextTip tp in PanelTextContent.proNodeType)
                    {
                        if (tp.name == ProNode.Category.ToString())
                        {
                            Text_Title.text = tp.GetDescription();
                            break;
                        }
                    }
                    KT_Upgrade.ReWrite(PanelTextContent.ktUpgrade);
                    KT_NextPriority.ReWrite(PanelTextContent.ktNextPriority);
                    #endregion

                    #region Product
                    string productID = ProNode.Recipe.ProductID;
                    var nameProduct = Product.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    nameProduct.text = ItemManager.Instance.GetItemName(productID);
                    //var imgProduct = Product.transform.Find("Icon").GetComponent<Image>();
                    //var spriteProduct = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(productID));
                    //if (spriteProduct == null)
                    //{
                    //    spriteProduct = ItemManager.Instance.GetItemSprite(productID);
                    //    tempSprite.Add(spriteProduct);
                    //}
                    //imgProduct.sprite = spriteProduct;
                    var amountProduct = Product.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    amountProduct.text = ProNode.GetItemAllNum(productID).ToString();

                    var tiemProduct = Product.transform.Find("Time").GetComponent<TMPro.TextMeshProUGUI>();
                    tiemProduct.text = "Time: " + LocalGameManager.Instance.RecipeManager.GetTimeCost(ProNode.Recipe.ID).ToString();
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
                        //// 更新Icon
                        //var img = item.transform.Find("Icon").GetComponent<Image>();
                        //// 查找临时存储的Sprite
                        //var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(itemID));
                        //// 不存在则生成
                        //if (sprite == null)
                        //{
                        //    sprite = ItemManager.Instance.GetItemSprite(itemID);
                        //    tempSprite.Add(sprite);
                        //}
                        //img.sprite = sprite;
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
                #region Worker
                if (Worker != null)
                {
                    var name = UIWorker.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    name.text = Worker.Name;
                    var img = UIWorker.transform.Find("Icon").GetComponent<Image>();
                    WorkerManager workerManager = ManagerNS.LocalGameManager.Instance.WorkerManager;
                    //var sprite = tempSprite.Find(s => s.texture == workerManager.GetTexture2D());
                    //if (sprite == null)
                    //{
                    //    sprite = workerManager.GetSprite();
                    //    tempSprite.Add(sprite);
                    //}
                    //img.sprite = sprite;
                    var onDuty = UIWorker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();
                    switch (Worker.Status)
                    {
                        case Status.Relaxing:
                            onDuty.text = PanelTextContent.textWorkerStateRelax;
                            break;
                        case Status.Fishing:
                            onDuty.text = PanelTextContent.textWorkerStateFish;
                            if (Worker.IsOnDuty)
                            {
                                onDuty.text = PanelTextContent.textWorkerOnDuty;
                            }
                            break;
                        case Status.Working:
                            onDuty.text = PanelTextContent.textWorkerStateWork;
                            break;
                    }
                    var rect = UIWorker.transform.Find("PrograssBar").Find("Cur").GetComponent<RectTransform>();
                    rect.offsetMax = new Vector2(rect.offsetMax.x, -1 * (int)(100 - 100 * Worker.APCurrent / Worker.APMax));
                    UIWorker.transform.Find("AP").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Worker.APCurrent}/{Worker.APMax}";
                }
                #endregion
                #region Eff
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
                    //// 更新Icon
                    //var img = item.transform.Find("Icon").GetComponent<Image>();
                    //// 查找临时存储的Sprite
                    //var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(product.id));
                    //// 不存在则生成
                    //if (sprite == null)
                    //{
                    //    sprite = ItemManager.Instance.GetItemSprite(product.id);
                    //    tempSprite.Add(sprite);
                    //}
                    //img.sprite = sprite;
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
                //var imgProduct = Product.transform.Find("Icon").GetComponent<Image>();
                //var spriteProduct = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(product.id));
                //if (spriteProduct == null)
                //{
                //    spriteProduct = ItemManager.Instance.GetItemSprite(product.id);
                //    tempSprite.Add(spriteProduct);
                //}
                //imgProduct.sprite = spriteProduct;
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
                    //// 更新Icon
                    //var img = item.transform.Find("Icon").GetComponent<Image>();
                    //var sprite = tempSprite.Find(s => s.texture == ItemManager.Instance.GetItemTexture2D(itemID));
                    //if (sprite == null)
                    //{
                    //    sprite = ItemManager.Instance.GetItemSprite(itemID);
                    //    tempSprite.Add(sprite);
                    //}
                    //img.sprite = sprite;
                }
                Recipe_Raw_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
                // 强制立即更新 GridLayoutGroup 的布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(Recipe_Raw_GridLayout.GetComponent<RectTransform>());
                #endregion

                #region BotKeyTips
                KT_ConfirmRecipe.ReWrite(PanelTextContent.ktConfirmRecipe);
                KT_BackRecipe.ReWrite(PanelTextContent.ktBackRecipe);
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
                        switch (worker.Status)
                        {
                            case Status.Relaxing:
                                state.text = PanelTextContent.textWorkerStateRelax;
                                break;
                            case Status.Fishing:
                                state.text = PanelTextContent.textWorkerStateFish;
                                if (worker.IsOnDuty)
                                {
                                    state.text = PanelTextContent.textWorkerOnDuty;
                                }
                                break;
                            case Status.Working:
                                state.text = PanelTextContent.textWorkerStateWork;
                                break;
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
                        item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text  = "";
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
                KT_ConfirmWorker.ReWrite(PanelTextContent.ktConfirmWorker);
                KT_BackWorker.ReWrite(PanelTextContent.ktBackWorker);
                #endregion
            }
            else if (CurMode == Mode.ChangeLevel)
            {
                this.ChangeLevel.gameObject.SetActive(true);
                this.BotKeyTips_ProNode.gameObject.SetActive(false);
                this.BotKeyTips_Level.gameObject.SetActive(true);
                this.Level_UIItemTemplate.gameObject.SetActive(true);
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
            public TextContent textWorkerOnDuty;
            public TextContent textPrefixTime;
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
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ProNodePanel>("Binary/TextContent/Inventory", "ProNodePanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI生产节点Panel数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region Action
        private void OnProduceTimerUpdateAction(double time)
        {
            RectTransform rect = Product.transform.Find("Mask").GetComponent<RectTransform>();
            float percent = 1 - (float)(time / ProNode.TimeCost);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, percent * Product.transform.Find("Icon").GetComponent<RectTransform>().sizeDelta.y);
            Product.transform.Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textPrefixTime + time.ToString("F2");
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
                    switch (Worker.Status)
                    {
                        case Status.Relaxing:
                            onDuty.text = PanelTextContent.textWorkerStateRelax;
                            break;
                        case Status.Fishing:
                            onDuty.text = PanelTextContent.textWorkerStateFish;
                            if (Worker.IsOnDuty)
                            {
                                onDuty.text = PanelTextContent.textWorkerOnDuty;
                            }
                            break;
                        case Status.Working:
                            onDuty.text = PanelTextContent.textWorkerStateWork;
                            break;
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
