using ML.Engine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace ML.Engine.UI
{
    public class UIBasePanel : UIBehaviour
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string ID { get; protected set; }
        /// <summary>
        /// 对象池
        /// </summary>
        public UIObjectPool objectPool;
        /// <summary>
        /// 所属UIManager
        /// </summary>
        private UIManager _uiMgr;
        /// <summary>
        /// 所属UIManager
        /// </summary>

        private bool hidePanel = true;
        public UIManager UIMgr
        {
            get
            {
                if (_uiMgr == null)
                {
                    _uiMgr = ML.Engine.Manager.GameManager.Instance.UIManager;
                }
                return _uiMgr;
            }
            set
            {
                UIMgr = value;
            }
        }

        /// <summary>
        /// 压入UI栈时调用
        /// </summary>
        public virtual void OnEnter()
        {
            this.gameObject.SetActive(true);
            this.objectPool = new UIObjectPool();
            this.InitObjectPool();
            this.InitBtnInfo();
            this.Enter();
        }

        /// <summary>
        /// 出栈时调用
        /// </summary>
        public virtual void OnExit()
        {
            this.Exit();
            Manager.GameManager.DestroyObj(this.gameObject);
            this.objectPool.OnDestroy();
        }

        /// <summary>
        /// 暂停时调用，即不处于栈顶时
        /// </summary>
        public virtual void OnPause()
        {
            this.Exit();
            if(hidePanel)
            { 
                this.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 再次成为栈顶时调用
        /// </summary>
        public virtual void OnRecovery()
        {
            if(hidePanel)
            {
                this.gameObject.SetActive(true);
            }
            this.Enter();
        }

        public void SetHidePanel()
        {
            this.hidePanel = !hidePanel;
        }


        protected virtual void Enter()
        {
            this.RegisterInput();
            this.Refresh();
        }

        protected virtual void Exit()
        {
            this.UnregisterInput();
        }


        protected virtual void UnregisterInput()
        {

        }

        protected virtual void RegisterInput()
        {

        }
        public virtual void Refresh()
        {

        }

        protected override void Awake()
        {
            
        }

        protected override void Start()
        {
            this.enabled = false;
        }

        protected virtual void InitObjectPool()
        {
            this.objectPool.GetFunctionExecutor().SetOnAllFunctionsCompleted(() =>
            {
                this.Refresh();
            });
            StartCoroutine(this.objectPool.GetFunctionExecutor().Execute());
            
        }

        //检测panel中的所有BtnListInitor和BtnListContainerInitor组件并初始化
        protected virtual void InitBtnInfo()
        {

        }



    }

    /// <summary>
    /// 若该UIBasePanel需要TextContent则可以加入泛型<TextContentStruct>
    /// </summary>
    public class UIBasePanel<T> : UIBasePanel
    {
        public T PanelTextContent => ABJAProcessorJson.Datas;
        public ML.Engine.ABResources.ABJsonAssetProcessor<T> ABJAProcessorJson;
        public string abpath;
        public string abname;
        public string description;

        public UIKeyTipList<T> UIKeyTipList;

        /// <summary>
        /// 加载Json完成后执行的回调，默认自动初始化KeyTip
        /// </summary>
        protected virtual void OnLoadJsonAssetComplete(T datas)
        {
            this.InitKeyTip(datas);
        }
        /// <summary>
        /// 加载Json
        /// </summary>
        private List<AsyncOperationHandle> InitUITextContents()
        {
            var handles = new List<AsyncOperationHandle>();
            var handle = this.ABJAProcessorJson = new ML.Engine.ABResources.ABJsonAssetProcessor<T>(this.abpath, this.abname, (datas) =>
            {
               this.OnLoadJsonAssetComplete(datas);
            }, this.description);
            handles.Add(this.ABJAProcessorJson.StartLoadJsonAssetData());

            return handles;
        }
        /// <summary>
        /// 初始化KeyTip
        /// </summary>
        private void InitKeyTip(T datas)
        {
            this.UIKeyTipList = new UIKeyTipList<T>(transform,datas);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Enter()
        {
            base.Enter();
        }

        protected virtual void InitTextContentPathData()
        {

        }

        protected override void InitObjectPool()
        {
            this.InitTextContentPathData();
            this.objectPool.GetFunctionExecutor().AddFunction(this.InitUITextContents);
            base.InitObjectPool();
        }
    }
}