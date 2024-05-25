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

            RawBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ProNode").Find("Recipe").Find("Raw").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            int num = ProNode.HasRecipe ? ProNode.Recipe.Raw.Count : 0;
            RawBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_RawTemplate.prefab", () => { synchronizer.Check(); });

            Recipes = new List<string>() { "" };
            Recipes.AddRange(ProNode.GetCanProduceRecipe());
            RecipeBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeRecipe").Find("Select").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            RecipeBtnList.ChangBtnNum(Recipes.Count, "Prefab_ProNode_UI/Prefab_ProNode_UI_RecipeTemplate.prefab", () => { synchronizer.Check(); });

            RecipeRawBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeRecipe").Find("Recipe").Find("Raw").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            RecipeRawBtnList.ChangBtnNum(0, "Prefab_ProNode_UI/Prefab_ProNode_UI_RecipeRawTemplate.prefab",
                () => { synchronizer.Check(); RecipeBtnList.OnSelectButtonChanged += () => { UpdateBtnInfo(); Refresh(); }; });

            WorkerBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeWorker").Find("Select").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            WorkerBtnList.OnSelectButtonChanged += () => { Refresh(); };
            WorkerBtnList.ChangBtnNum(0, "Prefab_ProNode_UI/Prefab_ProNode_UI_WorkerTemplate.prefab", () => { synchronizer.Check(); });

            num = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(ProNode.WorldProNode.Classification.ToString()).Count;
            UpgradeBtnList = new ML.Engine.UI.UIBtnList(transform.Find("Upgrade").Find("Raw").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            UpgradeBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_UpgradeRawTemplate.prefab", () => { synchronizer.Check(); });

        }
        protected void UpdateBtnInfo()
        {
            if (!IsInitBtnList) return;
            IsInitBtnList = false;
            Synchronizer synchronizer = new Synchronizer(4, () => { IsInitBtnList = true; Refresh(); });
            int num = ProNode.HasRecipe ? ProNode.Recipe.Raw.Count : 0;
            if (RawBtnList != null && RawBtnList.BtnCnt != num)
            {
                RawBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_RawTemplate.prefab", () => { synchronizer.Check(); });
            }
            else
            {
                synchronizer.Check();
            }
            num = ManagerNS.LocalGameManager.Instance.RecipeManager.GetRaw(Recipes[RecipeIndex]).Count;
            if (RecipeRawBtnList != null && RecipeRawBtnList.BtnCnt != num)
            {
                RecipeRawBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_RecipeRawTemplate.prefab", () => { synchronizer.Check(); });
            }
            else
            {
                synchronizer.Check();
            }
            num = Workers.Count > 0 ? Workers.Count : 0;
            if (WorkerBtnList != null && WorkerBtnList.BtnCnt != num)
            {
                WorkerBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_WorkerTemplate.prefab", () => { synchronizer.Check(); });
            }
            else
            {
                synchronizer.Check();
            }
            num = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(ProNode.WorldProNode.Classification.ToString()).Count;
            if (UpgradeBtnList != null && UpgradeBtnList.BtnCnt != num)
            {
                UpgradeBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_UpgradeRawTemplate.prefab", () => { synchronizer.Check(); });
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
                    ProNode_Priority.Find("Selected").gameObject.SetActive(false);
                }
                ProNode.TransportPriority = value;
                Text_Priority.text = PanelTextContent.TransportPriority[(int)ProNode.TransportPriority];
                ProNode_Priority = transform.Find("TopTitle").Find("Priority").GetChild((int)ProNode.TransportPriority);
                ProNode_Priority.Find("Selected").gameObject.SetActive(true);
            }
        }

        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();

        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;
        private Sprite WorkerIcon;
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
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Text_Priority = transform.Find("TopTitle").Find("Priority").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            #endregion
            #region ProNode
            ProNode_Product = transform.Find("ProNode").Find("Recipe").Find("Product");
            ProNode_Worker = transform.Find("ProNode").Find("Worker");
            ProNode_Eff = transform.Find("ProNode").Find("Eff");
            #endregion
            #region ChangeRecipe
            Recipe_UI = transform.Find("ChangeRecipe");
            Recipe_Desc = Recipe_UI.Find("Desc");
            #endregion
            #region Upgrade
            Upgrade_UI = transform.Find("Upgrade");
            Upgrade_UI.gameObject.SetActive(false);
            Upgrade_Build = Upgrade_UI.Find("Build");
            Upgrade_LvOld = Upgrade_UI.Find("Level").Find("LvOld");
            Upgrade_LvNew = Upgrade_UI.Find("Level").Find("LvNew");
            #endregion
            #region BotKeyTips
            BotKeyTips = transform.Find("BotKeyTips").Find("KeyTips");
            BotKeyTips1 = transform.Find("BotKeyTips").Find("KeyTips1");
            BotKeyTips1.gameObject.SetActive(false);
            #endregion
            IsInit = true;
        }
        #endregion

        #region Override
        protected override void Enter()
        {
            tempSprite.Add("", transform.Find("ProNode").Find("Recipe").Find("Product").Find("Icon").GetComponent<Image>().sprite);
            ProNode.OnDataChangeEvent += RefreshDynamic;
            ProNode.OnProduceUpdateEvent += OnProduceTimerUpdateAction;
            ProNode.OnProduceEndEvent += Refresh;
            ManagerNS.LocalGameManager.Instance.WorkerManager.OnDeleteWorkerEvent += OnDeleteWorkerEvent;
            WorkerIcon = ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_Beast");
            WorkerMaleIcon = ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_GenderMale");
            WorkerFemalIcon = ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_GenderFemale");
            base.Enter();
        }

        protected override void Exit()
        {
            ProNode.OnDataChangeEvent -= RefreshDynamic;
            ProNode.OnProduceUpdateEvent -= OnProduceTimerUpdateAction;
            ProNode.OnProduceEndEvent -= Refresh;
            ManagerNS.LocalGameManager.Instance.WorkerManager.OnDeleteWorkerEvent -= OnDeleteWorkerEvent;
            tempSprite.Remove("");
            foreach (var s in tempSprite.Values.ToArray())
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            ML.Engine.Manager.GameManager.DestroyObj(WorkerIcon);
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
                if (ProNode.HasRecipe)
                {
                    ML.Engine.InventorySystem.ItemManager.Instance.AddItemIconObject(ProNode.Recipe.Product.id, ProNode.WorldProNode.transform,
                    new Vector3(0, ProNode.WorldProNode.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0), Quaternion.Euler(Vector3.zero), Vector3.one,
                    (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
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
                int num = ProNode.StackAll;
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
            transform.Find("ChangeWorker").gameObject.SetActive(CurMode == Mode.ChangeWorker);
            Upgrade_UI.gameObject.SetActive(CurMode == Mode.Upgrade);
            BotKeyTips.gameObject.SetActive(CurMode == Mode.ProNode);
            BotKeyTips1.gameObject.SetActive(CurMode != Mode.ProNode);
            BotKeyTips1.Find("KT_Confirm").gameObject.SetActive(CurMode != Mode.Upgrade);
            transform.Find("TopTitle").Find("KT_Upgrade").gameObject.SetActive(HasUpgrade);
            if (CurMode == Mode.ProNode)
            {
                BotKeyTips.Find("KT_ChangeWorker").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Worker);
                BotKeyTips.Find("KT_RemoveWorker").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Worker && ProNode.HaveWorker);
                BotKeyTips.Find("KT_ChangeRecipe").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
                BotKeyTips.Find("KT_Remove1").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe);
                BotKeyTips.Find("KT_Remove10").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe);
                BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe && ProNode.Recipe.Raw.Count > 0);
                BotKeyTips.Find("KT_Return").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
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
                ProNode_Product.Find("Selected").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
                bool hasRecipe = ProNode.HasRecipe;
                ProNode_Product.Find("Mask").gameObject.SetActive(hasRecipe);
                ProNode_Product.Find("Name").gameObject.SetActive(hasRecipe);
                ProNode_Product.Find("Back").gameObject.SetActive(hasRecipe);
                ProNode_Product.Find("Back1").gameObject.SetActive(hasRecipe);
                ProNode_Product.Find("Amount").gameObject.SetActive(hasRecipe);

                if (!hasRecipe || ProNode.State != ProNodeState.Production)
                {
                    ProNode_Product.Find("Mask").GetComponent<Image>().fillAmount = 0;
                }

                if (hasRecipe)
                {
                    #region Product
                    string productID = ProNode.Recipe.ProductID ?? "";
                    if (!tempSprite.ContainsKey(productID))
                    {
                        tempSprite[productID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(productID);
                    }
                    ProNode_Product.Find("Icon").GetComponent<Image>().sprite = tempSprite[productID];
                    ProNode_Product.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(productID);
                    ProNode_Product.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.StackAll.ToString();
                    ProNode_Product.Find("Back").GetComponent<Image>().color = ProNode.StackAll >= ProNode.StackMax * ProNode.ProductNum ? new Color(113/255f, 182/255f, 4/255f) : Color.black;
                    #endregion

                    #region Raw
                    var Raws = ProNode.Recipe.Raw;
                    for (int i = 0; i < Raws.Count; ++i)
                    {
                        if (i >= RawBtnList.BtnCnt) { break; }
                        var itemID = Raws[i].id ?? "";
                        var item = RawBtnList.GetBtn(i).transform;
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                        }
                        item.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];
                        var needAmount = item.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                        needAmount.text = ProNode.Recipe.GetRawNum(itemID).ToString();
                        var amount = item.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                        amount.text = ProNode.GetItemAllNum(itemID).ToString();
                        item.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(itemID);
                        if (int.Parse(amount.text) < int.Parse(needAmount.text))
                        {
                            item.Find("Back").GetComponent<Image>().color = Color.red;
                        }
                        else
                        {
                            item.Find("Back").GetComponent<Image>().color = Color.black;
                        }
                    }
                    #endregion
                }
                else
                {
                    ProNode_Product.Find("Icon").GetComponent<Image>().sprite = tempSprite[""];
                }
                #endregion

                #region Worker
                ProNode_Worker.Find("Selected").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Worker);
                ProNode_Worker.gameObject.SetActive(true);
                bool hasWorker = ProNode.HaveWorker;
                ProNode_Worker.Find("Bar").gameObject.SetActive(hasWorker);
                ProNode_Worker.Find("Bar1").gameObject.SetActive(hasWorker);
                ProNode_Worker.Find("Image").gameObject.SetActive(hasWorker);
                ProNode_Worker.Find("Name").gameObject.SetActive(hasWorker);
                ProNode_Worker.Find("Empty").gameObject.SetActive(!hasWorker);
                ProNode_Worker.Find("Gender").gameObject.SetActive(hasWorker);
                var onDuty = ProNode_Worker.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();
                ProNode_Worker.Find("Icon").GetComponent<Image>().sprite = hasWorker ? WorkerIcon : tempSprite[""];
                onDuty.text = hasWorker ? PanelTextContent.workerStatus[(int)Worker.Status] : PanelTextContent.textLack;
                if (hasWorker)
                {
                    ProNode_Worker.Find("Bar1").GetComponent<Image>().fillAmount = (float)Worker.APCurrent / Worker.APMax;
                    ProNode_Worker.Find("Bar1").GetComponent<Image>().color = ManagerNS.LocalGameManager.Instance.ProNodeManager.GetAPBarColor(Worker.APCurrent);
                    ProNode_Worker.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = Worker.Name;
                    ProNode_Worker.Find("Gender").GetComponent<Image>().sprite = Worker.Gender == Gender.Male ? WorkerMaleIcon : WorkerFemalIcon;
                }
                #endregion

                #region Eff
                ProNode_Eff.Find("EffPrefix").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEff;
                ProNode_Eff.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + ProNode.GetEff().ToString() + "%";

                string buildID = ML.Engine.BuildingSystem.BuildingManager.Instance.GetID(ProNode.WorldProNode.Classification.ToString());
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = ML.Engine.BuildingSystem.BuildingManager.Instance.GetBuildIcon(buildID);
                }
                ProNode_Eff.Find("IconProNode").GetComponent<Image>().sprite = tempSprite[buildID];
                ProNode_Eff.Find("EffProNode").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + ProNode.EffBase.ToString() + "%";
                ProNode_Eff.Find("IconWorker").gameObject.SetActive(hasWorker);
                ProNode_Eff.Find("EffWorker").gameObject.SetActive(hasWorker);
                if (hasWorker)
                {
                    ProNode_Eff.Find("IconWorker").GetComponent<Image>().sprite = WorkerIcon;
                    ProNode_Eff.Find("EffWorker").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + Worker.GetEff(ProNode.ExpType).ToString() + "%";
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
                    productID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(recipeID).id ?? "";
                    var item = RecipeBtnList.GetBtn(i).transform;
                    if (!tempSprite.ContainsKey(productID))
                    {
                        tempSprite[productID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(productID);
                    }
                    item.Find("Icon").GetComponent<Image>().sprite = tempSprite[productID];
                }
                #endregion

                #region Product
                productID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(Recipes[RecipeIndex]).id ?? "";
                if (!tempSprite.ContainsKey(productID))
                {
                    tempSprite[productID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(productID);
                }
                Recipe_UI.Find("Recipe").Find("Product").GetComponent<Image>().sprite = tempSprite[productID];
                Recipe_UI.Find("Recipe").Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textTime + ":"
                    + ManagerNS.LocalGameManager.Instance.RecipeManager.GetTimeCost(Recipes[RecipeIndex]).ToString() + "s";
                Recipe_Desc.Find("Icon").GetComponent<Image>().sprite = tempSprite[productID];
                bool isValidItemID = ML.Engine.InventorySystem.ItemManager.Instance.IsValidItemID(productID);
                Recipe_Desc.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(productID) : PanelTextContent.textEmpty;
                Recipe_Desc.Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetItemDescription(productID) : PanelTextContent.textEmpty;
                Recipe_Desc.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetEffectDescription(productID) : PanelTextContent.textEmpty;
                Recipe_Desc.Find("WeightIcon").gameObject.SetActive(isValidItemID);
                Recipe_Desc.Find("Weight").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetWeight(productID).ToString() : "";
                #endregion

                #region Raw
                var recipeRaws = ManagerNS.LocalGameManager.Instance.RecipeManager.GetRaw(Recipes[RecipeIndex]);
                for (int i = 0; i < recipeRaws.Count; ++i)
                {
                    if (i >= RecipeRawBtnList.BtnCnt) { break; }
                    var itemID = recipeRaws[i].id;
                    var item = RecipeRawBtnList.GetBtn(i).transform;
                    var name = item.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    name.text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(itemID);
                    if (name.text == "")
                    {
                        name.text = PanelTextContent.textEmpty;
                    }
                    item.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>().text = recipeRaws[i].num.ToString();
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                    }
                    item.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];
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
                    // Icon
                    item.Find("Icon").GetComponent<Image>().sprite = WorkerIcon;
                    // Bar1
                    var bar1 = item.Find("Bar1").GetComponent<Image>();
                    bar1.fillAmount = (float)worker.APCurrent / worker.APMax;
                    bar1.color = ManagerNS.LocalGameManager.Instance.ProNodeManager.GetAPBarColor(worker.APCurrent);
                    // Name
                    item.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = worker.Name;
                    // Gender
                    item.Find("Gender").GetComponent<Image>().sprite = worker.Gender == Gender.Male ? WorkerMaleIcon : WorkerFemalIcon;
                    // Eff
                    bool effEnable = worker.GetEff(ProNode.ExpType) > 0;
                    item.Find("Eff").gameObject.SetActive(effEnable);
                    item.Find("EffZero").gameObject.SetActive(!effEnable);
                    item.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + worker.GetEff(ProNode.ExpType).ToString() + '%';
                    string workType = ProNode.ExpType.ToString();
                    if (!tempSprite.ContainsKey(workType))
                    {
                        tempSprite[workType] = ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(workType);
                    }
                    item.transform.Find("IconWorkType").GetComponent<Image>().sprite = tempSprite[workType] ?? tempSprite[""];
                    if (worker.Skill.ContainsKey(ProNode.ExpType))
                    {
                        GridLayoutGroup gridLayout = item.transform.Find("Level").GetComponent<GridLayoutGroup>();
                        Transform template = item.transform.Find("Level").Find("Icon");
                        int num = ManagerNS.LocalGameManager.Instance.ProNodeManager.GetExpRateIconNum(worker.GetEff(ProNode.ExpType));
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
                    item.transform.Find("StateWork").gameObject.SetActive(!isCurrentWorker && (worker.HaveProNode || worker.HaveTransport));
                    item.transform.Find("StateRelax").gameObject.SetActive(!isCurrentWorker && !worker.HaveProNode && !worker.HaveTransport);
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
                Upgrade_Build.Find("Icon").GetComponent<Image>().sprite = tempSprite[buildID];
                // Name
                Upgrade_Build.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.BuildingSystem.BuildingManager.Instance.GetName(buildCID) ?? "";
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
                    int current = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).InventoryItemAmount(itemID);
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                    }
                    uiItemData.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];

                    uiItemData.Find("Background3").gameObject.SetActive(current < need);

                    var nametext = uiItemData.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                    var amounttext = uiItemData.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    var needtext = uiItemData.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                    uiItemData.Find("Background3").gameObject.SetActive(current < need);
                    flagUpgradeBtn = current >= need;
                    if (itemID != "")
                    {
                        nametext.text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(itemID);
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

                Upgrade_UI.Find("BtnBackground1").gameObject.SetActive(flagUpgradeBtn);
                #endregion

                #region Level
                Upgrade_LvOld.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: " + ProNode.Level.ToString();
                Upgrade_LvOld.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textLvDesc + ProNode.EffBase + "%";

                if (ProNode.Level + 1 <= ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.LevelMax)
                {
                    Upgrade_UI.Find("BtnBackground").gameObject.SetActive(true);
                    Upgrade_UI.Find("KT_UpgradeConfirm").gameObject.SetActive(true);

                    Upgrade_Build.Find("Image").gameObject.SetActive(true);
                    Upgrade_LvNew.Find("Lv").GetComponent<TMPro.TextMeshProUGUI>().text = "Lv: " + (ProNode.Level + 1).ToString();
                    Upgrade_LvNew.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textLvDesc +
                        (ProNode.EffBase + ManagerNS.LocalGameManager.Instance.ProNodeManager.Config.LevelUpgradeEff[ProNode.Level]) + "%";
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

        private void OnProduceTimerUpdateAction(double time)
        {
            ProNode_Product.Find("Mask").GetComponent<Image>().fillAmount = 1 - (float)(time / ProNode.GetTimeCost());
        }

        public void RefreshDynamic()
        {
            if (!ProNode.HasRecipe || ProNode.State != ProNodeState.Production)
            {
                ProNode_Product.Find("Mask").GetComponent<Image>().fillAmount = 0;
            }
            if (ProNode.HasRecipe)
            {
                ProNode_Product.Find("Back").GetComponent<Image>().color = ProNode.StackAll >= ProNode.StackMax * ProNode.ProductNum ? new Color(113/255f, 182/255f, 4/255f) : Color.black;
                ProNode_Product.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.GetItemAllNum(ProNode.Recipe.ProductID).ToString();
                for (int i = 0; i < ProNode.Recipe.Raw.Count; ++i)
                {
                    if (i >= RawBtnList.BtnCnt) { break; }
                    Transform item = RawBtnList.GetBtn(i).transform;
                    var amount = item.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                    amount.text = ProNode.GetItemAllNum(ProNode.Recipe.Raw[i].id).ToString();
                    // NeedAmount
                    var needAmount = item.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                    needAmount.text = ProNode.Recipe.GetRawNum(ProNode.Recipe.Raw[i].id).ToString();
                    item.Find("Back").GetComponent<Image>().color = int.Parse(amount.text) < int.Parse(needAmount.text) ? Color.red : Color.black;
                }
            }
            if (ProNode.HaveWorker)
            {
                var onDuty = ProNode_Worker.transform.Find("OnDuty").GetComponent<TMPro.TextMeshProUGUI>();
                onDuty.text = PanelTextContent.workerStatus[(int)Worker.Status];
                ProNode_Eff.Find("EffPrefix").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEff;
                ProNode_Eff.Find("Eff").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + ProNode.GetEff().ToString() + "%";
                string buildID = ML.Engine.BuildingSystem.BuildingManager.Instance.GetID(ProNode.WorldProNode.Classification.ToString());
                if (!tempSprite.ContainsKey(buildID))
                {
                    tempSprite[buildID] = ML.Engine.BuildingSystem.BuildingManager.Instance.GetBuildIcon(buildID);
                }
                ProNode_Eff.Find("IconProNode").GetComponent<Image>().sprite = tempSprite[buildID];
                ProNode_Eff.Find("EffProNode").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + ProNode.EffBase.ToString() + "%";
                ProNode_Eff.Find("IconWorker").GetComponent<Image>().sprite = WorkerIcon;
                ProNode_Eff.Find("EffWorker").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + Worker.GetEff(ProNode.ExpType).ToString() + "%";

                var bar1 = ProNode_Worker.Find("Bar1").GetComponent<Image>();
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
