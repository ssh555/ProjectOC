using ML.Engine.TextContent;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ML.Engine.Extension;
using ML.Engine.Manager;
using static ProjectOC.WorkerNS.UI.UIWorkerHome;
using System.Linq;


namespace ProjectOC.WorkerNS.UI
{
    public class UIWorkerHome : ML.Engine.UI.UIBasePanel<WorkerHomePanel>
    {
        #region 数据
        private TMPro.TextMeshProUGUI Text_Title;
        private Sprite EmptySprite;
        private GridLayoutGroup GridLayout;
        private Transform UIItemTemplate;
        private Transform HomeMain;
        private Transform KeyTips;

        public Worker Worker => Home.Worker;

        public WorkerHome Home;

        #region Worker
        [ShowInInspector]
        private List<Worker> Workers = new List<Worker>();
        private bool IsInitWorkers = false;
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
                        var grid = GridLayout.GetGridSize();
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
                        while (currentWorkerIndex >= Workers.Count)
                        {
                            currentWorkerIndex -= grid.y;
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
                Refresh();
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

        #region TextContent
        [System.Serializable]
        public struct WorkerHomePanel
        {
            public TextContent textEmpty;
            public TextContent textHome;
            public TextContent textHomePost;
            public TextContent textConfirmPre;
            public TextContent textConfirmPost;
            public TextContent textMood;
            public KeyTip Confirm;
            public KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/Building";
            this.abname = "WorkerHomePanel";
            this.description = "WorkerHomePanel数据加载完成";
        }
        #endregion
        #endregion

        #region Unity
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            InitTextContentPathData();
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            GridLayout = transform.Find("Home").Find("Select").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
            UIItemTemplate = GridLayout.transform.Find("UIItemTemplate");
            UIItemTemplate.gameObject.SetActive(false);
            EmptySprite = UIItemTemplate.Find("Icon").GetComponent<Image>().sprite;
            HomeMain = transform.Find("Home").Find("Main");
            KeyTips = transform.Find("BotKeyTips").Find("KeyTips");
            IsInit = true;
        }
        #endregion

        #region Override Internal
        protected override void Enter()
        {
            tempSprite["HasHome"] = LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_HasHome");
            tempSprite["NoHome"] = LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_NoHome");
            tempSprite["Worker"] = LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_Worker");
            tempSprite["WorkerHome"] = LocalGameManager.Instance.WorkerManager.GetSprite("Tex2D_Worker_UI_WorkerHome");
            LocalGameManager.Instance.WorkerManager.OnDeleteWokerEvent += OnDeleteWokerEvent;
            IsInitWorkers = false;
            base.Enter();
        }
        public void OnDeleteWokerEvent(Worker worker)
        {
            IsInitWorkers = false;
            Refresh();
        }

        protected override void Exit()
        {
            LocalGameManager.Instance.WorkerManager.OnDeleteWokerEvent -= OnDeleteWokerEvent;
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
            }
            base.Exit();
        }

        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        private List<GameObject> tempUIItems = new List<GameObject>();

        protected override void UnregisterInput()
        {
            Home.OnWorkerMoodChangeEvent -= RefreshMood;
            if (Home.HaveWorker)
            {
                Home.Worker.OnMoodChangeEvent -= RefreshMood;
            }
            ProjectOC.Input.InputManager.PlayerInput.UIWorkerHome.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIWorkerHome.Alter.started -= Alter_started;
        }

        protected override void RegisterInput()
        {
            Home.OnWorkerMoodChangeEvent += RefreshMood;
            if (Home.HaveWorker)
            {
                Home.Worker.OnMoodChangeEvent += RefreshMood;
            }
            ProjectOC.Input.InputManager.PlayerInput.UIWorkerHome.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIWorkerHome.Alter.started += Alter_started;
        }

        private void Alter_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var f_offset = obj.ReadValue<Vector2>();
            var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
            var grid = GridLayout.GetGridSize();
            CurrentWorkerIndex += -offset.y * grid.y + offset.x;
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurrentWorker != null && CurrentWorker != Home.Worker)
            {
                if (Home.HaveWorker)
                {
                    string text = PanelTextContent.textConfirmPre + "<color=yellow>" + CurrentWorker.Name + "</color>" + PanelTextContent.textConfirmPost;
                    GameManager.Instance.UIManager.PushNoticeUIInstance(ML.Engine.UI.UIManager.NoticeUIType.PopUpUI, new ML.Engine.UI.UIManager.PopUpUIData(text, null, null,
                        () =>
                        {
                            (Home as IWorkerContainer).SetWorker(CurrentWorker);
                        }
                    ));
                }
                else
                {
                    (Home as IWorkerContainer).SetWorker(CurrentWorker);
                    IsInitWorkers = false;
                    Refresh();
                }
            }
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }
        #endregion

        #region UI
        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }
            Text_Title.text = PanelTextContent.textHome;

            if (!IsInitWorkers)
            {
                Workers = new List<Worker>() {};
                Workers.AddRange(LocalGameManager.Instance.WorkerManager.GetWorkers());
                Workers = Workers.OrderBy(worker => worker.HaveHome).ThenBy(worker => worker != null).ThenBy(worker => worker.ID).ToList();
                if (Worker != null)
                {
                    Workers.Remove(Worker);
                    Workers.Insert(0, Worker);
                }
                currentWorkerIndex = 0;
                lastWorkerIndex = 0;
                IsInitWorkers = true;
            }

            this.KeyTips.Find("KT_Confirm").gameObject.SetActive(CurrentWorker != null && CurrentWorker != Home.Worker);

            #region Select
            int delta = tempUIItems.Count - Workers.Count;
            if (delta > 0)
            {
                for (int i = 0; i < delta; ++i)
                {
                    tempUIItems[tempUIItems.Count - 1 - i].SetActive(false);
                }
            }
            else if (delta < 0)
            {
                delta = -delta;
                for (int i = 0; i < delta; ++i)
                {
                    var uiitem = Instantiate(UIItemTemplate, GridLayout.transform, false);
                    tempUIItems.Add(uiitem.gameObject);
                }
            }
            GameObject cur = null;
            GameObject last = null;

            for (int i = 0; i < Workers.Count; ++i)
            {
                var worker = Workers[i];
                var item = tempUIItems[i];
                // Active
                item.SetActive(true);
                // Icon
                item.transform.Find("Icon").GetComponent<Image>().sprite = worker != null ? tempSprite["Worker"] : EmptySprite;
                // Name
                item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = !string.IsNullOrEmpty(worker?.Name) ? worker.Name : PanelTextContent.textEmpty;
                // HomeIcon
                item.transform.Find("HomeIcon").GetComponent<Image>().sprite = worker != null && worker.HaveHome ? tempSprite["HasHome"] : tempSprite["NoHome"];

                bool isSelected = CurrentWorker == worker;
                item.transform.Find("Selected").gameObject.SetActive(isSelected);
                if (isSelected)
                {
                    cur = item;
                }
                if (i == lastWorkerIndex)
                {
                    last = item;
                }
            }
            #region 更新滑动窗口
            if (cur != null && last != null)
            {
                RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                RectTransform contentRect = scrollRect.content;

                Vector3[] corners = new Vector3[4];
                uiRectTransform.GetWorldCorners(corners);
                bool allCornersVisible = true;
                for (int i = 0; i < 4; ++i)
                {
                    Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                    if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                    {
                        allCornersVisible = false;
                        break;
                    }
                }

                if (!allCornersVisible)
                {
                    Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                    Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;
                    Vector2 offset = positionB - positionA;
                    Vector2 normalizedPosition = scrollRect.normalizedPosition;
                    normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                    scrollRect.normalizedPosition = normalizedPosition;
                }
            }
            else
            {
                GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(GridLayout.GetComponent<RectTransform>());
            #endregion
            #endregion
            bool hasHome = Worker != null && Worker.HaveHome;

            string text = Worker != null ? Worker.Name : PanelTextContent.textEmpty;
            HomeMain.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = "<color=yellow>" + text + "</color>" + PanelTextContent.textHomePost;

            HomeMain.Find("Icon").GetComponent<Image>().sprite = hasHome ? tempSprite["Worker"] : tempSprite["WorkerHome"];

            HomeMain.Find("Bar").gameObject.SetActive(hasHome);
            HomeMain.Find("Mood").gameObject.SetActive(hasHome);
            HomeMain.Find("Value").gameObject.SetActive(hasHome);
            if (hasHome)
            {
                var bar = HomeMain.Find("Bar").Find("Cur").GetComponent<RectTransform>();
                float sizeDeltaX = HomeMain.Find("Bar").GetComponent<RectTransform>().sizeDelta.x * Worker.EMCurrent / Worker.EMMax;
                bar.sizeDelta = new Vector2(sizeDeltaX, bar.sizeDelta.y);
                HomeMain.Find("Mood").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textMood;
                HomeMain.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = Worker.EMCurrent.ToString();
            }
        }

        public void RefreshMood(int mood)
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }
            bool hasWorker = Worker != null && Worker.HaveHome;

            HomeMain.Find("Icon").GetComponent<Image>().sprite = hasWorker ? tempSprite["Worker"] : tempSprite["WorkerHome"];

            HomeMain.Find("Bar").gameObject.SetActive(hasWorker);
            HomeMain.Find("Mood").gameObject.SetActive(hasWorker);
            HomeMain.Find("Value").gameObject.SetActive(hasWorker);
            if (hasWorker)
            {
                var bar = HomeMain.Find("Bar").Find("Cur").GetComponent<RectTransform>();
                float sizeDeltaX = HomeMain.Find("Bar").GetComponent<RectTransform>().sizeDelta.x * Worker.EMCurrent / Worker.EMMax;
                bar.sizeDelta = new Vector2(sizeDeltaX, bar.sizeDelta.y);
                HomeMain.Find("Mood").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textMood;
                HomeMain.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = Worker.EMCurrent.ToString();
            }
        }
        #endregion
    }
}
