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
using ML.Engine.InventorySystem;
using ML.Engine.Timer;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.Manager;
using ML.Engine.Input;
using UnityEngine.InputSystem;
using static ProjectOC.TechTree.TechTreeManager;
using UnityEngine.ResourceManagement.AsyncOperations;
using Sirenix.OdinInspector;

namespace ProjectOC.TechTree.UI
{
    public sealed class UITechPointPanel : UIBasePanel<TPPanel>, ITickComponent
    {
        #region DrawCircle
        [LabelText("间距")]
        public float gapDistance = 100;
        private int sliceNum = 16;
        private Vector3 BasePos;

        private void SetBtnPos(RectTransform rectTransform,int[] cor)
        {
            if (cor.Length != 2) return;
            float angle = 270 - cor[0] * ((float)360.0 / sliceNum);
            angle = angle * Mathf.PI / 180;

            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            rectTransform.anchoredPosition = BasePos + dir * cor[1] * gapDistance;
        }

        private void LinkEdge(Transform edge, Transform obj)
        {
            // 旋转 -> Self to Target
            RectTransform UIA = edge.transform as RectTransform;
            RectTransform UIB = obj.transform as RectTransform;

            Vector3 v1 = UIB.anchoredPosition;//target
            Vector3 v2 = (UIA.parent.parent as RectTransform).anchoredPosition;//self
            Vector3 v3 = new Vector3(1, 0, 0);
            Vector3 v4 = v1 - v2;
            float angle = Vector3.Angle(v4, v3);
            angle = v1.y < v2.y ? -angle : angle;

            UIA.rotation = Quaternion.Euler(0, 0, angle);

            // Width : Distance(Self, Target) - Size
            float dis = Vector3.Distance(UIB.anchoredPosition, (UIA.parent.parent as RectTransform).anchoredPosition);// 
            var e = (edge as RectTransform);
            e.sizeDelta = new Vector2(dis, e.sizeDelta.y);
        }

        #endregion

        #region 数据初始化
        private Dictionary<string, Transform> btn_IdDic = new Dictionary<string, Transform>();
        private void InitData()
        {
            this.BasePos = this.cursorNavigation.Content.Find("UIBtnList").Find("Container").transform.position;
            this.EdgeParent = this.cursorNavigation.Content.Find("UIBtnList").Find("Container").Find("Edges");
            var AllID = TechTreeManager.Instance.GetAllTPID();
            for (int i = 0; i < AllID.Length; i++) 
            {
                string id = AllID[i];
                if (!this.TTTPGO.TryGetValue(id, out var obj))
                {
                    this.cursorNavigation.UIBtnList.AddBtn("Assets/_ProjectOC/OCResources/UI/TechPoint/TechPointTemplate.prefab",
                        OnSelectEnter: () =>
                        {
                            CurrentID = id;
                            this.Refresh();
                        },

                        OnSelectExit: () =>
                        {
                            CurrentID = "";
                            ClearTempOnAlterTP();
                            this.Refresh();
                        },
                        BtnSettingAction:
                        (btn) =>
                        {
                            
                            this.TTTPGO.Add(id, btn.gameObject);
                            btn.name = id;
                            var rec = btn.GetComponent<RectTransform>();
                            rec.localScale = new Vector3(1, 1, 1);
                            rec.anchoredPosition = Vector2.zero;
                            var icon = btn.transform.Find("Icon").GetComponent<Image>();
                            if (!tempSprite.ContainsKey(id))
                            {
                                this.tempSprite.Add(id, TechTreeManager.Instance.GetTPSprite(id));
                            }
                            icon.sprite = tempSprite[id];

                            btn_IdDic.Add(id, btn.transform);

                            SetBtnPos(rec, TechTreeManager.Instance.GetTPGrid(id));
                            
                            // 0 : Locked | 1 : Unlocked | 2 : Unlocking
                            int status = TechTreeManager.Instance.IsUnlockedTP(id) ? 1 : (TechTreeManager.Instance.UnlockingTPTimers.ContainsKey(id) ? 2 : 0);
                            // Mask & Timer
                            var mask = btn.transform.Find("Mask").GetComponent<Image>();
                            if (status == 0)
                            {
                                mask.fillAmount = 1;
                            }
                            else if (status == 1)
                            {
                                mask.fillAmount = 0;
                            }
                            else if (status == 2)
                            {
                                var timer = TechTreeManager.Instance.UnlockingTPTimers[id];
                                timer.OnUpdateEvent += (time) =>
                                {
                                    mask.fillAmount = (float)(timer.CurrentTime / TechTreeManager.Instance.GetTPTimeCost(id));
                                };
                                timer.OnEndEvent += Refresh;
                                mask.fillAmount = (float)(timer.CurrentTime / TechTreeManager.Instance.GetTPTimeCost(id));
                                tempTimer.Add(timer);
                            }

                            if(btn_IdDic.Count == AllID.Length)
                            {
                                //连边
                                foreach (var (id, tbtn) in btn_IdDic)
                                {
                                    Transform edgetmp = tbtn.Find("EdgeParent").Find("EdgeTemplate");
                                    string[] ts = TechTreeManager.Instance.GetPreTechPoints(id);

                                    foreach (var s in ts)
                                    {
                                        if (btn_IdDic.ContainsKey(s))
                                        {
                                            var edge = GameObject.Instantiate(edgetmp.gameObject, edgetmp.parent).transform;
                                            edge.gameObject.SetActive(true);
                                            var obj = btn_IdDic[s];
                                            LinkEdge(edge, obj);
                                            edge.SetParent(this.EdgeParent, true);
                                        }
                                    }
                                }
                            }
                        }
                        );
                }
            }
        }


        [ShowInInspector]

        private string CurrentID = "";

        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        #endregion

        public IInventory inventory;
        /// <summary>
        /// 是否满足破译当前节点的条件
        /// </summary>
        private bool CanDecipher => TechTreeManager.Instance.CanUnlockTechPoint(inventory, CurrentID);

        private TextMeshProUGUI topTitle;

        #region TechPointPanel

        private Image TPIcon;

        private TextMeshProUGUI TPName;

        private TextMeshProUGUI TPDescription;

        /// <summary>
        /// 可破译项不同语言的提示
        /// </summary>
        private TextMeshProUGUI TPDecipherTip;

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
        private Transform TPKT_Decipher;

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

        private Transform EmptyPanel;
        #endregion

        #region GraphCursorNavigation
        [ShowInInspector]
        private GraphCursorNavigation cursorNavigation;
        private Transform EdgeParent;
        #endregion

        #region Unity
        protected override void Awake()
        {
            base.Awake();

            topTitle = this.transform.Find("TopPanel").GetComponentInChildren<TextMeshProUGUI>();

            Transform ContentPanel = this.transform.Find("ContentPanel");
            #region TechPointPanel
            ContentPanel = this.transform.Find("ContentPanel").Find("TechPointPanel").Find("Viewport").Find("Content");

            this.TPIcon = ContentPanel.Find("Icon").GetComponent<Image>();
            this.TPName = ContentPanel.Find("Name").GetComponent<TextMeshProUGUI>();
            this.TPDescription = ContentPanel.Find("Description").GetComponent<TextMeshProUGUI>();

            this.TPDecipherTip = ContentPanel.Find("TitleTip").Find("TipText").GetComponent<TextMeshProUGUI>();

            this.TPUnlockTemplate = ContentPanel.Find("UnlockIDList").Find("Viewport").Find("Content").Find("UnlockTemplate");
            this.TPUnlockTemplate.gameObject.SetActive(false);

            this.TPLockedState = ContentPanel.Find("InformationInspector").Find("Locked");
            this.TPKT_Decipher = this.TPLockedState.Find("Viewport").Find("Content").Find("KT_Decipher");

            this.TPTimeCost = this.TPLockedState.Find("Viewport").Find("Content").Find("TimeCost").GetComponent<TextMeshProUGUI>();
            this.TPItemCostTemplate = this.TPLockedState.Find("Viewport").Find("Content").Find("ItemCostTemplate");
            this.TPItemCostTemplate.gameObject.SetActive(false);

            this.TPUnlockedState = ContentPanel.Find("InformationInspector").Find("Unlocked");
            this.TPUnlockedState.gameObject.SetActive(false);

            this.TPUnlockingState = ContentPanel.Find("InformationInspector").Find("Unlocking");
            this.TPUnlockingTimeCost = this.TPUnlockingState.Find("DownTimer").Find("Time").GetComponent<TextMeshProUGUI>();
            this.TPUnlockingProgressBar = this.TPUnlockingState.Find("ProgressBar").GetComponent<Slider>();
            this.TPUnlockingState.gameObject.SetActive(false);

            this.EmptyPanel = this.transform.Find("ContentPanel").Find("EmptyPanel");
            #endregion
            this.cursorNavigation = this.transform.GetComponentInChildren<GraphCursorNavigation>();
        }

        protected override void Start()
        {
            base.Start();
        }


        protected override void OnDestroy()
        {
            ClearTemp();
            (this as ITickComponent).DisposeTick();
            // 肯定是通过实例化的形式产生的
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.ReleaseInstance(this.gameObject);
        }
        #endregion

        #region Override
        /// <summary>
        /// 按键输入
        /// </summary>
        public void Tick(float deltatime)
        {
            // 破译
            if (ProjectOC.Input.InputManager.PlayerInput.TechTree.Decipher.WasPressedThisFrame())
            {
                Decipher();
            }
            // 退出
            if (ML.Engine.Input.InputManager.Instance.Common.Common.Back.WasPressedThisFrame())
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ProjectOC.Input.InputManager.PlayerInput.TechTree.Enable();
            this.cursorNavigation.EnableGraphCursorNavigation(ProjectOC.Input.InputManager.PlayerInput.TechTree.AlterTP);
            InitData();
        }

        public override void OnPause()
        {
            base.OnPause();
            ProjectOC.Input.InputManager.PlayerInput.TechTree.Disable();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            ProjectOC.Input.InputManager.PlayerInput.TechTree.Enable();
            Refresh();
        }

        public override void OnExit()
        {
            base.OnExit();
            ProjectOC.Input.InputManager.PlayerInput.TechTree.Disable();
            this.cursorNavigation.DisableGraphCursorNavigation();
        }

        protected override void Enter()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            base.Enter();
        }

        protected override void Exit()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            base.Exit();
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



        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        private List<CounterDownTimer> tempTimer = new List<CounterDownTimer>();
        private List<GameObject> tempGO = new List<GameObject>();
        /// <summary>
        /// Category对应的UIGO
        /// </summary>
        private Dictionary<TechPointCategory, GameObject> TPCGO = new Dictionary<TechPointCategory, GameObject>();
        /// <summary>
        /// TechTreePanel中TP对应的UIGO
        /// </summary>
        private Dictionary<string, GameObject> TTTPGO = new Dictionary<string, GameObject>();
        /// <summary>
        /// TechTreePanel中TP的EDGE对应的UIGO
        /// </summary>
        private Dictionary<string, GameObject> TTTPEGO = new Dictionary<string, GameObject>();

        /// <summary>
        /// TechPointPanel可解锁项的UIGO
        /// </summary>
        private Dictionary<string, GameObject> TPUnlockGO = new Dictionary<string, GameObject>();
        /// <summary>
        /// TechPointPanel破译消耗的Item的UIGO
        /// </summary>
        private Dictionary<string, GameObject> TPCItemGO = new Dictionary<string, GameObject>();



        private void ClearTempOnAlterTP()
        {
            foreach (var s in TPCItemGO.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            TPCItemGO.Clear();
            foreach (var s in TPUnlockGO.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            TPUnlockGO.Clear();

        }

        private void ClearTemp()
        {
            // sprite
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
            }
            tempSprite.Clear();
            // TPCategory
            foreach (var s in TPCGO.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            TPCGO.Clear();
            // TechTree
            foreach (var s in TTTPGO.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            TTTPGO.Clear();
            foreach (var s in TTTPEGO.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            TTTPEGO.Clear();
            // TechPoint
            foreach (var s in TPCItemGO.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            TPCItemGO.Clear();
            foreach (var s in TPUnlockGO.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            TPUnlockGO.Clear();

            // Temp
            foreach (var s in tempGO)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            tempGO.Clear();
            foreach (var s in tempTimer)
            {
                s.OnUpdateEvent = null;
                s.OnEndEvent -= Refresh;
            }
            tempTimer.Clear();
        }

        public override void Refresh()
        {
            if (!this.objectPool.IsLoadFinish()) 
            {
                return;
            }

            foreach (var t in tempTimer)
            {
                t.OnUpdateEvent = null;
                t.OnEndEvent -= Refresh;
            }
            tempTimer.Clear();

            topTitle.text = PanelTextContent.toptitle.GetText();

            this.EmptyPanel.gameObject.SetActive(CurrentID == "");
            #region TechPointPanel
            if(!string.IsNullOrEmpty(CurrentID))
            {
                // 0 : Locked | 1 : Unlocked | 2 : Unlocking
                int tpStatus = TechTreeManager.Instance.IsUnlockedTP(CurrentID) ? 1 : (TechTreeManager.Instance.UnlockingTPTimers.ContainsKey(CurrentID) ? 2 : 0);

                var TM = TechTreeManager.Instance;

                // Icon
                var ts = TM.GetTPSprite(CurrentID);
                if (!tempSprite.ContainsKey(CurrentID))
                {
                    tempSprite.Add(CurrentID, ts);
                }

                this.TPIcon.sprite = ts;


                // Name
                this.TPName.text = TM.GetTPName(CurrentID);
                // Description
                this.TPDescription.text = TM.GetTPDescription(CurrentID);
                // DecipherTip
                this.TPDecipherTip.text = tpStatus == 1 ? PanelTextContent.unlockedtitletip.GetText() : PanelTextContent.lockedtitletip.GetText();


                // 可解锁项
                foreach (var id in TM.GetTPCanUnlockedID(CurrentID))
                {
                    if (!TPUnlockGO.TryGetValue(id, out var unlock))
                    {
                        unlock = Instantiate(TPUnlockTemplate.gameObject, TPUnlockTemplate.parent, false);
                        unlock.SetActive(true);
                        this.TPUnlockGO.Add(id, unlock);
                    }

                    // Image
                    if (tempSprite.ContainsKey(id))
                    {
                        var s = tempSprite[id];
                        unlock.GetComponentInChildren<Image>().sprite = s;
                    }
                    else
                    {
                        var s = CompositeManager.Instance.GetCompositonSprite(GetTPIconItemID(id));
                        tempSprite.Add(id, s);
                        unlock.GetComponentInChildren<Image>().sprite = s;
                    }
                    // Text
                    unlock.GetComponentInChildren<TextMeshProUGUI>().text = CompositeManager.Instance.GetCompositonName(GetTPIconItemID(id));
                }

                // 面板状态
                // 未解锁u
                if (tpStatus == 0)
                {
                    this.TPLockedState.gameObject.SetActive(true);
                    this.TPUnlockedState.gameObject.SetActive(false);
                    this.TPUnlockingState.gameObject.SetActive(false);

                    // 是否可以破译
                    // to-do : UI
                    bool canDecipher = CanDecipher;
                    this.TPKT_Decipher.Find("CanDecipherImg").GetComponent<Image>().color = canDecipher ? new Color32(77, 233, 16, 255) : Color.gray;
                    this.TPKT_Decipher.Find("Mask").GetComponent<Image>().gameObject.SetActive(!canDecipher);

                    // 时间消耗
                    this.TPTimeCost.text = PanelTextContent.timecosttip + TM.GetTPTimeCost(CurrentID).ToString() + "s";

                    // Item 消耗
                    foreach (var f in TM.GetTPItemCost(CurrentID))
                    {
                        if (!this.TPCItemGO.TryGetValue(f.id, out var item))
                        {
                            item = Instantiate(TPItemCostTemplate.gameObject, TPItemCostTemplate.parent, false);
                            item.SetActive(true);
                            this.TPCItemGO.Add(f.id, item);

                            // Image
                            if (!tempSprite.ContainsKey(f.id))
                            {
                                tempSprite.Add(f.id, ItemManager.Instance.GetItemSprite(f.id));
                            }
                            var s = tempSprite[f.id];
                            s.name = s.name.Replace("(Clone)", "");
                            item.transform.Find("Image").GetComponent<Image>().sprite = s;
                        }

                        // Num
                        item.transform.Find("Image").Find("Num").GetComponent<TextMeshProUGUI>().text = f.num.ToString();

                        // Name
                        item.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(f.id);
                    }
                }
                // 已解锁
                else if (tpStatus == 1)
                {
                    this.TPLockedState.gameObject.SetActive(false);
                    this.TPUnlockedState.gameObject.SetActive(true);
                    this.TPUnlockingState.gameObject.SetActive(false);
                }
                // 正在解锁
                else if (tpStatus == 2)
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
            }

            
            #endregion

            foreach (var (id, btn) in btn_IdDic) 
            {
                // 0 : Locked | 1 : Unlocked | 2 : Unlocking
                int status = TechTreeManager.Instance.IsUnlockedTP(id) ? 1 : (TechTreeManager.Instance.UnlockingTPTimers.ContainsKey(id) ? 2 : 0);
                // Mask & Timer
                var mask = btn.transform.Find("Mask").GetComponent<Image>();
                if (status == 0)
                {
                    mask.fillAmount = 1;
                }
                else if (status == 1)
                {
                    mask.fillAmount = 0;
                }
                else if (status == 2)
                {
                    var timer = TechTreeManager.Instance.UnlockingTPTimers[id];
                    timer.OnUpdateEvent += (time) =>
                    {
                        mask.fillAmount = (float)(timer.CurrentTime / TechTreeManager.Instance.GetTPTimeCost(id));
                    };
                    timer.OnEndEvent += Refresh;
                    mask.fillAmount = (float)(timer.CurrentTime / TechTreeManager.Instance.GetTPTimeCost(id));
                    tempTimer.Add(timer);
                }
            }
        }

        private string GetTPIconItemID(string id)
        {
            if(BuildingManager.Instance.BPartTableDictOnID.ContainsKey(id))
            {
                return id;
            }
            else if(ManagerNS.LocalGameManager.Instance.RecipeManager.IsValidID(id))
            {
                return id;
            }
            throw new Exception($"科技树配置中的可解锁项ID\"{id}\"既不是RecipeID，也不是BuildID");
        }

        protected override void OnLoadJsonAssetComplete(TPPanel datas)
        {
            base.OnLoadJsonAssetComplete(datas);
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/TechTree";
            this.abname = "TechPointPanel";
            this.description = "TechPointPanel数据加载完成";
        }

        protected override void InitBtnInfo()
        {
            this.cursorNavigation.InitUIBtnList();
        }
        #endregion
    }
}
