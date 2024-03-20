using ML.Engine.ABResources;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using static ProjectOC.Player.UI.PlayerUIPanel;
using static ProjectOC.ResonanceWheelSystem.UI.BeastPanel;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheel_sub2;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheelUI;

namespace ML.Engine.UI
{
    public class UIBasePanel : MonoBehaviour
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string ID { get; protected set; }
        /// <summary>
        /// 资源加载执行器
        /// </summary>
        public FunctionExecutor<AsyncOperationHandle> functionExecutor = new FunctionExecutor<AsyncOperationHandle>();

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
        }

        /// <summary>
        /// 暂停时调用，即不处于栈顶时
        /// </summary>
        public virtual void OnPause()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// 再次成为栈顶时调用
        /// </summary>
        public virtual void OnRecovery()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// 出栈时调用
        /// </summary>
        public virtual void OnExit()
        {
            Manager.GameManager.DestroyObj(this.gameObject);
        }


        protected virtual void Enter()
        {
            this.Refresh();
        }

        protected virtual void Exit()
        {

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

        private UIKeyTipList UIKeyTipList;

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
        private AsyncOperationHandle InitUITextContents()
        {
            this.ABJAProcessorJson = new ML.Engine.ABResources.ABJsonAssetProcessor<T>(this.abpath, this.abname, (datas) =>
            {
               this.OnLoadJsonAssetComplete(datas);
            }, this.description);
            return this.ABJAProcessorJson.StartLoadJsonAssetData();
        }
        /// <summary>
        /// 初始化KeyTip
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
    }
}