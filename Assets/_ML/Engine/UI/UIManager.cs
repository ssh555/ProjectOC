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

        #region UIջ����
        /// <summary>
        /// ��ǰ��ʾ��UIջ
        /// </summary>
        [ShowInInspector]
        private Stack<UIBasePanel> panelStack = new Stack<UIBasePanel>();

        /// <summary>
        /// �رյ�ǰ���ϲ�UI����ײ�UI�޷��رգ�������û����ײ�UI����Ϊnull
        /// </summary>
        public UIBasePanel PopPanel()
        {
            Debug.Log("PopPanel " +GetTopUIPanel().name + " " + Time.frameCount);
            ClearNull();
            // ��ײ�UI->start�޷��Ƴ�
            if (panelStack.Count < 2)
            {
                return null;
            }
            // ����ջ��
            UIBasePanel topPanel = panelStack.Pop();
            topPanel.OnExit();
            // ����ջ��
            panelStack.Peek().OnRecovery();
            return topPanel;
        }

        /// <summary>
        /// ��UI��ʾ�ڽ�����
        /// </summary>
        /// <param name="panelType"></param>
        public void PushPanel(UIBasePanel panel)
        {
            Debug.Log("PushPanel " + panel.name+" "+Time.frameCount);
            ClearNull();
            // ��ͣջ��
            if (panelStack.Count > 0)
            {
                panelStack.Peek().OnPause();
            }
            // ѹ��ջ
            panel.OnEnter();
            panelStack.Push(panel);
        }
        /// <summary>
        /// ��UIջ����Ϊ0��1ʱ�����Ը�����ײ�UIPanel��ֻ��ͨ���˺�������ײ�UIPanel��ջ
        /// ��ջΪ��ʱ������ͨ��PushPanelѹ����ײ��UIPanel
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
        /// ���ջ����UIPanel
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
            // ����һ���µ� GameObject������ Canvas ������ӵ���
            GameObject canvasObject = new GameObject("Canvas");
            GameObject.DontDestroyOnLoad(canvasObject);
            Canvas canvas = canvasObject.AddComponent<Canvas>();

            // ���� Canvas ������
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // ��� CanvasScaler �����ȷ�� UI �ڲ�ͬ�ֱ����±���һ��
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            // ��� GraphicRaycaster �����ȷ�� Canvas ���������ʹ�������
            canvasObject.AddComponent<GraphicRaycaster>();

            // ����һ���µ� EventSystem ���󣬲���������Ϊ Canvas ���Ӷ���
            GameObject eventSystem = new GameObject("EventSystem");
            GameObject.DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            eventSystem.transform.SetParent(canvasObject.transform);

            // �� Canvas �´��� NormalPanel
            GameObject normalPanel = new GameObject("NormalPanel");
            normalPanel.transform.SetParent(canvasObject.transform, false);
            // �������� UI ����ͽű�
            normalPanel.AddComponent<RectTransform>();
            normalPanel.AddComponent<CanvasRenderer>();
            normalPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
            this.normalPanel = normalPanel.transform;
            // �� Canvas �´��� BottomPanel
            GameObject bottomPanel = new GameObject("BottomPanel");
            bottomPanel.transform.SetParent(canvasObject.transform, false);
            // �������� UI ����ͽű�
            bottomPanel.AddComponent<RectTransform>();
            bottomPanel.AddComponent<CanvasRenderer>();
            bottomPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
            this.bottomPanel = bottomPanel.transform;
            return canvas;
        }

        #region ֪ͨUI
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

            // ���캯��
            public FloatTextUIData(string message)
            {
                msg = message;
            }
        }

        public struct BtnUIData
        {
            public string msg;
            public UnityAction action;

            // ���캯��
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

            // ���캯��
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

            // ���캯��
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

            // ���캯��
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
