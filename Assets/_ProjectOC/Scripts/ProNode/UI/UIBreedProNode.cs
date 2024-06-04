using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ML.Engine.Utility;
using static ProjectOC.ProNodeNS.UI.UIBreedProNode;

namespace ProjectOC.ProNodeNS.UI
{
    public class UIBreedProNode : ML.Engine.UI.UIBasePanel<ProNodePanel>
    {
        #region Data
        #region Mode
        public enum Mode
        {
            Creature1,
            Creature2,
            Output,
            Creature3,
            Discard,
            ChangeCreature
        }
        private Mode preMode;
        private Mode curMode;
        public Mode CurMode
        {
            get => curMode;
            set
            {
                CreatureBtnList.DisableBtnList();
                preMode = curMode;
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

        public BreedProNode ProNode;
        private List<ML.Engine.InventorySystem.CreatureItem> Creatures = new List<ML.Engine.InventorySystem.CreatureItem>();

        #region BtnList
        private ML.Engine.UI.UIBtnList CreatureBtnList;
        private int CreatureIndex => CreatureBtnList?.GetCurSelectedPos1() ?? 0;
        private bool IsInitBtnList;
        protected override void InitBtnInfo()
        {
            CreatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find("ChangeCreature").Find("Select").Find("Viewport").GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            CreatureBtnList.ChangBtnNum(0, "Prefab_ProNode_UI/Prefab_ProNode_UI_CreatureTemplate.prefab", () => { IsInitBtnList = true; Refresh(); });
            CreatureBtnList.OnSelectButtonChanged += Refresh;
        }
        protected void UpdateBtnInfo()
        {
            if (!IsInitBtnList) return;
            IsInitBtnList = false;
            Synchronizer synchronizer = new Synchronizer(1, () => { IsInitBtnList = true; Refresh(); });
            if (Creatures.Count != CreatureBtnList.BtnCnt)
            {
                CreatureBtnList.ChangBtnNum(Creatures.Count, "Prefab_ProNode_UI/Prefab_ProNode_UI_CreatureTemplate.prefab", () => { synchronizer.Check(); });
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
        private Transform ProNode_UI;
        private Transform ProNode_Discard;
        private Transform Creature_UI;
        private Transform Creature_Recipe;
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
            public TextContent textTime;
            public TextContent textBag;
            public TextContent textOnBreed;
            public TextContent textBreedResult;
            public TextContent textOutput;
            public TextContent textWarnCreature;
            public TextContent textWarnGender;
            public TextContent textGetBreed;
            public TextContent textGetDiscard;

            public KeyTip NextPriority;
            public KeyTip ChangeCreature;
            public KeyTip ChangeBar;
            public KeyTip FastRemove;
            public KeyTip FastAdd;
            public KeyTip Return;
            public KeyTip Confirm;
            public KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            abpath = "OCTextContent/ProNode";
            abname = "BreedProNodePanel";
            description = "BreedProNodePanel数据加载完成";
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
            ProNode_UI = transform.Find("ProNode");
            ProNode_Discard = ProNode_UI.Find("Discard");
            #endregion
            #region ChangeCreature
            Creature_UI = transform.Find("ChangeCreature");
            Creature_Recipe = Creature_UI.Find("Recipe");
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
            tempSprite.Add("", transform.Find("ProNode").Find("Creature1").Find("Icon").GetComponent<Image>().sprite);
            tempSprite.Add("Male", ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_GenderMale"));
            tempSprite.Add("Female", ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_GenderFemale"));
            base.Enter();
        }

        protected override void Exit()
        {
            ProNode.OnDataChangeEvent -= Refresh;
            ProNode.OnProduceUpdateEvent -= OnProduceTimerUpdateAction;
            ProNode.OnProduceEndEvent -= Refresh;
            tempSprite.Remove("");
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
                if (CurMode == Mode.Creature1 && ProNode.HasRecipe)
                {
                    if (offset.x > 0) { CurMode = Mode.Creature2; }
                    else if(offset.y < 0) { CurMode = Mode.Output; }
                }
                else if (CurMode == Mode.Creature2)
                {
                    if (offset.x < 0) { CurMode = Mode.Creature1; }
                    else if (offset.y < 0) { CurMode = Mode.Output; }
                }
                else if (CurMode == Mode.Output)
                {
                    if (offset.y > 0) { CurMode = Mode.Creature1; }
                    else if (offset.y < 0) { CurMode = Mode.Discard; }
                    else if (offset.x != 0) 
                    {
                        int cur = ProNode.OutputThreshold + (offset.x > 0 ? 1 : -1);
                        cur = System.Math.Min(cur, 50);
                        cur = System.Math.Max(cur, 0);
                        ProNode.OutputThreshold = cur;
                    }
                }
                else if (CurMode == Mode.Discard)
                {
                    if (offset.x > 0 && ProNode.Creature3 != null) { CurMode = Mode.Creature3; }
                    else if (offset.y > 0) { CurMode = Mode.Output; }
                }
                else if (CurMode == Mode.Creature3)
                {
                    if (offset.x < 0) { CurMode = Mode.Discard; }
                    else if (offset.y > 0) { CurMode = Mode.Output; }
                }
                Refresh();
            }
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CheckChangeMode(CurMode))
            {
                CurMode = Mode.ChangeCreature;
                Refresh();
            }
            else if (CurMode == Mode.ChangeCreature && CheckChangeMode(preMode) && BotKeyTips1.Find("KT_Confirm").gameObject.activeSelf)
            {
                ProNode.ChangeCreature((int)preMode, Creatures[CreatureIndex]);
                UpdateBtnInfo();
                CurMode = preMode;
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
                CurMode = preMode;
                Refresh();
            }
        }
        private void Remove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (ProNode.HasRecipe)
            {
                if (CurMode == Mode.Discard)
                {
                    ProNode.Remove(4, ProNode.DiscardStack);
                }
                else if (CurMode == Mode.Creature3)
                {
                    ProNode.ChangeData(0, null);
                }
                Refresh();
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (ProNode.HasRecipe && CurMode == Mode.Creature1)
            {
                ProNode.FastAdd();
                Refresh();
            }
        }
        #endregion

        #region UI
        public bool CheckChangeMode(Mode mode)
        {
            return mode == Mode.Creature1 || mode == Mode.Creature2;
        }
        private void SetUIActive()
        {
            Creature_UI.gameObject.SetActive(CurMode == Mode.ChangeCreature);
            BotKeyTips.gameObject.SetActive(CurMode != Mode.ChangeCreature);
            BotKeyTips1.gameObject.SetActive(CurMode == Mode.ChangeCreature);
            if (CurMode != Mode.ChangeCreature)
            {
                bool isProduce = ProNode.IsOnProduce;
                ProNode_UI.Find("State").gameObject.SetActive(isProduce);
                ProNode_UI.Find("Time").gameObject.SetActive(isProduce);
                ProNode_UI.Find("Bar").gameObject.SetActive(isProduce);
                ProNode_UI.Find("Creature1").Find("Selected").gameObject.SetActive(CurMode == Mode.Creature1);
                ProNode_UI.Find("Creature2").Find("Selected").gameObject.SetActive(CurMode == Mode.Creature2);
                ProNode_UI.Find("Creature3").Find("Selected").gameObject.SetActive(CurMode == Mode.Creature3);
                ProNode_Discard.Find("Selected").gameObject.SetActive(CurMode == Mode.Discard);
                BotKeyTips.Find("KT_ChangeCreature").gameObject.SetActive(CheckChangeMode(CurMode));
                BotKeyTips.Find("KT_ChangeBar").gameObject.SetActive(CurMode == Mode.Output);
                BotKeyTips.Find("KT_FastRemove").gameObject.SetActive(CurMode == Mode.Discard);
                BotKeyTips.Find("KT_Remove").gameObject.SetActive(CurMode == Mode.Creature3);
                BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(CurMode == Mode.Creature1 && ProNode.HasRecipe);
                LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
            }
        }
        public void RefreshCreature(Transform creature, ML.Engine.InventorySystem.CreatureItem item)
        {
            string itemID = item?.ID ?? "";
            int value = item?.Output ?? 0;
            if (!tempSprite.ContainsKey(itemID))
            {
                tempSprite.Add(itemID, ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(itemID));
            }
            creature.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];
            Transform output = creature.Find("Output");
            RectTransform outputCurRect = output.Find("Cur").GetComponent<RectTransform>();
            outputCurRect.sizeDelta = new Vector2(outputCurRect.sizeDelta.x, (120f * value) / 50);
            output.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = value.ToString();
        }
        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            CurPriority = ProNode.TransportPriority;
            SetUIActive();
            if (CurMode != Mode.ChangeCreature)
            {
                Text_Title.text = PanelTextContent.textProNodeType;
                bool hasRecipe = ProNode.HasRecipe;
                #region Raw
                Transform raw = ProNode_UI.Find("Raw");
                bool hasRaw = hasRecipe && ProNode.Recipe.Raw.Count > 0;
                string rawID = hasRaw ? ProNode.Recipe.Raw[0].id : "";
                int rawNum = hasRaw ? ProNode.Recipe.Raw[0].num : 0;
                if (!tempSprite.ContainsKey(rawID))
                {
                    tempSprite[rawID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(rawID);
                }
                raw.Find("Icon").GetComponent<Image>().sprite = tempSprite[rawID];
                int amount = ProNode.GetItemAllNum(rawID);
                raw.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = amount.ToString();
                raw.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>().text = rawNum.ToString();
                raw.Find("Back").GetComponent<Image>().color = amount < rawNum ? Color.red : Color.black;
                #endregion

                bool isProduce = ProNode.IsOnProduce;
                ProNode_UI.Find("State").GetComponent<TMPro.TextMeshProUGUI>().text = isProduce ? PanelTextContent.textOnBreed : "";
                #region Bar
                Transform bar = ProNode_UI.Find("OutputBar");
                RectTransform barRect = bar.Find("Cur").GetComponent<RectTransform>();
                float posX = 300 * ProNode.OutputThreshold / 50f;
                barRect.sizeDelta = new Vector2(posX, barRect.sizeDelta.y);
                RectTransform iconRect = bar.Find("Icon").GetComponent<RectTransform>();
                iconRect.anchoredPosition = new Vector2(posX, iconRect.anchoredPosition.y);
                bar.Find("Icon").Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.OutputThreshold.ToString();
                bar.Find("Icon").Find("Selected").gameObject.SetActive(CurMode == Mode.Output);
                bar.Find("Icon").Find("ValueSelected").gameObject.SetActive(CurMode == Mode.Output);
                #endregion

                #region Creature
                RefreshCreature(ProNode_UI.Find("Creature1"), ProNode.Creature1);
                RefreshCreature(ProNode_UI.Find("Creature2"), ProNode.Creature2);
                RefreshCreature(ProNode_UI.Find("Creature3"), ProNode.Creature3);
                #endregion

                #region Discard
                ProNode_Discard.Find("Back").gameObject.SetActive(hasRecipe);
                ProNode_Discard.Find("Back1").gameObject.SetActive(hasRecipe);
                ProNode_Discard.Find("Amount").gameObject.SetActive(hasRecipe);
                ProNode_Discard.Find("Name").gameObject.SetActive(hasRecipe);
                string discardID = ProNode.Creature1?.Discard.id ?? "";
                int discardNum = ProNode.Creature1?.Discard.num ?? 0;
                if (!tempSprite.ContainsKey(discardID))
                {
                    tempSprite[discardID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(discardID);
                }
                ProNode_Discard.Find("Icon").GetComponent<Image>().sprite = tempSprite[discardID];
                ProNode_Discard.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(discardID);
                ProNode_Discard.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.DiscardStack.ToString();
                ProNode_Discard.Find("Back").GetComponent<Image>().color = 
                    ProNode.DiscardStackAll >= ProNode.StackMax * discardNum ? new Color(113 / 255f, 182 / 255f, 4 / 255f) : Color.black;
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

                #region Recipe
                var creature = Creatures[CreatureIndex];
                bool showWarn = preMode == Mode.Creature2;
                bool notTheSameCreature = true;
                bool notTheSameGender = true;
                if (creature != null && showWarn)
                {
                    notTheSameCreature = creature.ID != ProNode.Creature1.ID;
                    notTheSameGender = ProNode.Creature1.Gender == Gender.None ? 
                        creature.Gender != Gender.None : creature.Gender == ProNode.Creature1.Gender;
                }
                bool canBreed = showWarn && !notTheSameCreature && !notTheSameGender;
                BotKeyTips1.Find("KT_Confirm").gameObject.SetActive(creature == null || preMode == Mode.Creature1 || canBreed);
                Creature_Recipe.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textBreedResult;
                Creature_Recipe.Find("Warn").gameObject.SetActive(showWarn);
                Creature_Recipe.Find("Warn").GetComponent<TMPro.TextMeshProUGUI>().text = notTheSameCreature ? PanelTextContent.textWarnCreature : 
                    (notTheSameGender ? PanelTextContent.textWarnGender : "");
                Creature_Recipe.Find("Output").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textOutput;
                Creature_Recipe.Find("OutputValue").gameObject.SetActive(canBreed);
                Creature_Recipe.Find("Empty").gameObject.SetActive(!canBreed);
                if (canBreed)
                {
                    int output1 = ProNode.Creature1.Output;
                    int output2 = creature.Output;
                    int low = -3 + output1 <= output2 ? output1 : output2;
                    int high = 3 + output1 <= output2 ? output2 : output1;
                    Creature_Recipe.Find("OutputValue").GetComponent<TMPro.TextMeshProUGUI>().text = $"{low}~{high}";
                }
                bool isValidCreature = creature != null;
                string recipeID = isValidCreature ? creature.BreRecipeID : "";
                string timeText = PanelTextContent.textTime + ":";
                if (canBreed)
                {
                    timeText = timeText + ManagerNS.LocalGameManager.Instance.RecipeManager.GetTimeCost(recipeID).ToString() + "s";
                }
                Creature_Recipe.Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = timeText;
                Creature_Recipe.Find("Empty1").gameObject.SetActive(!canBreed);
                #endregion

                #region Desc
                string productID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(recipeID).id;
                if (!tempSprite.ContainsKey(productID))
                {
                    tempSprite[productID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(productID);
                }
                Creature_Desc.Find("Icon").GetComponent<Image>().sprite = tempSprite[productID];
                bool isValidItemID = ML.Engine.InventorySystem.ItemManager.Instance.IsValidItemID(productID);
                Creature_Desc.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? 
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(productID) : PanelTextContent.textEmpty;
                Creature_Desc.Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? 
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemDescription(productID) : PanelTextContent.textEmpty;
                #region Output
                Creature_Desc.Find("Output").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textGetBreed;
                string proRecipeID = isValidCreature ? creature.ProRecipeID : "";
                string proProductID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(proRecipeID).id;
                if (!tempSprite.ContainsKey(proProductID))
                {
                    tempSprite[proProductID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(proProductID);
                }
                Creature_Desc.Find("Output").Find("Icon").GetComponent<Image>().sprite = tempSprite[proProductID];
                Creature_Desc.Find("Output").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = 
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(proProductID);
                #endregion

                #region Discard
                Creature_Desc.Find("Discard").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textGetDiscard;
                string discardID = isValidCreature ? creature.Discard.id : "";
                int discardNum = isValidCreature ? creature.Discard.num : 0;
                if (!tempSprite.ContainsKey(discardID))
                {
                    tempSprite[discardID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(discardID);
                }
                Creature_Desc.Find("Discard").Find("Icon").GetComponent<Image>().sprite = tempSprite[discardID];
                Creature_Desc.Find("Discard").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = 
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(discardID);
                Creature_Desc.Find("Discard").Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>().text = discardNum.ToString();
                #endregion
                #endregion
            }
        }

        private void OnProduceTimerUpdateAction(double time)
        {
            if (CurMode != Mode.ChangeCreature)
            {
                bool isProduce = ProNode.IsOnProduce;
                float timeCost = ProNode.GetTimeCost();
                int minute = (int)(time / 60);
                int second = (int)(time - 60 * minute);
                ProNode_UI.Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = isProduce ? $"{minute} min {second} s" : "";
                Transform bar = ProNode_UI.Find("Bar");
                RectTransform barRect = bar.Find("Cur").GetComponent<RectTransform>();
                float posX = 300 * (1 - (float)(time / timeCost));
                barRect.sizeDelta = new Vector2(posX, barRect.sizeDelta.y);
            }
        }
        #endregion
    }
}