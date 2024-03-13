using ML.Engine.Manager;
using ML.Engine.TextContent;
using ProjectOC.ResonanceWheelSystem.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ML.Engine.UI.OptionPanel;
using static ML.Engine.UI.StartMenuPanel;
using static ProjectOC.Player.UI.PlayerUIPanel;



namespace ML.Engine.UI
{
    public class OptionPanel : ML.Engine.UI.UIBasePanel<OptionPanelStruct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {

            base.Awake();
            this.InitTextContentPathData();

            /*            this.functionExecutor.AddFunction(new List<Func<AsyncOperationHandle>> {
                            this.InitDescriptionPrefab,
                            this.InitBeastBioPrefab,
                            this.InitUITexture2D});*/
            this.functionExecutor.SetOnAllFunctionsCompleted(() =>
            {
                this.Refresh();
            });

            StartCoroutine(functionExecutor.Execute());
            ToptitleText = this.transform.Find("TopTitle").Find("Text").GetComponent<TextMeshProUGUI>();
            btnList = this.transform.Find("ButtonList");
            gridLayout = btnList.GetComponent<GridLayoutGroup>();
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
            ML.Engine.Input.InputManager.Instance.Common.Option.Enable();
            base.Enter();
        }

        protected override void Exit()
        {
            ML.Engine.Input.InputManager.Instance.Common.Option.Disable();
            this.UnregisterInput();
            base.Exit();
        }

        #endregion

        #region Internal

        private void UnregisterInput()
        {


            //切换按钮
            ML.Engine.Input.InputManager.Instance.Common.Option.SwichBtn.started -= SwichBtn_started;

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;



        }

        private void RegisterInput()
        {

            //切换按钮
            ML.Engine.Input.InputManager.Instance.Common.Option.SwichBtn.started += SwichBtn_started;

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }



        private void SwichBtn_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            string actionName = obj.action.name;

            // 使用 ReadValue<T>() 方法获取附加数据
            string actionMapName = obj.action.actionMap.name;

            var vector2 = obj.ReadValue<UnityEngine.Vector2>();
            float angle = Mathf.Atan2(vector2.x, vector2.y);

            angle = angle * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle = angle + 360;
            }

            if (angle < 45 || angle > 315)
            {
                this.UIBtnList.MoveUPIUISelected();
            }  
            else if (angle > 45 && angle < 135)
            {
                this.UIBtnList.MoveRightIUISelected();
            }
            else if (angle > 135 && angle < 225)
            {
                this.UIBtnList.MoveDownIUISelected();
            }
            else if (angle > 225 && angle < 315)
            {
                this.UIBtnList.MoveLeftIUISelected();
            }
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //this.CurSelected.Interact();
            this.UIBtnList.GetCurSelected().Interact();
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

        private GridLayoutGroup gridLayout;

        private TMPro.TextMeshProUGUI ToptitleText;



        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }
        }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct OptionPanelStruct
        {
            public ML.Engine.TextContent.TextContent TopTitle;

            public TextTip[] Btns;

            //BotKeyTips
            public KeyTip Confirm;
            public KeyTip Back;
        }

        protected override void OnLoadJsonAssetComplete(OptionPanelStruct datas)
        {
            InitBtnData(datas);
        }

        private void InitTextContentPathData()
        {
            this.abpath = "ML/Json/TextContent";
            this.abname = "OptionPanel";
            this.description = "OptionPanel数据加载完成";
        }
        private Transform btnList;
        private UIBtnList UIBtnList; 
        private void InitBtnData(OptionPanelStruct datas)
        {
            UIBtnList = new UIBtnList(parent: btnList, limitNum: gridLayout.constraintCount, BtnType: 2);
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }

            //GraphicBtn
            this.UIBtnList.SetBtnAction("GraphicBtn",
            () =>
                {
                    Debug.Log("GraphicBtn");
                }
            );
            //AudioBtn
            this.UIBtnList.SetBtnAction("AudioBtn",
            () =>
            {
                Debug.Log("AudioBtn");
            }
            );
            //ControllerBtn
            this.UIBtnList.SetBtnAction("ControllerBtn",
            () =>
            {
                Debug.Log("ControllerBtn");
            }
            );
            //TutorialBtn
            this.UIBtnList.SetBtnAction("TutorialBtn",
            () =>
            {
                Debug.Log("TutorialBtn");
            }
            );
            //BackBtn
            this.UIBtnList.SetBtnAction("BackBtn",
            () =>
            {
                Debug.Log("BackBtn");
            }
            );
            //QuitGameBtn
            this.UIBtnList.SetBtnAction("QuitGameBtn",
            () =>
            {
                Debug.Log("QuitGameBtn");
            }
            );
 
        }
        #endregion



    }

}
