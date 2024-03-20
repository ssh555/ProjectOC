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
                else//场景切换导致UIPanel为空却占用一个栈位，强行pop
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
        /// 获得栈顶的UIPanel
        /// </summary>
        /// <returns></returns>
        public UIBasePanel GetTopUIPanel()
        {
            return this.panelStack.Peek();
        }

        #endregion

        private Canvas CreateCanvas()
        {
            // 创建一个新的GameObject，并将Canvas组件附加到它
            GameObject canvasObject = new GameObject("Canvas");
            GameObject.DontDestroyOnLoad(canvasObject);
            Canvas canvas = canvasObject.AddComponent<Canvas>();

            // 设置Canvas的属性
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            

            // 添加CanvasScaler组件，确保UI在不同分辨率下保持一致
            var cscalar = canvasObject.AddComponent<CanvasScaler>();
            cscalar.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cscalar.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            cscalar.referenceResolution = new Vector2(1920, 1080);


            // 添加GraphicRaycaster组件，确保Canvas可以与鼠标和触摸交互
            canvasObject.AddComponent<GraphicRaycaster>();

            // 创建一个新的EventSystem对象，并将其设置为Canvas的子对象
            GameObject eventSystem = new GameObject("EventSystem");
            GameObject.DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            eventSystem.transform.SetParent(canvasObject.transform);

            return canvas;
        }

        #region 通知UI
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
        //TODO 之后还需实例化时先取消对应脚本的活性
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
                    //手动模拟入栈
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
