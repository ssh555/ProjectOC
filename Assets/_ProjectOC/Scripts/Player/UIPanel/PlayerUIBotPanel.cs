using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.Player;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



namespace ProjectOC.Player.UI
{
    public class PlayerUIBotPanel : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;
        public PlayerCharacter player;
        private void Start()
        {

            InitUITextContents();

            

            
            IsInit = true;



            Refresh();
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

        #endregion







        #region Internal

        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.Enable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
            this.player.interactComponent.Enable();
            this.Refresh();
        }

        private void Exit()
        {
            this.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.Disable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Disable();
            this.player.interactComponent.Disable();
        }

        private void UnregisterInput()
        {





        }

        private void RegisterInput()
        {

            // 切换类目
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMenu.performed += OpenMenu_performed;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMap.performed += OpenMap_performed;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.SelectGrid.performed += SelectGrid_performed;
        }



        private void OpenMenu_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

        }

        private void OpenMap_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

        }

        private void OpenOption_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            GameManager.Instance.EnterPoint.GetOptionPanelInstance().Completed += (handle) =>
            {
                // 实例化
                var panel = handle.Result.GetComponent<OptionPanel>();

                panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                // Push
                GameManager.Instance.UIManager.PushPanel(panel);
            };
        }

        private void SelectGrid_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            /*Vector2 vector2 = obj.ReadValue<Vector2>();

            float angle = Mathf.Atan2(vector2.x, vector2.y);

            angle = angle * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle = angle + 360;
            }

            if (angle < 36 || angle > 324) CurrentGridIndex = 0;
            else if (angle > 36 && angle < 108) CurrentGridIndex = 4;
            else if (angle > 108 && angle < 180) CurrentGridIndex = 3;
            else if (angle > 180 && angle < 252) CurrentGridIndex = 2;
            else if (angle > 252 && angle < 324) CurrentGridIndex = 1;*/

            this.Refresh();
        }
        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private Dictionary<ML.Engine.InventorySystem.ItemType, GameObject> tempItemType = new Dictionary<ML.Engine.InventorySystem.ItemType, GameObject>();
        private List<GameObject> tempUIItems = new List<GameObject>();


        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                Destroy(s);
            }
            foreach (var s in tempItemType.Values)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItems)
            {
                Destroy(s);
            }
        }

        #endregion

        #region UI对象引用

        private IUISelected CurSelected;

        private SelectedButton NewGameBtn;
        private SelectedButton ContinueGameBtn;
        private SelectedButton OptionBtn;
        private SelectedButton QuitGameBtn;

        private TMPro.TextMeshProUGUI NewGameBtnText;
        private TMPro.TextMeshProUGUI ContinueGameBtnText;
        private TMPro.TextMeshProUGUI OptionBtnText;
        private TMPro.TextMeshProUGUI QuitGameBtnText;

        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson_StartMenuPanel == null || !ABJAProcessorJson_StartMenuPanel.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }

            NewGameBtnText.text = PanelTextContent_StartMenuPanel.NewGameBtn;
            ContinueGameBtnText.text = PanelTextContent_StartMenuPanel.ContinueGameBtn;
            OptionBtnText.text = PanelTextContent_StartMenuPanel.OptionBtn;
            QuitGameBtnText.text = PanelTextContent_StartMenuPanel.QuitGameBtn;

        }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct StartMenuPanelStruct
        {
            public ML.Engine.TextContent.TextContent NewGameBtn;
            public ML.Engine.TextContent.TextContent ContinueGameBtn;
            public ML.Engine.TextContent.TextContent OptionBtn;
            public ML.Engine.TextContent.TextContent QuitGameBtn;
        }

        public StartMenuPanelStruct PanelTextContent_StartMenuPanel => ABJAProcessorJson_StartMenuPanel.Datas;
        public ML.Engine.ABResources.ABJsonAssetProcessor<StartMenuPanelStruct> ABJAProcessorJson_StartMenuPanel;
        private void InitUITextContents()
        {
            ABJAProcessorJson_StartMenuPanel = new ML.Engine.ABResources.ABJsonAssetProcessor<StartMenuPanelStruct>("OC/Json/TextContent/StartMenuPanel", "StartMenuPanel", (datas) =>
            {
                Refresh();
                this.enabled = false;
            }, "StartMenuPanel数据");
            ABJAProcessorJson_StartMenuPanel.StartLoadJsonAssetData();
        }
        #endregion



    }

}
