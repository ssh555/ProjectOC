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
        /// Ψһ��ʶ��
        /// </summary>
        public string ID { get; protected set; }
        /// <summary>
        /// �����
        /// </summary>
        public ObjectPool objectPool;
        public FunctionExecutor<List<AsyncOperationHandle>> functionExecutor = new FunctionExecutor<List<AsyncOperationHandle>>();
        /// <summary>
        /// ����UIManager
        /// </summary>
        private UIManager _uiMgr;
        /// <summary>
        /// ����UIManager
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
        /// ѹ��UIջʱ����
        /// </summary>
        public virtual void OnEnter()
        {
            this.gameObject.SetActive(true);
            this.objectPool = new ObjectPool();
        }

        /// <summary>
        /// ��ͣʱ���ã���������ջ��ʱ
        /// </summary>
        public virtual void OnPause()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// �ٴγ�Ϊջ��ʱ����
        /// </summary>
        public virtual void OnRecovery()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// ��ջʱ����
        /// </summary>
        public virtual void OnExit()
        {
            Manager.GameManager.DestroyObj(this.gameObject);
        }


        protected virtual void Enter()
        {
            this.InitObjectPool();
            this.Refresh();
        }

        protected virtual void Exit()
        {
            this.objectPool.OnDestroy();
        }

        public virtual void Refresh()
        {

        }

        protected virtual void Awake()
        {
            
        }

        protected virtual void Start()
        {
            this.enabled = false;
        }

        protected virtual void InitObjectPool()
        {
            
        }


    }

    /// <summary>
    /// ����UIBasePanel��ҪTextContent����Լ��뷺��<TextContentStruct>
    /// </summary>
    public class UIBasePanel<T> : UIBasePanel
    {
        public T PanelTextContent => ABJAProcessorJson.Datas;
        public ML.Engine.ABResources.ABJsonAssetProcessor<T> ABJAProcessorJson;
        public string abpath;
        public string abname;
        public string description;

        private UIKeyTipList UIKeyTipList;

        /// <summary>
        /// ����Json��ɺ�ִ�еĻص���Ĭ���Զ���ʼ��KeyTip
        /// </summary>
        protected virtual void OnLoadJsonAssetComplete(T datas)
        {
            this.InitKeyTip(datas);
        }
        /// <summary>
        /// ����Json
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
        /// ��ʼ��KeyTip
        /// </summary>
        private void InitKeyTip(T datas)
        {
            UIKeyTipList = new UIKeyTipList(transform);

            KeyTip[] keyTips = GameManager.Instance.InputManager.ExportKeyTipValues(datas);
            foreach (var keyTip in keyTips)
            {
                InputAction inputAction = GameManager.Instance.InputManager.GetInputAction((keyTip.keymap.ActionMapName, keyTip.keymap.ActionName));

                this.UIKeyTipList.SetKeyTiptext(keyTip.keyname, GameManager.Instance.InputManager.GetInputActionBindText(inputAction));
                this.UIKeyTipList.SetDescriptiontext(keyTip.keyname, keyTip.description.GetText());
            }
        }

        protected override void Awake()
        {
            base.Awake();
            this.functionExecutor.AddFunction(this.InitUITextContents);
        }

        protected override void Enter()
        {
            base.Enter();
            
            
        }

        protected override void InitObjectPool()
        {
            base.InitObjectPool();
            this.objectPool.GetFunctionExecutor().AddFunction(this.InitUITextContents);
        }
    }
}