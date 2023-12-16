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

        private TextMeshProUGUI topTitle;

        #region CategoryPanel
        private UIKeyTip categoryLast;
        private Transform categoryTemplate;
        private UIKeyTip categoryNext;
        #endregion

        #region TechTreePanel
        public Transform TechPointTemplate;
        #endregion


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
        }

        private void Start()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            this.enabled = false;
        }

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

        private void Decipher()
        {
            //if(TechTreeManager.Instance.CanUnlockTechPoint)
            //Debug.Log(TechTreeManager.Instance.UnlockTechPoint(inventory, CurrentID));
            Refresh();
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
                            mask.fillAmount = (float)(1 - timer.CurrentTime / TechTreeManager.Instance.GetTPTimeCost(id));
                            
                            if (timer.IsTimeUp)
                            {
                                timer.OnUpdateEvent = null;
                                Refresh();
                            }
                        };
                        mask.fillAmount = (float)(1 - timer.CurrentTime / TechTreeManager.Instance.GetTPTimeCost(id));
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



        }


        private void OnDestroy()
        {
            ClearTemp();

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
    }

}
