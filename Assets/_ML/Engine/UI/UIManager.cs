using ML.Engine.Manager;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
namespace ML.Engine.UI
{
    public sealed class UIManager : Manager.GlobalManager.IGlobalManager
    {
        public UIManager()
        {
            GetCanvas = CreateCanvas();

            InitUIPrefabInstance<SideBarUI>(this.SideBarUIPrefabPath);
            InitUIPrefabInstance<PopUpUI>(this.PopUpUIPrefabPath);
            InitUIPrefabInstance<FloatTextUI>(this.FloatTextUIPrefabPath);
            InitUIPrefabInstance<BtnUI>(this.BtnUIPrefabPath);
            InitUIPrefabInstance<InputFieldUI>(this.InputFieldPrefabPath);
        }

        public Canvas GetCanvas
        {
            get;
            private set;
        }

        private Transform normalPanel;
        public Transform NormalPanel { get { return normalPanel; } }

        private Transform bottomPanel;
        public Transform BottomPanel { get { return bottomPanel; } }

        #region UI栈管理
        /// <summary>
        /// 当前显示的UI栈
        /// </summary>
        [ShowInInspector]
        private Stack<UIBasePanel> panelStack = new Stack<UIBasePanel>();

        /// <summary>
        /// 关闭当前最上层UI，最底层UI无法关闭，但可以没有最底层UI，即为null
        /// </summary>
        public UIBasePanel PopPanel()
        {
            Debug.Log("PopPanel " +GetTopUIPanel().name + " " + Time.frameCount);
            ClearNull();
            // 最底层UI->start无法移除
            if (panelStack.Count < 2)
            {
                return null;
            }
            // 弹出栈顶
            UIBasePanel topPanel = panelStack.Pop();
            topPanel.OnExit();
            // 激活栈顶
            panelStack.Peek().OnRecovery();
            return topPanel;
        }

        /// <summary>
        /// 把UI显示在界面上
        /// </summary>
        /// <param name="panelType"></param>
        public void PushPanel(UIBasePanel panel)
        {
            Debug.Log("PushPanel " + panel.name+" "+Time.frameCount);
            ClearNull();
            // 暂停栈顶
            if (panelStack.Count > 0)
            {
                panelStack.Peek().OnPause();
            }
            // 压入栈
            panel.OnEnter();
            panelStack.Push(panel);
        }
        /// <summary>
        /// 当UI栈数量为0或1时，可以更改最底层UIPanel，只能通过此函数将最底层UIPanel出栈
        /// 但栈为空时，可以通过PushPanel压入最底层的UIPanel
        /// </summary>
        public bool ChangeBotUIPanel(UIBasePanel panel)
        {
            ClearNull();
            if (this.panelStack.Count > 2)
            {
                return false;
            }
            if (this.panelStack.Count == 1)
            {
                this.panelStack.Pop().OnExit();
            }

            if(panel != null)
            {
                this.PushPanel(panel);
            }

            return true;
        }

        public void ClearNull()
        {
            while (panelStack.Count!=0 && panelStack.Peek() == null)
            {
                panelStack.Pop();
            }
        }

        /// <summary>
        /// 获得栈顶的UIPanel
        /// </summary>
        /// <returns></returns>
        public UIBasePanel GetTopUIPanel()
        {
            ClearNull();
            return this.panelStack.Peek();
        }

        #endregion

        private Canvas CreateCanvas()
        {
            // 创建一个新的 GameObject，并将 Canvas 组件附加到它
            GameObject canvasObject = new GameObject("Canvas");
            GameObject.DontDestroyOnLoad(canvasObject);
            Canvas canvas = canvasObject.AddComponent<Canvas>();

            // 设置 Canvas 的属性
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 添加 CanvasScaler 组件，确保 UI 在不同分辨率下保持一致
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            // 添加 GraphicRaycaster 组件，确保 Canvas 可以与鼠标和触摸交互
            canvasObject.AddComponent<GraphicRaycaster>();

            // 创建一个新的 EventSystem 对象，并将其设置为 Canvas 的子对象
            GameObject eventSystem = new GameObject("EventSystem");
            GameObject.DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            eventSystem.transform.SetParent(canvasObject.transform);

            // 在 Canvas 下创建 NormalPanel
            GameObject normalPanel = new GameObject("NormalPanel");
            normalPanel.transform.SetParent(canvasObject.transform, false);
            // 添加所需的 UI 组件和脚本
            normalPanel.AddComponent<RectTransform>();
            normalPanel.AddComponent<CanvasRenderer>();
            normalPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
            this.normalPanel = normalPanel.transform;
            // 在 Canvas 下创建 BottomPanel
            GameObject bottomPanel = new GameObject("BottomPanel");
            bottomPanel.transform.SetParent(canvasObject.transform, false);
            // 添加所需的 UI 组件和脚本
            bottomPanel.AddComponent<RectTransform>();
            bottomPanel.AddComponent<CanvasRenderer>();
            bottomPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
            this.bottomPanel = bottomPanel.transform;
            return canvas;
        }

        #region 通知UI
        private string SideBarUIPrefabPath = "Prefab_MainInteract_NoticeUI_UIPanel/Prefab_MainInteract_NoticeUI_UIPanel_SideBarUI.prefab";
        private string PopUpUIPrefabPath = "Prefab_MainInteract_NoticeUI_UIPanel/Prefab_MainInteract_NoticeUI_UIPanel_PopUpUI.prefab";
        private string FloatTextUIPrefabPath = "Prefab_MainInteract_NoticeUI_UIPanel/Prefab_MainInteract_NoticeUI_UIPanel_FloatTextUI.prefab";
        private string BtnUIPrefabPath = "Prefab_MainInteract_NoticeUI_UIPanel/Prefab_MainInteract_NoticeUI_UIPanel_BtnUI.prefab";
        private string InputFieldPrefabPath = "Prefab_MainInteract_NoticeUI_UIPanel/Prefab_MainInteract_NoticeUI_UIPanel_InputFieldUI.prefab";
        [ShowInInspector]
        private GameObject SideBarUIPrefab, PopUpUIPrefab, FloatTextUIPrefab, BtnUIPrefab,InputFieldPrefab;

        public enum NoticeUIType
        {
            FloatTextUI = 0,
            PopUpUI,
            SideBarUI,
            BtnUI,
            InputFieldUI
        }

        private void InitUIPrefabInstance<T>(string prefabPath) where T : INoticeUI
        {
            Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(prefabPath, isGlobal: true).Completed += (handle) =>
            {
                GameObject prefab = handle.Result;

                if (typeof(T) == typeof(PopUpUI))
                {
                    this.PopUpUIPrefab = prefab;
                }
                else if(typeof(T) == typeof(FloatTextUI))
                {
                    this.FloatTextUIPrefab = prefab;
                }
                else if(typeof(T) == typeof(SideBarUI))
                {
                    this.SideBarUIPrefab = prefab;
                }
                else if (typeof(T) == typeof(BtnUI))
                {
                    this.BtnUIPrefab = prefab;
                }
                else if (typeof(T) == typeof(InputFieldUI))
                {
                    this.InputFieldPrefab = prefab;
                }
                prefab.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false);
                prefab.GetComponent<T>().SaveAsInstance();
            };
        }
        public struct FloatTextUIData
        {
            public string msg;

            // 构造函数
            public FloatTextUIData(string message)
            {
                msg = message;
            }
        }

        public struct BtnUIData
        {
            public string msg;
            public UnityAction action;

            // 构造函数
            public BtnUIData(string message, UnityAction act)
            {
                msg = message;
                action = act;
            }
        }

        public struct PopUpUIData
        {
            public string msg1;
            public string msg2;
            public List<Sprite> spriteList;
            public UnityAction ConfirmAction;
            public UnityAction CancelAction;

            // 构造函数
            public PopUpUIData(string message1, string message2, List<Sprite> sprites, UnityAction confirmAction,UnityAction cancelAction = null)
            {
                msg1 = message1;
                msg2 = message2;
                spriteList = sprites;
                ConfirmAction = confirmAction;
                CancelAction = cancelAction;
            }
        }
        public struct InputFieldUIData
        {
            public string msg1;
            public string msg2;
            public List<Sprite> spriteList;
            public UnityAction<string,string> ConfirmAction;
            public UnityAction CancelAction;

            // 构造函数
            public InputFieldUIData(string message1, string message2, List<Sprite> sprites, UnityAction<string,string> confirmAction,UnityAction cancelAction = null)
            {
                msg1 = message1;
                msg2 = message2;
                spriteList = sprites;
                ConfirmAction = confirmAction;
                CancelAction = cancelAction;
            }
        }
        
        public struct SideBarUIData
        {
            public string msg1;
            public string msg2;
            public Sprite ShowSprite;

            // 构造函数
            public SideBarUIData(string message1, string message2,Sprite sprite)
            {
                msg1 = message1;
                msg2 = message2;
                ShowSprite = sprite;
            }
        }


        public void PushNoticeUIInstance<T>(NoticeUIType noticeUIType, T data)
        {
            GameObject panelGo = null;
            switch (noticeUIType)
            {

                case NoticeUIType.FloatTextUI:
                    panelGo = GameObject.Instantiate(this.FloatTextUIPrefab);
                    FloatTextUIData floatTextData = (FloatTextUIData)(object)data;
                    panelGo.GetComponent<FloatTextUI>().CopyInstance(floatTextData);
                    panelGo.transform.SetParent(GameManager.Instance.UIManager.NormalPanel, false);
                    break;
                case NoticeUIType.BtnUI:

                    if(bottomPanel.Find("PlayerUIBotPanel(Clone)") == null)
                    {
                        return;
                    }
                    Transform BtnNoticeUI = bottomPanel.Find("PlayerUIBotPanel(Clone)").Find("BtnNoticeUI");
                    if (BtnNoticeUI != null)
                    {
                        var Content = BtnNoticeUI.Find("Scroll View").Find("Viewport").Find("Content");
                        panelGo = GameObject.Instantiate(this.BtnUIPrefab);
                        panelGo.transform.SetParent(Content, false);
                        panelGo.name = panelGo.GetHashCode().ToString();
                        BtnUIData btnData = (BtnUIData)(object)data;
                        SelectedButton selectedButton = panelGo.GetComponent<SelectedButton>();

                        //TODO

                        panelGo.GetComponent<BtnUI>().CopyInstance(btnData);

                    }
                    break;
                case NoticeUIType.PopUpUI:
                    panelGo = GameObject.Instantiate(this.PopUpUIPrefab);
                    PopUpUIData popUpData = (PopUpUIData)(object)data;
                    panelGo.GetComponent<PopUpUI>().CopyInstance(popUpData);
                    
                    GameManager.Instance.UIManager.GetTopUIPanel().SetHidePanel(false);
                    GameManager.Instance.UIManager.PushPanel(panelGo.GetComponent<PopUpUI>());

                    panelGo.transform.SetParent(GameManager.Instance.UIManager.normalPanel, false);
                    break;
                case NoticeUIType.InputFieldUI:
                    panelGo = GameObject.Instantiate(this.InputFieldPrefab);
                    InputFieldUIData inputFieldData = (InputFieldUIData)(object)data;
                    panelGo.GetComponent<InputFieldUI>().CopyInstance(inputFieldData);
                    
                    GameManager.Instance.UIManager.GetTopUIPanel().SetHidePanel(false);
                    GameManager.Instance.UIManager.PushPanel(panelGo.GetComponent<InputFieldUI>());
                    
                    panelGo.transform.SetParent(GameManager.Instance.UIManager.normalPanel, false);
                    break;
                
                case NoticeUIType.SideBarUI:
                    panelGo = GameObject.Instantiate(this.SideBarUIPrefab);
                    SideBarUIData sideBarData = (SideBarUIData)(object)data;
                    panelGo.GetComponent<SideBarUI>().CopyInstance(sideBarData);
                    panelGo.transform.SetParent(GameManager.Instance.UIManager.normalPanel, false);
                    break;
            }
        }
        #endregion
    }

}
