using ML.Engine.BuildingSystem;
using ML.Engine.TextContent;
using ML.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Purchasing;
using ML.Engine.InventorySystem;
using ML.Engine.Timer;
using ML.Engine.InventorySystem.CompositeSystem;
using BehaviorDesigner.Runtime.Tasks.Movement;

namespace ProjectOC.TechTree.UI
{
    public sealed class UITechPointPanel : UIBasePanel, ITickComponent
    {
        #region 静态数据初始化
        private static bool IsInit = false;
        private static void InitStaticData()
        {
            if(IsInit)
            {
                return;
            }

            category = ((TechPointCategory[])Enum.GetValues(typeof(TechPointCategory))).Where(c => (int)c > 0).ToArray();
            RefreshCategory(0);

        }

        private static TechPointCategory[] category;
        private static int cIndex;

        private static string[] TechPointList;
        private static Vector2Int GridRange;
        private static Vector2Int CurrentGrid;
        private static string CurrentID
        {
            get
            {
                return TechPointList[CurrentGrid.x * GridRange.y + CurrentGrid.y];
            }
        }


        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        private static void RefreshTechPointList()
        {
            var tpids = TechTreeManager.Instance.GetAllTPID().Where(id => TechTreeManager.Instance.GetTPCategory(id) == category[cIndex]);
            int gridX = 0;
            int gridY = 0;
            foreach (var id in tpids)
            {
                var grid = TechTreeManager.Instance.GetTPGrid(id);
                gridX = Math.Max(grid[0], gridX);
                gridY = Math.Max(grid[1], gridY);
            }
            GridRange = new Vector2Int(gridX + 1, gridY + 1);
            TechPointList = new string[GridRange.x * GridRange.y];
            foreach (var id in tpids)
            {
                var grid = TechTreeManager.Instance.GetTPGrid(id);
                TechPointList[grid[0] * GridRange.y + grid[1]] = id;
            }
            int[] g = TechTreeManager.Instance.GetTPGrid(TechPointList.First(id => (id != null && id != "")));
            CurrentGrid = new Vector2Int(g[0], g[0]);
        }

        private static void RefreshCategory(int index)
        {
            cIndex = index;
            RefreshTechPointList();
        }

        private static void AlterTP(Vector2Int offset)
        {
            do
            {
                CurrentGrid.x = (CurrentGrid.x + GridRange.x + offset.y) % GridRange.x;
                CurrentGrid.y = (CurrentGrid.y + GridRange.y + offset.x) % GridRange.y;
            } while (CurrentID == null || CurrentID == "");
        }

        #endregion

        public Inventory inventory;
        /// <summary>
        /// 是否满足破译当前节点的条件
        /// </summary>
        private bool CanDecipher => TechTreeManager.Instance.CanUnlockTechPoint(inventory, CurrentID);

        private TextMeshProUGUI topTitle;

        #region CategoryPanel
        private UIKeyTip categoryLast;
        private Transform categoryTemplate;
        private UIKeyTip categoryNext;
        #endregion

        #region TechTreePanel
        public Transform TechPointTemplate;
        #endregion

        #region TechPointPanel
        private Image TPIcon;

        private TextMeshProUGUI TPName;

        private TextMeshProUGUI TPDescription;

        /// <summary>
        /// 可破译项不同语言的提示
        /// </summary>
        private TextMeshProUGUI TPDecipherTip;

        private UIKeyTip TPKTInspector;

        /// <summary>
        /// 破译后可以解锁项的UI模板
        /// </summary>
        private Transform TPUnlockTemplate;

        /// <summary>
        /// TP节点处于未解锁状态
        /// </summary>
        private Transform TPLockedState;

        /// <summary>
        /// 破译按键提示
        /// </summary>
        private UIKeyTip TPKT_Decipher;

        /// <summary>
        /// 破译的时间消耗
        /// </summary>
        private TextMeshProUGUI TPTimeCost;

        /// <summary>
        /// 破译消耗Item的UI模板
        /// </summary>
        private Transform TPItemCostTemplate;

        /// <summary>
        /// TP节点处于解锁状态
        /// </summary>
        private Transform TPUnlockedState;

        /// <summary>
        /// TP节点处于正在解锁(破译)状态
        /// </summary>
        private Transform TPUnlockingState;

        /// <summary>
        /// 正在破译的倒计时Text
        /// </summary>
        private TextMeshProUGUI TPUnlockingTimeCost;

        /// <summary>
        /// 正在破译的倒计时进度条
        /// </summary>
        private Slider TPUnlockingProgressBar;

        #endregion

        #region BotPanel
        private UIKeyTip KT_Back;
        #endregion

        #region Unity
        private void Awake()
        {

            topTitle = this.transform.Find("TopPanel").GetComponentInChildren<TextMeshProUGUI>();

            Transform ContentPanel = this.transform.Find("ContentPanel");
            #region CategoryPanel
            Transform categoryPanel = ContentPanel.Find("CategoryPanel");

            Transform clast = categoryPanel.Find("Last");
            this.categoryLast = new UIKeyTip();
            this.categoryLast.img = clast.GetComponentInChildren<Image>();
            this.categoryLast.keytip = this.categoryLast.img.GetComponentInChildren<TextMeshProUGUI>();

            Transform cnext = categoryPanel.Find("Next");
            this.categoryNext = new UIKeyTip();
            this.categoryNext.img = cnext.GetComponentInChildren<Image>();
            this.categoryNext.keytip = this.categoryNext.img.GetComponentInChildren<TextMeshProUGUI>();

            this.categoryTemplate = categoryPanel.Find("Content").Find("CategoryTemplate");
            this.categoryTemplate.gameObject.SetActive(false);
            #endregion

            #region TechTreePanel
            this.TechPointTemplate = ContentPanel.Find("TechTreePanel").Find("Viewport").Find("Content").Find("TechPointTemplate");

            this.TechPointTemplate.gameObject.SetActive(false);
            #endregion

            #region TechPointPanel
            ContentPanel = this.transform.Find("ContentPanel").Find("TechPointPanel").Find("Viewport").Find("Content");

            this.TPIcon = ContentPanel.Find("Icon").GetComponent<Image>();
            this.TPName = ContentPanel.Find("Name").GetComponent<TextMeshProUGUI>();
            this.TPDescription = ContentPanel.Find("Description").GetComponent<TextMeshProUGUI>();

            this.TPDecipherTip = ContentPanel.Find("TitleTip").Find("TipText").GetComponent<TextMeshProUGUI>();
            this.TPKTInspector = new UIKeyTip();
            this.TPKTInspector.img = ContentPanel.Find("TitleTip").Find("KT_Inspect").Find("Image").GetComponent<Image>();
            this.TPKTInspector.keytip = this.TPKTInspector.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            this.TPKTInspector.description = this.TPKTInspector.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();

            this.TPUnlockTemplate = ContentPanel.Find("UnlockIDList").Find("Viewport").Find("Content").Find("UnlockTemplate");
            this.TPUnlockTemplate.gameObject.SetActive(false);

            this.TPLockedState = ContentPanel.Find("InformationInspector").Find("Locked");
            this.TPKT_Decipher = new UIKeyTip();
            this.TPKT_Decipher.img = this.TPLockedState.Find("Viewport").Find("Content").Find("KT_Decipher").Find("Image").GetComponent<Image>();
            this.TPKT_Decipher.keytip = this.TPKT_Decipher.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            this.TPKT_Decipher.description = this.TPKT_Decipher.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            this.TPTimeCost = this.TPLockedState.Find("Viewport").Find("Content").Find("TimeCost").GetComponent<TextMeshProUGUI>();
            this.TPItemCostTemplate = this.TPLockedState.Find("Viewport").Find("Content").Find("ItemCostTemplate");
            this.TPItemCostTemplate.gameObject.SetActive(false);

            this.TPUnlockedState = ContentPanel.Find("InformationInspector").Find("Unlocked");
            this.TPUnlockedState.gameObject.SetActive(false);

            this.TPUnlockingState = ContentPanel.Find("InformationInspector").Find("Unlocking");
            this.TPUnlockingTimeCost = this.TPUnlockingState.Find("DownTimer").Find("Time").GetComponent<TextMeshProUGUI>();
            this.TPUnlockingProgressBar = this.TPUnlockingState.Find("ProgressBar").GetComponent<Slider>();
            this.TPUnlockingState.gameObject.SetActive(false);
            #endregion

            #region BackPanel
            ContentPanel = this.transform.Find("ContentPanel").Find("BotPanel").Find("KT_Back");
            this.KT_Back = new UIKeyTip();
            this.KT_Back.img = ContentPanel.Find("Image").GetComponent<Image>();
            this.KT_Back.keytip = this.KT_Back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            this.KT_Back.description = this.KT_Back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            #endregion
        }

        private void Start()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            this.enabled = false;
        }

        private void OnDestroy()
        {
            ClearTemp();

        }
        #endregion

        #region Override
        /// <summary>
        /// 按键输入
        /// </summary>
        public void Tick(float deltatime)
        {
            // 向前
            if (ProjectOC.Input.InputManager.TechTreeInput.TechTree.LastTerm.WasPressedThisFrame())
            {
                RefreshCategory((cIndex - 1 + category.Length) % category.Length);
                Refresh();
            }
            // 向后
            if (ProjectOC.Input.InputManager.TechTreeInput.TechTree.NextTerm.WasPressedThisFrame())
            {
                RefreshCategory((cIndex + 1 + category.Length) % category.Length);
                Refresh();
            }
            // 破译
            if (ProjectOC.Input.InputManager.TechTreeInput.TechTree.Decipher.WasPressedThisFrame())
            {
                Decipher();
            }
            // 退出
            if (ML.Engine.Input.InputManager.Instance.Common.Common.Back.WasPressedThisFrame())
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
            }
            // 科技点
            if (ProjectOC.Input.InputManager.TechTreeInput.TechTree.AlterTP.WasPressedThisFrame())
            {
                Vector2Int offset = Vector2Int.RoundToInt(ProjectOC.Input.InputManager.TechTreeInput.TechTree.AlterTP.ReadValue<Vector2>());
                AlterTP(offset);
                Refresh();
            }
        }

        public override void OnEnter()
        {
            InitStaticData();

            base.OnEnter();
            ProjectOC.Input.InputManager.TechTreeInput.Enable();
            ProjectOC.Input.InputManager.PlayerInput.Disable();
            Refresh();
        }

        public override void OnPause()
        {
            base.OnPause();
            ProjectOC.Input.InputManager.PlayerInput.Enable();
            ProjectOC.Input.InputManager.TechTreeInput.Disable();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            ProjectOC.Input.InputManager.PlayerInput.Disable();
            ProjectOC.Input.InputManager.TechTreeInput.Enable();
            Refresh();
        }

        public override void OnExit()
        {
            base.OnExit();
            ProjectOC.Input.InputManager.PlayerInput.Enable();
            ProjectOC.Input.InputManager.TechTreeInput.Disable();
        }
        #endregion

        #region Internal

        private void Decipher()
        {
            if (this.CanDecipher)
            {
                TechTreeManager.Instance.UnlockTechPoint(inventory, CurrentID, false);
                Refresh();
            }
        }

        private List<GameObject> tempGO = new List<GameObject>();
        private List<Sprite> tempSprite = new List<Sprite>();
        private List<CounterDownTimer> tempTimer = new List<CounterDownTimer>();

        private void ClearTemp()
        {
            foreach(var s in tempSprite)
            {
                Destroy(s);
            }
            tempSprite.Clear();
            foreach (var s in tempGO)
            {
                Destroy(s);
            }
            tempGO.Clear();
            foreach (var s in tempTimer)
            {
                s.OnUpdateEvent = null;
            }
            tempTimer.Clear();
        }

        private void Refresh()
        {
            ClearTemp();

            topTitle.text = TechTreeManager.Instance.TPPanelTextContent.toptitle.GetText();

            #region CategoryPanel
            this.categoryLast.ReWrite(TechTreeManager.Instance.TPPanelTextContent.categorylast);
            this.categoryNext.ReWrite(TechTreeManager.Instance.TPPanelTextContent.categorynext);

            foreach(var c in category)
            {
                var obj = GameObject.Instantiate(this.categoryTemplate.gameObject, this.categoryTemplate.parent, false);
                obj.SetActive(true);
                var sprite = TechTreeManager.Instance.GetTPCategorySprite(c);
                obj.GetComponentInChildren<Image>().sprite = sprite;
                obj.GetComponentInChildren<TextMeshProUGUI>().text = TechTreeManager.Instance.CategoryDict[c.ToString()].GetDescription();


                tempSprite.Add(sprite);
                tempGO.Add(obj);

                if(c != category[cIndex])
                {
                    DisactiveCategory(obj);
                }
                else
                {
                    ActiveCategory(obj);
                }
            }
            #endregion

            #region TechTreePanel
            // 设置Grid Layout 每行的数量为科技点的最深的层数
            var gridlayout = this.TechPointTemplate.parent.GetComponent<GridLayoutGroup>();
            gridlayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridlayout.constraintCount = GridRange.y;
            Dictionary<string, GameObject> tpPoint = new Dictionary<string, GameObject>();
            // 加点
            foreach (string id in TechPointList)
            {
                if(id == "" || id == null)
                {
                    var obj = new GameObject();
                    obj.transform.SetParent(this.TechPointTemplate.parent);
                    this.tempGO.Add(obj);
                    obj.AddComponent<RectTransform>();

                }
                else
                {
                    var obj = GameObject.Instantiate(this.TechPointTemplate.gameObject);
                    obj.transform.SetParent(this.TechPointTemplate.parent);
                    obj.SetActive(true);
                    this.tempGO.Add(obj);

                    // 0 : Locked | 1 : Unlocked | 2 : Unlocking
                    int status = TechTreeManager.Instance.IsUnlockedTP(id) ? 1 : (TechTreeManager.Instance.UnlockingTPTimers.ContainsKey(id) ? 2 :0);

                    // Icon
                    var icon = obj.transform.Find("Icon").GetComponent<Image>();
                    icon.sprite = TechTreeManager.Instance.GetTPSprite(id);
                    this.tempSprite.Add(icon.sprite);
                    // Mask & Timer
                    var mask = obj.transform.Find("Mask").GetComponent<Image>();
                    if (status == 0)
                    {
                        mask.fillAmount = 1;
                    }
                    else if(status == 1)
                    {
                        mask.fillAmount = 0;
                    }
                    else if (status == 2)
                    {
                        var timer = TechTreeManager.Instance.UnlockingTPTimers[id];
                        timer.OnUpdateEvent += (time) =>
                        {
                            mask.fillAmount = (float)(timer.CurrentTime / TechTreeManager.Instance.GetTPTimeCost(id));
                            
                            if (timer.IsTimeUp)
                            {
                                timer.OnUpdateEvent = null;
                                Refresh();
                            }
                        };
                        mask.fillAmount = (float)(timer.CurrentTime / TechTreeManager.Instance.GetTPTimeCost(id));
                        tempTimer.Add(timer);
                    }
                    // Select
                    var selected = obj.transform.Find("Selected").gameObject;
                    selected.SetActive((CurrentID == id));

                    tpPoint.Add(id, obj);
                }
            }

            // 强制立即更新 GridLayoutGroup 的布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridlayout.GetComponent<RectTransform>());
            // Edge : 动态调整 Width Rotation.z, Height固定为10
            foreach (var kv in tpPoint)
            {
                var edgeTemplate = kv.Value.transform.Find("EdgeParent").Find("EdgeTemplate");
                edgeTemplate.gameObject.SetActive(false);
                var preTP = TechTreeManager.Instance.GetPreTechPoints(kv.Key);
                Color32 color = TechTreeManager.Instance.IsUnlockedTP(kv.Key) ? new Color32(255, 165, 0, 255) : Color.white;
                foreach (var tp in preTP)
                {
                    var edge = GameObject.Instantiate(edgeTemplate, edgeTemplate.parent);
                    tempGO.Add(edge.gameObject);
                    edge.gameObject.SetActive(true);

                    var obj = tpPoint[tp];

                    // 旋转parnet -> Selt to Target
                    RectTransform UIA = edge.parent.transform as RectTransform;
                    RectTransform UIB = obj.transform as RectTransform;
                    UIA.rotation = Quaternion.Euler(0, 0, Vector3.Angle(new Vector3(1, 0, 0), UIB.position - UIA.position));

                    // Width : Distance(Self, Target) - 100
                    float dis = Vector3.Distance(UIB.position, (UIA.parent as RectTransform).position) - 100;
                    var e = (edge as RectTransform);
                    e.sizeDelta = new Vector2(dis, e.sizeDelta.y);

                    // IsUnlocked
                    edge.GetComponent<Image>().color = color;
                }

            }
            #endregion

            #region TechPointPanel
            // 0 : Locked | 1 : Unlocked | 2 : Unlocking
            int tpStatus = TechTreeManager.Instance.IsUnlockedTP(CurrentID) ? 1 : (TechTreeManager.Instance.UnlockingTPTimers.ContainsKey(CurrentID) ? 2 : 0);

            var TM = TechTreeManager.Instance;

            // Icon
            var s = TM.GetTPSprite(CurrentID);
            tempSprite.Add(s);
            this.TPIcon.sprite = s;

            // Name
            this.TPName.text = TM.GetTPName(CurrentID);

            // Description
            this.TPDescription.text = TM.GetTPDescription(CurrentID);

            // DecipherTip
            this.TPDecipherTip.text = tpStatus == 1 ? TM.TPPanelTextContent.unlockedtitletip.GetText() : TM.TPPanelTextContent.lockedtitletip.GetText();

            // KTInspector
            TPKTInspector.ReWrite(TM.TPPanelTextContent.inspector);

            // 可解锁项
            foreach(var id in TM.GetTPCanUnlockedID(CurrentID))
            {
                var unlock = Instantiate(TPUnlockTemplate.gameObject, TPUnlockTemplate.parent, false);
                unlock.SetActive(true);

                // Image
                s = CompositeSystem.Instance.GetCompositonSprite(id);
                tempSprite.Add(s);
                unlock.GetComponentInChildren<Image>().sprite = s;

                // Text
                unlock.GetComponentInChildren<TextMeshProUGUI>().text = CompositeSystem.Instance.GetCompositonName(id);

                this.tempGO.Add(unlock);
            }
            
            // 面板状态
            // 未解锁
            if(tpStatus == 0)
            {
                this.TPLockedState.gameObject.SetActive(true);
                this.TPUnlockedState.gameObject.SetActive(false);
                this.TPUnlockingState.gameObject.SetActive(false);

                // 破译按键提示
                this.TPKT_Decipher.ReWrite(TM.TPPanelTextContent.decipher);

                // 是否可以破译
                // to-do : UI
                bool canDecipher = CanDecipher;
                this.TPKT_Decipher.img.transform.parent.Find("CanDecipherImg").GetComponent<Image>().color = canDecipher ? new Color32(77, 233, 16, 255) : Color.gray;
                this.TPKT_Decipher.img.transform.parent.Find("Mask").GetComponent<Image>().gameObject.SetActive(!canDecipher);

                // 时间消耗
                this.TPTimeCost.text = "时间: " + TM.GetTPTimeCost(CurrentID).ToString() + "s";

                // Item 消耗
                foreach(var f in TM.GetTPItemCost(CurrentID))
                {
                    var item = Instantiate(TPItemCostTemplate.gameObject, TPItemCostTemplate.parent, false);
                    item.SetActive(true);

                    // Image
                    s = ItemSpawner.Instance.GetItemSprite(f.id);
                    tempSprite.Add(s);
                    item.transform.Find("Image").GetComponent<Image>().sprite = s;

                    // Num
                    item.transform.Find("Image").Find("Num").GetComponent<TextMeshProUGUI>().text = f.num.ToString();

                    // Name
                    item.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = ItemSpawner.Instance.GetItemName(f.id);

                    this.tempGO.Add(item);

                }
            }
            // 已解锁
            else if(tpStatus == 1)
            {
                this.TPLockedState.gameObject.SetActive(false);
                this.TPUnlockedState.gameObject.SetActive(true);
                this.TPUnlockingState.gameObject.SetActive(false);
            }
            // 正在解锁
            else if(tpStatus == 2)
            {
                this.TPLockedState.gameObject.SetActive(false);
                this.TPUnlockedState.gameObject.SetActive(false);
                this.TPUnlockingState.gameObject.SetActive(true);

                var timer = TechTreeManager.Instance.UnlockingTPTimers[CurrentID];

                // 正在破译倒计时Text
                this.TPUnlockingTimeCost.text = timer.CurrentTime.ToString("0.00");
                // 正在破译倒计时进度条
                this.TPUnlockingProgressBar.value = (float)timer.CurrentTime / TM.GetTPTimeCost(CurrentID);

                timer.OnUpdateEvent += (time) =>
                {
                    // 正在破译倒计时Text
                    this.TPUnlockingTimeCost.text = timer.CurrentTime.ToString("0.00");
                    // 正在破译倒计时进度条
                    this.TPUnlockingProgressBar.value = (float)timer.CurrentTime / TM.GetTPTimeCost(CurrentID);

                    // 不需要刷新。因为 TechTree 加入的里面已经带有刷新
                    //if (timer.IsTimeUp)
                    //{
                    //    timer.OnUpdateEvent = null;
                    //    Refresh();
                    //}
                };
                // 不需要加入，因为前面 TechTree 时已经加入了
                //tempTimer.Add(timer);
            }
            #endregion

            #region BotPanel
            this.KT_Back.ReWrite(TM.TPPanelTextContent.back);
            #endregion
        }


        #region to-do : UI
        /// <summary>
        /// to-do : UI
        /// </summary>
        private void ActiveCategory(GameObject obj)
        {
            obj.GetComponentInChildren<Image>().color = Color.cyan;

        }

        /// <summary>
        /// to-do : UI
        /// </summary>
        private void DisactiveCategory(GameObject obj)
        {
            obj.GetComponentInChildren<Image>().color = Color.white;
        }
        #endregion

        #endregion
    }

}
