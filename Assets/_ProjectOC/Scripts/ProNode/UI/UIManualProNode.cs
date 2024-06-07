using ML.Engine.TextContent;
using static ProjectOC.ProNodeNS.UI.UIManualProNode;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ML.Engine.Utility;

namespace ProjectOC.ProNodeNS.UI
{
    public class UIManualProNode : ML.Engine.UI.UIBasePanel<ProNodePanel>
    {
        #region Str
        private const string str = "";
        private const string strProNode = "ProNode";
        private const string strRecipe = "Recipe";
        private const string strRaw = "Raw";
        private const string strViewport = "Viewport";
        private const string strChangeRecipe = "ChangeRecipe";
        private const string strSelect = "Select";
        private const string strUpgrade = "Upgrade";
        private const string strSelected = "Selected";
        private const string strTopTitle = "TopTitle";
        private const string strPriority = "Priority";
        private const string strMask = "Mask";
        private const string strName = "Name";
        private const string strBack = "Back";
        private const string strBack1 = "Back1";
        private const string strAmount = "Amount";
        private const string strIcon = "Icon";
        private const string strProduct = "Product";
        private const string strText = "Text";
        private const string strDesc = "Desc";
        private const string strBuild = "Build";
        private const string strEff = "Eff";
        private const string strLevel = "Level";
        private const string strLvOld = "LvOld";
        private const string strLvNew = "LvNew";
        private const string strBotKeyTips = "BotKeyTips";
        private const string strKeyTips = "KeyTips";
        private const string strKeyTips1 = "KeyTips1";
        private const string strKT_Confirm = "KT_Confirm";
        private const string strKT_Upgrade = "KT_Upgrade";
        private const string strKT_Remove1 = "KT_Remove1";
        private const string strKT_Remove10 = "KT_Remove10";
        private const string strKT_FastAdd = "KT_FastAdd";
        private const string strNeedAmount = "NeedAmount";
        private const string strEffPrefix = "EffPrefix";
        private const string strIconProNode = "IconProNode";
        private const string strEffProNode = "EffProNode";
        private const string strAddSignal = "+";
        private const string strModSignal = "%";
        private const string strColonSignal = ":";
        private const string strs = "s";
        private const string strItemDesc = "ItemDesc";
        private const string strEffectDesc = "EffectDesc";
        private const string strTime = "Time";
        private const string strWeightIcon = "WeightIcon";
        private const string strWeight = "Weight";
        private const string str0 = "0";
        private const string strBackground3 = "Background3";
        private const string strBtnBackground = "BtnBackground";
        private const string strBtnBackground1 = "BtnBackground1";
        private const string strLv = "Lv";
        private const string strLvColon = "Lv: ";
        private const string strLvColonMax = "Lv: Max";
        private const string strImage = "Image";
        private const string strKT_UpgradeConfirm = "KT_UpgradeConfirm";
        private const string strChangeWorker = "ChangeWorker";
        private const string strWorker = "Worker";
        private const string strBar = "Bar";
        private const string strBar1 = "Bar1";
        private const string strEmpty = "Empty";
        private const string strGender = "Gender";
        private const string strOnDuty = "OnDuty";
        private const string strIconWorker = "IconWorker";
        private const string strEffWorker = "EffWorker";
        private const string strEffZero = "EffZero";
        private const string strIconWorkType = "IconWorkType";
        private const string strStateCur = "StateCur";
        private const string strStateWork = "StateWork";
        private const string strStateRelax = "StateRelax";

        private const string strKT_ChangeWorker = "KT_ChangeWorker";
        private const string strKT_RemoveWorker = "KT_RemoveWorker";
        private const string strKT_ChangeRecipe = "KT_ChangeRecipe";
        private const string strKT_Return = "KT_Return";
        private const string strTex2D_Worker_UI_GenderMale = "Tex2D_Worker_UI_GenderMale";
        private const string strTex2D_Worker_UI_GenderFemale = "Tex2D_Worker_UI_GenderFemale";
        private const string strPrefab_ProNode_UI_RawTemplate = "Prefab_ProNode_UI/Prefab_ProNode_UI_RawTemplate.prefab";
        private const string strPrefab_ProNode_UI_RecipeTemplate = "Prefab_ProNode_UI/Prefab_ProNode_UI_RecipeTemplate.prefab";
        private const string strPrefab_ProNode_UI_RecipeRawTemplate = "Prefab_ProNode_UI/Prefab_ProNode_UI_RecipeRawTemplate.prefab";
        private const string strPrefab_ProNode_UI_UpgradeRawTemplate = "Prefab_ProNode_UI/Prefab_ProNode_UI_UpgradeRawTemplate.prefab";
        private const string strPrefab_ProNode_UI_WorkerTemplate = "Prefab_ProNode_UI/Prefab_ProNode_UI_WorkerTemplate.prefab";
        #endregion

        #region Data
        #region Mode
        public enum Mode
        {
            ProNode,
            ChangeRecipe,
            ChangeWorker,
            Upgrade
        }
        private Mode curMode;
        public Mode CurMode
        {
            get => curMode;
            set
            {
                RecipeBtnList.DisableBtnList();
                WorkerBtnList.DisableBtnList();
                curMode = value;
                if (curMode == Mode.ChangeRecipe)
                {
                    RecipeBtnList.EnableBtnList();
                    if (ProNode.HasRecipe)
                    {
                        for (int i = 0; i < RecipeBtnList.BtnCnt; ++i)
                        {
                            if (Recipes[i] == ProNode.Recipe.ID)
                            {
                                RecipeBtnList.MoveIndexIUISelected(i);
                                break;
                            }
                        }
                    }
                }
                else if (curMode == Mode.ChangeWorker)
                {
                    Workers.Clear();
                    Workers.AddRange(ManagerNS.LocalGameManager.Instance.WorkerManager.GetNotBanWorkers());
                    Workers.Sort(new WorkerNS.Worker.SortForProNodeUI() { WorkType = ProNode.ExpType });
                    if (Worker != null)
                    {
                        Workers.Remove(Worker);
                        Workers.Insert(0, Worker);
                    }
                    UpdateBtnInfo();
                    WorkerBtnList.EnableBtnList();
                }
            }
        }
        public enum ProNodeSelectMode
        {
            Recipe,
            Worker
        }
        public ProNodeSelectMode CurProNodeMode;
        #endregion

        public bool HasUpgrade;
        public ManualProNode ProNode;
        public WorkerNS.Worker Worker => ProNode.Worker;
        private List<string> Recipes = new List<string>();
        private List<WorkerNS.Worker> Workers = new List<WorkerNS.Worker>();

        #region BtnList
        private ML.Engine.UI.UIBtnList RawBtnList;
        private ML.Engine.UI.UIBtnList RecipeBtnList;
        private int RecipeIndex => RecipeBtnList?.GetCurSelectedPos1() ?? 0;
        private ML.Engine.UI.UIBtnList RecipeRawBtnList;
        private ML.Engine.UI.UIBtnList WorkerBtnList;
        private int WrokerIndex => WorkerBtnList?.GetCurSelectedPos1() ?? 0;
        private ML.Engine.UI.UIBtnList UpgradeBtnList;

        private bool IsInitBtnList;
        protected override void InitBtnInfo()
        {
            Synchronizer synchronizer = new Synchronizer(5, () => { IsInitBtnList = true; Refresh(); });

            RawBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strProNode).Find(strRecipe).Find(strRaw).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            int num = ProNode.HasRecipe ? ProNode.Recipe.Raw.Count : 0;
            RawBtnList.ChangBtnNum(num, strPrefab_ProNode_UI_RawTemplate, () => { synchronizer.Check(); });

            Recipes = new List<string>() { str };
            Recipes.AddRange(ProNode.GetCanProduceRecipe());
            RecipeBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strChangeRecipe).Find(strSelect).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            RecipeBtnList.ChangBtnNum(Recipes.Count, strPrefab_ProNode_UI_RecipeTemplate, () => { synchronizer.Check(); });

            RecipeRawBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strChangeRecipe).Find(strRecipe).Find(strRaw).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            RecipeRawBtnList.ChangBtnNum(0, strPrefab_ProNode_UI_RecipeRawTemplate,
                () => { synchronizer.Check(); RecipeBtnList.OnSelectButtonChanged += () => { UpdateBtnInfo(); Refresh(); }; });

            WorkerBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strChangeWorker).Find(strSelect).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            WorkerBtnList.OnSelectButtonChanged += () => { Refresh(); };
            WorkerBtnList.ChangBtnNum(0, strPrefab_ProNode_UI_WorkerTemplate, () => { synchronizer.Check(); });

            num = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(ProNode.WorldProNode.Classification.ToString()).Count;
            UpgradeBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strUpgrade).Find(strRaw).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            UpgradeBtnList.ChangBtnNum(num, strPrefab_ProNode_UI_UpgradeRawTemplate, () => { synchronizer.Check(); });

        }
        protected void UpdateBtnInfo()
        {
            if (!IsInitBtnList) return;
            IsInitBtnList = false;
            Synchronizer synchronizer = new Synchronizer(4, () => { IsInitBtnList = true; Refresh(); });
            int num = ProNode.HasRecipe ? ProNode.Recipe.Raw.Count : 0;
            if (RawBtnList != null && RawBtnList.BtnCnt != num)
            {
                RawBtnList.ChangBtnNum(num, strPrefab_ProNode_UI_RawTemplate, () => { synchronizer.Check(); });
            }
            else
            {
                synchronizer.Check();
            }
            num = ManagerNS.LocalGameManager.Instance.RecipeManager.GetRaw(Recipes[RecipeIndex]).Count;
            if (RecipeRawBtnList != null && RecipeRawBtnList.BtnCnt != num)
            {
                RecipeRawBtnList.ChangBtnNum(num, strPrefab_ProNode_UI_RecipeRawTemplate, () => { synchronizer.Check(); });
            }
            else
            {
                synchronizer.Check();
            }
            num = Workers.Count > 0 ? Workers.Count : 0;
            if (WorkerBtnList != null && WorkerBtnList.BtnCnt != num)
            {
                WorkerBtnList.ChangBtnNum(num, strPrefab_ProNode_UI_WorkerTemplate, () => { synchronizer.Check(); });
            }
            else
            {
                synchronizer.Check();
            }
            num = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(ProNode.WorldProNode.Classification.ToString()).Count;
            if (UpgradeBtnList != null && UpgradeBtnList.BtnCnt != num)
            {
                UpgradeBtnList.ChangBtnNum(num, strPrefab_ProNode_UI_UpgradeRawTemplate, () => { synchronizer.Check(); });
            }
            else
            {
                synchronizer.Check();
            }
        }
        #endregion

        private MissionNS.TransportPriority CurPriority
        {
            get => ProNode.TransportPriority;
            set
            {
                if (ProNode_Priority != null)
                {
                    ProNode_Priority.Find(strSelected).gameObject.SetActive(false);
                }
                ProNode.TransportPriority = value;
                Text_Priority.text = PanelTextContent.TransportPriority[(int)ProNode.TransportPriority];
                ProNode_Priority = transform.Find(strTopTitle).Find(strPriority).GetChild((int)ProNode.TransportPriority);
                ProNode_Priority.Find(strSelected).gameObject.SetActive(true);
            }
        }

        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();

        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;
        private Sprite WorkerMaleIcon;
        private Sprite WorkerFemalIcon;

        private Transform ProNode_Priority;
        private Transform ProNode_Product;
        private Transform ProNode_Worker;
        private Transform ProNode_Eff;
        private Transform Recipe_UI;
        private Transform Recipe_Desc;
        private Transform Upgrade_UI;
        private Transform Upgrade_Build;
        private Transform Upgrade_LvOld;
        private Transform Upgrade_LvNew;
        private Transform BotKeyTips;
        private Transform BotKeyTips1;

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
            abpath = "OCTextContent/ProNode";
            abname = "ManualProNodePanel";
            description = "ManualProNodePanel数据加载完成";
        }
        #endregion

        private bool ItemIsDestroyed = false;
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            #region TopTitle
            Text_Title = transform.Find(strTopTitle).Find(strText).GetComponent<TMPro.TextMeshProUGUI>();
            Text_Priority = transform.Find(strTopTitle).Find(strPriority).Find(strText).GetComponent<TMPro.TextMeshProUGUI>();
            #endregion
            #region ProNode
            ProNode_Product = transform.Find(strProNode).Find(strRecipe).Find(strProduct);
            ProNode_Worker = transform.Find(strProNode).Find(strWorker);
            ProNode_Eff = transform.Find(strProNode).Find(strEff);
            #endregion
            #region ChangeRecipe
            Recipe_UI = transform.Find(strChangeRecipe);
            Recipe_Desc = Recipe_UI.Find(strDesc);
            #endregion
            #region Upgrade
            Upgrade_UI = transform.Find(strUpgrade);
            Upgrade_UI.gameObject.SetActive(false);
            Upgrade_Build = Upgrade_UI.Find(strBuild);
            Upgrade_LvOld = Upgrade_UI.Find(strLevel).Find(strLvOld);
            Upgrade_LvNew = Upgrade_UI.Find(strLevel).Find(strLvNew);
            #endregion
            #region BotKeyTips
            BotKeyTips = transform.Find(strBotKeyTips).Find(strKeyTips);
            BotKeyTips1 = transform.Find(strBotKeyTips).Find(strKeyTips1);
            BotKeyTips1.gameObject.SetActive(false);
            #endregion
            IsInit = true;
        }
        #endregion

        #region Override
        protected override void Enter()
        {
            tempSprite.Add(str, transform.Find(strProNode).Find(strRecipe).Find(strProduct).Find(strIcon).GetComponent<Image>().sprite);
            ProNode.OnDataChangeEvent += RefreshDynamic;
            ProNode.OnProduceUpdateEvent += OnProduceTimerUpdateAction;
            ProNode.OnProduceEndEvent += Refresh;
            ManagerNS.LocalGameManager.Instance.WorkerManager.OnDeleteWorkerEvent += OnDeleteWorkerEvent;
            WorkerMaleIcon = ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(strTex2D_Worker_UI_GenderMale);
            WorkerFemalIcon = ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(strTex2D_Worker_UI_GenderFemale);
            base.Enter();
        }

        protected override void Exit()
        {
            ProNode.OnDataChangeEvent -= RefreshDynamic;
            ProNode.OnProduceUpdateEvent -= OnProduceTimerUpdateAction;
            ProNode.OnProduceEndEvent -= Refresh;
            ManagerNS.LocalGameManager.Instance.WorkerManager.OnDeleteWorkerEvent -= OnDeleteWorkerEvent;
            tempSprite.Remove(str);
            foreach (var s in tempSprite.Values.ToArray())
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            tempSprite.Clear();
            ML.Engine.Manager.GameManager.DestroyObj(WorkerMaleIcon);
            ML.Engine.Manager.GameManager.DestroyObj(WorkerFemalIcon);
            base.Exit();
        }
        #endregion

        #region Internal
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
            RecipeBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
            WorkerBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
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
            CurPriority = (MissionNS.TransportPriority)(((int)ProNode.TransportPriority + 1) % System.Enum.GetValues(typeof(MissionNS.TransportPriority)).Length);
        }
        private void Alter_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                var f_offset = obj.ReadValue<Vector2>();
                var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
                if (offset.x > 0)
                {
                    CurProNodeMode = ProNodeSelectMode.Worker;
                }
                else if (offset.x < 0)
                {
                    CurProNodeMode = ProNodeSelectMode.Recipe;
                }
                Refresh();
            }
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                if (CurProNodeMode == ProNodeSelectMode.Worker)
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
                ProNode.ChangeRecipe(Recipes[RecipeIndex]);
                UpdateBtnInfo();
                CurMode = Mode.ProNode;
            }
            else if (CurMode == Mode.ChangeWorker && Workers.Count > 0)
            {
                (ProNode as WorkerNS.IWorkerContainer).SetWorker(Workers[WrokerIndex]);
                CurMode = Mode.ProNode;
            }
            else if (CurMode == Mode.Upgrade)
            {
                ML.Engine.BuildingSystem.BuildingManager.Instance.Upgrade(ProNode.WorldProNode);
                UpdateBtnInfo();
            }
            Refresh();
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                string id = ProNode.HasRecipe ? ProNode.Recipe.Product.id : "";
                ML.Engine.InventorySystem.ItemManager.Instance.AddItemIconObject(id, ProNode.WorldProNode.transform,
                    new Vector3(0, ProNode.WorldProNode.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0), Quaternion.Euler(Vector3.zero), Vector3.one,
                    (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
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
                if (ItemIsDestroyed)
                {
                    ItemIsDestroyed = false;
                }
                else
                {
                    ProNode.Remove(0, 1);
                    Refresh();
                }
            }
        }
        private void Remove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode && CurProNodeMode == ProNodeSelectMode.Recipe)
            {
                ItemIsDestroyed = true;
                int num = ProNode.Stack;
                num = num < 10 ? num : 10;
                ProNode.Remove(0, num);
                Refresh();
            }
        }
        private void FastAdd_RemoveWorker_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.ProNode)
            {
                if (CurProNodeMode == ProNodeSelectMode.Worker)
                {
                    (ProNode as WorkerNS.IWorkerContainer).RemoveWorker();
                }
                else
                {
                    ProNode.FastAdd();
                }
            }
            Refresh();
        }
        #endregion

        #region UI
        private void SetUIActive()
        {
            Recipe_UI.gameObject.SetActive(CurMode == Mode.ChangeRecipe);
            transform.Find(strChangeWorker).gameObject.SetActive(CurMode == Mode.ChangeWorker);
            Upgrade_UI.gameObject.SetActive(CurMode == Mode.Upgrade);
            BotKeyTips.gameObject.SetActive(CurMode == Mode.ProNode);
            BotKeyTips1.gameObject.SetActive(CurMode != Mode.ProNode);
            BotKeyTips1.Find(strKT_Confirm).gameObject.SetActive(CurMode != Mode.Upgrade);
            transform.Find(strTopTitle).Find(strKT_Upgrade).gameObject.SetActive(HasUpgrade);
            if (CurMode == Mode.ProNode)
            {
                BotKeyTips.Find(strKT_ChangeWorker).gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Worker);
                BotKeyTips.Find(strKT_RemoveWorker).gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Worker && ProNode.HaveWorker);
                BotKeyTips.Find(strKT_ChangeRecipe).gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
                BotKeyTips.Find(strKT_Remove1).gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe);
                BotKeyTips.Find(strKT_Remove10).gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe);
                BotKeyTips.Find(strKT_FastAdd).gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe && ProNode.Recipe.Raw.Count > 0);
                BotKeyTips.Find(strKT_Return).gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
                LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
            }
        }
        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            CurPriority = ProNode.TransportPriority;
            SetUIActive();
            if (CurMode == Mode.ProNode)
            {
                foreach (TextTip tp in PanelTextContent.proNodeType)
                {
                    if (tp.name == ProNode.Category.ToString())
                    {
                        Text_Title.text = tp.GetDescription();
                        break;
                    }
                }
                #region Product
                ProNode_Product.Find(strSelected).gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
                bool hasRecipe = ProNode.HasRecipe;
                ProNode_Product.Find(strMask).gameObject.SetActive(hasRecipe);
                ProNode_Product.Find(strName).gameObject.SetActive(hasRecipe);
                ProNode_Product.Find(strBack).gameObject.SetActive(hasRecipe);
                ProNode_Product.Find(strBack1).gameObject.SetActive(hasRecipe);
                ProNode_Product.Find(strAmount).gameObject.SetActive(hasRecipe);

                if (!hasRecipe || ProNode.State != ProNodeState.Production)
                {
                    ProNode_Product.Find(strMask).GetComponent<Image>().fillAmount = 0;
                }

                if (hasRecipe)
                {
                    #region Product
                    string productID = ProNode.Recipe.ProductID ?? str;
                    if (!tempSprite.ContainsKey(productID))
                    {
                        tempSprite[productID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(productID);
                    }
                    ProNode_Product.Find(strIcon).GetComponent<Image>().sprite = tempSprite[productID];
                    ProNode_Product.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(productID);
                    ProNode_Product.Find(strAmount).GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.Stack.ToString();
                    ProNode_Product.Find(strBack).GetComponent<Image>().color = ProNode.StackAll >= ProNode.StackMax * ProNode.ProductNum ? new Color(113/255f, 182/255f, 4/255f) : Color.black;
                    #endregion

                    #region Raw
                    var Raws = ProNode.Recipe.Raw;
                    for (int i = 0; i < Raws.Count; ++i)
                    {
                        if (i >= RawBtnList.BtnCnt) { break; }
                        var itemID = Raws[i].id ?? str;
                        var item = RawBtnList.GetBtn(i).transform;
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                        }
                        item.Find(strIcon).GetComponent<Image>().sprite = tempSprite[itemID];
                        var needAmount = item.Find(strNeedAmount).GetComponent<TMPro.TextMeshProUGUI>();
                        needAmount.text = ProNode.Recipe.GetRawNum(itemID).ToString();
                        var amount = item.Find(strAmount).GetComponent<TMPro.TextMeshProUGUI>();
                        amount.text = ProNode.GetItemAllNum(itemID).ToString();
                        item.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(itemID);
                        if (int.Parse(amount.text) < int.Parse(needAmount.text))
                        {
                            item.Find(strBack).GetComponent<Image>().color = Color.red;
                        }
                        else
                        {
                            item.Find(strBack).GetComponent<Image>().color = Color.black;
                        }
                    }
                    #endregion
                }
                else
                {
                    ProNode_Product.Find(strIcon).GetComponent<Image>().sprite = tempSprite[str];
                }
                #endregion

                #region Worker
                ProNode_Worker.Find(strSelected).gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Worker);
                ProNode_Worker.gameObject.SetActive(true);
                bool hasWorker = ProNode.HaveWorker;
                ProNode_Worker.Find(strBar).gameObject.SetActive(hasWorker);
                ProNode_Worker.Find(strBar1).gameObject.SetActive(hasWorker);
                ProNode_Worker.Find(strImage).gameObject.SetActive(hasWorker);
                ProNode_Worker.Find(strName).gameObject.SetActive(hasWorker);
                ProNode_Worker.Find(strEmpty).gameObject.SetActive(!hasWorker);
                ProNode_Worker.Find(strGender).gameObject.SetActive(hasWorker);
                var onDuty = ProNode_Worker.Find(strOnDuty).GetComponent<TMPro.TextMeshProUGUI>();
                if (hasWorker && !tempSprite.ContainsKey(Worker.Category.ToString()))
                {
                    tempSprite.Add(Worker.Category.ToString(), ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkerProfile(Worker.Category));
                }
                ProNode_Worker.Find(strIcon).GetComponent<Image>().sprite = hasWorker ? tempSprite[Worker.Category.ToString()] : tempSprite[str];
                onDuty.text = hasWorker ? PanelTextContent.workerStatus[(int)Worker.Status] : PanelTextContent.textLack;
                if (hasWorker)
                {
                    ProNode_Worker.Find(strBar1).GetComponent<Image>().fillAmount = (float)Worker.APCurrent / Worker.APMax;
                    ProNode_Worker.Find(strBar1).GetComponent<Image>().color = ManagerNS.LocalGameManager.Instance.ProNodeManager.GetAPBarColor(Worker.APCurrent);
                    ProNode_Worker.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = Worker.Name;
                    ProNode_Worker.Find(strGender).GetComponent<Image>().sprite = Worker.Gender == Gender.Male ? WorkerMaleIcon : WorkerFemalIcon;
                }
                #endregion

                #region Eff
                ProNode_Eff.Find(strEffPrefix).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEff;
                ProNode_Eff.Find(strEff).GetComponent<TMPro.TextMeshProUGUI>().text = strAddSignal + ProNode.GetEff().ToString() + strModSignal;

                string buildID = ML.Engine.BuildingSystem.BuildingManager.Instance.GetID(ProNode.WorldProNode.Classification.ToString());
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = ML.Engine.BuildingSystem.BuildingManager.Instance.GetBuildIcon(buildID);
                }
                ProNode_Eff.Find(strIconProNode).GetComponent<Image>().sprite = tempSprite[buildID];
                ProNode_Eff.Find(strEffProNode).GetComponent<TMPro.TextMeshProUGUI>().text = strAddSignal + ProNode.EffBase.ToString() + strModSignal;
                ProNode_Eff.Find(strIconWorker).gameObject.SetActive(hasWorker);
                ProNode_Eff.Find(strEffWorker).gameObject.SetActive(hasWorker);
                if (hasWorker)
                {
                    ProNode_Eff.Find(strIconWorker).GetComponent<Image>().sprite = tempSprite[Worker.Category.ToString()];
                    ProNode_Eff.Find(strEffWorker).GetComponent<TMPro.TextMeshProUGUI>().text = strAddSignal + Worker.GetEff(ProNode.ExpType).ToString() + strModSignal;
                }
                #endregion
            }
            else if (CurMode == Mode.ChangeRecipe)
            {
                string productID;
                #region Select
                for (int i = 0; i < Recipes.Count; ++i)
                {
                    if (i >= RecipeBtnList.BtnCnt) { break; }
                    var recipeID = Recipes[i];
                    productID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(recipeID).id ?? str;
                    var item = RecipeBtnList.GetBtn(i).transform;
                    if (!tempSprite.ContainsKey(productID))
                    {
                        tempSprite[productID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(productID);
                    }
                    item.Find(strIcon).GetComponent<Image>().sprite = tempSprite[productID];
                }
                #endregion

                #region Product
                productID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(Recipes[RecipeIndex]).id ?? str;
                if (!tempSprite.ContainsKey(productID))
                {
                    tempSprite[productID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(productID);
                }
                Recipe_UI.Find(strRecipe).Find(strProduct).GetComponent<Image>().sprite = tempSprite[productID];
                Recipe_UI.Find(strRecipe).Find(strTime).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textTime + strColonSignal
                    + ManagerNS.LocalGameManager.Instance.RecipeManager.GetTimeCost(Recipes[RecipeIndex]).ToString() + strs;
                Recipe_Desc.Find(strIcon).GetComponent<Image>().sprite = tempSprite[productID];
                bool isValidItemID = ML.Engine.InventorySystem.ItemManager.Instance.IsValidItemID(productID);
                Recipe_Desc.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(productID) : PanelTextContent.textEmpty;
                Recipe_Desc.Find(strItemDesc).GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetItemDescription(productID) : PanelTextContent.textEmpty;
                Recipe_Desc.Find(strEffectDesc).GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetEffectDescription(productID) : PanelTextContent.textEmpty;
                Recipe_Desc.Find(strWeightIcon).gameObject.SetActive(isValidItemID);
                Recipe_Desc.Find(strWeight).GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetWeight(productID).ToString() : str;
                #endregion

                #region Raw
                var recipeRaws = ManagerNS.LocalGameManager.Instance.RecipeManager.GetRaw(Recipes[RecipeIndex]);
                for (int i = 0; i < recipeRaws.Count; ++i)
                {
                    if (i >= RecipeRawBtnList.BtnCnt) { break; }
                    var itemID = recipeRaws[i].id;
                    var item = RecipeRawBtnList.GetBtn(i).transform;
                    var name = item.Find(strName).GetComponent<TMPro.TextMeshProUGUI>();
                    name.text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(itemID);
                    if (name.text == str)
                    {
                        name.text = PanelTextContent.textEmpty;
                    }
                    item.Find(strNeedAmount).GetComponent<TMPro.TextMeshProUGUI>().text = recipeRaws[i].num.ToString();
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                    }
                    item.Find(strIcon).GetComponent<Image>().sprite = tempSprite[itemID];
                }
                #endregion
            }
            else if (CurMode == Mode.ChangeWorker)
            {
                for (int i = 0; i < Workers.Count; ++i)
                {
                    if (i >= WorkerBtnList.BtnCnt) { break; }
                    var worker = Workers[i];
                    var item = WorkerBtnList.GetBtn(i).transform;
                    if (!tempSprite.ContainsKey(worker.Category.ToString()))
                    {
                        tempSprite.Add(worker.Category.ToString(), ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkerProfile(worker.Category));
                    }
                    // Icon
                    item.Find(strIcon).GetComponent<Image>().sprite = tempSprite[worker.Category.ToString()];
                    // Bar1
                    var bar1 = item.Find(strBar1).GetComponent<Image>();
                    bar1.fillAmount = (float)worker.APCurrent / worker.APMax;
                    bar1.color = ManagerNS.LocalGameManager.Instance.ProNodeManager.GetAPBarColor(worker.APCurrent);
                    // Name
                    item.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = worker.Name;
                    // Gender
                    item.Find(strGender).GetComponent<Image>().sprite = worker.Gender == Gender.Male ? WorkerMaleIcon : WorkerFemalIcon;
                    // Eff
                    bool effEnable = worker.GetEff(ProNode.ExpType) > 0;
                    item.Find(strEff).gameObject.SetActive(effEnable);
                    item.Find(strEffZero).gameObject.SetActive(!effEnable);
                    item.Find(strEff).GetComponent<TMPro.TextMeshProUGUI>().text = strAddSignal + worker.GetEff(ProNode.ExpType).ToString() + '%';
                    string workType = ProNode.ExpType.ToString();
                    if (!tempSprite.ContainsKey(workType))
                    {
                        tempSprite[workType] = ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(workType);
                    }
                    item.transform.Find(strIconWorkType).GetComponent<Image>().sprite = tempSprite[workType] ?? tempSprite[str];
                    if (worker.Skill.ContainsKey(ProNode.ExpType))
                    {
                        GridLayoutGroup gridLayout = item.transform.Find(strLevel).GetComponent<GridLayoutGroup>();
                        Transform template = item.transform.Find(strLevel).Find(strIcon);
                        int num = ManagerNS.LocalGameManager.Instance.ProNodeManager.GetExpRateIconNum(worker.GetEff(ProNode.ExpType));
                        Image[] images = item.transform.Find(strLevel).GetComponentsInChildren<Image>();
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
                    item.transform.Find(strStateCur).gameObject.SetActive(isCurrentWorker);
                    item.transform.Find(strStateWork).gameObject.SetActive(!isCurrentWorker && (worker.HaveProNode || worker.HaveTransport));
                    item.transform.Find(strStateRelax).gameObject.SetActive(!isCurrentWorker && !worker.HaveProNode && !worker.HaveTransport);
                }
            }
            else if (CurMode == Mode.Upgrade)
            {
                #region Build
                // Icon
                string buildCID = ProNode.WorldProNode.Classification.ToString();
                string buildID = ML.Engine.BuildingSystem.BuildingManager.Instance.GetID(buildCID);
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = ML.Engine.BuildingSystem.BuildingManager.Instance.GetBuildIcon(buildID);
                }
                Upgrade_Build.Find(strIcon).GetComponent<Image>().sprite = tempSprite[buildID];
                // Name
                Upgrade_Build.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.BuildingSystem.BuildingManager.Instance.GetName(buildCID) ?? str;
                #endregion

                #region Raw
                bool flagUpgradeBtn = true;
                var raw = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(buildCID);
                for (int i = 0; i < raw.Count; ++i)
                {
                    if (i >= raw.Count) { break; }
                    var uiItemData = UpgradeBtnList.GetBtn(i).transform;
                    string itemID = raw[i].id;
                    int need = raw[i].num;
                    int current = ManagerNS.LocalGameManager.Instance.Player.InventoryItemAmount(itemID);
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                    }
                    uiItemData.Find(strIcon).GetComponent<Image>().sprite = tempSprite[itemID];

                    uiItemData.Find(strBackground3).gameObject.SetActive(current < need);

                    var nametext = uiItemData.Find(strName).GetComponent<TMPro.TextMeshProUGUI>();
                    var amounttext = uiItemData.Find(strAmount).GetComponent<TMPro.TextMeshProUGUI>();
                    var needtext = uiItemData.Find(strNeedAmount).GetComponent<TMPro.TextMeshProUGUI>();
                    uiItemData.Find(strBackground3).gameObject.SetActive(current < need);
                    flagUpgradeBtn = current >= need;
                    if (itemID != str)
                    {
                        nametext.text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(itemID);
                        amounttext.text = current.ToString();
                        needtext.text = need.ToString();
                    }
                    else
                    {
                        nametext.text = PanelTextContent.textEmpty;
                        amounttext.text = str0;
                        needtext.text = str0;
                    }
                }

                Upgrade_UI.Find(strBtnBackground1).gameObject.SetActive(flagUpgradeBtn);
                #endregion

                #region Level
                Upgrade_LvOld.Find(strLv).GetComponent<TMPro.TextMeshProUGUI>().text = strLvColon + ProNode.Level.ToString();
                Upgrade_LvOld.Find(strDesc).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textLvDesc + ProNode.EffBase + strModSignal;

                if (ProNode.Level + 1 <= ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.LevelMax)
                {
                    Upgrade_UI.Find(strBtnBackground).gameObject.SetActive(true);
                    Upgrade_UI.Find(strKT_UpgradeConfirm).gameObject.SetActive(true);

                    Upgrade_Build.Find(strImage).gameObject.SetActive(true);
                    Upgrade_LvNew.Find(strLv).GetComponent<TMPro.TextMeshProUGUI>().text = strLvColon + (ProNode.Level + 1).ToString();
                    Upgrade_LvNew.Find(strDesc).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textLvDesc +
                        (ProNode.EffBase + ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.LevelUpgradeEff[ProNode.Level]) + strModSignal;
                }
                else
                {
                    Upgrade_UI.Find(strBtnBackground).gameObject.SetActive(false);
                    Upgrade_UI.Find(strBtnBackground1).gameObject.SetActive(false);
                    Upgrade_UI.Find(strKT_UpgradeConfirm).gameObject.SetActive(false);

                    Upgrade_Build.Find(strImage).gameObject.SetActive(false);
                    Upgrade_LvOld.Find(strLv).GetComponent<TMPro.TextMeshProUGUI>().text = strLvColonMax;
                    Upgrade_LvNew.gameObject.SetActive(false);
                }
                #endregion
            }
        }

        private void OnProduceTimerUpdateAction(double time)
        {
            ProNode_Product.Find(strMask).GetComponent<Image>().fillAmount = 1 - (float)(time / ProNode.GetTimeCost());
        }

        public void RefreshDynamic()
        {
            if (!ProNode.HasRecipe || ProNode.State != ProNodeState.Production)
            {
                ProNode_Product.Find(strMask).GetComponent<Image>().fillAmount = 0;
            }
            if (ProNode.HasRecipe)
            {
                ProNode_Product.Find(strBack).GetComponent<Image>().color = ProNode.StackAll >= ProNode.StackMax * ProNode.ProductNum ? new Color(113/255f, 182/255f, 4/255f) : Color.black;
                ProNode_Product.Find(strAmount).GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.GetItemAllNum(ProNode.Recipe.ProductID).ToString();
                for (int i = 0; i < ProNode.Recipe.Raw.Count; ++i)
                {
                    if (i >= RawBtnList.BtnCnt) { break; }
                    Transform item = RawBtnList.GetBtn(i).transform;
                    var amount = item.Find(strAmount).GetComponent<TMPro.TextMeshProUGUI>();
                    amount.text = ProNode.GetItemAllNum(ProNode.Recipe.Raw[i].id).ToString();
                    // NeedAmount
                    var needAmount = item.Find(strNeedAmount).GetComponent<TMPro.TextMeshProUGUI>();
                    needAmount.text = ProNode.Recipe.GetRawNum(ProNode.Recipe.Raw[i].id).ToString();
                    item.Find(strBack).GetComponent<Image>().color = int.Parse(amount.text) < int.Parse(needAmount.text) ? Color.red : Color.black;
                }
            }
            if (ProNode.HaveWorker)
            {
                var onDuty = ProNode_Worker.transform.Find(strOnDuty).GetComponent<TMPro.TextMeshProUGUI>();
                onDuty.text = PanelTextContent.workerStatus[(int)Worker.Status];
                ProNode_Eff.Find(strEffPrefix).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEff;
                ProNode_Eff.Find(strEff).GetComponent<TMPro.TextMeshProUGUI>().text = strAddSignal + ProNode.GetEff().ToString() + strModSignal;
                string buildID = ML.Engine.BuildingSystem.BuildingManager.Instance.GetID(ProNode.WorldProNode.Classification.ToString());
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = ML.Engine.BuildingSystem.BuildingManager.Instance.GetBuildIcon(buildID);
                }
                ProNode_Eff.Find(strIconProNode).GetComponent<Image>().sprite = tempSprite[buildID];
                ProNode_Eff.Find(strEffProNode).GetComponent<TMPro.TextMeshProUGUI>().text = strAddSignal + ProNode.EffBase.ToString() + strModSignal;
                ProNode_Eff.Find(strIconWorker).GetComponent<Image>().sprite = tempSprite[Worker.Category.ToString()];
                ProNode_Eff.Find(strEffWorker).GetComponent<TMPro.TextMeshProUGUI>().text = strAddSignal + Worker.GetEff(ProNode.ExpType).ToString() + strModSignal;

                var bar1 = ProNode_Worker.Find(strBar1).GetComponent<Image>();
                bar1.fillAmount = (float)Worker.APCurrent / Worker.APMax;
                bar1.color = ManagerNS.LocalGameManager.Instance.ProNodeManager.GetAPBarColor(Worker.APCurrent);
            }
        }

        public void OnDeleteWorkerEvent(WorkerNS.Worker worker)
        {
            if (CurMode == Mode.ChangeWorker)
            {
                Workers.Remove(worker);
                UpdateBtnInfo();
            }
            Refresh();
        }
        #endregion
    }
}
