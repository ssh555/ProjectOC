using ML.Engine.ABResources;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Utility;
using ProjectOC.ManagerNS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;


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
        public ObjectPool objectPool;
        /// <summary>
        /// 所属UIManager
        /// </summary>
        private UIManager _uiMgr;
        /// <summary>
        /// 所属UIManager
        /// </summary>
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
            this.objectPool = new ObjectPool();
            this.InitObjectPool();
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
            //this.gameObject.SetActive(false);
        }

        /// <summary>
        /// 再次成为栈顶时调用
        /// </summary>
        public virtual void OnRecovery()
        {
            //this.gameObject.SetActive(true);
            this.Enter();
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
                Debug.Log("InitObjectPoolRefresh");
                this.Refresh();
            });

            StartCoroutine(this.objectPool.GetFunctionExecutor().Execute());
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

        private UIKeyTipList<T> UIKeyTipList;

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
                Debug.Log("InitUITextContentscompelete");
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
            UIKeyTipList = new UIKeyTipList<T>(transform,datas);

            
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