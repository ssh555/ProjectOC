using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using ProjectOC.ResonanceWheelSystem.UI;
using Sirenix.OdinInspector;
using TMPro;
using static ML.Engine.UI.PopUpUI;

namespace ML.Engine.UI
{
    public class PopUpUI : ML.Engine.UI.UIBasePanel<PopUpUIStruct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {

            base.Awake();
            this.InitTextContentPathData();
            this.functionExecutor.SetOnAllFunctionsCompleted(() =>
            {
                this.Refresh();
            });

            StartCoroutine(functionExecutor.Execute());
            this.Text = this.transform.Find("Image").Find("Text").GetComponent<TextMeshProUGUI>(); ;
        }

        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Enter();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.Exit();
            ClearTemp();
        }

        public override void OnPause()
        {
            base.OnPause();
            this.Exit();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.Enter();
        }

        protected override void Enter()
        {
            this.RegisterInput();
            base.Enter();
        }

        protected override void Exit()
        {
            this.UnregisterInput();
            base.Exit();
        }

        #endregion

        #region Internal

        private void UnregisterInput()
        {

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

        }

        private void RegisterInput()
        {
            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //手动模拟出栈
            GameManager.Instance.UIManager.GetTopUIPanel().OnRecovery();
            this.OnExit();
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //手动模拟出栈
            GameManager.Instance.UIManager.GetTopUIPanel().OnRecovery();
            this.OnExit();
        }
        #endregion

        #region UI
        #region Temp
        private void ClearTemp()
        {
            
        }

        #endregion

        #region UI对象引用

        public TMPro.TextMeshProUGUI Text;

        #endregion


        #endregion

        #region TextContent
        [System.Serializable]
        public struct PopUpUIStruct
        {
            //BotKeyTips
            public KeyTip Confirm;
            public KeyTip Back;
        }
        private void InitTextContentPathData()
        {
            this.abpath = "ML/Json/TextContent";
            this.abname = "PopUpUI";
            this.description = "PopUpUI数据加载完成";
        }
        #endregion
    }

}
