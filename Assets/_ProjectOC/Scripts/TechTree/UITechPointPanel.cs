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
        #region ��̬���ݳ�ʼ��
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
        private static Vector2Int LastGrid;
        private static string LastID
        {
            get
            {
                return TechPointList[LastGrid.x * GridRange.y + LastGrid.y];
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
                LastGrid = CurrentGrid;
                CurrentGrid.x = (CurrentGrid.x + GridRange.x - offset.y) % GridRange.x;
                CurrentGrid.y = (CurrentGrid.y + GridRange.y + offset.x) % GridRange.y;
            } while (CurrentID == null || CurrentID == "");
        }

        #endregion

        public Inventory inventory;
        /// <summary>
        /// �Ƿ��������뵱ǰ�ڵ������
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
        public ScrollRect TTTPScrollRect;
        #endregion

        #region TechPointPanel
        private Image TPIcon;

        private TextMeshProUGUI TPName;

        private TextMeshProUGUI TPDescription;

        /// <summary>
        /// �������ͬ���Ե���ʾ
        /// </summary>
        private TextMeshProUGUI TPDecipherTip;

        private UIKeyTip TPKTInspector;

        /// <summary>
        /// �������Խ������UIģ��
        /// </summary>
        private Transform TPUnlockTemplate;

        /// <summary>
        /// TP�ڵ㴦��δ����״̬
        /// </summary>
        private Transform TPLockedState;

        /// <summary>
        /// ���밴����ʾ
        /// </summary>
        private UIKeyTip TPKT_Decipher;

        /// <summary>
        /// �����ʱ������
        /// </summary>
        private TextMeshProUGUI TPTimeCost;

        /// <summary>
        /// ��������Item��UIģ��
        /// </summary>
        private Transform TPItemCostTemplate;

        /// <summary>
        /// TP�ڵ㴦�ڽ���״̬
        /// </summary>
        private Transform TPUnlockedState;

        /// <summary>
        /// TP�ڵ㴦�����ڽ���(����)״̬
        /// </summary>
        private Transform TPUnlockingState;

        /// <summary>
        /// ��������ĵ���ʱText
        /// </summary>
        private TextMeshProUGUI TPUnlockingTimeCost;

        /// <summary>
        /// ��������ĵ���ʱ������
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
            this.TTTPScrollRect = ContentPanel.Find("TechTreePanel").GetComponent<ScrollRect>();
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
        /// ��������
        /// </summary>
        public void Tick(float deltatime)
        {
            // ��ǰ
            if (ProjectOC.Input.InputManager.TechTreeInput.TechTree.LastTerm.WasPressedThisFrame())
            {
                RefreshCategory((cIndex - 1 + category.Length) % category.Length);
                Refresh();
            }
            // ���
            if (ProjectOC.Input.InputManager.TechTreeInput.TechTree.NextTerm.WasPressedThisFrame())
            {
                RefreshCategory((cIndex + 1 + category.Length) % category.Length);
                Refresh();
            }
            // ����
            if (ProjectOC.Input.InputManager.TechTreeInput.TechTree.Decipher.WasPressedThisFrame())
            {
                Decipher();
            }
            // �˳�
            if (ML.Engine.Input.InputManager.Instance.Common.Common.Back.WasPressedThisFrame())
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
            }
            // �Ƽ���
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

            //// to-delete
            //for(int i = 0; i < 99; ++i)
            //{
            //    this.inventory.AddItem(ItemSpawner.Instance.SpawnItem("100001"));
            //    this.inventory.AddItem(ItemSpawner.Instance.SpawnItem("100002"));
            //    this.inventory.AddItem(ItemSpawner.Instance.SpawnItem("100003"));
            //}
        
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

                //// to-delete
                //string text = "";
                //foreach(var item in inventory.GetItemList())
                //{
                //    if(item != null)
                //    {
                //        text += item.ID + ": " + item.Amount + "\n";
                //    }
                //}
                //Debug.Log("����ʣ��: \n" + text);
            }
        }


        private bool IsUIInit = false;
        private List<Sprite> tempSprite = new List<Sprite>();
        private List<CounterDownTimer> tempTimer = new List<CounterDownTimer>();
        private List<GameObject> tempGO = new List<GameObject>();
        /// <summary>
        /// Category��Ӧ��UIGO
        /// </summary>
        private Dictionary<TechPointCategory, GameObject> TPCGO = new Dictionary<TechPointCategory, GameObject>();
        /// <summary>
        /// TechTreePanel��TP��Ӧ��UIGO
        /// </summary>
        private Dictionary<string, GameObject> TTTPGO = new Dictionary<string, GameObject>();
        /// <summary>
        /// TechTreePanel��TP��EDGE��Ӧ��UIGO
        /// </summary>
        private Dictionary<string, GameObject> TTTPEGO = new Dictionary<string, GameObject>();

        /// <summary>
        /// TechPointPanel�ɽ������UIGO
        /// </summary>
        private Dictionary<string, GameObject> TPUnlockGO = new Dictionary<string, GameObject>();
        /// <summary>
        /// TechPointPanel�������ĵ�Item��UIGO
        /// </summary>
        private Dictionary<string, GameObject> TPCItemGO = new Dictionary<string, GameObject>();

        private int lastCIndex;
        private void ClearTemp()
        {
            foreach(var s in tempSprite)
            {
                Destroy(s);
            }
            tempSprite.Clear();
            foreach (var s in TPCGO.Values)
            {
                Destroy(s);
            }
            TPCGO.Clear();
            foreach (var s in TTTPGO.Values)
            {
                Destroy(s);
            }
            TTTPGO.Clear();
            foreach (var s in TTTPEGO.Values)
            {
                Destroy(s);
            }
            TTTPEGO.Clear();
            foreach (var s in TPCItemGO.Values)
            {
                Destroy(s);
            }
            TPCItemGO.Clear();
            foreach (var s in TPUnlockGO.Values)
            {
                Destroy(s);
            }
            TPUnlockGO.Clear();
            foreach (var s in tempGO)
            {
                Destroy(s);
            }
            tempGO.Clear();
            foreach (var s in tempTimer)
            {
                s.OnUpdateEvent = null;
                s.OnEndEvent -= Refresh;
            }
            tempTimer.Clear();
        }

        private void Refresh()
        {
            if(lastCIndex != cIndex)
            {
                lastCIndex = cIndex;
                this.IsUIInit = false;
            }

            foreach (var t in tempTimer)
            {
                t.OnUpdateEvent = null;
                t.OnEndEvent -= Refresh;
            }
            tempTimer.Clear();

            if(!this.IsUIInit)
            {
                ClearTemp();
            }

            topTitle.text = TechTreeManager.Instance.TPPanelTextContent.toptitle.GetText();

            #region CategoryPanel
            this.categoryLast.ReWrite(TechTreeManager.Instance.TPPanelTextContent.categorylast);
            this.categoryNext.ReWrite(TechTreeManager.Instance.TPPanelTextContent.categorynext);

            foreach(var c in category)
            {
                if(!this.TPCGO.TryGetValue(c, out var obj))
                {
                    obj = GameObject.Instantiate(this.categoryTemplate.gameObject, this.categoryTemplate.parent, false);
                    obj.SetActive(true);
                    TPCGO.Add(c, obj);

                    var sprite = TechTreeManager.Instance.GetTPCategorySprite(c);
                    obj.GetComponentInChildren<Image>().sprite = sprite;
                    tempSprite.Add(sprite);
                }

                obj.GetComponentInChildren<TextMeshProUGUI>().text = TechTreeManager.Instance.CategoryDict[c.ToString()].GetDescription();
                if (c != category[cIndex])
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
            // ����Grid Layout ÿ�е�����Ϊ�Ƽ��������Ĳ���
            var gridlayout = this.TechPointTemplate.parent.GetComponent<GridLayoutGroup>();
            gridlayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridlayout.constraintCount = GridRange.y;
            Dictionary<string, GameObject> tpPoint = new Dictionary<string, GameObject>();
            // �ӵ�
            foreach (string id in TechPointList)
            {
                if(id == "" || id == null)
                {
                    if(!this.IsUIInit)
                    {
                        var obj = new GameObject();
                        obj.transform.SetParent(this.TechPointTemplate.parent);

                        obj.AddComponent<RectTransform>();

                        tempGO.Add(obj);
                    }
                }
                else
                {
                    if(!this.TTTPGO.TryGetValue(id, out var obj))
                    {
                        obj = GameObject.Instantiate(this.TechPointTemplate.gameObject);
                        obj.transform.SetParent(this.TechPointTemplate.parent);
                        obj.SetActive(true);
                        this.TTTPGO.Add(id, obj);

                        // Icon
                        var icon = obj.transform.Find("Icon").GetComponent<Image>();
                        icon.sprite = TechTreeManager.Instance.GetTPSprite(id);
                        this.tempSprite.Add(icon.sprite);
                    }


                    // 0 : Locked | 1 : Unlocked | 2 : Unlocking
                    int status = TechTreeManager.Instance.IsUnlockedTP(id) ? 1 : (TechTreeManager.Instance.UnlockingTPTimers.ContainsKey(id) ? 2 :0);
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
                        };
                        timer.OnEndEvent += Refresh;
                        mask.fillAmount = (float)(timer.CurrentTime / TechTreeManager.Instance.GetTPTimeCost(id));
                        tempTimer.Add(timer);
                    }
                    // Select
                    if(CurrentID == id)
                    {
                        var selected = obj.transform.Find("Selected").gameObject;
                        selected.SetActive(true);

                        
                    }
                    else
                    {
                        var selected = obj.transform.Find("Selected").gameObject;
                        selected.SetActive(false);
                    }


                    tpPoint.Add(id, obj);
                }

            }
           
            #region ���»�������
            var cur = this.TTTPGO[CurrentID];

            // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
            RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
            RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
            // ��ȡ ScrollRect ���
            ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
            // ��ȡ Content �� RectTransform ���
            RectTransform contentRect = scrollRect.content;

            // ��ȡ UI Ԫ�ص��ĸ��ǵ�
            Vector3[] corners = new Vector3[4];
            uiRectTransform.GetWorldCorners(corners);
            bool allCornersVisible = true;
            for (int i = 0; i < 4; ++i)
            {
                // ������ռ�ĵ�ת��Ϊ��Ļ�ռ�ĵ�
                Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                // �ж� ScrollRect �Ƿ���������
                if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                {
                    allCornersVisible = false;
                    break;
                }
            }
            // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
            if (!allCornersVisible)
            {
                // ����ǰѡ�е������������һ������TP��λ��

                // ���û���λ��

                // ��ȡ�� A �͵� B �� Content �е�λ��
                Vector2 positionA = (this.TTTPGO[LastID].transform as RectTransform).anchoredPosition;
                Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;

                // ����� B ����ڵ� A ��ƫ����
                Vector2 offset = positionB - positionA;

                // ����ƫ�������� ScrollRect �Ļ���λ��
                Vector2 normalizedPosition = scrollRect.normalizedPosition;
                normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                scrollRect.normalizedPosition = normalizedPosition;
            }
            #endregion

            // ǿ���������� GridLayoutGroup �Ĳ���
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridlayout.GetComponent<RectTransform>());
            // Edge : ��̬���� Width Rotation.z, Height�̶�Ϊ10
            foreach (var kv in tpPoint)
            {
                var edgeTemplate = kv.Value.transform.Find("EdgeParent").Find("EdgeTemplate");
                edgeTemplate.gameObject.SetActive(false);
                var preTP = TechTreeManager.Instance.GetPreTechPoints(kv.Key);
                Color32 color = TechTreeManager.Instance.IsUnlockedTP(kv.Key) ? new Color32(255, 165, 0, 255) : Color.white;
                foreach (var tp in preTP)
                {
                    Transform edge = null;
                    if (!this.TTTPEGO.TryGetValue(kv.Key + tp, out var edgeGO))
                    {
                        edge = GameObject.Instantiate(edgeTemplate, edgeTemplate.parent);

                        TTTPEGO.Add(kv.Key + tp, edge.gameObject);

                        edge.gameObject.SetActive(true);

                        var obj = tpPoint[tp];

                        // ��ת -> Self to Target
                        RectTransform UIA = edge.transform as RectTransform;
                        RectTransform UIB = obj.transform as RectTransform;
                        UIA.rotation = Quaternion.Euler(0, 0, Vector3.Angle(new Vector3(1, 0, 0), UIB.position - UIA.position));

                        // Width : Distance(Self, Target) - Size
                        float dis = Vector3.Distance(UIB.position, (UIA.parent.parent as RectTransform).position) - gridlayout.cellSize.x;

                        var e = (edge as RectTransform);
                        e.sizeDelta = new Vector2(dis, e.sizeDelta.y);

                        // λ��
                        UIA.localPosition = UIA.rotation * new Vector3(50, 0, 0);
                    }
                    else
                    {
                        edge = TTTPEGO[kv.Key + tp].transform;
                    }

                    // IsUnlocked
                    edge.GetComponent<Image>().color = color;
                }

            }
            #endregion

            #region TechPointPanel
            // 0 : Locked | 1 : Unlocked | 2 : Unlocking
            int tpStatus = TechTreeManager.Instance.IsUnlockedTP(CurrentID) ? 1 : (TechTreeManager.Instance.UnlockingTPTimers.ContainsKey(CurrentID) ? 2 : 0);

            var TM = TechTreeManager.Instance;

            if(!this.IsUIInit)
            {
                // Icon
                var s = TM.GetTPSprite(CurrentID);
                tempSprite.Add(s);
                this.TPIcon.sprite = s;
            }
            // Name
            this.TPName.text = TM.GetTPName(CurrentID);
            // Description
            this.TPDescription.text = TM.GetTPDescription(CurrentID);
            // DecipherTip
            this.TPDecipherTip.text = tpStatus == 1 ? TM.TPPanelTextContent.unlockedtitletip.GetText() : TM.TPPanelTextContent.lockedtitletip.GetText();
            // KTInspector
            TPKTInspector.ReWrite(TM.TPPanelTextContent.inspector);

            // �ɽ�����
            foreach(var id in TM.GetTPCanUnlockedID(CurrentID))
            {
                if(!TPUnlockGO.TryGetValue(id, out var unlock))
                {
                    unlock = Instantiate(TPUnlockTemplate.gameObject, TPUnlockTemplate.parent, false);
                    unlock.SetActive(true);
                    this.TPUnlockGO.Add(id, unlock);

                    // Image
                    var s = CompositeSystem.Instance.GetCompositonSprite(id);
                    tempSprite.Add(s);
                    unlock.GetComponentInChildren<Image>().sprite = s;
                }

                // Text
                unlock.GetComponentInChildren<TextMeshProUGUI>().text = CompositeSystem.Instance.GetCompositonName(id);
            }
            
            // ���״̬
            // δ����
            if(tpStatus == 0)
            {
                this.TPLockedState.gameObject.SetActive(true);
                this.TPUnlockedState.gameObject.SetActive(false);
                this.TPUnlockingState.gameObject.SetActive(false);

                // ���밴����ʾ
                this.TPKT_Decipher.ReWrite(TM.TPPanelTextContent.decipher);

                // �Ƿ��������
                // to-do : UI
                bool canDecipher = CanDecipher;
                this.TPKT_Decipher.img.transform.parent.Find("CanDecipherImg").GetComponent<Image>().color = canDecipher ? new Color32(77, 233, 16, 255) : Color.gray;
                this.TPKT_Decipher.img.transform.parent.Find("Mask").GetComponent<Image>().gameObject.SetActive(!canDecipher);

                // ʱ������
                this.TPTimeCost.text = TM.TPPanelTextContent.timecosttip + TM.GetTPTimeCost(CurrentID).ToString() + "s";

                // Item ����
                foreach(var f in TM.GetTPItemCost(CurrentID))
                {
                    if(!this.TPCItemGO.TryGetValue(f.id, out var item))
                    {
                        item = Instantiate(TPItemCostTemplate.gameObject, TPItemCostTemplate.parent, false);
                        item.SetActive(true);
                        this.TPCItemGO.Add(f.id, item);

                        // Image
                        var s = ItemSpawner.Instance.GetItemSprite(f.id);
                        tempSprite.Add(s);
                        item.transform.Find("Image").GetComponent<Image>().sprite = s;
                    }

                    // Num
                    item.transform.Find("Image").Find("Num").GetComponent<TextMeshProUGUI>().text = f.num.ToString();

                    // Name
                    item.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = ItemSpawner.Instance.GetItemName(f.id);
                }
            }
            // �ѽ���
            else if(tpStatus == 1)
            {
                this.TPLockedState.gameObject.SetActive(false);
                this.TPUnlockedState.gameObject.SetActive(true);
                this.TPUnlockingState.gameObject.SetActive(false);
            }
            // ���ڽ���
            else if(tpStatus == 2)
            {
                this.TPLockedState.gameObject.SetActive(false);
                this.TPUnlockedState.gameObject.SetActive(false);
                this.TPUnlockingState.gameObject.SetActive(true);

                var timer = TechTreeManager.Instance.UnlockingTPTimers[CurrentID];

                // �������뵹��ʱText
                this.TPUnlockingTimeCost.text = timer.CurrentTime.ToString("0.00");
                // �������뵹��ʱ������
                this.TPUnlockingProgressBar.value = (float)timer.CurrentTime / TM.GetTPTimeCost(CurrentID);

                timer.OnUpdateEvent += (time) =>
                {
                    // �������뵹��ʱText
                    this.TPUnlockingTimeCost.text = timer.CurrentTime.ToString("0.00");
                    // �������뵹��ʱ������
                    this.TPUnlockingProgressBar.value = (float)timer.CurrentTime / TM.GetTPTimeCost(CurrentID);

                    // ����Ҫˢ�¡���Ϊ TechTree ����������Ѿ�����ˢ��
                    //if (timer.IsTimeUp)
                    //{
                    //    timer.OnUpdateEvent = null;
                    //    Refresh();
                    //}
                };
                // ����Ҫ���룬��Ϊǰ�� TechTree ʱ�Ѿ�������
                //tempTimer.Add(timer);
            }
            #endregion

            #region BotPanel
            this.KT_Back.ReWrite(TM.TPPanelTextContent.back);
            #endregion

            if(!this.IsUIInit)
            {
                if (this.TTTPGO.ContainsKey(CurrentID))
                {
                    TTTPScrollRect.normalizedPosition = new Vector2(0, 1);
                }
            }
            this.IsUIInit = true;
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
