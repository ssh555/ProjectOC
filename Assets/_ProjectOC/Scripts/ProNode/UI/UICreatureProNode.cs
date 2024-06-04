using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ML.Engine.Utility;
using static ProjectOC.ProNodeNS.UI.UICreatureProNode;
using System;

namespace ProjectOC.ProNodeNS.UI
{
    public class UICreatureProNode : ML.Engine.UI.UIBasePanel<ProNodePanel>
    {
        #region Data
        #region Mode
        public enum Mode
        {
            Creature,
            Output,
            Discard,
            ChangeCreature,
        }
        private Mode curMode;
        public Mode CurMode
        {
            get => curMode;
            set
            {
                CreatureBtnList.DisableBtnList();
                curMode = value;
                if (curMode == Mode.ChangeCreature)
                {
                    CreatureBtnList.EnableBtnList();
                    Creatures.Clear();
                    Creatures.Add(null);
                    foreach (var item in ManagerNS.LocalGameManager.Instance.Player.GetInventory().GetItemList())
                    {
                        if (item is ML.Engine.InventorySystem.CreatureItem creature)
                        {
                            Creatures.Add(creature);
                        }
                    }
                    UpdateBtnInfo();
                    CreatureBtnList.MoveIndexIUISelected(0);
                }
            }
        }
        #endregion

        public CreatureProNode ProNode;
        private List<ML.Engine.InventorySystem.CreatureItem> Creatures = new List<ML.Engine.InventorySystem.CreatureItem>();

        #region BtnList
        private ML.Engine.UI.UIBtnList RawBtnList;
        private ML.Engine.UI.UIBtnList CreatureBtnList;
        private int CreatureIndex => CreatureBtnList?.GetCurSelectedPos1() ?? 0;
        private ML.Engine.UI.UIBtnList RecipeRawBtnList;
        private bool IsInitBtnList;
        protected override void InitBtnInfo()
        {
            Synchronizer synchronizer = new Synchronizer(3, () => { IsInitBtnList = true; Refresh(); });

            RawBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ProNode").Find("Recipe").Find("Raw").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            int num = ProNode.HasRecipe ? ProNode.Recipe.Raw.Count : 0;
            RawBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_RawTemplate.prefab", () => { synchronizer.Check(); });

            CreatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeCreature").Find("Select").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            CreatureBtnList.ChangBtnNum(0, "Prefab_ProNode_UI/Prefab_ProNode_UI_CreatureTemplate.prefab", () => { synchronizer.Check(); });
            CreatureBtnList.OnSelectButtonChanged += Refresh;

            RecipeRawBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeCreature").Find("Recipe").Find("Raw").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            RecipeRawBtnList.ChangBtnNum(0, "Prefab_ProNode_UI/Prefab_ProNode_UI_RecipeRawTemplate.prefab", 
                () => { synchronizer.Check(); CreatureBtnList.OnSelectButtonChanged += UpdateBtnInfo; });
        }
        protected void UpdateBtnInfo()
        {
            if (!IsInitBtnList) return;
            IsInitBtnList = false;
            Synchronizer synchronizer = new Synchronizer(3, () => { IsInitBtnList = true; Refresh(); });
            int num = ProNode.HasRecipe ? ProNode.Recipe.Raw.Count : 0;
            if (num != RawBtnList.BtnCnt)
            {
                RawBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_RawTemplate.prefab", () => { synchronizer.Check(); });
            }
            else { synchronizer.Check(); }
            num = Creatures.Count;
            if (num != CreatureBtnList.BtnCnt)
            {
                CreatureBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_CreatureTemplate.prefab", () => { synchronizer.Check(); });
            }
            else { synchronizer.Check(); }
            num = num > 0 && 0 <= CreatureIndex && CreatureIndex < Creatures.Count ? ManagerNS.LocalGameManager.Instance.RecipeManager.GetRaw(Creatures[CreatureIndex]?.ProRecipeID ?? "").Count : 0;
            if (num != RecipeRawBtnList.BtnCnt)
            {
                RecipeRawBtnList.ChangBtnNum(num, "Prefab_ProNode_UI/Prefab_ProNode_UI_RecipeRawTemplate.prefab", () => { synchronizer.Check(); });
            }
            else { synchronizer.Check(); }
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
        private Transform ProNode_Priority;
        private Transform ProNode_Product;
        private Transform ProNode_Creature;
        private Transform ProNode_Discard;
        private Transform ProNode_Eff;
        private Transform Creature_UI;
        private Transform Creature_Desc;
        private Transform BotKeyTips;
        private Transform BotKeyTips1;

        #region TextContent
        [System.Serializable]
        public struct ProNodePanel
        {
            public TextContent textProNodeType;
            public TextContent textEmpty;
            public TextContent textLack;
            public TextContent[] TransportPriority;
            public TextContent textStateVacancy;
            public TextContent textStateStagnation;
            public TextContent textStateProduction;
            public TextContent textTime;
            public TextContent textEff;
            public TextContent textBag;

            public KeyTip NextPriority;
            public KeyTip ChangeRecipe;
            public KeyTip ChangeBar;
            public KeyTip FastRemove;
            public KeyTip Remove1;
            public KeyTip Remove10;
            public KeyTip FastAdd;
            public KeyTip Return;
            public KeyTip Confirm;
            public KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            abpath = "OCTextContent/ProNode";
            abname = "CreatureProNodePanel";
            description = "CreatureProNodePanel数据加载完成";
        }
        #endregion
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
            ProNode_Creature = transform.Find("ProNode").Find("Creature");
            ProNode_Discard = transform.Find("ProNode").Find("Discard");
            ProNode_Eff = transform.Find("ProNode").Find("Eff");
            #endregion
            #region ChangeCreature
            Creature_UI = transform.Find("ChangeCreature");
            Creature_Desc = Creature_UI.Find("Desc");
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
            ProNode.OnDataChangeEvent += Refresh;
            ProNode.OnProduceUpdateEvent += OnProduceTimerUpdateAction;
            ProNode.OnProduceEndEvent += Refresh;
            tempSprite.Add("", ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_Empty"));
            tempSprite.Add("Male", ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_GenderMale"));
            tempSprite.Add("Female", ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_GenderFemale"));
            base.Enter();
        }

        protected override void Exit()
        {
            ProNode.OnDataChangeEvent -= Refresh;
            ProNode.OnProduceUpdateEvent -= OnProduceTimerUpdateAction;
            ProNode.OnProduceEndEvent -= Refresh;
            foreach (var s in tempSprite.Values.ToArray())
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            tempSprite.Clear();
            base.Exit();
        }
        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed -= NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.canceled -= Remove1;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.performed -= Remove10;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.canceled -= Remove_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.performed -= Remove_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed -= FastAdd_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.started -= Alter_started;
        }
        protected override void RegisterInput()
        {
            CreatureBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.NextPriority.performed += NextPriority_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.canceled += Remove1;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.performed += Remove10;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.canceled += Remove_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.performed += Remove_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed += FastAdd_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.started += Alter_started;
        }
        private void NextPriority_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurPriority = (MissionNS.TransportPriority)(((int)ProNode.TransportPriority + 1) % System.Enum.GetValues(typeof(MissionNS.TransportPriority)).Length);
        }
        private void Alter_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var f_offset = obj.ReadValue<Vector2>();
            var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
            if (CurMode != Mode.ChangeCreature)
            {
                if (offset.x > 0 && CurMode < Mode.Discard && ProNode.HasRecipe)
                {
                    CurMode += 1;
                }
                else if (offset.x < 0 && CurMode > Mode.Creature && ProNode.HasRecipe)
                {
                    CurMode -= 1;
                }
                else if (offset.y != 0 && CurMode == Mode.Output)
                {
                    if (offset.y > 0 && ProNode.OutputThreshold < 50)
                    {
                        ProNode.OutputThreshold += 1;
                    }
                    else if (offset.y < 0 && ProNode.OutputThreshold > 0)
                    {
                        ProNode.OutputThreshold -= 1;
                    }
                }
                Refresh();
            }
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Creature)
            {
                CurMode = Mode.ChangeCreature;
                Refresh();
            }
            else if (CurMode == Mode.ChangeCreature)
            {
                ProNode.ChangeCreature(Creatures[CreatureIndex]);
                UpdateBtnInfo();
                CurMode = Mode.Creature;
                Refresh();
            }
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode != Mode.ChangeCreature)
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
                CurMode = Mode.Creature;
                Refresh();
            }
        }
        private bool ItemIsDestroyed = false;
        private void Remove1(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Discard)
            {
                if (ItemIsDestroyed) { ItemIsDestroyed = false; }
                else
                {
                    int index = ProNode.DataContainer.GetCapacity() - 1;
                    ProNode.Remove(index, 1);
                    Refresh();
                }
            }
        }
        private void Remove10(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Discard)
            {
                ItemIsDestroyed = true;
                int num = ProNode.DiscardStack;
                num = num < 10 ? num : 10;
                int index = ProNode.DataContainer.GetCapacity() - 1;
                ProNode.Remove(index, num);
                Refresh();
            }
        }
        private void Remove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (ProNode.HasRecipe)
            {
                if (CurMode == Mode.Creature)
                {
                    ProNode.Remove(0, ProNode.Stack);
                    int index = ProNode.DataContainer.GetCapacity() - 1;
                    ProNode.Remove(index, ProNode.DiscardStack);
                }
                Refresh();
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (ProNode.HasRecipe && CurMode == Mode.Creature)
            {
                ProNode.FastAdd();
                Refresh();
            }
        }
        #endregion

        #region UI
        private void SetUIActive()
        {
            Creature_UI.gameObject.SetActive(CurMode == Mode.ChangeCreature);
            BotKeyTips.gameObject.SetActive(CurMode != Mode.ChangeCreature);
            BotKeyTips1.gameObject.SetActive(CurMode == Mode.ChangeCreature);
            if (CurMode != Mode.ChangeCreature)
            {
                ProNode_Creature.Find("Selected").gameObject.SetActive(CurMode == Mode.Creature);
                ProNode_Creature.Find("Output").Find("Output").Find("Value").Find("Selected").gameObject.SetActive(CurMode == Mode.Output);
                ProNode_Discard.Find("Selected").gameObject.SetActive(CurMode == Mode.Discard);
                BotKeyTips.Find("KT_ChangeRecipe").gameObject.SetActive(CurMode == Mode.Creature);
                BotKeyTips.Find("KT_ChangeBar").gameObject.SetActive(CurMode == Mode.Output);
                BotKeyTips.Find("KT_FastRemove").gameObject.SetActive(CurMode == Mode.Creature && ProNode.HasRecipe);
                BotKeyTips.Find("KT_Remove1").gameObject.SetActive(CurMode == Mode.Discard);
                BotKeyTips.Find("KT_Remove10").gameObject.SetActive(CurMode == Mode.Discard);
                BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(CurMode == Mode.Creature && ProNode.HasRecipe);
                BotKeyTips.Find("KT_Return").gameObject.SetActive(CurMode != Mode.ChangeCreature);
                LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
            }
        }
        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            CurPriority = ProNode.TransportPriority;
            SetUIActive();
            if (CurMode != Mode.ChangeCreature)
            {
                Text_Title.text = PanelTextContent.textProNodeType;
                #region Product
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
                    string productID = ProNode.Recipe.ProductID;
                    if (!tempSprite.ContainsKey(productID))
                    {
                        tempSprite[productID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(productID);
                    }
                    ProNode_Product.Find("Icon").GetComponent<Image>().sprite = tempSprite[productID];
                    ProNode_Product.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(productID);
                    ProNode_Product.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.Stack.ToString();
                    ProNode_Product.Find("Back").GetComponent<Image>().color = ProNode.StackAll >= ProNode.StackMax * ProNode.ProductNum ? new Color(113 / 255f, 182 / 255f, 4 / 255f) : Color.black;
                    #endregion

                    #region Raw
                    var Raws = ProNode.Recipe.Raw;
                    for (int i = 0; i < Raws.Count; ++i)
                    {
                        if (i >= RawBtnList.BtnCnt) { break; }
                        var rawItemID = Raws[i].id ?? "";
                        var item = RawBtnList.GetBtn(i).transform;
                        if (!tempSprite.ContainsKey(rawItemID))
                        {
                            tempSprite[rawItemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(rawItemID);
                        }
                        item.Find("Icon").GetComponent<Image>().sprite = tempSprite[rawItemID];
                        var needAmount = item.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
                        needAmount.text = ProNode.Recipe.GetRawNum(rawItemID).ToString();
                        var amount = item.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
                        amount.text = ProNode.GetItemAllNum(rawItemID).ToString();
                        item.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(rawItemID);
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

                #region Creature
                var creature = ProNode.Creature;
                string itemID = creature?.ID ?? "";
                bool hasCreature = creature != null;
                if (!tempSprite.ContainsKey(itemID))
                {
                    tempSprite.Add(itemID, ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(itemID));
                }
                ProNode_Creature.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];
                ProNode_Creature.Find("Name").gameObject.SetActive(hasCreature);
                ProNode_Creature.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemName(itemID);
                ProNode_Creature.Find("Empty").gameObject.SetActive(!hasCreature);
                Transform output = ProNode_Creature.Find("Output");
                Transform activity = ProNode_Creature.Find("Activity");
                output.gameObject.SetActive(hasCreature);
                activity.gameObject.SetActive(hasCreature);
                if (hasCreature)
                {
                    RectTransform outputCurRect = output.Find("Cur").GetComponent<RectTransform>();
                    outputCurRect.sizeDelta = new Vector2(outputCurRect.sizeDelta.x, (120f * creature.Output) / 50);
                    Transform outputThreshold = output.Find("Output");
                    RectTransform outputThresholdCurRect = outputThreshold.GetComponent<RectTransform>();
                    float newY = (120f * ProNode.OutputThreshold) / 50;
                    outputThresholdCurRect.anchoredPosition = new Vector2(outputThresholdCurRect.anchoredPosition.x, newY);
                    outputThreshold.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.OutputThreshold.ToString();
                    output.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = creature.Output.ToString();

                    int initActivity = ManagerNS.LocalGameManager.Instance.CreatureManager.GetActivity(itemID);
                    initActivity = initActivity > 0 ? initActivity : 1;
                    int activityValue = creature.Activity > 0 ? creature.Activity : 0;
                    initActivity = System.Math.Max(activityValue, initActivity);
                    RectTransform activityCurRect = activity.Find("Cur").GetComponent<RectTransform>();
                    activityCurRect.sizeDelta = new Vector2(activityCurRect.sizeDelta.x, (120f * activityValue) / initActivity);
                    activity.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = activityValue.ToString();
                }
                #endregion

                #region Discard
                ProNode_Discard.Find("Back").gameObject.SetActive(hasCreature);
                ProNode_Discard.Find("Back1").gameObject.SetActive(hasCreature);
                ProNode_Discard.Find("Amount").gameObject.SetActive(hasCreature);
                ProNode_Discard.Find("Name").gameObject.SetActive(hasCreature);
                string discardID = creature?.Discard.id ?? "";
                int discardNum = creature?.Discard.num ?? 0;
                if (!tempSprite.ContainsKey(discardID))
                {
                    tempSprite[discardID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(discardID);
                }
                ProNode_Discard.Find("Icon").GetComponent<Image>().sprite = tempSprite[discardID];
                ProNode_Discard.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(discardID);
                ProNode_Discard.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.DiscardStack.ToString();
                ProNode_Discard.Find("Back").GetComponent<Image>().color = ProNode.DiscardStackAll >= ProNode.StackMax * discardNum ? new Color(113 / 255f, 182 / 255f, 4 / 255f) : Color.black;
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
                ProNode_Eff.Find("IconCreature").gameObject.SetActive(hasCreature);
                ProNode_Eff.Find("EffCreature").gameObject.SetActive(hasCreature);
                if (hasCreature)
                {
                    ProNode_Eff.Find("IconCreature").GetComponent<Image>().sprite = tempSprite[creature.ID];
                    ProNode_Eff.Find("EffCreature").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + (ProNode.GetEff() - ProNode.EffBase).ToString() + "%";
                }
                #endregion
            }
            else
            {
                Creature_UI.Find("Bag").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textBag;
                #region Select
                for (int i = 0; i < Creatures.Count; ++i)
                {
                    bool notNull = Creatures[i] != null;
                    Transform item = CreatureBtnList.GetBtn(i).transform;
                    item.Find("OutputIcon").gameObject.SetActive(notNull);
                    item.Find("Output").gameObject.SetActive(notNull);
                    item.Find("ActivityIcon").gameObject.SetActive(notNull);
                    item.Find("Activity").gameObject.SetActive(notNull);
                    item.Find("Gender").gameObject.SetActive(notNull && Creatures[i].Gender != Gender.None);
                    if (notNull)
                    {
                        string itemID = Creatures[i].ID;
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            tempSprite[itemID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(itemID);
                        }
                        item.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];
                        item.Find("Output").GetComponent<TMPro.TextMeshProUGUI>().text = Creatures[i].Output.ToString();
                        item.Find("Activity").GetComponent<TMPro.TextMeshProUGUI>().text = Creatures[i].Activity.ToString();
                        item.Find("Gender").GetComponent<Image>().sprite = Creatures[i].Gender == Gender.Male ? tempSprite["Male"] : tempSprite["Female"];
                    }
                    else
                    {
                        item.Find("Icon").GetComponent<Image>().sprite = tempSprite[""];
                    }
                }
                #endregion

                #region Product
                string recipeID = Creatures[CreatureIndex]?.ProRecipeID ?? "";
                var product = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(recipeID);
                string productID = product.id;
                int productNum = product.num;
                if (!tempSprite.ContainsKey(productID))
                {
                    tempSprite[productID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(productID);
                }
                Creature_UI.Find("Recipe").Find("Product").GetComponent<Image>().sprite = tempSprite[productID];
                Creature_UI.Find("Recipe").Find("Amount").gameObject.SetActive(!string.IsNullOrEmpty(productID));
                Creature_UI.Find("Recipe").Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = $"×{productNum}";
                Creature_UI.Find("Recipe").Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textTime + ":"
                    + ManagerNS.LocalGameManager.Instance.RecipeManager.GetTimeCost(recipeID).ToString() + "s";
                Creature_Desc.Find("Icon").GetComponent<Image>().sprite = tempSprite[productID];
                bool isValidItemID = ML.Engine.InventorySystem.ItemManager.Instance.IsValidItemID(productID);
                Creature_Desc.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(productID) : PanelTextContent.textEmpty;
                Creature_Desc.Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetItemDescription(productID) : PanelTextContent.textEmpty;
                Creature_Desc.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? ML.Engine.InventorySystem.ItemManager.Instance.GetEffectDescription(productID) : PanelTextContent.textEmpty;
                #endregion

                #region Raw
                var recipeRaws = ManagerNS.LocalGameManager.Instance.RecipeManager.GetRaw(recipeID);
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
        }

        private void OnProduceTimerUpdateAction(double time)
        {
            ProNode_Product.Find("Mask").GetComponent<Image>().fillAmount = 1 - (float)(time / ProNode.GetTimeCost());
        }
        #endregion
    }
}