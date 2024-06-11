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
        #region Str
        private const string str = "";
        private const string strProNode = "ProNode";
        private const string strRecipe = "Recipe";
        private const string strRaw = "Raw";
        private const string strViewport = "Viewport";
        private const string strSelect = "Select";
        private const string strSelected = "Selected";
        private const string strTopTitle = "TopTitle";
        private const string strPriority = "Priority";
        private const string strName = "Name";
        private const string strBack = "Back";
        private const string strBack1 = "Back1";
        private const string strAmount = "Amount";
        private const string strIcon = "Icon";
        private const string strText = "Text";
        private const string strDesc = "Desc";
        private const string strBotKeyTips = "BotKeyTips";
        private const string strKeyTips = "KeyTips";
        private const string strKeyTips1 = "KeyTips1";
        private const string strKT_Confirm = "KT_Confirm";
        private const string strKT_FastAdd = "KT_FastAdd";
        private const string strNeedAmount = "NeedAmount";
        private const string strColonSignal = ":";
        private const string strs = "s";
        private const string strItemDesc = "ItemDesc";
        private const string strTime = "Time";
        private const string strChangeCreature = "ChangeCreature";
        private const string strPrefab_ProNode_UI_CreatureTemplate = "Prefab_ProNode_UI/Prefab_ProNode_UI_CreatureTemplate.prefab";
        private const string strMale = "Male";
        private const string strFemale = "Female";
        private const string strTex2D_Worker_UI_GenderMale = "Tex2D_Worker_UI_GenderMale";
        private const string strTex2D_Worker_UI_GenderFemale = "Tex2D_Worker_UI_GenderFemale";
        private const string strState = "State";
        private const string strBar = "Bar";
        private const string strCreature1 = "Creature1";
        private const string strCreature2 = "Creature2";
        private const string strCreature3 = "Creature3";
        private const string strKT_ChangeCreature = "KT_ChangeCreature";
        private const string strKT_ChangeBar = "KT_ChangeBar";
        private const string strKT_FastRemove = "KT_FastRemove";
        private const string strKT_Remove = "KT_Remove";
        private const string strOutput = "Output";
        private const string strOutputBar = "OutputBar";
        private const string strCur = "Cur";
        private const string strValue = "Value";
        private const string strValueSelected = "ValueSelected";
        private const string strBag = "Bag";
        private const string strOutputIcon = "OutputIcon";
        private const string strActivityIcon = "ActivityIcon";
        private const string strActivity = "Activity";
        private const string strGender = "Gender";
        private const string strDiscard = "Discard";
        private const string strWarn = "Warn";
        private const string strOutputValue = "OutputValue";
        private const string strEmpty = "Empty";
        private const string strEmpty1 = "Empty1";
        #endregion

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
            CreatureBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strChangeCreature).Find(strSelect).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            CreatureBtnList.ChangBtnNum(0, strPrefab_ProNode_UI_CreatureTemplate, () => { IsInitBtnList = true; Refresh(); });
            CreatureBtnList.OnSelectButtonChanged += Refresh;
        }
        protected void UpdateBtnInfo()
        {
            if (!IsInitBtnList) return;
            IsInitBtnList = false;
            Synchronizer synchronizer = new Synchronizer(1, () => { IsInitBtnList = true; Refresh(); });
            if (Creatures.Count != CreatureBtnList.BtnCnt)
            {
                CreatureBtnList.ChangBtnNum(Creatures.Count, strPrefab_ProNode_UI_CreatureTemplate, () => { synchronizer.Check(); });
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
            Text_Title = transform.Find(strTopTitle).Find(strText).GetComponent<TMPro.TextMeshProUGUI>();
            Text_Priority = transform.Find(strTopTitle).Find(strPriority).Find(strText).GetComponent<TMPro.TextMeshProUGUI>();
            #endregion
            #region ProNode
            ProNode_UI = transform.Find(strProNode);
            ProNode_Discard = ProNode_UI.Find(strDiscard);
            #endregion
            #region ChangeCreature
            Creature_UI = transform.Find(strChangeCreature);
            Creature_Recipe = Creature_UI.Find(strRecipe);
            Creature_Desc = Creature_UI.Find(strDesc);
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
            ProNode.OnDataChangeEvent += Refresh;
            ProNode.OnProduceUpdateEvent += OnProduceTimerUpdateAction;
            ProNode.OnProduceEndEvent += Refresh;
            tempSprite.Add(str, transform.Find(strProNode).Find(strCreature1).Find(strIcon).GetComponent<Image>().sprite);
            tempSprite.Add(strMale, ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(strTex2D_Worker_UI_GenderMale));
            tempSprite.Add(strFemale, ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(strTex2D_Worker_UI_GenderFemale));
            base.Enter();
        }

        protected override void Exit()
        {
            ProNode.OnDataChangeEvent -= Refresh;
            ProNode.OnProduceUpdateEvent -= OnProduceTimerUpdateAction;
            ProNode.OnProduceEndEvent -= Refresh;
            tempSprite.Remove(str);
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
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.canceled -= Alter_canceled;
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
            ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.canceled += Alter_canceled;
        }
        private void NextPriority_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurPriority = (MissionNS.TransportPriority)(((int)ProNode.TransportPriority + 1) % System.Enum.GetValues(typeof(MissionNS.TransportPriority)).Length);
        }
        private ML.Engine.Timer.CounterDownTimer alterTimer;
        private ML.Engine.Timer.CounterDownTimer AlterTimer
        {
            get
            {
                if (alterTimer == null)
                {
                    alterTimer = new ML.Engine.Timer.CounterDownTimer(0.1f, true, false);
                    alterTimer.OnEndEvent += OnAlterTimerEndEvent;
                }
                return alterTimer;
            }
        }
        private int alterMode;
        private void OnAlterTimerEndEvent()
        {
            if (CurMode == Mode.Output)
            {
                if (alterMode > 0 && ProNode.OutputThreshold < 50)
                {
                    ProNode.OutputThreshold += 1;
                }
                else if (alterMode < 0 && ProNode.OutputThreshold > 0)
                {
                    ProNode.OutputThreshold -= 1;
                }
                Refresh();
            }
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
                        alterMode = offset.x;
                        OnAlterTimerEndEvent();
                        AlterTimer.Reset(0.2f);
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
        private void Alter_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Output)
            {
                alterTimer?.End();
            }
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CheckChangeMode(CurMode))
            {
                CurMode = Mode.ChangeCreature;
                Refresh();
            }
            else if (CurMode == Mode.ChangeCreature && CheckChangeMode(preMode) && BotKeyTips1.Find(strKT_Confirm).gameObject.activeSelf)
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
                string id = ProNode.HasRecipe ? ProNode.Recipe.Product.id : "";
                ML.Engine.InventorySystem.ItemManager.Instance.AddItemIconObject(id, ProNode.WorldProNode.transform,
                    new Vector3(0, ProNode.WorldProNode.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0), Quaternion.Euler(Vector3.zero), Vector3.one,
                    (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
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
                ProNode_UI.Find(strState).gameObject.SetActive(isProduce);
                ProNode_UI.Find(strTime).gameObject.SetActive(isProduce);
                ProNode_UI.Find(strBar).gameObject.SetActive(isProduce);
                ProNode_UI.Find(strCreature1).Find(strSelected).gameObject.SetActive(CurMode == Mode.Creature1);
                ProNode_UI.Find(strCreature2).Find(strSelected).gameObject.SetActive(CurMode == Mode.Creature2);
                ProNode_UI.Find(strCreature3).Find(strSelected).gameObject.SetActive(CurMode == Mode.Creature3);
                ProNode_Discard.Find(strSelected).gameObject.SetActive(CurMode == Mode.Discard);
                BotKeyTips.Find(strKT_ChangeCreature).gameObject.SetActive(CheckChangeMode(CurMode));
                BotKeyTips.Find(strKT_ChangeBar).gameObject.SetActive(CurMode == Mode.Output);
                BotKeyTips.Find(strKT_FastRemove).gameObject.SetActive(CurMode == Mode.Discard);
                BotKeyTips.Find(strKT_Remove).gameObject.SetActive(CurMode == Mode.Creature3);
                BotKeyTips.Find(strKT_FastAdd).gameObject.SetActive(CurMode == Mode.Creature1 && ProNode.HasRecipe);
                LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
            }
        }
        public void RefreshCreature(Transform creature, ML.Engine.InventorySystem.CreatureItem item)
        {
            string itemID = item?.ID ?? str;
            int value = item?.Output ?? 0;
            if (!tempSprite.ContainsKey(itemID))
            {
                tempSprite.Add(itemID, ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(itemID));
            }
            creature.Find(strIcon).GetComponent<Image>().sprite = tempSprite[itemID];
            Transform output = creature.Find(strOutput);
            RectTransform outputCurRect = output.Find(strCur).GetComponent<RectTransform>();
            outputCurRect.sizeDelta = new Vector2(outputCurRect.sizeDelta.x, (120f * value) / 50);
            output.Find(strValue).GetComponent<TMPro.TextMeshProUGUI>().text = value.ToString();
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
                Transform raw = ProNode_UI.Find(strRaw);
                bool hasRaw = hasRecipe && ProNode.Recipe.Raw.Count > 0;
                string rawID = hasRaw ? ProNode.Recipe.Raw[0].id : str;
                int rawNum = hasRaw ? ProNode.Recipe.Raw[0].num : 0;
                if (!tempSprite.ContainsKey(rawID))
                {
                    tempSprite[rawID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(rawID);
                }
                raw.Find(strIcon).GetComponent<Image>().sprite = tempSprite[rawID];
                int amount = ProNode.GetItemAllNum(rawID);
                raw.Find(strAmount).GetComponent<TMPro.TextMeshProUGUI>().text = amount.ToString();
                raw.Find(strNeedAmount).GetComponent<TMPro.TextMeshProUGUI>().text = rawNum.ToString();
                raw.Find(strBack).GetComponent<Image>().color = amount < rawNum ? Color.red : Color.black;
                #endregion

                bool isProduce = ProNode.IsOnProduce;
                ProNode_UI.Find(strState).GetComponent<TMPro.TextMeshProUGUI>().text = isProduce ? PanelTextContent.textOnBreed : str;
                #region Bar
                Transform bar = ProNode_UI.Find(strOutputBar);
                RectTransform barRect = bar.Find(strCur).GetComponent<RectTransform>();
                float posX = 300 * ProNode.OutputThreshold / 50f;
                barRect.sizeDelta = new Vector2(posX, barRect.sizeDelta.y);
                RectTransform iconRect = bar.Find(strIcon).GetComponent<RectTransform>();
                iconRect.anchoredPosition = new Vector2(posX, iconRect.anchoredPosition.y);
                bar.Find(strIcon).Find(strValue).GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.OutputThreshold.ToString();
                bar.Find(strIcon).Find(strSelected).gameObject.SetActive(CurMode == Mode.Output);
                bar.Find(strIcon).Find(strValueSelected).gameObject.SetActive(CurMode == Mode.Output);
                #endregion

                #region Creature
                RefreshCreature(ProNode_UI.Find(strCreature1), ProNode.Creature1);
                RefreshCreature(ProNode_UI.Find(strCreature2), ProNode.Creature2);
                RefreshCreature(ProNode_UI.Find(strCreature3), ProNode.Creature3);
                #endregion

                #region Discard
                ProNode_Discard.Find(strBack).gameObject.SetActive(hasRecipe);
                ProNode_Discard.Find(strBack1).gameObject.SetActive(hasRecipe);
                ProNode_Discard.Find(strAmount).gameObject.SetActive(hasRecipe);
                ProNode_Discard.Find(strName).gameObject.SetActive(hasRecipe);
                string discardID = ProNode.Creature1?.Discard.id ?? str;
                int discardNum = ProNode.Creature1?.Discard.num ?? 0;
                if (!tempSprite.ContainsKey(discardID))
                {
                    tempSprite[discardID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(discardID);
                }
                ProNode_Discard.Find(strIcon).GetComponent<Image>().sprite = tempSprite[discardID];
                ProNode_Discard.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(discardID);
                ProNode_Discard.Find(strAmount).GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.DiscardStack.ToString();
                ProNode_Discard.Find(strBack).GetComponent<Image>().color = 
                    ProNode.DiscardStackAll >= ProNode.StackMax * discardNum ? new Color(113 / 255f, 182 / 255f, 4 / 255f) : Color.black;
                #endregion
            }
            else
            {
                Creature_UI.Find(strBag).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textBag;
                #region Select
                for (int i = 0; i < Creatures.Count; ++i)
                {
                    bool notNull = Creatures[i] != null;
                    Transform item = CreatureBtnList.GetBtn(i).transform;
                    item.Find(strOutputIcon).gameObject.SetActive(notNull);
                    item.Find(strOutput).gameObject.SetActive(notNull);
                    item.Find(strActivityIcon).gameObject.SetActive(notNull);
                    item.Find(strActivity).gameObject.SetActive(notNull);
                    item.Find(strGender).gameObject.SetActive(notNull && Creatures[i].Gender != Gender.None);
                    if (notNull)
                    {
                        string itemID = Creatures[i].ID;
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            tempSprite[itemID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(itemID);
                        }
                        item.Find(strIcon).GetComponent<Image>().sprite = tempSprite[itemID];
                        item.Find(strOutput).GetComponent<TMPro.TextMeshProUGUI>().text = Creatures[i].Output.ToString();
                        item.Find(strActivity).GetComponent<TMPro.TextMeshProUGUI>().text = Creatures[i].Activity.ToString();
                        item.Find(strGender).GetComponent<Image>().sprite = Creatures[i].Gender == Gender.Male ? tempSprite[strMale] : tempSprite[strFemale];
                    }
                    else
                    {
                        item.Find(strIcon).GetComponent<Image>().sprite = tempSprite[str];
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
                BotKeyTips1.Find(strKT_Confirm).gameObject.SetActive(creature == null || preMode == Mode.Creature1 || canBreed);
                Creature_Recipe.Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textBreedResult;
                Creature_Recipe.Find(strWarn).gameObject.SetActive(showWarn);
                Creature_Recipe.Find(strWarn).GetComponent<TMPro.TextMeshProUGUI>().text = notTheSameCreature ? PanelTextContent.textWarnCreature : 
                    (notTheSameGender ? PanelTextContent.textWarnGender : str);
                Creature_Recipe.Find(strOutput).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textOutput;
                Creature_Recipe.Find(strOutputValue).gameObject.SetActive(canBreed);
                Creature_Recipe.Find(strEmpty).gameObject.SetActive(!canBreed);
                if (canBreed)
                {
                    int output1 = ProNode.Creature1.Output;
                    int output2 = creature.Output;
                    int low = -3 + (output1 <= output2 ? output1 : output2);
                    int high = 3 + (output1 <= output2 ? output2 : output1);
                    low = low >= 0 ? low : 0;
                    high = high <= 50 ? high : 50;
                    Creature_Recipe.Find(strOutputValue).GetComponent<TMPro.TextMeshProUGUI>().text = $"{low}~{high}";
                }
                bool isValidCreature = creature != null;
                string recipeID = isValidCreature ? creature.BreRecipeID : str;
                string timeText = PanelTextContent.textTime + strColonSignal;
                if (canBreed)
                {
                    timeText = timeText + ManagerNS.LocalGameManager.Instance.RecipeManager.GetTimeCost(recipeID).ToString() + strs;
                }
                Creature_Recipe.Find(strTime).GetComponent<TMPro.TextMeshProUGUI>().text = timeText;
                Creature_Recipe.Find(strEmpty1).gameObject.SetActive(!canBreed);
                #endregion

                #region Desc
                string productID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(recipeID).id;
                if (!tempSprite.ContainsKey(productID))
                {
                    tempSprite[productID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(productID);
                }
                Creature_Desc.Find(strIcon).GetComponent<Image>().sprite = tempSprite[productID];
                bool isValidItemID = ML.Engine.InventorySystem.ItemManager.Instance.IsValidItemID(productID);
                Creature_Desc.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? 
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(productID) : PanelTextContent.textEmpty;
                Creature_Desc.Find(strItemDesc).GetComponent<TMPro.TextMeshProUGUI>().text = isValidItemID ? 
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemDescription(productID) : PanelTextContent.textEmpty;
                #region Output
                Creature_Desc.Find(strOutput).Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textGetBreed;
                string proRecipeID = isValidCreature ? creature.ProRecipeID : str;
                string proProductID = ManagerNS.LocalGameManager.Instance.RecipeManager.GetProduct(proRecipeID).id;
                if (!tempSprite.ContainsKey(proProductID))
                {
                    tempSprite[proProductID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(proProductID);
                }
                Creature_Desc.Find(strOutput).Find(strIcon).GetComponent<Image>().sprite = tempSprite[proProductID];
                Creature_Desc.Find(strOutput).Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = 
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(proProductID);
                #endregion

                #region Discard
                Creature_Desc.Find(strDiscard).Find(strText).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textGetDiscard;
                string discardID = isValidCreature ? creature.Discard.id : str;
                int discardNum = isValidCreature ? creature.Discard.num : 0;
                if (!tempSprite.ContainsKey(discardID))
                {
                    tempSprite[discardID] = ManagerNS.LocalGameManager.Instance.ItemManager.GetItemSprite(discardID);
                }
                Creature_Desc.Find(strDiscard).Find(strIcon).GetComponent<Image>().sprite = tempSprite[discardID];
                Creature_Desc.Find(strDiscard).Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = 
                    ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(discardID);
                Creature_Desc.Find(strDiscard).Find(strNeedAmount).GetComponent<TMPro.TextMeshProUGUI>().text = discardNum.ToString();
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
                ProNode_UI.Find(strTime).GetComponent<TMPro.TextMeshProUGUI>().text = isProduce ? $"{minute} min {second} s" : str;
                Transform bar = ProNode_UI.Find(strBar);
                RectTransform barRect = bar.Find(strCur).GetComponent<RectTransform>();
                float posX = 300 * (1 - (float)(time / timeCost));
                barRect.sizeDelta = new Vector2(posX, barRect.sizeDelta.y);
            }
        }
        #endregion
    }
}