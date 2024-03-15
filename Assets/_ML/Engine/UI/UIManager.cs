using ML.Engine.Manager;
using ProjectOC.InventorySystem.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
namespace ML.Engine.UI
{
    public sealed class UIManager : Manager.GlobalManager.IGlobalManager
    {
        public UIManager()
        {
            GetCanvas = CreateCanvas();
            InitSideBarUIPrefabInstance();
            InitPopUpUIPrefabInstance();
            InitFloatTextUIPrefabInstance();
            InitBtnUIPrefabInstance();
        }

        public Canvas GetCanvas
        {
            get;
            private set;
        }

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
            if(this.panelStack.Count > 2)
            {
                return false;
            }
            if (this.panelStack.Count == 1)
            {
                if(GetTopUIPanel() != null)
                {
                    this.panelStack.Pop().OnExit();
                }
                else//�����л�����UIPanelΪ��ȴռ��һ��ջλ��ǿ��pop
                {
                    panelStack.Pop();
                }
            }

            if(panel != null)
            {
                this.PushPanel(panel);
            }

            return true;
        }

        /// <summary>
        /// ���ջ����UIPanel
        /// </summary>
        /// <returns></returns>
        public UIBasePanel GetTopUIPanel()
        {
            return this.panelStack.Peek();
        }

        #endregion

        private Canvas CreateCanvas()
        {
            // ����һ���µ�GameObject������Canvas������ӵ���
            GameObject canvasObject = new GameObject("Canvas");
            GameObject.DontDestroyOnLoad(canvasObject);
            Canvas canvas = canvasObject.AddComponent<Canvas>();

            // ����Canvas������
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            

            // ���CanvasScaler�����ȷ��UI�ڲ�ͬ�ֱ����±���һ��
            var cscalar = canvasObject.AddComponent<CanvasScaler>();
            cscalar.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cscalar.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            cscalar.referenceResolution = new Vector2(1920, 1080);


            // ���GraphicRaycaster�����ȷ��Canvas���������ʹ�������
            canvasObject.AddComponent<GraphicRaycaster>();

            // ����һ���µ�EventSystem���󣬲���������ΪCanvas���Ӷ���
            GameObject eventSystem = new GameObject("EventSystem");
            GameObject.DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            eventSystem.transform.SetParent(canvasObject.transform);

            return canvas;
        }

        #region ֪ͨUI
        private string SideBarUIPrefabPath = "NoticeUI/SideBarUI.prefab";
        private string PopUpUIPrefabPath = "NoticeUI/PopUpUI.prefab";
        private string FloatTextUIPrefabPath = "NoticeUI/FloatTextUI.prefab";
        private string BtnUIPrefabPath = "NoticeUI/BtnUI.prefab";
        [ShowInInspector]
        private GameObject SideBarUIPrefab, PopUpUIPrefab, FloatTextUIPrefab, BtnUIPrefab;
        public enum NoticeUIType
        {
            FloatTextUI = 0,
            PopUpUI,
            SideBarUI,
            BtnUI
        }
        //TODO ֮����ʵ����ʱ��ȡ����Ӧ�ű��Ļ���
        private void InitSideBarUIPrefabInstance()
        {
            Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.SideBarUIPrefabPath, isGlobal: true).Completed += (handle) =>
            {
                this.SideBarUIPrefab = handle.Result;
                this.SideBarUIPrefab.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                this.SideBarUIPrefab.GetComponent<SideBarUI>().SaveAsInstance();
            };
        }
        private void InitPopUpUIPrefabInstance()
        {
            Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.PopUpUIPrefabPath, isGlobal: true).Completed += (handle) =>
            {
                this.PopUpUIPrefab = handle.Result;
                this.PopUpUIPrefab.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                this.PopUpUIPrefab.SetActive(false);
            };
        }
        private void InitFloatTextUIPrefabInstance()
        {
            Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.FloatTextUIPrefabPath, isGlobal: true).Completed += (handle) =>
            {

                this.FloatTextUIPrefab = handle.Result;
                this.FloatTextUIPrefab.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                this.FloatTextUIPrefab.GetComponent<FloatTextUI>().SaveAsInstance();

            };
        }
        private void InitBtnUIPrefabInstance()
        {
            Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.BtnUIPrefabPath, isGlobal: true).Completed += (handle) =>
            {
                this.BtnUIPrefab = handle.Result;
                this.BtnUIPrefab.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                this.BtnUIPrefab.SetActive(false);
            };
        }

        public void PushNoticeUIInstance(NoticeUIType noticeUIType,string msg)
        {
            GameObject panel = null;
            switch (noticeUIType)
            {
                
                case NoticeUIType.FloatTextUI:

                    panel = GameObject.Instantiate(this.FloatTextUIPrefab);
                    panel.GetComponent<FloatTextUI>().Text.text = msg;
                    panel.GetComponent<FloatTextUI>().CopyInstance();
                    break;
                case NoticeUIType.BtnUI:

                    panel = GameObject.Instantiate(this.BtnUIPrefab);
                    break;
                case NoticeUIType.PopUpUI:

                    panel = GameObject.Instantiate(this.PopUpUIPrefab);
                    panel.GetComponent<PopUpUI>().Text.text = msg;
                    //�ֶ�ģ����ջ
                    panelStack.Peek().OnPause();
                    panel.GetComponent<PopUpUI>().OnEnter();
                    break;
                case NoticeUIType.SideBarUI:

                    panel = GameObject.Instantiate(this.SideBarUIPrefab);
                    panel.GetComponent<SideBarUI>().Text.text = msg;
                    panel.GetComponent<SideBarUI>().CopyInstance();
                    break;
            }
            panel.transform.localScale = Vector3.one;
            panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);
        }

        #endregion
    }

}
