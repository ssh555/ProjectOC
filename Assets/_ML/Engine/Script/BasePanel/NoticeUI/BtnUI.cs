using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using ProjectOC.ResonanceWheelSystem.UI;
using Sirenix.OdinInspector;
using TMPro;
using static ML.Engine.UI.BtnUI;

namespace ML.Engine.UI
{
    public class BtnUI : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {

            base.Awake();

            this.Text = this.transform.Find("Image1").Find("Text").GetComponent<TextMeshProUGUI>(); ;
        }

        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        #endregion

        #region Override

        #endregion

        #region Internal

        protected override void UnregisterInput()
        {

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

        }

        protected override void RegisterInput()
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


        #endregion

        #region UI对象引用

        public TMPro.TextMeshProUGUI Text;

        #endregion


        #endregion

        #region TextContent
        [System.Serializable]
        public struct BtnUIStruct
        {
        }

        #endregion
    }

}
